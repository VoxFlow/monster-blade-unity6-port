using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryGrid : NBUIGrid2D<InventoryCell, ItemDetails>
{
	private Equipment.Part m_categoryOnTop = Equipment.Part.Max;

	private int m_tierOnTop = 5;

	private Dictionary<Equipment.Part, List<InventoryCell>> m_groups = new Dictionary<Equipment.Part, List<InventoryCell>>();

	private bool m_cached;

	public Equipment.Part currentTopCategory
	{
		get
		{
			return m_categoryOnTop;
		}
		set
		{
			m_categoryOnTop = value;
		}
	}

	private void Start()
	{
	}

	public void ShowOnlyCategory(Equipment.Part category)
	{
		if (!m_cached)
		{
			CacheGroups();
		}
		m_tierOnTop = 5;
		IEnumerable<InventoryCell> enumerable = m_allCells.Where((InventoryCell cell) => cell.category != category);
		foreach (InventoryCell item in enumerable)
		{
			item.gameObject.SetActiveRecursively(false);
		}
		List<InventoryCell> list = (from cell in m_allCells
			where cell.category == category && !cell.data.IsBaseItem
			orderby cell.data.EquipOrder, cell.data.NewOrder.ToString(), cell.data.Tier.ToString() descending
			select cell).ToList();
		ReplaceVisibleCells(list);
		Reposition();
	}

	public void ShuffleTopCategory()
	{
		bool flag = false;
		int num = 0;
		do
		{
			m_categoryOnTop = (Equipment.Part)((int)(m_categoryOnTop + 1) % 6);
			flag = GroupWithTopCategory(m_categoryOnTop);
			num++;
		}
		while (num < 6 && !flag);
	}

	public void ShuffleWithinCategory(Equipment.Part category)
	{
		if (m_visibleCells.Count <= 1)
		{
			return;
		}
		if (!m_cached)
		{
			CacheGroups();
		}
		List<InventoryCell> list = new List<InventoryCell>();
		List<InventoryCell> source = m_groups[category];
		m_tierOnTop = (m_tierOnTop - 1 + 6) % 6;
		List<InventoryCell> list2 = null;
		int i = 0;
		int num = 5;
		for (; i < 6; i++)
		{
			int currTier = (num + m_tierOnTop + 1) % 6;
			list2 = ((currTier == 0) ? null : source.Where((InventoryCell cell) => !cell.data.IsBaseItem && cell.data.Tier == currTier).ToList());
			if (num == 5 && (list2 == null || list2.Count == 0))
			{
				m_tierOnTop = (m_tierOnTop - 1 + 6) % 6;
				num++;
			}
			else if (list2 != null)
			{
				list.AddRange(list2);
			}
			num--;
		}
		Debug.Log("TIER ON TOP = " + m_tierOnTop + ", iterations count = " + i);
		ReplaceVisibleCells(list);
		Reposition();
	}

	public void Sort(bool p_dirty = false)
	{
		m_categoryOnTop = Equipment.Part.Helm;
		List<InventoryCell> list = new List<InventoryCell>();
		List<InventoryCell> collection = (from cell in m_allCells
			where cell.gameObject.active && cell.data.IsEquip && !cell.data.IsBaseItem
			orderby cell.category
			select cell).ToList();
		List<InventoryCell> collection2 = (from cell in m_allCells
			where cell.gameObject.active && cell.data.isNew && !cell.data.IsBaseItem
			orderby cell.category, cell.data.LevelStr descending, cell.data.Tier.ToString() descending
			select cell).ToList();
		List<InventoryCell> list2 = (from cell in m_allCells
			where cell.gameObject.active && cell.data.IsBaseItem
			orderby cell.category
			select cell).ToList();
		foreach (InventoryCell item in list2)
		{
			item.gameObject.SetActiveRecursively(false);
		}
		list.AddRange(collection);
		list.AddRange(collection2);
		if (p_dirty)
		{
			m_cached = false;
		}
		if (!m_cached)
		{
			CacheGroups();
		}
		int num = 6;
		for (int num2 = 0; num2 < num; num2++)
		{
			List<InventoryCell> collection3 = (from cell in m_groups[(Equipment.Part)num2]
				where cell.gameObject.active && !cell.data.IsEquip && !cell.data.isNew && !cell.data.IsBaseItem
				orderby cell.data.Level descending, cell.data.Tier.ToString() descending
				select cell).ToList();
			list.AddRange(collection3);
		}
		ReplaceVisibleCells(list);
		Reposition();
	}

	public void RemoveCellsFromInventory(List<InventoryCell> p_cells)
	{
		if (m_cached)
		{
			foreach (InventoryCell p_cell in p_cells)
			{
				m_groups[p_cell.category].Remove(p_cell);
			}
		}
		RemoveCells(p_cells);
	}

	private void ReplaceVisibleCells(List<InventoryCell> cells)
	{
		List<InventoryCell> list = m_visibleCells;
		m_visibleCells = cells;
		list.Clear();
		list = null;
	}

	private void CacheGroups()
	{
		Equipment.Part currentCategory = Equipment.Part.Helm;
		int num = 6;
		for (int i = 0; i < num; i++)
		{
			List<InventoryCell> value = (from cell in m_allCells
				where cell.category == currentCategory && !cell.data.IsBaseItem
				orderby cell.data.EquipOrder.ToString(), cell.data.NewOrder.ToString(), cell.data.LevelStr descending, cell.data.Tier.ToString() descending
				select cell).ToList();
			if (m_groups.ContainsKey(currentCategory))
			{
				m_groups[currentCategory].Clear();
				m_groups.Remove(currentCategory);
			}
			m_groups.Add(currentCategory, value);
			currentCategory = (Equipment.Part)((int)(currentCategory + 1) % num);
		}
		m_cached = true;
	}

	public bool GroupWithTopCategory(Equipment.Part categoryOnTop = Equipment.Part.Weapon)
	{
		if (!m_cached)
		{
			CacheGroups();
		}
		List<InventoryCell> list = new List<InventoryCell>();
		Equipment.Part part = categoryOnTop;
		int num = 6;
		for (int i = 0; i < num; i++)
		{
			List<InventoryCell> list2 = m_groups[part].Where((InventoryCell cell) => cell.gameObject.active).ToList();
			if (i == 0 && list2.Count == 0)
			{
				return false;
			}
			list.AddRange(list2);
			part = (Equipment.Part)((int)(part + 1) % num);
		}
		ReplaceVisibleCells(list);
		Reposition();
		return true;
	}

	public void UnCheckAllCells()
	{
		foreach (InventoryCell allCell in m_allCells)
		{
			allCell.Checked = false;
		}
	}
}
