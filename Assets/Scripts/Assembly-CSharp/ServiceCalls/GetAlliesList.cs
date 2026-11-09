using System.Collections;
using Models;

namespace ServiceCalls
{
	public class GetAlliesList : Services.ServiceHandle
	{
		public string nubeeId = string.Empty;

		public string deviceId = string.Empty;

		public override IEnumerator Execute()
		{
			string cmd = GLOBAL.SRVCMD_GET_ALLIES_LIST + "/" + nubeeId + "/" + deviceId;
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
			JSONObject jsonPayload = jsonObj.GetField("payload");
			if (jsonPayload == null)
			{
				PerformFailClientError(-9, "Invalid server response");
				yield break;
			}
			JSONObject jsonStranger = jsonPayload.GetField("stranger");
			if (jsonStranger == null)
			{
				PerformFailClientError(-9, "Invalid server response");
				yield break;
			}
			JSONObject jsonFriends = jsonPayload.GetField("friends");
			if (jsonFriends == null)
			{
				PerformFailClientError(-9, "Invalid server response");
				yield break;
			}
			AlliesDetails details = new AlliesDetails();
			if (!jsonStranger.IsEmptyArray())
			{
				foreach (JSONObject jsonStrangerInfo in jsonStranger.list)
				{
					if (jsonStrangerInfo == null)
					{
						PerformFailClientError(-9, "Invalid server response");
						yield break;
					}
					JSONObject jsonStrangerCharacter = jsonStrangerInfo.GetField("character");
					if (jsonStrangerCharacter == null)
					{
						PerformFailClientError(-9, "Invalid server response");
						yield break;
					}
					FriendAccountDetails stranger = new FriendAccountDetails
					{
						nubeeId = ServiceCallUtils.GetStringField(jsonStrangerInfo, "nubee_id", string.Empty),
						deviceId = ServiceCallUtils.GetStringField(jsonStrangerInfo, "device_id", string.Empty),
						name = ServiceCallUtils.GetStringField(jsonStrangerInfo, "name", string.Empty),
						isFriend = false
					};
					ServiceCallUtils.SetCharacterFromPayload(jsonStrangerCharacter, ref stranger.character);
					ServiceCallUtils.GetFriendPetDataFromPayload(jsonStrangerCharacter, ref stranger.character);
					details.m_listStranger.Add(stranger);
				}
			}
			if (!jsonFriends.IsEmptyArray())
			{
				foreach (JSONObject jsonFriendInfo in jsonFriends.list)
				{
					if (jsonFriendInfo == null)
					{
						PerformFailClientError(-9, "Invalid server response");
						yield break;
					}
					JSONObject jsonFriendCharacter = jsonFriendInfo.GetField("character");
					if (jsonFriendCharacter == null)
					{
						PerformFailClientError(-9, "Invalid server response");
						yield break;
					}
					FriendAccountDetails friend = new FriendAccountDetails
					{
						nubeeId = ServiceCallUtils.GetStringField(jsonFriendInfo, "nubee_id", string.Empty),
						deviceId = ServiceCallUtils.GetStringField(jsonFriendInfo, "device_id", string.Empty),
						name = ServiceCallUtils.GetStringField(jsonFriendInfo, "name", string.Empty),
						coolDown = ServiceCallUtils.GetField<int>(jsonFriendInfo, "cooldown")
					};
					ServiceCallUtils.SetCharacterFromPayload(jsonFriendCharacter, ref friend.character);
					ServiceCallUtils.GetFriendPetDataFromPayload(jsonFriendCharacter, ref friend.character);
					details.m_listFriends.Add(friend);
				}
			}
			returnValue = details;
			PerformSuccess();
		}
	}
}
