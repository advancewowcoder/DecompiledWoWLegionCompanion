using System;
using System.IO;
using System.Text;

namespace bnet.protocol.account
{
	public class AccountReference : IProtoBuf
	{
		public bool HasId;

		private uint _Id;

		public bool HasEmail;

		private string _Email;

		public bool HasHandle;

		private GameAccountHandle _Handle;

		public bool HasBattleTag;

		private string _BattleTag;

		public bool HasRegion;

		private uint _Region;

		public uint Id
		{
			get
			{
				return this._Id;
			}
			set
			{
				this._Id = value;
				this.HasId = true;
			}
		}

		public string Email
		{
			get
			{
				return this._Email;
			}
			set
			{
				this._Email = value;
				this.HasEmail = (value != null);
			}
		}

		public GameAccountHandle Handle
		{
			get
			{
				return this._Handle;
			}
			set
			{
				this._Handle = value;
				this.HasHandle = (value != null);
			}
		}

		public string BattleTag
		{
			get
			{
				return this._BattleTag;
			}
			set
			{
				this._BattleTag = value;
				this.HasBattleTag = (value != null);
			}
		}

		public uint Region
		{
			get
			{
				return this._Region;
			}
			set
			{
				this._Region = value;
				this.HasRegion = true;
			}
		}

		public bool IsInitialized
		{
			get
			{
				return true;
			}
		}

		public void Deserialize(Stream stream)
		{
			AccountReference.Deserialize(stream, this);
		}

		public static AccountReference Deserialize(Stream stream, AccountReference instance)
		{
			return AccountReference.Deserialize(stream, instance, -1L);
		}

		public static AccountReference DeserializeLengthDelimited(Stream stream)
		{
			AccountReference accountReference = new AccountReference();
			AccountReference.DeserializeLengthDelimited(stream, accountReference);
			return accountReference;
		}

		public static AccountReference DeserializeLengthDelimited(Stream stream, AccountReference instance)
		{
			long num = (long)((ulong)ProtocolParser.ReadUInt32(stream));
			num += stream.get_Position();
			return AccountReference.Deserialize(stream, instance, num);
		}

		public static AccountReference Deserialize(Stream stream, AccountReference instance, long limit)
		{
			BinaryReader binaryReader = new BinaryReader(stream);
			instance.Region = 0u;
			while (limit < 0L || stream.get_Position() < limit)
			{
				int num = stream.ReadByte();
				if (num == -1)
				{
					if (limit >= 0L)
					{
						throw new EndOfStreamException();
					}
					return instance;
				}
				else
				{
					int num2 = num;
					if (num2 != 13)
					{
						if (num2 != 18)
						{
							if (num2 != 26)
							{
								if (num2 != 34)
								{
									if (num2 != 80)
									{
										Key key = ProtocolParser.ReadKey((byte)num, stream);
										uint field = key.Field;
										if (field == 0u)
										{
											throw new ProtocolBufferException("Invalid field id: 0, something went wrong in the stream");
										}
										ProtocolParser.SkipKey(stream, key);
									}
									else
									{
										instance.Region = ProtocolParser.ReadUInt32(stream);
									}
								}
								else
								{
									instance.BattleTag = ProtocolParser.ReadString(stream);
								}
							}
							else if (instance.Handle == null)
							{
								instance.Handle = GameAccountHandle.DeserializeLengthDelimited(stream);
							}
							else
							{
								GameAccountHandle.DeserializeLengthDelimited(stream, instance.Handle);
							}
						}
						else
						{
							instance.Email = ProtocolParser.ReadString(stream);
						}
					}
					else
					{
						instance.Id = binaryReader.ReadUInt32();
					}
				}
			}
			if (stream.get_Position() == limit)
			{
				return instance;
			}
			throw new ProtocolBufferException("Read past max limit");
		}

		public void Serialize(Stream stream)
		{
			AccountReference.Serialize(stream, this);
		}

		public static void Serialize(Stream stream, AccountReference instance)
		{
			BinaryWriter binaryWriter = new BinaryWriter(stream);
			if (instance.HasId)
			{
				stream.WriteByte(13);
				binaryWriter.Write(instance.Id);
			}
			if (instance.HasEmail)
			{
				stream.WriteByte(18);
				ProtocolParser.WriteBytes(stream, Encoding.get_UTF8().GetBytes(instance.Email));
			}
			if (instance.HasHandle)
			{
				stream.WriteByte(26);
				ProtocolParser.WriteUInt32(stream, instance.Handle.GetSerializedSize());
				GameAccountHandle.Serialize(stream, instance.Handle);
			}
			if (instance.HasBattleTag)
			{
				stream.WriteByte(34);
				ProtocolParser.WriteBytes(stream, Encoding.get_UTF8().GetBytes(instance.BattleTag));
			}
			if (instance.HasRegion)
			{
				stream.WriteByte(80);
				ProtocolParser.WriteUInt32(stream, instance.Region);
			}
		}

		public uint GetSerializedSize()
		{
			uint num = 0u;
			if (this.HasId)
			{
				num += 1u;
				num += 4u;
			}
			if (this.HasEmail)
			{
				num += 1u;
				uint byteCount = (uint)Encoding.get_UTF8().GetByteCount(this.Email);
				num += ProtocolParser.SizeOfUInt32(byteCount) + byteCount;
			}
			if (this.HasHandle)
			{
				num += 1u;
				uint serializedSize = this.Handle.GetSerializedSize();
				num += serializedSize + ProtocolParser.SizeOfUInt32(serializedSize);
			}
			if (this.HasBattleTag)
			{
				num += 1u;
				uint byteCount2 = (uint)Encoding.get_UTF8().GetByteCount(this.BattleTag);
				num += ProtocolParser.SizeOfUInt32(byteCount2) + byteCount2;
			}
			if (this.HasRegion)
			{
				num += 1u;
				num += ProtocolParser.SizeOfUInt32(this.Region);
			}
			return num;
		}

		public void SetId(uint val)
		{
			this.Id = val;
		}

		public void SetEmail(string val)
		{
			this.Email = val;
		}

		public void SetHandle(GameAccountHandle val)
		{
			this.Handle = val;
		}

		public void SetBattleTag(string val)
		{
			this.BattleTag = val;
		}

		public void SetRegion(uint val)
		{
			this.Region = val;
		}

		public override int GetHashCode()
		{
			int num = base.GetType().GetHashCode();
			if (this.HasId)
			{
				num ^= this.Id.GetHashCode();
			}
			if (this.HasEmail)
			{
				num ^= this.Email.GetHashCode();
			}
			if (this.HasHandle)
			{
				num ^= this.Handle.GetHashCode();
			}
			if (this.HasBattleTag)
			{
				num ^= this.BattleTag.GetHashCode();
			}
			if (this.HasRegion)
			{
				num ^= this.Region.GetHashCode();
			}
			return num;
		}

		public override bool Equals(object obj)
		{
			AccountReference accountReference = obj as AccountReference;
			return accountReference != null && this.HasId == accountReference.HasId && (!this.HasId || this.Id.Equals(accountReference.Id)) && this.HasEmail == accountReference.HasEmail && (!this.HasEmail || this.Email.Equals(accountReference.Email)) && this.HasHandle == accountReference.HasHandle && (!this.HasHandle || this.Handle.Equals(accountReference.Handle)) && this.HasBattleTag == accountReference.HasBattleTag && (!this.HasBattleTag || this.BattleTag.Equals(accountReference.BattleTag)) && this.HasRegion == accountReference.HasRegion && (!this.HasRegion || this.Region.Equals(accountReference.Region));
		}

		public static AccountReference ParseFrom(byte[] bs)
		{
			return ProtobufUtil.ParseFrom<AccountReference>(bs, 0, -1);
		}
	}
}
