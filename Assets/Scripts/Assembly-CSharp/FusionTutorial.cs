using System.Collections;
using UnityEngine;

public class FusionTutorial : Tutorial
{
	private InventoryCell m_cell;

	private Action m_pointToBaseItemAction;

	private Action m_pointToMaterialItemAction;

	private TutorialButtonFilter m_buttonFilter;

	private bool m_hasSelectedBaseItem;

	private bool m_hasSelectedMaterialItem;

	private FusionPanel m_fusionPanel;

	private InventoryPanel m_inventory;

	private TutorialManager m_tutorialMgr;

	private bool m_bEnhanceButtonEnabled;

	protected override void OnEnable()
	{
		base.OnEnable();
		Messenger<MainMenu.Menu>.AddListener("mainmenu_screen_enter", OnEnterScreen);
		Messenger<NBUIMenuPanel>.AddListener("mainmenu_show_panel", OnShowPanel);
		Messenger<CharacterMenuScreen.Menu>.AddListener("character_menu_selected", OnCharacterMenuSelected);
		Messenger.AddListener("inventory_cancel", OnInventoryCanceled);
		Messenger<InventoryCell>.AddListener("inventory_item_selected", OnItemSelected);
		Messenger<InventoryPanel.PanelType>.AddListener("inventory_ok", OnInventoryOk);
		Messenger.AddListener("inventory_ready", OnInventoryReady);
		Messenger.AddListener("fusion_select_base", OnFusionSelectBase);
		Messenger.AddListener("fusion_select_material", OnSelectMaterial);
		Messenger.AddListener("fusion_start", OnFusionStart);
		Messenger.AddListener("fusion_show_result", OnFusionShowResult);
		Messenger.AddListener("fusion_done", OnFusionDone);
		Messenger.AddListener("homepanel_upgrade", OnHomePanelUpgradeButtonPressed);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Messenger<MainMenu.Menu>.RemoveListener("mainmenu_screen_enter", OnEnterScreen);
		Messenger<NBUIMenuPanel>.RemoveListener("mainmenu_show_panel", OnShowPanel);
		Messenger<CharacterMenuScreen.Menu>.RemoveListener("character_menu_selected", OnCharacterMenuSelected);
		Messenger.RemoveListener("inventory_cancel", OnInventoryCanceled);
		Messenger<InventoryCell>.RemoveListener("inventory_item_selected", OnItemSelected);
		Messenger<InventoryPanel.PanelType>.RemoveListener("inventory_ok", OnInventoryOk);
		Messenger.RemoveListener("inventory_ready", OnInventoryReady);
		Messenger.RemoveListener("fusion_select_base", OnFusionSelectBase);
		Messenger.RemoveListener("fusion_select_material", OnSelectMaterial);
		Messenger.RemoveListener("fusion_start", OnFusionStart);
		Messenger.RemoveListener("fusion_show_result", OnFusionShowResult);
		Messenger.RemoveListener("fusion_done", OnFusionDone);
		Messenger.RemoveListener("homepanel_upgrade", OnHomePanelUpgradeButtonPressed);
	}

	private void EnablePanelButtons()
	{
		Dbg.Assert(MainMenu.Instance);
		Dbg.Assert(MainMenu.Instance.currScreen is CharacterMenuScreen);
		MainMenu.Instance.currScreen.menuPanels[0].EnableButton(0, true);
		MainMenu.Instance.currScreen.menuPanels[0].EnableButton(1, true);
		MainMenu.Instance.currScreen.menuPanels[0].EnableButton(2, m_bEnhanceButtonEnabled);
		MainMenu.Instance.currScreen.menuPanels[0].EnableButton(3, false);
		MainMenu.Instance.currScreen.menuPanels[0].EnableButton(4, false);
	}

	public override void Start()
	{
		base.Start();
		Dbg.Assert(UIPopupController.Instance);
		m_tutorialMgr = SingletonComponent<TutorialManager>.Instance;
		Dbg.Assert(m_tutorialMgr);
		Dbg.Assert(MainMenu.Instance);
		m_inventory = MainMenu.Instance.inventory;
		Dbg.Assert(m_inventory);
		m_inventory.m_showWarning = false;
		m_buttonFilter = base.gameObject.AddComponent<TutorialButtonFilter>();
		Dbg.Assert(m_buttonFilter);
	}

	private void OnShowPanel(NBUIMenuPanel panel)
	{
		if (panel is FusionPanel && TryAdvance("show_fusion_panel"))
		{
			AnalyticsManager.FirstInstallEvents("tut_upgrade");
		}
	}

	private void Reset()
	{
		m_hasSelectedBaseItem = false;
		m_hasSelectedMaterialItem = false;
		if ((bool)m_inventory)
		{
			m_inventory.buttonOk.isEnabled = true;
			m_inventory.buttonBuySlots.isEnabled = true;
			m_inventory.buttonCancel.isEnabled = true;
			m_inventory.buttonSort.isEnabled = true;
		}
		if ((bool)m_fusionPanel)
		{
			Dbg.Assert((bool)m_fusionPanel.baseCell && (bool)m_fusionPanel.baseCell.GetComponent<Collider>());
			m_fusionPanel.baseCell.GetComponent<Collider>().enabled = true;
			Dbg.Assert(m_fusionPanel.itemCells != null);
			InventoryCell[] itemCells = m_fusionPanel.itemCells;
			foreach (InventoryCell inventoryCell in itemCells)
			{
				Dbg.Assert(inventoryCell.GetComponent<Collider>());
				inventoryCell.GetComponent<Collider>().enabled = true;
			}
		}
	}

	private void OnHomePanelUpgradeButtonPressed()
	{
		TryAdvance("click_upgrade");
	}

	private void OnEnterScreen(MainMenu.Menu menu)
	{
		switch (menu)
		{
		case MainMenu.Menu.Character:
			EnablePanelButtons();
			break;
		case MainMenu.Menu.Home:
			if (TryAdvance("click_dungeon_menu"))
			{
				AnalyticsManager.FirstInstallEvents("tut_home2");
			}
			break;
		}
	}

	private void OnCharacterMenuSelected(CharacterMenuScreen.Menu menu)
	{
		if (menu == CharacterMenuScreen.Menu.FUSION)
		{
			TryAdvance("click_fusion_menu");
		}
	}

	private void OnInventoryCanceled()
	{
		TryAdvance("click_cancel");
	}

	private void OnInventoryOk(InventoryPanel.PanelType type)
	{
		switch (type)
		{
		case InventoryPanel.PanelType.FUSION_BASEITEM:
			if (TryAdvance("click_ok"))
			{
				AnalyticsManager.FirstInstallEvents("tut_base_tapok");
				m_hasSelectedBaseItem = true;
			}
			break;
		case InventoryPanel.PanelType.FUSION:
			if (TryAdvance("click_ok"))
			{
				AnalyticsManager.FirstInstallEvents("tut_material_tapok");
				m_hasSelectedMaterialItem = true;
			}
			break;
		}
	}

	private void OnInventoryReady()
	{
		TryAdvance("inventory_ready");
	}

	private void OnItemSelected(InventoryCell cell)
	{
		if (cell == null)
		{
			return;
		}
		InventoryPanel.PanelType panelType = m_inventory.panelType;
		if (!m_inventory.isReady || (panelType != InventoryPanel.PanelType.FUSION && panelType != InventoryPanel.PanelType.FUSION_BASEITEM))
		{
			return;
		}
		if (cell == m_cell)
		{
			Dbg.Assert(cell.m_checkbox);
			if (cell.m_checkbox.gameObject.active)
			{
				if (cell.Checked)
				{
					if (!TryAdvance("click_item"))
					{
					}
				}
				else if (!m_hasSelectedMaterialItem)
				{
					RollbackAndDoAction(m_pointToMaterialItemAction);
				}
			}
			else
			{
				TryAdvance("click_item");
			}
		}
		else if (!m_hasSelectedBaseItem)
		{
			RollbackAndDoAction(m_pointToBaseItemAction);
		}
		else if (!m_hasSelectedMaterialItem)
		{
			RollbackAndDoAction(m_pointToMaterialItemAction);
		}
	}

	private void OnFusionSelectBase()
	{
		if (TryAdvance("click_base_slot"))
		{
			AnalyticsManager.FirstInstallEvents("tut_baseitem");
		}
	}

	private void OnSelectMaterial()
	{
		if (TryAdvance("click_material_slot"))
		{
			AnalyticsManager.FirstInstallEvents("tut_materials");
		}
	}

	private void OnFusionStart()
	{
		SingletonComponent<TutorialManager>.Instance.AddCompletedState(State.Fusion);
		SingletonComponent<TutorialManager>.Instance.SaveProgress();
		if (TryAdvance("fusion_start"))
		{
			AnalyticsManager.FirstInstallEvents("tut_upgrade_tapok");
		}
	}

	private void OnFusionShowResult()
	{
		TryAdvance("fusion_result");
	}

	private void OnFusionDone()
	{
		TryAdvance("fusion_done");
	}

	public override void DoAction(Action action)
	{
		UICamera.currentFilter = null;
		base.DoAction(action);
	}

	private void ShowSubtitleHideFinger(Action p_action)
	{
		ShowSubtitle(p_action);
		m_tutorialMgr.HidePointer();
		m_tutorialMgr.HideInstruction();
	}

	private void ShowFingerOnUpgradeButton(Action p_action)
	{
		HideSubtitle();
		HomePanel homePanel = MainMenu.Instance.currScreen.currentPanel as HomePanel;
		Dbg.Assert(homePanel);
		Dbg.Assert(homePanel.m_upgradeButton);
		Vector3 p_location = Vector3.right * 100f + Vector3.back * 20f + Vector3.down * 20f;
		SingletonComponent<TutorialManager>.Instance.ShowPointerWithDelay(p_location, PointingHand.Direction.DownLeft, homePanel.m_upgradeButton.transform, 0.2f);
		SingletonComponent<TutorialManager>.Instance.ShowInstruction(p_action.text, InstructionBox.Anchor.Bottom);
		if ((bool)m_buttonFilter)
		{
			m_buttonFilter.m_pressable = homePanel.m_upgradeButton.gameObject;
			UICamera.currentFilter = m_buttonFilter;
		}
	}

	private void HideFingerDelayTime(Action p_action)
	{
		SingletonComponent<TutorialManager>.Instance.HidePointer();
		SingletonComponent<TutorialManager>.Instance.HideInstruction();
		DelayTime(p_action);
	}

	private IEnumerator ShowFingerOnBaseSlot(Action p_action)
	{
		SingletonComponent<TutorialManager>.Instance.HideInstruction();
		if (!m_fusionPanel)
		{
			CharacterMenuScreen screen = MainMenu.Instance.currScreen as CharacterMenuScreen;
			while (!screen)
			{
				yield return null;
				screen = MainMenu.Instance.currScreen as CharacterMenuScreen;
			}
			m_fusionPanel = screen.menuPanels[3] as FusionPanel;
		}
		Dbg.Assert(m_fusionPanel);
		Vector3 fingerPos = m_fusionPanel.baseCell.transform.localPosition;
		m_tutorialMgr.ShowPointerWithDelay(fingerPos, PointingHand.Direction.DownLeft, MainMenu.Instance.m_panel.transform, 0.2f);
		m_tutorialMgr.ShowInstruction(p_action.text, InstructionBox.Anchor.Bottom);
		if ((bool)m_buttonFilter)
		{
			m_buttonFilter.m_pressable = m_fusionPanel.baseCell.gameObject;
			UICamera.currentFilter = m_buttonFilter;
		}
	}

	private void ShowFingerOnBaseItem(Action p_action)
	{
		m_pointToBaseItemAction = p_action;
		m_tutorialMgr.HidePointer();
		HideSubtitle();
		m_inventory.buttonOk.isEnabled = false;
		m_inventory.buttonBuySlots.isEnabled = false;
		m_inventory.buttonCancel.isEnabled = false;
		m_inventory.buttonSort.isEnabled = false;
		ShowPointerToItem(0);
		m_tutorialMgr.ShowInstruction(p_action.text, InstructionBox.Anchor.Bottom);
	}

	private void ShowPointerToItem(int p_index)
	{
		Vector3 p_location = new Vector3(-405f, 200f, 1f);
		int count = m_inventory.grid.visibleCells.Count;
		Dbg.Assert(count > p_index);
		if (count > p_index)
		{
			m_cell = MainMenu.Instance.inventory.grid.visibleCells[p_index];
			p_location = m_cell.transform.position / GLOBAL.UI_SCALE_FACTOR;
			p_location += Vector3.up * -10f + Vector3.right * 10f;
			p_location.z = 1f;
		}
		Dbg.Assert(m_cell);
		m_tutorialMgr.HidePointer();
		m_tutorialMgr.ShowPointerWithDelay(p_location, PointingHand.Direction.DownRight, UIPopupController.Instance.Panel, 0.2f);
		if ((bool)m_buttonFilter)
		{
			m_buttonFilter.m_pressable = m_cell.gameObject;
			UICamera.currentFilter = m_buttonFilter;
		}
	}

	private void ShowFingerOnOkButton(Action p_action)
	{
		if (m_pointToBaseItemAction != null && !m_hasSelectedBaseItem)
		{
			m_pointToBaseItemAction.nextId = p_action.id;
		}
		else if (m_pointToMaterialItemAction != null && m_hasSelectedMaterialItem)
		{
		}
		if (null != m_inventory)
		{
			switch (m_inventory.panelType)
			{
			case InventoryPanel.PanelType.FUSION_BASEITEM:
				AnalyticsManager.FirstInstallEvents("tut_selectHellish");
				break;
			case InventoryPanel.PanelType.FUSION:
				AnalyticsManager.FirstInstallEvents("tut_material3");
				break;
			}
		}
		HideSubtitle();
		Dbg.Assert(m_inventory.buttonOk);
		m_inventory.buttonOk.isEnabled = true;
		Vector3 p_location = Vector3.down * 50f + Vector3.left * 50f + Vector3.back * 25f;
		m_tutorialMgr.ShowPointerWithDelay(p_location, PointingHand.Direction.DownRight, m_inventory.buttonOk.transform, 0.2f);
		m_tutorialMgr.ShowInstruction(p_action.text);
		if ((bool)m_buttonFilter)
		{
			m_buttonFilter.m_pressable = m_inventory.buttonOk.gameObject;
			UICamera.currentFilter = m_buttonFilter;
		}
	}

	private void ShowFingerOnMaterialSlot(Action p_action)
	{
		HideSubtitle();
		m_hasSelectedBaseItem = true;
		if (!m_fusionPanel)
		{
			m_fusionPanel = MainMenu.Instance.currScreen.menuPanels[3] as FusionPanel;
		}
		Dbg.Assert((bool)m_fusionPanel && (bool)m_fusionPanel.baseCell && (bool)m_fusionPanel.baseCell.GetComponent<Collider>());
		Dbg.Assert(m_fusionPanel.itemCells != null && m_fusionPanel.itemCells.Length > 0);
		m_fusionPanel.baseCell.GetComponent<Collider>().enabled = false;
		Vector3 zero = Vector3.zero;
		zero = m_fusionPanel.itemCells[0].transform.position / GLOBAL.UI_SCALE_FACTOR;
		zero += Vector3.up * -10f + Vector3.right * 10f;
		zero.z = 1f;
		Dbg.Assert(MainMenu.Instance.m_panel);
		m_tutorialMgr.ShowPointerWithDelay(zero, PointingHand.Direction.DownRight, MainMenu.Instance.m_panel.transform, 0.2f);
		m_tutorialMgr.ShowInstruction(p_action.text, InstructionBox.Anchor.Bottom);
		if ((bool)m_buttonFilter)
		{
			m_buttonFilter.m_pressable = m_fusionPanel.itemCells[0].gameObject;
			UICamera.currentFilter = m_buttonFilter;
		}
	}

	private void ShowFingerOnMaterial1(Action p_action)
	{
		m_pointToMaterialItemAction = p_action;
		m_inventory.buttonOk.isEnabled = false;
		m_inventory.buttonBuySlots.isEnabled = false;
		m_inventory.buttonCancel.isEnabled = false;
		m_inventory.buttonSort.isEnabled = false;
		ShowPointerToItem(0);
		m_tutorialMgr.ShowInstruction(p_action.text, InstructionBox.Anchor.Bottom);
	}

	private void ShowFingerOnMaterial2(Action p_action)
	{
		ShowPointerToItem(1);
		AnalyticsManager.FirstInstallEvents("tut_material1");
		m_tutorialMgr.ShowInstruction(p_action.text, InstructionBox.Anchor.Bottom);
	}

	private void ShowFingerOnMaterial3(Action p_action)
	{
		ShowPointerToItem(2);
		AnalyticsManager.FirstInstallEvents("tut_material2");
		m_tutorialMgr.ShowInstruction(p_action.text, InstructionBox.Anchor.Bottom);
	}

	private void ShowFingerOnStartFusionButton(Action p_action)
	{
		HideSubtitle();
		Dbg.Assert(m_fusionPanel.itemCells != null);
		InventoryCell[] itemCells = m_fusionPanel.itemCells;
		foreach (InventoryCell inventoryCell in itemCells)
		{
			Dbg.Assert(inventoryCell.GetComponent<Collider>());
			inventoryCell.GetComponent<Collider>().enabled = false;
		}
		Vector3 vector = new Vector3(100f, -50f, 1f);
		Dbg.Assert(m_fusionPanel.buttonAction);
		vector = m_fusionPanel.buttonAction.transform.localPosition;
		vector += Vector3.up * -40f;
		m_tutorialMgr.ShowPointerWithDelay(vector, PointingHand.Direction.DownRight, MainMenu.Instance.m_panel.transform, 0.2f);
		m_tutorialMgr.ShowInstruction(p_action.text);
		if ((bool)m_buttonFilter)
		{
			m_buttonFilter.m_pressable = m_fusionPanel.buttonAction.gameObject;
			UICamera.currentFilter = m_buttonFilter;
		}
	}

	private void ShowFingerGoToDungeon(Action p_action)
	{
		HideSubtitle();
		SingletonComponent<TutorialManager>.Instance.ShowPointerAtBottomMenu(MainMenu.Menu.Home, 0.2f);
		SingletonComponent<TutorialManager>.Instance.ShowInstruction(p_action.text);
		if ((bool)m_buttonFilter)
		{
			m_buttonFilter.m_pressable = MainMenu.Instance.bottomBar.m_buttons[0].gameObject;
			UICamera.currentFilter = m_buttonFilter;
		}
	}

	private void DoNothing()
	{
		m_tutorialMgr.HidePointer();
		m_tutorialMgr.HideInstruction();
	}

	private void FusionEnd()
	{
		HideSubtitle();
		Reset();
		m_inventory.m_showWarning = true;
		Messenger<State>.Invoke("tutorial_completed", State.Fusion);
	}
}
