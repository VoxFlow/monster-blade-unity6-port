// Offline server: battle allies without the social servers (generated strangers; substitute). Register CD-043.
using System;
using System.Collections.Generic;
using System.Text;

public static partial class LocalServer
{
	private static readonly string[] s_strangerNames = { "Slayer #1042", "Slayer #2387", "Slayer #3519", "Slayer #4806", "Slayer #5273", "Slayer #6641", "Slayer #7158", "Slayer #8924" };

	private static readonly string[] s_gearSlots = { "WEAPON", "ARMOUR_HELM", "ARMOUR_CHEST", "ARMOUR_SHOULDERS", "ARMOUR_LEGGINGS" };

	private static string AlliesJson(OfflineProfile p)
	{
		List<string> list = new List<string>();
		if (TutorialFinished(p))
		{
			Random rng = new Random(DateTime.Today.DayOfYear * 7919 + DateTime.Today.Year);
			long now = Now();
			for (int i = 0; i < 3; i++)
			{
				OfflineProfile s = Stranger(p, rng, i);
				string id = (900000001 + i * 7 + DateTime.Today.DayOfYear).ToString();
				string ch = CharacterJson(s, now).Replace("\"id\":" + Str(OfflineServerMode.NubeeId), "\"id\":" + Str(id))
					.Replace("\"nubee_id\":" + Str(OfflineServerMode.NubeeId), "\"nubee_id\":" + Str(id));
				StringBuilder sb = new StringBuilder("{");
				Field(sb, "nubee_id", id); Field(sb, "device_id", "offline-stranger-" + i); Field(sb, "name", s.name);
				list.Add(sb.ToString().TrimEnd(',') + ",\"character\":" + ch + "}");
			}
		}
		return "{\"stranger\":" + Array(list) + ",\"friends\":[]}";
	}

	private static OfflineProfile Stranger(OfflineProfile p, Random rng, int i)
	{
		OfflineProfile s = new OfflineProfile();
		s.name = s_strangerNames[rng.Next(s_strangerNames.Length)];
		s.level = Math.Max(1, Math.Min(MaxLevel, p.level - 1 + rng.Next(3)));
		s.strength = 24 + s.level; s.agility = p.agility; s.constitution = 29 + s.level;
		s.max_stamina = p.max_stamina; s.max_shields = p.max_shields; s.stamina = p.max_stamina; s.shields = p.max_shields;
		OfflineTable tiers = OfflineTables.Get("TierRange");
		int tier = 1;
		for (int t = 5; t >= 1; t--)
		{
			if (s.level >= tiers.GetInt(tiers.Row(t.ToString()), "BASE_LEVEL_START")) { tier = t; break; }
		}
		string[] elements = { "Fire", "Nature", "Lightning", "Water", "Metal" };
		string element = elements[rng.Next(elements.Length)];
		OfflineTable items = OfflineTables.Get("Items");
		OfflineTable eq = OfflineTables.Get("ItemsEquipments");
		foreach (string slot in s_gearSlots)
		{
			List<string[]> pool = new List<string[]>();
			foreach (string[] e in eq.Rows)
			{
				if (eq.Get(e, "ITEM_TIER") != tier.ToString()) continue;
				string[] r = items.Row(e[0]);
				if (r == null || items.GetFloat(r, "DROP_PROBABILITY") <= 0f) continue;
				string type = items.Get(r, "ITEM_TYPE");
				if (!type.EndsWith(slot) || type.StartsWith("BASE_")) continue;
				if (slot == "WEAPON" && !items.Get(r, "ITEM_ELEMENT").Equals(element, StringComparison.OrdinalIgnoreCase)) continue;
				pool.Add(r);
			}
			if (pool.Count == 0) continue;
			string[] pick = pool[rng.Next(pool.Count)];
			int start = tiers.GetInt(tiers.Row(tier.ToString()), "BASE_LEVEL_START");
			int range = tiers.GetInt(tiers.Row(tier.ToString()), "LEVEL_RANGE");
			OfflineItem it = s.AddItem(pick[0], items.Get(pick, "ITEM_TYPE"), Math.Max(start, Math.Min(start + range - 1, s.level)), tier, false);
			it.is_equipped = true;
		}
		return s;
	}
}
