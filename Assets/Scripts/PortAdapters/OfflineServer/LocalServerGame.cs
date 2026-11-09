// Offline server: game state - start-up sections, character, dungeons and events, rewards. Registers CD-005, CD-012, CD-037.
using System;
using System.Collections.Generic;
using System.Text;

public static partial class LocalServer
{
	private class Session
	{
		public string eventId;
		public string dungeonId;
		public List<string> monsters = new List<string>();
		public List<Chest> chests;
		public Chest mystery;
		public int difficulty = 2;
	}

	private static readonly Dictionary<string, Session> s_sessions = new Dictionary<string, Session>();

	private static int s_sessionCounter;

	private static readonly Dictionary<string, string> s_endReplies = new Dictionary<string, string>();

	private static readonly Random s_rng = new Random();

	private static readonly string[] TutorialEvents = new string[] { "TUTORIAL_1", "TUTORIAL_2", "TUTORIAL_3" };

	private static string GameInitSections()
	{
		OfflineProfile p = OfflineProfile.Current;
		long now = Now();
		string s = ",\"character\":" + CharacterJson(p, now)
			+ ",\"pets\":" + PetsJson(p)
			+ ",\"events\":" + EventsJson(p)
			+ ",\"main_event\":{\"event_id\":" + Str(TutorialFinished(p) ? CurrentGodEvent() : "") + "}"
			+ ",\"unlocked_dungeons\":" + UnlockedJson(p)
			+ ",\"inventory\":" + InventoryJson(p)
			+ ",\"store_items\":" + StoreItemsJson()
			+ ",\"armor_collection\":" + ArmorCollectionJson(p)
			+ ",\"gacha\":" + GachaEventsJson() + ",\"achievements\":" + AchievementsJson(p) + ",\"gift_senders\":[]"
			+ ",\"finished_tutorials\":" + FinishedTutorialsJson(p) + ",\"other_tutorial_progression\":" + OtherTutorialJson(p);
		p.Save();
		return s;
	}

	private static string FinishedTutorialsJson(OfflineProfile p)
	{
		StringBuilder sb = new StringBuilder("{");
		foreach (string id in TutorialEvents)
		{
			if (p.cleared_events.Contains(id))
			{
				Field(sb, id, "1");
			}
		}
		return sb.Length > 1 ? Close(sb) : "[]";
	}

	private static string OtherTutorialJson(OfflineProfile p)
	{
		if (p.cleared_events.Count == 0 && p.tutorial_flags.Count == 0)
		{
			return "[]";
		}
		StringBuilder sb = new StringBuilder("{");
		foreach (string k in new string[] { "fusion", "gacha", "gacha_premium", "pet_hunt", "equip_item", "name_input" })
		{
			Field(sb, k, p.tutorial_flags.Contains(k) ? "1" : "0");
		}
		return Close(sb);
	}

	private static void TutorialFlag(Dictionary<string, string> f, string key)
	{
		OfflineProfile p = OfflineProfile.Current;
		if (key == "name_input" && Get(f, "name").Length > 0 && p.name != Get(f, "name"))
		{
			p.name = Get(f, "name");
			p.Save();
		}
		if (Get(f, "is_tutorial") == "1" && !p.tutorial_flags.Contains(key))
		{
			p.tutorial_flags.Add(key);
			p.Save();
		}
	}

	private static string CharacterJson(OfflineProfile p, long now)
	{
		return "{" + CharacterFields(p, now) + "}";
	}

	private static string CharacterFields(OfflineProfile p, long now)
	{
		int nextStamina = p.RegenStamina(now);
		int nextShield = p.RegenShields(now);
		StringBuilder sb = new StringBuilder();
		Field(sb, "id", OfflineServerMode.NubeeId); Field(sb, "nubee_id", OfflineServerMode.NubeeId);
		Field(sb, "facebook_id", ""); Field(sb, "weibo_id", ""); Field(sb, "name", p.name); Field(sb, "active_title", "");
		Field(sb, "level", p.level); Field(sb, "exp", p.exp);
		Field(sb, "strength", p.strength); Field(sb, "agility", p.agility); Field(sb, "constitution", p.constitution);
		Field(sb, "date_created", ""); Field(sb, "date_updated", "");
		Field(sb, "max_inventory_slots", p.max_inventory_slots); Field(sb, "used_inventory_slots", p.inventory.Count);
		Field(sb, "max_stamina", p.max_stamina); Field(sb, "max_shields", p.max_shields);
		Field(sb, "stamina", p.stamina); Field(sb, "shields", p.shields);
		Field(sb, "last_stamina_refreshed_ts", nextStamina); Field(sb, "last_shields_refreshed_ts", nextShield);
		Field(sb, "next_warcry", 0);
		Field(sb, "total_friend_count", 0); Field(sb, "friend_limit", p.friend_limit); Field(sb, "pending_friends_count", 0);
		Field(sb, "IAPban", 0);
		sb.Append(",\"currencies\":{");
		int start = sb.Length;
		Field(sb, "social", p.social); Field(sb, "premium", p.premium); Field(sb, "in_game", p.in_game);
		Field(sb, "premium_gacha_ticket", p.premium_gacha_ticket); Field(sb, "premium_gacha_gift_ticket", p.premium_gacha_gift_ticket);
		Field(sb, "social_gacha_ticket", p.social_gacha_ticket); Field(sb, "medal", p.medal);
		Field(sb, "dragon_point", p.dragon_point); Field(sb, "holy_diamond", p.holy_diamond);
		sb.Append('}');
		sb.Append(",\"equipments\":[");
		bool first = true;
		foreach (OfflineItem it in p.inventory)
		{
			if (!it.is_equipped) continue;
			if (!first) sb.Append(',');
			first = false;
			sb.Append(ItemJson(it));
		}
		sb.Append(']');
		sb.Append(",\"leaderboard\":{\"rank\":\"-1\",\"score\":\"0\"}");
		string s = sb.ToString();
		return s.StartsWith(",") ? s.Substring(1) : s;
	}

	private static string ItemJson(OfflineItem it)
	{
		StringBuilder sb = new StringBuilder("{");
		Field(sb, "db_id", it.db_id); Field(sb, "item_id", it.item_id); Field(sb, "item_type", it.item_type);
		Field(sb, "item_level", it.item_level); Field(sb, "item_tier", it.item_tier); Field(sb, "fusion_xp", it.fusion_xp);
		Field(sb, "is_equipped", it.is_equipped ? 1 : 0);
		return Close(sb);
	}

	private static string InventoryJson(OfflineProfile p)
	{
		List<string> items = new List<string>();
		foreach (OfflineItem it in p.inventory) items.Add(ItemJson(it));
		return Array(items);
	}

	private static string ArmorCollectionJson(OfflineProfile p)
	{
		p.RecordCollection();
		List<string> ids = new List<string>();
		foreach (string k in p.collection) ids.Add(Str(k));
		return Array(ids);
	}

	private static string UnlockedJson(OfflineProfile p)
	{
		Dictionary<string, List<OfflineDungeon>> byCity = new Dictionary<string, List<OfflineDungeon>>();
		List<string> order = new List<string>();
		foreach (OfflineDungeon d in p.unlocked)
		{
			if (!byCity.ContainsKey(d.city_id)) { byCity[d.city_id] = new List<OfflineDungeon>(); order.Add(d.city_id); }
			byCity[d.city_id].Add(d);
		}
		List<string> cities = new List<string>();
		foreach (string city in order)
		{
			List<string> ds = new List<string>();
			foreach (OfflineDungeon d in byCity[city])
			{
				StringBuilder sb = new StringBuilder("{");
				Field(sb, "dungeon_id", d.dungeon_id); Field(sb, "highest_score", d.highest_score); Field(sb, "is_cleared", d.is_cleared ? 1 : 0);
				ds.Add(Close(sb));
			}
			cities.Add("{\"city_id\":" + Str(city) + ",\"dungeons\":" + Array(ds) + "}");
		}
		return Array(cities);
	}

	private static string UnlockedDungeonsJson(string cityId)
	{
		List<string> ds = new List<string>();
		foreach (OfflineDungeon d in OfflineProfile.Current.unlocked)
		{
			if (d.city_id != cityId) continue;
			StringBuilder sb = new StringBuilder("{");
			Field(sb, "dungeon_id", d.dungeon_id); Field(sb, "highest_score", d.highest_score); Field(sb, "is_cleared", d.is_cleared ? 1 : 0);
			ds.Add(Close(sb));
		}
		return Array(ds);
	}

	private static string UnlockedCitiesJson()
	{
		List<string> seen = new List<string>();
		List<string> list = new List<string>();
		foreach (OfflineDungeon d in OfflineProfile.Current.unlocked)
		{
			if (d.city_id != null && !seen.Contains(d.city_id))
			{
				seen.Add(d.city_id);
				list.Add("{\"city_id\":" + Str(d.city_id) + "}");
			}
		}
		return Array(list);
	}

	public static string TestEventDay = TestDayArg();

	private static string TestDayArg()
	{
		return null;
	}

	public static string[] TodaysWeekdayEvent(OfflineProfile p)
	{
		OfflineTable ev = OfflineTables.Get("Events");
		string today = TestEventDay ?? DateTime.Now.DayOfWeek.ToString().ToUpperInvariant();
		string[] best = null, lowest = null;
		foreach (string[] r in ev.Rows)
		{
			if (ev.Get(r, "EVENT_ID") != today) continue;
			int min = ev.GetInt(r, "MIN");
			if (lowest == null || min < ev.GetInt(lowest, "MIN")) lowest = r;
			if (min <= p.level + 4 && (best == null || min > ev.GetInt(best, "MIN"))) best = r;
		}
		return best ?? lowest;
	}

	private static bool TutorialFinished(OfflineProfile p)
	{
		foreach (string id in TutorialEvents) if (!p.cleared_events.Contains(id)) return false;
		return true;
	}

	private static string EventsJson(OfflineProfile p)
	{
		bool tutorialDone = TutorialFinished(p);
		List<string> list = new List<string>();
		if (!tutorialDone)
		{
			OfflineTable ev = OfflineTables.Get("Events");
			foreach (string id in TutorialEvents)
			{
				list.Add(EventJson(ev, ev.Row(id), p));
				if (!p.cleared_events.Contains(id)) break;
			}
		}
		else
		{
			OfflineTable ev = OfflineTables.Get("Events");
			string today = TestEventDay ?? DateTime.Now.DayOfWeek.ToString().ToUpperInvariant();
			long end = (long)(DateTime.Today.AddDays(1).ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
			string[] wr = TodaysWeekdayEvent(p);
			if (wr != null) list.Add(EventJson(ev, wr, p).TrimEnd('}') + ",\"end_time\":" + Str(end) + "}");
			foreach (GodEvent g in s_godEvents)
			{
				list.Add(EventJson(ev, GodEventRow(g.id, p.level, 2), p).TrimEnd('}') + ",\"end_time\":" + Str(GodEventEnd(g)) + "}");
			}
		}
		return Array(list);
	}

	private static string EventName(string name, string boss)
	{
		if (!name.StartsWith("content_event_")) return Localized(name);
		if (boss.StartsWith("METAL_BOBODO")) return "Feathers Of Silver";
		if (boss.StartsWith("METAL_DODOBO")) return "Eggs Of Gold";
		return name;
	}

	private static string Localized(string key)
	{
		NBLocalization loc = SingletonComponent<NBLocalization>.Instance;
		if (loc == null || string.IsNullOrEmpty(key)) return key;
		string t = loc.Get(key);
		return string.IsNullOrEmpty(t) || t == key ? key : t.Trim();
	}

	private static string[] TutorialLineup(OfflineTable ev, string eventId, string[] r)
	{
		return eventId == "TUTORIAL_3" && ev.Row("TUTORIAL_4") != null ? ev.Row("TUTORIAL_4") : r;
	}

	private static string EventJson(OfflineTable ev, string[] r, OfflineProfile p)
	{
		StringBuilder sb = new StringBuilder("{");
		string id = ev.Get(r, "ID");
		r = TutorialLineup(ev, id, r);
		string name = EventName(ev.Get(r, "NAME"), ev.Get(r, "BOSS_MONSTER_ID"));
		Field(sb, "id", id); Field(sb, "boss_monster_id", ev.Get(r, "BOSS_MONSTER_ID")); Field(sb, "event_city_id", ev.Get(r, "EVENT_ID"));
		Field(sb, "stage", ev.Get(r, "STAGE")); Field(sb, "type", ev.Get(r, "TYPE")); Field(sb, "name", name);
		string imageUrl = string.Empty, imageMd5 = string.Empty;
		if (FindGodEvent(id) != null) GodBanner(id, out imageUrl, out imageMd5);
		Field(sb, "image_url", imageUrl); Field(sb, "image_md5", imageMd5);
		Field(sb, "igc_reward", ev.Get(r, "IGC_REWARD")); Field(sb, "is_new", 0); Field(sb, "is_cleared", p.cleared_events.Contains(id) ? 1 : 0);
		Field(sb, "num_battles", ev.Get(r, "NUM_BATTLES")); Field(sb, "xp_multiplier", ev.Get(r, "XP_MULTIPLIER"));
		Field(sb, "element", ev.Get(r, "ELEMENT"));
		return Close(sb);
	}

	private static string StoreItemsJson()
	{
		OfflineTable t = OfflineTables.Get("ShopItem");
		Dictionary<string, int> typeIds = new Dictionary<string, int> { { "Premium", 1 }, { "BagSlot", 3 }, { "Stamina", 4 }, { "BattlePoint", 5 } };
		List<string> list = new List<string>();
		for (int i = 0; i < t.Rows.Count; i++)
		{
			string[] r = t.Rows[i];
			int typeId;
			if (!typeIds.TryGetValue(t.Get(r, "STORE_ITEM_TYPE"), out typeId)) continue;
			StringBuilder sb = new StringBuilder("{");
			Field(sb, "id", i + 1); Field(sb, "store_item_type_id", typeId); Field(sb, "name", t.Get(r, "NAME"));
			Field(sb, "denomination", t.Get(r, "DENOMINATION")); Field(sb, "price", t.Get(r, "PRICE"));
			Field(sb, "is_active", t.Get(r, "IS_ACTIVE")); Field(sb, "percent_discount", t.Get(r, "PERCENT_DISCOUNT"));
			Field(sb, "sprite_name", t.Get(r, "SPRITE_NAME")); Field(sb, "ios_product_id", t.Get(r, "IOS_PRODUCT_ID"));
			Field(sb, "android_product_id", t.Get(r, "ANDROID_PRODUCT_ID"));
			list.Add(Close(sb));
		}
		return Array(list);
	}

	private static string StartEvent(Dictionary<string, string> f)
	{
		string eventId = Get(f, "event_id");
		OfflineTable ev = OfflineTables.Get("Events");
		int difficulty = 0;
		int.TryParse(Get(f, "difficulty"), out difficulty);
		string[] r = FindGodEvent(eventId) != null ? GodEventRow(eventId, OfflineProfile.Current.level, difficulty) : ev.Row(eventId);
		if (r == null) return Fail(404, "unknown event " + eventId);
		r = TutorialLineup(ev, eventId, r);
		int battles = Math.Max(1, ev.GetInt(r, "NUM_BATTLES"));
		List<string> types = new List<string>();
		for (int i = 1; i <= 9; i++) { string m = ev.Get(r, "MONSTER_TYPE_" + i); if (m.Length > 0) types.Add(m); }
		Session s = new Session { eventId = eventId, difficulty = difficulty };
		bool tutorial = ev.Get(r, "TYPE").Equals("Tutorial", StringComparison.OrdinalIgnoreCase);
		for (int b = 0; b < battles - 1; b++)
		{
			s.monsters.Add(types.Count == 0 ? ev.Get(r, "BOSS_MONSTER_ID") : tutorial ? types[Math.Min(b, types.Count - 1)] : types[s_rng.Next(types.Count)]);
		}
		s.monsters.Add(ev.Get(r, "BOSS_MONSTER_ID"));
		if (!tutorial)
		{
			int cost = difficulty == 0 ? GLOBAL.GAME_SETTINGS.event_easy_cost : difficulty == 1 ? GLOBAL.GAME_SETTINGS.event_normal_cost : GLOBAL.GAME_SETTINGS.event_hard_cost;
			OfflineProfile p = OfflineProfile.Current;
			p.RegenShields(Now());
			p.shields = Math.Max(0, p.shields - cost);
			p.Save();
			GodEvent god = FindGodEvent(eventId);
			s.chests = RollChests(ev.Get(r, "ELEMENT"), ev.GetInt(r, "DIFFICULTY"), s.monsters.Count, god != null);
			if (god != null && s.chests.Count > 1)
			{
				List<Chest> pre = RollChests(ev.Get(r, "ELEMENT"), Math.Min(4, ev.GetInt(r, "DIFFICULTY")), s.chests.Count - 1, true, true);
				for (int k = 0; k < pre.Count; k++) s.chests[k] = pre[k];
			}
			if (god != null)
			{
				GodSetDrops(god, s.chests, ev.GetInt(r, "MIN"), difficulty);
				s.mystery = GodMystery(god, ev.Get(r, "ELEMENT"), ev.GetInt(r, "DIFFICULTY"));
			}
			s.mystery = s.mystery ?? RollMystery(ev.Get(r, "ELEMENT"), ev.GetInt(r, "DIFFICULTY"));
		}
		else
		{
			s.chests = TutorialChests(eventId, s.monsters.Count) ?? s.chests;
		}
		return BattleStart(s);
	}

	private static string StartDungeon(Dictionary<string, string> f)
	{
		string dungeonId = Get(f, "dungeon_id");
		OfflineTable dt = OfflineTables.Get("Dungeons");
		OfflineTable mt = OfflineTables.Get("Monsters");
		string[] r = dt.Row(dungeonId);
		if (r == null) return Fail(404, "unknown dungeon " + dungeonId);
		int battles = Math.Max(1, dt.GetInt(r, "NUMBER_OF_BATTLES"));
		string element = ElementPrefix(dt.Get(r, "ELEMENT"));
		string level = dt.Get(r, "MIN");
		List<string> species = new List<string>();
		for (int i = 1; i <= 4; i++) { string sp = dt.Get(r, "SPECIES_" + i); if (sp.Length > 0) species.Add(sp.ToUpperInvariant()); }
		Session s = new Session { dungeonId = dungeonId };
		string previous = null;
		for (int b = 0; b < battles - 1; b++)
		{
			List<string> pool = species.FindAll(x => x != previous && mt.Row(element + "_" + x + "_" + level) != null);
			if (pool.Count == 0) pool = species.FindAll(x => mt.Row(element + "_" + x + "_" + level) != null);
			string pick = pool.Count > 0 ? pool[s_rng.Next(pool.Count)] : null;
			s.monsters.Add(pick != null ? element + "_" + pick + "_" + level : dt.Get(r, "BOSS_MONSTER_ID"));
			previous = pick;
		}
		s.monsters.Add(dt.Get(r, "BOSS_MONSTER_ID"));
		DodoboBattle(s, dt.GetFloat(r, "CHANCE_DODOBO"), dt.GetInt(r, "DIFFICULTY"), dt.GetInt(r, "MIN"));
		OfflineProfile p = OfflineProfile.Current;
		p.RegenStamina(Now());
		p.stamina = Math.Max(0, p.stamina - dt.GetInt(r, "STAMINA_COST"));
		p.Save();
		s.chests = RollChests(dt.Get(r, "ELEMENT"), dt.GetInt(r, "DIFFICULTY"), s.monsters.Count);
		s.mystery = RollMystery(dt.Get(r, "ELEMENT"), dt.GetInt(r, "DIFFICULTY"));
		NamedBossDrops(dungeonId, dt.Get(r, "ELEMENT"), dt.GetInt(r, "DIFFICULTY"), s.chests, ref s.mystery);
		return BattleStart(s);
	}

	private static void DodoboBattle(Session s, float chance, int difficulty, int level)
	{
		if (s.monsters.Count < 2 || s_rng.NextDouble() >= chance) return;
		string species = s_rng.Next(2) == 0 ? "Dodobo" : "Bobodo";
		OfflineTable mt = OfflineTables.Get("Monsters");
		string best = null;
		int bestGap = int.MaxValue;
		foreach (string[] m in mt.Rows)
		{
			if (mt.Get(m, "SPECIES") != species || mt.Get(m, "ELEMENT") != "Metal" || mt.GetInt(m, "DIFFICULTY") != difficulty) continue;
			int gap = Math.Abs(mt.GetInt(m, "LEVEL") - level);
			if (gap < bestGap) { bestGap = gap; best = m[0]; }
		}
		if (best == null) return;
		int slot = s_rng.Next(s.monsters.Count - 1);
		UnityEngine.Debug.Log("[OfflineServer] dodobo battle: " + best + " replaces battle " + slot + " (" + s.monsters[slot] + ")");
		s.monsters[slot] = best;
	}

	private static string ElementPrefix(string element)
	{
		switch (element.ToLowerInvariant())
		{
		case "earth":
		case "nature":
			return "EARTH";
		default:
			return element.ToUpperInvariant();
		}
	}

	private static string BattleStart(Session s)
	{
		string key = "offline_" + (++s_sessionCounter) + "_" + Now();
		s_sessions[key] = s;
		StringBuilder ids = new StringBuilder("{");
		for (int i = 0; i < s.monsters.Count; i++) Field(ids, "monster_id_" + i, s.monsters[i]);
		return Ok("{\"dungeon_key\":" + Str(key) + ",\"monster_ids\":" + Close(ids) + ",\"chest_map\":" + ChestMapJson(s.chests) + "}");
	}

	private static string EndDungeon(Dictionary<string, string> f)
	{
		Session s;
		string cached;
		if (s_endReplies.TryGetValue(Get(f, "dungeon_key"), out cached)) return cached;
		if (!s_sessions.TryGetValue(Get(f, "dungeon_key"), out s)) return Fail(404, "unknown dungeon_key");
		s_sessions.Remove(Get(f, "dungeon_key"));
		OfflineProfile p = OfflineProfile.Current;
		long now = Now();
		bool completed = Get(f, "end_status") == "completed";
		int defeated = 0, dodobo = 0;
		int.TryParse(Get(f, "monsters_defeated"), out defeated);
		int.TryParse(Get(f, "dodobo_currency"), out dodobo);
		float rating = 0f;
		float.TryParse(Get(f, "lowest_rating"), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out rating);
		int gainedXp = 0;
		OfflineTable mtl = OfflineTables.Get("Monsters");
		int lastLevel = s.monsters.Count > 0 ? mtl.GetInt(mtl.Row(s.monsters[s.monsters.Count - 1]), "LEVEL") : p.level;
		bool allGold = completed && rating >= GLOBAL.GAME_SETTINGS.medal_criteria_gold && Math.Abs(p.level - lastLevel) <= 5;
		string rewarded = completed ? ChestRewards(p, s.chests, defeated, allGold ? s.mystery : null) : "[]";
		p.in_game += dodobo;
		if (completed && Get(f, "assists_friend_id0").Length > 0) p.social += 10;
		if (completed)
		{
			OfflineTable mt = OfflineTables.Get("Monsters");
			float mult = 1f;
			if (s.eventId != null)
			{
				OfflineTable ev = OfflineTables.Get("Events");
				string[] r = ev.Row(s.eventId);
				p.in_game += ev.GetInt(r, "IGC_REWARD") * s.monsters.Count;
				p.premium += s.eventId == "TUTORIAL_1" ? 1 : ev.GetInt(r, "PREMIUM_REWARD");
				if (System.Array.IndexOf(TutorialEvents, s.eventId) < 0)
				{
					p.dragon_point += EventDragonPoints(p.level, s.difficulty);
				}
				mult = Math.Max(1f, ev.GetFloat(r, "XP_MULTIPLIER"));
				if (!p.cleared_events.Contains(s.eventId))
				{
					p.cleared_events.Add(s.eventId);
					if (s.eventId == "TUTORIAL_1") p.premium_gacha_ticket++;
				}
			}
			else
			{
				OfflineTable dt = OfflineTables.Get("Dungeons");
				string[] r = dt.Row(s.dungeonId);
				p.in_game += dt.GetInt(r, "IGC_REWARD") * s.monsters.Count;
				OfflineDungeon d = p.unlocked.Find(x => x.dungeon_id == s.dungeonId);
				if (d != null) { d.is_cleared = true; d.highest_score = Math.Max(d.highest_score, rating); }
				string next = dt.Get(r, "UNLOCK_DUNGEON");
				if (next.Length > 0 && p.unlocked.Find(x => x.dungeon_id == next) == null)
				{
					p.unlocked.Add(new OfflineDungeon { city_id = dt.Get(dt.Row(next), "CITY_ID"), dungeon_id = next });
				}
			}
			for (int i = 0; i < Math.Min(defeated, s.monsters.Count); i++)
			{
				string[] mr = mt.Row(s.monsters[i]);
				gainedXp += LevelGapXp(mt.GetInt(mr, "REWARD_EXPERIENCE"), p.level, mt.GetInt(mr, "LEVEL"));
			}
			gainedXp = (int)(gainedXp * mult);
			if (p.level >= MaxLevel) gainedXp = 0;
			p.exp += gainedXp;
			LevelUp(p, now);
		}
		else if (s.eventId != null && System.Array.IndexOf(TutorialEvents, s.eventId) < 0)
		{
			p.dragon_point += 5;
		}
		if (!completed && s.eventId != null && System.Array.IndexOf(TutorialEvents, s.eventId) >= 0 && !p.cleared_events.Contains(s.eventId))
		{
			p.cleared_events.Add(s.eventId);
		}
		p.Save();
		string payload = "{" + CharacterFields(p, now)
			+ ",\"rewarded_items\":" + rewarded + ",\"gained_xp\":" + Str(gainedXp)
			+ ",\"unlocked_dungeons\":" + UnlockedJson(p)
			+ ",\"eligible_events\":" + EventsJson(p)
			+ ",\"gift_senders\":[]}";
		p.Save();
		s_endReplies[Get(f, "dungeon_key")] = Ok(payload);
		return s_endReplies[Get(f, "dungeon_key")];
	}

	private const int MaxLevel = 50;

	private static int EventDragonPoints(int level, int difficulty)
	{
		int per25 = difficulty >= 2 ? 12 : difficulty == 1 ? 6 : 3;
		return level * per25 / 25;
	}

	private static int LevelGapXp(int xp, int playerLevel, int monsterLevel)
	{
		float diff = playerLevel - monsterLevel;
		int n;
		if (diff >= 0f) n = diff < 10f ? xp - (int)(diff * 0.1f * xp) : 0;
		else n = xp + (int)(Math.Min(-diff, 5f) * 0.1f * xp);
		return Math.Max(0, n);
	}

	private static void LevelUp(OfflineProfile p, long now)
	{
		OfflineTable xt = OfflineTables.Get("Experiences");
		while (true)
		{
			string[] row = xt.Row(p.level.ToString());
			int need = xt.GetInt(row, "EXP_NEXT_LEVEL");
			if (row == null || need <= 0 || p.exp < need || p.level >= MaxLevel || xt.Row((p.level + 1).ToString()) == null) break;
			p.exp -= need;
			p.level++;
			p.strength++;
			p.constitution++;
			if (p.level == 15 || p.level == 30 || p.level == 45) p.premium_gacha_gift_ticket++;
			p.max_stamina += xt.GetInt(xt.Row(p.level.ToString()), "REWARD_STAMINA");
			p.stamina = p.max_stamina;
			p.shields = p.max_shields;
			p.stamina_epoch = now;
			p.shields_epoch = now;
		}
	}

	private static string Get(Dictionary<string, string> f, string k)
	{
		string v;
		return f.TryGetValue(k, out v) ? v : string.Empty;
	}

	private static string Fail(int code, string message)
	{
		UnityEngine.Debug.LogWarning("[OfflineServer] fail " + code + ": " + message);
		return "{\"code\":" + code + ",\"ts\":" + Str(Now()) + ",\"message\":" + Str(message) + "}";
	}

	private static void Field(StringBuilder sb, string key, object value)
	{
		if (sb.Length > 0 && sb[sb.Length - 1] != '{') sb.Append(',');
		sb.Append('"').Append(key).Append("\":").Append(Str(value));
	}

	private static string Close(StringBuilder sb)
	{
		return sb.Append('}').ToString();
	}

	private static string Array(List<string> items)
	{
		return "[" + string.Join(",", items.ToArray()) + "]";
	}
}
