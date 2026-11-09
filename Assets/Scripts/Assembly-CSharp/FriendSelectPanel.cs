using System.Collections;
using System.Collections.Generic;
using Models;
using ServiceCalls;
using UnityEngine;

public class FriendSelectPanel : NBUIMenuPanel
{
	public int m_maxFriendCount = 10;

	public FriendGrid m_friendListGrid;

	public GameObject eventSprite;

	public GameObject m_scrollBar;

	public UISprite m_container;

	private UIPopupController m_popupController;

	private SocialManager m_socialManager;

	private List<FriendAccountDetails> m_socialFriends;

	private List<PlayerData> m_friends;

	private FriendCell _selectedFriendCell;

	private FriendCellEvent m_friendCellEvent;

	private bool m_isBeforeBattle;

	private int m_currentPanel;

	public FriendCell.CellType m_friendCellType;

	private int m_retryCount;

	private int m_retryMax = 5;

	private string m_errorInfoKey = string.Empty;

	private ServiceResponseCodes m_errorCode = ServiceResponseCodes.FAILED;

	private List<string> m_bundleToNotUnload = new List<string>();

	private UIDraggablePanel m_scrollView;

	private bool m_isLoadingDungeon;

	public DungeonMenuScreen m_dungeonMenuScreen;

	public float m_timeToSwitch = 3f;

	private bool m_isFriendCellClicked;

	private bool m_isFromViewFriendInfo;

	private bool isEnteringFriendInfo;

	public UIDraggablePanel scrollView
	{
		get
		{
			return m_scrollView;
		}
	}

	private bool MonsterReadyForBattle
	{
		get
		{
			return SingletonComponent<PlayerDataManager>.Instance != null && SingletonComponent<PlayerDataManager>.Instance.IsMonsterLoaded;
		}
	}

	private void Init()
	{
		IsBeforeBattle();
		InitMembers();
		ServiceCallAtStart();
		if (MenuBackground3D.Instance != null)
		{
			MenuBackground3D.Instance.CurrentBackground.EnableRolling(false);
		}
	}

	private void IsBeforeBattle()
	{
		m_isBeforeBattle = true;
		if (MainMenu.Instance != null)
		{
			m_isBeforeBattle = MainMenu.Instance.currScreen is DungeonMenuScreen;
			Dbg.Log("FRIEND_SELECT_PANEL : Before Battle : " + m_isBeforeBattle);
		}
	}

	private void InitMembers()
	{
		m_bundleToNotUnload.Clear();
		m_popupController = UIPopupController.Instance;
		m_socialManager = SingletonComponent<SocialManager>.Instance;
		m_friends = new List<PlayerData>();
		if (m_isBeforeBattle)
		{
			m_friendCellType = FriendCell.CellType.FRIEND_SELECT;
		}
	}

	private void OnEnable()
	{
		m_retryCount = 0;
		Init();
	}

	private void OnDisable()
	{
		Reset();
	}

	public override void Reset()
	{
		if (m_friendListGrid != null)
		{
			m_friendListGrid.Clear();
		}
		m_friends.Clear();
		_selectedFriendCell = null;
		if (!isEnteringFriendInfo && SingletonComponent<InventoryManager>.Instance != null)
		{
			SingletonComponent<InventoryManager>.Instance.FreeTempItems();
		}
		isEnteringFriendInfo = false;
		if (m_scrollView != null)
		{
			m_scrollView.currentMomentum = Vector3.zero;
		}
	}

	private new void Awake()
	{
		m_scrollView = NGUITools.FindInParents<UIDraggablePanel>(m_friendListGrid.gameObject);
	}

	private void ServiceCallAtStart()
	{
		string p_headerLabel = string.Empty;
		m_popupController.ShowLoadingPopupNetWait();
		Vector3 localPosition = m_friendListGrid.gameObject.transform.localPosition;
		switch (m_friendCellType)
		{
		case FriendCell.CellType.FRIEND_LIST:
			m_socialManager.RefreshFriendsList(0, OnRefreshFriendListSuccess, OnRefreshListFail);
			p_headerLabel = "friendlist_header";
			break;
		case FriendCell.CellType.FRIEND_SELECT:
			if (m_isFromViewFriendInfo)
			{
				m_isFromViewFriendInfo = false;
				StartCoroutine(SkipRefreshAlliesList());
			}
			else
			{
				DungeonManager instance = SingletonComponent<DungeonManager>.Instance;
				TutorialManager instance2 = SingletonComponent<TutorialManager>.Instance;
				if (instance != null && instance.CurrentDungeonEvent != null && instance2 != null && instance2.NeedSpecificFriend(instance.CurrentDungeonEvent.id))
				{
					ShowTutorial2Friend();
				}
				else
				{
					m_socialManager.RefreshAlliesList(OnRefreshAlliesSuccess, OnRefreshListFail);
				}
			}
			p_headerLabel = "friendselect_header";
			break;
		case FriendCell.CellType.FRIEND_GIFT:
			m_socialManager.RefreshFriendsList(0, OnRefreshFriendListSuccess, OnRefreshListFail);
			UpdateGiftHeader();
			break;
		}
		m_friendListGrid.gameObject.transform.localPosition = localPosition;
		if (MainMenu.Instance != null)
		{
			MainMenu.Instance.EnableHeader(true, p_headerLabel);
		}
	}

	private void OnRefreshFriendListSuccess(ServiceResponseCodes p_code)
	{
		m_socialFriends = m_socialManager.GetFriendsList();
		if (m_socialFriends == null || m_socialFriends.Count == 0)
		{
			ShowHeaderInfo();
			m_scrollBar.SetActiveRecursively(false);
			m_popupController.ShowOKPopupMSG("common_info", "social_no_friends", MainMenu.Instance.currScreen.Home);
			m_popupController.HideLoadingPopup();
		}
		else
		{
			RefreshList();
		}
	}

	private void OnRefreshRequestFriendSuccess(ServiceResponseCodes p_code)
	{
		m_socialFriends = m_socialManager.GetPendingFriendsList();
		if (m_socialFriends == null || m_socialFriends.Count == 0)
		{
			ShowHeaderInfo();
			m_scrollBar.SetActiveRecursively(false);
			m_popupController.ShowOKPopupMSG("common_info", "social_no_friends_request");
			m_popupController.HideLoadingPopup();
		}
		else
		{
			RefreshList();
		}
	}

	private void OnRefreshAlliesSuccess(ServiceResponseCodes p_code)
	{
		m_socialFriends = m_socialManager.GetAlliesList(3);
		if (m_socialFriends == null || m_socialFriends.Count == 0)
		{
			ShowHeaderInfo();
			m_scrollBar.SetActiveRecursively(false);
			OnEnterDungeonYes();
			m_popupController.HideLoadingPopup();
		}
		else
		{
			RefreshList(true);
			Messenger.Invoke("select_allies");
		}
	}

	private IEnumerator SkipRefreshAlliesList()
	{
		yield return new MBWaitForSeconds(0.5f);   // PORT (CD-059)
		OnRefreshAlliesSuccess(ServiceResponseCodes.SUCCESS);
	}

	private void SetupDefaultCells(bool useCooldown)
	{
		foreach (PlayerData friend in m_friends)
		{
			if (useCooldown)
			{
				if (friend.FriendAssistCoolDown == -1 || friend.FriendAssistCoolDown == 0)
				{
					m_friendListGrid.AddData(friend);
					FriendCell friendCell = m_friendListGrid.AddCell(0f, 0f);
					friendCell.cellType = m_friendCellType;
					friendCell.eventTarget = this;
				}
			}
			else
			{
				m_friendListGrid.AddData(friend);
				FriendCell friendCell2 = m_friendListGrid.AddCell(0f, 0f);
				friendCell2.cellType = m_friendCellType;
				friendCell2.eventTarget = this;
			}
		}
		if (m_friendListGrid.visibleCells.Count <= 1)
		{
			m_scrollView.scale = Vector3.zero;
		}
	}

	private void SetupFriendSelectCells(bool useCooldown)
	{
		TutorialManager instance = SingletonComponent<TutorialManager>.Instance;
		if (instance == null || instance.IsCompleted(Tutorial.State.All))
		{
			m_friendListGrid.AddData(null);
			FriendCell friendCell = m_friendListGrid.AddCell(0f, 0f);
			friendCell.cellType = FriendCell.CellType.FRIEND_FB_INVITE;
			friendCell.eventTarget = this;
		}
		List<PlayerData> list = new List<PlayerData>();
		List<PlayerData> list2 = new List<PlayerData>();
		foreach (PlayerData friend in m_friends)
		{
			if (friend != null && (!useCooldown || friend.FriendAssistCoolDown == -1 || friend.FriendAssistCoolDown == 0))
			{
				if (friend.IsFriend)
				{
					list.Add(friend);
				}
				else
				{
					list2.Add(friend);
				}
			}
		}
		for (int count = list.Count; count > m_maxFriendCount; count = list.Count)
		{
			int index = Random.Range(0, count);
			list.RemoveAt(index);
		}
		foreach (PlayerData item in list)
		{
			m_friendListGrid.AddData(item);
			FriendCell friendCell2 = m_friendListGrid.AddCell(0f, 0f);
			friendCell2.cellType = m_friendCellType;
			friendCell2.eventTarget = this;
		}
		if (list2.Count > 0)
		{
			int index2 = Random.Range(0, list2.Count);
			PlayerData data = list2[index2];
			m_friendListGrid.AddData(data);
			FriendCell friendCell3 = m_friendListGrid.AddCell(0f, 0f);
			friendCell3.cellType = m_friendCellType;
			friendCell3.eventTarget = this;
		}
	}

	private void PopulateList(bool useCooldown = false)
	{
		if (m_friends == null || m_friends.Count <= 0)
		{
			OnEnterDungeonYes();
			m_popupController.HideLoadingPopup();
			m_scrollBar.SetActiveRecursively(false);
			return;
		}
		m_friendListGrid.Clear();
		if (m_friendCellType == FriendCell.CellType.FRIEND_SELECT)
		{
			SetupFriendSelectCells(useCooldown);
		}
		else
		{
			SetupDefaultCells(useCooldown);
		}
		UpdateContainer();
		m_scrollView.currentMomentum = Vector3.zero;
		m_popupController.HideLoadingPopup();
		m_scrollView.scale.x = ((m_friendListGrid.visibleCells.Count > 1) ? 1 : 0);
	}

	private void UpdateContainer()
	{
		if (m_friendListGrid == null)
		{
			return;
		}
		int count = m_friendListGrid.cells.Count;
		if (count > 0)
		{
			float num = m_friendListGrid.cells[0].width + m_friendListGrid.hSpace;
			float num2 = GLOBAL.BASE_WIDTH * 1.5f;
			float num3 = (float)count * num + num2;
			m_container.transform.localScale = new Vector3(num3, 10f, 1f);
			m_container.transform.localPosition = new Vector3(num3 * 0.5f, 0f, 0f);
			TutorialManager instance = SingletonComponent<TutorialManager>.Instance;
			bool flag = instance == null || instance.IsCompleted(Tutorial.State.All);
			if (m_friendListGrid.cells.Count > 1 && flag)
			{
				OnCenterCell(m_friendListGrid.cells[1]);
			}
			else
			{
				OnCenterCell(m_friendListGrid.cells[0]);
			}
			m_scrollView.UpdateScrollbars(true);
		}
	}

	private void OnCenterCell(FriendCell cell)
	{
		GameObject gameObject = m_friendListGrid.transform.parent.gameObject;
		Vector3 localPosition = gameObject.transform.localPosition;
		int num = Mathf.Clamp(cell.cellIndex, 0, m_friendListGrid.cells.Count - 1);
		localPosition.x = 0f - (float)num * (cell.width + m_friendListGrid.hSpace);
		UIPanel component = gameObject.GetComponent<UIPanel>();
		Vector3 vector = localPosition - component.transform.localPosition;
		Vector4 clipRange = component.clipRange;
		clipRange.x -= vector.x;
		clipRange.y -= vector.y;
		component.clipRange = clipRange;
		component.transform.localPosition = localPosition;
	}

	private void ShowTutorial2Friend()
	{
		m_friends.Clear();
		PlayerData playerData = new PlayerData(SingletonComponent<PlayerDataManager>.Instance.GetPlayerData());
		playerData.CharacterType = CharacterType.Friend;
		string[] fRIEND_ASSIST_TUTORIAL_ = GLOBAL.FRIEND_ASSIST_TUTORIAL_2;
		foreach (string itemID in fRIEND_ASSIST_TUTORIAL_)
		{
			ItemDetails p_item = SingletonComponent<InventoryManager>.Instance.CreateItem(itemID);
			playerData.Equip(p_item);
		}
		playerData.Name = SingletonComponent<NBLocalization>.Instance.Get(GLOBAL.FRIEND_ASSIST_TUTORIAL_2_NAME);
		m_friends.Add(playerData);
		Dbg.LogWarn("Friends count = " + m_friends.Count);
		ShowHeaderInfo();
		PopulateList();
		Messenger.Invoke("select_allies");
	}

	private void RefreshList(bool useCooldown = false)
	{
		m_friends.Clear();
		foreach (FriendAccountDetails socialFriend in m_socialFriends)
		{
			PlayerData playerData = m_socialManager.ServerFriendToPlayerData(socialFriend);
			if (playerData != null)
			{
				m_friends.Add(playerData);
			}
		}
		Dbg.LogWarn("Friends count = " + m_friends.Count);
		ShowHeaderInfo();
		PopulateList(useCooldown);
	}

	private void OnRefreshListFail(ServiceResponseCodes p_code)
	{
		Dbg.LogWarn("Refresh Friend List Failed. Error code : " + p_code);
		if (p_code == ServiceResponseCodes.EXCEPTION_USER_INVALID_DEVICE)
		{
			m_popupController.HideLoadingPopup();
			m_popupController.ShowInvalidDevice();
		}
		else
		{
			OnInitRetry();
		}
	}

	private void OnInitRetry()
	{
		Dbg.Log("Retry FriendListSelectPanel : " + m_retryCount + "/" + m_retryMax);
		m_retryCount++;
		if (m_retryCount < m_retryMax)
		{
			Init();
			return;
		}
		m_retryCount = 0;
		m_popupController.HideLoadingPopup();
		m_popupController.ShowNoNetRetry(OnInitRetry);
	}

	private void OnFriendCellEvent(FriendCellEvent evt)
	{
		if (m_isLoadingDungeon || evt == null)
		{
			return;
		}
		bool flag = m_retryCount > 0;
		if (m_friendCellEvent == null)
		{
			m_friendCellEvent = evt;
		}
		_selectedFriendCell = evt.cell;
		if (evt.type == FriendCellEventType.FRIEND_INVITE)
		{
			m_socialManager.InviteFriend(null, OnFBLogin, OnFBInviteFinished);
		}
		else if (_selectedFriendCell == null || _selectedFriendCell.data == null)
		{
			m_popupController.HideLoadingPopup();
			Dbg.LogError("No Cell data set for the friend.");
		}
		else
		{
			if (m_isFriendCellClicked)
			{
				return;
			}
			Dbg.Log(string.Concat("FRIEND_SELECT_PANEL : OnfriendCellEvent : ", evt.type, "\n", _selectedFriendCell.data));
			switch (evt.type)
			{
			case FriendCellEventType.FRIEND_SELECT:
				m_isFriendCellClicked = true;
				if (m_isBeforeBattle)
				{
					m_popupController.ShowFriendSelectConfirm(_selectedFriendCell.data, OnEnterDungeonYes, OnEnterDungeonCancel);
					Messenger<FriendCell>.Invoke("dungeon_select_friend", _selectedFriendCell);
				}
				else
				{
					Dbg.Log("FRIEND_SELECT_PANEL : OnfriendCellEvent -> ShowFriend Details ...");
					string txt5 = SingletonComponent<NBLocalization>.Instance.Get("see_friend_details");
					m_popupController.ShowSmallPopupMSG(txt5, OnFriendSelected, OnGenericCancel);
				}
				break;
			case FriendCellEventType.FRIEND_INFO:
			{
				Dbg.Log("Show Friend Details ... ");
				if (m_friendCellType == FriendCell.CellType.FRIEND_SELECT)
				{
					m_isFromViewFriendInfo = true;
				}
				string txt4 = SingletonComponent<NBLocalization>.Instance.Get("see_friend_details");
				m_isFriendCellClicked = true;
				m_popupController.ShowSmallPopupMSG(txt4, OnFriendSelected, OnGenericCancel);
				break;
			}
			case FriendCellEventType.FRIEND_ADD:
				break;
			case FriendCellEventType.FRIEND_REMOVE:
			{
				string format2 = SingletonComponent<NBLocalization>.Instance.Get("social_confirm_remove_friend");
				string txt2 = string.Format(format2, _selectedFriendCell.data.Name);
				if (flag)
				{
					OnFriendRemoved();
					break;
				}
				m_isFriendCellClicked = true;
				m_popupController.ShowSmallPopupMSG(txt2, OnFriendRemoved, OnGenericCancel);
				break;
			}
			case FriendCellEventType.FRIEND_ACCEPT:
				m_popupController.ShowLoadingPopupNetWait();
				m_socialManager.ApproveFriend(_selectedFriendCell.data.m_userData.nubeeId, OnFriendApprovedSuccess, OnFriendApprovedFail);
				break;
			case FriendCellEventType.FRIEND_REJECT:
			{
				string format3 = SingletonComponent<NBLocalization>.Instance.Get("social_reject_friend");
				string txt3 = string.Format(format3, _selectedFriendCell.data.Name);
				if (flag)
				{
					OnFriendRejected();
					break;
				}
				m_isFriendCellClicked = true;
				m_popupController.ShowSmallPopupMSG(txt3, OnFriendRejected, OnGenericCancel);
				break;
			}
			case FriendCellEventType.FRIEND_GIFT:
			{
				string format = SingletonComponent<NBLocalization>.Instance.Get("social_send_gift_friend");
				string txt = string.Format(format, _selectedFriendCell.data.Name);
				if (flag)
				{
					OnSendGift();
					break;
				}
				m_isFriendCellClicked = true;
				m_popupController.ShowSmallPopupMSG(txt, OnSendGift, OnGenericCancel);
				break;
			}
			}
		}
	}

	private void OnFBLogin(ServiceResponseCodes p_code)
	{
		Dbg.Log("[FriendSelectPanel::OnFBLogin] p_code: " + p_code);
		if (!(null == m_popupController))
		{
			switch (p_code)
			{
			case ServiceResponseCodes.SUCCESS:
				m_popupController.ShowLoadingPopupNetWait();
				break;
			case ServiceResponseCodes.FAILED:
				m_popupController.HideLoadingPopup();
				break;
			}
		}
	}

	private void OnFBInviteFinished(ServiceResponseCodes p_code)
	{
		Dbg.Log("[FriendSelectPanel::OnFBInviteFinished] p_code: " + p_code);
		if (null != m_popupController)
		{
			m_popupController.HideLoadingPopup();
		}
	}

	private void OnFriendSelected()
	{
		isEnteringFriendInfo = true;
		m_isFriendCellClicked = false;
		if (parentScreen != null && _selectedFriendCell != null)
		{
			parentScreen.SendMessage("OnFriendDetailsClick", _selectedFriendCell.data, SendMessageOptions.DontRequireReceiver);
		}
	}

	private void OnFriendRemoved()
	{
		m_isFriendCellClicked = false;
		if (_selectedFriendCell != null)
		{
			m_popupController.ShowLoadingPopupNetWait();
			m_socialManager.RemoveFriend(_selectedFriendCell.data.m_userData.nubeeId, OnFriendRemovedSucceed, OnFriendRemovedFail);
		}
	}

	private void OnFriendRejected()
	{
		m_isFriendCellClicked = false;
		if (_selectedFriendCell != null)
		{
			m_popupController.ShowLoadingPopupNetWait();
			m_socialManager.DenyFriend(_selectedFriendCell.data.m_userData.nubeeId, OnFriendRejectedSuccess, OnFriendRejectedFail);
		}
	}

	private void OnSendGift()
	{
		m_isFriendCellClicked = false;
		if (_selectedFriendCell != null)
		{
			m_popupController.ShowLoadingPopupNetWait();
			m_socialManager.SendGift(_selectedFriendCell.data.m_userData.nubeeId, OnSendGiftSuccess, OnSendGiftFail);
		}
	}

	private void OnAddFriend()
	{
		m_popupController.ShowAddFriendPopup();
	}

	private void OnFriendRemovedSucceed(ServiceResponseCodes p_code)
	{
		m_popupController.HideLoadingPopup();
		if (_selectedFriendCell != null)
		{
			string format = SingletonComponent<NBLocalization>.Instance.Get("social_remove_friend_success");
			string txt = string.Format(format, _selectedFriendCell.data.Name);
			m_popupController.ShowOKPopupMSG("common_info", txt);
			Dbg.Log("FRIEND_SELECT_PANEL : --> Friend Removed <-- : " + _selectedFriendCell.data.Name);
		}
		RemoveFriendClientSide(true);
	}

	private void OnFriendApprovedSuccess(ServiceResponseCodes p_code)
	{
		m_popupController.HideLoadingPopup();
		if (_selectedFriendCell != null)
		{
			string format = SingletonComponent<NBLocalization>.Instance.Get("social_approved_friend");
			string txt = string.Format(format, _selectedFriendCell.data.Name);
			m_popupController.ShowOKPopupMSG("common_info", txt);
		}
		RemoveFriendClientSide();
	}

	private void OnFriendRejectedSuccess(ServiceResponseCodes p_code)
	{
		m_popupController.HideLoadingPopup();
		if (_selectedFriendCell != null)
		{
			m_popupController.ShowOKPopupMSG("common_info", "social_rejected_friend_success");
			Dbg.Log("FRIEND_SELECT_PANEL : --> Friend Rejected <-- : " + _selectedFriendCell.data.Name);
		}
		RemoveFriendClientSide();
	}

	private void OnSendGiftSuccess(ServiceResponseCodes p_code)
	{
		UpdateGiftHeader();
		m_popupController.HideLoadingPopup();
		if (_selectedFriendCell != null)
		{
			m_popupController.ShowOKPopupMSG("common_info", "social_send_gift_success");
			Dbg.Log("FRIEND_SELECT_PANEL : --> Send Gift Success <-- : " + _selectedFriendCell.data.Name);
			ResetEventRetry();
		}
		MainMenu.Instance.UpdateSocialCounter();
		MainMenu.Instance.UpdateGachaCounter();
	}

	private void OnFriendRemovedFail(ServiceResponseCodes p_code)
	{
		m_errorInfoKey = "social_remove_friend_fail";
		m_errorCode = p_code;
		OnFriendCellEventRetry();
	}

	private void OnFriendApprovedFail(ServiceResponseCodes p_code)
	{
		m_errorInfoKey = string.Empty;
		m_errorCode = p_code;
		OnFriendCellEventRetry();
	}

	private void OnFriendRejectedFail(ServiceResponseCodes p_code)
	{
		m_errorInfoKey = "social_rejected_friend_fail";
		m_errorCode = p_code;
		OnFriendCellEventRetry();
	}

	private void OnSendGiftFail(ServiceResponseCodes p_code)
	{
		m_errorInfoKey = "social_send_gift_fail";
		m_errorCode = p_code;
		OnFriendCellEventRetry();
	}

	private void ResetEventRetry()
	{
		Dbg.Log("ResetEventRetry");
		_selectedFriendCell = null;
		m_friendCellEvent = null;
		m_retryCount = 0;
		m_errorInfoKey = string.Empty;
		m_errorCode = ServiceResponseCodes.FAILED;
	}

	private void OnFriendCellEventRetry()
	{
		string text = SingletonComponent<NBLocalization>.Instance.Get(m_errorInfoKey);
		Dbg.Log(string.Concat("OnFriendCellEventRetry : ", m_retryCount, "/", m_retryMax, "  with code : ", m_errorCode, "\n", _selectedFriendCell.data.Name, "\n", text));
		if (m_errorCode == ServiceResponseCodes.EXCEPTION_USER_INVALID_DEVICE)
		{
			ResetEventRetry();
			m_popupController.HideLoadingPopup();
			m_popupController.ShowInvalidDevice();
			return;
		}
		if (m_errorCode == ServiceResponseCodes.EXCEPTION_FRIEND_LIMIT_EXCEEDED)
		{
			m_popupController.HideLoadingPopup();
			m_popupController.ShowOKPopupMSG("common_info", "exceed_friend_limit");
			return;
		}
		if (m_errorCode == ServiceResponseCodes.EXCEPTION_USER_INVALID_CHARACTER)
		{
			m_popupController.HideLoadingPopup();
			m_popupController.ShowOKPopupMSG("common_info", "character_not_found");
			return;
		}
		if (m_errorCode == ServiceResponseCodes.EXCEPTION_NOT_ENOUGH_CURRENCY)
		{
			m_popupController.HideLoadingPopup();
			m_popupController.ShowOKPopupMSG("common_info", m_errorInfoKey);
			return;
		}
		m_retryCount++;
		if (m_retryCount < m_retryMax)
		{
			OnFriendCellEvent(m_friendCellEvent);
			return;
		}
		m_retryCount = 0;
		m_popupController.HideLoadingPopup();
		m_popupController.ShowNoNetRetryInfo(m_errorInfoKey, OnFriendCellEventRetry, ResetEventRetry);
	}

	private void OnEnterDungeonYes()
	{
		m_isFriendCellClicked = false;
		if (_selectedFriendCell != null && _selectedFriendCell.data != null)
		{
			SingletonComponent<PlayerDataManager>.Instance.SetFriendDataFromServer(_selectedFriendCell.data, null);
		}
		SingletonComponent<DungeonManager>.Instance.GenerateNewSessionID();
		EnterDungeon();
	}

	private void EnterDungeon()
	{
		DungeonManager instance = SingletonComponent<DungeonManager>.Instance;
		if (parentScreen is DungeonMenuScreen)
		{
			DungeonMenuScreen dungeonMenuScreen = parentScreen as DungeonMenuScreen;
			dungeonMenuScreen.EnableBackBtnCollider(false);
		}
		m_popupController.ShowLoadingPopupNetWait();
		UIPopupController.Instance.m_overlay.alpha = 0f;
		SingletonComponent<LeaderboardManager>.Instance.ClearDungeonScoreParams();
		if (instance.BattleType == Battle.Type.Dungeon)
		{
			SingletonComponent<TutorialManager>.Instance.enabled = false;
			SingletonComponent<TutorialManager>.Instance.currentState = Tutorial.State.None;
			instance.StartDungeonOnline(OnPrepareBattlesSuccess, OnPrepareBattlesFail);
		}
		else if (instance.BattleType == Battle.Type.DungeonEvent)
		{
			string id = instance.CurrentDungeonEvent.id;
			if (id.Contains("TUTORIAL"))
			{
				SingletonComponent<TutorialManager>.Instance.enabled = true;
				Tutorial.State currentState = Tutorial.State.None;
				switch (id)
				{
				case "TUTORIAL_1":
					currentState = Tutorial.State.Combat1;
					break;
				case "TUTORIAL_2":
					currentState = Tutorial.State.Combat2;
					break;
				case "TUTORIAL_3":
					currentState = Tutorial.State.Combat3;
					break;
				default:
					SingletonComponent<TutorialManager>.Instance.enabled = false;
					break;
				}
				SingletonComponent<TutorialManager>.Instance.currentState = currentState;
				Messenger.Invoke("dungeon_tutorial_try_enter");
			}
			instance.StartDungeonEventOnline(OnPrepareBattlesSuccess, OnPrepareBattlesFail, 0, string.Empty);
		}
		BackButtonMapping backButtonMapping = Object.FindObjectOfType(typeof(BackButtonMapping)) as BackButtonMapping;
		if (backButtonMapping != null)
		{
			backButtonMapping.enabled = false;
		}
	}

	private void OnEnterDungeonCancel()
	{
		m_isFriendCellClicked = false;
		_selectedFriendCell = null;
		UIPopupController.Instance.EnableUI();
	}

	private void OnGenericCancel()
	{
		m_isFriendCellClicked = false;
		UIPopupController.Instance.EnableUI();
	}

	private void OnPrepareBattlesSuccess(Services.ServiceHandle p_handle)
	{
		m_isLoadingDungeon = true;
		Messenger.Invoke("dungeon_enter");
		bool flag = false;
		if (SingletonComponent<TutorialManager>.Instance != null && SingletonComponent<TutorialManager>.Instance.enabled)
		{
			if (SingletonComponent<TutorialManager>.Instance.IsCompleted(Tutorial.State.AllCombats))
			{
				flag = true;
			}
		}
		else
		{
			flag = true;
		}
		if (flag)
		{
			SaveData.MsData gameData = SingletonComponent<SaveData>.Instance.GameData;
			if (!gameData.HasEnteredAnEvent && SingletonComponent<TutorialManager>.Instance != null && SingletonComponent<TutorialManager>.Instance.IsCompleted(Tutorial.State.All) && SingletonComponent<DungeonManager>.Instance.BattleType == Battle.Type.DungeonEvent)
			{
				gameData.HasEnteredAnEvent = true;
				gameData.SaveAchievementData();
			}
		}
		UIPopupController.Instance.HideLoadingPopup(false);
		StartCoroutine(DecrementStaminaThenGotoBattle());
	}

	private IEnumerator DecrementStaminaThenGotoBattle()
	{
		if (SingletonComponent<DungeonManager>.Instance.BattleType == Battle.Type.Dungeon)
		{
			MainMenu.Instance.DecreaseStaminaValue(SingletonComponent<DungeonManager>.Instance.CurrentDungeon.stamina);
		}
		else if (SingletonComponent<DungeonManager>.Instance.BattleType == Battle.Type.DungeonEvent)
		{
			if (SingletonComponent<DungeonManager>.Instance.CurrentDungeonEvent.stamina > 0)
			{
				MainMenu.Instance.DecreaseStaminaValue(SingletonComponent<DungeonManager>.Instance.CurrentDungeonEvent.stamina);
			}
			DecrementBattlePointThenGotoBattle();
		}
		SingletonComponent<SoundManager>.Instance.StopMusic(true);
		yield return new MBWaitForSeconds(1f);   // PORT (CD-059)
		SingletonComponent<LeaderboardManager>.Instance.SetLastRankInLocal(SingletonComponent<LeaderboardManager>.Instance.m_player.Rank);
		PlayerDataManager pdm = SingletonComponent<PlayerDataManager>.Instance;
		PlayerData pd = null;
		if (null != pdm)
		{
			pd = pdm.GetPlayerData();
			if (pd != null)
			{
				pd.m_isInLevelRangeForCurrentDungeon = false;
				if (null != SingletonComponent<NotifyManager>.Instance)
				{
					SingletonComponent<NotifyManager>.Instance.RegisterAllLocalNotifications();
				}
			}
		}
		SingletonComponent<GameManager>.Instance.LoadScene("PreloadBattle", true, false);
	}

	private void DecrementBattlePointThenGotoBattle()
	{
		PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		if (playerData != null && SingletonComponent<DungeonManager>.Instance != null)
		{
			if (playerData.m_userData.shield >= playerData.m_userData.maxShield)
			{
				playerData.m_userData.timeNextShield = GLOBAL.GAME_SETTINGS.shields_reset_time;
			}
			playerData.m_userData.shield -= SingletonComponent<DungeonManager>.Instance.DifficultyOfShieldCost(SingletonComponent<DungeonManager>.Instance.CurrentDungeonEvent.difficulty);
		}
		if (playerData.m_userData.shield < 0)
		{
			playerData.m_userData.shield = 0;
		}
	}

	private void UnloadMenuPopups()
	{
		List<string> list = new List<string>();
		list.Add("INFO_POPUP");
		list.Add("OK_POPUP");
		list.Add("SMALL");
		list.Add("SHOP_LIST_POPUP");
		list.Add("PRE_NEXT_BATTLE_POPUP");
		UIPopupController.Instance.UnloadExcept(list);
	}

	private void OnPrepareBattlesFail(Services.ServiceHandle p_handle, string p_error)
	{
		BackButtonMapping backButtonMapping = Object.FindObjectOfType(typeof(BackButtonMapping)) as BackButtonMapping;
		if (backButtonMapping != null)
		{
			backButtonMapping.enabled = true;
		}
		if (parentScreen is DungeonMenuScreen)
		{
			DungeonMenuScreen dungeonMenuScreen = parentScreen as DungeonMenuScreen;
			dungeonMenuScreen.EnableBackBtnCollider(true);
		}
		m_popupController.HideLoadingPopup();
		Dbg.LogWarn("OnPrepareBattlesFail: " + ServiceCallUtils.GetErrorMessage(p_handle));
		if (p_handle == null)
		{
			UIPopupController.Instance.ShowServerGenericError(OnStartDungeonErrorPopupClosed);
			return;
		}
		if (p_handle.result == Services.ConnectionResults.Timeout)
		{
			UIPopupController.Instance.ShowTimeoutForceRetry(EnterDungeon);
			return;
		}
		if (p_handle.IsNoInternet())
		{
			UIPopupController.Instance.ShowNoNetForceRetry(EnterDungeon);
			return;
		}
		switch ((ServiceResponseCodes)p_handle.nReturnCode)
		{
		case ServiceResponseCodes.EXCEPTION_DUNGEON_NOT_ENOUGH_STAMINA:
			UIPopupController.Instance.ShowOKPopupMSG("common_info", "dungeon_prompt_not_enough_stamina", OnStartDungeonErrorPopupClosed);
			if (null != MainMenu.Instance)
			{
				MainMenu.Instance.RefreshStamina();
			}
			break;
		case ServiceResponseCodes.EXCEPTION_DUNGEON_NOT_ENOUGH_SHIELD:
			UIPopupController.Instance.ShowInsufficientShield(OnStartDungeonErrorPopupClosed);
			break;
		default:
			UIPopupController.Instance.ShowServerGenericError(OnStartDungeonErrorPopupClosed, p_handle.nReturnCode);
			break;
		}
	}

	private void OnStartDungeonErrorPopupClosed()
	{
		TutorialManager instance = SingletonComponent<TutorialManager>.Instance;
		if (null != instance && instance.enabled)
		{
			Messenger.Invoke("dungeon_tutorial_try_fail");
		}
	}

	private void ShowHeaderInfo()
	{
		PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		if (m_friendCellType == FriendCell.CellType.FRIEND_LIST && playerData != null && playerData.m_userData.totalFriends < m_friends.Count)
		{
			playerData.m_userData.totalFriends = m_friends.Count;
		}
	}

	private void UpdateGiftHeader()
	{
		PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		string format = SingletonComponent<NBLocalization>.Instance.Get("friendgift_header");
		string p_headerLabel = string.Format(format, playerData.m_userData.giftGacha);
		if (MainMenu.Instance != null)
		{
			MainMenu.Instance.EnableHeader(true, p_headerLabel);
		}
	}

	private void RemoveFriendClientSide(bool _updateTotalFriend = false)
	{
		if (_selectedFriendCell == null)
		{
			return;
		}
		m_friends.Remove(_selectedFriendCell.data);
		m_friendListGrid.Remove(_selectedFriendCell);
		UpdateContainer();
		if (_updateTotalFriend)
		{
			PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
			if (playerData != null)
			{
				playerData.m_userData.totalFriends--;
				if (playerData.m_userData.totalFriends < m_friends.Count)
				{
					playerData.m_userData.totalFriends = m_friends.Count;
				}
			}
			ShowHeaderInfo();
		}
		ResetEventRetry();
	}

	private List<string> GetEquipedBundle(PlayerData _playerData)
	{
		List<string> list = new List<string>();
		if (_playerData == null)
		{
			return list;
		}
		string text = "Equiped Bundles:\n";
		Equipment.Data[] equipments = _playerData.Equipments;
		foreach (Equipment.Data data in equipments)
		{
			ItemDetails itemDetails = data.itemDetails;
			if (itemDetails != null && itemDetails.equipment.characterElement != null && !(SingletonComponent<ItemVisualManager>.Instance == null))
			{
				string text2 = SingletonComponent<ItemVisualManager>.Instance.ContainInBundle(itemDetails.BGAtlasName);
				text = text + text2 + "\n";
				list.Add(text2);
			}
		}
		Dbg.Log(text);
		return list;
	}
}
