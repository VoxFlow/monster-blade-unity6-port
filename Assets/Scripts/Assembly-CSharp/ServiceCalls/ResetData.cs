using System.Collections;
using UnityEngine;

namespace ServiceCalls
{
	public class ResetData : Services.ServiceHandle
	{
		public string nubeeId = string.Empty;

		public string deviceId = string.Empty;

		public override bool IsAnalyticsActivity()
		{
			return false;
		}

		public override IEnumerator Execute()
		{
			string cmd = GLOBAL.SRVCMD_RESET_DATA;
			long ts = TimeUtil.GetUnixEpochTime();
			string dataForChecksum = nubeeId + "|" + deviceId + "|";
			string checksum = Services.GenerateChecksum(dataForChecksum + ts, Services.Key);
			WWWForm form = new WWWForm();
			form.AddField("nubee_id", nubeeId);
			form.AddField("device_id", deviceId);
			form.AddField("ts", ts.ToString());
			form.AddField("checksum", checksum);
			result = Services.ConnectionResults.Busy;
			yield return SingletonComponent<JobManager>.Instance.StartCoroutine(WaitForQuery(cmd, form));
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
				returnValue = null;
				PerformSuccess();
			}
		}
	}
}
