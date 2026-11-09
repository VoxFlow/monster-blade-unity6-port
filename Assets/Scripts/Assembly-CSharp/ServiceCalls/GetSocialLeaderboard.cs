using System.Collections;

namespace ServiceCalls
{
	public class GetSocialLeaderboard : Services.ServiceHandle
	{
		public string m_nubeeId = string.Empty;

		public string m_deviceId = string.Empty;

		public int m_offset;

		public override IEnumerator Execute()
		{
			string cmdStr = GLOBAL.SRVCMD_LEADERBOARD_LOCAL + "/" + m_nubeeId + "/" + m_deviceId + "/" + m_offset;
			long currentTime = TimeUtil.GetUnixEpochTime();
			string checksum = Services.GenerateChecksum(m_nubeeId + "|" + m_deviceId + "|" + m_offset.ToString() + "|" + currentTime, Services.Key);
			Dbg.Log("generated checksum = " + checksum, Dbg.NBLogType.SERVICES);
			result = Services.ConnectionResults.Busy;
			yield return SingletonComponent<JobManager>.Instance.StartCoroutine(WaitForQuery(cmdStr, null));
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
				if (!("OK" == jsonObj["message"].str))
				{
					yield break;
				}
				SingletonComponent<LeaderboardManager>.Instance.m_Leaderboard.Clear();
				JSONObject payload = jsonObj["payload"];
				if (payload.HasField("rankings"))
				{
					SetDataToClientLeaderBoard(payload["rankings"]);
				}
				else
				{
					Dbg.LogWarn("[Warn] Unable to find \"rankings\" in return payload.");
				}
				if (payload.HasField("user_area"))
				{
					JSONObject userAreaPayload = payload["user_area"];
					if (userAreaPayload.HasField("user"))
					{
						ServiceCallUtils.SetLeaderboardDataFromPayload(userAreaPayload["user"], ref SingletonComponent<LeaderboardManager>.Instance.m_socialPlayer);
						if (!SingletonComponent<LeaderboardManager>.Instance.m_socialLeaderboard.ContainsKey(SingletonComponent<LeaderboardManager>.Instance.m_socialPlayer.Rank))
						{
							SingletonComponent<LeaderboardManager>.Instance.m_socialLeaderboard.Add(SingletonComponent<LeaderboardManager>.Instance.m_socialPlayer.Rank, SingletonComponent<LeaderboardManager>.Instance.m_socialPlayer);
						}
					}
					else
					{
						Dbg.LogWarn("[Warn] Unable to find \"user\" in return payload.");
					}
					if (userAreaPayload.HasField("area"))
					{
						SetDataToClientLeaderBoard(userAreaPayload["area"]);
					}
					else
					{
						Dbg.LogWarn("[Warn] Unable to find \"area\" in return payload.");
					}
				}
				else
				{
					Dbg.LogWarn("[Warn] Unable to find \"user_area\" in return payload.");
				}
				if (payload.HasField("expiry"))
				{
					SingletonComponent<LeaderboardManager>.Instance.ExpiryTimeStr = payload["expiry"].str;
				}
				returnValue = null;
				PerformSuccess();
			}
		}

		private void SetDataToClientLeaderBoard(JSONObject p_data)
		{
			if (!p_data.IsEmptyArray())
			{
				foreach (JSONObject item in p_data.list)
				{
					LeaderPlayer p_leadPlayer = new LeaderPlayer(0, string.Empty, 0);
					int key = ServiceCallUtils.SetLeaderboardDataFromPayload(item, ref p_leadPlayer);
					ServiceCallUtils.GetFriendPetDataFromPayload(item, ref p_leadPlayer.CharaAccountData);
					if (SingletonComponent<LeaderboardManager>.Instance.m_socialLeaderboard.ContainsKey(key))
					{
						SingletonComponent<LeaderboardManager>.Instance.m_socialLeaderboard.Remove(key);
					}
					SingletonComponent<LeaderboardManager>.Instance.m_socialLeaderboard.Add(key, p_leadPlayer);
				}
				return;
			}
			Dbg.LogWarn("[Warn] The social leader board rankings is empty");
		}
	}
}
