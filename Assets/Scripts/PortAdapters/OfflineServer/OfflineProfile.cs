// The account state of the offline server, stored as a local JSON profile. Register CD-012.
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class OfflineItem
{
	public string db_id;
	public string item_id;
	public string item_type;
	public int item_level = 1;
	public int item_tier = 1;
	public int fusion_xp;
	public bool is_equipped;
}

[Serializable]
public class OfflineDungeon
{
	public string city_id;
	public string dungeon_id;
	public float highest_score;
	public bool is_cleared;
}

[Serializable]
public class OfflineAchievement
{
	public string id;
	public int progress;
	public bool marked;
	public bool claimed;
}

[Serializable]
public class OfflineProfile
{
	public int version = 1;
	public string name = string.Empty;
	public int level = 1;
	public int exp;
	public int strength = 25;
	public int agility = 5;
	public int constitution = 30;
	public int max_stamina = 25;
	public int stamina = 25;
	public long stamina_epoch;
	public int max_shields = 3;
	public int shields = 3;
	public long shields_epoch;
	public int max_inventory_slots = 20;
	public int friend_limit = 1;
	public float in_game;
	public float premium = 3;
	public float social = 25;
	public int premium_gacha_ticket;
	public int premium_gacha_gift_ticket;
	public int social_gacha_ticket;
	public int medal;
	public int dragon_point;
	public int holy_diamond;
	public long next_db_id = 1;
	public List<OfflineItem> inventory = new List<OfflineItem>();
	public List<OfflineDungeon> unlocked = new List<OfflineDungeon>();
	public List<string> cleared_events = new List<string>();
	public List<string> tutorial_flags = new List<string>();
	public List<OfflineAchievement> achievements = new List<OfflineAchievement>();
	public int daily_count;
	public string daily_last = string.Empty;
	public List<string> pets = new List<string>();
	public string active_pet = string.Empty;
	public List<string> collection = new List<string>();

	private static OfflineProfile s_current;

	private static string FilePath
	{
		get { return Path.Combine(UnityEngine.Application.persistentDataPath, "OfflineServer/profile.json"); }
	}

	public static OfflineProfile Current
	{
		get
		{
			if (s_current == null)
			{
				s_current = Load();
			}
			return s_current;
		}
	}

	private static OfflineProfile Load()
	{
		if (File.Exists(FilePath))
		{
			try
			{
				OfflineProfile loaded = JsonUtility.FromJson<OfflineProfile>(File.ReadAllText(FilePath));
				foreach (OfflineItem it in loaded.inventory)
				{
					if (it.item_type != null && it.item_type.StartsWith("BASE_")) it.item_level = 0;
				}
				if (loaded.constitution == 240 && loaded.strength == 40)
				{
					loaded.strength = 24 + loaded.level;
					loaded.constitution = 29 + loaded.level;
					Debug.Log("[OfflineServer] profile stats repaired to strength " + loaded.strength + ", constitution " + loaded.constitution + " (level " + loaded.level + ")");
				}
				loaded.RecordCollection();
				return loaded;
			}
			catch (Exception e)
			{
				Debug.LogError("[OfflineServer] profile unreadable, starting a new one: " + e.Message);
			}
		}
		OfflineProfile p = new OfflineProfile();
		p.CreateStarter();
		p.Save();
		Debug.Log("[OfflineServer] new offline profile created at " + FilePath);
		return p;
	}

	public void Save()
	{
		RecordCollection();
		Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
		File.WriteAllText(FilePath, JsonUtility.ToJson(this, true));
	}

	private void CreateStarter()
	{
		long now = LocalServer.Now();
		stamina_epoch = now;
		shields_epoch = now;
		OfflineTable items = OfflineTables.Get("Items");
		foreach (string id in new string[] { "WEAPON_BASE_VERY_COMMON", "ARMOUR_HELM_BASE_VERY_COMMON", "ARMOUR_SHOULDERS_BASE_VERY_COMMON", "ARMOUR_CHEST_BASE_VERY_COMMON", "ARMOUR_LEGGINGS_BASE_VERY_COMMON" })
		{
			AddItem(id, items.Get(items.Row(id), "ITEM_TYPE"), 0, 1, true);
		}
		OfflineTable dungeons = OfflineTables.Get("Dungeons");
		string[] first = dungeons.Rows[0];
		unlocked.Add(new OfflineDungeon { city_id = dungeons.Get(first, "CITY_ID"), dungeon_id = dungeons.Get(first, "ID") });
	}

	public OfflineItem AddItem(string itemId, string itemType, int itemLevel, int itemTier, bool equipped)
	{
		OfflineItem it = new OfflineItem
		{
			db_id = (next_db_id++).ToString(),
			item_id = itemId,
			item_type = itemType,
			item_level = itemLevel,
			item_tier = itemTier,
			is_equipped = equipped
		};
		inventory.Add(it);
		Collect(it);
		return it;
	}

	public static string CollectionKey(string itemId, int tier)
	{
		int i = itemId.LastIndexOf('_');
		return (i > 0 ? itemId.Substring(0, i) : itemId) + "^" + tier;
	}

	private void Collect(OfflineItem it)
	{
		if (it.item_id == null || !(it.item_id.StartsWith("ARMOUR_") || it.item_id.StartsWith("WEAPON_"))) return;
		string k = CollectionKey(it.item_id, it.item_tier);
		if (!collection.Contains(k)) collection.Add(k);
	}

	public void RecordCollection()
	{
		foreach (OfflineItem it in inventory) Collect(it);
	}

	public int RegenStamina(long now)
	{
		return Regen(ref stamina, max_stamina, ref stamina_epoch, LocalServerGameSettings.StaminaResetSeconds, now);
	}

	public int RegenShields(long now)
	{
		return Regen(ref shields, max_shields, ref shields_epoch, LocalServerGameSettings.ShieldsResetSeconds, now);
	}

	private static int Regen(ref int value, int max, ref long epoch, long interval, long now)
	{
		if (interval <= 0) interval = 1;
		if (value >= max)
		{
			epoch = now;
			return (int)interval;
		}
		long points = (now - epoch) / interval;
		if (points > 0)
		{
			value = (int)Math.Min(max, value + points);
			epoch += points * interval;
			if (value >= max) epoch = now;
		}
		return (int)Math.Max(1, interval - (now - epoch));
	}
}
