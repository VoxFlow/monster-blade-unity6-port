using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventSelectPanel : NBUIMenuPanel
{
	public UIAnchor m_bpDisplayTemplate;

	public BattlePointsDisplay m_bpDisplay;

	public EventGrid m_eventList;

	public UISprite container;

	public Component eventTarget;

	private SpringPanel m_spring;

	private bool m_clicked;

	private EventCell m_currentCell;

	private NBTimer m_countDownTimer;

	public EventCell currentCell
	{
		get
		{
			return m_currentCell;
		}
	}

	private void OnEnable()
	{
		Init();
	}

	private void OnDisable()
	{
		Reset();
	}

	public override void OnPop()
	{
		SingletonComponent<DungeonManager>.Instance.BattleType = Battle.Type.Max;
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

	public void ResetClicked()
	{
		m_clicked = false;
	}

	public override void Reset()
	{
		m_clicked = false;
		int count = m_eventList.cells.Count;
		for (int i = 0; i < count; i++)
		{
			EventCell eventCell = m_eventList.cells[i];
			eventCell.hidden = true;
		}
		if (m_bpDisplay != null)
		{
			Object.DestroyImmediate(m_bpDisplay.gameObject);
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
		if (MenuBackground3D.Instance != null)
		{
			MenuBackground3D.Instance.ShowDefaultBackGround();
		}
		List<DungeonEventStruct> allDungeonEvents = SingletonComponent<DungeonManager>.Instance.AllDungeonEvents;
		int num = allDungeonEvents.Count;
		if (num <= 0)
		{
			return;
		}
		m_eventList.Clear();
		TutorialManager instance = SingletonComponent<TutorialManager>.Instance;
		foreach (DungeonEventStruct item in allDungeonEvents)
		{
			if ((bool)instance && !instance.enabled && item.id.Contains("TUTORIAL"))
			{
				num--;
				continue;
			}
			m_eventList.AddData(item);
			EventCell eventCell = m_eventList.AddCell(0f, 0f);
			if (item.locked)
			{
				eventCell.SetLock(true, string.Empty);
			}
			eventCell.m_eventTarget = this;
			eventCell.hidden = true;
			eventCell.gameObject.AddComponent<TweenCell>();
		}
		float num2 = m_eventList.cells[0].GetComponent<Collider>().bounds.size.x / GLOBAL.UI_SCALE_FACTOR;
		float num3 = (float)(num - 1) * num2;
		float num4 = num3;
		if (num > 1)
		{
			num4 += GLOBAL.BASE_WIDTH * 1.5f;
		}
		container.transform.localScale = new Vector3(num4, 10f, 1f);
		container.transform.localPosition = new Vector3(num3 * 0.5f, 0f, 0f);
		if (m_eventList.cells.Count <= 0)
		{
			return;
		}
		DungeonEventStruct currentDungeonEvent = SingletonComponent<DungeonManager>.Instance.CurrentDungeonEvent;
		if (currentDungeonEvent == null)
		{
			return;
		}
		EventCell eventCell2 = null;
		if (SingletonComponent<DailyManager>.Instance != null && !string.IsNullOrEmpty(SingletonComponent<DailyManager>.Instance.UserJoinEventFromPromo))
		{
			foreach (EventCell cell in m_eventList.cells)
			{
				if (cell.data.id == SingletonComponent<DailyManager>.Instance.UserJoinEventFromPromo)
				{
					eventCell2 = cell;
					SingletonComponent<DailyManager>.Instance.UserJoinEventFromPromo = string.Empty;
					break;
				}
			}
		}
		else
		{
			eventCell2 = m_eventList.cells[m_eventList.cells.Count - 1];
		}
		DungeonEventStruct nextDungeonEvent = SingletonComponent<DungeonManager>.Instance.NextDungeonEvent;
		if (nextDungeonEvent != null)
		{
			foreach (EventCell cell2 in m_eventList.cells)
			{
				if (cell2.data == null || !(cell2.data.id == nextDungeonEvent.id))
				{
					continue;
				}
				eventCell2 = cell2;
				break;
			}
		}
		OnCenterCell(eventCell2, true);
		NBUIGridFX component = m_eventList.GetComponent<NBUIGridFX>();
		if ((bool)component)
		{
			component.OnScrollBarChanged(null);
		}
		StartCoroutine(ShowCells(eventCell2.cellIndex));
		OnUpdateCountDownTimer();
	}

	private void OnUpdateCountDownTimer()
	{
		if (null == m_countDownTimer)
		{
			m_countDownTimer = base.gameObject.AddComponent<NBTimer>();
		}
		if (!(null != m_countDownTimer))
		{
			return;
		}
		TutorialManager instance = SingletonComponent<TutorialManager>.Instance;
		bool flag = false;
		if (null != m_eventList)
		{
			int count = m_eventList.visibleCells.Count;
			if (instance != null && instance.enabled)
			{
				for (int i = 0; i < count; i++)
				{
					EventCell eventCell = m_eventList.visibleCells[i];
					eventCell.ShowCountDownTimer(false);
				}
			}
			else
			{
				for (int j = 0; j < count; j++)
				{
					EventCell eventCell2 = m_eventList.visibleCells[j];
					flag |= eventCell2.RefreshCountDownTimer();
				}
			}
		}
		if (flag)
		{
			m_countDownTimer.CountDown(60f, "OnUpdateCountDownTimer");
		}
	}

	private IEnumerator ShowCells(int currentIndex)
	{
		yield return null;
		int count = m_eventList.cells.Count;
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
			EventCell cell = m_eventList.cells[i];
			TweenCell tc = cell.GetComponent<TweenCell>();
			tc.Init();
			cell.hidden = false;
			tc.OnEnter();
			yield return new MBWaitForSeconds(0.1f);   // PORT (CD-059)
		}
		for (int j = 0; j < count; j++)
		{
			EventCell cell2 = m_eventList.cells[j];
			if (cell2.hidden)
			{
				cell2.hidden = false;
			}
		}
	}

	private IEnumerator HideCells(int currentIndex)
	{
		int count = m_eventList.cells.Count;
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
		EventCell curr = m_eventList.cells[currentIndex];
		TweenCell t = curr.GetComponent<TweenCell>();
		t.exitScale = Vector3.one * 1.5f;
		t.exitDuration = 0.1f;
		t.OnExit();
		lastExitDelay = t.exitDuration;
		for (int i = start; i <= end; i++)
		{
			if (i != currentIndex)
			{
				EventCell cell = m_eventList.cells[i];
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
			EventCell cell2 = m_eventList.cells[j];
			cell2.hidden = true;
		}
	}

	private void OnCenterCell(EventCell cell, bool immediate = false)
	{
		m_currentCell = cell;
		GameObject gameObject = m_eventList.transform.parent.gameObject;
		Vector3 localPosition = gameObject.transform.localPosition;
		int num = Mathf.Clamp(cell.cellIndex, 0, m_eventList.cells.Count - 1);
		localPosition.x = 0f - (float)num * (cell.width + m_eventList.hSpace);
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

	private void OnEventCellEvent(EventCell cell)
	{
		m_eventList.SendMessage("OnClick", null, SendMessageOptions.DontRequireReceiver);
		if (!m_clicked)
		{
			m_clicked = true;
			StartCoroutine(DoSelectCell(cell));
		}
	}

	private IEnumerator DoSelectCell(EventCell cell)
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
		if (!(SingletonComponent<PlayerDataManager>.Instance == null) && !(UIPopupController.Instance == null))
		{
			Messenger<EventCell>.Invoke("dungeon_select_event", cell);
			eventTarget.SendMessage("OnEventCellEvent", cell, SendMessageOptions.DontRequireReceiver);
		}
	}
}
