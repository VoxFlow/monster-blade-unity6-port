// Offline server: dungeon and event chests (substitute rules from the item tables). Register CD-037.
using System;
using System.Collections.Generic;
using System.Text;

public static partial class LocalServer
{
	private class Chest
	{
		public string itemId;
		public string itemType;
		public int tier;
		public int level;
		public string chestType;
	}

	private static Dictionary<string, List<string>> s_equipTiers;

	private static List<Chest> RollChests(string element, int difficulty, int battles, bool everyBattle = false, bool noTop = false)
	{
		List<Chest> pool = new List<Chest>();
		List<float> weights = new List<float>();
		OfflineTable items = OfflineTables.Get("Items");
		OfflineTable tiers = OfflineTables.Get("TierRange");
		int tier = Math.Max(1, Math.Min(5, difficulty));
		string itemElement = element.Equals("Earth", StringComparison.OrdinalIgnoreCase) ? "Nature" : element;
		int baseLevel = Math.Max(1, tiers.GetInt(tiers.Row(tier.ToString()), "BASE_LEVEL_START"));
		Dictionary<string, int> familyMax = new Dictionary<string, int>();
		foreach (string[] r in items.Rows)
		{
			string family; int n;
			if (items.Get(r, "ITEM_TYPE") == "ESSENCE" && EssenceTier(items.Get(r, "ITEM_ID"), out family, out n))
			{
				int m; familyMax.TryGetValue(family, out m); familyMax[family] = Math.Max(m, n);
			}
		}
		foreach (string[] r in items.Rows)
		{
			float p = items.GetFloat(r, "DROP_PROBABILITY");
			string type = items.Get(r, "ITEM_TYPE");
			string id = items.Get(r, "ITEM_ID");
			if (p <= 0f || !items.Get(r, "ITEM_ELEMENT").Equals(itemElement, StringComparison.OrdinalIgnoreCase) || type.StartsWith("BASE_"))
			{
				continue;
			}
			if (noTop && items.Get(r, "ITEM_CHEST") == "VeryHigh") continue;
			Chest c = new Chest { itemId = id, itemType = type, chestType = items.Get(r, "ITEM_CHEST") };
			if (type == "ESSENCE")
			{
				string family; int n;
				if (!EssenceTier(id, out family, out n) || n != Math.Min(tier, familyMax[family])) continue;
				c.tier = 1; c.level = -1;
			}
			else
			{
				if (!EquipTiers(id).Contains(tier.ToString())) continue;
				c.tier = tier; c.level = baseLevel;
			}
			pool.Add(c);
			weights.Add(p);
		}
		List<Chest> chests = new List<Chest>();
		for (int b = 0; b < battles; b++)
		{
			int k = WeightedPick(weights);
			bool boss = b == battles - 1;
			chests.Add(k >= 0 && (boss || everyBattle || s_rng.NextDouble() < weights[k]) ? pool[k] : null);
		}
		return chests;
	}

	private static bool EssenceTier(string id, out string family, out int n)
	{
		int i = id.LastIndexOf("_T", StringComparison.Ordinal);
		family = i > 0 ? id.Substring(0, i) : id;
		n = 0;
		return i > 0 && int.TryParse(id.Substring(i + 2), out n);
	}

	private static List<string> EquipTiers(string itemId)
	{
		if (s_equipTiers == null)
		{
			s_equipTiers = new Dictionary<string, List<string>>();
			OfflineTable eq = OfflineTables.Get("ItemsEquipments");
			foreach (string[] r in eq.Rows)
			{
				List<string> l;
				if (!s_equipTiers.TryGetValue(r[0], out l)) s_equipTiers[r[0]] = l = new List<string>();
				l.Add(eq.Get(r, "ITEM_TIER"));
			}
		}
		List<string> t;
		return s_equipTiers.TryGetValue(itemId, out t) ? t : new List<string>();
	}

	private static int WeightedPick(List<float> weights)
	{
		float sum = 0f;
		foreach (float w in weights) sum += w;
		if (sum <= 0f) return -1;
		double x = s_rng.NextDouble() * sum;
		for (int i = 0; i < weights.Count; i++)
		{
			x -= weights[i];
			if (x < 0) return i;
		}
		return weights.Count - 1;
	}

	private static string ChestMapJson(List<Chest> chests)
	{
		StringBuilder sb = new StringBuilder("{");
		for (int j = 0; chests != null && j < chests.Count; j++)
		{
			Chest c = chests[j];
			if (c == null) continue;
			if (sb.Length > 1) sb.Append(',');
			StringBuilder it = new StringBuilder("{");
			Field(it, "item_id", c.itemId); Field(it, "item_type", c.itemType); Field(it, "item_level", c.level);
			Field(it, "item_tier", c.tier); Field(it, "chest_type", c.chestType);
			sb.Append("\"monster_id").Append(j).Append("\":[").Append(Close(it)).Append(']');
		}
		sb.Append('}');
		UnityEngine.Debug.Log("[OfflineServer] chest_map: " + sb);
		return sb.ToString();
	}

	private static Chest RollMystery(string element, int difficulty)
	{
		return RollChests(element, difficulty, 1)[0];
	}

	private static string ChestRewards(OfflineProfile p, List<Chest> chests, int defeated, Chest mystery)
	{
		StringBuilder sb = new StringBuilder("{");
		int n = 0;
		for (int j = 0; chests != null && j < Math.Min(defeated, chests.Count); j++)
		{
			Chest c = chests[j];
			if (c == null) continue;
			OfflineItem it = p.AddItem(c.itemId, c.itemType, c.level, c.tier, false);
			if (n++ > 0) sb.Append(',');
			sb.Append("\"chest_").Append(j).Append("\":").Append(ItemJson(it));
		}
		if (mystery != null)
		{
			OfflineItem it = p.AddItem(mystery.itemId, mystery.itemType, mystery.level, mystery.tier, false);
			if (n++ > 0) sb.Append(',');
			sb.Append("\"final_loot\":").Append(ItemJson(it));
		}
		string json = n == 0 ? "[]" : sb.Append('}').ToString();
		UnityEngine.Debug.Log("[OfflineServer] chest rewards (" + defeated + " defeated): " + json);
		return json;
	}
}
