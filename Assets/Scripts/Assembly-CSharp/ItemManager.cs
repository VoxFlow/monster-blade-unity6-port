using System.Collections.Generic;
using Models;
using UnityEngine;

public class ItemManager : SingletonComponent<ItemManager>
{
	private const int m_MaxEvolveFuseSlots = 5;

	public Color betterColor = Color.green;

	public Color worseColor = Color.red;

	public Color sameColor = Color.white;

	private string[] m_colorTags = new string[3] { "[00FF00]", "[FF0000]", "[FFFFFF]" };

	public UIAtlas m_skillAtlas;

	public static ItemDetails s_NULL_DATA;

	public static int MAX_ITEM_LEVEL = 71;

	public static bool s_doNotUseItems;

	private ItemVisualManager Ivm;

	private Dictionary<string, ItemBase> m_itemsBase;

	private Dictionary<string, ItemEquipment> m_equipments;

	private Dictionary<string, ItemSkill> m_itemsSkill;

	private Dictionary<string, ItemType> m_itemsType;

	private Dictionary<string, ItemLoot> m_loots;

	private Dictionary<string, ArmorSet> m_armorSets;

	private Dictionary<string, ArmorSet> m_specialArmorSets;

	private Dictionary<int, FusionValue> m_fusionValues;

	private Dictionary<int, TierRange> m_tierRange;

	private Dictionary<ItemEnum.Skill, SkillVisual> m_skillVisuals;

	private List<string> m_skillSpriteList = new List<string>();

	private NBBinaryFormat m_csvItems;

	private NBBinaryFormat m_csvEquips;

	private NBBinaryFormat m_csvItemsSkill;

	private NBBinaryFormat m_csvItemsType;

	private NBBinaryFormat m_csvLoots;

	private NBBinaryFormat m_csvFusionValue;

	private NBBinaryFormat m_csvTierRange;

	private NBBinaryFormat m_csvSkillVisuals;

	private NBBinaryFormat m_csvArmorCollections;

	private string m_csvItemParam = "Items";

	private string m_csvEquipParam = "ItemsEquipments";

	private string m_csvItemSkillParam = "ItemsSkill";

	private string m_csvItemTypeParam = "ItemsType";

	private string m_csvLootParam = "ItemsLoot";

	private string m_csvFusionValueParam = "FusionXP";

	private string m_csvTierRangeParam = "TierRange";

	private string m_csvSkillVisualParam = "SkillVisual";

	private string m_csvArmorCollectionsParam = "ArmorCollections";

	public void InitManager()
	{
		NBProfiler.BeginTask("[ITEM_MANAGER][INIT_MANAGER]");
		if (s_doNotUseItems)
		{
			Dbg.Log(" --> DO NOT USE ITEMS <-- ItemManager InitManager() return;", Dbg.NBLogType.INVENTORY);
			return;
		}
		if (PrepareCsvConnections())
		{
			PrepareItemDatabase();
		}
		NBProfiler.EndTask("[ITEM_MANAGER][INIT_MANAGER]");
	}

	public bool PrepareCsvConnections()
	{
		ParameterManager instance = SingletonComponent<ParameterManager>.Instance;
		if (instance == null)
		{
			Dbg.LogError("ItemManager cannot find ParameterManager.", Dbg.NBLogType.INVENTORY);
			return false;
		}
		m_csvItems = instance.GetParametersFor(m_csvItemParam);
		if (m_csvItems == null)
		{
			Dbg.LogError("ItemManager cannot load items csv.", Dbg.NBLogType.INVENTORY);
			return false;
		}
		m_csvEquips = instance.GetParametersFor(m_csvEquipParam);
		if (m_csvEquips == null)
		{
			Dbg.LogError("ItemManager cannot load equipments csv.", Dbg.NBLogType.INVENTORY);
			return false;
		}
		m_csvLoots = instance.GetParametersFor(m_csvLootParam);
		if (m_csvLoots == null)
		{
			Dbg.LogError("ItemManager cannot load loots csv.", Dbg.NBLogType.INVENTORY);
			return false;
		}
		m_csvItemsSkill = instance.GetParametersFor(m_csvItemSkillParam);
		if (m_csvItemsSkill == null)
		{
			Dbg.LogError("ItemManager cannot load items skill csv.", Dbg.NBLogType.INVENTORY);
			return false;
		}
		m_csvItemsType = instance.GetParametersFor(m_csvItemTypeParam);
		if (m_csvItemsType == null)
		{
			Dbg.LogError("ItemManager cannot load items type csv.", Dbg.NBLogType.INVENTORY);
			return false;
		}
		m_csvFusionValue = instance.GetParametersFor(m_csvFusionValueParam);
		if (m_csvFusionValue == null)
		{
			Dbg.LogError("FusionValue cannot load items type csv.", Dbg.NBLogType.INVENTORY);
			return false;
		}
		m_csvTierRange = instance.GetParametersFor(m_csvTierRangeParam);
		if (m_csvTierRange == null)
		{
			Dbg.LogError("TierRange cannot load its csv.", Dbg.NBLogType.INVENTORY);
			return false;
		}
		m_csvSkillVisuals = instance.GetParametersFor(m_csvSkillVisualParam);
		if (m_csvSkillVisuals == null)
		{
			Dbg.LogError("m_csvSkillVisuals cannot load its csv.", Dbg.NBLogType.INVENTORY);
			return false;
		}
		m_csvArmorCollections = instance.GetParametersFor(m_csvArmorCollectionsParam);
		if (m_csvArmorCollections == null)
		{
			Dbg.LogError("m_csvArmorCollections cannot load its csv.", Dbg.NBLogType.INVENTORY);
			return false;
		}
		return true;
	}

	public void PrepareItemDatabase()
	{
		int num = 0;
		ParameterManager instance = SingletonComponent<ParameterManager>.Instance;
		Ivm = SingletonComponent<ItemVisualManager>.Instance;
		m_itemsBase = new Dictionary<string, ItemBase>();
		m_equipments = new Dictionary<string, ItemEquipment>();
		m_itemsSkill = new Dictionary<string, ItemSkill>();
		m_itemsType = new Dictionary<string, ItemType>();
		m_loots = new Dictionary<string, ItemLoot>();
		m_armorSets = new Dictionary<string, ArmorSet>();
		m_specialArmorSets = new Dictionary<string, ArmorSet>();
		m_fusionValues = new Dictionary<int, FusionValue>();
		m_tierRange = new Dictionary<int, TierRange>();
		m_skillVisuals = new Dictionary<ItemEnum.Skill, SkillVisual>();
		m_skillSpriteList = m_skillAtlas.GetListOfSprites();
		Dbg.Log("NULL The skill atlas. This is not an error.", Dbg.NBLogType.INVENTORY);
		m_skillAtlas = null;
		List<string> rowsNames = m_csvTierRange.RowsNames;
		if (rowsNames != null && rowsNames.Count > 0)
		{
			NBGeneric.TryParse<int>(rowsNames[0], out TierRange.TIER_MIN);
			NBGeneric.TryParse<int>(rowsNames[rowsNames.Count - 1], out TierRange.TIER_MAX);
		}
		for (num = 0; num < m_csvTierRange.RowsCount; num++)
		{
			int dataGeneric = m_csvTierRange.GetDataGeneric<int>(num, 0);
			int dataGeneric2 = m_csvTierRange.GetDataGeneric<int>(num, 1);
			int bLvlEnd = dataGeneric + dataGeneric2 - 1;
			TierRange value = new TierRange(dataGeneric, bLvlEnd, dataGeneric2);
			m_tierRange.Add(num + 1, value);
		}
		rowsNames = m_csvSkillVisuals.RowsNames;
		for (num = 0; num < m_csvSkillVisuals.RowsCount; num++)
		{
			string text = rowsNames[num];
			ItemEnum.Skill skill = ItemEnum.SkillFromString(text);
			string data = m_csvSkillVisuals.GetData(num, 0);
			string data2 = m_csvSkillVisuals.GetData(num, 1);
			string text2 = m_csvSkillVisuals.GetData(num, 2);
			if (text2 != string.Empty)
			{
				text2 = "Skill_icons/" + text2;
			}
			string data3 = m_csvSkillVisuals.GetData(num, 3);
			string data4 = m_csvSkillVisuals.GetData(num, 4);
			string text3 = m_csvSkillVisuals.GetData(num, 5);
			if (text3 != string.Empty)
			{
				text3 = "Skill_icons/" + text3;
			}
			SkillVisual value2 = new SkillVisual(skill, data, data2, text2, data3, data4, text3);
			if (!m_skillVisuals.ContainsKey(skill))
			{
				m_skillVisuals.Add(skill, value2);
			}
		}
		rowsNames = m_csvFusionValue.RowsNames;
		for (num = 0; num < m_csvFusionValue.RowsCount; num++)
		{
			int num2 = int.Parse((!(rowsNames[num] == string.Empty)) ? rowsNames[num] : "1");
			int dataGeneric3 = m_csvFusionValue.GetDataGeneric<int>(num, 0);
			int dataGeneric4 = m_csvFusionValue.GetDataGeneric<int>(num, 1);
			int dataGeneric5 = m_csvFusionValue.GetDataGeneric<int>(num, 2);
			FusionValue value3 = new FusionValue(num2, dataGeneric3, dataGeneric4, dataGeneric5);
			if (!m_fusionValues.ContainsKey(num2))
			{
				m_fusionValues.Add(num2, value3);
			}
			else
			{
				Dbg.LogError("The key already exists in dictionary : " + num2, Dbg.NBLogType.INVENTORY);
			}
		}
		CreateNullItem();
		instance.UnloadParametersFor(m_csvFusionValueParam);
		instance.UnloadParametersFor(m_csvTierRangeParam);
		rowsNames = m_csvArmorCollections.RowsNames;
		int count = rowsNames.Count;
		ArmorSet armorSet = null;
		for (num = 0; num < count; num++)
		{
			string data5 = m_csvArmorCollections.GetData(num, "SET_NAME");
			string data6 = m_csvArmorCollections.GetData(num, "SET_IMAGE_PATH");
			string data7 = m_csvArmorCollections.GetData(num, "SET_DESCRIPTION");
			string data8 = m_csvArmorCollections.GetData(num, "SET_BONUS_NAME");
			string data9 = m_csvArmorCollections.GetData(num, "SET_BONUS_DESCRIPTION");
			string data10 = m_csvArmorCollections.GetData(num, "SET_ELEMENT");
			int dataGeneric6 = m_csvArmorCollections.GetDataGeneric<int>(num, "SET_TIER");
			int dataGeneric7 = m_csvArmorCollections.GetDataGeneric<int>(num, "SET_MAX_TIER");
			string text4 = rowsNames[num];
			Element.Type p_elementType = Element.FromString(data10);
			armorSet = new ArmorSet(text4, data5, data6, p_elementType, dataGeneric6, dataGeneric7, data7, data8, data9);
			if (IsSpecial(text4))
			{
				if (!m_specialArmorSets.ContainsKey(text4))
				{
					m_specialArmorSets.Add(text4, armorSet);
				}
			}
			else if (!m_armorSets.ContainsKey(text4))
			{
				m_armorSets.Add(text4, armorSet);
			}
		}
	}

	private bool IsSpecial(string p_setID)
	{
		return p_setID.Contains("special");
	}

	public void CreateArmorCollection()
	{
		if (null == SingletonComponent<PlayerDataManager>.Instance)
		{
			return;
		}
		PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		foreach (ServerInventoryItem value in playerData.details.m_armorCollection.Values)
		{
			string itemIDNoSkill = value.itemIDNoSkill;
			InventoryManager instance = SingletonComponent<InventoryManager>.Instance;
			bool p_isNew = !(null == instance) && instance.IsNewCollectionItem(value.collectionId);
			int p_level = -1;
			if (!itemIDNoSkill.Contains(GLOBAL.DEFAULT_BASE_ITEM_FLAG))
			{
				itemIDNoSkill += GLOBAL.DEFAULT_ITEM_SKILL_PREFIX;
			}
			else
			{
				itemIDNoSkill += GLOBAL.DEFAULT_BASE_ITEM_ENDING;
				p_level = 0;
			}
			EnableArmor(itemIDNoSkill, value.tier, p_isNew, p_level);
		}
	}

	private void PrintArmorCount()
	{
	}

	public bool IsSpecialItem(string p_itemId, int p_tier)
	{
		string key = p_itemId + "_" + p_tier;
		if (m_equipments.ContainsKey(key))
		{
			ItemEquipment itemEquipment = m_equipments[key];
			if (itemEquipment != null)
			{
				return itemEquipment.isSpecial;
			}
		}
		return false;
	}

	public bool IsNormalItem(string p_itemId, int p_tier)
	{
		return !IsSpecialItem(p_itemId, p_tier);
	}

	private void EnableArmor(string p_itemKey, int p_tier, bool p_isNew, int p_level = -1)
	{
		ItemDetails itemDetails = CreateItemDetails(p_itemKey, p_tier, p_level);
		string setID = itemDetails.equipment.setID;
		Equipment.Part part = itemDetails.equipment.part;
		if (m_armorSets.ContainsKey(setID))
		{
			if (m_armorSets.ContainsKey(setID))
			{
				ArmorSet armorSet = m_armorSets[setID];
				armorSet.isNew |= p_isNew;
				itemDetails.isEnabled = true;
				armorSet.AddItem(itemDetails);
			}
		}
		else if (m_specialArmorSets.ContainsKey(setID))
		{
			ArmorSet armorSet2 = m_specialArmorSets[setID];
			armorSet2.isNew |= p_isNew;
			itemDetails.isEnabled = true;
			armorSet2.AddItem(itemDetails);
		}
	}

	private void CreateNullItem()
	{
		ItemBase itemBase = new ItemBase("WEAPON_BASE_VERY_COMMON", "content_sword_label", ItemEnum.Chest.LOWEST, 0f, ItemEnum.Type.BASE_WEAPON, Element.Type.Metal, ItemEnum.Skill.NONE);
		ItemEquipment equipment = new ItemEquipment(itemBase.itemID, 1, string.Empty, string.Empty, new List<string>(), "null_item", Equipment.Part.Weapon, null, false, TierRange.DEFAULT_TIER_RANGE, 1);
		ItemSkill skill = new ItemSkill(itemBase.skill, 1);
		ItemType typeData = new ItemType(itemBase.type, 1, 0f, 0f, 0f, 0f, 0f);
		ItemLoot loot = new ItemLoot();
		FusionValue fusion = new FusionValue(1, 1, 1, 1);
		s_NULL_DATA = new ItemDetails(itemBase, equipment, skill, typeData, loot, fusion);
		s_NULL_DATA.SetVisual(SingletonComponent<ItemVisualManager>.Instance.m_globalIcons, SingletonComponent<ItemVisualManager>.Instance.GetElementSprite(itemBase.elementType), SingletonComponent<ItemVisualManager>.Instance.m_errorSpriteName);
	}

	public bool VerifyItemLevelTier(ItemEnum.Type p_itemType, int p_baseLevel, int p_tier)
	{
		if (p_tier < TierRange.TIER_MIN || p_tier > TierRange.TIER_MAX)
		{
			Dbg.LogError("Item tier Range should be from " + TierRange.TIER_MIN + " to " + TierRange.TIER_MAX);
			return false;
		}
		if ((p_itemType == ItemEnum.Type.ESSENCE || p_itemType == ItemEnum.Type.CONSUMABLE) && p_baseLevel != -1)
		{
			Dbg.LogError("Essence should have level -1");
			return false;
		}
		if (p_itemType == ItemEnum.Type.BASE_WEAPON || p_itemType == ItemEnum.Type.BASE_ARMOUR_CHEST || p_itemType == ItemEnum.Type.BASE_ARMOUR_HELM || p_itemType == ItemEnum.Type.BASE_ARMOUR_LEGGINGS || p_itemType == ItemEnum.Type.BASE_ARMOUR_SHOULDERS)
		{
			if (p_baseLevel != 0)
			{
				Dbg.LogError("Base Item should have level 0");
				return false;
			}
		}
		else
		{
			TierRange value = null;
			if (!m_tierRange.TryGetValue(p_tier, out value))
			{
				Dbg.LogError("Could not find tier : " + p_tier);
				return false;
			}
		}
		return true;
	}

	public TierRange GetTierRange(int _tier)
	{
		TierRange value = TierRange.DEFAULT_TIER_RANGE;
		if (!m_tierRange.TryGetValue(_tier, out value))
		{
			Dbg.LogError("Could not find this TierRange for tier number : " + _tier + ". Return Default one.", Dbg.NBLogType.INVENTORY);
		}
		return value;
	}

	public static int TierClamp(int _tier)
	{
		return Mathf.Clamp(_tier, 1, 5);
	}

	public IEnumerator<string> GetArmorSetKeysEnumerator(bool p_special = false)
	{
		if (p_special)
		{
			if (m_specialArmorSets == null)
			{
				return null;
			}
			return m_specialArmorSets.Keys.GetEnumerator();
		}
		if (m_armorSets == null)
		{
			return null;
		}
		return m_armorSets.Keys.GetEnumerator();
	}

	public int GetArmorSetCount(bool p_special = false)
	{
		if (p_special)
		{
			if (m_specialArmorSets == null)
			{
				return 0;
			}
			return m_specialArmorSets.Count;
		}
		if (m_armorSets == null)
		{
			return 0;
		}
		return m_armorSets.Count;
	}

	public ArmorSet GetArmorSet(string p_setID, bool p_special = false)
	{
		if (p_setID == null || string.Empty == p_setID)
		{
			return null;
		}
		if (p_special)
		{
			if (m_specialArmorSets == null)
			{
				return null;
			}
			if (m_specialArmorSets.ContainsKey(p_setID))
			{
				return m_specialArmorSets[p_setID];
			}
			return null;
		}
		if (m_armorSets == null)
		{
			return null;
		}
		if (m_armorSets.ContainsKey(p_setID))
		{
			return m_armorSets[p_setID];
		}
		return null;
	}

	private bool InitItem(string _itemID, int _tier, int _baseLevel)
	{
		string text = _itemID.ToUpper();
		if (!InitBaseItem(text))
		{
			return false;
		}
		ItemBase itemBase = GetItemBase(text);
		if (itemBase == null)
		{
			return false;
		}
		if (itemBase.type != ItemEnum.Type.ESSENCE)
		{
			if (!InitEquipItem(text, ref _tier))
			{
				return false;
			}
			if (_baseLevel == -1)
			{
				TierRange tierRange = GetTierRange(_tier);
				_baseLevel = tierRange.BaseLevelStart;
			}
			if (!InitSkill(itemBase.skill, _baseLevel, _tier))
			{
				return false;
			}
			if (!InitType(itemBase.type, _baseLevel))
			{
				return false;
			}
		}
		else if (!InitLoot(text))
		{
			return false;
		}
		return true;
	}

	private bool InitBaseItem(string _upperItemID)
	{
		int num = m_csvItems.RowsNames.IndexOf(_upperItemID);
		if (num == -1)
		{
			Dbg.LogWarn("Could not find row : " + _upperItemID + " ItemCSV.", Dbg.NBLogType.INVENTORY);
			return false;
		}
		string data = m_csvItems.GetData(num, 0);
		ItemEnum.Chest chest = ItemEnum.ChestFromString(m_csvItems.GetData(num, 1));
		float dataGeneric = m_csvItems.GetDataGeneric<float>(num, 2);
		ItemEnum.Type type = ItemEnum.TypeFromString(m_csvItems.GetData(num, 3));
		Element.Type elementType = Element.FromString(m_csvItems.GetData(num, 4));
		ItemEnum.Skill skill = ItemEnum.SkillFromString(m_csvItems.GetData(num, 5));
		if (type == ItemEnum.Type.Max)
		{
			Dbg.LogError("MAX TYPE ? for ID " + _upperItemID, Dbg.NBLogType.INVENTORY);
			return false;
		}
		ItemBase value = new ItemBase(_upperItemID, data, chest, dataGeneric, type, elementType, skill);
		if (!m_itemsBase.ContainsKey(_upperItemID))
		{
			m_itemsBase.Add(_upperItemID, value);
		}
		return true;
	}

	private bool InitEquipItem(string _upperItemID, ref int _tier)
	{
		List<string> rowsNames = m_csvEquips.RowsNames;
		int num = rowsNames.IndexOf(_upperItemID);
		if (num == -1)
		{
			Dbg.LogWarn("Could not find row : " + _upperItemID + " in EquipCSV.", Dbg.NBLogType.INVENTORY);
			return false;
		}
		int i = num;
		int num2;
		for (num2 = num + TierRange.TIER_MAX; i < num2; i++)
		{
			int dataGeneric = m_csvEquips.GetDataGeneric<int>(i, 0);
			if (_tier == -1)
			{
				_tier = dataGeneric;
				break;
			}
			if (dataGeneric == _tier)
			{
				num = i;
				break;
			}
		}
		if (i == num2)
		{
			Dbg.LogWarn("Could not find row : " + _upperItemID + " in EquipCSV with tier : " + _tier, Dbg.NBLogType.INVENTORY);
			return false;
		}
		string data = m_csvEquips.GetData(num, "SET_ID");
		string data2 = m_csvEquips.GetData(num, "ASSET_BUNDLE");
		List<string> catalysts = new List<string>(m_csvEquips.GetDataAsStringArray(num, "CATALYST"));
		string data3 = m_csvEquips.GetData(num, "ITEM_NAME");
		Equipment.Part part = Equipment.FromString(_upperItemID);
		CharacterElement characterElement = SingletonComponent<CharacterElementManager>.Instance.GetCharacterElement(data2);
		int maxTier = _tier;
		bool canEvolve = false;
		for (i = num + 1; i < num + TierRange.TIER_MAX; i++)
		{
			if (i < rowsNames.Count && rowsNames[i] == _upperItemID)
			{
				maxTier = m_csvEquips.GetDataGeneric<int>(i, 0);
				canEvolve = true;
			}
		}
		if (m_itemsBase.ContainsKey(_upperItemID) && (m_itemsBase[_upperItemID].type == ItemEnum.Type.CONSUMABLE || m_itemsBase[_upperItemID].type == ItemEnum.Type.ESSENCE || m_itemsBase[_upperItemID].type == ItemEnum.Type.PET))
		{
			canEvolve = false;
			Dbg.Log("[Info] The item :" + _upperItemID + " cannot evolve", Dbg.NBLogType.INVENTORY);
		}
		TierRange tierRange = GetTierRange(_tier);
		ItemEquipment itemEquipment = new ItemEquipment(_upperItemID, _tier, data, data2, catalysts, data3, part, characterElement, canEvolve, tierRange, maxTier, IsSpecial(data));
		string key = _upperItemID + "_" + itemEquipment.tier;
		if (!m_equipments.ContainsKey(key))
		{
			m_equipments.Add(key, itemEquipment);
		}
		return true;
	}

	private bool InitSkill(ItemEnum.Skill _skill, int _baseLevel, int _tier)
	{
		int num = _baseLevel + TierClamp(_tier) - 1;
		string text = _skill.ToString() + "_" + num;
		List<string> rowsNames = m_csvItemsSkill.RowsNames;
		string text2 = _skill.ToString();
		int num2 = rowsNames.IndexOf(text2);
		if (num2 == -1)
		{
			Dbg.LogWarn("Could not find row : " + text2 + " in EquipCSV.", Dbg.NBLogType.INVENTORY);
			return false;
		}
		int i;
		for (i = num2; i < rowsNames.Count; i++)
		{
			int dataGeneric = m_csvItemsSkill.GetDataGeneric<int>(i, 0);
			if (dataGeneric == num)
			{
				num2 = i;
				break;
			}
		}
		if (i == rowsNames.Count)
		{
			Dbg.LogWarn("Could not find row : " + text + " in ItemSkillCSV.", Dbg.NBLogType.INVENTORY);
			return false;
		}
		int dataGeneric2 = m_csvItemsSkill.GetDataGeneric<int>(num2, 1);
		string data = m_csvItemsSkill.GetData(num2, 2);
		SkillEnum.SelfSkill selfAction = SkillEnum.ParseSelfSkill(data);
		float dataGeneric3 = m_csvItemsSkill.GetDataGeneric<float>(num2, 3);
		string data2 = m_csvItemsSkill.GetData(num2, 4);
		SkillEnum.AssistSkill assistAction = SkillEnum.ParseAssistSkill(data2);
		float dataGeneric4 = m_csvItemsSkill.GetDataGeneric<float>(num2, 5);
		SkillVisual value = SkillVisual.NONE_SKILL_VISUAL;
		if (!m_skillVisuals.TryGetValue(_skill, out value))
		{
			Dbg.LogError(string.Concat("Could not find Skill : ", _skill, " in visualSkill CSV. Use SkillVisual.NONE_SKILL_VISUAL"), Dbg.NBLogType.INVENTORY);
			value = SkillVisual.NONE_SKILL_VISUAL;
		}
		ItemSkill value2 = new ItemSkill(_skill, num, dataGeneric2, selfAction, dataGeneric3, assistAction, dataGeneric4, value);
		if (!m_itemsSkill.ContainsKey(text))
		{
			m_itemsSkill.Add(text, value2);
		}
		return true;
	}

	private bool InitType(ItemEnum.Type _itemType, int _baseLevel)
	{
		List<string> rowsNames = m_csvItemsType.RowsNames;
		string item = _itemType.ToString();
		int num = rowsNames.IndexOf(item);
		if (num == -1)
		{
			Dbg.LogWarn(string.Concat("Could not find row : ", _itemType, " in ItemTypeCSV."), Dbg.NBLogType.INVENTORY);
			return false;
		}
		int i;
		for (i = num; i < rowsNames.Count; i++)
		{
			int dataGeneric = m_csvItemsType.GetDataGeneric<int>(i, 0);
			if (dataGeneric == _baseLevel)
			{
				num = i;
				break;
			}
		}
		if (i == rowsNames.Count)
		{
			Dbg.LogWarn(string.Concat("Could not find row : ", _itemType, " in ItemType with baseLevel : ", _baseLevel), Dbg.NBLogType.INVENTORY);
			return false;
		}
		float dataGeneric2 = m_csvItemsType.GetDataGeneric<float>(num, 1);
		float dataGeneric3 = m_csvItemsType.GetDataGeneric<float>(num, 2);
		float dataGeneric4 = m_csvItemsType.GetDataGeneric<float>(num, 3);
		float dataGeneric5 = m_csvItemsType.GetDataGeneric<float>(num, 4);
		float dataGeneric6 = m_csvItemsType.GetDataGeneric<float>(num, 5);
		ItemType itemType = new ItemType(_itemType, _baseLevel, dataGeneric2, dataGeneric3, dataGeneric4, dataGeneric5, dataGeneric6);
		string key = string.Concat(_itemType, "_", itemType.baseLevel);
		if (!m_itemsType.ContainsKey(key))
		{
			m_itemsType.Add(key, itemType);
		}
		return true;
	}

	private bool InitLoot(string _upperItemID)
	{
		List<string> rowsNames = m_csvLoots.RowsNames;
		string text = _upperItemID;
		if (!rowsNames.Contains(text))
		{
			Dbg.LogWarn("Could not find row : " + text + " in ItemLootCSV.", Dbg.NBLogType.INVENTORY);
			return false;
		}
		ItemEnum.LootType type = ItemEnum.LootTypeString(m_csvLoots.GetData(text, "ITEM_EFFECT"));
		int dataGeneric = m_csvLoots.GetDataGeneric<int>(text, "ITEM_TIER");
		bool consumable = m_csvLoots.GetDataGeneric<int>(text, "CONSUMABLE") == 1;
		float dataGeneric2 = m_csvLoots.GetDataGeneric<float>(text, "DURATION");
		float dataGeneric3 = m_csvLoots.GetDataGeneric<float>(text, "DUNGEON_DROP_RATE");
		float dataGeneric4 = m_csvLoots.GetDataGeneric<float>(text, "GACHA_DROP_RATE_SOCIAL");
		float dataGeneric5 = m_csvLoots.GetDataGeneric<float>(text, "GACHA_DROP_RATE_PREMIUM");
		string data = m_csvLoots.GetData(text, "ATLAS_NAME");
		string data2 = m_csvLoots.GetData(text, "SPRITE_NAME");
		string data3 = m_csvLoots.GetData(text, "DESCRIPTION_HEADER");
		string data4 = m_csvLoots.GetData(text, "DESCRIPTION_CONTENT");
		float fusionValue = m_csvLoots.GetDataGeneric<int>(text, "FUSION_GAIN_BASE");
		float sellValue = m_csvLoots.GetDataGeneric<int>(text, "SELL_VALUE_BASE");
		ItemLoot value = new ItemLoot(text, type, dataGeneric, consumable, dataGeneric2, dataGeneric3, dataGeneric4, dataGeneric5, data, data2, data3, data4, fusionValue, sellValue);
		text = text + "_" + dataGeneric;
		if (!m_loots.ContainsKey(text))
		{
			m_loots.Add(text, value);
		}
		return true;
	}

	private ItemBase GetItemBase(string _itemID)
	{
		ItemBase value = null;
		if (!m_itemsBase.TryGetValue(_itemID, out value))
		{
			Dbg.LogWarn("Could not find ItemBase : " + _itemID, Dbg.NBLogType.INVENTORY);
			return null;
		}
		return value;
	}

	private ItemEquipment GetItemEquipment(string _itemID, int _tier)
	{
		string text = _itemID + "_" + TierClamp(_tier);
		ItemEquipment value = null;
		if (!m_equipments.TryGetValue(text, out value))
		{
			if (_tier <= -1)
			{
				return GetFirstEquipment(_itemID);
			}
			Dbg.LogWarn("Could not find ItemEquipment : " + text, Dbg.NBLogType.INVENTORY);
		}
		return value;
	}

	private ItemSkill GetItemSkill(ItemEnum.Skill _skill, int _baseLevel, int _tier)
	{
		int num = _baseLevel + TierClamp(_tier) - 1;
		string text = string.Concat(_skill, "_", num);
		ItemSkill value = null;
		if (!m_itemsSkill.TryGetValue(text, out value))
		{
			if (_tier <= -1 || _baseLevel <= -1)
			{
				return GetFirstSkill(_skill);
			}
			Dbg.LogWarn("Could not find ItemSkill : " + text, Dbg.NBLogType.INVENTORY);
		}
		return value;
	}

	private ItemType GetItemType(ItemEnum.Type _type, int _baseLevel)
	{
		string text = string.Concat(_type, "_", _baseLevel);
		ItemType value = null;
		if (!m_itemsType.TryGetValue(text, out value))
		{
			if (_baseLevel <= -1)
			{
				return GetFirstItemType(_type);
			}
			Dbg.LogWarn("Could not find ItemType : " + text);
		}
		return value;
	}

	private ItemLoot GetItemLoot(string _itemID, int _tier)
	{
		ItemLoot value = null;
		int num = Mathf.Clamp(_tier, 0, 5);
		string text = _itemID + "_" + num;
		if (!m_loots.TryGetValue(text, out value))
		{
			if (_tier <= -1)
			{
				return GetFirstLoot(_itemID);
			}
			Dbg.LogWarn("Could not find ItemLoot : " + text, Dbg.NBLogType.INVENTORY);
		}
		return value;
	}

	private FusionValue GetFusion(int _tier)
	{
		int num = TierClamp(_tier);
		FusionValue value = null;
		if (!m_fusionValues.TryGetValue(num, out value))
		{
			Dbg.LogWarn("Could not find the Fusion Values : " + num, Dbg.NBLogType.INVENTORY);
		}
		return value;
	}

	private ItemEquipment GetFirstEquipment(string _itemID)
	{
		string text = _itemID.ToLower();
		foreach (ItemEquipment value in m_equipments.Values)
		{
			if (value.itemID.ToLower() == text)
			{
				return value;
			}
		}
		Dbg.LogWarn("Could not find this item in CSV. Create a new one : " + _itemID, Dbg.NBLogType.INVENTORY);
		return new ItemEquipment();
	}

	private ItemLoot GetFirstLoot(string _itemID)
	{
		string text = _itemID.ToLower();
		foreach (ItemLoot value in m_loots.Values)
		{
			if (value.itemID.ToLower() == text)
			{
				return value;
			}
		}
		Dbg.LogWarn("The first loot was not found return new one : " + _itemID, Dbg.NBLogType.INVENTORY);
		return new ItemLoot();
	}

	private ItemSkill GetFirstSkill(ItemEnum.Skill _skill)
	{
		foreach (ItemSkill value in m_itemsSkill.Values)
		{
			if (value.type == _skill)
			{
				return value;
			}
		}
		Dbg.LogWarn(string.Concat("Could not find the first item skill for : ", _skill, " Create a new one."), Dbg.NBLogType.INVENTORY);
		return new ItemSkill();
	}

	private ItemType GetFirstItemType(ItemEnum.Type _type)
	{
		foreach (ItemType value in m_itemsType.Values)
		{
			if (value.type == _type)
			{
				return value;
			}
		}
		Dbg.LogWarn(string.Concat("Could not find first Item type for : ", _type, " Create a new one!"), Dbg.NBLogType.INVENTORY);
		return new ItemType(_type, 1, 0f, 0f, 0f, 0f, 0f);
	}

	public ItemDetails CreateItemDetails(ServerInventoryItem _serverInventoryItem)
	{
		string item_id = _serverInventoryItem.item_id;
		int tier = _serverInventoryItem.tier;
		int item_level = _serverInventoryItem.item_level;
		ItemDetails itemDetails = CreateItemDetails(item_id, tier, item_level);
		if (itemDetails == s_NULL_DATA || itemDetails == null)
		{
			Dbg.LogError("Could not recreate this item. It won't be added to your local inventory : " + _serverInventoryItem.db_id + "  " + item_id, Dbg.NBLogType.INVENTORY);
			return s_NULL_DATA;
		}
		if (itemDetails != null)
		{
			itemDetails.serverData = _serverInventoryItem;
			itemDetails.SetData(_serverInventoryItem.db_id, _serverInventoryItem.item_type, _serverInventoryItem.isEquip, _serverInventoryItem.fusion_xp);
		}
		return itemDetails;
	}

	public ItemDetails CreateItemDetails(string _itemID, int _tier = -1, int _baseLevel = -1)
	{
		if (!InitItem(_itemID, _tier, _baseLevel))
		{
			Dbg.LogError("Could not load this item from CSV : " + _itemID, Dbg.NBLogType.INVENTORY);
			return s_NULL_DATA;
		}
		ItemBase itemBase = GetItemBase(_itemID);
		ItemEquipment itemEquipment = null;
		ItemSkill itemSkill = null;
		ItemType itemType = null;
		ItemLoot itemLoot = null;
		FusionValue fusionValue = null;
		if (itemBase == null)
		{
			return s_NULL_DATA;
		}
		ItemEnum.Category category = ItemEnum.CategoryFromString(itemBase.type.ToString());
		bool flag = category == ItemEnum.Category.LOOT;
		int num = _baseLevel;
		if (num == -1)
		{
			if (_tier != -1 && !flag)
			{
				TierRange tierRange = GetTierRange(_tier);
				num = tierRange.BaseLevelStart;
			}
		}
		else if (!VerifyItemLevelTier(itemBase.type, num, _tier))
		{
			return s_NULL_DATA;
		}
		if (flag)
		{
			itemEquipment = new ItemEquipment();
			itemSkill = new ItemSkill();
			itemType = new ItemType(ItemEnum.Type.NONE, num, 0f, 0f, 0f, 0f, 0f);
			itemLoot = ((!_itemID.Contains("ESSENCE")) ? GetItemLoot(_itemID, _tier) : GetItemLoot(_itemID, 1));
			fusionValue = GetFusion(_tier);
		}
		else
		{
			itemEquipment = GetItemEquipment(_itemID, _tier);
			itemSkill = GetItemSkill(itemBase.skill, num, _tier);
			itemType = GetItemType(itemBase.type, num);
			itemLoot = new ItemLoot();
			fusionValue = GetFusion(_tier);
		}
		if (itemEquipment == null || itemSkill == null || itemType == null || itemLoot == null || fusionValue == null)
		{
			Dbg.LogError("Could not recreate this item from CSV.", Dbg.NBLogType.INVENTORY);
			return s_NULL_DATA;
		}
		ItemDetails itemDetails = new ItemDetails(itemBase, itemEquipment, itemSkill, itemType, itemLoot, fusionValue);
		itemDetails.SetData("-5", category.ToString(), false, 0);
		string bgSrpite = string.Empty;
		if (itemDetails.Category == ItemEnum.Category.EQUIPMENT && itemEquipment.characterElement != null)
		{
			bgSrpite = itemEquipment.characterElement.iconName;
		}
		if (itemDetails.Category == ItemEnum.Category.LOOT)
		{
			bgSrpite = itemLoot.spriteName;
		}
		SetItemVisual(itemDetails, bgSrpite, itemBase.elementType);
		return itemDetails;
	}

	public void SetItemVisual(ItemDetails _item, string _bgSrpite, Element.Type _elementType)
	{
		UIAtlas uIAtlas = null;
		uIAtlas = ((_item.Category != ItemEnum.Category.LOOT) ? SingletonComponent<ItemVisualManager>.Instance.m_globalIcons : SingletonComponent<ItemVisualManager>.Instance.GetAtlas(_item.BGAtlasName));
		string _outSpriteName = SingletonComponent<ItemVisualManager>.Instance.m_errorSpriteName;
		Ivm.GetVisual(_item.BGAtlasName, _bgSrpite, out uIAtlas, out _outSpriteName);
		_item.SetVisual(uIAtlas, Ivm.GetElementSprite(_elementType), _outSpriteName);
	}

	public Color GetCompareColor(float itemValue, float equippedValue)
	{
		Color result = ((itemValue > equippedValue) ? betterColor : ((!(itemValue < equippedValue)) ? sameColor : worseColor));
		result.a = 1f;
		return result;
	}

	public string GetCompareColorTag(float itemValue, float equippedValue)
	{
		return (itemValue > equippedValue) ? m_colorTags[0] : ((!(itemValue < equippedValue)) ? m_colorTags[2] : m_colorTags[1]);
	}

	public NBBinaryFormat GetEquipItemCsv()
	{
		return m_csvEquips;
	}

	public NBBinaryFormat GetItemCsv()
	{
		return m_csvItems;
	}

	public NBBinaryFormat GetArmorCollectionsCsv()
	{
		return m_csvArmorCollections;
	}
}
