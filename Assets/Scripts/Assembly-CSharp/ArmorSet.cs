using System.Collections.Generic;

public class ArmorSet
{
	public readonly string id;

	public readonly string name;

	public readonly string texturePath;

	public readonly Element.Type elementType;

	public readonly int tier;

	public readonly int maxTier;

	public readonly string description;

	public readonly string bonusName;

	public readonly string bonusDescription;

	public readonly int maxCount = 5;

	public bool isNew;

	protected Dictionary<Equipment.Part, ItemDetails> m_items;

	public int count
	{
		get
		{
			if (m_items == null || m_items.Count <= 0)
			{
				return 0;
			}
			int num = 0;
			foreach (ItemDetails value in m_items.Values)
			{
				if (value.isEnabled)
				{
					num++;
				}
			}
			return num;
		}
	}

	public bool isCompleted
	{
		get
		{
			return count >= maxCount;
		}
	}

	public bool isSpecial { get; private set; }

	public ArmorSet(string p_id, string p_name, string p_texturePath, Element.Type p_elementType, int p_tier, int p_maxTier, string p_description, string p_bonusName, string p_bonusDescription)
	{
		id = p_id;
		name = p_name;
		texturePath = p_texturePath;
		elementType = p_elementType;
		tier = p_tier;
		maxTier = p_maxTier;
		if (maxTier < tier)
		{
			maxTier = tier;
		}
		description = p_description;
		bonusName = p_bonusName;
		bonusDescription = p_bonusDescription;
		isSpecial = id.Contains("special");
	}

	public bool AddItem(ItemDetails p_item)
	{
		if (m_items == null)
		{
			m_items = new Dictionary<Equipment.Part, ItemDetails>();
		}
		if (m_items != null && p_item != null)
		{
			if (!m_items.ContainsKey(p_item.equipment.part))
			{
				m_items.Add(p_item.equipment.part, p_item);
				return true;
			}
			ItemDetails itemDetails = m_items[p_item.equipment.part];
			if (itemDetails != null)
			{
				itemDetails.isEnabled |= p_item.isEnabled;
			}
		}
		return false;
	}

	public IEnumerator<ItemDetails> GetItemsEnumerator()
	{
		if (m_items == null)
		{
			return null;
		}
		return m_items.Values.GetEnumerator();
	}

	public ItemDetails GetItem(Equipment.Part p_part)
	{
		if (m_items == null || !m_items.ContainsKey(p_part))
		{
			return null;
		}
		return m_items[p_part];
	}
}
