using System;
using System.Collections.Generic;
using UnityEngine;

public class ArmorCollectionsDetail : MonoBehaviour
{
	public InventoryCell m_inventoryCellPrefab;

	public TiersStars m_tiers;

	public NBUILocalize m_nameLabel;

	public UISprite m_elementSprite;

	public UILabel m_healthLabel;

	public UILabel m_damageLabel;

	public UILabel m_armorLabel;

	public UILabel m_criticalLabel;

	public Transform m_itemCellsRoot;

	public Transform[] m_itemSlots;

	public NBUILocalize m_bonusDescriptionLabel;

	public UILabel m_collectNumLabel;

	public UISlider m_collectSlider;

	public UIButton m_claimButton;

	public NBUILocalize m_descriptionLabel;

	private static Equipment.Part[] s_itemCellsOrder = new Equipment.Part[5]
	{
		Equipment.Part.Weapon,
		Equipment.Part.Helm,
		Equipment.Part.Shoulder,
		Equipment.Part.Chest,
		Equipment.Part.Pants
	};

	private ArmorSet m_armor;

	private ArmorCollectionsScreen m_screen;

	private InventoryCell[] m_itemCells = new InventoryCell[5];

	private PlayerData m_playerData;

	private void OnEnable()
	{
		Init();
	}

	private void Init()
	{
		if (null != m_claimButton)
		{
			m_claimButton.gameObject.SetActiveRecursively(false);
		}
	}

	private void Awake()
	{
	}

	public void SetArmor(ArmorSet p_armor)
	{
		m_armor = p_armor;
		if (m_armor == null)
		{
			return;
		}
		SetName(m_armor.name);
		SetCompleteCount(m_armor.count, m_armor.maxCount);
		SetTier(m_armor.tier, m_armor.maxTier);
		SetElementType(m_armor.elementType);
		SetCompleteCount(m_armor.count, m_armor.maxCount);
		SetDescription(m_armor.description);
		SetBonusDescription(m_armor.bonusDescription);
		if (m_playerData == null)
		{
			m_playerData = new PlayerData();
			m_playerData.CharacterType = CharacterType.Dummy;
			m_playerData.ApplyCriticalFormula = false;
			m_playerData.Str = 0f;
			m_playerData.Con = 0f;
			m_playerData.Agi = 0f;
		}
		InventoryManager instance = SingletonComponent<InventoryManager>.Instance;
		IEnumerator<ItemDetails> itemsEnumerator = m_armor.GetItemsEnumerator();
		if (itemsEnumerator != null)
		{
			while (itemsEnumerator.MoveNext())
			{
				ItemDetails current = itemsEnumerator.Current;
				SetItem(current);
				if (null != instance)
				{
					m_playerData.Equip(current);
					instance.RemoveFromNewCollectionItems(current);
					m_armor.isNew = false;
				}
			}
		}
		UpdateStats();
	}

	public PlayerData GetPlayerData()
	{
		return m_playerData;
	}

	private void SetItem(ItemDetails p_item)
	{
		int num = Array.IndexOf(s_itemCellsOrder, p_item.equipment.part);
		if (num < m_itemSlots.Length && num < m_itemCells.Length)
		{
			if (null == m_itemCells[num])
			{
				m_itemCells[num] = UnityEngine.Object.Instantiate(m_inventoryCellPrefab) as InventoryCell;
				m_itemCells[num].transform.parent = m_itemCellsRoot;
				m_itemCells[num].transform.localPosition = m_itemSlots[num].transform.localPosition;
				m_itemCells[num].transform.localScale = m_itemSlots[num].transform.localScale;
				UIButtonMessage component = m_itemCells[num].GetComponent<UIButtonMessage>();
				component.target = base.gameObject;
			}
			if (null != m_itemCells[num])
			{
				m_itemCells[num].data = p_item;
				m_itemCells[num].Enable = p_item.isEnabled;
			}
		}
	}

	private void SetName(string p_name)
	{
		if (null != m_nameLabel)
		{
			m_nameLabel.Key = p_name;
		}
	}

	private void SetCompleteCount(int p_count, int p_maxCount)
	{
		float sliderValue = 0f;
		if (p_maxCount != 0)
		{
			sliderValue = (float)p_count * 1f / (float)p_maxCount;
		}
		if (null != m_collectSlider)
		{
			m_collectSlider.sliderValue = sliderValue;
		}
		if (null != m_collectNumLabel)
		{
			m_collectNumLabel.text = p_count + "/" + p_maxCount;
		}
	}

	private void SetTier(int p_tier, int p_maxTier)
	{
		if (null != m_tiers)
		{
			m_tiers.SetTier(p_tier, p_maxTier);
		}
	}

	private void SetElementType(Element.Type p_type)
	{
		if (null != m_elementSprite)
		{
			string value = string.Empty;
			if (GLOBAL.s_elementIconMap.TryGetValue(p_type, out value))
			{
				m_elementSprite.spriteName = value;
			}
		}
	}

	private void SetDescription(string p_text)
	{
		if (null != m_descriptionLabel)
		{
			m_descriptionLabel.Key = p_text;
		}
	}

	private void SetBonusDescription(string p_text)
	{
		if (null != m_bonusDescriptionLabel)
		{
			m_bonusDescriptionLabel.Key = p_text;
		}
	}

	private void UpdateStats()
	{
		if (m_playerData != null)
		{
			m_playerData.ComputeFinalValues();
			if (null != m_healthLabel)
			{
				m_healthLabel.text = Mathf.CeilToInt(m_playerData.MaxHp).ToString();
			}
			if (null != m_damageLabel)
			{
				m_damageLabel.text = Mathf.CeilToInt(m_playerData.MinAttack) + " - " + Mathf.CeilToInt(m_playerData.MaxAttack);
			}
			if (null != m_armorLabel)
			{
				m_armorLabel.text = Mathf.CeilToInt(m_playerData.Def) + string.Empty;
			}
			if (null != m_criticalLabel)
			{
				m_criticalLabel.text = string.Format("{0:0.00}%", m_playerData.CriticalChance);
			}
		}
	}

	private void OnButtonClick(GameObject p_sender)
	{
		if (null != p_sender)
		{
			UIPopupController instance = UIPopupController.Instance;
			InventoryCell component = p_sender.GetComponent<InventoryCell>();
			if (null != component && null != instance)
			{
				instance.ShowItemStats(component.data, null, true, false);
			}
		}
	}
}
