using System.Collections;
using System.Collections.Generic;
using Models;

namespace ServiceCalls
{
	public class GetAllGachaEvents : Services.ServiceHandle
	{
		public override bool IsNubeeAccountRequired()
		{
			return false;
		}

		public override bool IsKeyRequired()
		{
			return false;
		}

		public override bool IsAnalyticsActivity()
		{
			return false;
		}

		public override IEnumerator Execute()
		{
			string cmd = GLOBAL.SRVCMD_GET_ALL_GACHA_EVENTS;
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
				List<GachaEventsInfo> list = new List<GachaEventsInfo>();
				ServiceCallUtils.GetGachaEvents(payload, ref list);
				returnValue = list;
				PerformSuccess();
			}
		}
	}
}
