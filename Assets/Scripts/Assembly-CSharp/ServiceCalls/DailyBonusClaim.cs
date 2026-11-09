using System.Collections;
using Models;
using UnityEngine;

namespace ServiceCalls
{
	public class DailyBonusClaim : Services.ServiceHandle
	{
		public string nubeeId = string.Empty;

		public string deviceId = string.Empty;

		public override IEnumerator Execute()
		{
			string cmd = GLOBAL.SRVCMD_DAILY_CLAIM;
			long ts = TimeUtil.GetUnixEpochTime();
			string checksum = Services.GenerateChecksum(nubeeId + "|" + deviceId + "|" + ts, Services.Key);
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
				yield break;
			}
			DailyBonusData ret = new DailyBonusData();
			JSONObject payload = jsonObj.GetField("payload");
			if (payload != null)
			{
				ServiceCallUtils.SetDailyBonusDataFromPayload(payload, ref ret);
			}
			returnValue = ret;
			PerformSuccess();
		}
	}
}
