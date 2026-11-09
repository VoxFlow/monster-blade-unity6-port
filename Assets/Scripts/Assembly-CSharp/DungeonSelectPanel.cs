using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonSelectPanel : NBUIMenuPanel
{
	public DungeonGrid dungeonList;

	public UISprite container;

	public Component eventTarget;

	public SetInfoPanel m_SetInfoPanelTemplate;

	private SetInfoPanel m_setInfoPanel;

	private CityDbStruct m_currentCity;

	private UIDraggablePanel m_scrollView;

	private SpringPanel m_spring;

	private bool m_clicked;

	private DungeonCell m_currentCell;

	private void OnEnable()
	{
		Messenger.AddListener("dungeon_select_cancel", ResetClick);
		Init();
	}

	private void OnDisable()
	{
		if (m_setInfoPanel != null)
		{
			Object.DestroyImmediate(m_setInfoPanel.gameObject);
			m_setInfoPanel = null;
		}
		Messenger.RemoveListener("dungeon_select_cancel", ResetClick);
		Reset();
	}

	private new void Awake()
	{
		m_scrollView = dungeonList.transform.parent.GetComponent<UIDraggablePanel>();
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
		int count = dungeonList.cells.Count;
		for (int i = 0; i < count; i++)
		{
			DungeonCell dungeonCell = dungeonList.cells[i];
			dungeonCell.hidden = true;
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
		if (SingletonComponent<DungeonManager>.Instance == null)
		{
			return;
		}
		CityDbStruct currentCity = SingletonComponent<DungeonManager>.Instance.CurrentCity;
		MenuBackground3D.Instance.ShowBackground(currentCity.element);
		if (m_currentCity == null || currentCity.id != m_currentCity.id)
		{
			List<DungeonManager.DungeonStruct> allUnlockedDungeonsOfCity = SingletonComponent<DungeonManager>.Instance.GetAllUnlockedDungeonsOfCity(currentCity);
			int count = allUnlockedDungeonsOfCity.Count;
			if (count <= 0)
			{
				return;
			}
			dungeonList.Clear();
			int num = 0;
			float elementIconCutRatio = 0.931f;
			foreach (DungeonManager.DungeonStruct item in allUnlockedDungeonsOfCity)
			{
				num = (item.number = num + 1);
				dungeonList.AddData(item);
				DungeonCell dungeonCell = dungeonList.AddCell(0f, 0f);
				if (item.dungeon.boss_id == currentCity.dungeonBosses[currentCity.dungeonBosses.Count - 1])
				{
					Dbg.LogEx("This dungeon's boss id is same with the City's boss id: {0} , show the boss icon", Dbg.NBLogType.COMMON_LOG, item.dungeon.boss_id);
					dungeonCell.m_IsLastInCity = true;
					dungeonCell.SetElementIcon(currentCity.iconSpriteName);
					dungeonCell.SetElementIconCutRatio(elementIconCutRatio);
				}
				dungeonCell.m_eventTarget = this;
				dungeonCell.hidden = true;
				dungeonCell.gameObject.AddComponent<TweenCell>();
			}
			float num2 = dungeonList.cells[0].GetComponent<Collider>().bounds.size.x / GLOBAL.UI_SCALE_FACTOR;
			float num3 = (float)(count - 1) * num2;
			float num4 = num3;
			if (count > 1)
			{
				num4 += GLOBAL.BASE_WIDTH * 1.5f;
			}
			container.transform.localScale = new Vector3(num4, 10f, 1f);
			container.transform.localPosition = new Vector3(num3 * 0.5f, 0f, 0f);
			m_currentCity = currentCity;
		}
		if (dungeonList.cells.Count <= 0)
		{
			return;
		}
		ResetPanelPosition();
		DungeonCell dungeonCell2 = dungeonList.cells[dungeonList.cells.Count - 1];
		DungeonDbStruct nextDungeon = SingletonComponent<DungeonManager>.Instance.NextDungeon;
		if (nextDungeon != null)
		{
			foreach (DungeonCell cell in dungeonList.cells)
			{
				if (cell.data == null || cell.data.dungeon == null || !(cell.data.dungeon.id == nextDungeon.id))
				{
					continue;
				}
				dungeonCell2 = cell;
				break;
			}
		}
		OnCenterCell(dungeonCell2, true);
		NBUIGridFX component = dungeonList.GetComponent<NBUIGridFX>();
		if ((bool)component)
		{
			component.OnScrollBarChanged(null);
		}
		StartCoroutine(ShowCells(dungeonCell2.cellIndex));
	}

	private void CreateAndShowSetInfoPanel(SetInformation p_setInfo)
	{
		GameObject gameObject = Object.Instantiate(m_SetInfoPanelTemplate.gameObject, Vector3.zero, Quaternion.identity) as GameObject;
		if (gameObject != null)
		{
			gameObject.transform.parent = base.transform;
			gameObject.transform.localPosition = new Vector3(0f, -140f, 0f);
			gameObject.transform.localScale = Vector3.one;
			m_setInfoPanel = gameObject.GetComponent<SetInfoPanel>();
			if (m_setInfoPanel != null)
			{
				m_setInfoPanel.Show();
				m_setInfoPanel.SetSetInfo(p_setInfo);
			}
		}
	}

	private void ResetPanelPosition()
	{
		m_scrollView.DisableSpring();
		Vector3 localPosition = m_scrollView.transform.localPosition;
		localPosition.x = 0f;
		UIPanel component = m_scrollView.GetComponent<UIPanel>();
		Vector3 vector = localPosition - component.transform.localPosition;
		Vector4 clipRange = component.clipRange;
		clipRange.x -= vector.x;
		clipRange.y -= vector.y;
		component.clipRange = clipRange;
		component.transform.localPosition = localPosition;
	}

	private IEnumerator ShowCells(int currentIndex)
	{
		yield return null;
		int count = dungeonList.cells.Count;
		int start = currentIndex - 3;
		int end = currentIndex + 3;
		if (start < 0)
		{
			start = 0;
		}
		if (end > count - 1)
		{
			end = count - 1;
		}
		for (int i = start; i <= end; i++)
		{
			DungeonCell cell = dungeonList.cells[i];
			cell.SetLock();
		}
		for (int j = start; j <= end; j++)
		{
			DungeonCell cell2 = dungeonList.cells[j];
			TweenCell tc = cell2.GetComponent<TweenCell>();
			tc.Init();
			cell2.hidden = false;
			tc.OnEnter();
			yield return new MBWaitForSeconds(0.1f);   // PORT (CD-059)
		}
		for (int k = 0; k < count; k++)
		{
			DungeonCell cell3 = dungeonList.cells[k];
			cell3.SetLock();
			if (cell3.hidden)
			{
				cell3.hidden = false;
			}
		}
	}

	private IEnumerator HideCells(int currentIndex)
	{
		int count = dungeonList.cells.Count;
		int start = currentIndex - 3;
		int end = currentIndex + 3;
		if (start < 0)
		{
			start = 0;
		}
		if (end > count - 1)
		{
			end = count - 1;
		}
		float lastExitDelay = 0f;
		DungeonCell curr = dungeonList.cells[currentIndex];
		TweenCell t = curr.GetComponent<TweenCell>();
		t.exitScale = Vector3.one * 1.5f;
		t.exitDuration = 0.1f;
		t.OnExit();
		lastExitDelay = t.exitDuration;
		for (int i = start; i <= end; i++)
		{
			if (i != currentIndex)
			{
				DungeonCell cell = dungeonList.cells[i];
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
			DungeonCell cell2 = dungeonList.cells[j];
			cell2.hidden = true;
		}
	}

	private void OnCenterCell(DungeonCell cell, bool immediate = false)
	{
		m_currentCell = cell;
		GameObject gameObject = dungeonList.transform.parent.gameObject;
		Vector3 localPosition = gameObject.transform.localPosition;
		int num = Mathf.Clamp(cell.cellIndex, 0, dungeonList.cells.Count - 1);
		localPosition.x = 0f - (float)num * (cell.width + dungeonList.hSpace);
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
		else if (Mathf.Abs(localPosition.x - gameObject.transform.localPosition.x) >= cell.width)
		{
			m_spring = SpringPanel.Begin(gameObject, localPosition, 13f);
		}
	}

	private void OnButtonClick(GameObject source)
	{
	}

	private void OnDungeonCellEvent(DungeonCell cell)
	{
		dungeonList.SendMessage("OnClick", null, SendMessageOptions.DontRequireReceiver);
		if (!m_clicked)
		{
			m_clicked = true;
			if (cell.CheckIfLocked())
			{
				m_clicked = false;
				return;
			}
			Messenger.Invoke("dungeon_select");
			StartCoroutine(DoSelectCell(cell));
		}
	}

	private void ResetClick()
	{
		m_clicked = false;
	}

	private IEnumerator DoSelectCell(DungeonCell cell)
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
		eventTarget.SendMessage("OnDungeonCellEvent", cell, SendMessageOptions.DontRequireReceiver);
	}
}
