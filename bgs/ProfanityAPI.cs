using bnet.protocol.profanity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace bgs
{
	public class ProfanityAPI : BattleNetAPI
	{
		private Random m_rand;

		private static readonly char[] REPLACEMENT_CHARS = new char[]
		{
			'!',
			'@',
			'#',
			'$',
			'%',
			'^',
			'&',
			'*'
		};

		private string m_localeName;

		private WordFilters m_wordFilters;

		public ProfanityAPI(BattleNetCSharp battlenet) : base(battlenet, "Profanity")
		{
			this.m_rand = new Random();
		}

		public string FilterProfanity(string unfiltered)
		{
			if (this.m_wordFilters == null)
			{
				return unfiltered;
			}
			string text = unfiltered;
			using (List<WordFilter>.Enumerator enumerator = this.m_wordFilters.FiltersList.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					WordFilter current = enumerator.get_Current();
					if (!(current.Type != "bad"))
					{
						Regex regex = new Regex(current.Regex, 1);
						MatchCollection matchCollection = regex.Matches(text);
						if (matchCollection.get_Count() != 0)
						{
							IEnumerator enumerator2 = matchCollection.GetEnumerator();
							try
							{
								while (enumerator2.MoveNext())
								{
									Match match = (Match)enumerator2.get_Current();
									if (match.get_Success())
									{
										char[] array = text.ToCharArray();
										if (match.get_Index() <= array.Length)
										{
											int num = match.get_Length();
											if (match.get_Index() + match.get_Length() > array.Length)
											{
												num = array.Length - match.get_Index();
											}
											for (int i = 0; i < num; i++)
											{
												array[match.get_Index() + i] = this.GetReplacementChar();
											}
											text = new string(array);
										}
									}
								}
							}
							finally
							{
								IDisposable disposable = enumerator2 as IDisposable;
								if (disposable != null)
								{
									disposable.Dispose();
								}
							}
						}
					}
				}
			}
			return text;
		}

		public override void Initialize()
		{
			this.m_wordFilters = null;
			ResourcesAPI resources = this.m_battleNet.Resources;
			if (resources == null)
			{
				base.ApiLog.LogWarning("ResourcesAPI is not initialized! Unable to proceed.");
				return;
			}
			this.m_localeName = BattleNet.Client().GetLocaleName();
			if (string.IsNullOrEmpty(this.m_localeName))
			{
				base.ApiLog.LogWarning("Unable to get Locale from Localization class");
				this.m_localeName = "enUS";
			}
			base.ApiLog.LogDebug("Locale is {0}", new object[]
			{
				this.m_localeName
			});
			resources.LookupResource(new FourCC("BN"), new FourCC("apft"), new FourCC(this.m_localeName), new ResourcesAPI.ResourceLookupCallback(this.ResouceLookupCallback), null);
		}

		private void ResouceLookupCallback(ContentHandle contentHandle, object userContext)
		{
			if (contentHandle == null)
			{
				base.ApiLog.LogWarning("BN resource look up failed unable to proceed");
				return;
			}
			base.ApiLog.LogDebug("Lookup done Region={0} Usage={1} SHA256={2}", new object[]
			{
				contentHandle.Region,
				contentHandle.Usage,
				contentHandle.Sha256Digest
			});
			this.m_battleNet.LocalStorage.GetFile(contentHandle, new LocalStorageAPI.DownloadCompletedCallback(this.DownloadCompletedCallback), null);
		}

		private void DownloadCompletedCallback(byte[] data, object userContext)
		{
			if (data == null)
			{
				base.ApiLog.LogWarning("Downloading of profanity data from depot failed!");
				return;
			}
			base.ApiLog.LogDebug("Downloading of profanity data completed");
			try
			{
				WordFilters wordFilters = WordFilters.ParseFrom(data);
				this.m_wordFilters = wordFilters;
			}
			catch (Exception ex)
			{
				base.ApiLog.LogWarning("Failed to parse received data into protocol buffer. Ex  = {0}", new object[]
				{
					ex.ToString()
				});
			}
			if (this.m_wordFilters == null || !this.m_wordFilters.IsInitialized)
			{
				base.ApiLog.LogWarning("WordFilters failed to initialize");
				return;
			}
		}

		private char GetReplacementChar()
		{
			int num = this.m_rand.Next(ProfanityAPI.REPLACEMENT_CHARS.Length);
			return ProfanityAPI.REPLACEMENT_CHARS[num];
		}
	}
}
