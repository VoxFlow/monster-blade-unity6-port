using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorCollectionsListPanel : NBUIMenuPanel
{
	public enum ArmorType
	{
		Normal = 0,
		Special = 1
	}

	public ArmorType m_armorType;

	public ArmorCollectionsGrid m_grid;

	public GameObject m_spacer;

	public ElementFilterButton m_sortButton;

	private ItemManager m_itemManager;

	private ArmorCollectionsScreen m_screen;

	private static ArmorCollectionsFilter[] m_filters = new ArmorCollectionsFilter[7]
	{
		new ArmorCollectionsFilter.NewSets(),
		new ArmorCollectionsFilter.CompletedSets(),
		new ArmorCollectionsFilter.ElementSets(Element.Type.Earth),
		new ArmorCollectionsFilter.ElementSets(Element.Type.Water),
		new ArmorCollectionsFilter.ElementSets(Element.Type.Fire),
		new ArmorCollectionsFilter.ElementSets(Element.Type.Lightning),
		new ArmorCollectionsFilter.ElementSets(Element.Type.Metal)
	};

	private int m_currentFilterIndex;

	private void OnEnable()
	{
		Init();
	}

	private void OnDisable()
	{
		Reset();
	}

	private new void Awake()
	{
		m_itemManager = SingletonComponent<ItemManager>.Instance;
		if (null != parentScreen)
		{
			m_screen = parentScreen as ArmorCollectionsScreen;
		}
	}

	private void ShowSortButton(bool p_show)
	{
		if (null != m_sortButton)
		{
			m_sortButton.gameObject.SetActiveRecursively(p_show);
			if (p_show)
			{
				RefreshSortButton();
			}
		}
	}

	private void RefreshSortButton()
	{
		NBLocalization instance = SingletonComponent<NBLocalization>.Instance;
		string p_textKey = string.Empty;
		switch (m_currentFilterIndex)
		{
		case 0:
			if (null != instance)
			{
				p_textKey = instance.Get("armorcollections_filter_new_label");
			}
			m_sortButton.Show(p_textKey);
			break;
		case 1:
			if (null != instance)
			{
				p_textKey = instance.Get("armorcollections_filter_completed_label");
			}
			m_sortButton.Show(p_textKey);
			break;
		case 2:
			m_sortButton.Show(Element.Type.Earth);
			break;
		case 3:
			m_sortButton.Show(Element.Type.Water);
			break;
		case 4:
			m_sortButton.Show(Element.Type.Fire);
			break;
		case 5:
			m_sortButton.Show(Element.Type.Lightning);
			break;
		case 6:
			m_sortButton.Show(Element.Type.Metal);
			break;
		}
	}

	private void Init()
	{
		bool p_special = m_armorType == ArmorType.Special;
		int armorSetCount = m_itemManager.GetArmorSetCount(p_special);
		int count = m_grid.cells.Count;
		if (!(null != m_grid))
		{
			return;
		}
		m_grid.HideNonVisibleCells();
		if (null != m_itemManager && count < armorSetCount)
		{
			ShowSortButton(false);
			string text = null;
			if (count > 0)
			{
				text = m_grid.cells[count - 1].data.id;
			}
			IEnumerator<string> armorSetKeysEnumerator = m_itemManager.GetArmorSetKeysEnumerator(p_special);
			while (armorSetKeysEnumerator.MoveNext())
			{
				string current = armorSetKeysEnumerator.Current;
				if (text != null)
				{
					if (text == current)
					{
						break;
					}
					continue;
				}
				CreateCellWithID(current, p_special);
				break;
			}
			StartCoroutine("DoCreateCells", armorSetKeysEnumerator);
		}
		else
		{
			Refresh();
			RefreshSortButton();
		}
	}

	private IEnumerator DoCreateCells(IEnumerator<string> p_enumerator)
	{
		if (p_enumerator == null)
		{
			yield break;
		}
		bool special = m_armorType == ArmorType.Special;
		bool refresh = true;
		while (p_enumerator.MoveNext())
		{
			string setID = p_enumerator.Current;
			CreateCellWithID(setID, special);
			if (refresh)
			{
				refresh = false;
				Refresh();
			}
			yield return null;
		}
		if (m_grid.visibleCells.Count == 0)
		{
			Sort();
		}
		else
		{
			m_grid.RecalculateBoundingBox(false);
			m_grid.Refresh();
			Refresh();
		}
		ShowSortButton(true);
	}

	private void CreateCellWithID(string p_setID, bool p_special)
	{
		if (null == m_grid)
		{
			return;
		}
		ArmorSet armorSet = m_itemManager.GetArmorSet(p_setID, p_special);
		m_grid.AddData(armorSet);
		ArmorCollectionsCell armorCollectionsCell = m_grid.AddCell(0f, 0f);
		if (null != armorCollectionsCell)
		{
			ArmorCollectionsFilter armorCollectionsFilter = m_filters[m_currentFilterIndex];
			if (!armorCollectionsFilter.Accept(armorCollectionsCell))
			{
				m_grid.Hide(armorCollectionsCell);
			}
			else
			{
				armorCollectionsCell.visible = true;
			}
			UIButtonMessage component = armorCollectionsCell.GetComponent<UIButtonMessage>();
			if (null != component)
			{
				component.target = base.gameObject;
			}
		}
	}

	private void Refresh(bool p_updateScrollBar = true)
	{
		NBUIGridFX component = m_grid.GetComponent<NBUIGridFX>();
		if (null != component)
		{
			component.Reset();
			if (p_updateScrollBar)
			{
				component.OnScrollBarChanged(null);
			}
		}
	}

	private new void Reset()
	{
		StopCoroutine("DoCreateCells");
	}

	private void Sort()
	{
		if (m_filters != null)
		{
			ArmorCollectionsFilter armorCollectionsFilter = null;
			do
			{
				m_currentFilterIndex = (m_currentFilterIndex + 1) % m_filters.Length;
				armorCollectionsFilter = m_filters[m_currentFilterIndex];
			}
			while (!m_grid.ShowWithFilter(armorCollectionsFilter));
			Refresh();
		}
	}

	private void OnButtonClick(GameObject p_sender)
	{
		if (null == p_sender)
		{
			return;
		}
		if (null != m_sortButton && m_sortButton.gameObject == p_sender)
		{
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_ok");
			Sort();
			RefreshSortButton();
			return;
		}
		ArmorCollectionsCell component = p_sender.GetComponent<ArmorCollectionsCell>();
		if (null != component)
		{
			if (null != m_screen)
			{
				m_screen.selectedArmorSet = component.data;
			}
			PushState(ArmorCollectionsScreen.ScreenState.ARMOR_DETAIL);
			if (null != SingletonComponent<SoundManager>.Instance)
			{
				SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_secondary_ui_tap");
			}
		}
	}

	private void PushState(ArmorCollectionsScreen.ScreenState p_state)
	{
		if (!(null == parentScreen))
		{
			ArmorCollectionsScreen armorCollectionsScreen = (ArmorCollectionsScreen)parentScreen;
			armorCollectionsScreen.PushState(p_state);
		}
	}
}
