using System.Collections.Generic;
using UnityEngine;

public class CharacterMenuScreen : NBUIMenuScreen
{
	public enum Menu
	{
		CHARACTER = 0,
		BAG = 1,
		FUSION = 2,
		EVOLVE = 3,
		SELL = 4,
		COLLECTION = 5
	}

	public enum ScreenState
	{
		HOME = 0,
		CHARACTER = 1,
		BAG = 2,
		FUSION = 3,
		EVOLVE = 4,
		SELL = 5
	}

	private ScreenState _currState;

	private ScreenState m_selectedState;

	[SerializeField]
	private GameObject m_particleEffects;

	[SerializeField]
	private GameObject m_localBackground;

	[SerializeField]
	private UILabel m_NewItemCounter;

	[SerializeField]
	private UIBadge m_armorCollectionBadge;

	private ScreenState currState
	{
		get
		{
			return _currState;
		}
		set
		{
			_currState = value;
			if (_currState == ScreenState.HOME || _currState == ScreenState.BAG || _currState == ScreenState.SELL)
			{
				backButton.SetActiveRecursively(false);
			}
			else
			{
				backButton.SetActiveRecursively(true);
			}
			string text = string.Empty;
			bool p_withTopBar = true;
			bool show = true;
			float p_value = -101f;
			switch (_currState)
			{
			case ScreenState.HOME:
			{
				text = "character_header";
				InventoryManager instance = SingletonComponent<InventoryManager>.Instance;
				if (null != instance && null != m_armorCollectionBadge)
				{
					int newCollectionItemsTotal = instance.GetNewCollectionItemsTotal();
					m_armorCollectionBadge.SetNumber(newCollectionItemsTotal);
				}
				break;
			}
			case ScreenState.CHARACTER:
				text = "equipment_header";
				p_withTopBar = false;
				show = false;
				p_value = -46f;
				break;
			case ScreenState.EVOLVE:
				text = "evolve_header";
				break;
			case ScreenState.FUSION:
				text = "fusion_header";
				break;
			}
			if (null != backButton)
			{
				backButton.transform.SetLocalY(p_value);
			}
			MainMenu instance2 = MainMenu.Instance;
			if (instance2 != null)
			{
				instance2.EnableHeader(!string.IsNullOrEmpty(text), text);
				instance2.ShowTopAndBottomBar(show);
				instance2.MoveHeaderToTop(p_withTopBar);
			}
		}
	}

	public override void OnEnable()
	{
		base.OnEnable();
		Init();
	}

	public override void OnDisable()
	{
		base.OnDisable();
		if (backButton != null)
		{
			backButton.SetActiveRecursively(false);
		}
		EnableBackground(false);
	}

	private void CallInit(bool _loaded = true)
	{
		Init();
	}

	public override void Start()
	{
		if (MainMenu.Instance.CharacterMenuScreenJumpToState != ScreenState.HOME)
		{
			PushState(MainMenu.Instance.CharacterMenuScreenJumpToState);
			MainMenu.Instance.CharacterMenuScreenJumpToState = ScreenState.HOME;
		}
	}

	protected override void Init()
	{
		EnableBackground(false);
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
		PushState(ScreenState.HOME);
		if (MainMenu.Instance != null && MainMenu.Instance.inventory != null)
		{
			MainMenu.Instance.inventory.SetDirty();
		}
		if (MenuBackground3D.Instance != null)
		{
			MenuBackground3D.Instance.ShowDefaultBackGround();
		}
	}

	protected override void Reset()
	{
		ClearStack();
	}

	public override void Home()
	{
		if (_currState != ScreenState.HOME)
		{
			base.Home();
		}
	}

	private void PushState(ScreenState nextState)
	{
		if (nextState >= ScreenState.HOME && (int)nextState < menuPanels.Length)
		{
			PushPanel(menuPanels[(int)nextState]);
			currState = nextState;
			if (nextState == ScreenState.HOME)
			{
				UpdateRedIconNum();
			}
		}
	}

	private void PopState()
	{
		PopPanel();
		ScreenState currentPanelIndex = (ScreenState)GetCurrentPanelIndex();
		currState = currentPanelIndex;
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
		m_selectedState = _currState;
		Menu id = (Menu)component.id;
		switch (id)
		{
		case Menu.COLLECTION:
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_secondary_ui_tap");
			MainMenu.Instance.ShowArmorCollections();
			return;
		case Menu.CHARACTER:
		{
			m_selectedState = ScreenState.CHARACTER;
			CharacterInfoPanel characterInfoPanel = menuPanels[1] as CharacterInfoPanel;
			if (null != characterInfoPanel)
			{
				characterInfoPanel.SetData();
			}
			break;
		}
		case Menu.BAG:
			m_selectedState = ScreenState.BAG;
			break;
		case Menu.FUSION:
			m_selectedState = ScreenState.FUSION;
			break;
		case Menu.EVOLVE:
			m_selectedState = ScreenState.EVOLVE;
			break;
		case Menu.SELL:
			m_selectedState = ScreenState.SELL;
			break;
		}
		if (m_selectedState != _currState)
		{
			Messenger<Menu>.Invoke("character_menu_selected", id);
			PushSelectedPanel();
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_secondary_ui_tap");
		}
	}

	private void PushSelectedPanel()
	{
		UIPopupController.Instance.HideLoadingPopup();
		PushState(m_selectedState);
	}

	public void ShowInventory(bool show, bool multipleSelection, bool isFusionBase = false, NBUIMenuPanel parent = null, bool allowFilter = false, Equipment.Part filter = Equipment.Part.Weapon)
	{
		if (show)
		{
			InventoryPanel inventory = MainMenu.Instance.inventory;
			inventory.allowMultipleSelection = multipleSelection;
			inventory.eventTarget = parent;
			inventory.SetDefaultSelectLimit();
			if (parent == menuPanels[2])
			{
				inventory.panelType = InventoryPanel.PanelType.INVENTORY;
			}
			else if (parent == menuPanels[1])
			{
				inventory.panelType = InventoryPanel.PanelType.EQUIP;
			}
			else if (parent == menuPanels[3])
			{
				if (isFusionBase)
				{
					inventory.panelType = InventoryPanel.PanelType.FUSION_BASEITEM;
				}
				else
				{
					inventory.panelType = InventoryPanel.PanelType.FUSION;
				}
			}
			else if (parent == menuPanels[4])
			{
				inventory.panelType = InventoryPanel.PanelType.EVOLVE;
			}
			else if (parent == menuPanels[5])
			{
				inventory.panelType = InventoryPanel.PanelType.SELL;
				inventory.SetSellSelectLimit();
			}
			inventory.filter = ((!allowFilter) ? Equipment.Part.Max : filter);
			MainMenu.Instance.ShowInventory(true);
		}
		else
		{
			MainMenu.Instance.ShowInventory(false);
		}
	}

	private void OnBack()
	{
		if (MainMenu.Instance.m_parentSceneIsHomepage)
		{
			MainMenu.Instance.ShowHomePage();
			MainMenu.Instance.m_parentSceneIsHomepage = false;
		}
		else
		{
			PopState();
			MainMenu.Instance.m_parentSceneIsHomepage = false;
		}
	}

	private void OnBagEvent(BagEvent evt)
	{
		if (evt.type == BagEventType.OPEN_INVENTORY)
		{
			NBUIMenuPanel parent = base.currentPanel;
			ShowInventory(true, false, false, parent);
		}
		else if (evt.type == BagEventType.CLOSE_INVENTORY)
		{
			PopState();
			ShowInventory(false, false);
			if (MainMenu.Instance != null)
			{
				if (SingletonComponent<InventoryManager>.Instance != null)
				{
					MainMenu.Instance.UpdateHintCounter(m_NewItemCounter, SingletonComponent<InventoryManager>.Instance.GetNumOfNewItems().ToString());
					return;
				}
				m_NewItemCounter.cachedTransform.parent.gameObject.SetActiveRecursively(false);
				Dbg.LogError("InventoryManager.Instance is null, unable to update the number, disable the m_NewItemCounter");
			}
			else
			{
				Dbg.LogError("MainMenu.Instance  is null, do nothing");
			}
		}
		else if (evt.type == BagEventType.EQUIP_ITEM)
		{
			InventoryPanel inventory = MainMenu.Instance.inventory;
			inventory.Refresh();
			if (evt.item != null)
			{
				inventory.SelectItem(evt.item);
			}
			inventory.MarkExcessCells();
			if (MainMenu.Instance != null && MainMenu.Instance.m_sharedCharacterInfoPanel != null)
			{
				MainMenu.Instance.m_sharedCharacterInfoPanel.ResetCharacterModel();
			}
		}
	}

	private void OnSellEvent(SellEvent evt)
	{
		switch (evt.type)
		{
		case SellEventType.OPEN_INVENTORY:
		{
			ShowInventory(true, true, false, base.currentPanel);
			InventoryPanel inventory = MainMenu.Instance.inventory;
			if (null != inventory)
			{
				inventory.RefreshCheckbox();
			}
			break;
		}
		case SellEventType.CLOSE_INVENTORY:
			PopState();
			ShowInventory(false, false);
			if (MainMenu.Instance != null)
			{
				if (SingletonComponent<InventoryManager>.Instance != null)
				{
					MainMenu.Instance.UpdateHintCounter(m_NewItemCounter, SingletonComponent<InventoryManager>.Instance.GetNumOfNewItems().ToString());
					break;
				}
				m_NewItemCounter.cachedTransform.parent.gameObject.SetActiveRecursively(false);
				Dbg.LogError("InventoryManager.Instance is null, unable to update the number, disable the m_NewItemCounter");
			}
			else
			{
				Dbg.LogError("MainMenu.Instance  is null, do nothing");
			}
			break;
		case SellEventType.SELL_ITEMS_WARNING:
			break;
		case SellEventType.SELL_ITEMS_CONFIRM:
			break;
		}
	}

	private void OnFusionEvent(FusionEvent evt)
	{
		switch (evt.eventType)
		{
		case FusionEventType.SELECT_BASE:
		{
			ShowInventory(true, false, true, base.currentPanel);
			InventoryPanel inventory2 = MainMenu.Instance.inventory;
			List<InventoryCell> list2 = new List<InventoryCell>();
			if ((bool)evt.baseCell)
			{
				list2.Add(evt.baseCell);
			}
			inventory2.SetSelectedItems(list2);
			break;
		}
		case FusionEventType.SELECT_ITEMS:
		{
			ShowInventory(true, true, false, base.currentPanel);
			InventoryPanel inventory = MainMenu.Instance.inventory;
			List<InventoryCell> list = new List<InventoryCell>();
			if ((bool)evt.baseCell)
			{
				list.Add(evt.baseCell);
			}
			inventory.SetDisabledItems(list);
			inventory.SetSelectedItems(evt.materialCells);
			inventory.RefreshCheckbox();
			break;
		}
		case FusionEventType.START_FUSION:
			break;
		case FusionEventType.RESULT:
			break;
		case FusionEventType.SELECT_DONE:
			break;
		}
	}

	private void OnEvolveEvent(EvolveEvent evt)
	{
		switch (evt.type)
		{
		case EvolveEventType.SELECT_BASE:
		{
			ShowInventory(true, false, false, base.currentPanel);
			InventoryPanel inventory = MainMenu.Instance.inventory;
			List<InventoryCell> list = new List<InventoryCell>();
			if ((bool)evt.baseCell)
			{
				list.Add(evt.baseCell);
			}
			inventory.SetSelectedItems(list);
			break;
		}
		case EvolveEventType.START_EVOLVE:
			break;
		case EvolveEventType.RESULT:
			break;
		}
	}

	private void OnItemSelectEvent(ItemSelectEvent evt)
	{
		if (evt.eventType == ItemSelectEventType.ITEM_SELL || evt.eventType == ItemSelectEventType.ITEM_SELL_NO_POPUP)
		{
			return;
		}
		if (evt.eventType == ItemSelectEventType.ITEM_CANCEL)
		{
			MainMenu instance = MainMenu.Instance;
			InventoryManager instance2 = SingletonComponent<InventoryManager>.Instance;
			if (null != instance)
			{
				if (null != instance2)
				{
					if (!MainMenu.Instance.inventory.show)
					{
						instance.UpdateHintCounter(m_NewItemCounter, instance2.GetNumOfNewItems().ToString());
					}
				}
				else if (null != m_NewItemCounter && null != m_NewItemCounter.cachedTransform)
				{
					m_NewItemCounter.cachedTransform.parent.gameObject.SetActiveRecursively(false);
				}
			}
		}
		ShowInventory(false, false);
	}

	private void OnEquipmentCellEvent(EquipmentCellEvent evt)
	{
		ShowInventory(true, false, false, base.currentPanel, true, evt.partType);
	}

	public void EnableBackground(bool enable)
	{
		if ((bool)m_particleEffects)
		{
			m_particleEffects.SetActiveRecursively(enable);
		}
		if ((bool)m_localBackground)
		{
			m_localBackground.SetActiveRecursively(enable);
		}
	}

	public override void UpdateRedIconNum()
	{
		if (MainMenu.Instance != null)
		{
			MainMenu.Instance.UpdateNewItemCounter();
			if (SingletonComponent<InventoryManager>.Instance != null)
			{
				MainMenu.Instance.UpdateHintCounter(m_NewItemCounter, SingletonComponent<InventoryManager>.Instance.GetNumOfNewItems().ToString());
				return;
			}
			m_NewItemCounter.cachedTransform.parent.gameObject.SetActiveRecursively(false);
			Dbg.LogError("InventoryManager.Instance is null, unable to update the number, disable the m_NewItemCounter");
		}
		else
		{
			Dbg.LogError("MainMenu.Instance is null, do nothing");
		}
	}

	public override void StandardPanelOnEnablePostProcess()
	{
		if (menuPanels[0] != null && menuPanels[0] is StandardMenuPanel)
		{
			StandardMenuPanel standardMenuPanel = menuPanels[0] as StandardMenuPanel;
			if (standardMenuPanel.buttons[3] != null)
			{
				standardMenuPanel.buttons[3].SetActiveRecursively(false);
			}
		}
	}

	private void StartFlyInAnimation()
	{
	}

	private void OnEndFlyInAnimation()
	{
	}

	private void StartFlyOutAnimation()
	{
	}

	private void OnEndFlyOutAnimation()
	{
	}
}
