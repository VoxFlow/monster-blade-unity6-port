// Offline server: treasure (gacha) answers. Registers CD-028, CD-037.
using System.Collections.Generic;
using System.Text;

public static partial class LocalServer
{
	private struct GachaDef
	{
		public int id;

		public int price;

		public int pay;

		public int free;

		public string filter;
	}

	private static readonly GachaDef[] s_gacha = new GachaDef[]
	{
		new GachaDef { id = 1, price = 20, pay = 0, free = 0, filter = "" },
		new GachaDef { id = 2, price = 20, pay = 0, free = 0, filter = "" },
		new GachaDef { id = 5, price = 30, pay = 0, free = 0, filter = "element" },
		new GachaDef { id = 6, price = 8, pay = 0, free = 0, filter = "" },
		new GachaDef { id = 9, price = 300, pay = 10, free = 1, filter = "element" },
		new GachaDef { id = 10, price = 300, pay = 10, free = 1, filter = "" },
		new GachaDef { id = 11, price = 30, pay = 0, free = 0, filter = "" },
		new GachaDef { id = 13, price = 300, pay = 10, free = 1, filter = "" },
	};

	private static string GachaEventsJson()
	{
		List<string> list = new List<string>();
		foreach (GachaDef g in s_gacha)
		{
			StringBuilder sb = new StringBuilder("{");
			Field(sb, "id", g.id); Field(sb, "gacha_type_id", g.id); Field(sb, "gacha_status_id", 2);
			Field(sb, "name", "offline_" + g.id); Field(sb, "date_created", ""); Field(sb, "start_date", ""); Field(sb, "end_date", "");
			Field(sb, "price", g.price); Field(sb, "reward_item_type", ""); Field(sb, "reward_amount", 0); Field(sb, "reward_same_set_flag", "0");
			Field(sb, "pay_times", g.pay); Field(sb, "free_times", g.free); Field(sb, "is_ticket_usable", g.id == 6 ? 1 : 0);
			Field(sb, "filter_by", g.filter); Field(sb, "element", "");
			list.Add(Close(sb));
		}
		return Array(list);
	}

	private static string AllGachaEvents()
	{
		return Ok(GachaEventsJson());
	}

	private static string PlayGacha(Dictionary<string, string> f)
	{
		string gachaId = Get(f, "gacha_id");
		if (Get(f, "is_tutorial") != "1" || (gachaId != "1" && gachaId != "6"))
		{
			return PlayGachaOffline(gachaId, false, Get(f, "is_use_ticket") == "1");
		}
		TutorialFlag(f, gachaId == "1" ? "gacha" : "gacha_premium");
		if (gachaId == "6" && OfflineProfile.Current.premium_gacha_ticket > 0) OfflineProfile.Current.premium_gacha_ticket--;
		string itemId = gachaId == "1" ? "WEAPON_COLOSSUS_T1_NONE" : "WEAPON_DEVIL_T3_HILL";
		int tier = gachaId == "1" ? 1 : 3;
		OfflineProfile p = OfflineProfile.Current;
		OfflineTable items = OfflineTables.Get("Items");
		OfflineTable tiers = OfflineTables.Get("TierRange");
		int level = tiers.GetInt(tiers.Row(tier.ToString()), "BASE_LEVEL_START");
		OfflineItem it = p.AddItem(itemId, items.Get(items.Row(itemId), "ITEM_TYPE"), level, tier, false);
		p.Save();
		long now = Now();
		return Ok("{\"character\":" + CharacterJson(p, now) + ",\"items\":[" + ItemJson(it) + "]}");
	}

	private static string PlayGachaOffline(string gachaId, bool multi, bool useTicket)
	{
		GachaDef g = new GachaDef();
		bool found = false;
		foreach (GachaDef d in s_gacha) if (d.id.ToString() == gachaId) { g = d; found = true; }
		if (!found) return Fail(404, "offline: unknown treasure " + gachaId);
		bool social = g.id == 1 || g.id == 2;
		bool lucky = g.id == 11;
		OfflineProfile p = OfflineProfile.Current;
		int count = multi ? System.Math.Max(1, g.pay + g.free) : 1;
		if (social)
		{
			int perPull = GLOBAL.GAME_SETTINGS != null ? GLOBAL.GAME_SETTINGS.social_points_per_ticket : g.price;
			int have = p.social_gacha_ticket + (int)(p.social / perPull);
			count = multi ? System.Math.Min(10, have) : 1;
			if (have < count || count == 0) return Fail(700, "not enough friend points");
			for (int i = 0; i < count; i++)
			{
				if (p.social_gacha_ticket > 0) p.social_gacha_ticket--;
				else p.social -= perPull;
			}
		}
		else if (lucky)
		{
			if (p.holy_diamond < g.price) return Fail(700, "not enough holy diamonds");
			p.holy_diamond -= g.price;
		}
		else if (useTicket && g.id == 6 && p.premium_gacha_ticket > 0) p.premium_gacha_ticket--;
		else if (useTicket && g.id == 6 && p.premium_gacha_gift_ticket > 0) p.premium_gacha_gift_ticket--;
		else if (p.premium < g.price) return Fail(700, "not enough diamonds");
		else p.premium -= g.price;
		if ((g.id == 6 || g.id == 10) && GLOBAL.GAME_SETTINGS != null)
		{
			Models.GameSettings gs = GLOBAL.GAME_SETTINGS;
			p.holy_diamond += multi ? s_rng.Next(gs.gacha_premium_multi_hd_bonus_min, gs.gacha_premium_multi_hd_bonus_max + 1)
				: s_rng.Next(gs.gacha_premium_single_hd_bonus_min, gs.gacha_premium_single_hd_bonus_max + 1);
		}
		OfflineTable items = OfflineTables.Get("Items");
		OfflineTable tiers = OfflineTables.Get("TierRange");
		List<string[]> pool = new List<string[]>();
		List<float> weights = new List<float>();
		string want = social ? "Social" : "Premium";
		foreach (string[] r in items.Rows)
		{
			if (items.Get(r, "AVAILABLE_IN_GACHA") != want || items.Get(r, "ITEM_TYPE") == "ESSENCE" || EquipTiers(r[0]).Count == 0) continue;
			if (lucky && MinTier(r[0]) < 4) continue;
			pool.Add(r);
			weights.Add(System.Math.Max(0.01f, items.GetFloat(r, "DROP_PROBABILITY")));
		}
		List<string> got = new List<string>();
		for (int i = 0; i < count && pool.Count > 0; i++)
		{
			string[] r = pool[WeightedPick(weights)];
			int tier = MinTier(r[0]);
			OfflineItem it = p.AddItem(r[0], items.Get(r, "ITEM_TYPE"), tiers.GetInt(tiers.Row(tier.ToString()), "BASE_LEVEL_START"), tier, false);
			got.Add(ItemJson(it));
		}
		p.Save();
		return Ok("{\"character\":" + CharacterJson(p, Now()) + ",\"items\":" + Array(got) + "}");
	}

	private static int MinTier(string itemId)
	{
		int tier = 5;
		foreach (string t in EquipTiers(itemId)) { int n; if (int.TryParse(t, out n) && n < tier) tier = n; }
		return tier;
	}
}
