using System.Collections;
using Models;
using UnityEngine;

namespace ServiceCalls
{
	public class SendPromoCode : Services.ServiceHandle
	{
		public string nubeeId = string.Empty;

		public string deviceId = string.Empty;

		public string promoId = string.Empty;

		public string promoCode = string.Empty;

		public override IEnumerator Execute()
		{
			string cmd = GLOBAL.SRVCMD_PROMO_CODE;
			long ts = TimeUtil.GetUnixEpochTime();
			string checksum = Services.GenerateChecksum(nubeeId + "|" + deviceId + "|" + promoId + "|" + promoCode + "|" + ts, Services.Key);
			WWWForm form = new WWWForm();
			form.AddField("nubee_id", nubeeId);
			form.AddField("device_id", deviceId);
			form.AddField("promotion_code_id", promoId);
			form.AddField("promotion_code", promoCode);
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
			CommonRewards promoCodeReturn = new CommonRewards();
			JSONObject payload = jsonObj.GetField("payload");
			if (payload != null)
			{
				JSONObject currenciesPayload = payload.GetField("currencies");
				ServiceCallUtils.SetCurrenciesFromPayload(currenciesPayload, ref promoCodeReturn.currencies);
				JSONObject itemPayload = payload.GetField("items");
				if (itemPayload != null && itemPayload.Count > 0 && itemPayload[0] != null && !itemPayload.IsEmptyArray())
				{
					ServiceCallUtils.SetItemFromPayload(itemPayload[0], ref promoCodeReturn.item);
				}
			}
			returnValue = promoCodeReturn;
			PerformSuccess();
		}
	}
}
