using System.Collections;
using Models;

namespace ServiceCalls
{
	public class StartupCheck : Services.ServiceHandle
	{
		public new bool IsNubeeAccountRequired()
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
			string cmd = GLOBAL.SERVER_CHECK;
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
				returnValue = new Models.ServerCheck
				{
					minimum_version = ServiceCallUtils.GetStringField(jsonObj, "minimum_version", string.Empty),
					maintenance = (ServiceCallUtils.GetField<int>(jsonObj, "maintenance") > 0)
				};
				PerformSuccess();
			}
		}
	}
}
