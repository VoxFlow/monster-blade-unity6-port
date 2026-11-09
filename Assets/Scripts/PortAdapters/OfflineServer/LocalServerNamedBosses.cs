// Offline server: the named bosses' equipment sets and rewards. Register CD-037.
using System;
using System.Collections.Generic;

public static partial class LocalServer
{
	private class NamedBoss
	{
		public string bronze;
		public string silver;
	}

	private static readonly Dictionary<string, NamedBoss> s_namedBosses = new Dictionary<string, NamedBoss>
	{
		{ "DUNGEON_3", new NamedBoss { bronze = "special_foreststrider_t3_a3" } },
		{ "DUNGEON_9", new NamedBoss { bronze = "special_icewarden_t3_a3" } },
		{ "DUNGEON_15", new NamedBoss { bronze = "special_flamebearer_t3_a3" } },
		{ "DUNGEON_24", new NamedBoss { bronze = "special_dragonhunter_t3_a3" } },
		{ "DUNGEON_30", new NamedBoss { bronze = "special_stormborn_t3_a3" } },
		{ "DUNGEON_33", new NamedBoss { bronze = "special_foreststrider_t3_a3", silver = "special_stoneguardian_t5_a5" } },
		{ "DUNGEON_39", new NamedBoss { bronze = "special_icewarden_t3_a3", silver = "special_frozenwarlord_t5_a5" } },
		{ "DUNGEON_45", new NamedBoss { bronze = "special_flamebearer_t3_a3", silver = "special_blazingemperor_t5_a5" } },
		{ "DUNGEON_54", new NamedBoss { bronze = "special_dragonhunter_t3_a3", silver = "special_dragonking_t5_a5" } },
		{ "DUNGEON_60", new NamedBoss { bronze = "special_stormborn_t3_a3", silver = "special_thundergod_t5_a5" } },
	};

	private static readonly Dictionary<string, int[]> s_namedAchievements = new Dictionary<string, int[]>
	{
		{ "achievement_city_1", new[] { 1, 150 } }, { "achievement_city_3", new[] { 1, 150 } }, { "achievement_city_5", new[] { 2, 400 } },
		{ "achievement_city_8", new[] { 3, 850 } }, { "achievement_city_10", new[] { 5, 1325 } },
		{ "achievement_city_11", new[] { 5, 1325 } }, { "achievement_city_13", new[] { 5, 1325 } }, { "achievement_city_15", new[] { 5, 1325 } },
		{ "achievement_city_18", new[] { 5, 1325 } }, { "achievement_city_20", new[] { 5, 1325 } },
	};

	private static Chest NamedSetPiece(string setId)
	{
		OfflineTable eq = OfflineTables.Get("ItemsEquipments");
		OfflineTable items = OfflineTables.Get("Items");
		OfflineTable tiers = OfflineTables.Get("TierRange");
		List<string[]> rows = new List<string[]>();
		foreach (string[] r in eq.Rows)
		{
			if (eq.Get(r, "SET_ID") == setId && items.Row(r[0]) != null) rows.Add(r);
		}
		if (rows.Count == 0) return null;
		string[] pick = rows[s_rng.Next(rows.Count)];
		int tier = eq.GetInt(pick, "ITEM_TIER");
		string[] it = items.Row(pick[0]);
		return new Chest { itemId = pick[0], itemType = items.Get(it, "ITEM_TYPE"), tier = tier, chestType = items.Get(it, "ITEM_CHEST"),
			level = Math.Max(1, tiers.GetInt(tiers.Row(tier.ToString()), "BASE_LEVEL_START")) };
	}

	private static void NamedBossDrops(string dungeonId, string element, int difficulty, List<Chest> chests, ref Chest mystery)
	{
		NamedBoss nb;
		if (!s_namedBosses.TryGetValue(dungeonId, out nb)) return;
		int last = chests == null ? -1 : chests.Count - 1;
		if (last >= 0 && s_rng.NextDouble() < 0.5)
		{
			string set = nb.silver != null && s_rng.NextDouble() < 0.5 ? nb.silver : nb.bronze;
			chests[last] = NamedSetPiece(set) ?? chests[last];
		}
		Chest m = null;
		if (nb.silver != null) m = NamedSetPiece(s_rng.NextDouble() < 0.5 ? nb.silver : nb.bronze);
		else if (s_rng.NextDouble() < 0.5) m = NamedSetPiece(nb.bronze);
		else
		{
			string e = element.Equals("Earth", StringComparison.OrdinalIgnoreCase) || element.Equals("Nature", StringComparison.OrdinalIgnoreCase) ? "EARTH" : element.ToUpperInvariant();
			string id = "ELEMENTAL_ESSENCE_" + e + "_T" + Math.Max(1, Math.Min(4, difficulty));
			OfflineTable items = OfflineTables.Get("Items");
			string[] row = items.Row(id);
			if (row != null) m = new Chest { itemId = id, itemType = items.Get(row, "ITEM_TYPE"), tier = 1, level = -1, chestType = items.Get(row, "ITEM_CHEST") };
		}
		if (m != null) mystery = m;
	}
}
