using System.Collections;
using System.Collections.Generic;

namespace ServiceCalls
{
	public class GetUnlockedCities : Services.ServiceHandle
	{
		public string nubeeId = string.Empty;

		public string deviceId = string.Empty;

		public override IEnumerator Execute()
		{
			string cmd = GLOBAL.SRVCMD_GET_UNLOCKED_CITIES + "/" + nubeeId + "/" + deviceId;
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
				yield break;
			}
			JSONObject payload = jsonObj["payload"];
			List<string> list = new List<string>();
			if (payload.type != JSONObject.Type.NULL && payload.Count > 0 && !payload.IsEmptyArray())
			{
				foreach (JSONObject city in payload.list)
				{
					list.Add(city["city_id"].str);
				}
			}
			returnValue = list;
			PerformSuccess();
		}
	}
}
