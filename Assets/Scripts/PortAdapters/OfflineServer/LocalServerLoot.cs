// Offline server: loot, fusion, selling and store purchases. Registers CD-031, CD-036.
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static partial class LocalServer
{
	private static List<Chest> TutorialChests(string eventId, int battles)
	{
		if ((eventId != "TUTORIAL_1" && eventId != "TUTORIAL_2") || OfflineProfile.Current.cleared_events.Contains(eventId))
		{
			return null;
		}
		bool boot = eventId == "TUTORIAL_1";
		string itemId = boot ? "BEGGERS_BOOT" : "WEAPON_BARBARIAN_T1_NONE";
		OfflineTable items = OfflineTables.Get("Items");
		string[] itemRow = items.Row(itemId);
		if (itemRow == null)
		{
			return null;
		}
		OfflineTable tiers = OfflineTables.Get("TierRange");
		Chest c = new Chest
		{
			itemId = itemId, itemType = items.Get(itemRow, "ITEM_TYPE"), tier = 1,
			level = boot ? -1 : tiers.GetInt(tiers.Row("1"), "BASE_LEVEL_START"), chestType = items.Get(itemRow, "ITEM_CHEST")
		};
		List<Chest> chests = new List<Chest>();
		for (int i = 0; i < battles; i++) chests.Add(i == battles - 1 ? c : null);
		return chests;
	}

	private static string FusionStart(Dictionary<string, string> f)
	{
		OfflineProfile p = OfflineProfile.Current;
		TutorialFlag(f, "fusion");
		OfflineItem baseItem = p.inventory.Find(x => x.db_id == Get(f, "base_item_db_id"));
		InventoryManager inv = Object.FindAnyObjectByType<InventoryManager>();
		if (baseItem == null || inv == null)
		{
			return Fail(404, "offline: fusion base item not found");
		}
		ItemDetails baseDetails = inv.GetByDBID(baseItem.db_id);
		List<OfflineItem> materials = new List<OfflineItem>();
		int xp = baseItem.fusion_xp;
		for (int i = 1; Get(f, "item_db_id" + i) != string.Empty; i++)
		{
			OfflineItem m = p.inventory.Find(x => x.db_id == Get(f, "item_db_id" + i));
			ItemDetails md = m != null ? inv.GetByDBID(m.db_id) : null;
			if (m == null || md == null)
			{
				return Fail(404, "offline: fusion material not found");
			}
			materials.Add(m);
			xp += md.XPForFusion;
		}
		int cost = baseDetails != null ? baseDetails.Cost * materials.Count : 0;
		if (baseDetails != null)
		{
			int toLevel = Mathf.Max(1, baseDetails.fusion.xpToLevelUp);
			int level = baseDetails.Level;
			while (xp >= toLevel && level < baseDetails.MaxLevelInTier)
			{
				xp -= toLevel;
				level++;
				baseItem.item_level++;
			}
			if (level >= baseDetails.MaxLevelInTier)
			{
				xp = Mathf.Min(xp, toLevel);
			}
		}
		baseItem.fusion_xp = xp;
		p.in_game = System.Math.Max(0, p.in_game - cost);
		foreach (OfflineItem m in materials)
		{
			p.inventory.Remove(m);
		}
		p.Save();
		return Ok("{\"new_base_item\":" + ItemJson(baseItem) + ",\"character\":" + CharacterJson(p, Now()) + "}");
	}

	private static string Evolve(Dictionary<string, string> f)
	{
		OfflineProfile p = OfflineProfile.Current;
		InventoryManager inv = Object.FindAnyObjectByType<InventoryManager>();
		string baseId = Get(f, "base_item_db_id");
		OfflineItem baseItem = p.inventory.Find(x => x.db_id == baseId);
		ItemDetails d = baseItem != null && inv != null ? inv.GetByDBID(baseId) : null;
		if (d == null || d.IsBaseItem || d.equipment == null || !d.equipment.canEvolve)
		{
			return Fail(404, "offline: item cannot evolve");
		}
		int nextTier = baseItem.item_tier + 1;
		if (!EquipTiers(baseItem.item_id).Contains(nextTier.ToString()))
		{
			return Fail(404, "offline: no higher tier");
		}
		List<OfflineItem> used = new List<OfflineItem>();
		Dictionary<string, List<ItemDetails>> byId = new Dictionary<string, List<ItemDetails>>();
		foreach (string cat in d.equipment.catalysts)
		{
			if (cat == string.Empty) continue;
			List<ItemDetails> l;
			if (!byId.TryGetValue(cat, out l)) byId[cat] = l = inv.GetItemsByItemID(cat);
			l.RemoveAll(x => x.DB_ID == baseId);
			if (l.Count == 0) return Fail(404, "offline: missing catalyst " + cat);
			used.Add(p.inventory.Find(x => x.db_id == l[0].DB_ID));
			l.RemoveAt(0);
		}
		int cost = d.Cost * used.Count;
		if (p.in_game < cost) return Fail(404, "offline: not enough gold");
		p.in_game -= cost;
		foreach (OfflineItem m in used) if (m != null) p.inventory.Remove(m);
		p.inventory.Remove(baseItem);
		OfflineTable tiers = OfflineTables.Get("TierRange");
		OfflineItem evolved = p.AddItem(baseItem.item_id, baseItem.item_type, tiers.GetInt(tiers.Row(nextTier.ToString()), "BASE_LEVEL_START"), nextTier, false);
		p.Save();
		return Ok(ItemJson(evolved));
	}

	private static string Retry(Dictionary<string, string> f)
	{
		if (!s_sessions.ContainsKey(Get(f, "dungeon_key"))) return Fail(404, "unknown dungeon_key");
		if (Get(f, "sns_reward") == "1") return Fail(500, "offline: share rewards are not available offline");
		OfflineProfile p = OfflineProfile.Current;
		int cost = GLOBAL.GAME_SETTINGS != null ? GLOBAL.GAME_SETTINGS.death_retry_cost : 1;
		if (p.premium < cost) return Fail(700, "not enough diamonds");
		p.premium -= cost;
		p.Save();
		return Ok(CharacterJson(p, Now()));
	}

	private static void SaveEquipped(Dictionary<string, string> f)
	{
		List<string> ids = new List<string>();
		for (int i = 1; i <= 5; i++) if (Get(f, "item" + i) != string.Empty) ids.Add(Get(f, "item" + i));
		if (ids.Count == 0) return;
		OfflineProfile p = OfflineProfile.Current;
		foreach (OfflineItem it in p.inventory) it.is_equipped = ids.Contains(it.db_id);
		p.Save();
	}

	private static string Purchase(Dictionary<string, string> f)
	{
		OfflineTable t = OfflineTables.Get("ShopItem");
		int id;
		if (!int.TryParse(Get(f, "store_item_id"), out id) || id < 1 || id > t.Rows.Count) return Fail(404, "offline: unknown store item");
		string[] r = t.Rows[id - 1];
		string type = t.Get(r, "STORE_ITEM_TYPE");
		if (type == "Premium") return Fail(500, "offline: real-money purchases are not available offline");
		OfflineProfile p = OfflineProfile.Current;
		int amount = 1;
		if (type == "BagSlot" && (!int.TryParse(Get(f, "amount"), out amount) || amount < 1)) amount = 1;
		float price = t.GetFloat(r, "PRICE") * amount;
		if (p.premium < price) return Fail(700, "not enough diamonds");
		long now = Now();
		switch (type)
		{
		case "Stamina": p.RegenStamina(now); p.stamina = System.Math.Max(p.stamina, p.max_stamina); break;
		case "BattlePoint": p.RegenShields(now); p.shields = System.Math.Max(p.shields, p.max_shields); break;
		case "BagSlot":
			int limit = GLOBAL.GAME_SETTINGS != null && GLOBAL.GAME_SETTINGS.slot_limitation > 0 ? GLOBAL.GAME_SETTINGS.slot_limitation : 150;
			p.max_inventory_slots = System.Math.Min(limit, p.max_inventory_slots + t.GetInt(r, "DENOMINATION") * amount);
			break;
		default:
			return Fail(404, "offline: unknown store item type " + type);
		}
		p.premium -= price;
		p.Save();
		return Ok("{\"product_id\":[],\"character\":" + CharacterJson(p, now) + "}");
	}

	private static string MultiSell(Dictionary<string, string> f)
	{
		OfflineProfile p = OfflineProfile.Current;
		InventoryManager inv = Object.FindAnyObjectByType<InventoryManager>();
		StringBuilder results = new StringBuilder("{");
		for (int i = 0; Get(f, "item_db_id" + i) != string.Empty; i++)
		{
			string dbId = Get(f, "item_db_id" + i);
			OfflineItem it = p.inventory.Find(x => x.db_id == dbId);
			ItemDetails d = it != null && inv != null ? inv.GetByDBID(dbId) : null;
			bool sold = it != null && d != null && !d.IsBaseItem;
			if (sold)
			{
				p.in_game += d.Cost;
				p.inventory.Remove(it);
			}
			if (i > 0) results.Append(',');
			results.Append("\"item_db_id").Append(i).Append("\":").Append(Str(sold ? 200 : 603));
		}
		p.Save();
		return Ok("{\"sell_results\":" + results.Append('}') + ",\"character\":" + CharacterJson(p, Now()) + "}");
	}
}
