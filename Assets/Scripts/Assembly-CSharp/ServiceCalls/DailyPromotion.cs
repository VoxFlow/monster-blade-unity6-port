using System.Collections;

namespace ServiceCalls
{
	public class DailyPromotion : Services.ServiceHandle
	{
		public string m_nubeeId = string.Empty;

		public string m_deviceId = string.Empty;

		public override IEnumerator Execute()
		{
			string cmdStr = GLOBAL.SRVCMD_DAILY_PROMOTION + "/" + m_nubeeId + "/" + m_deviceId;
			result = Services.ConnectionResults.Busy;
			yield return SingletonComponent<JobManager>.Instance.StartCoroutine(WaitForQuery(cmdStr, null));
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
				if (!("OK" == jsonObj["message"].str))
				{
					yield break;
				}
				SingletonComponent<DailyManager>.Instance.PromoDatas.Clear();
				JSONObject payload = jsonObj["payload"];
				if (payload != null)
				{
					foreach (JSONObject tempObj in payload.list)
					{
						PromotionData tempPromoData = new PromotionData();
						ServiceCallUtils.SetPromotionFromPayload(tempObj, ref tempPromoData);
						if (!string.IsNullOrEmpty(tempPromoData.Name) && ValidPromoType(tempPromoData.PromoType))
						{
							SingletonComponent<DailyManager>.Instance.PromoDatas.Add(tempPromoData);
						}
					}
				}
				returnValue = null;
				PerformSuccess();
			}
		}

		private bool ValidPromoType(string in_promoType)
		{
			int num;
			switch (in_promoType)
			{
			default:
				num = ((in_promoType == "url") ? 1 : 0);
				break;
			case "shop":
			case "event":
			case "gacha":
				num = 1;
				break;
			}
			return (byte)num != 0;
		}
	}
}
