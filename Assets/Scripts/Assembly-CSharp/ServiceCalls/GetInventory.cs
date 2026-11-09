using System.Collections;
using System.Collections.Generic;
using Models;

namespace ServiceCalls
{
	public class GetInventory : Services.ServiceHandle
	{
		public string nubeeId = string.Empty;

		public string deviceId = string.Empty;

		public int offset;

		public override bool IsAnalyticsActivity()
		{
			return false;
		}

		public override IEnumerator Execute()
		{
			string cmd = GLOBAL.SRVCMD_GET_INVENTORY + nubeeId + "/" + deviceId + "/" + offset;
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
				List<ServerInventoryItem> items = new List<ServerInventoryItem>();
				ServiceCallUtils.GetInventoryFromPayload(payload, ref items);
				returnValue = items;
				PerformSuccess();
			}
		}
	}
}
