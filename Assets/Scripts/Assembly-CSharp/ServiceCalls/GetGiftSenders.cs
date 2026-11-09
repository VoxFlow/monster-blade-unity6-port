using System.Collections;
using System.Collections.Generic;

namespace ServiceCalls
{
	public class GetGiftSenders : Services.ServiceHandle
	{
		public string nubeeId = string.Empty;

		public string deviceId = string.Empty;

		public override bool IsAnalyticsActivity()
		{
			return false;
		}

		public override IEnumerator Execute()
		{
			string cmd = GLOBAL.SRVCMD_GET_GIFT_SENDERS + "/" + nubeeId + "/" + deviceId;
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
			List<string> giftSenders = new List<string>();
			JSONObject payload = jsonObj["payload"];
			if (payload.type != JSONObject.Type.NULL && payload.Count > 0 && !payload.IsEmptyArray())
			{
				foreach (JSONObject name in payload.list)
				{
					giftSenders.Add(name.str);
				}
			}
			returnValue = giftSenders;
			PerformSuccess();
		}
	}
}
