using System.Collections;
using System.Collections.Generic;
using Models;
using UnityEngine;

public class CharacterInfoPanel : NBUIMenuPanel
{
	public InventoryCell inventoryCellPrefab;

	public Transform[] equipmentSlotPositions = new Transform[5];

	public static Vector3 m_playerPosition = new Vector3(99.85f, 0f, 0.5f);

	public static Vector3 m_playerPositionAdditionalOffset = new Vector3(0.24f, 0f, 0f);

	private PlayerData m_playerData;

	private bool m_isMainCharacter = true;

	private Dictionary<string, InventoryCell> m_equipCells;

	private static string[] m_cellOrder = new string[5]
	{
		Equipment.Weapon,
		Equipment.Head,
		Equipment.Shoulder,
		Equipment.Body,
		Equipment.Leg
	};

	private List<string> m_allIdleAnims;

	public static string s_idleAnimation = "Custom_Idle";

	private GameObject m_character;

	private StageManager m_stageMgr;

	private ItemManager m_itemManager;

	public GameObject m_elementRoot;

	public UILabel m_fireValue;

	public UILabel m_waterValue;

	public UILabel m_earthValue;

	public UILabel m_lightningValue;

	public UILabel m_metalValue;

	public UILabel m_healthValue;

	public UILabel m_armourValue;

	public UILabel m_damageValue;

	public UILabel m_criticalValue;

	public UISprite m_spriteSkillAssist;

	public UILabel m_labelSkillAssist;

	public UILabel m_labelSkillAssistDetail;

	public Component eventTarget;

	private GameObject m_petModel;

	[SerializeField]
	private UILabel m_playerRank;

	[SerializeField]
	private UILabel m_playerName;

	[SerializeField]
	private UILabel m_playerLevel;

	private bool m_showRankInfo;

	private bool m_showFriendInfo;

	[SerializeField]
	private GameObject m_equippedReminder;

	private int m_maxSet = 5;

	private int m_currSetIndex;

	private bool m_currSetDirty;

	private List<Dictionary<string, IEquipable>> m_preSets;

	[SerializeField]
	private UIButton m_leftSetButton;

	[SerializeField]
	private UIButton m_rightSetButton;

	[SerializeField]
	private UILabel m_currentSetLabel;

	[SerializeField]
	private NBUILocalize m_setBonusLabel;

	[SerializeField]
	private PetInventoryCell m_petButton;

	[SerializeField]
	private UISprite m_loadingSprite;

	private CharacterSetting3D m_characterSetup;

	private bool _init;

	private bool CheckInputs()
	{
		m_stageMgr = SingletonComponent<StageManager>.Instance;
		if (m_stageMgr == null)
		{
			return false;
		}
		m_itemManager = SingletonComponent<ItemManager>.Instance;
		if (m_itemManager == null)
		{
			return false;
		}
		if (SingletonComponent<PlayerDataManager>.Instance == null)
		{
			return false;
		}
		if (m_isMainCharacter)
		{
			m_playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		}
		if (m_playerData == null)
		{
			return false;
		}
		if (m_elementRoot == null)
		{
			Dbg.LogError("No fire element value given.");
			return false;
		}
		if (m_fireValue == null)
		{
			Dbg.LogError("No fire element value given.");
			return false;
		}
		if (m_waterValue == null)
		{
			Dbg.LogError("No water element value given.");
			return false;
		}
		if (m_earthValue == null)
		{
			Dbg.LogError("No earth element value given.");
			return false;
		}
		if (m_lightningValue == null)
		{
			Dbg.LogError("No lightning element value given.");
			return false;
		}
		if (m_healthValue == null)
		{
			Dbg.LogError("No Health value given.");
			return false;
		}
		if (m_armourValue == null)
		{
			Dbg.LogError("No Armour value given.");
			return false;
		}
		if (m_damageValue == null)
		{
			Dbg.LogError("No Damage value given.");
			return false;
		}
		if (m_criticalValue == null)
		{
			Dbg.LogError("No Critical value given.");
			return false;
		}
		return true;
	}

	private new void Awake()
	{
		if (!CheckInputs())
		{
			base.enabled = false;
		}
	}

	private void Start()
	{
		m_showRankInfo = false;
		m_showFriendInfo = false;
	}

	public override void Reset()
	{
		if (SingletonComponent<InventoryManager>.Instance != null)
		{
			SingletonComponent<InventoryManager>.Instance.FreeTempItems();
		}
		_init = false;
		ShowCharacter(false);
		ShowPet(false);
		m_showRankInfo = false;
		m_showFriendInfo = false;
		StageManager instance = SingletonComponent<StageManager>.Instance;
		if (!m_isMainCharacter)
		{
			if (null != instance)
			{
				instance.NeedUpdatePlayerTemp = true;
			}
		}
		else
		{
			PetManager instance2 = SingletonComponent<PetManager>.Instance;
			InventoryManager instance3 = SingletonComponent<InventoryManager>.Instance;
			if (instance3 != null && null != instance2)
			{
				instance3.SaveEquipmentToServer();
				instance2.SwitchActivePet(m_playerData.details.activePet);
				MBPrefs.SetInt("player.equipment.preset.current", m_currSetIndex);
				MBPrefs.Save();
			}
		}
		StopCoroutine("DoRegenerateCharacter");
	}

	private void Init()
	{
		if (m_playerData == null)
		{
			return;
		}
		if (m_isMainCharacter)
		{
			LoadSets();
		}
		if (null == m_characterSetup)
		{
			m_characterSetup = Object.FindObjectOfType(typeof(CharacterSetting3D)) as CharacterSetting3D;
		}
		SetPlayerInfo(m_playerData.Name, m_playerData.Level, m_playerData.details.leaderboardRank);
		m_showFriendInfo = false;
		if (null == m_character)
		{
			m_character = SingletonComponent<StageManager>.Instance.PlayerTemplate;
		}
		SingletonComponent<StageManager>.Instance.NeedUpdatePlayerTemp = true;
		ShowLoading(true);
		SingletonComponent<StageManager>.Instance.CreatePlayerMainMenuTemplate(SetupCharacter, m_playerData, m_isMainCharacter, false);
		PetDetails activePetDetails = m_playerData.GetActivePetDetails();
		if (activePetDetails != null && string.Empty != activePetDetails.m_petId)
		{
			RegeneratePet();
			ShowPet(true);
		}
		else
		{
			ShowPet(false);
		}
		NBProfiler.PrintUsedHeap("[CHARACTER_INFO_PANEL_START]");
		if (MenuBackground3D.Instance != null)
		{
			if (MenuBackground3D.Instance.CurrentBackground != null)
			{
				if (!MenuBackground3D.Instance.CurrentBackground.gameObject.active)
				{
					MenuBackground3D.Instance.ShowDefaultBackGround();
				}
			}
			else
			{
				MenuBackground3D.Instance.ShowDefaultBackGround();
			}
		}
		eventTarget = parentScreen;
		if (m_elementRoot != null)
		{
			m_elementRoot.SetActiveRecursively(true);
		}
		Dbg.Assert(m_fireValue);
		Dbg.Assert(m_waterValue);
		Dbg.Assert(m_earthValue);
		Dbg.Assert(m_lightningValue);
		Dbg.Assert(m_healthValue);
		Dbg.Assert(m_armourValue);
		Dbg.Assert(m_damageValue);
		Dbg.Assert(m_criticalValue);
		Dbg.Assert(m_spriteSkillAssist);
		Dbg.Assert(m_labelSkillAssist);
		Dbg.Assert(m_labelSkillAssistDetail);
		m_currSetIndex = MBPrefs.GetInt("player.equipment.preset.current", 0);
		m_currSetDirty = true;
		SetCurrentSetLabel(m_currSetIndex);
		if (!m_isMainCharacter)
		{
			ShowSetButtons(false);
		}
		_init = true;
	}

	public void SetData(PlayerData _playerData = null)
	{
		bool flag = _playerData == null;
		if (!flag || !m_isMainCharacter)
		{
			ResetCharacterModel();
		}
		m_isMainCharacter = flag;
		if (m_isMainCharacter)
		{
			m_playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		}
		else
		{
			m_playerData = _playerData;
		}
	}

	private void ShowSetButtons(bool p_show)
	{
		if (null != m_currentSetLabel && null != m_currentSetLabel.transform.parent)
		{
			m_currentSetLabel.transform.parent.gameObject.SetActiveRecursively(p_show);
		}
	}

	protected void SetPlayerInfo(string p_name, int p_level, int p_rank = -1)
	{
		SetPlayerName(p_name);
		SetPlayerLevel(p_level);
		SetPlayerRank(p_rank);
		if (null != m_equippedReminder)
		{
			m_equippedReminder.SetActiveRecursively(false);
		}
		m_showFriendInfo = true;
	}

	private void SetPlayerName(string p_name)
	{
		if (p_name != null)
		{
			m_playerName.text = p_name;
		}
	}

	private void SetPlayerLevel(int p_level)
	{
		if (null != m_playerLevel)
		{
			NBLocalization instance = SingletonComponent<NBLocalization>.Instance;
			string text = "{0}";
			if (null != instance)
			{
				text = instance.Get("common_level") + " " + text;
			}
			string text2 = string.Format(text, p_level);
			m_playerLevel.text = text2;
		}
	}

	private void ShowPlayerRank(bool p_show)
	{
		if (null != m_playerRank)
		{
			m_playerRank.gameObject.SetActiveRecursively(p_show);
		}
	}

	private void SetPlayerRank(int p_rank)
	{
		if (null != m_playerRank)
		{
			NBLocalization instance = SingletonComponent<NBLocalization>.Instance;
			string text = "{0}";
			if (null != instance)
			{
				text = instance.Get("leaderboard_rank_label") + " #" + text;
			}
			string arg = ((p_rank <= 0) ? "-" : p_rank.ToString());
			string text2 = string.Format(text, arg);
			m_playerRank.text = text2;
		}
	}

	private void EnableRankInfoUi(bool p_enable)
	{
		m_playerRank.gameObject.SetActiveRecursively(p_enable);
		m_playerLevel.gameObject.SetActiveRecursively(p_enable);
		m_playerName.gameObject.SetActiveRecursively(p_enable);
		m_showRankInfo = p_enable;
	}

	private void OnEnable()
	{
		Init();
	}

	private void OnDisable()
	{
		Reset();
	}

	private void OnDestroy()
	{
		if (!m_isMainCharacter)
		{
			DestroyCurrentCharacterGo();
		}
	}

	public void DestroyCurrentCharacterGo()
	{
		m_character = null;
	}

	private void PlayRandomIdle()
	{
		if (m_character == null)
		{
			return;
		}
		m_allIdleAnims = m_character.GetComponent<Animation>().FindAnimationWithPrefix("idle");
		if (m_allIdleAnims.Count >= 1)
		{
			s_idleAnimation = m_allIdleAnims[Random.Range(0, m_allIdleAnims.Count - 1)];
			if (s_idleAnimation == "Idle_02")
			{
				s_idleAnimation = "Idle_Custom";
			}
			m_character.GetComponent<Animation>().CrossFade(s_idleAnimation, 0f);
			m_character.GetComponent<Animation>()[s_idleAnimation].wrapMode = WrapMode.Loop;
			Dbg.Log(" Playing anim : " + s_idleAnimation);
		}
	}

	private void SetupCharacter()
	{
		m_character = SingletonComponent<StageManager>.Instance.PlayerTemplate;
		GameObject playerTemplate = SingletonComponent<StageManager>.Instance.PlayerTemplate;
		if (!(playerTemplate == null))
		{
			if (m_isMainCharacter)
			{
				m_playerData.ComputeFinalValues();
			}
			if (parentScreen.currentPanel == this)
			{
				ShowCharacter(true);
				SetCharacterPosition();
				CreateOrUpdateEquipCells(true);
			}
			ShowLoading(false);
		}
	}

	private void ShowCharacter(bool _show)
	{
		if (!(m_character == null))
		{
			m_character.SetActiveRecursively(_show);
			m_character.GetComponent<Animation>().CrossFade(s_idleAnimation, 0f);
			m_character.GetComponent<Animation>()[s_idleAnimation].wrapMode = WrapMode.Loop;
		}
	}

	private void ShowPet(bool p_show)
	{
		if (null != m_petModel)
		{
			m_petModel.SetActiveRecursively(p_show);
		}
		if (null != m_petButton)
		{
			m_petButton.gameObject.SetActiveRecursively(p_show);
		}
	}

	private void SetCharacterPosition()
	{
		if (null != m_characterSetup)
		{
			m_characterSetup.AttachObject(m_character, "MainMenuPlayerStatsRoot");
			m_characterSetup.AttachCamera("CharacterScreenCameraRoot");
		}
	}

	private void CreateOrUpdateEquipCells(bool p_enableCells = false)
	{
		if (m_equipCells != null)
		{
			UpdateEquipCells(p_enableCells);
			return;
		}
		int num = 0;
		m_equipCells = new Dictionary<string, InventoryCell>();
		string[] cellOrder = m_cellOrder;
		foreach (string text in cellOrder)
		{
			if (!(text == Equipment.Belt))
			{
				InventoryCell inventoryCell = (InventoryCell)Object.Instantiate(inventoryCellPrefab);
				inventoryCell.name = equipmentSlotPositions[num].name;
				inventoryCell.transform.parent = equipmentSlotPositions[num].parent;
				inventoryCell.transform.localPosition = equipmentSlotPositions[num].localPosition;
				inventoryCell.transform.localScale = equipmentSlotPositions[num].localScale;
				inventoryCell.SetLayerRecursively(base.gameObject.layer);
				inventoryCell.m_size = InventoryCell.Size.Medium;
				equipmentSlotPositions[num].gameObject.SetActiveRecursively(false);
				UIButtonMessage orAddComponent = NBComponentHelper.GetOrAddComponent<UIButtonMessage>(inventoryCell.gameObject);
				orAddComponent.id = num;
				orAddComponent.functionName = "OnButtonClick";
				orAddComponent.target = base.gameObject;
				if (!m_equipCells.ContainsKey(text))
				{
					m_equipCells.Add(text, inventoryCell);
				}
				num++;
			}
		}
		UpdateEquipCells(p_enableCells);
	}

	private void UpdateEquipCells(bool p_enableCells = false)
	{
		Dbg.LogWarn("Equip cells = " + m_equipCells.Count);
		foreach (string key in m_equipCells.Keys)
		{
			InventoryCell inventoryCell = m_equipCells[key];
			Equipment.Data equipmentData = m_playerData.GetEquipmentData(Equipment.FromString(key));
			if (equipmentData.itemDetails != null)
			{
				if (p_enableCells && !inventoryCell.gameObject.active)
				{
					inventoryCell.gameObject.active = true;
				}
				inventoryCell.data = equipmentData.itemDetails;
				inventoryCell.Selected = false;
			}
		}
		PetDetails activePetDetails = m_playerData.GetActivePetDetails();
		SetPetButton(activePetDetails);
		Equipment.Data equipmentData2 = m_playerData.GetEquipmentData(Equipment.Part.Weapon);
		if (equipmentData2 != null)
		{
			string setID = equipmentData2.itemDetails.equipment.setID;
			string bonusName = "common_none";
			if (m_playerData.IsEquipingFullSet(setID))
			{
				ItemManager instance = SingletonComponent<ItemManager>.Instance;
				if (null != instance)
				{
					ArmorSet armorSet = instance.GetArmorSet(setID, equipmentData2.itemDetails.equipment.isSpecial);
					if (armorSet != null)
					{
						bonusName = armorSet.bonusName;
					}
				}
			}
			SetBonusName(bonusName);
		}
		UpdatePlayerStats();
	}

	private void SetBonusName(string p_textKey)
	{
		if (null != m_setBonusLabel)
		{
			m_setBonusLabel.Key = p_textKey;
		}
	}

	private void UpdatePlayerStats()
	{
		if (m_playerData == null)
		{
			return;
		}
		m_healthValue.text = Mathf.CeilToInt(m_playerData.MaxHp).ToString();
		m_armourValue.text = Mathf.CeilToInt(m_playerData.Def) + string.Empty;
		m_damageValue.text = Mathf.CeilToInt(m_playerData.MinAttack) + " - " + Mathf.CeilToInt(m_playerData.MaxAttack);
		m_criticalValue.text = string.Format("{0:0.00}%", m_playerData.CriticalChance);
		m_fireValue.text = Mathf.CeilToInt(m_playerData.GetResistanceAgainst(Element.Type.Fire)).ToString();
		m_waterValue.text = Mathf.CeilToInt(m_playerData.GetResistanceAgainst(Element.Type.Water)).ToString();
		m_earthValue.text = Mathf.CeilToInt(m_playerData.GetResistanceAgainst(Element.Type.Earth)).ToString();
		m_lightningValue.text = Mathf.CeilToInt(m_playerData.GetResistanceAgainst(Element.Type.Lightning)).ToString();
		m_metalValue.text = Mathf.CeilToInt(m_playerData.GetResistanceAgainst(Element.Type.Metal)).ToString();
		ItemDetails itemDetails = m_playerData.Equipments[4].itemDetails;
		if (itemDetails != null && itemDetails != ItemManager.s_NULL_DATA && !itemDetails.IsBaseItem)
		{
			if ((bool)m_labelSkillAssist)
			{
				m_labelSkillAssist.enabled = true;
			}
			ItemSkill skill = itemDetails.skill;
			if ((bool)m_spriteSkillAssist && skill.visual.sprite_assist != string.Empty)
			{
				m_spriteSkillAssist.enabled = true;
			}
			else
			{
				m_spriteSkillAssist.enabled = false;
			}
			m_spriteSkillAssist.spriteName = skill.visual.sprite_assist;
			m_labelSkillAssist.text = SingletonComponent<NBLocalization>.Instance.Get(skill.visual.label_assist);
			string text = SingletonComponent<NBLocalization>.Instance.Get(skill.visual.description_assist);
			if (text.Contains("{0"))
			{
				m_labelSkillAssistDetail.text = string.Format(text, skill.assistValue);
			}
			else
			{
				m_labelSkillAssistDetail.text = text;
			}
		}
		else
		{
			if ((bool)m_spriteSkillAssist)
			{
				m_spriteSkillAssist.enabled = false;
			}
			if ((bool)m_labelSkillAssist)
			{
				m_labelSkillAssist.enabled = false;
			}
			if ((bool)m_labelSkillAssistDetail)
			{
				m_labelSkillAssistDetail.text = SingletonComponent<NBLocalization>.Instance.Get("gacha_item_no_skill_label");
			}
		}
	}

	private void SaveCurrentSet()
	{
		if (m_playerData == null || !m_currSetDirty || !m_isMainCharacter)
		{
			return;
		}
		Dictionary<string, IEquipable> dictionary = m_preSets[m_currSetIndex];
		foreach (KeyValuePair<string, InventoryCell> equipCell in m_equipCells)
		{
			string key = equipCell.Key;
			InventoryCell value = equipCell.Value;
			MBPrefs.SetString("player.equipment.preset." + m_currSetIndex + "." + key, value.data.DB_ID);
			dictionary[key] = value.data;
		}
		PetDetails activePetDetails = m_playerData.GetActivePetDetails();
		string value2 = string.Empty;
		if (activePetDetails != null)
		{
			value2 = activePetDetails.m_petId;
		}
		MBPrefs.SetString("player.equipment.preset." + m_currSetIndex + ".pet", value2);
		dictionary["pet"] = activePetDetails;
		m_currSetDirty = false;
	}

	private void ClearSets()
	{
		if (m_preSets == null)
		{
			return;
		}
		foreach (Dictionary<string, IEquipable> preSet in m_preSets)
		{
			((IDictionary)preSet).Clear();
		}
		m_preSets.Clear();
	}

	private void LoadSets()
	{
		InventoryManager instance = SingletonComponent<InventoryManager>.Instance;
		if (null == instance)
		{
			return;
		}
		if (m_preSets == null)
		{
			m_preSets = new List<Dictionary<string, IEquipable>>();
		}
		if (m_preSets == null)
		{
			return;
		}
		ClearSets();
		for (int i = 0; i < m_maxSet; i++)
		{
			Dictionary<string, IEquipable> dictionary = new Dictionary<string, IEquipable>();
			m_preSets.Add(dictionary);
			string[] cellOrder = m_cellOrder;
			foreach (string text in cellOrder)
			{
				string text2 = MBPrefs.GetString("player.equipment.preset." + i + "." + text, string.Empty);
				if (!dictionary.ContainsKey(text))
				{
					ItemDetails value = null;
					if (string.Empty != text2)
					{
						value = instance.GetByDBID(text2);
					}
					dictionary.Add(text, value);
				}
			}
			string p_petID = MBPrefs.GetString("player.equipment.preset." + i + ".pet", string.Empty);
			if (!dictionary.ContainsKey("pet"))
			{
				PetDetails petDetails = m_playerData.GetPetDetails(p_petID);
				dictionary.Add("pet", petDetails);
			}
		}
	}

	private void SetCurrentSetLabel(int p_index)
	{
		if (null != m_currentSetLabel)
		{
			NBLocalization instance = SingletonComponent<NBLocalization>.Instance;
			string text = "{0}";
			if (null != instance)
			{
				text = instance.Get("equipment_set_label") + " " + text;
			}
			string text2 = string.Format(text, p_index + 1);
			m_currentSetLabel.text = text2;
		}
	}

	private void ToggleSet(bool p_next = true)
	{
		TutorialManager instance = SingletonComponent<TutorialManager>.Instance;
		if (null != instance && instance.enabled)
		{
			UIPopupController.Instance.ShowOKPopupMSG("common_info", "tutorial_characterstats_change_set_warning");
			return;
		}
		SaveCurrentSet();
		if (p_next)
		{
			m_currSetIndex = (m_currSetIndex + 1) % m_maxSet;
		}
		else
		{
			m_currSetIndex = (m_currSetIndex - 1 + m_maxSet) % m_maxSet;
		}
		Dbg.Log("Current set = " + m_currSetIndex, Dbg.NBLogType.CHARACTER_INFO_PANEL);
		SetCurrentSetLabel(m_currSetIndex);
		if (m_currSetIndex < 0 || m_currSetIndex >= m_preSets.Count)
		{
			return;
		}
		bool flag = false;
		Dictionary<string, IEquipable> dictionary = m_preSets[m_currSetIndex];
		foreach (KeyValuePair<string, IEquipable> item in dictionary)
		{
			Equipment.Part p_part = Equipment.FromString(item.Key);
			IEquipable value = item.Value;
			bool flag2 = item.Key == "pet";
			if (value == null)
			{
				if ((!flag2 && m_playerData.IsEquippingBase(p_part)) || (flag2 && m_playerData.GetActivePetDetails() == null))
				{
					continue;
				}
			}
			else if (value.IsEquipped(m_playerData))
			{
				continue;
			}
			if (value == null)
			{
				if (!flag2)
				{
					InventoryManager instance2 = SingletonComponent<InventoryManager>.Instance;
					if (null != instance2)
					{
						instance2.UnequipItemFromPlayer(p_part);
					}
					flag = true;
					continue;
				}
				PetDetails firstPetDetails = m_playerData.GetFirstPetDetails();
				if (firstPetDetails != null)
				{
					m_playerData.Equip(firstPetDetails);
					RegeneratePet();
					ShowPet(true);
					m_currSetDirty = true;
				}
				else
				{
					ShowPet(false);
				}
			}
			else
			{
				value.Equip(m_playerData);
				if (!flag2)
				{
					flag = true;
					continue;
				}
				RegeneratePet();
				ShowPet(true);
			}
		}
		UpdateEquipCells();
		if (flag)
		{
			RegenerateCharacter();
		}
	}

	private void ShowLoading(bool p_show)
	{
		if (null != m_loadingSprite)
		{
			m_loadingSprite.gameObject.SetActiveRecursively(p_show);
		}
	}

	private void OnButtonClick(GameObject p_sender)
	{
		if (null == p_sender || null == eventTarget)
		{
			return;
		}
		if (null != m_leftSetButton && m_leftSetButton.gameObject == p_sender)
		{
			ToggleSet(false);
			return;
		}
		if (null != m_rightSetButton && m_rightSetButton.gameObject == p_sender)
		{
			ToggleSet();
			return;
		}
		if (null != m_petButton && m_petButton.gameObject == p_sender)
		{
			SwitchPet();
			return;
		}
		UIButtonMessage component = p_sender.GetComponent<UIButtonMessage>();
		EquipmentCellEvent equipmentCellEvent = default(EquipmentCellEvent);
		switch (component.id)
		{
		case 0:
			equipmentCellEvent.partType = Equipment.Part.Weapon;
			break;
		case 1:
			equipmentCellEvent.partType = Equipment.Part.Helm;
			break;
		case 2:
			equipmentCellEvent.partType = Equipment.Part.Shoulder;
			break;
		case 3:
			equipmentCellEvent.partType = Equipment.Part.Chest;
			break;
		case 4:
			equipmentCellEvent.partType = Equipment.Part.Pants;
			break;
		default:
			equipmentCellEvent.partType = Equipment.Part.Weapon;
			break;
		}
		if (m_isMainCharacter)
		{
			eventTarget.SendMessage("OnEquipmentCellEvent", equipmentCellEvent, SendMessageOptions.DontRequireReceiver);
		}
		else
		{
			InventoryCell component2 = p_sender.GetComponent<InventoryCell>();
			UIPopupController.Instance.ShowItemStats(component2.data);
		}
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_notification");
	}

	private void SwitchPet()
	{
		if (m_playerData == null)
		{
			return;
		}
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_secondary_ui_tap");
		if (m_playerData.details == null || m_playerData.details.unlockedPets == null)
		{
			return;
		}
		IDictionary unlockedPets = m_playerData.details.unlockedPets;
		if (unlockedPets.Count <= 1)
		{
			return;
		}
		IDictionaryEnumerator enumerator = unlockedPets.GetEnumerator();
		string text = string.Empty;
		string text2 = string.Empty;
		string activePet = m_playerData.details.activePet;
		while (enumerator.MoveNext())
		{
			string text3 = enumerator.Key as string;
			if (text3 != null && string.IsNullOrEmpty(text))
			{
				text = text3;
			}
			if (text3 == activePet)
			{
				text2 = text;
				if (enumerator.MoveNext())
				{
					text3 = enumerator.Key as string;
					text2 = text3;
				}
				break;
			}
		}
		if (!string.IsNullOrEmpty(text2))
		{
			PetDetails petDetails = m_playerData.GetPetDetails(text2);
			m_playerData.Equip(petDetails);
			SetPetButton(petDetails);
			RegeneratePet();
			m_currSetDirty = true;
		}
	}

	private void SetPetButton(PetDetails p_pet)
	{
		if (!(null != m_petButton) || p_pet == null)
		{
			return;
		}
		if (p_pet.PetIconPath == null || p_pet.PetFrameName == null)
		{
			PetSetup instance = Singleton<PetSetup>.Instance;
			if (instance != null)
			{
				instance.GetExtraInfo(ref p_pet);
			}
		}
		m_petButton.data = p_pet;
	}

	private void OnItemSelectEvent(ItemSelectEvent evt)
	{
		if (evt == null)
		{
			return;
		}
		if (!eventTarget)
		{
			eventTarget = parentScreen;
		}
		if (evt.eventType == ItemSelectEventType.ITEM_EQUIP)
		{
			if (evt.selectedItems == null || evt.selectedItems.Count <= 0)
			{
				return;
			}
			InventoryCell inventoryCell = evt.selectedItems[0];
			InventoryManager instance = SingletonComponent<InventoryManager>.Instance;
			if (null != instance && null != inventoryCell && m_playerData != null)
			{
				ItemDetails data = inventoryCell.data;
				m_playerData.Equip(data);
				InventoryPanel inventory = MainMenu.Instance.inventory;
				if (null != inventory)
				{
					inventory.SelectItem(data);
					inventory.RefreshVisibleCells();
				}
				m_currSetDirty = true;
			}
		}
		else if (evt.eventType == ItemSelectEventType.ITEM_CANCEL)
		{
			if (m_currSetDirty)
			{
				RegenerateCharacter();
			}
			eventTarget.SendMessage("OnItemSelectEvent", evt, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void ResetCharacterModel()
	{
		DestroyCurrentCharacterGo();
	}

	public void RegenerateCharacter()
	{
		ShowLoading(true);
		StopCoroutine("DoRegenerateCharacter");
		StartCoroutine("DoRegenerateCharacter");
	}

	public IEnumerator DoRegenerateCharacter()
	{
		StageManager sm = SingletonComponent<StageManager>.Instance;
		if (!(null == sm))
		{
			if (sm.IsGenerating)
			{
				sm.StopCharacterGeneration();
			}
			while (sm.IsGenerating)
			{
				yield return null;
			}
			SingletonComponent<StageManager>.Instance.NeedUpdatePlayerTemp = true;
			SingletonComponent<StageManager>.Instance.CreatePlayerMainMenuTemplate(SetupCharacter, false);
		}
	}

	public void RegeneratePet()
	{
		PetManager instance = SingletonComponent<PetManager>.Instance;
		if (!(null == m_characterSetup) && !(null == instance) && m_playerData != null)
		{
			m_petModel = instance.CreateMainMenuPet(m_playerData);
			m_characterSetup.AttachObject(m_petModel, "CharacterScreenPetRoot");
		}
	}
}
