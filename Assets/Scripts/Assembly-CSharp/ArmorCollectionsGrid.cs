using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArmorCollectionsGrid : NBUIGrid2D<ArmorCollectionsCell, ArmorSet>
{
	public UIDraggablePanel m_scrollView;

	public UISprite m_spacer;

	public int m_maxVisibleCells = 5;

	private float m_lastScrollValue;

	private float m_maxViewDistance;

	private float m_cellWidth;

	private float m_minPos;

	private float m_maxPos;

	private int m_lastIndex = -1;

	private bool m_init;

	private int m_numExtraCells;

	private int m_lastCount;

	protected override void Awake()
	{
	}

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
		int count = m_visibleCells.Count;
		if (count > 0)
		{
			ArmorCollectionsCell armorCollectionsCell = base.visibleCells[0];
			m_cellWidth = armorCollectionsCell.GetComponent<Collider>().bounds.size.x / GLOBAL.UI_SCALE_FACTOR;
			if (null != m_scrollView && null != m_scrollView.horizontalScrollBar)
			{
				UIScrollBar horizontalScrollBar = m_scrollView.horizontalScrollBar;
				horizontalScrollBar.onChange = (UIScrollBar.OnScrollBarChange)Delegate.Remove(horizontalScrollBar.onChange, new UIScrollBar.OnScrollBarChange(OnScroll));
				UIScrollBar horizontalScrollBar2 = m_scrollView.horizontalScrollBar;
				horizontalScrollBar2.onChange = (UIScrollBar.OnScrollBarChange)Delegate.Combine(horizontalScrollBar2.onChange, new UIScrollBar.OnScrollBarChange(OnScroll));
			}
			m_minPos = m_visibleCells[0].transform.localPosition.x;
			m_maxPos = m_visibleCells[count - 1].transform.localPosition.x;
			m_numExtraCells = (int)base.visibleCells[0].m_cullThresholdMultiplier;
			m_init = true;
		}
	}

	private void Start()
	{
		if (!m_init)
		{
			Init();
		}
	}

	private void Reset()
	{
		if (null != m_scrollView && null != m_scrollView.horizontalScrollBar)
		{
			UIScrollBar horizontalScrollBar = m_scrollView.horizontalScrollBar;
			horizontalScrollBar.onChange = (UIScrollBar.OnScrollBarChange)Delegate.Remove(horizontalScrollBar.onChange, new UIScrollBar.OnScrollBarChange(OnScroll));
		}
		m_init = false;
		m_lastIndex = -1;
	}

	public void HideNonVisibleCells()
	{
		List<ArmorCollectionsCell> p_cells = m_allCells.Where((ArmorCollectionsCell cell) => !cell.visible).ToList();
		Hide(p_cells);
	}

	public bool ShowWithFilter(ArmorCollectionsFilter p_filter)
	{
		if (p_filter == null)
		{
			return false;
		}
		List<ArmorCollectionsCell> list = p_filter.Filter(m_allCells);
		if (list == null || list.Count == 0)
		{
			return false;
		}
		MarkVisible(m_visibleCells, false);
		MarkVisible(list, true);
		Hide(m_visibleCells);
		ReplaceVisibleCells(list);
		Reposition(true);
		RecalculateBoundingBox();
		Refresh();
		return true;
	}

	private void MarkVisible(List<ArmorCollectionsCell> p_cells, bool p_visible)
	{
		if (p_cells == null)
		{
			return;
		}
		int count = p_cells.Count;
		for (int i = 0; i < count; i++)
		{
			ArmorCollectionsCell armorCollectionsCell = p_cells[i];
			if (null != armorCollectionsCell)
			{
				armorCollectionsCell.visible = p_visible;
			}
		}
	}

	private void Hide(List<ArmorCollectionsCell> p_cells)
	{
		if (p_cells == null)
		{
			return;
		}
		int count = p_cells.Count;
		for (int i = 0; i < count; i++)
		{
			ArmorCollectionsCell armorCollectionsCell = p_cells[i];
			if (null != armorCollectionsCell && !armorCollectionsCell.visible)
			{
				armorCollectionsCell.gameObject.SetActiveRecursively(false);
				armorCollectionsCell.transform.localPosition = Vector3.up * 1000f;
			}
		}
	}

	public void RecalculateBoundingBox(bool p_updateScrollBar = true)
	{
		if (null == m_spacer || m_visibleCells == null || null == m_scrollView || m_visibleCells.Count == 0)
		{
			return;
		}
		int count = m_visibleCells.Count;
		if (count != m_lastCount)
		{
			m_lastCount = count;
			float width = m_visibleCells[0].width;
			width += hSpace;
			float num = (float)(count - 1) * width;
			float num2 = num;
			UIPanel component = m_scrollView.GetComponent<UIPanel>();
			if (null != component && count >= 1)
			{
				num2 += component.clipRange.z;
			}
			m_spacer.transform.localScale = new Vector3(num2, 10f, 1f);
			m_spacer.transform.localPosition = new Vector3(num * 0.5f, 0f, 50f);
			UISprite component2 = m_spacer.GetComponent<UISprite>();
			if (null != component2)
			{
				component2.enabled = false;
			}
			m_minPos = m_visibleCells[0].transform.localPosition.x;
			m_maxPos = m_visibleCells[count - 1].transform.localPosition.x;
			if (p_updateScrollBar)
			{
				float scrollValue = m_scrollView.horizontalScrollBar.scrollValue;
				m_scrollView.ResetPosition();
				m_scrollView.horizontalScrollBar.scrollValue = scrollValue;
			}
		}
	}

	public void Refresh()
	{
		m_lastIndex = -1;
		OnScroll(null);
	}

	private void ReplaceVisibleCells(List<ArmorCollectionsCell> cells)
	{
		List<ArmorCollectionsCell> list = m_visibleCells;
		m_visibleCells = cells;
		list.Clear();
		list = null;
	}

	private void OnScroll(UIScrollBar p_bar)
	{
		if (m_maxViewDistance <= 0f)
		{
			Init();
		}
		int count = m_visibleCells.Count;
		float num = 0f - m_scrollView.transform.localPosition.x;
		float num2 = m_maxPos - m_minPos;
		int num3 = 0;
		if (!Mathf.Approximately(num2, 0f))
		{
			num3 = (int)((num - m_minPos) * (float)(count - 1) / num2);
		}
		if (num3 == m_lastIndex)
		{
			return;
		}
		int num4 = m_maxVisibleCells / 2 + m_numExtraCells;
		for (int i = num3 - num4; i <= num3 + num4; i++)
		{
			if (i >= 0 && i < count)
			{
				ArmorCollectionsCell armorCollectionsCell = m_visibleCells[i];
				if (!armorCollectionsCell.gameObject.active)
				{
					armorCollectionsCell.gameObject.SetActiveRecursively(true);
				}
			}
		}
		m_lastIndex = num3;
	}
}
