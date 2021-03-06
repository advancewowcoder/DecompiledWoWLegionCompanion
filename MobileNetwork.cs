using bgs;
using JamLib;
using System;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using UnityEngine;

public class MobileNetwork
{
	public class MobileNetworkEventArgs : EventArgs
	{
		public int Count
		{
			get;
			set;
		}

		public MobileNetworkEventArgs()
		{
			this.Count = 0;
		}
	}

	private const int BUFFER_SIZE = 524288;

	public byte[] m_byteArray;

	public BufferedStream m_bufferedStream;

	private TcpConnection m_connection = new TcpConnection();

	public uint m_messageLength;

	public int m_messageCount;

	private static LogThreadHelper s_log = new LogThreadHelper("MobileNetwork");

	public event EventHandler<EventArgs> ConnectionStateChanged;

	public event EventHandler<EventArgs> ServerDisconnectedEventHandler;

	public event EventHandler<EventArgs> ServerConnectionLostEventHandler;

	public event EventHandler<MobileNetwork.MobileNetworkEventArgs> MessageReceivedEventHandler;

	public event EventHandler<EventArgs> UnknownMessageReceivedEventHandler;

	public bool IsConnected
	{
		get
		{
			return this.m_connection.Socket != null && this.m_connection.Socket.get_Connected();
		}
	}

	protected void OnConnectionStateChanged()
	{
		if (this.ConnectionStateChanged != null)
		{
			this.ConnectionStateChanged.Invoke(this, EventArgs.Empty);
		}
	}

	protected void OnMessageReceived(object msg, int count)
	{
		if (this.MessageReceivedEventHandler != null)
		{
			MobileNetwork.MobileNetworkEventArgs mobileNetworkEventArgs = new MobileNetwork.MobileNetworkEventArgs();
			mobileNetworkEventArgs.Count = count;
			this.MessageReceivedEventHandler.Invoke(msg, mobileNetworkEventArgs);
		}
	}

	protected void OnUnknownMessageReceived(string msg)
	{
		if (this.UnknownMessageReceivedEventHandler != null)
		{
			this.UnknownMessageReceivedEventHandler.Invoke(msg, EventArgs.Empty);
		}
	}

	public bool ConnectAsync(string serverAddress, int serverPort)
	{
		this.m_connection.LogDebug = new Action<string>(MobileNetwork.s_log.LogDebug);
		this.m_connection.LogWarning = new Action<string>(MobileNetwork.s_log.LogWarning);
		this.m_connection.OnFailure = new Action(this.ConnectFailedCallback);
		this.m_connection.OnSuccess = new Action(this.ConnectCallback);
		this.m_connection.Connect(serverAddress, serverPort);
		this.InitSocketReadState();
		return true;
	}

	private void ConnectCallback()
	{
		this.SocketStartReceiving();
		this.OnConnectionStateChanged();
	}

	private void ConnectFailedCallback()
	{
		this.OnConnectionStateChanged();
	}

	private void ServerDisconnected()
	{
		if (this.m_connection.Socket != null)
		{
			this.m_connection.Disconnect();
			this.SocketStopReceiving();
			if (this.ServerDisconnectedEventHandler != null)
			{
				this.ServerDisconnectedEventHandler.Invoke(this, EventArgs.Empty);
			}
			return;
		}
	}

	public bool Disconnect()
	{
		if (this.m_connection.Socket != null)
		{
			this.m_connection.Disconnect();
			this.SocketStopReceiving();
			this.OnConnectionStateChanged();
			return true;
		}
		return false;
	}

	private void ConnectionLost()
	{
		if (this.m_connection.Socket != null)
		{
			this.m_connection.Disconnect();
			this.SocketStopReceiving();
			if (this.ServerConnectionLostEventHandler != null)
			{
				this.ServerConnectionLostEventHandler.Invoke(this, EventArgs.Empty);
			}
			this.OnConnectionStateChanged();
		}
	}

	public void SendStringMessage(string message)
	{
		try
		{
			byte[] bytes = Encoding.get_UTF8().GetBytes(message);
			byte[] array = new byte[]
			{
				(byte)(bytes.Length >> 24 & 255),
				(byte)(bytes.Length >> 16 & 255),
				(byte)(bytes.Length >> 8 & 255),
				(byte)(bytes.Length & 255)
			};
			if (this.IsConnected)
			{
				this.m_connection.Socket.Send(array);
				this.m_connection.Socket.Send(bytes);
			}
			else
			{
				Debug.Log("SendStringMessage(): Connection lost.");
				this.ConnectionLost();
			}
		}
		catch (SocketException ex)
		{
			Debug.Log("SendStringMessage() exception: " + ex.ToString());
			if (ex.get_ErrorCode() == 10058)
			{
				this.ServerDisconnected();
			}
			else
			{
				Debug.Log("SendStringMessage(): Connection lost in exception.");
				this.ConnectionLost();
			}
		}
	}

	public void SendMessage(object obj)
	{
		string message = MessageFactory.Serialize(obj);
		this.SendStringMessage(message);
	}

	public object Deserialize(string msg)
	{
		object result;
		try
		{
			result = MessageFactory.Deserialize(msg);
		}
		catch (Exception obj)
		{
			this.AddOutput("~~~~~~~~~~~~~~~~~~~~~~~~~");
			this.AddOutput("FAILED TO PARSE JSON CODE");
			this.AddOutput("~~~~~~~~~~~~~~~~~~~~~~~~~");
			this.AddOutput(msg);
			this.AddOutput("~~~~~~~~~~~~~~~~~~~~~~~~~");
			this.AddOutput(obj);
			this.AddOutput("~~~~~~~~~~~~~~~~~~~~~~~~~");
			result = null;
		}
		return result;
	}

	public ulong ConvertDateTimeToTimeT(DateTime dt)
	{
		DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 1);
		if (dt <= dateTime)
		{
			return 0uL;
		}
		return (ulong)Math.Floor((dt - dateTime).get_TotalSeconds());
	}

	public DateTime ConvertTimeTToDateTime(ulong timet)
	{
		DateTime result = new DateTime(1970, 1, 1, 0, 0, 0, 1);
		if (timet <= 0uL)
		{
			return result;
		}
		return result.AddSeconds(timet);
	}

	private void AddOutput(string text)
	{
		Debug.Log(text);
	}

	private void AddOutput(object obj)
	{
		Debug.Log(obj.ToString());
	}

	private void InitSocketReadState()
	{
		if (this.m_byteArray == null)
		{
			this.m_byteArray = new byte[524288];
		}
		if (this.m_bufferedStream == null)
		{
			this.m_bufferedStream = new BufferedStream(new MemoryStream());
		}
	}

	private void SocketStartReceiving()
	{
		this.m_connection.Socket.BeginReceive(this.m_byteArray, 0, 524288, 0, new AsyncCallback(this.SocketReadCallback), null);
	}

	private void SocketStopReceiving()
	{
		this.m_bufferedStream.Seek(0L, 0);
	}

	private void SocketReadCallback(IAsyncResult asyncResult)
	{
		int num = this.m_connection.Socket.EndReceive(asyncResult);
		this.m_bufferedStream.Write(this.m_byteArray, 0, num);
		this.ProcessSocketData();
		this.SocketStartReceiving();
	}

	private void ProcessSocketData()
	{
		while (this.m_bufferedStream.get_Length() > 4L)
		{
			if (this.m_messageLength == 0u)
			{
				long position = this.m_bufferedStream.get_Position();
				byte[] array = new byte[4];
				this.m_bufferedStream.Seek(0L, 0);
				this.m_bufferedStream.Read(array, 0, 4);
				this.m_messageLength = (uint)((int)array[0] << 24 | (int)array[1] << 16 | (int)array[2] << 8 | (int)array[3]);
				this.m_bufferedStream.Seek(position, 0);
			}
			if (this.m_messageLength <= 0u || this.m_bufferedStream.get_Length() < (long)((ulong)(this.m_messageLength + 4u)))
			{
				break;
			}
			this.m_bufferedStream.Seek(4L, 0);
			byte[] array2 = new byte[this.m_messageLength];
			this.m_bufferedStream.Read(array2, 0, (int)this.m_messageLength);
			string @string = Encoding.get_UTF8().GetString(array2);
			object obj = this.Deserialize(@string);
			if (obj != null)
			{
				this.m_messageCount++;
				this.OnMessageReceived(obj, this.m_messageCount);
			}
			else
			{
				this.OnUnknownMessageReceived(@string);
			}
			BufferedStream bufferedStream = new BufferedStream(new MemoryStream());
			while (this.m_bufferedStream.get_Position() < this.m_bufferedStream.get_Length())
			{
				int num = this.m_bufferedStream.Read(this.m_byteArray, 0, this.m_byteArray.Length);
				bufferedStream.Write(this.m_byteArray, 0, num);
			}
			this.m_bufferedStream = bufferedStream;
			this.m_messageLength = 0u;
		}
	}

	public object InputFactory(Type type, object oldMessage)
	{
		if (type == null)
		{
			return null;
		}
		ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
		object result = constructor.Invoke(null);
		if (oldMessage == null)
		{
			return result;
		}
		return result;
	}

	public static void Process()
	{
		MobileNetwork.s_log.Process();
	}
}
