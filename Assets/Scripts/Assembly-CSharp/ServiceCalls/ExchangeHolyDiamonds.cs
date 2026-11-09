using System.Collections;
using Models;
using UnityEngine;

namespace ServiceCalls
{
	public class ExchangeHolyDiamonds : Services.ServiceHandle
	{
		public string nubeeId = string.Empty;

		public string deviceId = string.Empty;

		public string holyShopId = string.Empty;

		public string requestId = string.Empty;

		public override IEnumerator Execute()
		{
			string cmd = GLOBAL.SRVCMD_EXCHANGE_HOLY_DIAMONDS;
			long ts = TimeUtil.GetUnixEpochTime();
			string checksum = Services.GenerateChecksum(nubeeId + "|" + deviceId + "|" + holyShopId + "|" + requestId + "|" + ts, Services.Key);
			WWWForm form = new WWWForm();
			form.AddField("nubee_id", nubeeId);
			form.AddField("device_id", deviceId);
			form.AddField("holy_shop_id", holyShopId);
			form.AddField("request_id", requestId);
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
			CommonRewards returnReward = new CommonRewards();
			JSONObject payload = jsonObj.GetField("payload");
			if (payload != null)
			{
				JSONObject currenciesPayload = payload.GetField("currencies");
				ServiceCallUtils.SetCurrenciesFromPayload(currenciesPayload, ref returnReward.currencies);
				JSONObject itemPayload = payload.GetField("items");
				if (itemPayload != null && itemPayload.Count > 0 && itemPayload[0] != null && !itemPayload.IsEmptyArray())
				{
					ServiceCallUtils.SetItemFromPayload(itemPayload[0], ref returnReward.item);
				}
				JSONObject petPayload = payload.GetField("pets");
				ServiceCallUtils.SetPetDataFromPayload(petPayload, out returnReward.pet);
			}
			returnValue = returnReward;
			PerformSuccess();
		}
	}
}
