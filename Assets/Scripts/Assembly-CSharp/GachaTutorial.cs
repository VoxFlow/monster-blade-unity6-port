using UnityEngine;

public class GachaTutorial : Tutorial
{
	private StandardMenuPanel m_homePanel;

	private GachaPremiumOptionsPanel m_premiumPanel;

	private bool[] m_homePanelButtonLocks;

	private bool[] m_premiumPanelButtonLocks;

	private bool m_bBackButtonLocked;

	private TutorialButtonFilter m_buttonFilter;

	protected override void OnEnable()
	{
		base.OnEnable();
		Messenger<GachaScreen.Choice>.AddListener("gacha_menu_selected", OnGachaMenuSelected);
		Messenger.AddListener("gacha_show_iteminfo", OnGachaShowItemInfo);
		Messenger.AddListener("gacha_done", OnGachaDone);
		Messenger<MainMenu.Menu>.AddListener("mainmenu_screen_enter", OnEnterScreen);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Messenger<GachaScreen.Choice>.RemoveListener("gacha_menu_selected", OnGachaMenuSelected);
		Messenger.RemoveListener("gacha_show_iteminfo", OnGachaShowItemInfo);
		Messenger.RemoveListener("gacha_done", OnGachaDone);
		Messenger<MainMenu.Menu>.RemoveListener("mainmenu_screen_enter", OnEnterScreen);
	}

	private void LockHomePanelButton(int nIndex, bool bLock)
	{
		m_homePanelButtonLocks[nIndex] = bLock;
		if (m_homePanel != null)
		{
			m_homePanel.LockButton(nIndex, bLock);
		}
	}

	private void LockPremiumPanelButton(int nIndex, bool bLock)
	{
		m_premiumPanelButtonLocks[nIndex] = bLock;
		if (m_premiumPanel != null)
		{
			m_premiumPanel.LockButton(nIndex, bLock);
		}
	}

	public override void Start()
	{
		base.Start();
		Dbg.Assert(MainMenu.Instance);
		Dbg.Assert(UIPopupController.Instance);
		if (MainMenu.Instance.currScreen is GachaScreen)
		{
			GachaScreen gachaScreen = MainMenu.Instance.currScreen as GachaScreen;
			Dbg.Assert(gachaScreen);
			m_homePanel = gachaScreen.menuPanels[0] as StandardMenuPanel;
			m_premiumPanel = gachaScreen.menuPanels[2] as GachaPremiumOptionsPanel;
		}
		m_homePanelButtonLocks = new bool[3];
		m_homePanelButtonLocks[0] = false;
		m_homePanelButtonLocks[1] = false;
		m_homePanelButtonLocks[2] = false;
		m_premiumPanelButtonLocks = new bool[2];
		m_premiumPanelButtonLocks[0] = false;
		m_premiumPanelButtonLocks[1] = false;
		LockHomePanelButton(0, true);
		LockHomePanelButton(1, true);
		LockHomePanelButton(2, true);
		m_buttonFilter = base.gameObject.AddComponent<TutorialButtonFilter>();
		Dbg.Assert(m_buttonFilter);
	}

	private void OnGachaMenuSelected(GachaScreen.Choice chest)
	{
		if (chest == GachaScreen.Choice.FRIENDSHIP && TryAdvance("click_social_gacha"))
		{
			AnalyticsManager.FirstInstallEvents("tut_commontreasure");
		}
	}

	private void OnGachaShowItemInfo()
	{
		TryAdvance("gacha_show_iteminfo");
	}

	private void OnGachaDone()
	{
		TryAdvance("gacha_done");
	}

	private void OnEnterScreen(MainMenu.Menu menu)
	{
		if (menu == MainMenu.Menu.Treasure)
		{
			Dbg.Assert(MainMenu.Instance);
			Dbg.Assert(MainMenu.Instance.currScreen);
			GachaScreen gachaScreen = MainMenu.Instance.currScreen as GachaScreen;
			Dbg.Assert(gachaScreen);
			m_homePanel = gachaScreen.menuPanels[0] as StandardMenuPanel;
			Dbg.Assert(m_homePanel);
			m_homePanel.LockButton(0, m_homePanelButtonLocks[0]);
			m_homePanel.LockButton(1, m_homePanelButtonLocks[1]);
			m_homePanel.LockButton(2, m_homePanelButtonLocks[2]);
			m_premiumPanel = gachaScreen.menuPanels[2] as GachaPremiumOptionsPanel;
			Dbg.Assert(m_premiumPanel);
			m_premiumPanel.LockButton(0, m_premiumPanelButtonLocks[0]);
			m_premiumPanel.LockButton(1, m_premiumPanelButtonLocks[1]);
			MainMenu.Instance.m_sharedBackButton.isLocked = m_bBackButtonLocked;
			if (TryAdvance("click_gacha_menu"))
			{
				AnalyticsManager.FirstInstallEvents("tut_treasure");
			}
		}
		else
		{
			m_homePanel = null;
			m_premiumPanel = null;
		}
	}

	public override void DoAction(Action action)
	{
		UICamera.currentFilter = null;
		base.DoAction(action);
	}

	public override void ShowSubtitle(Action action)
	{
		base.ShowSubtitle(action);
		SingletonComponent<TutorialManager>.Instance.HidePointer();
		SingletonComponent<TutorialManager>.Instance.HideInstruction();
	}

	private void ShowFingerGoToGacha(Action action)
	{
		HideSubtitle();
		Dbg.Assert(MainMenu.Instance.bottomBar);
		MainMenu.Instance.bottomBar.UnlockMenu(MainMenu.Menu.Treasure);
		SingletonComponent<TutorialManager>.Instance.ShowPointerAtBottomMenu(MainMenu.Menu.Treasure, 0.2f);
		SingletonComponent<TutorialManager>.Instance.ShowInstruction(action.text);
		if ((bool)m_buttonFilter)
		{
			m_buttonFilter.m_pressable = MainMenu.Instance.bottomBar.m_buttons[5].gameObject;
			UICamera.currentFilter = m_buttonFilter;
		}
	}

	private void ShowSubtitleAndDisableChest(Action action)
	{
		base.ShowSubtitle(action);
	}

	private void ShowSubtitleAndEnableChest(Action action)
	{
		HideSubtitle();
		LockHomePanelButton(0, false);
		Vector3 vector = new Vector3(360f, 180f, 1f);
		Dbg.Assert(m_homePanel.buttons[0]);
		vector = Vector3.up * -25f + Vector3.right * 60f - Vector3.forward * 45f;
		Dbg.Assert(UIPopupController.Instance.Panel);
		SingletonComponent<TutorialManager>.Instance.ShowPointerWithDelay(vector, PointingHand.Direction.DownLeft, m_homePanel.buttons[0].transform, 0.2f);
		SingletonComponent<TutorialManager>.Instance.ShowInstruction(action.text, InstructionBox.Anchor.Bottom);
		UIPopupController.LockUICamera("UI_Bar", true);
		if ((bool)m_buttonFilter)
		{
			m_buttonFilter.m_pressable = m_homePanel.buttons[0];
			UICamera.currentFilter = m_buttonFilter;
		}
	}

	private void ShowSubtitleAndDisableIcons(Action action)
	{
		base.ShowSubtitle(action);
		LockPremiumPanelButton(0, true);
		LockPremiumPanelButton(1, true);
	}

	private void ShowSubtitleAndEnableIcons(Action action)
	{
		base.ShowSubtitle(action);
		if (MainMenu.Instance != null && MainMenu.Instance.currScreen is GachaScreen)
		{
			MainMenu.Instance.currScreen.m_backButton.isLocked = true;
			m_bBackButtonLocked = true;
		}
		LockPremiumPanelButton(1, false);
		Vector3 vector = new Vector3(-292f, 130f, 1f);
		vector = Vector3.up * 20f - Vector3.right * 20f - Vector3.forward * 45f;
		Dbg.Assert(m_premiumPanel != null);
		SingletonComponent<TutorialManager>.Instance.ShowPointerWithDelay(vector, PointingHand.Direction.DownRight, m_premiumPanel.buttons[1].transform, 0.2f);
	}

	private void DoNothing()
	{
		SingletonComponent<TutorialManager>.Instance.HidePointer();
		HideInstruction();
		HideSubtitle();
	}

	private void Reset()
	{
		if (MainMenu.Instance != null && MainMenu.Instance.currScreen is GachaScreen)
		{
			MainMenu.Instance.currScreen.m_backButton.isLocked = false;
			m_bBackButtonLocked = false;
		}
		LockHomePanelButton(0, false);
		LockHomePanelButton(1, false);
		LockHomePanelButton(2, false);
		LockPremiumPanelButton(0, false);
		LockPremiumPanelButton(1, false);
	}

	private void GachaEnd(Action action)
	{
		HideSubtitle();
		Reset();
		Messenger<State>.Invoke("tutorial_completed", State.Gacha);
	}
}
