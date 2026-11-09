using System.Collections.Generic;
using UnityEngine;

public class ItemWarningPopup : MonoBehaviour
{
	public enum MessageType
	{
		Equipped = 0,
		Tier3Up = 1,
		Both = 2
	}

	public GameObject[] m_messages = new GameObject[3];

	public NBUILocalize m_labelHeader;

	public NBUILocalize m_labelFooter;

	public UIButton m_buttonYes;

	public UIButton m_buttonNo;

	public GameObject m_eventTarget;

	public bool m_isSingleItem;

	public List<ItemDetails> m_items;

	public MessageType m_type;

	private void OnEnable()
	{
		Init();
	}

	private void OnDisable()
	{
		Reset();
	}

	private void Init()
	{
		if (m_items == null)
		{
			return;
		}
		int type = (int)m_type;
		for (int i = 0; i < m_messages.Length; i++)
		{
			m_messages[i].SetActiveRecursively(false);
		}
		if (0 > type || type >= m_messages.Length)
		{
			return;
		}
		m_messages[type].SetActiveRecursively(true);
		int num = 3;
		ItemDetails itemDetails = m_items[0];
		foreach (ItemDetails item in m_items)
		{
			if (item.Tier > num)
			{
				num = item.Tier;
			}
		}
		TiersStars componentInChildren = MBHier.GetComponentInChildren<TiersStars>(m_messages[type]);
		if ((bool)componentInChildren)
		{
			componentInChildren.SetTier(itemDetails);
		}
		if (m_type != MessageType.Both && m_type != MessageType.Tier3Up)
		{
			return;
		}
		Transform transform = m_messages[type].transform.Find("Label (item)");
		if (transform != null)
		{
			if (m_type == MessageType.Both)
			{
				transform.localPosition = new Vector3(68f + (float)(itemDetails.equipment.maxTier - 3) * 16f, transform.localPosition.y, transform.localPosition.z);
			}
			else
			{
				transform.localPosition = new Vector3(-12f + (float)(itemDetails.equipment.maxTier - 3) * 16f, transform.localPosition.y, transform.localPosition.z);
			}
		}
	}

	private void Reset()
	{
	}
}
