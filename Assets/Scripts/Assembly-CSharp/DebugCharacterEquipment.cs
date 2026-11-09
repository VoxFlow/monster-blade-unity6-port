using System.Collections.Generic;
using UnityEngine;

public class DebugCharacterEquipment : MonoBehaviour
{
	private class ItemIdTierSet
	{
		public string ItemId = string.Empty;

		public int ItemTier = 1;

		public ItemIdTierSet()
		{
		}

		public ItemIdTierSet(string id, int tier)
		{
			ItemId = id;
			ItemTier = tier;
		}
	}

	private class SetEquipments
	{
		public ItemIdTierSet weaponData;

		public ItemIdTierSet helmData;

		public ItemIdTierSet chestData;

		public ItemIdTierSet shoulderData;

		public ItemIdTierSet leggingData;
	}

	private bool[] itemChecked = new bool[5];

	private string[] curEquippedItemIds = new string[5]
	{
		string.Empty,
		string.Empty,
		string.Empty,
		string.Empty,
		string.Empty
	};

	private string setName = "test";

	private PlayerData m_playerData;

	public Vector3 m_playerPosition = new Vector3(99.85f, 0f, 0.5f);

	private GameObject m_character;

	private CharacterInfoPanel m_charaInfoPanelRef;

	private List<ItemIdTierSet> weapons = new List<ItemIdTierSet>();

	private List<ItemIdTierSet> chests = new List<ItemIdTierSet>();

	private List<ItemIdTierSet> helms = new List<ItemIdTierSet>();

	private List<ItemIdTierSet> shoulders = new List<ItemIdTierSet>();

	private List<ItemIdTierSet> leggings = new List<ItemIdTierSet>();

	private Dictionary<string, SetEquipments> sets = new Dictionary<string, SetEquipments>();

	private List<string> setsIndex = new List<string>();

	private int weaponsIndexer;

	private int chestsIndexer;

	private int helmsIndexer;

	private int shouldersIndexer;

	private int leggingsIndexer;

	private int setIndexer;

	private int weaponNum = 1;

	private int chestNum = 1;

	private int helmNum = 1;

	private int shoulderNum = 1;

	private int leggingNum = 1;

	private int setNum = 1;

	private ItemDetails tempHelm;

	private ItemDetails tempShoulder;

	private ItemDetails tempChest;

	private ItemDetails tempLegging;

	private ItemDetails tempWeapon;

	public void Init()
	{
		m_charaInfoPanelRef = GetComponent<CharacterInfoPanel>();
		NBBinaryFormat equipItemCsv = SingletonComponent<ItemManager>.Instance.GetEquipItemCsv();
		int num = 0;
		foreach (string rowsName in equipItemCsv.RowsNames)
		{
			string data = equipItemCsv.GetData(num, 1);
			int p_outResult = 1;
			NBGeneric.TryParse<int>(equipItemCsv.GetData(num, 0), out p_outResult);
			if (rowsName.Contains("WEAPON"))
			{
				ItemIdTierSet itemIdTierSet = new ItemIdTierSet(rowsName, p_outResult);
				weapons.Add(itemIdTierSet);
				if (!string.IsNullOrEmpty(data))
				{
					if (sets.ContainsKey(data))
					{
						if (sets[data].weaponData == null)
						{
							sets[data].weaponData = itemIdTierSet;
						}
					}
					else
					{
						SetEquipments setEquipments = new SetEquipments();
						setEquipments.weaponData = itemIdTierSet;
						sets.Add(data, setEquipments);
						setsIndex.Add(data);
					}
				}
			}
			else if (rowsName.Contains("ARMOUR_HELM"))
			{
				ItemIdTierSet itemIdTierSet2 = new ItemIdTierSet(rowsName, p_outResult);
				helms.Add(itemIdTierSet2);
				if (!string.IsNullOrEmpty(data))
				{
					if (sets.ContainsKey(data))
					{
						if (sets[data].helmData == null)
						{
							sets[data].helmData = itemIdTierSet2;
						}
					}
					else
					{
						SetEquipments setEquipments2 = new SetEquipments();
						setEquipments2.helmData = itemIdTierSet2;
						sets.Add(data, setEquipments2);
						setsIndex.Add(data);
					}
				}
			}
			else if (rowsName.Contains("ARMOUR_SHOULDERS"))
			{
				ItemIdTierSet itemIdTierSet3 = new ItemIdTierSet(rowsName, p_outResult);
				shoulders.Add(itemIdTierSet3);
				if (!string.IsNullOrEmpty(data))
				{
					if (sets.ContainsKey(data))
					{
						if (sets[data].shoulderData == null)
						{
							sets[data].shoulderData = itemIdTierSet3;
						}
					}
					else
					{
						SetEquipments setEquipments3 = new SetEquipments();
						setEquipments3.shoulderData = itemIdTierSet3;
						sets.Add(data, setEquipments3);
						setsIndex.Add(data);
					}
				}
			}
			else if (rowsName.Contains("ARMOUR_CHEST"))
			{
				ItemIdTierSet itemIdTierSet4 = new ItemIdTierSet(rowsName, p_outResult);
				chests.Add(itemIdTierSet4);
				if (!string.IsNullOrEmpty(data))
				{
					if (sets.ContainsKey(data))
					{
						if (sets[data].chestData == null)
						{
							sets[data].chestData = itemIdTierSet4;
						}
					}
					else
					{
						SetEquipments setEquipments4 = new SetEquipments();
						setEquipments4.chestData = itemIdTierSet4;
						sets.Add(data, setEquipments4);
						setsIndex.Add(data);
					}
				}
			}
			else if (rowsName.Contains("ARMOUR_LEGGINGS"))
			{
				ItemIdTierSet itemIdTierSet5 = new ItemIdTierSet(rowsName, p_outResult);
				leggings.Add(itemIdTierSet5);
				if (!string.IsNullOrEmpty(data))
				{
					if (sets.ContainsKey(data))
					{
						if (sets[data].leggingData == null)
						{
							sets[data].leggingData = itemIdTierSet5;
						}
					}
					else
					{
						SetEquipments setEquipments5 = new SetEquipments();
						setEquipments5.leggingData = itemIdTierSet5;
						sets.Add(data, setEquipments5);
						setsIndex.Add(data);
					}
				}
			}
			num++;
		}
		weaponNum = weapons.Count;
		chestNum = chests.Count;
		helmNum = helms.Count;
		shoulderNum = shoulders.Count;
		leggingNum = leggings.Count;
		setNum = sets.Count;
	}

	private void Awake()
	{
	}

	private void OnDisable()
	{
		weapons.Clear();
		chests.Clear();
		helms.Clear();
		shoulders.Clear();
		leggings.Clear();
		sets.Clear();
		setsIndex.Clear();
		DestroyCurrentCharacterGo();
		weaponsIndexer = 0;
		chestsIndexer = 0;
		helmsIndexer = 0;
		shouldersIndexer = 0;
		leggingsIndexer = 0;
		setIndexer = 0;
		weaponNum = 1;
		chestNum = 1;
		helmNum = 1;
		shoulderNum = 1;
		leggingNum = 1;
		setNum = 1;
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void OnGUI()
	{
		GUI.backgroundColor = Color.yellow;
		GUI.contentColor = Color.white;
		ShowSetName();
		ShowPrevNextSetBtns();
		ShowPrevNextItemBtns();
		ShowCheckboxOnItems();
	}

	private void ShowSetName()
	{
		GUI.Label(new Rect(225f, 60f, 100f, 32f), setName);
	}

	private void ShowPrevNextSetBtns()
	{
		if (GUI.Button(new Rect(140f, 40f, 80f, 50f), "Prev Set"))
		{
			m_charaInfoPanelRef.DestroyCurrentCharacterGo();
			DestroyCurrentCharacterGo();
			m_playerData = SingletonComponent<PlayerDataManager>.Instance.GetTemporaryPlayerData();
			ReGenerateCharaSet(true);
			UpdateItemNames();
			Singleton<CharacterGenerator>.Instance.GenerateMainMenuCharacter(SetupCharacter, m_playerData, true);
		}
		if (GUI.Button(new Rect(280f, 40f, 80f, 50f), "Next Set"))
		{
			m_charaInfoPanelRef.DestroyCurrentCharacterGo();
			DestroyCurrentCharacterGo();
			m_playerData = SingletonComponent<PlayerDataManager>.Instance.GetTemporaryPlayerData();
			ReGenerateCharaSet(false);
			UpdateItemNames();
			Singleton<CharacterGenerator>.Instance.GenerateMainMenuCharacter(SetupCharacter, m_playerData, true);
		}
	}

	private void ShowPrevNextItemBtns()
	{
		if (GUI.Button(new Rect(60f, Screen.height - 140, 100f, 50f), "Prev Item"))
		{
			m_charaInfoPanelRef.DestroyCurrentCharacterGo();
			DestroyCurrentCharacterGo();
			m_playerData = SingletonComponent<PlayerDataManager>.Instance.GetTemporaryPlayerData();
			ReGenerateChara(true);
			UpdateItemNames();
			Singleton<CharacterGenerator>.Instance.GenerateMainMenuCharacter(SetupCharacter, m_playerData, true);
		}
		if (GUI.Button(new Rect(180f, Screen.height - 140, 100f, 50f), "Next Item"))
		{
			m_charaInfoPanelRef.DestroyCurrentCharacterGo();
			DestroyCurrentCharacterGo();
			m_playerData = SingletonComponent<PlayerDataManager>.Instance.GetTemporaryPlayerData();
			ReGenerateChara(false);
			UpdateItemNames();
			Singleton<CharacterGenerator>.Instance.GenerateMainMenuCharacter(SetupCharacter, m_playerData, true);
		}
	}

	private void ReGenerateChara(bool p_isPrev)
	{
		if (itemChecked[0])
		{
			tempWeapon = PrepareWeapon(p_isPrev);
		}
		if (itemChecked[1])
		{
			tempHelm = PrepareHelm(p_isPrev);
		}
		if (itemChecked[2])
		{
			tempShoulder = PrepareShoulder(p_isPrev);
		}
		if (itemChecked[3])
		{
			tempChest = PrepareChest(p_isPrev);
		}
		if (itemChecked[4])
		{
			tempLegging = PrepareLegging(p_isPrev);
		}
		if (tempHelm != null)
		{
			m_playerData.Equipments[0].itemDetails = tempHelm;
			m_playerData.Equipments[0].element = tempHelm.Element;
		}
		if (tempShoulder != null)
		{
			m_playerData.Equipments[1].itemDetails = tempShoulder;
			m_playerData.Equipments[1].element = tempShoulder.Element;
		}
		if (tempChest != null)
		{
			m_playerData.Equipments[2].itemDetails = tempChest;
			m_playerData.Equipments[2].element = tempChest.Element;
		}
		if (tempLegging != null)
		{
			m_playerData.Equipments[3].itemDetails = tempLegging;
			m_playerData.Equipments[3].element = tempLegging.Element;
		}
		if (tempWeapon != null)
		{
			m_playerData.Equipments[4].itemDetails = tempWeapon;
			m_playerData.Equipments[4].element = tempWeapon.Element;
		}
	}

	private void ReGenerateCharaSet(bool p_IsPrev)
	{
		if (p_IsPrev)
		{
			setIndexer--;
			if (setIndexer < 0)
			{
				setIndexer = setNum - 1;
			}
		}
		else
		{
			setIndexer++;
			if (setIndexer >= setNum)
			{
				setIndexer = 0;
			}
		}
		setName = setsIndex[setIndexer];
		Debug.Log("Set name : " + setName);
		SetEquipments setEquipments = sets[setName];
		if (setEquipments != null)
		{
			if (setEquipments.helmData != null)
			{
				Debug.Log("Try to create helm : " + setEquipments.helmData.ItemId);
				ItemDetails p_itemDetails = SingletonComponent<InventoryManager>.Instance.CreateItem(setEquipments.helmData.ItemId, setEquipments.helmData.ItemTier);
				SetItemDataToAffectPlayerStatus(0, p_itemDetails);
			}
			if (setEquipments.shoulderData != null)
			{
				Debug.Log("Try to create shoulder : " + setEquipments.shoulderData.ItemId);
				ItemDetails p_itemDetails2 = SingletonComponent<InventoryManager>.Instance.CreateItem(setEquipments.shoulderData.ItemId, setEquipments.shoulderData.ItemTier);
				SetItemDataToAffectPlayerStatus(1, p_itemDetails2);
			}
			if (setEquipments.chestData != null)
			{
				Debug.Log("Try to create chest : " + setEquipments.chestData.ItemId);
				ItemDetails p_itemDetails3 = SingletonComponent<InventoryManager>.Instance.CreateItem(setEquipments.chestData.ItemId, setEquipments.chestData.ItemTier);
				SetItemDataToAffectPlayerStatus(2, p_itemDetails3);
			}
			if (setEquipments.leggingData != null)
			{
				Debug.Log("Try to create legging : " + setEquipments.leggingData.ItemId);
				ItemDetails p_itemDetails4 = SingletonComponent<InventoryManager>.Instance.CreateItem(setEquipments.leggingData.ItemId, setEquipments.leggingData.ItemTier);
				SetItemDataToAffectPlayerStatus(3, p_itemDetails4);
			}
			if (setEquipments.weaponData != null)
			{
				Debug.Log("Try to create weapon : " + setEquipments.weaponData.ItemId);
				ItemDetails p_itemDetails5 = SingletonComponent<InventoryManager>.Instance.CreateItem(setEquipments.weaponData.ItemId, setEquipments.weaponData.ItemTier);
				SetItemDataToAffectPlayerStatus(4, p_itemDetails5);
			}
		}
	}

	private void SetItemDataToAffectPlayerStatus(int p_equipmentIndex, ItemDetails p_itemDetails)
	{
		m_playerData.Equipments[p_equipmentIndex].itemDetails = p_itemDetails;
		m_playerData.Equipments[p_equipmentIndex].lifeMaxMod = p_itemDetails.itemTypeWithSkill.hp;
		m_playerData.Equipments[p_equipmentIndex].minDamageMod = p_itemDetails.itemTypeWithSkill.minDamage;
		m_playerData.Equipments[p_equipmentIndex].maxDamageMod = p_itemDetails.itemTypeWithSkill.maxDamage;
		m_playerData.Equipments[p_equipmentIndex].armorMod = p_itemDetails.itemTypeWithSkill.armour;
		m_playerData.Equipments[p_equipmentIndex].critMod = p_itemDetails.itemTypeWithSkill.critical;
		m_playerData.Equipments[p_equipmentIndex].element = p_itemDetails.Element;
	}

	private ItemDetails PrepareChest(bool p_isPrev)
	{
		if (p_isPrev)
		{
			chestsIndexer--;
			if (chestsIndexer < 0)
			{
				chestsIndexer = chestNum - 1;
			}
		}
		else
		{
			chestsIndexer++;
			if (chestsIndexer >= chestNum)
			{
				chestsIndexer = 0;
			}
		}
		Debug.Log("Try to create chest : " + chests[chestsIndexer].ItemId);
		return SingletonComponent<InventoryManager>.Instance.CreateItem(chests[chestsIndexer].ItemId, chests[chestsIndexer].ItemTier);
	}

	private ItemDetails PrepareHelm(bool p_isPrev)
	{
		if (p_isPrev)
		{
			helmsIndexer--;
			if (helmsIndexer < 0)
			{
				helmsIndexer = helmNum - 1;
			}
		}
		else
		{
			helmsIndexer++;
			if (helmsIndexer >= helmNum)
			{
				helmsIndexer = 0;
			}
		}
		Debug.Log("Try to create helm : " + helms[helmsIndexer].ItemId);
		return SingletonComponent<InventoryManager>.Instance.CreateItem(helms[helmsIndexer].ItemId, helms[helmsIndexer].ItemTier);
	}

	private ItemDetails PrepareWeapon(bool p_isPrev)
	{
		if (p_isPrev)
		{
			weaponsIndexer--;
			if (weaponsIndexer < 0)
			{
				weaponsIndexer = weaponNum - 1;
			}
		}
		else
		{
			weaponsIndexer++;
			if (weaponsIndexer >= weaponNum)
			{
				weaponsIndexer = 0;
			}
		}
		Debug.Log("Try to create weapon : " + weapons[weaponsIndexer].ItemId);
		return SingletonComponent<InventoryManager>.Instance.CreateItem(weapons[weaponsIndexer].ItemId, weapons[weaponsIndexer].ItemTier);
	}

	private ItemDetails PrepareShoulder(bool p_isPrev)
	{
		if (p_isPrev)
		{
			shouldersIndexer--;
			if (shouldersIndexer < 0)
			{
				shouldersIndexer = shoulderNum - 1;
			}
		}
		else
		{
			shouldersIndexer++;
			if (shouldersIndexer >= shoulderNum)
			{
				shouldersIndexer = 0;
			}
		}
		Debug.Log("Try to create shoulder : " + shoulders[shouldersIndexer].ItemId);
		return SingletonComponent<InventoryManager>.Instance.CreateItem(shoulders[shouldersIndexer].ItemId, shoulders[shouldersIndexer].ItemTier);
	}

	private ItemDetails PrepareLegging(bool p_isPrev)
	{
		if (p_isPrev)
		{
			leggingsIndexer--;
			if (leggingsIndexer < 0)
			{
				leggingsIndexer = leggingNum - 1;
			}
		}
		else
		{
			leggingsIndexer++;
			if (leggingsIndexer >= leggingNum)
			{
				leggingsIndexer = 0;
			}
		}
		Debug.Log("Try to create legging : " + leggings[leggingsIndexer].ItemId);
		return SingletonComponent<InventoryManager>.Instance.CreateItem(leggings[leggingsIndexer].ItemId, leggings[leggingsIndexer].ItemTier);
	}

	private void ShowCheckboxOnItems()
	{
		itemChecked[0] = GUI.Toggle(new Rect(360f, Screen.height - 250, 120f, 40f), itemChecked[0], curEquippedItemIds[0]);
		itemChecked[1] = GUI.Toggle(new Rect(480f, Screen.height - 250, 120f, 40f), itemChecked[1], curEquippedItemIds[1]);
		itemChecked[2] = GUI.Toggle(new Rect(600f, Screen.height - 250, 120f, 40f), itemChecked[2], curEquippedItemIds[2]);
		itemChecked[3] = GUI.Toggle(new Rect(720f, Screen.height - 250, 120f, 40f), itemChecked[3], curEquippedItemIds[3]);
		itemChecked[4] = GUI.Toggle(new Rect(840f, Screen.height - 250, 120f, 40f), itemChecked[4], curEquippedItemIds[4]);
	}

	private void SetupCharacter(GameObject _gao)
	{
		UIPopupController.Instance.HideLoadingPopup();
		if (!(_gao == null))
		{
			m_character = _gao;
			m_playerData.ComputeFinalValues();
			m_character.layer = LayerMask.NameToLayer("Player");
			Transform transform = m_character.transform;
			if (Screen.width % 1024 == 0)
			{
				transform.position = m_playerPosition + new Vector3(0.24f, 0f, 0f);
			}
			else
			{
				transform.position = m_playerPosition;
			}
			Quaternion rotation = transform.rotation;
			rotation.eulerAngles = new Vector3(0f, 170f, 0f);
			transform.rotation = new Quaternion(rotation.x, rotation.y, rotation.z, rotation.w);
			BoxCollider orAddComponent = NBComponentHelper.GetOrAddComponent<BoxCollider>(m_character);
			if (orAddComponent != null)
			{
				orAddComponent.size = new Vector3(0.6f, 2.1f, 0.77f);
				orAddComponent.center = new Vector3(0f, 1f, 0f);
			}
			SpinWithMouse orAddComponent2 = NBComponentHelper.GetOrAddComponent<SpinWithMouse>(m_character);
			Dbg.Log(orAddComponent2.name);
			orAddComponent2.target = m_character.transform;
			ShowCharacter(true);
		}
	}

	private void UpdateItemNames()
	{
		UpdateItemName(0, 4);
		UpdateItemName(1, 0);
		UpdateItemName(2, 1);
		UpdateItemName(3, 2);
		UpdateItemName(4, 3);
	}

	private void UpdateItemName(int p_curItemIndex, int p_index)
	{
		if (m_playerData.Equipments[p_index].itemDetails != null)
		{
			curEquippedItemIds[p_curItemIndex] = SingletonComponent<NBLocalization>.Instance.Get(m_playerData.Equipments[p_index].itemDetails.Name);
		}
		else
		{
			Debug.LogWarning("Why the itemDetails of equip item " + m_playerData.Equipments[p_index].id + " is null!!!?");
		}
	}

	private void ShowCharacter(bool _show)
	{
		if (!(m_character == null))
		{
			m_character.SetActiveRecursively(_show);
			m_character.GetComponent<Animation>().CrossFade("Custom_Idle");
			m_character.GetComponent<Animation>()["Custom_Idle"].wrapMode = WrapMode.Loop;
		}
	}

	private void DestroyCurrentCharacterGo()
	{
		if ((bool)m_character)
		{
			Object.DestroyImmediate(m_character);
			m_character = null;
		}
	}
}
