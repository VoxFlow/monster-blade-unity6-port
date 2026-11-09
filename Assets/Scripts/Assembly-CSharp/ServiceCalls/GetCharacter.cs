using System.Collections;
using Models;

namespace ServiceCalls
{
	public class GetCharacter : Services.ServiceHandle
	{
		public string nubeeId = string.Empty;

		public string deviceId = string.Empty;

		public override bool IsAnalyticsActivity()
		{
			return false;
		}

		public override IEnumerator Execute()
		{
			string cmd = GLOBAL.SRVCMD_GET_CHARACTER + nubeeId + "/" + deviceId;
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
				CharacterAccountDetails details = new CharacterAccountDetails();
				ServiceCallUtils.SetCharacterFromPayload(payload, ref details);
				returnValue = details;
				PerformSuccess();
			}
		}
	}
}
