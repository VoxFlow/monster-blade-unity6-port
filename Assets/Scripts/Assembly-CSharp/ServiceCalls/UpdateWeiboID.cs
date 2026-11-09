using System.Collections;
using UnityEngine;

namespace ServiceCalls
{
	public class UpdateWeiboID : Services.ServiceHandle
	{
		public string nubeeId = string.Empty;

		public string deviceId = string.Empty;

		public string weiboId = string.Empty;

		public override IEnumerator Execute()
		{
			string cmd = GLOBAL.SRVCMD_UPDATE_WEIBO_ID;
			long ts = TimeUtil.GetUnixEpochTime();
			string szDataForChecksum = nubeeId + "|" + deviceId + "|" + weiboId;
			WWWForm form = new WWWForm();
			form.AddField("nubee_id", nubeeId);
			form.AddField("device_id", deviceId);
			form.AddField("weibo_id", weiboId);
			form.AddField("ts", ts.ToString());
			string checksum = Services.GenerateChecksum(szDataForChecksum + ts, Services.Key);
			form.AddField("checksum", checksum);
			Dbg.Log("checksum : " + szDataForChecksum + ts);
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
