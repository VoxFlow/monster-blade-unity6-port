using System.Collections;
using System.Collections.Generic;
using Models;

namespace ServiceCalls
{
	public class GetFriendsList : Services.ServiceHandle
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
			string cmd = GLOBAL.SRVCMD_GET_FRIENDS_LIST + "/" + nubeeId + "/" + deviceId + "/" + offset;
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
			List<FriendAccountDetails> list = new List<FriendAccountDetails>();
			if (!payload.IsEmptyArray())
			{
				foreach (JSONObject friend in payload.list)
				{
					FriendAccountDetails details = new FriendAccountDetails
					{
						nubeeId = friend["nubee_id"].str,
						deviceId = friend["device_id"].str,
						name = friend["name"].str
					};
					if (friend.HasField("cooldown"))
					{
						int tempInt = 0;
						NBGeneric.TryParse<int>(friend["cooldown"].str, out tempInt);
						details.coolDown = tempInt;
					}
					JSONObject character = friend["character"];
					ServiceCallUtils.SetCharacterFromPayload(character, ref details.character);
					ServiceCallUtils.GetFriendPetDataFromPayload(character, ref details.character);
					list.Add(details);
				}
			}
			returnValue = list;
			PerformSuccess();
		}
	}
}
