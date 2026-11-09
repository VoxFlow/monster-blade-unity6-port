using System.Collections;
using System.Threading;
using Models;
using UnityEngine;

namespace ServiceCalls
{
	public class GameInitialization : Services.ServiceHandle
	{
		private string _jsonString;

		private JSONObject _jsonObj;

		private bool _analyzeResult;

		public string nubeeId = string.Empty;

		public string deviceId = string.Empty;

		public string platform = string.Empty;

		public string version = string.Empty;

		public string filter = string.Empty;

		public string language = string.Empty;

		public string deviceTypeId = "1";

		public string gender = string.Empty;

		public string market = string.Empty;

		public string osVersion = string.Empty;

		public string deviceName = string.Empty;

		private void ThreadFunc()
		{
			_jsonObj = new JSONObject(_jsonString);
		}

		private IEnumerator AnalyzeResultsAndBreakAsync()
		{
			_analyzeResult = CheckError();
			if (!_analyzeResult)
			{
				_jsonString = UrlConnection.text;
				Thread thread = new Thread(ThreadFunc);
				thread.Start();
				while (thread.IsAlive)
				{
					yield return null;
				}
				_analyzeResult = CheckJSON(_jsonObj);
			}
		}

		public override IEnumerator Execute()
		{
			NBProfiler.BeginTask("STARTUP_SEQUENCE_1");
			string cmd = GLOBAL.SRVCMD_GAME_INITIALIZE;
			long ts = TimeUtil.GetUnixEpochTime();
			string checksum = Services.GenerateChecksum(nubeeId + "|" + deviceId + "|" + platform + "|" + version + "|" + filter + "|" + language + "|" + deviceTypeId + "|" + gender + "|" + market + "|" + osVersion + "|" + deviceName + "|" + ts, Services.Key);
			WWWForm form = new WWWForm();
			form.AddField("nubee_id", nubeeId);
			form.AddField("device_id", deviceId);
			form.AddField("platform", platform);
			form.AddField("version", version);
			form.AddField("filter", filter);
			form.AddField("lang", language);
			form.AddField("device_type_id", deviceTypeId);
			form.AddField("gender", gender);
			form.AddField("market", market);
			form.AddField("os_version", osVersion);
			form.AddField("device_name", deviceName);
			form.AddField("ts", ts.ToString());
			form.AddField("checksum", checksum);
			result = Services.ConnectionResults.Busy;
			yield return SingletonComponent<JobManager>.Instance.StartCoroutine(WaitForQuery(cmd, form));
			yield return SingletonComponent<JobManager>.Instance.StartCoroutine(AnalyzeResultsAndBreakAsync());
			JSONObject jsonObj = _jsonObj;
			_jsonObj = null;
			float start = Time.realtimeSinceStartup;
			if (_analyzeResult)
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
			NBProfiler.EndTask("STARTUP_SEQUENCE_1");
			float t = Time.realtimeSinceStartup;
			if (start + 0.016f < t)
			{
				start = t;
				yield return null;
			}
			NBProfiler.BeginTask("STARTUP_SEQUENCE_2");
			GameInit gameInit = new GameInit();
			JSONObject payload = jsonObj.GetField("payload");
			if (payload != null && payload.type != JSONObject.Type.NULL)
			{
				NBProfiler.BeginTask("STARTUP_SEQUENCE_GAME_SETTINGS");
				if (payload.HasField("game_settings"))
				{
					JSONObject settingsPayload = payload["game_settings"];
					ServiceCallUtils.GameSettingsFromPayload(settingsPayload, ref gameInit.gameSettings);
				}
				else
				{
					Dbg.LogError("No Game Settings found in the initialization payload.", Dbg.NBLogType.SERVICES);
				}
				NBProfiler.EndTask("STARTUP_SEQUENCE_GAME_SETTINGS");
				t = Time.realtimeSinceStartup;
				if (start + 0.016f < t)
				{
					start = t;
					yield return null;
				}
				NBProfiler.BeginTask("STARTUP_SEQUENCE_PATCHINFO");
				if (payload.HasField("patch_info"))
				{
					JSONObject patchInfoPayload = payload["patch_info"];
					ServiceCallUtils.PatchInfoFromPayload(patchInfoPayload, ref gameInit.patchInfo);
				}
				else
				{
					Dbg.LogError("No Patch Info found in the initialization payload.", Dbg.NBLogType.SERVICES);
				}
				NBProfiler.EndTask("STARTUP_SEQUENCE_PATCHINFO");
				t = Time.realtimeSinceStartup;
				if (start + 0.016f < t)
				{
					start = t;
					yield return null;
				}
				NBProfiler.BeginTask("STARTUP_SEQUENCE_STORE_ITEMS");
				if (payload.HasField("store_items"))
				{
					JSONObject storeItemsPayload = payload["store_items"];
					ServiceCallUtils.ServerStoreFromPayload(storeItemsPayload, ref gameInit.serverStoreItems);
				}
				else
				{
					Dbg.LogError("No Store Item found in the initialization payload.", Dbg.NBLogType.SERVICES);
				}
				NBProfiler.EndTask("STARTUP_SEQUENCE_STORE_ITEMS");
				t = Time.realtimeSinceStartup;
				if (start + 0.016f < t)
				{
					start = t;
					yield return null;
				}
				NBProfiler.BeginTask("STARTUP_SEQUENCE_PETS");
				ServiceCallUtils.GetUnlockedPetDataFromPayload(payload, ref gameInit.characterAccountDetails);
				NBProfiler.EndTask("STARTUP_SEQUENCE_PETS");
				NBProfiler.BeginTask("STARTUP_SEQUENCE_UNLOCK_DUNGEON");
				if (payload.HasField("unlocked_dungeons"))
				{
					gameInit.unlockedCitiesJson = payload["unlocked_dungeons"];
				}
				else
				{
					Dbg.LogError("No Unlocked dungeon found in the initialization payload.", Dbg.NBLogType.SERVICES);
				}
				NBProfiler.EndTask("STARTUP_SEQUENCE_UNLOCK_DUNGEON");
				t = Time.realtimeSinceStartup;
				if (start + 0.016f < t)
				{
					start = t;
					yield return null;
				}
				NBProfiler.BeginTask("STARTUP_SEQUENCE_UNLOCK_EVENT");
				if (payload.HasField("events"))
				{
					gameInit.dungeonEvents = payload["events"];
				}
				else
				{
					Dbg.LogError("No Event found in the initialization payload.", Dbg.NBLogType.SERVICES);
				}
				NBProfiler.EndTask("STARTUP_SEQUENCE_UNLOCK_EVENT");
				t = Time.realtimeSinceStartup;
				if (start + 0.016f < t)
				{
					start = t;
					yield return null;
				}
				NBProfiler.BeginTask("STARTUP_SEQUENCE_MAIN_EVENT");
				if (payload.HasField("main_event"))
				{
					JSONObject mainEvent = payload["main_event"];
					gameInit.mainEventID = ServiceCallUtils.GetStringField(mainEvent, "event_id", string.Empty);
				}
				else
				{
					Dbg.LogError("No Main Event found in the initialization payload.", Dbg.NBLogType.SERVICES);
				}
				NBProfiler.EndTask("STARTUP_SEQUENCE_MAIN_EVENT");
				t = Time.realtimeSinceStartup;
				if (start + 0.016f < t)
				{
					start = t;
					yield return null;
				}
				NBProfiler.BeginTask("STARTUP_SEQUENCE_UNLOCK_CHARACTER");
				if (payload.HasField("character"))
				{
					JSONObject characterPayload = payload["character"];
					ServiceCallUtils.SetCharacterFromPayload(characterPayload, ref gameInit.characterAccountDetails);
				}
				else
				{
					Dbg.LogError("No Character found in the initialization payload.", Dbg.NBLogType.SERVICES);
				}
				NBProfiler.EndTask("STARTUP_SEQUENCE_UNLOCK_CHARACTER");
				t = Time.realtimeSinceStartup;
				if (start + 0.016f < t)
				{
					start = t;
					yield return null;
				}
				NBProfiler.BeginTask("STARTUP_SEQUENCE_UNLOCK_INVENTORY");
				if (payload.HasField("inventory"))
				{
					JSONObject inventoryPayload = payload["inventory"];
					ServiceCallUtils.GetInventoryFromPayload(inventoryPayload, ref gameInit.inventoryItems);
				}
				else
				{
					Dbg.LogError("No Inventory found in the initialization payload.", Dbg.NBLogType.SERVICES);
				}
				NBProfiler.EndTask("STARTUP_SEQUENCE_UNLOCK_INVENTORY");
				t = Time.realtimeSinceStartup;
				if (start + 0.016f < t)
				{
					start = t;
					yield return null;
				}
				NBProfiler.BeginTask("STARTUP_SEQUENCE_UNLOCK_GACHA");
				if (payload.HasField("gacha"))
				{
					JSONObject gachaPayload = payload["gacha"];
					ServiceCallUtils.GetGachaEvents(gachaPayload, ref gameInit.gachaEvents);
				}
				else
				{
					Dbg.LogError("No Gacha found in the initialization payload.", Dbg.NBLogType.SERVICES);
				}
				NBProfiler.EndTask("STARTUP_SEQUENCE_UNLOCK_GACHA");
				t = Time.realtimeSinceStartup;
				if (start + 0.016f < t)
				{
					start = t;
					yield return null;
				}
				NBProfiler.BeginTask("STARTUP_SEQUENCE_UNLOCK_ACHIEVEMENTS");
				if (payload.HasField("achievements"))
				{
					JSONObject achievementPayload = payload["achievements"];
					ServiceCallUtils.GetAchievements(achievementPayload, ref gameInit.achievements);
				}
				else
				{
					Dbg.LogError("No Achievements found in the initialization payload.", Dbg.NBLogType.SERVICES);
				}
				NBProfiler.EndTask("STARTUP_SEQUENCE_UNLOCK_ACHIEVEMENTS");
				t = Time.realtimeSinceStartup;
				if (start + 0.016f < t)
				{
					start = t;
					yield return null;
				}
				NBProfiler.BeginTask("STARTUP_SEQUENCE_UNLOCK_TUTORIAL");
				if (payload.HasField("finished_tutorials"))
				{
					JSONObject finishedTutorial = payload.GetField("finished_tutorials");
					if (finishedTutorial != null && !finishedTutorial.IsEmptyArray())
					{
						ServiceCallUtils.UpdateCombatTutorialProgress(finishedTutorial);
					}
					else
					{
						SingletonComponent<TutorialManager>.Instance.ClearProgress();
						SingletonComponent<TutorialManager>.Instance.SaveProgress();
						Dbg.LogWarn("Unable to get field \"finished_tutorials\" or it's an empty array");
					}
				}
				else
				{
					Dbg.LogWarn("Unable to get field \"finished_tutorials\" ");
				}
				NBProfiler.EndTask("STARTUP_SEQUENCE_UNLOCK_TUTORIAL");
				t = Time.realtimeSinceStartup;
				if (start + 0.016f < t)
				{
					start = t;
					yield return null;
				}
				NBProfiler.BeginTask("STARTUP_SEQUENCE_UNLOCK_TUTORIAL_PROG");
				if (payload.HasField("other_tutorial_progression"))
				{
					JSONObject otherTutorialProg = payload.GetField("other_tutorial_progression");
					if (otherTutorialProg != null && !otherTutorialProg.IsEmptyArray())
					{
						ServiceCallUtils.UpdateOtherTutorialProgress(otherTutorialProg);
					}
					else
					{
						SingletonComponent<TutorialManager>.Instance.ClearProgress();
						SingletonComponent<TutorialManager>.Instance.SaveProgress();
						Dbg.LogWarn("Unable to get field \"finished_tutorials\" or it's an empty array");
					}
				}
				else
				{
					Dbg.LogWarn("Unable to get field \"other_tutorial_progression\"");
				}
				NBProfiler.EndTask("STARTUP_SEQUENCE_UNLOCK_TUTORIAL_PROG");
				t = Time.realtimeSinceStartup;
				if (start + 0.016f < t)
				{
					start = t;
					yield return null;
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
				JSONObject jsonGiftSenders = payload.GetField("gift_senders");
				gameInit.giftSenders = jsonGiftSenders;
				t = Time.realtimeSinceStartup;
				if (start + 0.016f < t)
				{
					start = t;
					yield return null;
				}
				if (payload.HasField("battle_helped"))
				{
					JSONObject tempBattleHelp = payload["battle_helped"];
					string tempStr = ServiceCallUtils.GetStringField(tempBattleHelp, "user_num", string.Empty);
					NBGeneric.TryParse<int>(tempStr, out gameInit.m_BattleHelpData.UserNum);
					tempStr = ServiceCallUtils.GetStringField(tempBattleHelp, "social_reward", string.Empty);
					NBGeneric.TryParse<int>(tempStr, out gameInit.m_BattleHelpData.SocialPointsGained);
				}
				else
				{
					Dbg.LogWarn("There's no battle_helped field");
				}
				if (payload.HasField("promotion_codes"))
				{
					JSONObject promotionCodePayload = payload.GetField("promotion_codes");
					ServiceCallUtils.GetPromoCodes(promotionCodePayload, ref gameInit.m_promoCodeDatas);
				}
				else
				{
					Dbg.LogWarn("No PromoCode from payload.");
				}
				if (payload.HasField("armor_collection"))
				{
					JSONObject armorCollectionPayload = payload.GetField("armor_collection");
					ServiceCallUtils.GetArmorCollection(armorCollectionPayload, ref gameInit.characterAccountDetails);
				}
				else
				{
					Dbg.LogWarn("No armor_collection from payload.");
				}
			}
			returnValue = gameInit;
			NBProfiler.EndTask("STARTUP_SEQUENCE_2");
			PerformSuccess();
		}
	}
}
