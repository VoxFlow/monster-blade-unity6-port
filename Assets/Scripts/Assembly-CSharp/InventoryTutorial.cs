using System.Collections.Generic;
using UnityEngine;

public class InventoryTutorial : Tutorial
{
	private InventoryCell m_cell;

	private Action m_pointToItemAction;

	private bool m_hasEquipped;

	private bool[] m_menuPanelsEnabled;

	private NBUIMenuPanel m_menuPanel;

	private TutorialButtonFilter m_buttonFilter;

	protected override void OnEnable()
	{
		base.OnEnable();
		Messenger<CharacterMenuScreen.Menu>.AddListener("character_menu_selected", OnCharacterMenuSelected);
		Messenger.AddListener("inventory_cancel", OnInventoryCanceled);
		Messenger<InventoryCell>.AddListener("inventory_item_selected", OnItemSelected);
		Messenger<ItemDetails>.AddListener("character_item_equipped", OnItemEquipped);
		Messenger<MainMenu.Menu>.AddListener("mainmenu_screen_enter", OnEnterScreen);
		Messenger<NBUIMenuPanel>.AddListener("mainmenu_show_panel", OnShowPanel);
		Messenger.AddListener("inventory_ready", OnInventoryReady);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Messenger<CharacterMenuScreen.Menu>.RemoveListener("character_menu_selected", OnCharacterMenuSelected);
		Messenger.RemoveListener("inventory_cancel", OnInventoryCanceled);
		Messenger<InventoryCell>.RemoveListener("inventory_item_selected", OnItemSelected);
		Messenger<ItemDetails>.RemoveListener("character_item_equipped", OnItemEquipped);
		Messenger<MainMenu.Menu>.RemoveListener("mainmenu_screen_enter", OnEnterScreen);
		Messenger<NBUIMenuPanel>.RemoveListener("mainmenu_show_panel", OnShowPanel);
		Messenger.RemoveListener("inventory_ready", OnInventoryReady);
	}

	private void EnableMenuPanelButton(int nIndex, bool b)
	{
		m_menuPanelsEnabled[nIndex] = b;
		if (m_menuPanel != null)
		{
			m_menuPanel.EnableButton(nIndex, b);
		}
	}

	public override void Start()
	{
		base.Start();
		TryAdvance("click");
		m_menuPanelsEnabled = new bool[6];
		if (MainMenu.Instance != null && MainMenu.Instance.currScreen is CharacterMenuScreen)
		{
			m_menuPanel = MainMenu.Instance.currScreen.menuPanels[0];
		}
		EnableMenuPanelButton(0, false);
		EnableMenuPanelButton(1, false);
		EnableMenuPanelButton(2, false);
		EnableMenuPanelButton(3, false);
		EnableMenuPanelButton(4, false);
		EnableMenuPanelButton(5, false);
		m_buttonFilter = base.gameObject.AddComponent<TutorialButtonFilter>();
		Dbg.Assert(m_buttonFilter);
		InventoryManager instance = SingletonComponent<InventoryManager>.Instance;
		if (null != instance)
		{
			List<ItemDetails> list = new List<ItemDetails>(instance.GetInventory.Values);
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				ItemDetails itemDetails = list[i];
				itemDetails.isNew = true;
			}
		}
	}

	private void OnCharacterMenuSelected(CharacterMenuScreen.Menu menu)
	{
		switch (menu)
		{
		case CharacterMenuScreen.Menu.BAG:
			if (TryAdvance("click_bag"))
			{
				AnalyticsManager.FirstInstallEvents("tut_bag");
			}
			break;
		case CharacterMenuScreen.Menu.CHARACTER:
			TryAdvance("click_stats");
			break;
		}
	}

	private void OnInventoryReady()
	{
		TryAdvance("inventory_ready");
	}

	private void OnInventoryCanceled()
	{
		if (TryAdvance("click_cancel"))
		{
			AnalyticsManager.FirstInstallEvents("tut_equipback");
		}
	}

	private void OnFriendsListReady()
	{
		if (!TryAdvance("click_event"))
		{
		}
	}

	private void OnItemSelected(InventoryCell cell)
	{
		if (cell == m_cell)
		{
			if (TryAdvance("click_item"))
			{
				AnalyticsManager.FirstInstallEvents("tut_bag1stitem");
			}
		}
		else if (!m_hasEquipped)
		{
			RollbackAndDoAction(m_pointToItemAction);
		}
	}

	private void OnItemEquipped(ItemDetails item)
	{
		if (TryAdvance("click_equip"))
		{
			AnalyticsManager.FirstInstallEvents("tut_equip");
			m_hasEquipped = true;
		}
	}

	private void OnEnterScreen(MainMenu.Menu menu)
	{
		if (menu == MainMenu.Menu.Character)
		{
			m_menuPanel = MainMenu.Instance.currScreen.menuPanels[0];
			m_menuPanel.EnableButton(0, m_menuPanelsEnabled[0]);
			m_menuPanel.EnableButton(1, m_menuPanelsEnabled[1]);
			m_menuPanel.EnableButton(2, m_menuPanelsEnabled[2]);
			m_menuPanel.EnableButton(3, m_menuPanelsEnabled[3]);
			m_menuPanel.EnableButton(4, m_menuPanelsEnabled[4]);
			m_menuPanel.EnableButton(5, m_menuPanelsEnabled[5]);
			if (TryAdvance("click_town_menu"))
			{
				AnalyticsManager.FirstInstallEvents("tut_character");
			}
		}
		else
		{
			m_menuPanel = null;
		}
		if (menu == MainMenu.Menu.Home && TryAdvance("click_dungeon"))
		{
			AnalyticsManager.FirstInstallEvents("tut_home");
		}
	}

	private void OnShowPanel(NBUIMenuPanel panel)
	{
		if (panel is EventSelectPanel)
		{
			if (!TryAdvance("click_hunt"))
			{
			}
		}
		else if (panel is CharacterInfoPanel)
		{
			if (TryAdvance("show_stats"))
			{
				AnalyticsManager.FirstInstallEvents("tut_stats");
			}
		}
		else if (panel is StandardMenuPanel)
		{
			if (!TryAdvance("click_back"))
			{
			}
		}
		else if (!(panel is FriendSelectPanel))
		{
		}
	}

	private void OnFriendSelected(FriendCell p_cell)
	{
		if (!TryAdvance("click_friend"))
		{
		}
	}

	private void OnEnterDungeon()
	{
		TryAdvance("enter_dungeon");
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

	private void DoNothing()
	{
		SingletonComponent<TutorialManager>.Instance.HidePointer();
		SingletonComponent<TutorialManager>.Instance.HideInstruction();
	}

	private void ShowFingerGoToTown(Action action)
	{
		HideSubtitle();
		MainMenu.Instance.bottomBar.UnlockMenu(MainMenu.Menu.Character);
		SingletonComponent<TutorialManager>.Instance.ShowPointerAtBottomMenu(MainMenu.Menu.Character, 0.2f);
		SingletonComponent<TutorialManager>.Instance.ShowInstruction(action.text);
		if ((bool)m_buttonFilter)
		{
			m_buttonFilter.m_pressable = MainMenu.Instance.bottomBar.m_buttons[1].gameObject;
			UICamera.currentFilter = m_buttonFilter;
		}
	}

	private void ShowSubtitleAndPointBag(Action action)
	{
		HideSubtitle();
		EnableMenuPanelButton(1, true);
		Vector3 p_location = Vector3.zero;
		StandardMenuPanel standardMenuPanel = m_menuPanel as StandardMenuPanel;
		if ((bool)standardMenuPanel && (bool)standardMenuPanel.buttons[1])
		{
			p_location = standardMenuPanel.buttons[1].transform.localPosition;
			p_location.x -= 20f;
			p_location.y += 20f;
			p_location.z = -10f;
		}
		Dbg.Assert(MainMenu.Instance.m_panel);
		SingletonComponent<TutorialManager>.Instance.ShowPointerWithDelay(p_location, PointingHand.Direction.Down, MainMenu.Instance.m_panel.transform, 0.2f);
		SingletonComponent<TutorialManager>.Instance.ShowInstruction(action.text, InstructionBox.Anchor.Bottom);
		UIPopupController.LockUICamera("UI_Bar", true);
		if ((bool)m_buttonFilter)
		{
			m_buttonFilter.m_pressable = standardMenuPanel.buttons[1];
			UICamera.currentFilter = m_buttonFilter;
		}
	}

	private void ShowFingerOnInventoryItem(Action action)
	{
		m_pointToItemAction = action;
		HideSubtitle();
		Vector3 p_location = new Vector3(-405f, 200f, 1f);
		int count = MainMenu.Instance.inventory.grid.visibleCells.Count;
		if (count >= 1)
		{
			m_cell = MainMenu.Instance.inventory.grid.visibleCells[0];
			p_location = m_cell.transform.position / GLOBAL.UI_SCALE_FACTOR;
			p_location += Vector3.up * -10f + Vector3.right * 10f;
			p_location.z = 1f;
		}
		MainMenu.Instance.inventory.buttonSort.isEnabled = false;
		MainMenu.Instance.inventory.buttonBuySlots.isEnabled = false;
		MainMenu.Instance.inventory.buttonOk.isEnabled = false;
		MainMenu.Instance.inventory.buttonCancel.isEnabled = false;
		SingletonComponent<TutorialManager>.Instance.ShowPointerWithDelay(p_location, PointingHand.Direction.DownRight, UIPopupController.Instance.Panel, 0.2f);
		SingletonComponent<TutorialManager>.Instance.ShowInstruction(action.text, InstructionBox.Anchor.Bottom);
	}

	private void ShowFingerOnEquipButton(Action action)
	{
		HideSubtitle();
		SingletonComponent<TutorialManager>.Instance.HidePointer();
		UIButton buttonOk = MainMenu.Instance.inventory.buttonOk;
		Dbg.Assert(buttonOk);
		buttonOk.isEnabled = true;
		Vector3 p_location = Vector3.down * 50f + Vector3.left * 50f + Vector3.back * 25f;
		SingletonComponent<TutorialManager>.Instance.ShowPointerWithDelay(p_location, PointingHand.Direction.DownRight, buttonOk.transform, 0.2f);
		SingletonComponent<TutorialManager>.Instance.ShowInstruction(action.text);
		if ((bool)m_buttonFilter)
		{
			m_buttonFilter.m_pressable = buttonOk.gameObject;
			UICamera.currentFilter = m_buttonFilter;
		}
	}

	private void ShowFingerOnCancelButton(Action action)
	{
		HideSubtitle();
		SingletonComponent<TutorialManager>.Instance.HidePointer();
		UIButton buttonCancel = MainMenu.Instance.inventory.buttonCancel;
		UIButton buttonSort = MainMenu.Instance.inventory.buttonSort;
		Dbg.Assert(buttonCancel);
		Dbg.Assert(buttonSort);
		buttonCancel.isEnabled = true;
		buttonSort.isEnabled = true;
		Vector3 p_location = Vector3.down * 50f + Vector3.left * 50f + Vector3.back * 25f;
		SingletonComponent<TutorialManager>.Instance.ShowPointerWithDelay(p_location, PointingHand.Direction.DownRight, buttonCancel.transform, 0.2f);
		SingletonComponent<TutorialManager>.Instance.ShowInstruction(action.text);
		if ((bool)m_buttonFilter)
		{
			m_buttonFilter.m_pressable = buttonCancel.gameObject;
			UICamera.currentFilter = m_buttonFilter;
		}
	}

	private void ShowSubtitleAndPointCharacter(Action action)
	{
		HideSubtitle();
		EnableMenuPanelButton(0, true);
		Vector3 p_location = Vector3.zero;
		StandardMenuPanel standardMenuPanel = m_menuPanel as StandardMenuPanel;
		if ((bool)standardMenuPanel && (bool)standardMenuPanel.buttons[0])
		{
			p_location = standardMenuPanel.buttons[0].transform.localPosition;
			p_location.x -= 20f;
			p_location.y += 20f;
			p_location.z = -10f;
		}
		Dbg.Assert(MainMenu.Instance.m_panel);
		SingletonComponent<TutorialManager>.Instance.ShowPointerWithDelay(p_location, PointingHand.Direction.Down, MainMenu.Instance.m_panel.transform, 0.2f);
		SingletonComponent<TutorialManager>.Instance.ShowInstruction(action.text, InstructionBox.Anchor.Bottom);
		UIPopupController.LockUICamera("UI_Bar", true);
		if ((bool)m_buttonFilter)
		{
			m_buttonFilter.m_pressable = standardMenuPanel.buttons[0];
			UICamera.currentFilter = m_buttonFilter;
		}
	}

	private void ShowFingerOnBackButton(Action action)
	{
		HideSubtitle();
		Vector3 p_location = Vector3.left * 20f + Vector3.up * 20f + Vector3.back * 100f;
		UIButton sharedBackButton = MainMenu.Instance.m_sharedBackButton;
		Transform p_parent = sharedBackButton.transform;
		SingletonComponent<TutorialManager>.Instance.ShowPointerWithDelay(p_location, PointingHand.Direction.UpLeft, p_parent, 0.2f);
		SingletonComponent<TutorialManager>.Instance.ShowInstruction(action.text, InstructionBox.Anchor.Bottom);
		if ((bool)m_buttonFilter)
		{
			m_buttonFilter.m_pressable = sharedBackButton.gameObject;
			UICamera.currentFilter = m_buttonFilter;
		}
	}

	private void ShowSubtitleAndEnableDungeon(Action action)
	{
		HideSubtitle();
		MainMenu.Instance.bottomBar.UnlockMenu(MainMenu.Menu.Home);
		SingletonComponent<TutorialManager>.Instance.ShowPointerAtBottomMenu(MainMenu.Menu.Home, 0.2f);
		DungeonEventStruct dungeonEventStruct = SingletonComponent<DungeonManager>.Instance.FindDungeonEvent("TUTORIAL_2");
		if (dungeonEventStruct != null)
		{
			dungeonEventStruct.locked = false;
		}
		SingletonComponent<TutorialManager>.Instance.ShowInstruction(action.text);
		if ((bool)m_buttonFilter)
		{
			m_buttonFilter.m_pressable = MainMenu.Instance.bottomBar.m_buttons[0].gameObject;
			UICamera.currentFilter = m_buttonFilter;
		}
	}

	private void InventoryEnd()
	{
		HideSubtitle();
		HideInstruction();
		Messenger<State>.Invoke("tutorial_completed", State.Inventory);
	}
}
