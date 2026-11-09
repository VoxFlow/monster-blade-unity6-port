using System;
using Models;
using UnityEngine;

public class PromoCodeCell : NBUIGridCell<PromoCodeData>
{
	public InventoryCell m_inventoryCellPrefab;

	public UILabel m_headerLabel;

	public UILabel m_itemNameLabel;

	public UILabel m_validDateLabel;

	public UILabel m_infoLabel;

	public UIInput m_codeInput;

	public UIButton m_sendButton;

	public Transform m_inventoryCellSlot;

	public Action<string, string> m_sendCallback;

	protected InventoryCell m_inventoryCell;

	public override PromoCodeData data
	{
		set
		{
			m_data = value;
			if (m_data != null)
			{
				SetHeader(m_data.name);
				if (m_data.type == RewardsType.ITEM)
				{
					SetItem(m_data.rewardsValue, m_data.tier);
				}
				SetValidDate(m_data.startTime, m_data.endTime);
				SetInfo(m_data.description);
			}
		}
	}

	private void OnLocalizationChanged(UILabel p_label)
	{
		if (null != m_codeInput && null != p_label && p_label == m_codeInput.label)
		{
			m_codeInput.defaultText = p_label.text;
		}
	}

	private void OnButtonClick(GameObject p_sender)
	{
		if (null != m_sendButton && p_sender == m_sendButton.gameObject && m_sendCallback != null && null != m_codeInput && m_data != null)
		{
			m_sendCallback(m_codeInput.text, m_data.ID);
		}
	}

	private void SetHeader(string p_text)
	{
		if (!(null == m_headerLabel))
		{
			m_headerLabel.text = p_text;
		}
	}

	private void SetItem(string p_itemId, int p_tier = -1)
	{
		ItemDetails item = null;
		InventoryManager instance = SingletonComponent<InventoryManager>.Instance;
		if (null != instance)
		{
			item = instance.CreateItem(p_itemId, p_tier);
		}
		SetItem(item);
	}

	private void SetItem(ItemDetails p_item)
	{
		if (p_item == null || null == m_inventoryCellPrefab)
		{
			return;
		}
		if (null == m_inventoryCell)
		{
			m_inventoryCell = UnityEngine.Object.Instantiate(m_inventoryCellPrefab) as InventoryCell;
			if (null != m_inventoryCellSlot)
			{
				m_inventoryCell.transform.parent = base.transform;
				m_inventoryCell.transform.localPosition = m_inventoryCellSlot.transform.localPosition;
				m_inventoryCell.transform.localScale = m_inventoryCellSlot.transform.localScale;
			}
		}
		if (null != m_inventoryCell)
		{
			m_inventoryCell.data = p_item;
		}
		if (null != m_itemNameLabel)
		{
			NBLocalization instance = SingletonComponent<NBLocalization>.Instance;
			if (null != instance)
			{
				string format = instance.Get("promo_code_reward_label");
				string arg = instance.Get(p_item.Name);
				string text = string.Format(format, arg);
				m_itemNameLabel.text = text;
			}
		}
	}

	private void SetValidDate(DateTime p_start, DateTime p_end)
	{
		NBLocalization instance = SingletonComponent<NBLocalization>.Instance;
		if (null != instance)
		{
			string format = instance.Get("promo_code_valid_date");
			string arg = p_start.ToString("yyyy/M/d");
			string arg2 = p_end.ToString("yyyy/M/d HH:mm");
			string validDate = string.Format(format, arg, arg2);
			SetValidDate(validDate);
		}
	}

	private void SetValidDate(string p_text)
	{
		if (!(null == m_validDateLabel))
		{
			m_validDateLabel.text = p_text;
		}
	}

	private void SetInfo(string p_text)
	{
		if (!(null == m_infoLabel))
		{
			m_infoLabel.text = p_text;
		}
	}
}
