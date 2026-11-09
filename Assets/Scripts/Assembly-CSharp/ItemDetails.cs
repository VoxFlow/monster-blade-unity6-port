using Models;
using UnityEngine;

public class ItemDetails : IEquipable
{
	private string db_id;

	private ItemEnum.Category category;

	public readonly ItemBase itemBase;

	public readonly ItemEquipment equipment;

	public readonly ItemSkill skill;

	private ItemType typeData;

	public readonly ItemType itemTypeWithSkill;

	public readonly ItemLoot loot;

	public readonly FusionValue fusion;

	public bool isNew;

	public bool isPet;

	public bool isSelected;

	public bool isEnabled = true;

	public ServerInventoryItem serverData;

	private int xp;

	private string elementSprite;

	private string elementSpriteMedium;

	private string elementSpriteSmall;

	private UIAtlas bgAtlas;

	private string bgSprite;

	private string originalBgSprite;

	private bool isEquip;

	private bool isExcess;

	public string DB_ID
	{
		get
		{
			return db_id;
		}
	}

	public ItemEnum.Category Category
	{
		get
		{
			return category;
		}
	}

	public string ItemID
	{
		get
		{
			return itemBase.itemID;
		}
	}

	public string Name
	{
		get
		{
			return itemBase.name;
		}
	}

	public ItemEnum.Chest Chest
	{
		get
		{
			return itemBase.chest;
		}
	}

	public Element.Type Element
	{
		get
		{
			return itemBase.elementType;
		}
	}

	public int BaseLevel
	{
		get
		{
			return itemTypeWithSkill.baseLevel;
		}
	}

	public int BaseOrder
	{
		get
		{
			return (!IsBaseItem) ? 1 : 0;
		}
	}

	public int EquipOrder
	{
		get
		{
			return (!isEquip) ? 1 : 0;
		}
	}

	public int NewOrder
	{
		get
		{
			return (!isNew) ? 1 : 0;
		}
	}

	public int Level
	{
		get
		{
			int num = BaseLevel - equipment.tierRange.BaseLevelStart + 1;
			return (num >= 0) ? num : 0;
		}
	}

	public int Cost
	{
		get
		{
			if (category == ItemEnum.Category.LOOT)
			{
				return (int)loot.sellValue;
			}
			return 10 * Tier * Level;
		}
	}

	public int SkillIdx
	{
		get
		{
			return BaseLevel + (Tier - 1);
		}
	}

	public int XP
	{
		get
		{
			return xp;
		}
	}

	public int XPForFusion
	{
		get
		{
			if (category == ItemEnum.Category.LOOT)
			{
				return (int)loot.fusionValue;
			}
			int num = fusion.startXpValue + fusion.xpIncreasePerLevel * (Level - 1);
			return (num <= 0) ? 1 : num;
		}
	}

	public float XPPercent
	{
		get
		{
			if (Mathf.Approximately(fusion.xpToLevelUp, 0f))
			{
				Dbg.LogError("Fusion XP to level up cannot be zero.");
				return 0f;
			}
			return xp * 100 / fusion.xpToLevelUp;
		}
	}

	public bool IsMaxXP
	{
		get
		{
			return xp >= fusion.xpToLevelUp;
		}
	}

	public bool IsBaseItem
	{
		get
		{
			return BaseLevel == 0;
		}
	}

	public string LevelStr
	{
		get
		{
			int level = Level;
			return (level >= 10) ? (string.Empty + level) : ("0" + level);
		}
	}

	public int Tier
	{
		get
		{
			int num = 1;
			num = ((category != ItemEnum.Category.LOOT) ? equipment.tier : loot.tier);
			return ItemManager.TierClamp(num);
		}
	}

	public int MaxLevelInTier
	{
		get
		{
			if (category == ItemEnum.Category.LOOT)
			{
				return 1;
			}
			return equipment.tierRange.LevelRange;
		}
	}

	public UIAtlas BGAtlas
	{
		get
		{
			return bgAtlas;
		}
	}

	public string BGAtlasName
	{
		get
		{
			if (Category == ItemEnum.Category.EQUIPMENT && equipment.characterElement != null)
			{
				return equipment.characterElement.atlasName;
			}
			if (Category == ItemEnum.Category.LOOT)
			{
				if (bgAtlas == null)
				{
					Dbg.Log("[Info] The loot item atlas is null, try to get it");
					if (SingletonComponent<ItemVisualManager>.Instance != null)
					{
						bgAtlas = SingletonComponent<ItemVisualManager>.Instance.GetAtlas(loot.atlasName);
						Dbg.Log("[Info] Get the loot atlas : " + bgAtlas.name);
					}
					else
					{
						Dbg.LogWarn("[Warn] The ItemVisualManager.Instance is null, unable to get loot atlas");
					}
				}
				return loot.atlasName;
			}
			return SingletonComponent<ItemVisualManager>.Instance.m_globalIcons.name;
		}
	}

	public string BGSpriteName
	{
		get
		{
			if (BGAtlasName == SingletonComponent<ItemVisualManager>.Instance.m_globalIcons.name)
			{
				return SingletonComponent<ItemVisualManager>.Instance.m_errorSpriteName;
			}
			return bgSprite;
		}
	}

	public string OriginalBGSprite
	{
		get
		{
			return originalBgSprite;
		}
	}

	public string ElementSprite
	{
		get
		{
			return elementSprite;
		}
	}

	public string ElementSpriteMedium
	{
		get
		{
			return elementSpriteMedium;
		}
	}

	public string ElementSpriteSmall
	{
		get
		{
			return elementSpriteSmall;
		}
	}

	public bool IsWeapon
	{
		get
		{
			return equipment.part == Equipment.Part.Weapon;
		}
	}

	public bool IsArmour
	{
		get
		{
			return equipment.part < Equipment.Part.Weapon;
		}
	}

	public bool IsEquipment
	{
		get
		{
			return category == ItemEnum.Category.EQUIPMENT;
		}
	}

	public bool IsEquip
	{
		get
		{
			return isEquip;
		}
		set
		{
			isEquip = value;
		}
	}

	public bool IsExcess
	{
		get
		{
			return isExcess;
		}
		set
		{
			isExcess = value;
		}
	}

	public bool IsEssence
	{
		get
		{
			return category == ItemEnum.Category.LOOT && loot.type == ItemEnum.LootType.ESSENCE;
		}
	}

	public ItemDetails(ItemBase _itemBase, ItemEquipment _equipment, ItemSkill _skill, ItemType _typeData, ItemLoot _loot, FusionValue _fusion)
	{
		db_id = "-5";
		category = ItemEnum.Category.NONE;
		itemBase = _itemBase;
		equipment = _equipment;
		skill = _skill;
		typeData = _typeData;
		loot = _loot;
		fusion = _fusion;
		itemTypeWithSkill = new ItemType(_typeData, _skill);
		xp = _fusion.startXpValue + (Level - 1) * _fusion.xpIncreasePerLevel;
		elementSprite = string.Empty;
		elementSpriteMedium = string.Empty;
		elementSpriteSmall = string.Empty;
		bgAtlas = null;
		isExcess = false;
	}

	public void Equip(IPlayer p_player)
	{
		if (p_player != null)
		{
			p_player.Equip(this);
		}
	}

	public bool IsEquipped(IPlayer p_player)
	{
		if (p_player == null)
		{
			return false;
		}
		return p_player.IsEquipped(this);
	}

	public void SetData(string _db_id, string _category, bool _isEquip, int _xp)
	{
		db_id = _db_id;
		category = ItemEnum.CategoryFromString(_category);
		if (category == ItemEnum.Category.EQUIPMENT && equipment != null && equipment.name != string.Empty)
		{
			itemBase.name = equipment.name;
		}
		if (serverData == null)
		{
			serverData = new ServerInventoryItem(ItemID, Tier, string.Empty, string.Empty, 0, 0, string.Empty);
		}
		isEquip = _isEquip;
		xp = _xp;
	}

	public void SetVisual(UIAtlas _bgAtlas, string _elementSprite, string _bgSpriteName = "")
	{
		bgAtlas = _bgAtlas;
		elementSprite = _elementSprite;
		originalBgSprite = _bgSpriteName;
		string value = "_";
		string text = elementSprite.Substring(0, elementSprite.LastIndexOf(value) + 1);
		elementSpriteMedium = text + "med";
		elementSpriteSmall = text + "small";
		bgSprite = _bgSpriteName;
		if (_bgSpriteName == string.Empty)
		{
			UseErrorVisual();
		}
	}

	public void UnloadVisual()
	{
		UseErrorVisual();
	}

	private void UseErrorVisual()
	{
		bgAtlas = SingletonComponent<ItemVisualManager>.Instance.m_globalIcons;
		bgSprite = SingletonComponent<ItemVisualManager>.Instance.m_errorSpriteName;
	}

	public override string ToString()
	{
		return string.Format("{0}  {1}\n{2}{3}{4}{5}\nItemType+Skill:{6}\n{7}{8}\n[Others]\nxp:{9}    Equip:{10}   ES:{11}    atlas:{12}    bgSprite:{13}\n", db_id, category, itemBase, equipment, skill, typeData, itemTypeWithSkill, loot, fusion, xp, IsEquip, ElementSprite, BGAtlasName, BGSpriteName);
	}
}
