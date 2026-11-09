using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonAreaSelectPanel : NBUIMenuPanel
{
	public DungeonAreaGrid areaList;

	public UISprite container;

	public Component eventTarget;

	[SerializeField]
	private UIDraggablePanel m_scollView;

	[SerializeField]
	private DungeonAreaSelectionArrow m_leftArrow;

	[SerializeField]
	private DungeonAreaSelectionArrow m_rightArrow;

	private float m_firstCellX;

	private float m_lastCellX = 300f;

	private bool m_init;

	private SpringPanel m_spring;

	private bool m_clicked;

	private DungeonAreaCell m_currentCell;

	private bool m_isScrolled;

	public bool IsScrolled
	{
		get
		{
			return m_isScrolled;
		}
		set
		{
			m_isScrolled = value;
		}
	}

	private void OnEnable()
	{
		m_isScrolled = false;
		Messenger.AddListener("city_select_cancel", OnResetClick);
		Init();
		UIAnchor component = m_leftArrow.GetComponent<UIAnchor>();
		if (component != null)
		{
			component.uiCamera = MainMenu.Instance.GetMainMenuCamera();
		}
		UIAnchor component2 = m_rightArrow.GetComponent<UIAnchor>();
		if (component2 != null)
		{
			component2.uiCamera = MainMenu.Instance.GetMainMenuCamera();
		}
		m_rightArrow.ParentPanel = this;
		m_leftArrow.ParentPanel = this;
		m_rightArrow.gameObject.SetActiveRecursively(false);
		m_leftArrow.gameObject.SetActiveRecursively(false);
	}

	private void OnDisable()
	{
		Messenger.RemoveListener("city_select_cancel", OnResetClick);
		Reset();
		m_leftArrow.gameObject.SetActiveRecursively(false);
		m_rightArrow.gameObject.SetActiveRecursively(false);
	}

	private new void Awake()
	{
		GameObject gameObject = Resources.Load("UI/Prefabs/Generic/DragPad") as GameObject;
		if (!gameObject)
		{
			return;
		}
		GameObject gameObject2 = Object.Instantiate(gameObject, Vector3.zero, Quaternion.identity) as GameObject;
		if ((bool)gameObject2)
		{
			gameObject2.transform.parent = base.gameObject.transform;
			gameObject2.transform.localScale = Vector3.one;
			gameObject2.transform.localPosition = Vector3.zero;
			UIDragPanelContents component = gameObject2.GetComponent<UIDragPanelContents>();
			UIDraggablePanel componentInChildren = MBHier.GetComponentInChildren<UIDraggablePanel>(base.gameObject);
			if ((bool)component && (bool)componentInChildren)
			{
				component.draggablePanel = componentInChildren;
			}
		}
	}

	public override void Reset()
	{
		m_clicked = false;
		int count = areaList.cells.Count;
		for (int i = 0; i < count; i++)
		{
			DungeonAreaCell dungeonAreaCell = areaList.cells[i];
			dungeonAreaCell.hidden = true;
		}
		DungeonMenuScreen component = base.transform.parent.GetComponent<DungeonMenuScreen>();
		if ((bool)component)
		{
			component.EnableBackground(false);
		}
	}

	public override void OnExitTransition()
	{
		if ((bool)m_currentCell)
		{
			StartCoroutine(HideCells(m_currentCell.cellIndex));
		}
	}

	private void Init()
	{
		if (!SingletonComponent<DungeonManager>.Instance)
		{
			return;
		}
		if (MenuBackground3D.Instance != null)
		{
			MenuBackground3D.Instance.ShowDefaultBackGround();
		}
		if (!m_init)
		{
			List<DungeonManager.CityStruct> allUnlockedCities = SingletonComponent<DungeonManager>.Instance.GetAllUnlockedCities();
			int count = allUnlockedCities.Count;
			if (count <= 0)
			{
				return;
			}
			foreach (DungeonManager.CityStruct item in allUnlockedCities)
			{
				areaList.AddData(item);
				DungeonAreaCell dungeonAreaCell = areaList.AddCell(0f, 0f);
				dungeonAreaCell.SetReqLevel(item.minLevel);
				dungeonAreaCell.eventTarget = this;
				dungeonAreaCell.hidden = true;
				dungeonAreaCell.gameObject.AddComponent<TweenCell>();
			}
			float num = areaList.cells[0].GetComponent<Collider>().bounds.size.x / GLOBAL.UI_SCALE_FACTOR;
			float num2 = (float)(count - 1) * num;
			float num3 = num2;
			if (count > 1)
			{
				num3 += GLOBAL.BASE_WIDTH * 1.5f;
			}
			container.transform.localScale = new Vector3(num3, 10f, 1f);
			container.transform.localPosition = new Vector3(num2 * 0.5f, 0f, 0f);
			m_firstCellX = areaList.cells[0].transform.localPosition.x;
			m_lastCellX = areaList.cells[areaList.cells.Count - 1].transform.localPosition.x;
			m_init = true;
		}
		if (areaList.cells.Count <= 0)
		{
			return;
		}
		CityDbStruct currentCity = SingletonComponent<DungeonManager>.Instance.CurrentCity;
		if (currentCity != null)
		{
			DungeonAreaCell dungeonAreaCell2 = areaList.cells[areaList.cells.Count - 1];
			OnCenterCell(dungeonAreaCell2, true);
			NBUIGridFX component = areaList.GetComponent<NBUIGridFX>();
			if ((bool)component)
			{
				component.OnScrollBarChanged(null);
			}
			StartCoroutine(ShowCells(dungeonAreaCell2.cellIndex));
		}
	}

	private IEnumerator ShowCells(int currentIndex)
	{
		yield return null;
		int count = areaList.cells.Count;
		int start = currentIndex - 2;
		int end = currentIndex + 2;
		if (start < 0)
		{
			start = 0;
		}
		if (end > count - 1)
		{
			end = count - 1;
		}
		for (int i = 0; i < start; i++)
		{
			DungeonAreaCell cell = areaList.cells[i];
			if ((bool)cell)
			{
				cell.SetLock();
			}
			if (cell.hidden)
			{
				cell.hidden = false;
			}
			TweenCell tc = cell.GetComponent<TweenCell>();
			if ((bool)tc)
			{
				tc.Reset();
			}
		}
		for (int j = start; j <= end; j++)
		{
			DungeonAreaCell cell2 = areaList.cells[j];
			if ((bool)cell2)
			{
				cell2.SetLock();
			}
			TweenCell tc2 = cell2.GetComponent<TweenCell>();
			if ((bool)tc2)
			{
				tc2.offset = -Vector3.up * 50f;
				tc2.Init();
				cell2.hidden = false;
				tc2.OnEnter();
			}
			yield return new MBWaitForSeconds(0.1f);   // PORT (CD-059)
		}
		for (int k = end + 1; k < count; k++)
		{
			DungeonAreaCell cell3 = areaList.cells[k];
			if ((bool)cell3)
			{
				cell3.SetLock();
			}
			if (cell3.hidden)
			{
				cell3.hidden = false;
			}
			TweenCell tc3 = cell3.GetComponent<TweenCell>();
			if ((bool)tc3)
			{
				tc3.Reset();
			}
		}
	}

	private new void Update()
	{
		if (m_isScrolled || areaList.cells.Count <= 3)
		{
			return;
		}
		if (0f - m_scollView.transform.localPosition.x > m_lastCellX - 10f)
		{
			if (!m_leftArrow.gameObject.active)
			{
				m_leftArrow.gameObject.SetActiveRecursively(true);
			}
			m_rightArrow.gameObject.SetActiveRecursively(false);
		}
		else if (0f - m_scollView.transform.localPosition.x < m_firstCellX + 10f)
		{
			m_leftArrow.gameObject.SetActiveRecursively(false);
			if (!m_rightArrow.gameObject.active)
			{
				m_rightArrow.gameObject.SetActiveRecursively(true);
			}
		}
		else
		{
			if (!m_leftArrow.gameObject.active)
			{
				m_leftArrow.gameObject.SetActiveRecursively(true);
			}
			if (!m_rightArrow.gameObject.active)
			{
				m_rightArrow.gameObject.SetActiveRecursively(true);
			}
		}
		if (m_scollView.Pressed)
		{
			m_leftArrow.FadeOut(3f);
			m_rightArrow.FadeOut(3f);
		}
	}

	private IEnumerator HideCells(int currentIndex)
	{
		int count = areaList.cells.Count;
		int start = currentIndex - 2;
		int end = currentIndex + 2;
		if (start < 0)
		{
			start = 0;
		}
		if (end > count - 1)
		{
			end = count - 1;
		}
		float lastExitDelay = 0f;
		DungeonAreaCell curr = areaList.cells[currentIndex];
		TweenCell t = curr.GetComponent<TweenCell>();
		t.exitScale = Vector3.one * 1.5f;
		t.exitDuration = 0.1f;
		t.OnExit();
		lastExitDelay = t.exitDuration;
		for (int i = start; i <= end; i++)
		{
			if (i != currentIndex)
			{
				DungeonAreaCell cell = areaList.cells[i];
				TweenCell tc = cell.GetComponent<TweenCell>();
				tc.exitScale = Vector3.zero;
				tc.exitDuration = 0.25f;
				tc.OnExit();
				lastExitDelay = tc.exitDuration;
				yield return new MBWaitForSeconds(0.1f);   // PORT (CD-059)
			}
		}
		yield return new MBWaitForSeconds(lastExitDelay);   // PORT (CD-059)
		for (int j = 0; j < count; j++)
		{
			DungeonAreaCell cell2 = areaList.cells[j];
			cell2.hidden = true;
		}
	}

	private void OnCenterCell(DungeonAreaCell cell, bool immediate = false)
	{
		m_currentCell = cell;
		GameObject gameObject = areaList.transform.parent.gameObject;
		Vector3 localPosition = gameObject.transform.localPosition;
		localPosition.x = 0f - cell.transform.localPosition.x;
		if (immediate)
		{
			UIPanel component = gameObject.GetComponent<UIPanel>();
			Vector3 vector = localPosition - component.transform.localPosition;
			Vector4 clipRange = component.clipRange;
			clipRange.x -= vector.x;
			clipRange.y -= vector.y;
			component.clipRange = clipRange;
			component.transform.localPosition = localPosition;
		}
		else if (Mathf.Abs(localPosition.x - gameObject.transform.localPosition.x) >= cell.width / 4f)
		{
			m_spring = SpringPanel.Begin(gameObject, localPosition, 13f);
		}
	}

	private void OnButtonClick(GameObject source)
	{
	}

	private void OnDungeonAreaCellEvent(DungeonAreaCell cell)
	{
		areaList.SendMessage("OnClick", null, SendMessageOptions.DontRequireReceiver);
		if (!m_clicked)
		{
			m_clicked = true;
			if (cell.CheckIfLocked())
			{
				m_clicked = false;
			}
			else
			{
				StartCoroutine(DoSelectCell(cell));
			}
		}
	}

	private void OnResetClick()
	{
		m_clicked = false;
	}

	private IEnumerator DoSelectCell(DungeonAreaCell cell)
	{
		yield return null;
		OnCenterCell(cell);
		if ((bool)m_spring)
		{
			while (m_spring.enabled)
			{
				yield return null;
			}
		}
		eventTarget.SendMessage("OnDungeonAreaCellEvent", cell, SendMessageOptions.DontRequireReceiver);
	}
}
