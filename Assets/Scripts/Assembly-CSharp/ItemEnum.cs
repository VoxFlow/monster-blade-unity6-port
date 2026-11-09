using UnityEngine;

public class ItemEnum
{
	public enum Skill
	{
		NONE = -3,
		WEAPON_NONE = -2,
		ARMOUR_NONE = -1,
		WEAPON_GHOUL = 0,
		ARMOUR_GHOUL = 1,
		WEAPON_HILL = 2,
		ARMOUR_HILL = 3,
		WEAPON_BRUTE = 4,
		ARMOUR_BRUTE = 5,
		WEAPON_ROGUE = 6,
		ARMOUR_ROGUE = 7,
		WEAPON_SOLDIER = 8,
		ARMOUR_SOLDIER = 9,
		Max = 10
	}

	public enum Type
	{
		NONE = -1,
		BASE_WEAPON = 0,
		BASE_ARMOUR_CHEST = 1,
		BASE_ARMOUR_HELM = 2,
		BASE_ARMOUR_SHOULDERS = 3,
		BASE_ARMOUR_LEGGINGS = 4,
		HEAVY_WEAPON = 5,
		FAST_WEAPON = 6,
		HEAVY_ARMOUR_CHEST = 7,
		FAST_ARMOUR_CHEST = 8,
		HEAVY_ARMOUR_HELM = 9,
		FAST_ARMOUR_HELM = 10,
		HEAVY_ARMOUR_SHOULDERS = 11,
		FAST_ARMOUR_SHOULDERS = 12,
		HEAVY_ARMOUR_LEGGINGS = 13,
		FAST_ARMOUR_LEGGINGS = 14,
		ESSENCE = 15,
		CONSUMABLE = 16,
		PET = 17,
		Max = 18
	}

	public enum Chest
	{
		NONE = -1,
		LOWEST = 0,
		LOW = 1,
		MEDIUM = 2,
		HIGH = 3,
		VERYHIGH = 4,
		Max = 5
	}

	public enum Category
	{
		NONE = -1,
		EQUIPMENT = 0,
		LOOT = 1,
		Max = 2
	}

	public enum LootType
	{
		NONE = -1,
		IGC = 0,
		HP = 1,
		ESSENCE = 2,
		PET = 3,
		Max = 4
	}

	public static Skill SkillFromString(string _string)
	{
		string text = _string.ToLower();
		for (int i = -3; i < 10; i++)
		{
			Skill skill = (Skill)i;
			string text2 = skill.ToString().ToLower();
			if (text == text2)
			{
				return skill;
			}
		}
		return Skill.Max;
	}

	public static Type TypeFromString(string _string)
	{
		string text = _string.ToLower();
		for (int i = -1; i < 18; i++)
		{
			Type type = (Type)i;
			string text2 = type.ToString().ToLower();
			if (text == text2)
			{
				return type;
			}
		}
		Debug.LogWarning(_string);
		return Type.Max;
	}

	public static Chest ChestFromString(string _string)
	{
		string text = _string.ToLower();
		for (int i = -1; i < 5; i++)
		{
			Chest chest = (Chest)i;
			string text2 = chest.ToString().ToLower();
			if (text == text2)
			{
				return chest;
			}
		}
		return Chest.Max;
	}

	public static Category CategoryFromString(string _string)
	{
		if (_string == Category.EQUIPMENT.ToString())
		{
			return Category.EQUIPMENT;
		}
		if (_string == Category.LOOT.ToString())
		{
			return Category.LOOT;
		}
		LootType lootType = LootTypeString(_string);
		if (lootType != LootType.Max)
		{
			return Category.LOOT;
		}
		Equipment.Part part = Equipment.FromString(_string);
		if (part != Equipment.Part.Max)
		{
			return Category.EQUIPMENT;
		}
		return Category.NONE;
	}

	public static LootType LootTypeString(string _string)
	{
		string text = _string.ToLower();
		for (int i = -1; i < 4; i++)
		{
			LootType lootType = (LootType)i;
			string value = lootType.ToString().ToLower();
			if (text.Contains(value))
			{
				return lootType;
			}
		}
		return LootType.Max;
	}
}
