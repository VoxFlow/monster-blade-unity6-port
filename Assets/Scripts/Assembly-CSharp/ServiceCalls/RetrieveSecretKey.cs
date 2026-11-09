using System.Collections;
using Models;

namespace ServiceCalls
{
	public class RetrieveSecretKey : Services.ServiceHandle
	{
		public string nubeeId = string.Empty;

		public string deviceId = string.Empty;

		public override bool IsAnalyticsActivity()
		{
			return false;
		}

		public override IEnumerator Execute()
		{
			long ts = TimeUtil.GetUnixEpochTime();
			string checksum = Services.GenerateChecksum(nubeeId + "|" + deviceId + "|" + ts, GLOBAL.SERVER_FIXED_KEY_SECURITY_KEY);
			string cmd = GLOBAL.SRVCMD_RETRIEVE_SECRET_KEY + nubeeId + "/" + deviceId + "?ts=" + ts + "&checksum=" + checksum;
			result = Services.ConnectionResults.Busy;
			yield return SingletonComponent<JobManager>.Instance.StartCoroutine(WaitForQuery(cmd, null));
			JSONObject jsonObj = null;
			if (AnalyzeResultsAndBreak(out jsonObj))
			{
				if (IsNoInternet())
				{
					if (ShouldRetry())
					{
						Dbg.LogWarn("There was a connection-related problem. Auto-retrying...");
						SingletonComponent<Services>.Instance.StartCoroutine(Execute());
					}
					else
					{
						PerformFailNoInternet();
					}
				}
				else
				{
					PerformFail(result.ToString());
				}
			}
			else
			{
				JSONObject payload = jsonObj["payload"];
				returnValue = new SecretKeyInfo
				{
					secretKey = payload["secret_key"].str,
					remaining = int.Parse(payload["remaining"].str),
					dateCreated = payload["date_created"].str,
					dateExpiry = payload["date_expiry"].str
				};
				PerformSuccess();
			}
		}
	}
}
