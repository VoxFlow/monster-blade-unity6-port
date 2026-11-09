using System.Collections;
using System.Collections.Generic;
using Models;

namespace ServiceCalls
{
	public class GetStoreItems : Services.ServiceHandle
	{
		public override bool IsAnalyticsActivity()
		{
			return false;
		}

		public override IEnumerator Execute()
		{
			string cmd = GLOBAL.SRVCMD_SHOP_ITEMS;
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
				List<ServerStoreItem> details = new List<ServerStoreItem>();
				JSONObject payload = jsonObj["payload"];
				ServiceCallUtils.ServerStoreFromPayload(payload, ref details);
				returnValue = details;
				PerformSuccess();
			}
		}
	}
}
