using System.Collections;
using UnityEngine;

public class GachaPremiumTutorial : Tutorial
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
		Messenger<GachaPremiumOptionsPanel.PremiumOptions>.AddListener("gacha_premiumtype_selected", OnGachaPremiumTypeSelected);
		Messenger.AddListener("gacha_done", OnGachaDone);
		Messenger<MainMenu.Menu>.AddListener("mainmenu_screen_enter", OnEnterScreen);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Messenger<GachaScreen.Choice>.RemoveListener("gacha_menu_selected", OnGachaMenuSelected);
		Messenger.RemoveListener("gacha_show_iteminfo", OnGachaShowItemInfo);
		Messenger<GachaPremiumOptionsPanel.PremiumOptions>.RemoveListener("gacha_premiumtype_selected", OnGachaPremiumTypeSelected);
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
		LockPremiumPanelButton(0, true);
		LockPremiumPanelButton(1, true);
		m_buttonFilter = base.gameObject.AddComponent<TutorialButtonFilter>();
		Dbg.Assert(m_buttonFilter);
		base.Start();
	}

	private void OnGachaMenuSelected(GachaScreen.Choice chest)
	{
		if (chest == GachaScreen.Choice.PREMIUM && TryAdvance("click_epic_treasure"))
		{
			AnalyticsManager.FirstInstallEvents("tut_epictreasure");
		}
	}

	private void OnGachaPremiumTypeSelected(GachaPremiumOptionsPanel.PremiumOptions type)
	{
		if (type == GachaPremiumOptionsPanel.PremiumOptions.GACHA)
		{
			TryAdvance("click_epic_treasure_1");
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
			m_premiumPanel = gachaScreen.menuPanels[2] as GachaPremiumOptionsPanel;
			Dbg.Assert(m_premiumPanel);
			m_premiumPanel.LockButton(0, m_premiumPanelButtonLocks[0]);
			m_premiumPanel.LockButton(1, m_premiumPanelButtonLocks[1]);
			MainMenu.Instance.m_sharedBackButton.isLocked = m_bBackButtonLocked;
			TryAdvance("click_gacha_menu");
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

	private void ShowSubtitleGiveDiamond(Action action)
	{
		if ((bool)SingletonComponent<GachaManager>.Instance && (bool)SingletonComponent<NBLocalization>.Instance)
		{
			int singlePremiumPrice = SingletonComponent<GachaManager>.Instance.SinglePremiumPrice;
			string format = SingletonComponent<NBLocalization>.Instance.Get(action.text);
			action.text = string.Format(format, singlePremiumPrice);
		}
		base.ShowSubtitle(action);
		LockHomePanelButton(1, false);
	}

	private void ShowFingerToEpicTreasure(Action action)
	{
		HideSubtitle();
		if (MainMenu.Instance != null && MainMenu.Instance.currScreen is GachaScreen)
		{
			MainMenu.Instance.currScreen.m_backButton.isLocked = true;
			m_bBackButtonLocked = true;
		}
		Dbg.Assert(m_homePanel.buttons[1]);
		Vector3 p_location = Vector3.up * -25f + Vector3.right * 65f - Vector3.forward * 45f;
		SingletonComponent<TutorialManager>.Instance.ShowPointerWithDelay(p_location, PointingHand.Direction.DownLeft, m_homePanel.buttons[1].transform, 0.2f);
		SingletonComponent<TutorialManager>.Instance.ShowInstruction(action.text, InstructionBox.Anchor.Bottom);
		UIPopupController.LockUICamera("UI_Bar", true);
		if ((bool)m_buttonFilter)
		{
			m_buttonFilter.m_pressable = m_homePanel.buttons[1];
			UICamera.currentFilter = m_buttonFilter;
		}
	}

	private void ShowFingerToEpicTreasureOption1(Action action)
	{
		HideSubtitle();
		HideInstruction();
		Dbg.Assert(m_premiumPanel.buttons[1]);
		LockPremiumPanelButton(1, false);
		if ((bool)m_buttonFilter)
		{
			m_buttonFilter.m_pressable = m_premiumPanel.buttons[1];
			UICamera.currentFilter = m_buttonFilter;
		}
		StartCoroutine(DoShowFingerToEpicTreasureOption1(action));
	}

	private IEnumerator DoShowFingerToEpicTreasureOption1(Action action)
	{
		yield return new MBWaitForSeconds(1.5f);   // PORT (CD-059)
		Vector3 fingerPos = Vector3.up * -250f + Vector3.right * 40f - Vector3.forward * 45f;
		SingletonComponent<TutorialManager>.Instance.ShowPointerWithDelay(fingerPos, PointingHand.Direction.DownLeft, m_premiumPanel.buttons[1].transform, 0.2f);
		SingletonComponent<TutorialManager>.Instance.ShowInstruction(action.text);
		UIPopupController.LockUICamera("UI_Bar", true);
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

	private void End(Action action)
	{
		HideSubtitle();
		Reset();
		Messenger<State>.Invoke("tutorial_completed", State.Gacha2);
	}
}
