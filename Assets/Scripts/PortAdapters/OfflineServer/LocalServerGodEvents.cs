// Offline server: the god-beast events (substitute rules from the shipped tables). Register CD-040.
using System;

public static partial class LocalServer
{
	private class GodEvent
	{
		public string id, name, boss, slot, stage, set;
		public string[] pool = s_kingPool;
	}

	private static readonly string[] s_kingPool = { "METAL_TRICEP", "EARTH_WINGSERPENT", "WATER_LIZARD" };

	private static readonly string[] s_vahagnPool = { "METAL_BULL" };

	private static readonly string[] s_aralezPool = { "METAL_WINGSERPENT" };

	private static readonly string[] s_godBeasts = { "EARTH_WINGCHICKEN_EVENT_WEAKER", "WATER_HYDRA_EVENT_WEAKER", "FIRE_WOLF_EVENT_WEAKER",
		"METAL_STONEDRAGON_EVENT_WEAKER", "LIGHTNING_LANDDRAGON_EVENT_WEAKER" };

	private static readonly GodEvent[] s_godEvents =
	{
		new GodEvent { id = "GODBEAST_HYDRA", name = "The Undying Godbeast", boss = "WATER_HYDRA_EVENT_BOSS", slot = "WEEK_4",
			stage = "WaterLevel_Prefab_3", set = "special_hydra_t5_a5" },
		new GodEvent { id = "GODBEAST_NHANG", name = "Godbeast Of The Skies", boss = "EARTH_WINGCHICKEN_EVENT_BOSS", slot = "WEEK_2",
			stage = "NatureLevel_Prefab_3", set = "special_wingdragon_t5_a5" },
		new GodEvent { id = "GODBEAST_ARALEZ", name = "Rage of the Godbeast", boss = "FIRE_WOLF_EVENT_BOSS", slot = "WEEK_1",
			stage = "FireLevel_Prefab_3", set = "special_wolf_t5_a5", pool = s_aralezPool },
		new GodEvent { id = "GODBEAST_VAHAGN", name = "The Vengeful Dragon God", boss = "LIGHTNING_LANDDRAGON_EVENT_BOSS", slot = "WEEK_3",
			stage = "LightningLevel_Prefab_3", set = "special_landdragon_t5_a5", pool = s_vahagnPool },
		new GodEvent { id = "GODBEAST_ARAMAZ", name = "The Beginning of the End", boss = "METAL_EPICDRAGON_EVENT_BOSS", slot = "MONTH",
			stage = "WaterLevel_Prefab_4", set = "special_epicdragon_t5_a5", pool = s_godBeasts },
		new GodEvent { id = "GODBEAST_ANAHIT", name = "Gargoyle Death Gaze", boss = "METAL_STONEDRAGON_EVENT_BOSS", slot = "MONTH",
			stage = "NeutralLevel_Prefab_3", set = "special_stonedragon_t5_a5" },
	};

	private static GodEvent FindGodEvent(string id)
	{
		return System.Array.Find(s_godEvents, g => g.id == id);
	}

	private static string MonsterAt(string family, int level)
	{
		return family + "_" + Math.Max(1, Math.Min(100, level));
	}

	private static string[] GodEventRow(string id, int playerLevel, int difficulty)
	{
		GodEvent g = FindGodEvent(id);
		if (g == null) return null;
		OfflineTable ev = OfflineTables.Get("Events");
		OfflineTable mt = OfflineTables.Get("Monsters");
		int level = playerLevel + 4 - 2 * (2 - Math.Max(0, Math.Min(2, difficulty)));
		string boss = MonsterAt(g.boss, level);
		string[] bossRow = mt.Row(boss);
		string element = mt.Get(bossRow, "ELEMENT");
		element = element.Length > 0 ? char.ToUpperInvariant(element[0]) + element.Substring(1).ToLowerInvariant() : "Metal";
		string[] pool = g.pool;
		string[] r = ev.NewRow();
		ev.Set(r, "ID", g.id); ev.Set(r, "NAME", g.name); ev.Set(r, "TYPE", "Dungeon");
		ev.Set(r, "EVENT_ID", g.slot == "MONTH" ? DateTime.Now.ToString("MMMM", System.Globalization.CultureInfo.InvariantCulture).ToUpperInvariant() : g.slot);
		ev.Set(r, "ELEMENT", element); ev.Set(r, "STAGE", g.stage); ev.Set(r, "BOSS_MONSTER_ID", boss);
		ev.Set(r, "DIFFICULTY", Math.Max(3, Math.Min(5, mt.GetInt(bossRow, "DIFFICULTY") + 1)));
		ev.Set(r, "MIN", level); ev.Set(r, "MAX", level); ev.Set(r, "NUM_BATTLES", 2); ev.Set(r, "IGC_REWARD", 250);
		ev.Set(r, "PREMIUM_REWARD", 0); ev.Set(r, "STAMINA_COST", 0); ev.Set(r, "XP_MULTIPLIER", 1);
		for (int i = 0; i < pool.Length && i < 9; i++) ev.Set(r, "MONSTER_TYPE_" + (i + 1), MonsterAt(pool[i], level));
		ev.Put(r);
		return r;
	}

	private static readonly string[] s_setSlots = { "ARMOUR_SHOULDERS", "ARMOUR_LEGGINGS", "ARMOUR_CHEST", "WEAPON", "ARMOUR_HELM" };

	private static int SetSlotsAt(int monsterLevel)
	{
		return monsterLevel < 13 ? 0 : monsterLevel <= 20 ? 2 : monsterLevel <= 35 ? 3 : monsterLevel <= 45 ? 4 : 5;
	}

	private static Chest GodSetPiece(GodEvent g, int slots)
	{
		OfflineTable eq = OfflineTables.Get("ItemsEquipments");
		OfflineTable items = OfflineTables.Get("Items");
		OfflineTable tiers = OfflineTables.Get("TierRange");
		System.Collections.Generic.List<string> ids = new System.Collections.Generic.List<string>();
		foreach (string[] r in eq.Rows)
		{
			if (eq.Get(r, "SET_ID") != g.set || ids.Contains(r[0])) continue;
			string type = items.Get(items.Row(r[0]), "ITEM_TYPE");
			for (int k = 0; k < slots && k < s_setSlots.Length; k++)
			{
				if (type.EndsWith(s_setSlots[k])) { ids.Add(r[0]); break; }
			}
		}
		if (ids.Count == 0) return null;
		string id = ids[s_rng.Next(ids.Count)];
		string[] it = items.Row(id);
		return new Chest { itemId = id, itemType = items.Get(it, "ITEM_TYPE"), tier = 5, chestType = items.Get(it, "ITEM_CHEST"),
			level = Math.Max(1, tiers.GetInt(tiers.Row("5"), "BASE_LEVEL_START")) };
	}

	private static void GodSetDrops(GodEvent g, System.Collections.Generic.List<Chest> chests, int monsterLevel, int difficulty)
	{
		int slots = SetSlotsAt(monsterLevel);
		if (slots == 0 || chests == null) return;
		double chance = difficulty >= 2 ? 0.35 : difficulty == 1 ? 0.20 : 0.10;
		int last = chests.Count - 1;
		if (last >= 0 && s_rng.NextDouble() < chance) chests[last] = GodSetPiece(g, slots) ?? chests[last];
	}

	private static Chest GodMystery(GodEvent g, string element, int tier)
	{
		if (s_rng.NextDouble() < 0.5)
		{
			Chest piece = GodSetPiece(g, s_setSlots.Length);
			if (piece != null) return piece;
		}
		string e = element.Equals("Earth", StringComparison.OrdinalIgnoreCase) || element.Equals("Nature", StringComparison.OrdinalIgnoreCase) ? "EARTH" : element.ToUpperInvariant();
		string id = "ELEMENTAL_ESSENCE_" + e + "_T" + Math.Max(1, Math.Min(4, tier));
		OfflineTable items = OfflineTables.Get("Items");
		string[] row = items.Row(id);
		return row == null ? null : new Chest { itemId = id, itemType = items.Get(row, "ITEM_TYPE"), tier = 1, level = -1, chestType = items.Get(row, "ITEM_CHEST") };
	}

	private static readonly string[] s_godRotation =
		{ "GODBEAST_ARALEZ", "GODBEAST_HYDRA", "GODBEAST_NHANG", "GODBEAST_ANAHIT", "GODBEAST_VAHAGN", "GODBEAST_ARAMAZ" };

	private static string CurrentGodEvent()
	{
		int weeks = (int)((DateTime.Today - new DateTime(2013, 9, 2)).TotalDays / 7);
		return s_godRotation[((weeks % s_godRotation.Length) + s_godRotation.Length) % s_godRotation.Length];
	}

	private static readonly System.Collections.Generic.Dictionary<string, string> s_bannerMd5 = new System.Collections.Generic.Dictionary<string, string>();

	private static void GodBanner(string id, out string url, out string md5)
	{
		url = md5 = string.Empty;
		string path = System.IO.Path.Combine(UnityEngine.Application.streamingAssetsPath, "OfflineServer/event_banners/" + id + ".png");
		if (!System.IO.File.Exists(path)) return;
		url = new Uri(path).AbsoluteUri;
		if (!s_bannerMd5.TryGetValue(id, out md5))
		{
			using (System.Security.Cryptography.MD5 h = System.Security.Cryptography.MD5.Create())
			{
				md5 = Services.ByteToString(h.ComputeHash(System.IO.File.ReadAllBytes(path)));
			}
			s_bannerMd5[id] = md5;
		}
	}

	private static long GodEventEnd(GodEvent g)
	{
		DateTime t = DateTime.Today;
		int toMonday = ((int)DayOfWeek.Monday - (int)t.DayOfWeek + 7) % 7;
		DateTime end = g.slot == "MONTH" ? new DateTime(t.Year, t.Month, 1).AddMonths(1) : t.AddDays(toMonday == 0 ? 7 : toMonday);
		return (long)(end.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
	}
}
