using UnityEngine;

public class SetCharaDemoPanel : NBUIMenuPanel
{
	[SerializeField]
	private InventoryCell m_invCellTemplate;

	private InventoryCell[] m_inveCells = new InventoryCell[5];

	private GameObject m_character;

	public Vector3 m_playerPosition = new Vector3(99.85f, 0f, 0.5f);

	private PlayerData m_playerData;

	private void OnEnable()
	{
		if (SingletonComponent<DungeonManager>.Instance.m_demoSetInfo != null)
		{
			if (UIPopupController.Instance != null)
			{
				UIPopupController.Instance.ShowLoadingPopup(SingletonComponent<NBLocalization>.Instance.Get("character_creation_header"), SingletonComponent<NBLocalization>.Instance.Get("character_creation_content"));
			}
			for (int i = 0; i < 5; i++)
			{
				GameObject gameObject = Object.Instantiate(m_invCellTemplate.gameObject, Vector3.zero, Quaternion.identity) as GameObject;
				gameObject.transform.parent = base.transform;
				m_inveCells[i] = gameObject.GetComponent<InventoryCell>();
				m_inveCells[i].transform.localPosition = new Vector3(-130 + 130 * i, 120f, 0f);
				m_inveCells[i].HideChildren();
			}
			m_playerData = SingletonComponent<PlayerDataManager>.Instance.GetTemporaryPlayerData();
			ItemDetails itemDetails = SingletonComponent<InventoryManager>.Instance.CreateItem(SingletonComponent<DungeonManager>.Instance.m_demoSetInfo.WeaponItemId, GetItemTierByItemId(SingletonComponent<DungeonManager>.Instance.m_demoSetInfo.WeaponItemId), 1);
			ItemDetails itemDetails2 = SingletonComponent<InventoryManager>.Instance.CreateItem(SingletonComponent<DungeonManager>.Instance.m_demoSetInfo.HelmItemId, GetItemTierByItemId(SingletonComponent<DungeonManager>.Instance.m_demoSetInfo.HelmItemId), 1);
			ItemDetails itemDetails3 = SingletonComponent<InventoryManager>.Instance.CreateItem(SingletonComponent<DungeonManager>.Instance.m_demoSetInfo.ShoulderItemId, GetItemTierByItemId(SingletonComponent<DungeonManager>.Instance.m_demoSetInfo.ShoulderItemId), 1);
			ItemDetails itemDetails4 = SingletonComponent<InventoryManager>.Instance.CreateItem(SingletonComponent<DungeonManager>.Instance.m_demoSetInfo.ChestItemId, GetItemTierByItemId(SingletonComponent<DungeonManager>.Instance.m_demoSetInfo.ChestItemId), 1);
			ItemDetails itemDetails5 = SingletonComponent<InventoryManager>.Instance.CreateItem(SingletonComponent<DungeonManager>.Instance.m_demoSetInfo.LeggingItemId, GetItemTierByItemId(SingletonComponent<DungeonManager>.Instance.m_demoSetInfo.LeggingItemId), 1);
			m_inveCells[0].data = itemDetails;
			m_inveCells[1].data = itemDetails2;
			m_inveCells[2].data = itemDetails3;
			m_inveCells[3].data = itemDetails4;
			m_inveCells[4].data = itemDetails5;
			SetItemDataToAffectPlayerStatus(0, itemDetails2);
			SetItemDataToAffectPlayerStatus(1, itemDetails3);
			SetItemDataToAffectPlayerStatus(2, itemDetails4);
			SetItemDataToAffectPlayerStatus(3, itemDetails5);
			SetItemDataToAffectPlayerStatus(4, itemDetails);
			Singleton<CharacterGenerator>.Instance.GenerateMainMenuCharacter(SetupCharacter, m_playerData, false);
		}
	}

	private void OnDisable()
	{
		m_playerData = null;
		if (m_character != null)
		{
			InventoryCell[] inveCells = m_inveCells;
			foreach (InventoryCell inventoryCell in inveCells)
			{
				Object.DestroyImmediate(inventoryCell.gameObject);
			}
			Object.DestroyImmediate(m_character);
			m_character = null;
		}
	}

	private void Start()
	{
	}

	private new void Update()
	{
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

	private void ShowCharacter(bool _show)
	{
		if (!(m_character == null))
		{
			m_character.SetActiveRecursively(_show);
			m_character.GetComponent<Animation>().CrossFade("Custom_Idle");
			m_character.GetComponent<Animation>()["Custom_Idle"].wrapMode = WrapMode.Loop;
		}
	}

	private int GetItemTierByItemId(string p_itemId)
	{
		if (p_itemId.Contains("T1"))
		{
			return 1;
		}
		if (p_itemId.Contains("T2"))
		{
			return 2;
		}
		if (p_itemId.Contains("T3"))
		{
			return 3;
		}
		if (p_itemId.Contains("T4"))
		{
			return 4;
		}
		if (p_itemId.Contains("T5"))
		{
			return 5;
		}
		return -1;
	}
}
