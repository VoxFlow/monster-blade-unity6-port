using System.Collections;
using Models;
using UnityEngine;

namespace ServiceCalls
{
	public class Purchase : Services.ServiceHandle
	{
		public string nubeeId = string.Empty;

		public string deviceId = string.Empty;

		public string storeItemId = string.Empty;

		public string receiptHash = string.Empty;

		public string bagSlotsMultiple = string.Empty;

		public string json = string.Empty;

		public override IEnumerator Execute()
		{
			string cmd = GLOBAL.SRVCMD_SHOP_PURCHASE;
			WWWForm form = new WWWForm();
			form.AddField("nubee_id", nubeeId);
			form.AddField("device_id", deviceId);
			form.AddField("store_item_id", storeItemId);
			form.AddField("receipt_hash", receiptHash);
			form.AddField("amount", bagSlotsMultiple);
			form.AddField("record", json);
			long ts = TimeUtil.GetUnixEpochTime();
			string dataForChecksum = nubeeId + "|" + deviceId + "|" + storeItemId + "|" + receiptHash + "|" + bagSlotsMultiple + "|" + json + "|";
			string checksum = Services.GenerateChecksum(dataForChecksum + ts, Services.Key);
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
			JSONObject payload = jsonObj.GetField("payload");
			if (payload == null)
			{
				PerformFailClientError(-8, "Invalid response");
				yield break;
			}
			PurchaseDetails purchaseDetails = new PurchaseDetails();
			JSONObject product_id_list = payload.GetField("product_id");
			purchaseDetails.jsonPurchaseString = ServiceCallUtils.ArrayToJSONString(product_id_list);
			if (payload.HasField("character"))
			{
				payload = payload["character"];
			}
			ServiceCallUtils.SetCharacterFromPayload(payload, ref purchaseDetails.characterAccountDetails);
			returnValue = purchaseDetails;
			PerformSuccess();
		}
	}
}
