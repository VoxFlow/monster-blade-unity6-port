using System.Collections;
using Models;
using UnityEngine;

namespace ServiceCalls
{
	public class DungeonEnd : Services.ServiceHandle
	{
		public string nubeeId = string.Empty;

		public string deviceId = string.Empty;

		public string dungeonKey = string.Empty;

		public string endStatus = string.Empty;

		public int monstersDefeated;

		public bool isKingBossDefeated;

		public string lowestScore = "0";

		public string overallScore = "0";

		public string assistFriendId = string.Empty;

		public int dodoboCurrency;

		public string sessionRequestID = string.Empty;

		public override IEnumerator Execute()
		{
			string cmd = GLOBAL.SRVCMD_DUNGEON_END;
			WWWForm form = new WWWForm();
			string dataForChecksum = nubeeId + "|" + deviceId + "|" + dungeonKey + "|" + endStatus + "|" + monstersDefeated + "|" + ((!isKingBossDefeated) ? "0" : "1") + "|" + overallScore + "|" + lowestScore + "|" + assistFriendId + "|" + dodoboCurrency + "|" + sessionRequestID + "|";
			form.AddField("nubee_id", nubeeId);
			form.AddField("device_id", deviceId);
			form.AddField("dungeon_key", dungeonKey);
			form.AddField("end_status", endStatus);
			form.AddField("monsters_defeated", monstersDefeated.ToString());
			form.AddField("is_king_boss_defeated", (!isKingBossDefeated) ? "0" : "1");
			form.AddField("overall_score", overallScore);
			form.AddField("lowest_rating", lowestScore);
			form.AddField("assists_friend_id0", assistFriendId);
			form.AddField("dodobo_currency", dodoboCurrency.ToString());
			form.AddField("request_id", sessionRequestID);
			long ts = TimeUtil.GetUnixEpochTime();
			form.AddField("ts", ts.ToString());
			string checksum = Services.GenerateChecksum(dataForChecksum + ts, Services.Key);
			form.AddField("checksum", checksum);
			result = Services.ConnectionResults.Busy;
			Dbg.Log(cmd, Dbg.NBLogType.SERVICES);
			yield return SingletonComponent<JobManager>.Instance.StartCoroutine(WaitForQuery(cmd, form));
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
			Dbg.LogWarn(" --> DungeonEnd Server Payload <--\n" + payload.ToString(), Dbg.NBLogType.SERVICES);
			if (payload.type != JSONObject.Type.NULL)
			{
				ServerBattleEndDetails details = new ServerBattleEndDetails
				{
					win = (endStatus == "completed")
				};
				ServiceCallUtils.SetCharacterFromPayload(payload, ref details.character);
				JSONObject jsonRewarded = payload["rewarded_items"];
				if (jsonRewarded != null && jsonRewarded.type != JSONObject.Type.NULL && !jsonRewarded.IsEmptyArray())
				{
					foreach (string key in jsonRewarded.keys)
					{
						if (details.rewarded_items.ContainsKey(key))
						{
							details.rewarded_items.Remove(key);
						}
						JSONObject rewardObj = jsonRewarded[key];
						ServerInventoryItem reward = null;
						ServiceCallUtils.SetItemFromPayload(rewardObj, ref reward);
						if (reward != null)
						{
							if (key.Contains("final_loot"))
							{
								reward.isFinalLoot = true;
							}
							details.rewarded_items.Add(key, reward);
						}
					}
				}
				if (payload.HasField("gained_xp"))
				{
					string tempGainedXpStr = payload["gained_xp"].str;
					int tempInt = 0;
					if (int.TryParse(tempGainedXpStr, out tempInt))
					{
						details.GainedXp = tempInt;
					}
					else
					{
						details.GainedXp = 0;
					}
				}
				if (payload.HasField("unlocked_dungeons"))
				{
					JSONObject dungeonPayload = payload["unlocked_dungeons"];
					details.unlockedCitiesJson = dungeonPayload;
				}
				else
				{
					Dbg.LogWarn("The \"unlocked_dungeons\" payload might be null or empty array");
				}
				if (payload.HasField("previous_leaderboard"))
				{
					ServiceCallUtils.ParseLastLdbResult(payload["previous_leaderboard"]);
					if (SingletonComponent<LeaderboardManager>.Instance.m_lastTop3Players.Count < 3 || SingletonComponent<LeaderboardManager>.Instance.m_lastPlayer.Rank <= 0)
					{
						SingletonComponent<LeaderboardManager>.Instance.m_NeedToShowResult = false;
					}
					else
					{
						SingletonComponent<LeaderboardManager>.Instance.m_NeedToShowResult = true;
					}
				}
				else
				{
					Dbg.LogWarn("No previous_leaderboard field");
				}
				JSONObject jsonEligibleEvents = payload.GetField("eligible_events");
				details.eligibleEvents = jsonEligibleEvents;
				JSONObject jsonGiftSenders = payload.GetField("gift_senders");
				details.giftSenders = jsonGiftSenders;
				returnValue = details;
			}
			else
			{
				returnValue = null;
			}
			PerformSuccess();
		}
	}
}
