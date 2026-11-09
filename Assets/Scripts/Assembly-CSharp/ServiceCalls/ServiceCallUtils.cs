using System;
using System.Collections.Generic;
using System.Text;
using Models;
using UnityEngine;

namespace ServiceCalls
{
	public class ServiceCallUtils
	{
		public static bool IsValidPayload(JSONObject p_payload)
		{
			bool result = false;
			if (p_payload != null)
			{
				switch (p_payload.type)
				{
				case JSONObject.Type.ARRAY:
					result = !p_payload.IsEmptyArray() && p_payload.Count > 0;
					break;
				case JSONObject.Type.OBJECT:
					result = p_payload.Count > 0;
					break;
				default:
					result = true;
					break;
				case JSONObject.Type.NULL:
					break;
				}
			}
			return result;
		}

		public static void SetCharacterFromPayload(JSONObject p_payload, ref CharacterAccountDetails p_character)
		{
			if (!IsValidPayload(p_payload) || p_character == null)
			{
				return;
			}
			p_character.id = GetStringField(p_payload, "id", string.Empty);
			p_character.nubee_id = GetStringField(p_payload, "nubee_id", string.Empty);
			p_character.fbid = GetStringField(p_payload, "facebook_id", string.Empty);
			p_character.wbid = GetStringField(p_payload, "weibo_id", string.Empty);
			p_character.name = GetStringField(p_payload, "name", string.Empty);
			p_character.title = GetStringField(p_payload, "active_title", string.Empty);
			p_character.level = GetField<int>(p_payload, "level");
			p_character.exp = GetField<int>(p_payload, "exp");
			p_character.strength = GetField<int>(p_payload, "strength");
			p_character.agility = GetField<int>(p_payload, "agility");
			p_character.constitution = GetField<int>(p_payload, "constitution");
			p_character.date_created = GetStringField(p_payload, "date_created", string.Empty);
			p_character.date_updated = GetStringField(p_payload, "date_updated", string.Empty);
			p_character.max_inventory_slots = GetField<int>(p_payload, "max_inventory_slots");
			p_character.used_inventory_slots = GetField<int>(p_payload, "used_inventory_slots");
			p_character.max_stamina = GetField<int>(p_payload, "max_stamina");
			p_character.max_shields = GetField<int>(p_payload, "max_shields");
			p_character.stamina = GetField<int>(p_payload, "stamina");
			p_character.isBanFromIAP = GetField<int>(p_payload, "IAPban") > 0;
			if (p_character.stamina > p_character.max_stamina)
			{
				Dbg.LogError("SetCharacterFromPayload : Current Stamina Higher than Max Stamina. Cap the current stamina.");
				p_character.stamina = p_character.max_stamina;
			}
			p_character.shields = GetField<int>(p_payload, "shields");
			p_character.timeTillNextStamina = GetField<int>(p_payload, "last_stamina_refreshed_ts");
			p_character.timeTillNextShield = GetField<int>(p_payload, "last_shields_refreshed_ts");
			p_character.timeTillNextWarcry = GetField<int>(p_payload, "next_warcry");
			p_character.totalNumberOfFriends = GetField<int>(p_payload, "total_friend_count");
			p_character.friendLimit = GetField<int>(p_payload, "friend_limit");
			p_character.pendingFriendRequest = GetField<int>(p_payload, "pending_friends_count");
			JSONObject field = p_payload.GetField("currencies");
			SetCurrenciesFromPayload(field, ref p_character.currencies);
			JSONObject field2 = p_payload.GetField("equipments");
			if (field2 != null)
			{
				foreach (JSONObject item in field2.list)
				{
					Equipment.Part part = Equipment.FromString(GetStringField(item, "item_type", string.Empty));
					if (part != Equipment.Part.Max)
					{
						int num = (int)part;
						p_character.equipment[num] = new CharacterEquipmentDetails();
						CharacterEquipmentDetails characterEquipmentDetails = p_character.equipment[num];
						characterEquipmentDetails.db_id = GetStringField(item, "db_id", string.Empty);
						characterEquipmentDetails.item_id = GetStringField(item, "item_id", string.Empty);
						characterEquipmentDetails.item_level = GetField<int>(item, "item_level");
						characterEquipmentDetails.item_tier = GetField<int>(item, "item_tier");
					}
				}
			}
			if (p_payload.HasField("leaderboard"))
			{
				JSONObject p_payload3 = p_payload["leaderboard"];
				p_character.leaderboardRank = GetField<int>(p_payload3, "rank");
				p_character.battleScore = GetField<int>(p_payload3, "score");
			}
		}

		public static void SetItemFromPayload(JSONObject p_payload, ref ServerInventoryItem p_item)
		{
			if (IsValidPayload(p_payload))
			{
				string stringField = GetStringField(p_payload, "db_id", string.Empty);
				string stringField2 = GetStringField(p_payload, "item_id", string.Empty);
				string stringField3 = GetStringField(p_payload, "item_type", string.Empty);
				string empty = string.Empty;
				int field = GetField<int>(p_payload, "item_level");
				int field2 = GetField<int>(p_payload, "item_tier");
				int field3 = GetField<int>(p_payload, "fusion_xp");
				int field4 = GetField<int>(p_payload, "is_equipped");
				p_item = new ServerInventoryItem(stringField2, field2, stringField, stringField3, field, field3, empty, field4);
			}
		}

		public static void ParseLastLdbResult(JSONObject p_payload)
		{
			if (SingletonComponent<LeaderboardManager>.Instance != null)
			{
				LeaderPlayer leaderPlayer = new LeaderPlayer(0, string.Empty, 0);
				foreach (string key in p_payload.keys)
				{
					switch (key)
					{
					case "top3":
					{
						JSONObject jSONObject = p_payload[key];
						foreach (JSONObject item in jSONObject.list)
						{
							LeaderPlayer leaderPlayer2 = new LeaderPlayer(0, string.Empty, 0);
							leaderPlayer2.Rank = GetField<int>(item, "rank");
							leaderPlayer2.CharaName = GetStringField(item, "name", string.Empty);
							leaderPlayer2.Score = GetField<int>(item, "score");
							if (SingletonComponent<LeaderboardManager>.Instance.m_lastTop3Players.ContainsKey(leaderPlayer2.Rank))
							{
								SingletonComponent<LeaderboardManager>.Instance.m_lastTop3Players.Remove(leaderPlayer2.Rank);
							}
							SingletonComponent<LeaderboardManager>.Instance.m_lastTop3Players.Add(leaderPlayer2.Rank, leaderPlayer2);
						}
						break;
					}
					case "user":
					{
						JSONObject p_payload2 = p_payload[key];
						leaderPlayer.Rank = GetField<int>(p_payload2, "rank");
						PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
						leaderPlayer.CharaName = playerData.Name;
						leaderPlayer.Score = GetField<int>(p_payload2, "score");
						break;
					}
					case "reward":
						leaderPlayer.AmountOfPgcReward = GetField<int>(p_payload[key], "global_premium");
						leaderPlayer.AmountOfSocialPoint = GetField<int>(p_payload[key], "global_social");
						break;
					case "timestamp":
						SingletonComponent<LeaderboardManager>.Instance.LastResultTimeStamp = GetField<long>(p_payload, "timestamp");
						break;
					}
				}
				SingletonComponent<LeaderboardManager>.Instance.m_lastPlayer = leaderPlayer;
			}
			else
			{
				Dbg.LogWarn("Leaderboard manager is null, unable the parse last leaderboard result");
			}
		}

		public static void ParseTop3Player(JSONObject p_payload)
		{
			if (!(SingletonComponent<LeaderboardManager>.Instance != null))
			{
				return;
			}
			SingletonComponent<LeaderboardManager>.Instance.m_NeedToShowResult = true;
			foreach (JSONObject item in p_payload.list)
			{
				LeaderPlayer leaderPlayer = new LeaderPlayer(0, string.Empty, 0);
				leaderPlayer.Rank = GetField<int>(item, "rank");
				leaderPlayer.CharaName = GetStringField(item, "name", string.Empty);
				leaderPlayer.Score = GetField<int>(item, "score");
				if (SingletonComponent<LeaderboardManager>.Instance.m_lastTop3Players.ContainsKey(leaderPlayer.Rank))
				{
					SingletonComponent<LeaderboardManager>.Instance.m_lastTop3Players.Remove(leaderPlayer.Rank);
				}
				SingletonComponent<LeaderboardManager>.Instance.m_lastTop3Players.Add(leaderPlayer.Rank, leaderPlayer);
			}
		}

		public static void ParseLdbSelfLastResult(JSONObject p_payload)
		{
			if (SingletonComponent<LeaderboardManager>.Instance != null)
			{
				LeaderPlayer leaderPlayer = new LeaderPlayer(0, string.Empty, 0);
				leaderPlayer.Rank = GetField<int>(p_payload, "rank");
				leaderPlayer.CharaName = GetStringField(p_payload, "name", string.Empty);
				leaderPlayer.Score = GetField<int>(p_payload, "score");
				leaderPlayer.AmountOfPgcReward = GetField<int>(p_payload, "reward");
				SingletonComponent<LeaderboardManager>.Instance.m_player = leaderPlayer;
			}
		}

		public static void UpdateCombatTutorialProgress(JSONObject p_payload)
		{
			foreach (string key in p_payload.keys)
			{
				SingletonComponent<TutorialManager>.Instance.AddCompletedState(TutorialCombatMatchingMap(key));
			}
			SingletonComponent<TutorialManager>.Instance.SaveProgress();
		}

		public static Tutorial.State TutorialCombatMatchingMap(string p_tutorialNum)
		{
			switch (p_tutorialNum)
			{
			case "TUTORIAL_1":
				return Tutorial.State.Combat1;
			case "TUTORIAL_2":
				return Tutorial.State.Combat2;
			case "TUTORIAL_3":
				return Tutorial.State.Combat3;
			default:
				return Tutorial.State.None;
			}
		}

		public static void UpdateOtherTutorialProgress(JSONObject p_payload)
		{
			if (p_payload == null)
			{
				Dbg.LogWarn("Why someone puts a null JSONObject!");
				return;
			}
			foreach (string key in p_payload.keys)
			{
				switch (key)
				{
				case "fusion":
					if ("1" == p_payload[key].str)
					{
						SingletonComponent<TutorialManager>.Instance.AddCompletedState(Tutorial.State.Fusion);
					}
					break;
				case "gacha":
					if ("1" == p_payload[key].str)
					{
						SingletonComponent<TutorialManager>.Instance.AddCompletedState(Tutorial.State.Gacha);
					}
					break;
				case "gacha_premium":
					if ("1" == p_payload[key].str)
					{
						SingletonComponent<TutorialManager>.Instance.AddCompletedState(Tutorial.State.Gacha2);
					}
					break;
				case "pet_hunt":
					if ("1" == p_payload[key].str)
					{
						SingletonComponent<TutorialManager>.Instance.AddCompletedState(Tutorial.State.Pet);
					}
					break;
				case "equip_item":
					if ("1" == p_payload[key].str)
					{
						SingletonComponent<TutorialManager>.Instance.AddCompletedState(Tutorial.State.Inventory);
					}
					break;
				case "name_input":
					if ("1" == p_payload[key].str)
					{
						SingletonComponent<TutorialManager>.Instance.AddCompletedState(Tutorial.State.AfterEnterName);
					}
					break;
				}
			}
			SingletonComponent<TutorialManager>.Instance.SaveProgress();
		}

		public static void SetCurrenciesFromPayload(JSONObject p_currenciesPayload, ref CurrencyDetails p_currencies)
		{
			if (p_currenciesPayload == null || p_currenciesPayload.type == JSONObject.Type.NULL || p_currencies == null)
			{
				Dbg.LogWarn("Currency payload is null.");
				return;
			}
			p_currencies.social = GetField<float>(p_currenciesPayload, "social");
			p_currencies.premium = GetField<float>(p_currenciesPayload, "premium");
			p_currencies.ingame = GetField<float>(p_currenciesPayload, "in_game");
			p_currencies.freeGacha = GetField<float>(p_currenciesPayload, "premium_gacha_ticket");
			p_currencies.giftGacha = GetField<float>(p_currenciesPayload, "premium_gacha_gift_ticket");
			p_currencies.freeSocialGacha = GetField<float>(p_currenciesPayload, "social_gacha_ticket");
			p_currencies.medal = GetField<float>(p_currenciesPayload, "medal");
			p_currencies.dragonPoints = GetField<int>(p_currenciesPayload, "dragon_point");
			p_currencies.holyDiamonds = GetField<int>(p_currenciesPayload, "holy_diamond");
		}

		public static void GetUnlockedPetDataFromPayload(JSONObject p_payload, ref CharacterAccountDetails p_character)
		{
			if (!IsValidPayload(p_payload) || p_character == null || !p_payload.HasField("pets"))
			{
				return;
			}
			JSONObject jSONObject = p_payload["pets"];
			if (jSONObject.HasField("unlocked"))
			{
				JSONObject jSONObject2 = jSONObject["unlocked"];
				if (jSONObject2.type != JSONObject.Type.ARRAY || jSONObject2.Count <= 0)
				{
					Dbg.LogError("[ServiceCallUtils:GetPetDataFromPayload] Cannot retreive unlocked pets: Invalid json object.");
				}
				else
				{
					int count = jSONObject2.Count;
					p_character.unlockedPets = new Dictionary<string, PetDetails>();
					for (int i = 0; i < count; i++)
					{
						JSONObject p_petPayload = (JSONObject)jSONObject2.list[i];
						PetDetails p_petDetails = null;
						SetPetDataFromPayload(p_petPayload, out p_petDetails);
						if (p_petDetails != null && !p_character.unlockedPets.ContainsKey(p_petDetails.m_petId))
						{
							p_character.unlockedPets.Add(p_petDetails.m_petId, p_petDetails);
						}
					}
				}
			}
			if (jSONObject.HasField("active"))
			{
				JSONObject p_petPayload2 = jSONObject["active"];
				SetActivePetFromPayload(p_petPayload2, ref p_character);
			}
			else
			{
				Dbg.LogError("[ServiceCallUtils:GetPetDataFromPayload] Unable to retrieve active pet.");
			}
		}

		public static void SetActivePetFromPayload(JSONObject p_petPayload, ref CharacterAccountDetails p_characterDetails)
		{
			if (IsValidPayload(p_petPayload) && p_characterDetails != null)
			{
				p_characterDetails.activePet = GetStringField(p_petPayload, "pet_id", string.Empty);
			}
		}

		public static void SetPetDataFromPayload(JSONObject p_petPayload, out PetDetails p_petDetails)
		{
			p_petDetails = null;
			if (IsValidPayload(p_petPayload))
			{
				string stringField = GetStringField(p_petPayload, "pet_id", string.Empty);
				p_petDetails = new PetDetails(stringField);
			}
		}

		public static void GetFriendPetDataFromPayload(JSONObject p_characterPayload, ref CharacterAccountDetails p_characterDetgails)
		{
			if (p_characterPayload.HasField("active_pet"))
			{
				JSONObject p_petPayload = p_characterPayload["active_pet"];
				p_characterDetgails.unlockedPets = new Dictionary<string, PetDetails>();
				PetDetails p_petDetails = null;
				SetPetDataFromPayload(p_petPayload, out p_petDetails);
				if (p_petDetails != null && !p_characterDetgails.unlockedPets.ContainsKey(p_petDetails.m_petId))
				{
					p_characterDetgails.unlockedPets.Add(p_petDetails.m_petId, p_petDetails);
					p_characterDetgails.activePet = p_petDetails.m_petId;
				}
			}
		}

		public static void GetPromoCodes(JSONObject p_payload, ref List<PromoCodeData> p_promoCodeData)
		{
			if (!IsValidPayload(p_payload) || p_promoCodeData == null)
			{
				return;
			}
			Dbg.Log(" -----> " + p_payload.Count + " number of PromoCode", Dbg.NBLogType.SERVICES);
			p_promoCodeData.Clear();
			for (int i = 0; i < p_payload.Count; i++)
			{
				JSONObject jSONObject = p_payload[i];
				if (jSONObject != null)
				{
					PromoCodeData promoCodeData = new PromoCodeData();
					promoCodeData.ID = GetStringField(jSONObject, "id", string.Empty);
					promoCodeData.name = GetStringField(jSONObject, "title", string.Empty);
					promoCodeData.startTime = GetField<DateTime>(jSONObject, "start_datetime");
					promoCodeData.endTime = GetField<DateTime>(jSONObject, "end_datetime");
					promoCodeData.type = (RewardsType)GetField<int>(jSONObject, "type");
					promoCodeData.rewardsValue = GetStringField(jSONObject, "value", string.Empty);
					promoCodeData.tier = GetField<int>(jSONObject, "tier");
					promoCodeData.isActive = GetField<int>(jSONObject, "is_active") >= 1;
					promoCodeData.description = GetStringField(jSONObject, "description", string.Empty);
					promoCodeData.lastModifiedDate = GetField<DateTime>(jSONObject, "modified_datetime");
					p_promoCodeData.Add(promoCodeData);
				}
			}
		}

		public static void GetArmorCollection(JSONObject p_payload, ref CharacterAccountDetails p_characterAccountDetails)
		{
			if (!IsValidPayload(p_payload) || p_characterAccountDetails == null)
			{
				return;
			}
			p_characterAccountDetails.m_armorCollection.Clear();
			for (int i = 0; i < p_payload.Count; i++)
			{
				string str = p_payload[i].str;
				if (!p_characterAccountDetails.m_armorCollection.ContainsKey(str))
				{
					ServerInventoryItem value = new ServerInventoryItem(str);
					p_characterAccountDetails.m_armorCollection.Add(str, value);
				}
			}
		}

		public static bool PetErrorCodeHandler(int p_error, out string p_errMsg)
		{
			switch (p_error)
			{
			case 1300:
				p_errMsg = "Pet not found";
				return true;
			case 1301:
				p_errMsg = "Feed limit reached";
				return true;
			case 1302:
				p_errMsg = "Invalid request";
				return true;
			case 1303:
				p_errMsg = "Play limit reached";
				return true;
			case 1304:
				p_errMsg = "Existing";
				return true;
			case 1305:
				p_errMsg = "Unclaimed rewards";
				return true;
			case 1306:
				p_errMsg = "Unable to claim rewards";
				return true;
			case 1307:
				p_errMsg = "Invalid duration";
				return true;
			case 1308:
				p_errMsg = "Pet out hunting";
				return true;
			default:
				p_errMsg = "Unknown error: " + p_error;
				return false;
			}
		}

		public static int SetLeaderboardDataFromPayload(JSONObject p_payload, ref LeaderPlayer p_leadPlayer)
		{
			int num = -1;
			int result = 0;
			if (p_payload.HasField("rank"))
			{
				num = int.Parse(p_payload["rank"].str);
			}
			if (p_payload.HasField("character"))
			{
				SetCharacterFromPayload(p_payload["character"], ref p_leadPlayer.CharaAccountData);
				GetFriendPetDataFromPayload(p_payload["character"], ref p_leadPlayer.CharaAccountData);
			}
			else if (p_payload.HasField("char"))
			{
				SetCharacterFromPayload(p_payload["char"], ref p_leadPlayer.CharaAccountData);
			}
			if (p_payload.HasField("score") && !int.TryParse(p_payload["score"].str, out result))
			{
				result = 0;
			}
			if (num > 0)
			{
				p_leadPlayer.CharaName = p_leadPlayer.CharaAccountData.name;
				p_leadPlayer.Score = result;
				p_leadPlayer.Rank = num;
			}
			return num;
		}

		public static void SetDailyBonusDataFromPayload(JSONObject p_payload, ref DailyBonusData p_data)
		{
			string stringField = GetStringField(p_payload, "id", string.Empty);
			NBGeneric.TryParse<int>(stringField, out p_data.Id);
			stringField = GetStringField(p_payload, "daily_login_bonus_type_id", string.Empty);
			NBGeneric.TryParse<int>(stringField, out p_data.TypeId);
			stringField = GetStringField(p_payload, "sequence", string.Empty);
			NBGeneric.TryParse<int>(stringField, out p_data.Sequence);
			stringField = GetStringField(p_payload, "is_active", string.Empty);
			int p_outResult = 0;
			NBGeneric.TryParse<int>(stringField, out p_outResult);
			p_data.IsActive = p_outResult > 0;
			if (p_payload.HasField("value"))
			{
				p_data.Item = GetStringField(p_payload, "value", string.Empty);
				if (string.IsNullOrEmpty(p_data.Item) && p_payload.HasField("item"))
				{
					p_data.MyItemData = ParseDailyBonusItemData(p_payload["item"]);
				}
			}
			stringField = GetStringField(p_payload, "required_logins", string.Empty);
			NBGeneric.TryParse<int>(stringField, out p_data.RequireLogins);
			stringField = GetStringField(p_payload, "required_logins_in_week_flag", string.Empty);
			p_data.RequireLoginWeek = "1" == stringField;
			stringField = GetStringField(p_payload, "marked", string.Empty);
			p_data.Marked = "1" == stringField;
			stringField = GetStringField(p_payload, "progress", string.Empty);
			NBGeneric.TryParse<int>(stringField, out p_data.Progress);
		}

		private static DailyBonusData.ItemData ParseDailyBonusItemData(JSONObject p_payload)
		{
			DailyBonusData.ItemData itemData = new DailyBonusData.ItemData();
			if (p_payload.type != JSONObject.Type.NULL)
			{
				foreach (string key in p_payload.keys)
				{
					int p_outResult = 0;
					switch (key)
					{
					case "item_id":
						itemData.ItemId = p_payload[key].str;
						break;
					case "item_level":
						if (NBGeneric.TryParse<int>(p_payload[key].str, out p_outResult))
						{
							itemData.ItemLevel = p_outResult;
						}
						break;
					case "item_type":
						itemData.ItemType = p_payload[key].str;
						break;
					case "item_tier":
						if (NBGeneric.TryParse<int>(p_payload[key].str, out p_outResult))
						{
							itemData.ItemTier = p_outResult;
						}
						break;
					case "db_id":
						itemData.dbId = p_payload[key].str;
						break;
					}
				}
			}
			return itemData;
		}

		public static void SetPromotionFromPayload(JSONObject p_payload, ref PromotionData p_data)
		{
			p_data.Id = GetField<int>(p_payload, "id");
			p_data.PromoType = GetStringField(p_payload, "promotion_type", string.Empty);
			p_data.Name = GetStringField(p_payload, "name", string.Empty);
			p_data.EventId = GetStringField(p_payload, "event_id", string.Empty);
			p_data.StartDateTime = GetField<DateTime>(p_payload, "start_datetime");
			p_data.EndDateTime = GetField<DateTime>(p_payload, "end_datetime");
			p_data.ImgUrl = GetStringField(p_payload, "image_url", string.Empty);
			p_data.ImgMd5 = GetStringField(p_payload, "image_md5", string.Empty);
			p_data.ImgUrlEn = GetStringField(p_payload, "image_en_url", string.Empty);
			p_data.ImgMd5En = GetStringField(p_payload, "image_en_md5", string.Empty);
			p_data.ImgUrlJp = GetStringField(p_payload, "image_ja_url", string.Empty);
			p_data.ImgMd5Jp = GetStringField(p_payload, "image_ja_md5", string.Empty);
			p_data.ImgUrlCht = GetStringField(p_payload, "image_zh_hant_url", string.Empty);
			p_data.ImgMd5Cht = GetStringField(p_payload, "image_zh_hant_md5", string.Empty);
			p_data.ImgUrlChs = GetStringField(p_payload, "image_zh_hans_url", string.Empty);
			p_data.ImgMd5Chs = GetStringField(p_payload, "image_zh_hans_md5", string.Empty);
			p_data.ImgUrlKr = GetStringField(p_payload, "image_ko_url", string.Empty);
			p_data.ImgMd5Kr = GetStringField(p_payload, "image_ko_md5", string.Empty);
			if (!p_payload.HasField("event_info") && p_payload.HasField("items"))
			{
				JSONObject jSONObject = p_payload["items"];
				p_data.PromoItems.Clear();
				foreach (JSONObject item in jSONObject.list)
				{
					PromoItemData promoItemData = new PromoItemData();
					promoItemData.Id = GetField<int>(item, "id");
					promoItemData.PromoId = GetField<int>(item, "promotion_id");
					promoItemData.ItemId = GetStringField(item, "item_id", string.Empty);
					p_data.PromoItems.Add(promoItemData);
				}
			}
			if (p_payload.HasField("event_info"))
			{
				JSONObject p_payload3 = p_payload["event_info"];
				p_data.endTime = GetField<long>(p_payload3, "end_time");
			}
		}

		public static void GameSettingsFromPayload(JSONObject p_payload, ref GameSettings p_gameSettings)
		{
			if (IsValidPayload(p_payload))
			{
				p_gameSettings.hunt_15m_essence_chance = GetField<int>(p_payload, "core_game_setting_hunt_15m_essence_chance");
				p_gameSettings.loot_equipment_item_level_max = GetField<int>(p_payload, "core_game_setting_loot_equipment_item_level_max");
				p_gameSettings.loot_equipment_item_level_min = GetField<int>(p_payload, "core_game_setting_loot_equipment_item_level_min");
				p_gameSettings.loot_mob_drop_rate = GetField<int>(p_payload, "core_game_setting_loot_mob_drop_rate");
				p_gameSettings.notification_per_user_limit = GetField<int>(p_payload, "core_game_setting_notification_per_user_limit");
				p_gameSettings.loot_boss_fusion_drop_rate = GetField<int>(p_payload, "core_game_setting_loot_boss_fusion_drop_rate");
				p_gameSettings.hunt_12h_min_igc = GetField<int>(p_payload, "core_game_setting_hunt_12h_min_igc");
				p_gameSettings.hunt_5h_igc_chance = GetField<int>(p_payload, "core_game_setting_hunt_5h_igc_chance");
				p_gameSettings.pet_stat_decline_duration = GetField<int>(p_payload, "core_game_setting_pet_stat_decline_duration");
				p_gameSettings.hunt_12h_event_chance = GetField<int>(p_payload, "core_game_setting_hunt_12h_event_chance");
				p_gameSettings.hunt_5h_igc_chance = GetField<int>(p_payload, "core_game_setting_hunt_5h_max_igc");
				p_gameSettings.pet_stat_decline_grace_period = GetField<int>(p_payload, "core_game_setting_pet_stat_decline_grace_period");
				p_gameSettings.friend_assists_reward_amount = GetField<int>(p_payload, "core_game_setting_friend_assists_reward_amount");
				p_gameSettings.hunt_5h_event_chance = GetField<int>(p_payload, "core_game_setting_hunt_5h_event_chance");
				p_gameSettings.server_version = GetStringField(p_payload, "core_game_setting_server_version", string.Empty);
				p_gameSettings.play_pet_xp = GetField<int>(p_payload, "core_game_setting_play_pet_xp");
				p_gameSettings.feed_pet_xp = GetField<int>(p_payload, "core_game_setting_feed_pet_xp");
				p_gameSettings.hunt_15m_igc_chance = GetField<int>(p_payload, "core_game_setting_hunt_15m_igc_chance");
				p_gameSettings.hunt_12h_essence_chance = GetField<int>(p_payload, "core_game_setting_hunt_12h_essence_chance");
				p_gameSettings.hunt_5h_min_igc = GetField<int>(p_payload, "core_game_setting_hunt_5h_min_igc");
				p_gameSettings.unlock_city_premium_currency_amount = GetField<int>(p_payload, "core_game_setting_unlock_city_premium_currency_amount");
				p_gameSettings.session_timeout = GetField<int>(p_payload, "core_game_setting_session_timeout");
				p_gameSettings.hunt_15m_max_igc = GetField<int>(p_payload, "core_game_setting_hunt_15m_max_igc");
				p_gameSettings.play_pet_limit = GetField<int>(p_payload, "core_game_setting_play_pet_limit");
				p_gameSettings.play_pet_happiness = GetField<int>(p_payload, "core_game_setting_play_pet_limit");
				p_gameSettings.stamina_reset_time = GetField<float>(p_payload, "core_game_setting_stamina_reset_time");
				p_gameSettings.loot_level_multiplier = GetField<float>(p_payload, "core_game_setting_play_pet_happiness");
				p_gameSettings.shields_reset_time = GetField<int>(p_payload, "core_game_setting_shields_reset_time");
				p_gameSettings.already_unlocked_pet_xp = GetField<int>(p_payload, "core_game_setting_already_unlocked_pet_xp");
				p_gameSettings.hunt_12h_igc_chance = GetField<int>(p_payload, "core_game_setting_hunt_12h_igc_chance");
				p_gameSettings.store_bag_slot_increase_amount = GetField<int>(p_payload, "core_game_setting_store_bag_slot_increase_amount");
				p_gameSettings.hunt_15m_min_igc = GetField<int>(p_payload, "core_game_setting_hunt_15m_min_igc");
				p_gameSettings.hunt_5h_essence_chance = GetField<int>(p_payload, "core_game_setting_hunt_5h_essence_chance");
				p_gameSettings.survival_igc_multiplier = GetField<int>(p_payload, "core_game_setting_survival_igc_multiplier");
				p_gameSettings.hunt_15m_event_chance = GetField<int>(p_payload, "core_game_setting_hunt_15m_event_chance");
				p_gameSettings.feed_pet_limit = GetField<int>(p_payload, "core_game_setting_feed_pet_limit");
				p_gameSettings.hunt_12h_max_igc = GetField<int>(p_payload, "core_game_setting_hunt_12h_max_igc");
				p_gameSettings.friend_assists_reset_time = GetField<int>(p_payload, "core_game_setting_friend_assists_reset_time");
				p_gameSettings.feed_pet_loyalty = GetField<int>(p_payload, "core_game_setting_feed_pet_loyalty");
				p_gameSettings.slot_limitation = GetField<int>(p_payload, "core_game_setting_store_bag_slot_limitation");
				p_gameSettings.social_points_per_ticket = GetField<int>(p_payload, "core_game_setting_per_social_points_reward_ticket");
				p_gameSettings.global_lb_top1_reward_pgc = GetField<int>(p_payload, "core_game_setting_reward_lb_global_1st_premium");
				p_gameSettings.global_lb_top10_reward_pgc = GetField<int>(p_payload, "core_game_setting_reward_lb_global_top_10_premium");
				p_gameSettings.global_lb_top100_reward_pgc = GetField<int>(p_payload, "core_game_setting_reward_lb_global_top_100_premium");
				p_gameSettings.global_lb_top1000_reward_social = GetField<int>(p_payload, "core_game_setting_reward_lb_global_top_1000_social");
				p_gameSettings.power_move_score_multiplier = GetField<int>(p_payload, "core_game_setting_combat_power_move_score_multiplier");
				p_gameSettings.medal_criteria_wood = GetField<float>(p_payload, "core_game_setting_combat_medal_criteria_wood");
				p_gameSettings.medal_criteria_bronze = GetField<float>(p_payload, "core_game_setting_combat_medal_criteria_bronze");
				p_gameSettings.medal_criteria_silver = GetField<float>(p_payload, "core_game_setting_combat_medal_criteria_silver");
				p_gameSettings.medal_criteria_gold = GetField<float>(p_payload, "core_game_setting_combat_medal_criteria_gold");
				p_gameSettings.expected_specials_ratio = GetField<float>(p_payload, "core_game_setting_combat_expected_specials_ratio");
				p_gameSettings.multiplier_specials = GetField<int>(p_payload, "core_game_setting_combat_multiplier_specials");
				p_gameSettings.death_retry_cost = GetField<int>(p_payload, "core_game_setting_combat_death_retry_cost");
				p_gameSettings.gacha_premium_single_hd_bonus_min = GetField(p_payload, "core_game_setting_gacha_premium_single_hd_bonus_min", p_gameSettings.gacha_premium_single_hd_bonus_min);
				p_gameSettings.gacha_premium_single_hd_bonus_max = GetField(p_payload, "core_game_setting_gacha_premium_single_hd_bonus_max", p_gameSettings.gacha_premium_single_hd_bonus_max);
				p_gameSettings.gacha_premium_multi_hd_bonus_min = GetField(p_payload, "core_game_setting_gacha_premium_multi_hd_bonus_min", p_gameSettings.gacha_premium_multi_hd_bonus_min);
				p_gameSettings.gacha_premium_multi_hd_bonus_max = GetField(p_payload, "core_game_setting_gacha_premium_multi_hd_bonus_max", p_gameSettings.gacha_premium_multi_hd_bonus_max);
				p_gameSettings.event_easy_cost = GetField<int>(p_payload, "core_game_setting_event_easy_cost");
				p_gameSettings.event_normal_cost = GetField<int>(p_payload, "core_game_setting_event_normal_cost");
				p_gameSettings.event_hard_cost = GetField<int>(p_payload, "core_game_setting_event_hard_cost");
				p_gameSettings.enable_local_push_notification = GetField<int>(p_payload, "core_game_setting_enable_local_push_notification") > 0;
				p_gameSettings.enable_promo_code = GetField<int>(p_payload, "core_game_setting_enable_promo_code") > 0;
			}
		}

		public static void PatchInfoFromPayload(JSONObject p_payload, ref PatchInfo p_patchInfo)
		{
			if (!IsValidPayload(p_payload))
			{
				return;
			}
			p_patchInfo.version = p_payload["server_version"].str;
			JSONObject jSONObject = p_payload["files"];
			if (jSONObject.type == JSONObject.Type.NULL || jSONObject.Count <= 0 || jSONObject.IsEmptyArray())
			{
				return;
			}
			foreach (JSONObject item in jSONObject.list)
			{
				PatchFileInfo patchFileInfo = new PatchFileInfo();
				patchFileInfo.filename = item.GetField("asset_filename").str;
				patchFileInfo.size = long.Parse(item.GetField("asset_size").str);
				patchFileInfo.checksum = item.GetField("asset_crc").str;
				patchFileInfo.version = item.GetField("asset_version").str;
				patchFileInfo.url = item.GetField("asset_url").str;
				p_patchInfo.files.Add(patchFileInfo);
			}
		}

		public static void ServerStoreFromPayload(JSONObject p_payload, ref List<ServerStoreItem> p_storeItems)
		{
			if (IsValidPayload(p_payload))
			{
				Dbg.Log(" -----> " + p_payload.Count + " number of itemTypes.", Dbg.NBLogType.SERVICES);
				for (int i = 0; i < p_payload.Count; i++)
				{
					JSONObject jSONObject = p_payload[i];
					ServerStoreItem serverStoreItem = new ServerStoreItem();
					serverStoreItem.db_id = jSONObject["id"].str;
					serverStoreItem.type = int.Parse(jSONObject["store_item_type_id"].str);
					serverStoreItem.name = jSONObject["name"].str;
					serverStoreItem.denomination = int.Parse(jSONObject["denomination"].str);
					serverStoreItem.price = float.Parse(jSONObject["price"].str);
					serverStoreItem.isActive = int.Parse(jSONObject["is_active"].str) > 0;
					serverStoreItem.percentDiscount = float.Parse(jSONObject["percent_discount"].str);
					serverStoreItem.discountedPrice = serverStoreItem.price - serverStoreItem.price * serverStoreItem.percentDiscount;
					serverStoreItem.spriteName = jSONObject["sprite_name"].str;
					serverStoreItem.productId = GetStringField(jSONObject, "ios_product_id", string.Empty);
					serverStoreItem.androidProductID = GetStringField(jSONObject, "android_product_id", string.Empty);
					p_storeItems.Add(serverStoreItem);
				}
			}
		}

		public static string ArrayToJSONString(JSONObject p_json)
		{
			if (p_json == null || p_json.type == JSONObject.Type.NULL || p_json.list == null || p_json.list.Count <= 0 || p_json.Count <= 0 || p_json.IsEmptyArray())
			{
				return string.Empty;
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("[");
			int i;
			for (i = 0; i < p_json.Count - 1; i++)
			{
				stringBuilder.Append("\"" + p_json[i].str + "\",");
			}
			stringBuilder.Append("\"" + p_json[i].str + "\"]");
			return stringBuilder.ToString();
		}

		public static void ShopItemsFromPayload(JSONObject p_payload, ref List<ShopData> p_shopItems, CurrencyEnum p_currencyType)
		{
			if (!IsValidPayload(p_payload) || p_shopItems == null)
			{
				return;
			}
			Dbg.Log(" -----> " + p_payload.Count + " shop items.", Dbg.NBLogType.SERVICES);
			for (int i = 0; i < p_payload.Count; i++)
			{
				ShopData shopData = null;
				JSONObject p_payload2 = p_payload[i];
				string stringField = GetStringField(p_payload2, "id", string.Empty);
				string stringField2 = GetStringField(p_payload2, "title", string.Empty);
				string stringField3 = GetStringField(p_payload2, "desc", string.Empty);
				int field = GetField<int>(p_payload2, "price");
				string stringField4 = GetStringField(p_payload2, "item", string.Empty);
				if (stringField4 != string.Empty && stringField4.StartsWith("PET"))
				{
					shopData = new ShopDataPet(stringField, stringField2, stringField3, field, p_currencyType, stringField4);
				}
				else if (stringField4 != string.Empty)
				{
					ServerInventoryItem p_item = new ServerInventoryItem(stringField4);
					shopData = new ShopDataItem(stringField, stringField2, stringField3, field, p_currencyType, p_item);
				}
				else
				{
					shopData = new ShopDataGacha(stringField, stringField2, stringField3, field, p_currencyType);
				}
				if (shopData != null && shopData.IsValid)
				{
					p_shopItems.Add(shopData);
				}
			}
		}

		public static void GetInventoryFromPayload(JSONObject p_payload, ref List<ServerInventoryItem> p_inventory)
		{
			if (!IsValidPayload(p_payload) || p_payload.Count < 0 || p_payload.IsEmptyArray())
			{
				return;
			}
			foreach (JSONObject item in p_payload.list)
			{
				ServerInventoryItem p_item = null;
				SetItemFromPayload(item, ref p_item);
				p_inventory.Add(p_item);
			}
		}

		public static void GetGachaEvents(JSONObject p_payload, ref List<GachaEventsInfo> p_gachaEvents)
		{
			if (!IsValidPayload(p_payload) || p_payload.Count < 0 || p_payload.IsEmptyArray())
			{
				return;
			}
			foreach (JSONObject item in p_payload.list)
			{
				GachaEventsInfo gachaEventsInfo = new GachaEventsInfo();
				gachaEventsInfo.id = GetStringField(item, "id", string.Empty);
				gachaEventsInfo.gacha_type_id = GetStringField(item, "gacha_type_id", string.Empty);
				gachaEventsInfo.gacha_status_id = GetStringField(item, "gacha_status_id", string.Empty);
				gachaEventsInfo.name = GetStringField(item, "name", string.Empty);
				gachaEventsInfo.date_created = GetStringField(item, "date_created", string.Empty);
				gachaEventsInfo.start_date = GetStringField(item, "start_date", string.Empty);
				gachaEventsInfo.end_date = GetStringField(item, "end_date", string.Empty);
				gachaEventsInfo.price = GetField<float>(item, "price");
				gachaEventsInfo.reward_item_type = GetStringField(item, "reward_item_type", string.Empty);
				gachaEventsInfo.reward_amount = GetField<int>(item, "reward_amount");
				gachaEventsInfo.reward_same_set_flag = GetStringField(item, "reward_same_set_flag", string.Empty);
				gachaEventsInfo.pay_times = GetField<int>(item, "pay_times");
				gachaEventsInfo.free_times = GetField<int>(item, "free_times");
				gachaEventsInfo.is_ticket_usable = GetField<int>(item, "is_ticket_usable") > 0;
				gachaEventsInfo.filter_by = GetStringField(item, "filter_by", string.Empty);
				gachaEventsInfo.element = GetStringField(item, "element", string.Empty);
				if (item.HasField("sets"))
				{
					ParseCollections(item["sets"], ref gachaEventsInfo.sets);
				}
				p_gachaEvents.Add(gachaEventsInfo);
			}
		}

		private static void ParseCollections(JSONObject p_jsonObj, ref List<List<ChestDetails>> p_sets)
		{
			p_sets.Clear();
			foreach (JSONObject item in p_jsonObj.list)
			{
				if (!item.HasField("items"))
				{
					continue;
				}
				List<ChestDetails> list = new List<ChestDetails>();
				foreach (JSONObject item2 in item.list)
				{
					ChestDetails chestDetails = new ChestDetails();
					chestDetails.item_id = GetStringField(item2, "item_id", string.Empty);
					chestDetails.item_level = GetField<int>(item2, "item_level");
					chestDetails.item_tier = GetField<int>(item2, "item_tier");
					list.Add(chestDetails);
				}
				p_sets.Add(list);
			}
		}

		public static void GetAchievements(JSONObject p_payload, ref Dictionary<int, AchievementManager.AchievementItem> p_achievements)
		{
			if (!IsValidPayload(p_payload) || p_payload.Count < 0 || p_payload.IsEmptyArray())
			{
				return;
			}
			foreach (JSONObject item in p_payload.list)
			{
				AchievementManager.AchievementItem achievementItem = ParseSingleAchievement(item);
				int result = -1;
				if (int.TryParse(achievementItem.db_id, out result))
				{
					p_achievements.Add(result, achievementItem);
				}
			}
		}

		private static AchievementManager.AchievementItem ParseSingleAchievement(JSONObject p_payload)
		{
			AchievementManager.AchievementItem achievementItem = new AchievementManager.AchievementItem();
			foreach (string key in p_payload.keys)
			{
				int p_outResult;
				switch (key)
				{
				case "id":
					achievementItem.db_id = p_payload[key].str;
					break;
				case "achievement_id":
					achievementItem.id = p_payload[key].str;
					break;
				case "ios_achievement_id":
					achievementItem.ios_id = p_payload[key].str;
					break;
				case "achievement_name":
					achievementItem.title = p_payload[key].str;
					break;
				case "description":
					achievementItem.description = p_payload[key].str;
					break;
				case "reward_xp":
					p_outResult = 0;
					if (NBGeneric.TryParse<int>(p_payload[key].str, out p_outResult))
					{
						achievementItem.rewards.rewardXP = p_outResult;
						break;
					}
					Dbg.LogWarn("Unable to parse the reward XP");
					achievementItem.rewards.rewardXP = 0;
					break;
				case "reward_igc":
					if (NBGeneric.TryParse<int>(p_payload[key].str, out p_outResult))
					{
						achievementItem.rewards.rewardIgc = p_outResult;
						break;
					}
					Dbg.LogWarn("Unable to parse the reward IGC");
					achievementItem.rewards.rewardIgc = 0;
					break;
				case "tracking":
					if (NBGeneric.TryParse<int>(p_payload[key].str, out p_outResult))
					{
						achievementItem.targetProgress = p_outResult;
						break;
					}
					Dbg.LogWarn("Unable to parse the achievement tracking");
					achievementItem.targetProgress = 999;
					break;
				case "item_id":
				{
					string str2 = p_payload[key].str;
					if (str2.Contains(","))
					{
						string[] array3 = str2.Split(',');
						string[] array4 = array3;
						foreach (string item2 in array4)
						{
							achievementItem.rewards.rewardItemIds.Add(item2);
						}
					}
					else if (!string.IsNullOrEmpty(str2))
					{
						achievementItem.rewards.rewardItemIds.Add(str2);
					}
					break;
				}
				case "pet_id":
				{
					string str = p_payload[key].str;
					if (str.Contains(","))
					{
						string[] array = str.Split(',');
						string[] array2 = array;
						foreach (string item in array2)
						{
							achievementItem.rewards.rewardPetIds.Add(item);
						}
					}
					else if (!string.IsNullOrEmpty(str))
					{
						achievementItem.rewards.rewardPetIds.Add(str);
					}
					break;
				}
				case "diamond":
					if (NBGeneric.TryParse<int>(p_payload[key].str, out p_outResult))
					{
						achievementItem.rewards.rewardPrem = p_outResult;
						break;
					}
					Dbg.LogWarn("Unable to parse the achievement reward PGC");
					achievementItem.rewards.rewardPrem = 0;
					break;
				case "unlocked":
					achievementItem.Achieved = p_payload[key].str == "1";
					break;
				case "is_marked":
					achievementItem.marked = p_payload[key].str == "1";
					break;
				case "is_claimed":
					achievementItem.Claimed = p_payload[key].str == "1";
					break;
				case "current_progress":
					if (NBGeneric.TryParse<int>(p_payload[key].str, out p_outResult))
					{
						achievementItem.progress = p_outResult;
						break;
					}
					Dbg.LogWarn("Unable to parse the achievement progress");
					achievementItem.progress = 0;
					break;
				}
			}
			if (achievementItem.Achieved)
			{
				achievementItem.progress = achievementItem.targetProgress;
			}
			achievementItem.displayed = MBPrefs.GetInt(achievementItem.id + ".displayed") > 0;
			return achievementItem;
		}

		public static AnnouncementData ParseFriendNotification(string p_str)
		{
			AnnouncementData announcementData = new AnnouncementData();
			p_str = p_str.Replace("{", string.Empty);
			p_str = p_str.Replace("}", string.Empty);
			p_str = p_str.Replace("\n", string.Empty);
			p_str = p_str.Replace("\t", string.Empty);
			p_str = p_str.Replace("\"", string.Empty);
			string[] array = p_str.Split(',');
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (!text.Contains(":"))
				{
					continue;
				}
				if (text.Contains("type"))
				{
					string text2 = text.Split(':')[1];
					if ("friend_assists" == text2)
					{
						announcementData.AnnTypeId = AnnoucementTypeId.MAX;
					}
				}
				else if (text.Contains("message"))
				{
					announcementData.AnnounceMessage.Body = text.Split(':')[1];
				}
				else if (text.Contains("ts"))
				{
					long p_outResult = 0L;
					NBGeneric.TryParse<long>(text.Split(':')[1], out p_outResult);
					announcementData.DbId = (int)p_outResult;
				}
			}
			return announcementData;
		}

		public static AnnouncementData ParseAnnouncementData(JSONObject p_payload)
		{
			AnnouncementData announcementData = new AnnouncementData();
			string stringField = GetStringField(p_payload, "id", string.Empty);
			NBGeneric.TryParse<int>(stringField, out announcementData.DbId);
			stringField = GetStringField(p_payload, "announcement_type_id", string.Empty);
			int p_outResult = 0;
			NBGeneric.TryParse<int>(stringField, out p_outResult);
			announcementData.AnnTypeId = (AnnoucementTypeId)p_outResult;
			announcementData.Name = GetStringField(p_payload, "name", string.Empty);
			announcementData.StartDate = GetStringField(p_payload, "start_date", string.Empty);
			announcementData.EndDate = GetStringField(p_payload, "end_date", string.Empty);
			announcementData.EventId = GetStringField(p_payload, "event_id", string.Empty);
			if (p_payload.HasField("message"))
			{
				announcementData.AnnounceMessage = ParseAnnouncementMessage(p_payload["message"]);
			}
			if (p_payload.HasField("attachments"))
			{
				announcementData.AnnouncementRewards = ParseAnnouncementRewards(p_payload["attachments"]);
			}
			if (p_payload.HasField("attachment_claim"))
			{
				bool claimed = ParseAnnouncementClaimed(p_payload["attachment_claim"]);
				foreach (int key in announcementData.AnnouncementRewards.Keys)
				{
					announcementData.AnnouncementRewards[key].Claimed = claimed;
				}
			}
			return announcementData;
		}

		public static AnnouncementMessage ParseAnnouncementMessage(JSONObject p_payload)
		{
			AnnouncementMessage announcementMessage = new AnnouncementMessage();
			if (p_payload != null && p_payload.type != JSONObject.Type.NULL)
			{
				string stringField = GetStringField(p_payload, "id", string.Empty);
				NBGeneric.TryParse<int>(stringField, out announcementMessage.DbId);
				stringField = GetStringField(p_payload, "announcement_id", string.Empty);
				NBGeneric.TryParse<int>(stringField, out announcementMessage.AnnoucementId);
				announcementMessage.Title = GetStringField(p_payload, "title", string.Empty);
				announcementMessage.Body = ConvertNewline(GetStringField(p_payload, "body", string.Empty));
				announcementMessage.Language = GetStringField(p_payload, "lang", string.Empty);
				announcementMessage.ImgKey = GetStringField(p_payload, "image_key", string.Empty);
				announcementMessage.ImgUrl = GetStringField(p_payload, "image_url", string.Empty);
				announcementMessage.ImgMd5 = GetStringField(p_payload, "image_md5", string.Empty);
			}
			return announcementMessage;
		}

		public static Dictionary<int, AnnouncementReward> ParseAnnouncementRewards(JSONObject p_payload)
		{
			Dictionary<int, AnnouncementReward> dictionary = new Dictionary<int, AnnouncementReward>();
			if (p_payload != null && p_payload.type != JSONObject.Type.NULL && !p_payload.IsEmptyArray())
			{
				AnnouncementReward announcementReward = null;
				foreach (JSONObject item in p_payload.list)
				{
					announcementReward = new AnnouncementReward();
					string stringField = GetStringField(item, "id", string.Empty);
					NBGeneric.TryParse<int>(stringField, out announcementReward.DbId);
					stringField = GetStringField(item, "announcement_id", string.Empty);
					NBGeneric.TryParse<int>(stringField, out announcementReward.AnnouncementId);
					stringField = GetStringField(item, "announcement_attachment_type_id", string.Empty);
					NBGeneric.TryParse<int>(stringField, out announcementReward.TypeId);
					stringField = GetStringField(item, "attachment_value", string.Empty);
					NBGeneric.TryParse<int>(stringField, out announcementReward.Amount);
					announcementReward.TypeName = GetStringField(item, "announcement_attachment_type_name", string.Empty);
					if (announcementReward.TypeName == "Item")
					{
						announcementReward.ItemId = item["attachment_value"].str;
						announcementReward.Amount = 1;
					}
				}
				dictionary.Add(announcementReward.DbId, announcementReward);
			}
			return dictionary;
		}

		public static bool ParseAnnouncementClaimed(JSONObject p_payload)
		{
			if (p_payload.ToString() == "\"\"")
			{
				return false;
			}
			if (p_payload != null && p_payload.type != JSONObject.Type.NULL)
			{
				return true;
			}
			return false;
		}

		public static Services.ConnectionResults GetStartDungeonFromPayload(JSONObject p_payload, ref ServerBattleDetails p_battleDetails)
		{
			if (!IsValidPayload(p_payload) || p_payload.Count < 0 || p_payload.IsEmptyArray() || p_battleDetails == null)
			{
				return Services.ConnectionResults.WrongPayload;
			}
			p_battleDetails.dungeon_key = GetStringField(p_payload, "dungeon_key", string.Empty);
			JSONObject field = p_payload.GetField("monster_ids");
			int num = 0;
			if (field != null && field.type != JSONObject.Type.NULL && !field.IsEmptyArray())
			{
				num = field.Count;
				for (int i = 0; i < num; i++)
				{
					p_battleDetails.monster_ids.Add(GetStringField(field, "monster_id_" + i, string.Empty));
				}
			}
			if (Dbg.Assert(num > 0, "No battle found from the paylod"))
			{
				return Services.ConnectionResults.WrongPayload;
			}
			JSONObject field2 = p_payload.GetField("chest_map");
			if (field2 != null && field2.type != JSONObject.Type.NULL && !field2.IsEmptyArray())
			{
				for (int j = 0; j < num; j++)
				{
					if (!field2.keys.Contains("monster_id" + j))
					{
						p_battleDetails.chest_list.Add(null);
						continue;
					}
					ChestDetails chestDetails = new ChestDetails();
					JSONObject field3 = field2.GetField("monster_id" + j);
					if (field3 == null || field3.type == JSONObject.Type.NULL || field3.IsEmptyArray())
					{
						continue;
					}
					foreach (JSONObject item in field3.list)
					{
						if (item != null)
						{
							chestDetails.monster_id = p_battleDetails.monster_ids[j];
							chestDetails.item_id = GetStringField(item, "item_id", string.Empty);
							chestDetails.item_type = GetStringField(item, "item_type", string.Empty);
							string stringField = GetStringField(item, "item_level", string.Empty);
							stringField = ((!(stringField == string.Empty)) ? stringField : "1");
							NBGeneric.TryParse<int>(stringField, out chestDetails.item_level);
							string stringField2 = GetStringField(item, "item_tier", string.Empty);
							stringField2 = ((!(stringField2 == string.Empty)) ? stringField2 : "1");
							NBGeneric.TryParse<int>(stringField2, out chestDetails.item_tier);
							chestDetails.chest_type = GetStringField(item, "chest_type", string.Empty);
						}
						p_battleDetails.chest_list.Add(chestDetails);
					}
				}
			}
			return Services.ConnectionResults.Success;
		}

		public static string GetStringField(JSONObject p_payload, string p_field, string p_defaultStr = "")
		{
			if (!IsValidPayload(p_payload) || !p_payload.HasField(p_field))
			{
				Dbg.LogWarn("Could not find field from payload : " + p_field, Dbg.NBLogType.SERVICES);
				return p_defaultStr;
			}
			return p_payload[p_field].str;
		}

		public static string ConvertNewline(string p_str)
		{
			if (p_str == null)
			{
				return string.Empty;
			}
			p_str = p_str.Replace("[newline]", "\n");
			return p_str;
		}

		public static T GetField<T>(JSONObject p_payload, string p_field)
		{
			return GetField(p_payload, p_field, default(T));
		}

		public static T GetField<T>(JSONObject p_payload, string p_field, T p_default)
		{
			T p_outResult = p_default;
			string stringField = GetStringField(p_payload, p_field, string.Empty);
			if (stringField == string.Empty)
			{
				return p_outResult;
			}
			if (!NBGeneric.TryParse(stringField, out p_outResult))
			{
				p_outResult = p_default;
			}
			return p_outResult;
		}

		public static int ErrorCode(Services.ServiceHandle p_handle)
		{
			return (p_handle != null) ? p_handle.nReturnCode : (-1);
		}

		public static string GetErrorMessage(Services.ServiceHandle p_handle)
		{
			if (p_handle == null)
			{
				return "(NULL)";
			}
			string text = null;
			if (p_handle.IsNoInternet())
			{
				return "(NO INTERNET)";
			}
			return string.Concat(str3: (p_handle.returnJson == null || !p_handle.returnJson.HasField("message")) ? "(EMPTY)" : p_handle.returnJson["message"].str, str0: "[", str1: p_handle.nReturnCode.ToString(), str2: "] ");
		}

		public static string FormatMessage(string p_key, Services.ServiceHandle p_handle)
		{
			if (p_handle == null)
			{
				return SingletonComponent<NBLocalization>.Instance.Get(p_key);
			}
			return "[Error:" + p_handle.nReturnCode + "] " + SingletonComponent<NBLocalization>.Instance.Get(p_key);
		}

		public static bool IsEnterDungeonError(Services.ServiceHandle p_handle)
		{
			return p_handle != null && p_handle.nReturnCode >= 1200 && p_handle.nReturnCode <= 1206;
		}
	}
}
