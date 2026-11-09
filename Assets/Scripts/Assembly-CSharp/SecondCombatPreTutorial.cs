using System.Collections;
using UnityEngine;

public class SecondCombatPreTutorial : Tutorial
{
	private TutorialButtonFilter m_buttonFilter;

	private Action m_showFriendAction;

	protected override void OnEnable()
	{
		base.OnEnable();
		Messenger<NBUIMenuPanel>.AddListener("mainmenu_show_panel", OnShowPanel);
		Messenger<EventCell>.AddListener("dungeon_select_event", OnSelectEvent);
		Messenger.AddListener("select_allies", OnFriendsListReady);
		Messenger<FriendCell>.AddListener("dungeon_select_friend", OnFriendSelected);
		Messenger.AddListener("dungeon_enter", OnEnterDungeon);
		Messenger.AddListener("dungeon_tutorial_try_enter", OnTryEnterDungeon);
		Messenger.AddListener("dungeon_tutorial_try_fail", OnTryEnterDungeonFail);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Messenger<NBUIMenuPanel>.RemoveListener("mainmenu_show_panel", OnShowPanel);
		Messenger<EventCell>.RemoveListener("dungeon_select_event", OnSelectEvent);
		Messenger.RemoveListener("select_allies", OnFriendsListReady);
		Messenger<FriendCell>.RemoveListener("dungeon_select_friend", OnFriendSelected);
		Messenger.RemoveListener("dungeon_enter", OnEnterDungeon);
		Messenger.RemoveListener("dungeon_tutorial_try_enter", OnTryEnterDungeon);
		Messenger.RemoveListener("dungeon_tutorial_try_fail", OnTryEnterDungeonFail);
	}

	protected override void Awake()
	{
		base.Awake();
	}

	public override void Start()
	{
		base.Start();
		m_buttonFilter = base.gameObject.AddComponent<TutorialButtonFilter>();
		Dbg.Assert(m_buttonFilter);
	}

	protected override void OnPopupClose()
	{
		base.OnPopupClose();
	}

	private void OnShowPanel(NBUIMenuPanel panel)
	{
		if (panel is EventSelectPanel && TryAdvance("click_hunt"))
		{
			AnalyticsManager.FirstInstallEvents("tut_hunt");
		}
	}

	private void OnSelectEvent(EventCell p_cell)
	{
		if (TryAdvance("click_event"))
		{
			AnalyticsManager.FirstInstallEvents("tut_tutdungeon2");
		}
	}

	private void OnFriendSelected(FriendCell p_cell)
	{
		if (TryAdvance("click_friend"))
		{
			AnalyticsManager.FirstInstallEvents("tut_selectfriend");
		}
	}

	private void OnTryEnterDungeon()
	{
		if (TryAdvance("try_enter_dungeon"))
		{
			AnalyticsManager.FirstInstallEvents("tut_friendok");
		}
	}

	private void OnTryEnterDungeonFail()
	{
		RollbackAndDoAction(m_showFriendAction);
	}

	private void OnEnterDungeon()
	{
		if (TryAdvance("enter_dungeon"))
		{
			AnalyticsManager.FirstInstallEvents("tut_battleloading");
		}
	}

	private void OnFriendsListReady()
	{
		if (TryAdvance("friendlist_ready"))
		{
			AnalyticsManager.FirstInstallEvents("tut_selectfriend_loading");
		}
	}

	public override void DoAction(Action action)
	{
		UICamera.currentFilter = null;
		base.DoAction(action);
	}

	public override void ShowSubtitle(Action p_action)
	{
		base.ShowSubtitle(p_action);
		SingletonComponent<TutorialManager>.Instance.HidePointer();
		SingletonComponent<TutorialManager>.Instance.HideInstruction();
	}

	private void ShowFingerOnHuntButton(Action p_action)
	{
		HideSubtitle();
		HomePanel homePanel = MainMenu.Instance.currScreen.currentPanel as HomePanel;
		Dbg.Assert(homePanel.m_huntButton);
		Vector3 p_location = -Vector3.right * 20f - Vector3.forward * 20f + Vector3.down * 20f;
		SingletonComponent<TutorialManager>.Instance.ShowPointerWithDelay(p_location, PointingHand.Direction.DownRight, homePanel.m_huntButton.transform, 1f);
		SingletonComponent<TutorialManager>.Instance.ShowInstruction(p_action.text, InstructionBox.Anchor.Bottom);
		if ((bool)m_buttonFilter)
		{
			m_buttonFilter.m_pressable = homePanel.m_huntButton.gameObject;
			UICamera.currentFilter = m_buttonFilter;
		}
	}

	private void ShowFingerOnEvent(Action action)
	{
		HideSubtitle();
		EventSelectPanel eventSelectPanel = MainMenu.Instance.currScreen.currentPanel as EventSelectPanel;
		if (!eventSelectPanel)
		{
			return;
		}
		int num = eventSelectPanel.m_eventList.visibleCells.Count - 1;
		if (num < 0)
		{
			return;
		}
		EventCell eventCell = eventSelectPanel.m_eventList.visibleCells[num];
		if ((bool)eventCell)
		{
			Vector3 p_location = Vector3.up * -40f - Vector3.right * 20f - Vector3.forward * 45f;
			SingletonComponent<TutorialManager>.Instance.ShowPointerWithDelay(p_location, PointingHand.Direction.DownRight, eventCell.transform, 1f);
			SingletonComponent<TutorialManager>.Instance.ShowInstruction(action.text, InstructionBox.Anchor.Bottom);
			UIPopupController.LockUICamera("UI_Bar", true);
			if ((bool)m_buttonFilter)
			{
				m_buttonFilter.m_pressable = eventCell.gameObject;
				UICamera.currentFilter = m_buttonFilter;
			}
		}
	}

	private void ShowFingerOnFriend(Action action)
	{
		m_showFriendAction = action;
		HideSubtitle();
		FriendSelectPanel friendSelectPanel = MainMenu.Instance.currScreen.currentPanel as FriendSelectPanel;
		friendSelectPanel.scrollView.scale = Vector3.zero;
		int count = friendSelectPanel.m_friendListGrid.visibleCells.Count;
		if (count > 0)
		{
			Vector3 p_location = Vector3.right * 72f + Vector3.down * 35f + Vector3.back * 5f;
			FriendCell friendCell = friendSelectPanel.m_friendListGrid.visibleCells[0];
			Dbg.Assert(friendCell);
			friendCell.m_buttonShowDetails.GetComponent<Collider>().enabled = false;
			SingletonComponent<TutorialManager>.Instance.ShowPointerWithDelay(p_location, PointingHand.Direction.DownLeft, friendCell.transform, 1f);
			SingletonComponent<TutorialManager>.Instance.ShowInstruction(action.text, InstructionBox.Anchor.Bottom);
			UIPopupController.LockUICamera("UI_Bar", true);
			if ((bool)m_buttonFilter && (bool)friendCell)
			{
				m_buttonFilter.m_pressable = friendCell.gameObject;
				UICamera.currentFilter = m_buttonFilter;
			}
		}
	}

	private void ShowFingerOnFriendPopup(Action action)
	{
		StartCoroutine(DoShowFingerOnFriendPopup(action));
	}

	private IEnumerator DoShowFingerOnFriendPopup(Action action)
	{
		yield return null;
		Vector3 fingerPos = new Vector3(0f, 0f, -6f);
		UIPopupController.Popup popup = UIPopupController.Instance.currentPopup;
		if (popup != null)
		{
			GameObject popupGO = popup.m_popupObj;
			Transform attachTransform = null;
			if ((bool)popupGO)
			{
				attachTransform = popupGO.transform.FindInHierarchy("BTN_YES");
				fingerPos += Vector3.up * -58f + Vector3.right * 72f;
				Dbg.Assert(attachTransform);
				if (popup != null)
				{
					popup.m_onNo = null;
				}
				Transform noButton = popupGO.transform.FindInHierarchy("BTN_NO");
				if ((bool)noButton)
				{
					UIButton b = noButton.GetComponent<UIButton>();
					if ((bool)b)
					{
						b.isEnabled = false;
					}
				}
			}
			SingletonComponent<TutorialManager>.Instance.ShowPointerWithDelay(fingerPos, PointingHand.Direction.DownLeft, attachTransform, 1f);
		}
		SingletonComponent<TutorialManager>.Instance.ShowInstruction(action.text, InstructionBox.Anchor.Bottom);
		UIPopupController.LockUICamera("UI_Bar", true);
	}

	private void DoNothing()
	{
		SingletonComponent<TutorialManager>.Instance.HidePointer();
		SingletonComponent<TutorialManager>.Instance.HideInstruction();
		HideSubtitle();
	}

	private void End(Action action)
	{
		HideSubtitle();
		HideInstruction();
		SingletonComponent<TutorialManager>.Instance.HidePointer();
	}
}
