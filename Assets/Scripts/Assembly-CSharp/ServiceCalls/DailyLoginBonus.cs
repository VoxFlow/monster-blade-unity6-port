using System.Collections;
using System.Collections.Generic;
using Models;

namespace ServiceCalls
{
	public class DailyLoginBonus : Services.ServiceHandle
	{
		public string m_nubeeId = string.Empty;

		public string m_deviceId = string.Empty;

		public override IEnumerator Execute()
		{
			string cmdStr = GLOBAL.SRVCMD_DAILY_BONUS + "/" + m_nubeeId + "/" + m_deviceId;
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
				JSONObject payload = jsonObj["payload"];
				List<DailyBonusData> tempDataList = new List<DailyBonusData>();
				if (payload != null)
				{
					foreach (JSONObject tempObj in payload.list)
					{
						DailyBonusData tempData = new DailyBonusData();
						ServiceCallUtils.SetDailyBonusDataFromPayload(tempObj, ref tempData);
						tempDataList.Add(tempData);
					}
				}
				returnValue = tempDataList;
				PerformSuccess();
			}
		}
	}
}
