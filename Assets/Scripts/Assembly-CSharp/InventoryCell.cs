using System.Collections.Generic;
using UnityEngine;

public class InventoryCell : NBUIGridCell<ItemDetails>
{
	public enum Size
	{
		Small = 0,
		Medium = 1,
		Large = 2
	}

	private List<GameObject> m_children = new List<GameObject>();

	public UISprite m_disableSprite;

	public UISprite m_emptySprite;

	public UISprite m_bgSprite;

	public UISprite m_itemSprite;

	public UISprite m_elementSprite;

	public UISprite m_equippedSprite;

	public UISprite m_excessSprite;

	public UISprite m_selectedSprite;

	public UISprite m_newSprite;

	public UISprite m_igcSprite;

	public UILabel m_label;

	public UILabel m_igcNum;

	public UICheckbox m_checkbox;

	public UISprite[] m_tiersStars;

	private bool m_showIgc;

	private bool m_showPet;

	private bool m_showDoubleLoot;

	public bool m_neverEmpty;

	public bool m_hidden;

	public Size m_size = Size.Medium;

	private CharacterElement m_characterElement;

	public bool m_ShowIgc
	{
		get
		{
			return m_showIgc;
		}
		set
		{
			m_showIgc = value;
		}
	}

	public bool m_ShowPet
	{
		get
		{
			return m_showPet;
		}
		set
		{
			m_showPet = value;
		}
	}

	public bool m_ShowDoubleLoot
	{
		get
		{
			return m_showDoubleLoot;
		}
		set
		{
			m_showDoubleLoot = value;
		}
	}

	public override ItemDetails data
	{
		set
		{
			if (value == null)
			{
				Dbg.LogError("Cant set null data to cell. use Empty = true.");
				return;
			}
			m_data = value;
			if (m_data != ItemManager.s_NULL_DATA)
			{
				m_ShowIgc = false;
				m_ShowPet = false;
				m_itemSprite.atlas = m_data.BGAtlas;
				m_itemSprite.spriteName = m_data.BGSpriteName;
				if (true)
				{
					switch (m_size)
					{
					case Size.Small:
						m_elementSprite.spriteName = m_data.ElementSpriteSmall;
						break;
					case Size.Large:
						m_elementSprite.spriteName = m_data.ElementSprite;
						break;
					default:
						m_elementSprite.spriteName = m_data.ElementSpriteMedium;
						break;
					}
				}
				else
				{
					m_elementSprite.enabled = false;
				}
				m_hidden = false;
				if (m_data.IsEquipment)
				{
					m_characterElement = m_data.equipment.characterElement;
				}
			}
			m_igcNum.gameObject.SetActiveRecursively(false);
			ShowCell();
		}
	}

	public bool Enable
	{
		get
		{
			if (m_data == null)
			{
				return false;
			}
			return m_data.isEnabled;
		}
		set
		{
			if (m_data != null)
			{
				m_data.isEnabled = value;
				m_disableSprite.gameObject.active = !value;
			}
		}
	}

	public bool Selected
	{
		get
		{
			if (m_data == null)
			{
				return false;
			}
			return m_data.isSelected;
		}
		set
		{
			if (m_data != null)
			{
				m_data.isSelected = value;
				m_selectedSprite.gameObject.active = value;
			}
		}
	}

	public bool Checked
	{
		get
		{
			return m_checkbox.gameObject.active && m_checkbox.isChecked;
		}
		set
		{
			m_checkbox.isChecked = value;
		}
	}

	public bool Empty
	{
		get
		{
			return m_data == ItemManager.s_NULL_DATA;
		}
		set
		{
			if (value)
			{
				data = ItemManager.s_NULL_DATA;
			}
		}
	}

	public bool Excess
	{
		get
		{
			return m_excessSprite.gameObject.active;
		}
		set
		{
			m_excessSprite.gameObject.active = value;
		}
	}

	public bool New
	{
		get
		{
			return m_newSprite.gameObject.active;
		}
		set
		{
			m_newSprite.gameObject.active = value;
			Dbg.LogEx("Set iventory cell, item {0}, label:new to {1}", Dbg.NBLogType.COMMON_LOG, data.ItemID, value.ToString());
		}
	}

	public Equipment.Part category
	{
		get
		{
			return Equipment.FromString(GetCategory());
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if ((bool)m_disableSprite)
		{
			m_children.Add(m_disableSprite.gameObject);
			m_disableSprite.gameObject.SetActive(false);
		}
		if ((bool)m_emptySprite)
		{
			m_children.Add(m_emptySprite.gameObject);
		}
		if ((bool)m_bgSprite)
		{
			m_children.Add(m_bgSprite.gameObject);
		}
		if ((bool)m_itemSprite)
		{
			m_children.Add(m_itemSprite.gameObject);
		}
		if ((bool)m_elementSprite)
		{
			m_children.Add(m_elementSprite.gameObject);
		}
		if ((bool)m_equippedSprite)
		{
			m_children.Add(m_equippedSprite.gameObject);
		}
		if ((bool)m_excessSprite)
		{
			m_children.Add(m_excessSprite.gameObject);
		}
		if ((bool)m_label)
		{
			m_children.Add(m_label.gameObject);
		}
		if ((bool)m_checkbox)
		{
			m_children.Add(m_checkbox.gameObject);
		}
		if ((bool)m_selectedSprite)
		{
			m_children.Add(m_selectedSprite.gameObject);
		}
		if ((bool)m_newSprite)
		{
			m_children.Add(m_newSprite.gameObject);
		}
		m_data = ItemManager.s_NULL_DATA;
	}

	private void OnEnable()
	{
		ShowCell();
	}

	private void OnDisable()
	{
		UISprite[] tiersStars = m_tiersStars;
		foreach (UISprite uISprite in tiersStars)
		{
			if (null != uISprite)
			{
				uISprite.gameObject.SetActiveRecursively(false);
			}
		}
	}

	protected override void Start()
	{
		base.Start();
	}

	public void SetLayerForAllChildren(int layer)
	{
		base.gameObject.layer = layer;
		foreach (GameObject child in m_children)
		{
			child.layer = layer;
		}
	}

	public void HideChildren()
	{
		foreach (GameObject child in m_children)
		{
			child.SetActiveRecursively(false);
		}
		UISprite[] tiersStars = m_tiersStars;
		foreach (UISprite uISprite in tiersStars)
		{
			uISprite.gameObject.SetActiveRecursively(false);
		}
	}

	public void EnableChildren(bool p_enable)
	{
		m_disableSprite.enabled = p_enable;
		m_emptySprite.enabled = p_enable;
		m_bgSprite.enabled = p_enable;
		m_itemSprite.enabled = p_enable;
		m_elementSprite.enabled = p_enable;
		m_equippedSprite.enabled = p_enable;
		m_excessSprite.enabled = p_enable;
		m_selectedSprite.enabled = p_enable;
		m_newSprite.enabled = p_enable;
		m_igcSprite.enabled = p_enable;
		m_label.enabled = p_enable;
		m_igcNum.enabled = p_enable;
		m_checkbox.enabled = p_enable;
		UISprite[] tiersStars = m_tiersStars;
		foreach (UISprite uISprite in tiersStars)
		{
			uISprite.enabled = p_enable;
		}
	}

	private void HideAllExcept()
	{
		m_disableSprite.gameObject.active = false;
		m_emptySprite.gameObject.active = false;
		m_equippedSprite.gameObject.active = false;
		m_excessSprite.gameObject.active = false;
		m_selectedSprite.gameObject.active = false;
		m_newSprite.gameObject.active = false;
		m_igcSprite.gameObject.active = false;
		m_label.gameObject.active = false;
		m_igcNum.gameObject.active = false;
		m_checkbox.gameObject.SetActiveRecursively(false);
	}

	public void Refresh()
	{
		ShowCell();
	}

	private void ShowCell()
	{
		if (m_ShowIgc)
		{
			m_igcSprite.gameObject.active = true;
			m_igcNum.gameObject.active = true;
		}
		else
		{
			m_igcSprite.gameObject.active = false;
			m_igcNum.gameObject.active = false;
		}
		HideChildren();
		if (m_hidden)
		{
			return;
		}
		if (m_data == ItemManager.s_NULL_DATA)
		{
			m_characterElement = null;
			if (!m_ShowIgc && !m_showPet)
			{
				m_emptySprite.gameObject.active = true;
			}
			else
			{
				if (m_showPet)
				{
					m_itemSprite.gameObject.active = true;
					bool flag = true;
					m_elementSprite.gameObject.active = flag;
				}
				m_emptySprite.gameObject.active = false;
			}
			if (m_ShowDoubleLoot)
			{
				bool flag2 = true;
				m_elementSprite.gameObject.active = flag2;
			}
			return;
		}
		if (m_data != null)
		{
			m_bgSprite.gameObject.active = true;
			m_itemSprite.gameObject.active = true;
			m_excessSprite.gameObject.active = m_data.IsExcess;
			bool flag3 = true;
			m_elementSprite.gameObject.active = flag3;
			m_selectedSprite.gameObject.active = m_data.isSelected;
			if (m_data.IsEquipment)
			{
				m_equippedSprite.gameObject.active = m_data.IsEquip;
				m_label.gameObject.active = true;
				if (m_data.Level <= 0)
				{
					m_label.text = "LVL1";
				}
				else
				{
					m_label.text = "LVL" + m_data.Level;
				}
			}
			if (!m_data.isEnabled)
			{
				Enable = false;
			}
		}
		ShowTierStars();
	}

	private void ShowTierStars()
	{
		if (data.Category == ItemEnum.Category.LOOT)
		{
			return;
		}
		for (int i = 0; i < data.equipment.maxTier; i++)
		{
			if (i < m_tiersStars.Length)
			{
				m_tiersStars[i].gameObject.SetActiveRecursively(true);
				m_tiersStars[i].enabled = true;
				if (i < data.Tier)
				{
					m_tiersStars[i].spriteName = "star";
				}
				else
				{
					m_tiersStars[i].spriteName = "no_star";
				}
			}
		}
		for (int j = data.equipment.maxTier; j < m_tiersStars.Length; j++)
		{
			m_tiersStars[j].enabled = false;
		}
	}

	public void ShowIgc(int p_gold, float p_iconPosY = -50f, float p_descTextPosY = -122f)
	{
		m_ShowIgc = true;
		Empty = false;
		if (p_gold < 100)
		{
			m_igcSprite.spriteName = "currency/icon_gold_one";
		}
		else if (p_gold > 100 && p_gold <= 1000)
		{
			m_igcSprite.spriteName = "currency/icon_gold_few";
		}
		else
		{
			m_igcSprite.spriteName = "currency/icon_gold_stack";
		}
		m_igcNum.transform.localPosition = new Vector3(0f, p_descTextPosY, -13f);
		m_igcNum.text = string.Format(SingletonComponent<NBLocalization>.Instance.Get("{0} reward_gold"), p_gold.ToString());
		m_igcNum.gameObject.SetActiveRecursively(true);
		m_igcSprite.transform.localPosition = new Vector3(0f, p_iconPosY, -12f);
		m_igcSprite.gameObject.SetActiveRecursively(true);
	}

	public void ShowPgc(int p_amount, float p_iconPosY = -50f, float p_descTextPosY = -122f)
	{
		m_ShowIgc = true;
		Empty = false;
		m_igcSprite.spriteName = "currency/icon_diamond_one";
		m_igcNum.transform.localPosition = new Vector3(0f, p_descTextPosY, -13f);
		m_igcNum.text = string.Format(SingletonComponent<NBLocalization>.Instance.Get("{0} shop_buy_diamonds_button"), p_amount.ToString());
		m_igcNum.gameObject.SetActiveRecursively(true);
		m_igcSprite.transform.localPosition = new Vector3(0f, p_iconPosY, -12f);
		m_igcSprite.gameObject.SetActiveRecursively(true);
	}

	public void ShowDoubleXp(UIAtlas p_atlas)
	{
		m_emptySprite.atlas = p_atlas;
		m_emptySprite.spriteName = "double_xp";
	}

	public void ShowDoubleLoot(UIAtlas p_atlas)
	{
		m_elementSprite.atlas = p_atlas;
		m_elementSprite.spriteName = "double_loot";
		bool activeRecursively = true;
		m_elementSprite.gameObject.SetActiveRecursively(activeRecursively);
		m_elementSprite.enabled = activeRecursively;
		m_emptySprite.atlas = p_atlas;
		m_emptySprite.spriteName = "double_loot_2";
		m_emptySprite.transform.localPosition = new Vector3(0f, -42f, 0f);
		m_emptySprite.transform.localScale = new Vector3(117f, 53f, 1f);
	}

	public void ShowSocialGacha(UIAtlas p_atlas)
	{
		m_emptySprite.atlas = p_atlas;
		m_emptySprite.spriteName = "common_treasure_icon";
	}

	public void ShowPremGacha(UIAtlas p_atlas)
	{
		m_emptySprite.atlas = p_atlas;
		m_emptySprite.spriteName = "epic_treasure_icon";
	}

	public CharacterElement GetCharacterElement()
	{
		if (m_characterElement == null)
		{
			Dbg.LogWarn("No CharacterElement for this Inventory Cell.");
		}
		return m_characterElement;
	}

	public string GetCategory()
	{
		if (m_characterElement != null)
		{
			return m_characterElement.category;
		}
		return string.Empty;
	}

	public bool IsEquipment()
	{
		bool flag = m_data != ItemManager.s_NULL_DATA;
		bool flag2 = m_characterElement != null;
		bool flag3 = flag && flag2;
		if (!flag3)
		{
			Dbg.LogError("This is an invalid Cell.");
		}
		return flag3;
	}
}
