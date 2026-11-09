using System.Collections.Generic;
using Models;
using UnityEngine;

public class SocialMenuScreen : NBUIMenuScreen
{
	public enum Choice
	{
		FRIENDLIST = 0,
		FRIENDREQUEST = 1,
		GIFT = 2,
		ADDFRIEND = 3,
		FBCONNECT = 4
	}

	public enum ScreenState
	{
		HOME = 0,
		FRIENDLIST = 1,
		FRIENDREQUESTLIST = 2,
		GIFTFRIENDLIST = 3,
		FRIEND_DETAILS = 4,
		MESSAGELIST = 5
	}

	public UILabel m_gachaGift;

	public UILabel m_notificationOnFbBtn;

	public UILabel m_pendingFriendRequestCounter;

	private ScreenState _currState;

	private int m_levelForGift = 15;

	public SocialMenuPanel m_socialPanel;

	private PlayerData m_playerData;

	public ScreenState currState
	{
		get
		{
			return _currState;
		}
		private set
		{
			_currState = value;
			backButton.SetActiveRecursively(_currState != ScreenState.HOME);
			string p_headerLabel = "social_header";
			bool p_enable = true;
			bool flag = true;
			float p_value = -101f;
			if (_currState == ScreenState.HOME)
			{
				UpdateRedIconNum();
				UpdatePendingFriendRequestNum();
			}
			else if (_currState == ScreenState.FRIEND_DETAILS)
			{
				p_headerLabel = "slayer_profile_header";
				flag = false;
				CharacterInfoPanel characterInfoPanel = menuPanels[4] as CharacterInfoPanel;
				if (null != characterInfoPanel && m_playerData != null)
				{
					characterInfoPanel.SetData(m_playerData);
				}
				p_value = -46f;
			}
			if (MainMenu.Instance != null)
			{
				MainMenu.Instance.EnableHeader(p_enable, p_headerLabel);
				MainMenu.Instance.ShowTopAndBottomBar(flag);
				MainMenu.Instance.MoveHeaderToTop(flag);
				if (null != backButton)
				{
					backButton.transform.SetLocalY(p_value);
				}
			}
		}
	}

	private bool MeetLevelRequirement
	{
		get
		{
			if (null == SingletonComponent<PlayerDataManager>.Instance || m_levelForGift == 0)
			{
				return false;
			}
			PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
			int num = playerData.Level / m_levelForGift + 1;
			int num2 = num * m_levelForGift;
			return num2 <= playerData.MaxLevel;
		}
	}

	private bool MeetGiftRequirement
	{
		get
		{
			if (null == SingletonComponent<PlayerDataManager>.Instance || null == SingletonComponent<SocialManager>.Instance)
			{
				return false;
			}
			PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
			bool flag = SingletonComponent<SocialManager>.Instance.IsSNSLogin();
			return playerData.m_userData.giftGacha > 0 && flag;
		}
	}

	public override void OnEnable()
	{
		base.OnEnable();
		if (!(SingletonComponent<PlayerDataManager>.Instance == null))
		{
			if (MainMenu.Instance != null && backButton == null && MainMenu.Instance.m_sharedBackButton != null)
			{
				backButton = MainMenu.Instance.m_sharedBackButton.gameObject;
				m_backButton = MainMenu.Instance.m_sharedBackButton;
			}
			if ((bool)backButton)
			{
				UIButtonMessage component = backButton.GetComponent<UIButtonMessage>();
				component.target = base.gameObject;
			}
			if (MenuBackground3D.Instance != null)
			{
				MenuBackground3D.Instance.ShowDefaultBackGround();
				MenuBackground3D.Instance.CurrentBackground.EnableRolling(false);
			}
		}
	}

	public override void OnDisable()
	{
		base.OnDisable();
		Reset();
	}

	protected override void Reset()
	{
		m_playerData = null;
	}

	protected override void Init()
	{
		PushState(ScreenState.HOME);
	}

	private void PushState(ScreenState nextState)
	{
		if (nextState >= ScreenState.HOME && (int)nextState < menuPanels.Length)
		{
			currState = nextState;
			PushPanel(menuPanels[(int)nextState]);
		}
	}

	private void PopState()
	{
		PopPanel();
		ScreenState currentPanelIndex = (ScreenState)GetCurrentPanelIndex();
		currState = currentPanelIndex;
	}

	private void FacebookLoginCallback(ServiceResponseCodes p_code)
	{
		switch (p_code)
		{
		case ServiceResponseCodes.SUCCESS:
			UIPopupController.Instance.ShowLoadingPopupNetWait();
			break;
		case ServiceResponseCodes.FAILED:
			UIPopupController.Instance.HideLoadingPopup();
			break;
		}
	}

	private void FacebookInviteFriendLoginCallback(ServiceResponseCodes p_code)
	{
		UIPopupController.Instance.HideLoadingPopup();
	}

	private void OnButtonClick(GameObject source)
	{
		if (source == backButton)
		{
			OnBack();
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_cancel");
			return;
		}
		UIButtonMessage component = source.GetComponent<UIButtonMessage>();
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_secondary_ui_tap");
		switch ((Choice)component.id)
		{
		case Choice.FRIENDLIST:
		{
			FriendSelectPanel friendSelectPanel2 = menuPanels[1] as FriendSelectPanel;
			friendSelectPanel2.m_friendCellType = FriendCell.CellType.FRIEND_LIST;
			PushState(ScreenState.FRIENDLIST);
			break;
		}
		case Choice.FRIENDREQUEST:
		{
			FriendSelectPanel friendSelectPanel = menuPanels[2] as FriendSelectPanel;
			friendSelectPanel.m_friendCellType = FriendCell.CellType.FRIEND_REQUEST;
			PushState(ScreenState.FRIENDREQUESTLIST);
			break;
		}
		case Choice.ADDFRIEND:
			if (SingletonComponent<SocialManager>.Instance != null)
			{
				SingletonComponent<SocialManager>.Instance.InviteFriend(null, FacebookLoginCallback, FacebookInviteFriendLoginCallback);
			}
			else
			{
				Dbg.LogError("SocialManager.Instance is null!");
			}
			break;
		case Choice.GIFT:
			EnterGiftScreen();
			break;
		case Choice.FBCONNECT:
			SingletonComponent<SocialManager>.Instance.InviteFriend(null, null, UpdatePanelButtons);
			break;
		}
	}

	private void UpdatePanelButtons(ServiceResponseCodes p_code)
	{
		if (!(m_socialPanel == null))
		{
			m_socialPanel.EnableButtonsByFBLoginStatus();
			UpdateRedIconNum();
		}
	}

	public void AddFriendRequestFriendListSuccess(ServiceResponseCodes p_code)
	{
		UIPopupController.Instance.HideLoadingPopup();
		if (SingletonComponent<SocialManager>.Instance != null)
		{
			List<FriendAccountDetails> friendsList = SingletonComponent<SocialManager>.Instance.GetFriendsList();
			PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
			if (playerData != null)
			{
				Dbg.Log(string.Format("User has {0} friend(s), limit number is {1}", friendsList.Count, playerData.m_FriendLimit));
				if (UIPopupController.Instance != null)
				{
					if (friendsList.Count >= playerData.m_FriendLimit)
					{
						UIPopupController.Instance.ShowOKPopupMSG("common_info", "exceed_friend_limit");
					}
					else
					{
						UIPopupController.Instance.ShowAddFriendPopup();
					}
				}
				else
				{
					Dbg.LogError("UIPopupController.Instance is null!!");
				}
			}
			else
			{
				Dbg.LogError("Unable to get player data");
			}
		}
		else
		{
			Dbg.LogError("SocialManager.Instance is null!!");
		}
	}

	public void AddFriendRequestFriendListFailed(ServiceResponseCodes p_code)
	{
		UIPopupController.Instance.HideLoadingPopup();
	}

	private void EnterGiftScreen()
	{
		if (MeetGiftRequirement)
		{
			FriendSelectPanel friendSelectPanel = menuPanels[3] as FriendSelectPanel;
			friendSelectPanel.m_friendCellType = FriendCell.CellType.FRIEND_GIFT;
			PushState(ScreenState.GIFTFRIENDLIST);
		}
		else if (MeetLevelRequirement)
		{
			PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
			int num = playerData.Level / m_levelForGift + 1;
			int num2 = num * m_levelForGift;
			string format = SingletonComponent<NBLocalization>.Instance.Get("social_no_gift_point");
			string txt = string.Format(format, num2);
			UIPopupController.Instance.ShowOKPopupMSG("social_no_gift_point_header", txt);
		}
		else
		{
			UIPopupController.Instance.ShowOKPopupMSG("social_no_gift_point_header", "social_cannot_send_gift");
		}
	}

	private void OnFriendDetailsClick(PlayerData data)
	{
		m_playerData = data;
		PushState(ScreenState.FRIEND_DETAILS);
	}

	private void OnBack()
	{
		PopState();
	}

	public override void UpdateRedIconNum()
	{
		Dbg.Log("UpdateRedIconNum");
		PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		bool flag = SingletonComponent<SocialManager>.Instance.IsSNSLogin();
		if (m_gachaGift != null && playerData.m_userData != null)
		{
			Dbg.Log("Showing number of gift: " + playerData.m_userData.giftGacha);
			if (playerData.m_userData.giftGacha <= 0 || !flag)
			{
				m_gachaGift.cachedTransform.parent.gameObject.SetActiveRecursively(false);
			}
			else if (menuPanels[0] != null && menuPanels[0].enabled)
			{
				m_gachaGift.cachedTransform.parent.gameObject.SetActiveRecursively(true);
				m_gachaGift.text = playerData.m_userData.giftGacha.ToString();
			}
		}
		else
		{
			Dbg.LogWarn("m_gachaGift or playerData.m_userData is null, unable to update gift red icon number");
			if (m_gachaGift != null && m_gachaGift.transform.parent != null)
			{
				m_gachaGift.transform.parent.gameObject.SetActiveRecursively(false);
			}
		}
		if (playerData.m_userData.giftGacha <= 0)
		{
			m_gachaGift.cachedTransform.parent.gameObject.SetActiveRecursively(false);
		}
		if (m_notificationOnFbBtn != null)
		{
			Dbg.LogWarn("Showing FB notification button.");
			if (!flag && playerData.m_userData.giftGacha > 0)
			{
				m_notificationOnFbBtn.cachedTransform.parent.gameObject.SetActiveRecursively(true);
				m_notificationOnFbBtn.text = playerData.m_userData.giftGacha.ToString();
			}
			else
			{
				m_notificationOnFbBtn.cachedTransform.parent.gameObject.SetActiveRecursively(false);
			}
		}
		UpdatePendingFriendRequestNum();
		if (MainMenu.Instance != null && MainMenu.Instance.m_gachaGiftCounter != null)
		{
			MainMenu.Instance.UpdateFreeTicketsCounter(MainMenu.Instance.m_gachaGiftCounter, playerData.m_userData.giftGacha);
		}
	}

	public void UpdatePendingFriendRequestNum()
	{
		Dbg.Log("UpdatePendingFriendRequestNum");
		if (SingletonComponent<SocialManager>.Instance != null && SingletonComponent<PlayerDataManager>.Instance != null)
		{
			PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
			int num = 0;
			num = ((playerData.m_PendingFriendRequest <= SingletonComponent<SocialManager>.Instance.GetPendingFriendsList().Count) ? SingletonComponent<SocialManager>.Instance.GetPendingFriendsList().Count : playerData.m_PendingFriendRequest);
			Dbg.Log("tempPendingNum: " + num);
			if (num <= 0)
			{
				m_pendingFriendRequestCounter.cachedTransform.parent.gameObject.SetActiveRecursively(false);
			}
			else if (menuPanels[0] != null && menuPanels[0].enabled)
			{
				m_pendingFriendRequestCounter.cachedTransform.gameObject.SetActiveRecursively(true);
				m_pendingFriendRequestCounter.text = num.ToString();
			}
		}
		else
		{
			Dbg.LogWarn("SocialManager.Instance is null, unable to update the number of friend request pending");
		}
	}
}
