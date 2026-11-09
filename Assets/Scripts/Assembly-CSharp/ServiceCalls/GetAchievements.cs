using System.Collections;
using System.Collections.Generic;

namespace ServiceCalls
{
	public class GetAchievements : Services.ServiceHandle
	{
		public string nubeeId = string.Empty;

		public string deviceId = string.Empty;

		public override IEnumerator Execute()
		{
			string cmd = GLOBAL.SRVCMD_GET_ACHIEVEMENTS + "/" + nubeeId + "/" + deviceId;
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
				Dictionary<int, AchievementManager.AchievementItem> itemList = new Dictionary<int, AchievementManager.AchievementItem>();
				ServiceCallUtils.GetAchievements(payload, ref itemList);
				returnValue = itemList;
				PerformSuccess();
			}
		}
	}
}
