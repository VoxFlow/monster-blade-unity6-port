using UnityEngine;

public class LeaderboardMenuScreen : NBUIMenuScreen
{
	private enum ScreenState
	{
		HOME = 0,
		RANK = 1,
		VIEW_PLAYER_DETAIL = 2
	}

	private bool m_isGlobal = true;

	private ScreenState m_currState;

	private ScreenState m_nextState;

	private ScreenState currState
	{
		get
		{
			return m_currState;
		}
		set
		{
			if (m_currState == value)
			{
				return;
			}
			m_currState = value;
			string text = string.Empty;
			if (m_currState == ScreenState.HOME)
			{
				backButton.SetActiveRecursively(false);
			}
			else if (m_currState == ScreenState.RANK)
			{
				backButton.SetActiveRecursively(true);
				text = "leaderboard_header";
				if ((bool)MainMenu.Instance.m_sharedBackButton)
				{
					UIButtonMessage component = MainMenu.Instance.m_sharedBackButton.GetComponent<UIButtonMessage>();
					if (component != null)
					{
						component.target = base.gameObject;
						component.functionName = "OnButtonClick";
					}
				}
			}
			else if (m_currState == ScreenState.VIEW_PLAYER_DETAIL)
			{
				backButton.SetActiveRecursively(true);
				text = "slayer_profile_header";
				if ((bool)MainMenu.Instance.m_sharedBackButton)
				{
					UIButtonMessage component2 = MainMenu.Instance.m_sharedBackButton.GetComponent<UIButtonMessage>();
					if (component2 != null)
					{
						component2.target = base.gameObject;
						component2.functionName = "OnBackBtnClickToLeaderboard";
					}
				}
			}
			if (!string.IsNullOrEmpty(text))
			{
				MainMenu.Instance.EnableHeader(true, text);
			}
		}
	}

	public override void OnEnable()
	{
		SingletonComponent<LeaderboardManager>.Instance.m_LeaderboardMenuScreenRef = this;
		base.OnEnable();
		Init();
		backButton.transform.localPosition = new Vector3(backButton.transform.localPosition.x, -49f, backButton.transform.localPosition.z);
		Dbg.Log("Showing leaderboard, move the back button up, pos: " + backButton.transform.localPosition);
	}

	public override void OnDisable()
	{
		if ((bool)backButton)
		{
			Vector3 localPosition = backButton.transform.localPosition;
			localPosition.y = -101f;
			backButton.transform.localPosition = localPosition;
			Dbg.LogEx("Disabling leaderboard, move the back button down to original pos: {0}", Dbg.NBLogType.COMMON_LOG, backButton.transform.localPosition.ToString());
		}
	}

	public override void Start()
	{
		base.Start();
	}

	protected override void Init()
	{
		if (MainMenu.Instance != null && backButton == null && MainMenu.Instance.m_sharedBackButton != null)
		{
			backButton = MainMenu.Instance.m_sharedBackButton.gameObject;
			m_backButton = MainMenu.Instance.m_sharedBackButton;
		}
		PushState(ScreenState.RANK);
	}

	private void PushState(ScreenState nextState)
	{
		if (nextState >= ScreenState.HOME && (int)nextState < menuPanels.Length)
		{
			_currentPanelInTransition = null;
			if (PushPanelWithTransitions(menuPanels[(int)nextState]))
			{
				m_nextState = nextState;
			}
		}
	}

	public override void PushPanel(NBUIMenuPanel newPanel)
	{
		base.PushPanel(newPanel);
		currState = m_nextState;
	}

	private void PopState(bool showPrevious = true)
	{
		PopPanelWithTransitions(showPrevious);
		ScreenState currentPanelIndex = (ScreenState)GetCurrentPanelIndex();
		currState = currentPanelIndex;
	}

	public void ShowMainPanel()
	{
		PushState(ScreenState.HOME);
	}

	public void ShowRankPanel(bool p_isGlobal)
	{
		m_isGlobal = p_isGlobal;
		PushState(ScreenState.RANK);
		if (menuPanels[1] != null)
		{
			LeaderboardRankPanel component = menuPanels[1].GetComponent<LeaderboardRankPanel>();
			if (component != null)
			{
				component.SetRemainTimeText(SingletonComponent<LeaderboardManager>.Instance.GetRemainingTime());
				component.SetShowGlobal(p_isGlobal);
				UIButtonMessage component2 = backButton.GetComponent<UIButtonMessage>();
				component2.target = menuPanels[1].gameObject;
				backButton.gameObject.SetActiveRecursively(true);
			}
		}
	}

	public void ShowRankPlayerDetails(PlayerData data, int p_rank)
	{
		if (data != null)
		{
			Dbg.Log("[Info] Show player details : " + data.Name);
			PushState(ScreenState.VIEW_PLAYER_DETAIL);
			CharacterInfoPanel characterInfoPanel = menuPanels[2] as CharacterInfoPanel;
			characterInfoPanel.SetData(data);
		}
	}

	public void OnBackBtnClickToLeaderboard()
	{
		UIPopupController.Instance.ShowLoadingPopupNetWait();
		if (MainMenu.Instance != null)
		{
			MainMenu.Instance.EnableHeader(true, "leaderboard_header");
			MainMenu.Instance.MoveHeaderToTop(false);
		}
		SingletonComponent<LeaderboardManager>.Instance.GetGlobalLeaderboard(0);
	}

	public void OnButtonClick(GameObject p_btnGo)
	{
		if (p_btnGo == backButton)
		{
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_cancel");
			MainMenu.Instance.ShowTopAndBottomBar(true);
			if ((bool)MainMenu.Instance.m_sharedBackButton)
			{
				UIButtonMessage component = MainMenu.Instance.m_sharedBackButton.GetComponent<UIButtonMessage>();
				component.target = null;
				MainMenu.Instance.m_sharedBackButton.gameObject.SetActiveRecursively(false);
			}
			MainMenu.Instance.ShowHomePage();
			return;
		}
		LeaderboardMainPanel component2 = menuPanels[0].GetComponent<LeaderboardMainPanel>();
		if (component2 != null)
		{
			if (m_isGlobal)
			{
				component2.OnGlobalBtnClick();
			}
			else
			{
				component2.OnSocialBtnClick();
			}
		}
	}
}
