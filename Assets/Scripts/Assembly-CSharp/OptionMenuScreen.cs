using UnityEngine;

public class OptionMenuScreen : NBUIMenuScreen
{
	public enum ScreenState
	{
		HOME = 0
	}

	public UILabel m_nubeeIdButton;

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
			if (m_currState != value)
			{
				m_currState = value;
				if (m_currState == ScreenState.HOME)
				{
					backButton.SetActiveRecursively(false);
				}
				else
				{
					backButton.SetActiveRecursively(true);
				}
			}
			string empty = string.Empty;
			empty = ((m_currState != ScreenState.HOME) ? "options_header" : "options_header");
			if (MainMenu.Instance != null)
			{
				MainMenu.Instance.EnableHeader(true, empty);
			}
		}
	}

	public override void Start()
	{
		if (!(m_nubeeIdButton == null) && !(SingletonComponent<Services>.Instance == null))
		{
			m_nubeeIdButton.gameObject.SetActiveRecursively(true);
			m_nubeeIdButton.text = "Nubee ID: " + SingletonComponent<Services>.Instance.NubeeId;
		}
	}

	public override void Home()
	{
		if (menuPanels[0] != null)
		{
			menuPanels[0].show = true;
		}
		if (backButton != null)
		{
			backButton.SetActiveRecursively(false);
		}
	}

	public override void OnEnable()
	{
		base.OnEnable();
		Init();
	}

	protected override void Init()
	{
		if (MenuBackground3D.Instance != null)
		{
			MenuBackground3D.Instance.ShowDefaultBackGround();
		}
		if (MainMenu.Instance != null && backButton == null && MainMenu.Instance.m_sharedBackButton != null)
		{
			backButton = MainMenu.Instance.m_sharedBackButton.gameObject;
			m_backButton = MainMenu.Instance.m_sharedBackButton;
		}
		if (MainMenu.Instance.OptionMenuScreenJumpToState != ScreenState.HOME)
		{
			PushState(MainMenu.Instance.OptionMenuScreenJumpToState);
			MainMenu.Instance.OptionMenuScreenJumpToState = ScreenState.HOME;
			if ((bool)backButton)
			{
				backButton.SetActiveRecursively(false);
			}
		}
		else
		{
			PushState(ScreenState.HOME);
		}
	}

	private void PushState(ScreenState nextState)
	{
		if (nextState >= ScreenState.HOME && (int)nextState < menuPanels.Length && PushPanelWithTransitions(menuPanels[(int)nextState]))
		{
			m_nextState = nextState;
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

	private void OnAchievementButtonClick()
	{
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_secondary_ui_tap");
		Dbg.Log("[Info] OnAchievementButtonClick");
	}

	private void OnFacebookButtonClick()
	{
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_secondary_ui_tap");
		SingletonComponent<SaveData>.Instance.GameData.FbLikedUs = true;
		Application.OpenURL("https://www.facebook.com/pages/Monster-Blade/119059501574462");
		Dbg.Log("[Info] OnFacebookButtonClick");
	}

	private void OnSettingsButtonClick()
	{
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_secondary_ui_tap");
		UIPopupController.Instance.ShowBattlePausePopup("options_header", OnOkBtnPressed, OnCancelBtnPressed, null, false, BattlePausePopup.PauseTab.SETTINGS, BattlePausePopup.Mode.OPTION_MENU);
		Dbg.Log("[Info] OnSettingsButtonClick");
	}

	private void OnCreditsButtonClick()
	{
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_secondary_ui_tap");
		SingletonComponent<SaveData>.Instance.GameData.AppStoreRateUs = true;
		Application.OpenURL(GLOBAL.APP_STORE_LINK);
		Dbg.Log("[Info] OnCreditsButtonClick");
	}

	private void OnPromoCodeButtonClick()
	{
		if (null != MainMenu.Instance)
		{
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_secondary_ui_tap");
			MainMenu.Instance.ShowPromoCodePopup();
		}
	}

	private void OnOkBtnPressed()
	{
		if (!(SingletonComponent<SoundManager>.Instance.MusicVolume <= 0f) && !(SingletonComponent<SoundManager>.Instance.SfxVolume <= 0f))
		{
			MBPrefs.SetFloat("MS:MusicVolume", SingletonComponent<SoundManager>.Instance.MusicVolume);
			MBPrefs.SetFloat("MS:SfxVolume", SingletonComponent<SoundManager>.Instance.SfxVolume);
		}
		MBPrefs.SetInt("MS:IsMute", SingletonComponent<SoundManager>.Instance.IsMute ? 1 : 0);
	}

	private void OnCancelBtnPressed()
	{
	}
}
