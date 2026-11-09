using System.Collections;
using Models;
using UnityEngine;

namespace ServiceCalls
{
	public class MetapsClaim : Services.ServiceHandle
	{
		public string nubeeId = string.Empty;

		public string deviceId = string.Empty;

		public string requestId = string.Empty;

		public string diamondsToAdd = string.Empty;

		public string packageName = string.Empty;

		public string campaignId = string.Empty;

		public override IEnumerator Execute()
		{
			string cmd = GLOBAL.SRVCMD_METAPS_CLAIM;
			long ts = TimeUtil.GetUnixEpochTime();
			string checksumStr = nubeeId + "|" + deviceId + "|" + requestId + "|" + packageName + "|" + campaignId + "|" + diamondsToAdd + "|" + ts;
			string checksum = Services.GenerateChecksum(checksumStr, Services.Key);
			WWWForm form = new WWWForm();
			form.AddField("nubee_id", nubeeId);
			form.AddField("device_id", deviceId);
			form.AddField("request_id", requestId);
			form.AddField("packageName", packageName);
			form.AddField("campaignId", campaignId);
			form.AddField("rewardForUser", diamondsToAdd);
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
			CurrencyDetails currencies = new CurrencyDetails();
			JSONObject payload = jsonObj.GetField("payload");
			if (payload != null)
			{
				JSONObject currencyPayload = payload.GetField("currencies");
				ServiceCallUtils.SetCurrenciesFromPayload(currencyPayload, ref currencies);
			}
			returnValue = currencies;
			PerformSuccess();
		}
	}
}
