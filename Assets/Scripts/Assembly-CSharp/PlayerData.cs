using System.Collections;
using System.Collections.Generic;
using Models;
using UnityEngine;

public class PlayerData : IPlayer
{
	public static readonly float defaultAgi = 5f;

	public static readonly float defaultCon = 30f;

	public static readonly float defaultStr = 25f;

	public readonly int MaxLevel = 50;

	public string m_name = GLOBAL.CHARACTER_DEFAULT_NAME;

	public int m_level = 1;

	public string m_title = string.Empty;

	public float m_agility;

	public float m_constitution;

	public float m_strength;

	public float m_lifeValue;

	public float m_lifeMaxValue;

	public float m_minAttackValue;

	public float m_maxAttackValue;

	public float m_defenseValue;

	public float m_criticalChance;

	public float m_criticalMul = 2f;

	public Equipment.Data[] m_equips;

	public bool m_isMonster;

	public MonsterData m_monsterData = new MonsterData();

	public UserData m_userData = new UserData();

	public int m_targetXp;

	public int m_previousTargetXp;

	public bool m_isInLevelRangeForCurrentDungeon;

	public int m_FriendLimit = 1;

	public int m_PendingFriendRequest;

	public int m_AnnouncementCount;

	private CharacterType m_characterType;

	private int m_cooldown;

	private string m_experienceCSV = "Experiences";

	private string m_colExpNextLevel = "EXP_NEXT_LEVEL";

	public CharacterAccountDetails details { get; private set; }

	public bool ApplyCriticalFormula { get; set; }

	public string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			m_name = ((!(value == string.Empty)) ? value : GLOBAL.CHARACTER_DEFAULT_NAME);
		}
	}

	public int Level
	{
		get
		{
			return m_level;
		}
		set
		{
			m_level = value;
		}
	}

	public float Agi
	{
		get
		{
			return m_agility;
		}
		set
		{
			m_agility = value;
			UpdateCriticalChance();
		}
	}

	public float Con
	{
		get
		{
			return m_constitution;
		}
		set
		{
			m_constitution = value;
			UpdateArmor();
		}
	}

	public float Str
	{
		get
		{
			return m_strength;
		}
		set
		{
			m_strength = value;
			UpdateDamage();
		}
	}

	public int BaseLife
	{
		get
		{
			return Mathf.RoundToInt(m_constitution * 40f);
		}
	}

	public float MaxHp
	{
		get
		{
			return m_lifeMaxValue;
		}
		set
		{
			m_lifeMaxValue = value;
		}
	}

	public float Hp
	{
		get
		{
			return m_lifeValue;
		}
		set
		{
			m_lifeValue = Mathf.Clamp(value, 0f, m_lifeMaxValue);
		}
	}

	public float HpPercent
	{
		get
		{
			return m_lifeValue / m_lifeMaxValue;
		}
		set
		{
			Hp = value * m_lifeMaxValue;
		}
	}

	public float MinAttack
	{
		get
		{
			return m_minAttackValue;
		}
		set
		{
			m_minAttackValue = value;
		}
	}

	public float MaxAttack
	{
		get
		{
			return m_maxAttackValue;
		}
		set
		{
			m_maxAttackValue = value;
		}
	}

	public float Def
	{
		get
		{
			return m_defenseValue;
		}
		set
		{
			m_defenseValue = value;
		}
	}

	public float Atk
	{
		get
		{
			return Random.Range(m_minAttackValue, m_maxAttackValue);
		}
	}

	public float CriticalChance
	{
		get
		{
			return m_criticalChance;
		}
		set
		{
			m_criticalChance = value;
		}
	}

	public float CriticalMul
	{
		get
		{
			return m_criticalMul;
		}
		set
		{
			m_criticalMul = value;
		}
	}

	public int Xp
	{
		get
		{
			return m_userData.currentXp;
		}
		set
		{
			if (!m_isMonster)
			{
				m_userData.currentXp = Mathf.Max(value, 0);
			}
		}
	}

	public int TargetXp
	{
		get
		{
			UpdateTargetXp(m_level);
			return m_targetXp;
		}
	}

	public int Igc
	{
		get
		{
			return m_userData.inGameCurrency;
		}
		set
		{
			if (!m_isMonster)
			{
				m_userData.inGameCurrency = value;
				m_userData.inGameCurrency = Mathf.Max(m_userData.inGameCurrency, 0);
			}
		}
	}

	public int SocialGachaCount
	{
		get
		{
			int num = 0;
			num += m_userData.freeSocialGacha;
			return num + m_userData.socialCurrency / GLOBAL.GAME_SETTINGS.social_points_per_ticket;
		}
	}

	public int FreeSocialGachaTicketCount
	{
		get
		{
			return m_userData.freeSocialGacha;
		}
	}

	public int PremiumGachaCount
	{
		get
		{
			return m_userData.freePremiumGacha;
		}
	}

	public int TotalGachaCount
	{
		get
		{
			return SocialGachaCount + PremiumGachaCount;
		}
	}

	public Equipment.Data[] Equipments
	{
		get
		{
			return m_equips;
		}
	}

	public Element.Type MonsterElement
	{
		get
		{
			return m_monsterData.element;
		}
		set
		{
			m_monsterData.element = value;
		}
	}

	public CharacterType CharacterType
	{
		get
		{
			return m_characterType;
		}
		set
		{
			m_characterType = value;
		}
	}

	public bool IsMainCharacter
	{
		get
		{
			return m_characterType == CharacterType.MainCharacter;
		}
	}

	public bool IsFriend
	{
		get
		{
			return m_characterType == CharacterType.Friend;
		}
	}

	public int FriendAssistCoolDown
	{
		get
		{
			return m_cooldown;
		}
		set
		{
			m_cooldown = value;
		}
	}

	public bool IsStranger
	{
		get
		{
			return m_characterType == CharacterType.Stranger;
		}
	}

	public Element.Type AttackElement
	{
		get
		{
			if (m_isMonster)
			{
				return MonsterElement;
			}
			return GetEquipmentData(Equipment.Part.Weapon).element;
		}
	}

	public SkillEnum.SelfSkill[] SelfSkills { get; private set; }

	public float[] SelfSkillValues { get; private set; }

	public SkillEnum.AssistSkill FriendAssistSkill
	{
		get
		{
			Equipment.Data data = Equipments[4];
			SkillEnum.AssistSkill result = SkillEnum.AssistSkill.None;
			if (data != null && data.itemDetails != null && data.itemDetails.skill != null)
			{
				result = data.itemDetails.skill.assistAction;
			}
			return result;
		}
	}

	public float FriendAssistValue
	{
		get
		{
			Equipment.Data data = Equipments[4];
			float result = 0f;
			if (data != null && data.itemDetails != null && data.itemDetails.skill != null)
			{
				result = data.itemDetails.skill.assistValue;
			}
			return result;
		}
	}

	public bool HasPet
	{
		get
		{
			if (details == null || details.unlockedPets == null)
			{
				return false;
			}
			return details.unlockedPets.Count > 0;
		}
	}

	public bool IsDummy
	{
		get
		{
			return !m_isMonster && m_userData.nubeeId == string.Empty;
		}
	}

	public PlayerData()
	{
		InitEquipment();
		Reset();
	}

	public PlayerData(PlayerData p_playerData)
	{
		InitEquipment();
		Reset();
		if (p_playerData == null)
		{
			return;
		}
		m_name = p_playerData.m_name;
		m_level = p_playerData.m_level;
		m_title = p_playerData.m_title;
		m_agility = p_playerData.m_agility;
		m_constitution = p_playerData.m_constitution;
		m_strength = p_playerData.m_strength;
		m_lifeValue = p_playerData.m_lifeValue;
		m_lifeMaxValue = p_playerData.m_lifeMaxValue;
		m_minAttackValue = p_playerData.m_minAttackValue;
		m_maxAttackValue = p_playerData.m_maxAttackValue;
		m_defenseValue = p_playerData.m_defenseValue;
		m_criticalChance = p_playerData.m_criticalChance;
		m_criticalMul = p_playerData.m_criticalMul;
		m_isMonster = p_playerData.m_isMonster;
		m_monsterData = new MonsterData(p_playerData.m_monsterData);
		m_userData = new UserData(p_playerData.m_userData);
		m_targetXp = p_playerData.m_targetXp;
		m_previousTargetXp = p_playerData.m_previousTargetXp;
		m_FriendLimit = p_playerData.m_FriendLimit;
		m_PendingFriendRequest = p_playerData.m_PendingFriendRequest;
		m_characterType = p_playerData.m_characterType;
		m_cooldown = p_playerData.m_cooldown;
		m_experienceCSV = p_playerData.m_experienceCSV;
		m_colExpNextLevel = p_playerData.m_colExpNextLevel;
		Equipment.Data[] equipments = p_playerData.Equipments;
		foreach (Equipment.Data data in equipments)
		{
			ItemDetails p_item = SingletonComponent<InventoryManager>.Instance.CreateItem(data.itemDetails.ItemID, data.itemDetails.Tier, data.itemDetails.BaseLevel);
			Equip(p_item);
		}
		if (p_playerData.details != null)
		{
			if (details == null)
			{
				details = new CharacterAccountDetails();
			}
			if (details != null)
			{
				details.currencies.dragonPoints = p_playerData.details.currencies.dragonPoints;
			}
		}
	}

	public void SetAccountDetails(CharacterAccountDetails p_accountDetails)
	{
		if (p_accountDetails == null)
		{
			return;
		}
		if (details != null)
		{
			if (details.m_armorCollection.Count >= p_accountDetails.m_armorCollection.Count)
			{
				p_accountDetails.m_armorCollection = new Dictionary<string, ServerInventoryItem>(details.m_armorCollection);
			}
			if (p_accountDetails.activePet == string.Empty)
			{
				p_accountDetails.activePet = details.activePet;
				p_accountDetails.unlockedPets = new Dictionary<string, PetDetails>(details.unlockedPets);
			}
		}
		details = new CharacterAccountDetails(p_accountDetails);
	}

	public bool AddArmorToCollection(ServerInventoryItem p_armor)
	{
		bool result = false;
		if (details != null && p_armor != null)
		{
			if (!p_armor.IsWeaponOrArmor())
			{
				return false;
			}
			if (!details.m_armorCollection.ContainsKey(p_armor.collectionId))
			{
				Dbg.Log("Adding Armor to collection : " + p_armor.collectionId);
				details.m_armorCollection.Add(p_armor.collectionId, p_armor);
				result = true;
			}
		}
		return result;
	}

	public PetDetails GetActivePetDetails()
	{
		if (details == null)
		{
			return null;
		}
		string activePet = details.activePet;
		return GetPetDetails(activePet);
	}

	public PetDetails GetFirstPetDetails()
	{
		if (details == null || details.unlockedPets == null)
		{
			return null;
		}
		PetDetails result = null;
		if (details.unlockedPets.Count > 0)
		{
			IEnumerator enumerator = details.unlockedPets.Values.GetEnumerator();
			if (enumerator.MoveNext())
			{
				result = enumerator.Current as PetDetails;
			}
		}
		return result;
	}

	public PetDetails GetPetDetails(string p_petID)
	{
		if (details == null || details.unlockedPets == null)
		{
			return null;
		}
		PetDetails value = null;
		details.unlockedPets.TryGetValue(p_petID, out value);
		return value;
	}

	public void Equip(ItemDetails p_item)
	{
		if (p_item == null)
		{
			Dbg.LogError("Cannot Equip null Item.", Dbg.NBLogType.INVENTORY);
			return;
		}
		Equipment.Data equipmentData = GetEquipmentData(p_item.equipment.part);
		if (equipmentData == null)
		{
			Dbg.LogError("No Equipment Data should be null. Which part did you try to access : " + p_item.equipment.part, Dbg.NBLogType.INVENTORY);
			return;
		}
		if (IsMainCharacter)
		{
			if (equipmentData.itemDetails != null)
			{
				equipmentData.itemDetails.IsEquip = false;
			}
			p_item.IsEquip = true;
		}
		equipmentData.itemDetails = p_item;
		equipmentData.lifeMaxMod = p_item.itemTypeWithSkill.hp;
		equipmentData.minDamageMod = p_item.itemTypeWithSkill.minDamage;
		equipmentData.maxDamageMod = p_item.itemTypeWithSkill.maxDamage;
		equipmentData.armorMod = p_item.itemTypeWithSkill.armour;
		equipmentData.critMod = p_item.itemTypeWithSkill.critical;
		equipmentData.element = p_item.Element;
	}

	public void Equip(PetDetails p_pet)
	{
		if (p_pet != null && details != null)
		{
			details.activePet = p_pet.m_petId;
		}
	}

	public bool IsEquipped(ItemDetails p_item)
	{
		if (p_item == null || p_item.equipment == null)
		{
			return false;
		}
		Equipment.Data equipmentData = GetEquipmentData(p_item.equipment.part);
		return equipmentData != null && equipmentData.itemDetails.DB_ID == p_item.DB_ID;
	}

	public bool IsEquipped(PetDetails p_pet)
	{
		if (p_pet == null || details == null)
		{
			return false;
		}
		return details.activePet == p_pet.m_petId;
	}

	public bool IsEquippingBase(Equipment.Part p_part)
	{
		Equipment.Data equipmentData = GetEquipmentData(p_part);
		if (equipmentData != null && equipmentData.itemDetails != null)
		{
			return equipmentData.itemDetails.IsBaseItem;
		}
		return false;
	}

	public void PrintCollection()
	{
	}

	private void InitEquipment()
	{
		m_equips = new Equipment.Data[5];
		for (int i = 0; i < m_equips.Length; i++)
		{
			Equipment.Data data = new Equipment.Data();
			m_equips[i] = data;
		}
	}

	private void Reset()
	{
		m_name = "Unnamed";
		m_level = 1;
		m_agility = defaultAgi;
		m_constitution = defaultCon;
		m_strength = defaultStr;
		m_targetXp = 0;
		m_previousTargetXp = 0;
		ResetValues();
		m_monsterData.element = Element.Type.Metal;
		for (int i = 0; i < m_monsterData.advanceAtkAbility.Length; i++)
		{
			m_monsterData.advanceAtkAbility[i] = false;
		}
		for (int j = 0; j < m_monsterData.specialAtkAbility.Length; j++)
		{
			m_monsterData.specialAtkAbility[j] = false;
		}
	}

	private void ResetValues()
	{
		m_lifeValue = 0f;
		m_lifeMaxValue = 0f;
		m_minAttackValue = 0f;
		m_maxAttackValue = 0f;
		m_defenseValue = 0f;
		m_criticalChance = 0f;
		m_criticalMul = 2f;
	}

	public void ComputeFinalValues()
	{
		ResetValues();
		UpdateLifeMaxValue();
		ResetHp();
		UpdateDamage();
		UpdateArmor();
		UpdateCriticalChance();
		UpdateSkills();
		Dbg.LogWarn(ToString());
	}

	public int GainXp(int p_howMuch)
	{
		if (m_isMonster)
		{
			return 0;
		}
		int num = 0;
		Xp += p_howMuch;
		UpdateTargetXp(m_level);
		while (CanLevelUp())
		{
			num++;
			Level = m_level + 1;
			Xp -= m_targetXp;
			UpdateTargetXp(m_level);
		}
		return num;
	}

	public bool CanLevelUp()
	{
		return m_userData.currentXp >= m_targetXp;
	}

	public void ResetHp()
	{
		m_lifeValue = m_lifeMaxValue;
	}

	public float GetResistanceAgainst(Element.Type p_element)
	{
		float num = m_constitution;
		Equipment.Data[] equips = m_equips;
		foreach (Equipment.Data data in equips)
		{
			if (data != null && data.itemDetails != null)
			{
				num += data.armorMod * Element.resistanceTable[(int)data.element, (int)p_element];
			}
		}
		return num;
	}

	public bool HasEnoughIGC(int _cost)
	{
		if (m_isMonster)
		{
			return false;
		}
		return _cost <= m_userData.inGameCurrency;
	}

	public void DecreasePremium(int _cost)
	{
		m_userData.premiumCurrency -= _cost;
		if (m_userData.premiumCurrency <= 0)
		{
			m_userData.premiumCurrency = 0;
		}
	}

	public bool HasEnoughPremium(int _cost)
	{
		if (m_isMonster)
		{
			return false;
		}
		return _cost <= m_userData.premiumCurrency;
	}

	public Equipment.Data GetEquipmentData(Equipment.Part p_equipmentType)
	{
		if (p_equipmentType == Equipment.Part.Max)
		{
			Dbg.LogError("Wrong part given.");
			return null;
		}
		return m_equips[(int)p_equipmentType];
	}

	public bool IsEquipingFullSet(string p_equipmentSet)
	{
		bool flag = true;
		for (int i = 0; i < 5; i++)
		{
			if (Equipments[i] == null || Equipments[i].itemDetails == null || Equipments[i].itemDetails.equipment == null)
			{
				flag = false;
				break;
			}
			flag &= string.Compare(Equipments[i].itemDetails.equipment.setID, p_equipmentSet, true) == 0;
		}
		return flag;
	}

	public int GetSkillIndex(SkillEnum.SelfSkill p_skill)
	{
		for (int i = 0; i < SelfSkills.Length; i++)
		{
			if (SelfSkills[i] == p_skill)
			{
				return i;
			}
		}
		return -1;
	}

	private void UpdateTargetXp(int p_currentLevel)
	{
		if (m_isMonster)
		{
			return;
		}
		ParameterManager instance = SingletonComponent<ParameterManager>.Instance;
		if (!instance)
		{
			return;
		}
		NBBinaryFormat parametersFor = instance.GetParametersFor(m_experienceCSV);
		if (parametersFor != null)
		{
			if (p_currentLevel > MaxLevel)
			{
				p_currentLevel = MaxLevel;
			}
			m_targetXp = parametersFor.GetDataGeneric<int>(string.Empty + p_currentLevel, m_colExpNextLevel);
			if (p_currentLevel > 1)
			{
				m_previousTargetXp = parametersFor.GetDataGeneric<int>(string.Empty + (p_currentLevel - 1), m_colExpNextLevel);
			}
			else
			{
				m_previousTargetXp = 0;
			}
		}
	}

	public void UpdateLifeMaxValue()
	{
		float num = 0f;
		float num2 = BaseLife;
		num += num2;
		Equipment.Data[] equips = m_equips;
		foreach (Equipment.Data data in equips)
		{
			num += data.lifeMaxMod;
		}
		MaxHp = num;
	}

	public void UpdateArmor()
	{
		float num = 0f;
		Equipment.Data[] equips = m_equips;
		foreach (Equipment.Data data in equips)
		{
			num += data.armorMod;
		}
		num += m_constitution;
		Def = num;
	}

	public void UpdateDamage()
	{
		float num = 0f;
		float num2 = 0f;
		Equipment.Data[] equips = m_equips;
		foreach (Equipment.Data data in equips)
		{
			num += data.minDamageMod;
			num2 += data.maxDamageMod;
		}
		num += m_strength;
		num2 += m_strength;
		MinAttack = num;
		MaxAttack = num2;
	}

	public void UpdateSkills()
	{
		if (IsMainCharacter)
		{
			UpdatePlayerSkills();
		}
		else
		{
			UpdateFriendSkills();
		}
	}

	public void UpdatePlayerSkills()
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		string text = "--SkillData-- for PLAYER \n";
		List<SkillEnum.SelfSkill> list = new List<SkillEnum.SelfSkill>();
		List<float> list2 = new List<float>();
		Equipment.Data[] equips = m_equips;
		foreach (Equipment.Data data in equips)
		{
			ItemDetails itemDetails = data.itemDetails;
			if (itemDetails != null && itemDetails.skill != null)
			{
				string text2 = text;
				text = string.Concat(text2, itemDetails.Category, "\t-->\t", itemDetails.skill.selfAction, "\n");
				if (itemDetails.skill.selfAction != SkillEnum.SelfSkill.None)
				{
					list.Add(itemDetails.skill.selfAction);
					list2.Add(itemDetails.skill.directValue);
				}
			}
		}
		float num5 = m_lifeMaxValue + m_lifeMaxValue * num / 100f;
		float num6 = m_minAttackValue + m_minAttackValue * num2 / 100f;
		float num7 = m_maxAttackValue + m_maxAttackValue * num2 / 100f;
		float num8 = m_criticalChance + num3;
		float num9 = m_defenseValue + m_defenseValue * num4 / 100f;
		Dbg.Log(text + "Life :\t\t" + m_lifeMaxValue + " to : " + num5 + "\nAttackMin:\t" + m_minAttackValue + " to : " + num6 + "\nAttackMax:\t" + m_maxAttackValue + " to : " + num7 + "\nCritical:\t" + m_criticalChance + " to : " + num8 + "\nDefense:\t" + m_defenseValue + " to : " + num9);
		m_lifeMaxValue = num5;
		m_minAttackValue = num6;
		m_maxAttackValue = num7;
		m_criticalChance = num8;
		m_defenseValue = num9;
		SelfSkills = list.ToArray();
		SelfSkillValues = list2.ToArray();
	}

	public void UpdateFriendSkills()
	{
		string text = "--SkillData-- for FRIEND \n";
		Equipment.Data equipmentData = GetEquipmentData(Equipment.Part.Weapon);
		if (equipmentData == null)
		{
			Dbg.LogError("this Friend doesn't have a valid Weapon.");
			return;
		}
		if (equipmentData.itemDetails != null)
		{
			string text2 = text;
			text = string.Concat(text2, equipmentData.itemDetails.Category, "\t-->\t", equipmentData.itemDetails.skill.assistValue, "\n");
		}
		Dbg.Log(text);
	}

	public void UpdateCriticalChance()
	{
		float num = 0f;
		Equipment.Data[] equips = m_equips;
		foreach (Equipment.Data data in equips)
		{
			num += data.critMod;
		}
		if (ApplyCriticalFormula)
		{
			m_criticalChance = (4f + m_agility) * 0.2f + num;
		}
		else
		{
			m_criticalChance = num;
		}
	}

	public override string ToString()
	{
		string text = string.Empty;
		Equipment.Data[] equipments = Equipments;
		foreach (Equipment.Data data in equipments)
		{
			string text2 = text;
			text = string.Concat(text2, "[", data, "],");
		}
		text = "[" + text.Remove(text.LastIndexOf("],"), 2) + "]";
		return string.Format("[PlayerData: Name={0}, Level={1}, Agi={2}, Con={3}, Str={4}, BaseLife={5}, MaxHp={6}, Hp={7}, MinAttack={8}, MaxAttack={9}, Def={10}, Atk={11}, CriticalChance={12}, CriticalMul={13}, Xp={15}, Equipments={16}, CharacterType:{17}\n\nUserData={18}\n]", Name, Level, Agi, Con, Str, BaseLife, MaxHp, Hp, MinAttack, MaxAttack, Def, Atk, CriticalChance, CriticalMul, 0f, Xp, text, m_characterType, m_userData);
	}
}
