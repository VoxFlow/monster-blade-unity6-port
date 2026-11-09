using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryPanel : NBUIMenuPanel
{
	public enum PanelType
	{
		FUSION_BASEITEM = 0,
		FUSION = 1,
		EVOLVE = 2,
		SELL = 3,
		EQUIP = 4,
		INVENTORY = 5
	}

	private const int MAX_VISIBLE_CELLS = 12;

	private const int MIN_SCROLLABLE_CELLS = 10;

	public InventoryGrid grid;

	public UIDraggablePanel scrollView;

	public UIButton buttonOk;

	public UIButton buttonCancel;

	public UIButton buttonSort;

	public UIButton buttonBuySlots;

	public UILabel m_labelSlotsFilled;

	public UILabel m_labelSlotsMax;

	public ItemInfo m_itemInfo;

	public UILabel XpPercent;

	public UILabel XpLabelTitle;

	public UILabel labelXpValue;

	public UILabel labelXpNextValue;

	public UISlider sliderXp;

	public UISlider sliderXpFusion;

	public UILabel labelCostValue;

	public UILabel labelItemCost;

	public UISprite spriteCoin;

	public EquipNotification m_equipNotificationPrefab;

	private EquipNotification m_equipNotification;

	public Component eventTarget;

	public bool m_showWarning = true;

	public bool allowMultipleSelection;

	public int maxSelectedItems = 6;

	private int maxDefaultSelectedItems = 6;

	public PanelType panelType;

	public Equipment.Part filter;

	public bool dirty;

	public bool m_showClickItem;

	private List<InventoryCell> _disabledItems = new List<InventoryCell>();

	private List<InventoryCell> _selectedItems = new List<InventoryCell>();

	private List<InventoryCell> _excessItems = new List<InventoryCell>();

	private Equipment.Part _topCategory;

	private InventoryCell _currSelectedCell;

	private InventoryCell _prevSelectedCell;

	private ItemSelectEvent m_lastEvent;

	private Vector3 m_momentum;

	private Vector3 m_scrollViewTargetPos;

	private float m_cellHeight;

	public UILabel okLabel;

	public UISprite okUISprite;

	public UIButtonMessage okButtonMessage;

	public string okDisableSpriteName = string.Empty;

	public Color okDisableColorText = Color.grey;

	private bool m_ready;

	private bool m_isGrouped;

	public bool isReady
	{
		get
		{
			return m_ready;
		}
	}

	private new void Awake()
	{
		UIEventListener uIEventListener = UIEventListener.Get(scrollView.gameObject);
		uIEventListener.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onPress, new UIEventListener.BoolDelegate(OnPressed));
		if (okDisableSpriteName == string.Empty || okUISprite == null || okLabel == null || okButtonMessage == null)
		{
			Dbg.LogWarn("No ok DisableSpriteName set / okUISprite / okButtonMessage or okLabel not found.");
		}
		if (!(null != m_itemInfo))
		{
		}
	}

	private void OnPressed(GameObject source, bool isPressed)
	{
		if (isPressed)
		{
		}
	}

	private void OnEnable()
	{
		Init();
	}

	private void OnDisable()
	{
		Reset();
	}

	private void InitInventoryList()
	{
		grid.Clear();
		if (SingletonComponent<InventoryManager>.Instance == null)
		{
			Dbg.LogWarn("Cant initialize inventory without InventoryManager.");
			return;
		}
		List<ItemDetails> list = new List<ItemDetails>(SingletonComponent<InventoryManager>.Instance.GetInventory.Values);
		foreach (ItemDetails item in list)
		{
			grid.AddData(item);
			InventoryCell inventoryCell = grid.AddCell(0f, 0f);
			if (item.isNew)
			{
				inventoryCell.New = true;
			}
			UIButtonMessage component = inventoryCell.GetComponent<UIButtonMessage>();
			component.target = base.gameObject;
			component.functionName = "OnItemSelected";
		}
		if (m_cellHeight == 0f && grid.cells.Count > 0)
		{
			m_cellHeight = grid.cells[0].height;
		}
		UpdateSlotsInfo();
	}

	public void UpdateSlotsInfo()
	{
		int occupiedSpaceOnDisplay = SingletonComponent<InventoryManager>.Instance.OccupiedSpaceOnDisplay;
		int maxSpaceOnDisplay = SingletonComponent<InventoryManager>.Instance.MaxSpaceOnDisplay;
		m_labelSlotsFilled.text = occupiedSpaceOnDisplay + string.Empty;
		NBLocalization instance = SingletonComponent<NBLocalization>.Instance;
		string text = string.Empty;
		if (null != instance)
		{
			text = instance.Get("bag_slots_label");
		}
		string text2 = string.Empty;
		if (text != string.Empty)
		{
			text2 = string.Format(text, 0, maxSpaceOnDisplay);
		}
		if (text2.Length > 1)
		{
			m_labelSlotsMax.text = text2.Substring(1);
		}
		UIWidgetBlink component = m_labelSlotsFilled.GetComponent<UIWidgetBlink>();
		component.enabled = occupiedSpaceOnDisplay > maxSpaceOnDisplay;
		MarkExcessCells();
	}

	private void Start()
	{
	}

	public void SetDirty()
	{
		dirty = true;
		UpdateSlotsInfo();
		SingletonComponent<InventoryManager>.Instance.SaveNewItemsState();
		if (grid.visibleCells.Count < 10)
		{
			scrollView.scale.y = 0f;
		}
		Messenger.Invoke("inventory_updated");
	}

	public void Refresh()
	{
		InitInventoryList();
		Equipment.Part currentTopCategory = grid.currentTopCategory;
		grid.Sort(true);
		if (m_isGrouped)
		{
			grid.GroupWithTopCategory(currentTopCategory);
			grid.currentTopCategory = currentTopCategory;
		}
	}

	public override void Reset()
	{
		foreach (InventoryCell disabledItem in _disabledItems)
		{
			if (disabledItem.gameObject.active)
			{
				disabledItem.Enable = true;
			}
		}
		_disabledItems.Clear();
		grid.UnCheckAllCells();
		foreach (InventoryCell selectedItem in _selectedItems)
		{
			selectedItem.Checked = false;
		}
		_selectedItems.Clear();
		if ((bool)_currSelectedCell)
		{
			_currSelectedCell.Selected = false;
		}
		_currSelectedCell = (_prevSelectedCell = null);
		ResetExcessCells();
		scrollView.currentMomentum = Vector3.zero;
		scrollView.ResetPosition();
		SingletonComponent<InventoryManager>.Instance.SaveNewItemsState();
		m_ready = false;
	}

	private void Init()
	{
		bool flag = false;
		m_ready = false;
		m_isGrouped = false;
		if (dirty)
		{
			InitInventoryList();
			grid.Sort(true);
			flag = true;
			dirty = false;
		}
		if (panelType == PanelType.INVENTORY || panelType == PanelType.EQUIP)
		{
			if (!flag)
			{
				grid.Sort();
			}
			SingletonComponent<NBLocalization>.Instance.SetLabelKey(okLabel, "inventory_bag_ok_button");
			if (filter != Equipment.Part.Max)
			{
				grid.ShowOnlyCategory(filter);
			}
		}
		else if (panelType == PanelType.FUSION_BASEITEM || panelType == PanelType.FUSION || panelType == PanelType.EVOLVE)
		{
			if (panelType == PanelType.FUSION_BASEITEM)
			{
				HideNonFuseableItems(true);
				SingletonComponent<NBLocalization>.Instance.SetLabelKey(okLabel, "evolve_bag_ok_button");
				SingletonComponent<NBLocalization>.Instance.SetLabelKey(labelItemCost, "fusion_cost_label");
			}
			else if (panelType == PanelType.FUSION)
			{
				HideNonFuseableItems(false, true);
				SingletonComponent<NBLocalization>.Instance.SetLabelKey(okLabel, "evolve_bag_ok_button");
				SingletonComponent<NBLocalization>.Instance.SetLabelKey(labelItemCost, "fusion_cost_label");
			}
			else if (panelType == PanelType.EVOLVE)
			{
				HideNonEvolvableItems();
				SingletonComponent<NBLocalization>.Instance.SetLabelKey(okLabel, "fusion_bag_ok_button");
				SingletonComponent<NBLocalization>.Instance.SetLabelKey(labelItemCost, "evolve_cost_label");
			}
		}
		else if (panelType == PanelType.SELL)
		{
			HideNonSellableItems();
			SingletonComponent<NBLocalization>.Instance.SetLabelKey(okLabel, "sell_bag_ok_button");
			SingletonComponent<NBLocalization>.Instance.SetLabelKey(labelItemCost, "sell_bag_item_cost_label");
		}
		RefreshCheckbox();
		scrollView.verticalScrollBar.scrollValue = 0f;
		scrollView.currentMomentum = Vector3.zero;
		SelectFirstActiveCell();
		if (allowMultipleSelection)
		{
			_selectedItems.Clear();
		}
		MarkExcessCells();
		HideInventoryCells();
		base.transform.localScale = Vector3.zero;
		if (!flag)
		{
			StartCoroutine(DoInventoryAnimation(0f));
		}
		else
		{
			StartCoroutine(DoInventoryAnimation(0.5f));
		}
		buttonBuySlots.isEnabled = true;
		if (SingletonComponent<TutorialManager>.Instance == null || !SingletonComponent<TutorialManager>.Instance.enabled)
		{
			Dbg.Log("It's not tutorial now! No need to disable the \"Buy Slot Button\"");
		}
		else if (buttonBuySlots == null)
		{
			Dbg.LogWarn("Unable to find \"Buy Slot Button\"!");
		}
		else if (!SingletonComponent<TutorialManager>.Instance.IsCompleted(Tutorial.State.Gacha))
		{
			buttonBuySlots.isEnabled = false;
			Dbg.Log("Need to disable \"Buy Slot Button\", because users didn't finish the Gacha tutorial yet");
		}
	}

	public void RefreshCheckbox()
	{
		foreach (InventoryCell visibleCell in grid.visibleCells)
		{
			if (visibleCell.Enable)
			{
				visibleCell.m_checkbox.gameObject.SetActiveRecursively(allowMultipleSelection);
			}
			if (visibleCell.data.isNew)
			{
				visibleCell.New = true;
			}
		}
	}

	public void RemoveCells(List<InventoryCell> p_cells)
	{
		grid.RemoveCellsFromInventory(p_cells);
		ClearSelectedItems();
		float y = 0f;
		if (grid.visibleCells.Count >= 10)
		{
			y = scrollView.verticalScrollBar.scrollValue;
		}
		scrollView.UpdateScrollbars(true);
		scrollView.SetDragAmount(0f, y, false);
		MarkExcessCells();
		SetDirty();
	}

	public void MarkExcessCells()
	{
		if (panelType == PanelType.EQUIP || (panelType == PanelType.FUSION && !allowMultipleSelection) || panelType == PanelType.EVOLVE)
		{
			return;
		}
		ResetExcessCells();
		int maxSpaceOnDisplay = SingletonComponent<InventoryManager>.Instance.MaxSpaceOnDisplay;
		int occupiedSpaceOnDisplay = SingletonComponent<InventoryManager>.Instance.OccupiedSpaceOnDisplay;
		int num = occupiedSpaceOnDisplay - maxSpaceOnDisplay;
		Dbg.LogWarn("excess = " + num);
		if (num > 0)
		{
			int count = grid.visibleCells.Count;
			int num2 = Mathf.Max(0, count - num);
			for (int num3 = count - 1; num3 >= num2; num3--)
			{
				InventoryCell inventoryCell = grid.visibleCells[num3];
				inventoryCell.Excess = true;
				_excessItems.Add(inventoryCell);
			}
		}
	}

	public bool HasExcessItems()
	{
		int maxSpaceOnDisplay = SingletonComponent<InventoryManager>.Instance.MaxSpaceOnDisplay;
		int occupiedSpaceOnDisplay = SingletonComponent<InventoryManager>.Instance.OccupiedSpaceOnDisplay;
		return occupiedSpaceOnDisplay > maxSpaceOnDisplay;
	}

	public int HowManyExcessItems()
	{
		int maxSpaceOnDisplay = SingletonComponent<InventoryManager>.Instance.MaxSpaceOnDisplay;
		int occupiedSpaceOnDisplay = SingletonComponent<InventoryManager>.Instance.OccupiedSpaceOnDisplay;
		return Mathf.Max(0, occupiedSpaceOnDisplay - maxSpaceOnDisplay);
	}

	private void ResetExcessCells()
	{
		foreach (InventoryCell excessItem in _excessItems)
		{
			if ((bool)excessItem)
			{
				excessItem.Excess = false;
			}
		}
		_excessItems.Clear();
	}

	private IEnumerator DoInventoryAnimation(float delay)
	{
		scrollView.scale.y = 0f;
		UIPopupController.SetAllInputsEnabled(false);
		yield return new MBWaitForSeconds(delay);   // PORT (CD-059)
		iTween.ScaleTo(base.gameObject, iTween.Hash("scale", Vector3.one, "time", 0.5f, "easetype", iTween.EaseType.easeOutBack));
		yield return new MBWaitForSeconds(0.5f);   // PORT (CD-059)
		int count = 0;
		foreach (InventoryCell cell in grid.visibleCells)
		{
			if (cell.gameObject.active)
			{
				iTween.ScaleTo(cell.gameObject, iTween.Hash("scale", Vector3.one, "time", 0.25f, "easetype", iTween.EaseType.easeOutBack));
				count++;
				if (count >= 12)
				{
					break;
				}
				yield return null;
			}
		}
		yield return new MBWaitForSeconds(0.5f);   // PORT (CD-059)
		yield return null;
		UIPopupController.Instance.EnableUI();
		if (grid.visibleCells.Count >= 10)
		{
			scrollView.scale.y = 1f;
		}
		m_ready = true;
		Messenger.Invoke("inventory_ready");
	}

	private void HideInventoryCells()
	{
		int num = 0;
		foreach (InventoryCell visibleCell in grid.visibleCells)
		{
			if (visibleCell.gameObject.active)
			{
				visibleCell.transform.localScale = Vector3.zero;
				num++;
				if (num >= 12)
				{
					break;
				}
			}
		}
	}

	private void SelectFirstActiveCell()
	{
		if (grid.visibleCells.Count > 0)
		{
			XpPercent.gameObject.SetActiveRecursively(true);
			XpLabelTitle.gameObject.SetActiveRecursively(true);
			labelXpValue.gameObject.SetActiveRecursively(true);
			labelXpNextValue.gameObject.SetActiveRecursively(true);
			sliderXp.gameObject.SetActiveRecursively(true);
			sliderXpFusion.gameObject.SetActiveRecursively(true);
			labelCostValue.gameObject.SetActiveRecursively(true);
			labelItemCost.gameObject.SetActiveRecursively(true);
			spriteCoin.gameObject.SetActiveRecursively(true);
			InventoryCell inventoryCell = grid.visibleCells.First();
			if (null != inventoryCell)
			{
				SelectCell(inventoryCell, false);
			}
		}
		else
		{
			m_itemInfo.ShowItem(ItemManager.s_NULL_DATA, false, false);
			XpPercent.gameObject.SetActiveRecursively(false);
			XpLabelTitle.gameObject.SetActiveRecursively(false);
			labelXpValue.gameObject.SetActiveRecursively(false);
			labelXpNextValue.gameObject.SetActiveRecursively(false);
			sliderXp.gameObject.SetActiveRecursively(false);
			sliderXpFusion.gameObject.SetActiveRecursively(false);
			labelCostValue.gameObject.SetActiveRecursively(false);
			labelItemCost.gameObject.SetActiveRecursively(false);
			spriteCoin.gameObject.SetActiveRecursively(false);
		}
	}

	public void SelectCellAtIndex(int index)
	{
		if (index >= 0 && index < grid.visibleCells.Count)
		{
			InventoryCell cell = grid.visibleCells[index];
			SelectCell(cell);
		}
	}

	public void SelectItem(ItemDetails item)
	{
		if (item == null)
		{
			return;
		}
		foreach (InventoryCell visibleCell in grid.visibleCells)
		{
			if (visibleCell.data.DB_ID == item.DB_ID)
			{
				SelectCell(visibleCell);
				break;
			}
		}
	}

	public void RefreshVisibleCells()
	{
		if (null == grid)
		{
			return;
		}
		foreach (InventoryCell visibleCell in grid.visibleCells)
		{
			if (null == visibleCell)
			{
				break;
			}
			visibleCell.Refresh();
		}
	}

	public void SelectCell(InventoryCell cell, bool p_tick = true)
	{
		if (!cell || cell.Empty)
		{
			return;
		}
		if (m_showClickItem)
		{
			Dbg.LogWarn("INVENTORY_PANEL : --> item : " + cell.data);
		}
		if (cell.data.isNew)
		{
			Dbg.Log("Cell is new, and selected, try to disable the new label");
			cell.data.isNew = false;
			cell.New = false;
			SingletonComponent<InventoryManager>.Instance.RemoveFromNewItems(cell.data);
		}
		if (cell.Enable)
		{
			if (allowMultipleSelection)
			{
				if (p_tick)
				{
					if (!cell.Checked)
					{
						if (_selectedItems.Count >= maxSelectedItems)
						{
							Dbg.LogWarn("You have selected 6 items");
							return;
						}
						_selectedItems.Add(cell);
					}
					else
					{
						_selectedItems.Remove(cell);
					}
					cell.Checked = !cell.Checked;
				}
			}
			else if (!cell.Selected)
			{
				_selectedItems.Clear();
				_selectedItems.Add(cell);
			}
		}
		if (_currSelectedCell != cell)
		{
			_prevSelectedCell = _currSelectedCell;
			if ((bool)_prevSelectedCell)
			{
				_prevSelectedCell.Selected = false;
			}
			_currSelectedCell = cell;
			_currSelectedCell.Selected = true;
		}
		UpdateItemInfo();
		Messenger<InventoryCell>.Invoke("inventory_item_selected", cell);
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_item_selection");
	}

	public void SetDisabledItems(List<InventoryCell> disabledItems)
	{
		if (disabledItems == null || disabledItems.Count == 0 || grid.visibleCells.Count <= 0)
		{
			return;
		}
		foreach (InventoryCell disabledItem in disabledItems)
		{
			foreach (InventoryCell cell in grid.cells)
			{
				if (cell.data.DB_ID == disabledItem.data.DB_ID)
				{
					if (cell.gameObject.active)
					{
						cell.Enable = false;
					}
					_disabledItems.Add(cell);
					break;
				}
			}
		}
		UpdateItemInfo();
	}

	public void SetSelectedItems(List<InventoryCell> selectedItems)
	{
		if (selectedItems == null || selectedItems.Count == 0 || grid.visibleCells.Count <= 0)
		{
			return;
		}
		if (allowMultipleSelection)
		{
			foreach (InventoryCell selectedItem in selectedItems)
			{
				foreach (InventoryCell visibleCell in grid.visibleCells)
				{
					if (visibleCell.data.DB_ID == selectedItem.data.DB_ID)
					{
						visibleCell.Checked = true;
						_selectedItems.Add(visibleCell);
					}
				}
			}
			UpdateItemInfo();
			return;
		}
		InventoryCell inventoryCell = selectedItems[0];
		foreach (InventoryCell visibleCell2 in grid.visibleCells)
		{
			if (visibleCell2.data.DB_ID == inventoryCell.data.DB_ID)
			{
				SelectCell(visibleCell2);
			}
		}
	}

	public void SetDefaultSelectLimit()
	{
		maxSelectedItems = maxDefaultSelectedItems;
	}

	public void SetSellSelectLimit()
	{
		if ((bool)SingletonComponent<InventoryManager>.Instance)
		{
			maxSelectedItems = SingletonComponent<InventoryManager>.Instance.OccupiedSpaceOnDisplay;
			Dbg.Log("SetSellSelectLimit --- > Max size : " + maxSelectedItems);
		}
	}

	public void ClearSelectedItems()
	{
		_selectedItems.Clear();
		if ((bool)_prevSelectedCell && !_prevSelectedCell.Empty)
		{
			SelectCell(_prevSelectedCell, false);
			return;
		}
		SelectFirstActiveCell();
		if (allowMultipleSelection)
		{
			_selectedItems.Clear();
		}
	}

	private void HideNonEvolvableItems()
	{
		int num = 0;
		foreach (InventoryCell cell in grid.cells)
		{
			ItemDetails data = cell.data;
			if (data != null && data != ItemManager.s_NULL_DATA)
			{
				if (!data.equipment.canEvolve)
				{
					cell.gameObject.SetActiveRecursively(false);
				}
				else
				{
					num++;
				}
			}
		}
		grid.Sort();
		if (num == 0)
		{
			UIPopupController.Instance.ShowOKPopupMSG("common_info", "character_inventory_noselectable");
		}
		Dbg.Log(num + " ItemShown possible to evolve.");
	}

	private void HideNonFuseableItems(bool hideEssence, bool hideEquipped = false)
	{
		foreach (InventoryCell cell in grid.cells)
		{
			ItemDetails data = cell.data;
			if (data != null && (data.IsBaseItem || (hideEssence && data.IsEssence) || (hideEquipped && data.IsEquip)))
			{
				cell.gameObject.SetActiveRecursively(false);
			}
		}
		grid.Sort();
	}

	public void HideNonSellableItems()
	{
		foreach (InventoryCell cell in grid.cells)
		{
			if (!(null == cell) && (!CanSell(cell) || cell.data.IsEquip))
			{
				cell.gameObject.SetActiveRecursively(false);
			}
		}
		grid.Sort();
	}

	private bool CanSell(InventoryCell cell)
	{
		if (cell == null)
		{
			return false;
		}
		ItemDetails data = cell.data;
		if (data == null)
		{
			return false;
		}
		return !cell.Empty && !data.IsBaseItem;
	}

	private void UpdateItemInfo()
	{
		if (!_currSelectedCell)
		{
			m_itemInfo.ShowItem(ItemManager.s_NULL_DATA, false, false);
			return;
		}
		ItemDetails data = _currSelectedCell.data;
		m_itemInfo.ShowItem(data, false);
		bool flag = panelType == PanelType.FUSION || !data.IsEssence;
		if (panelType == PanelType.EQUIP || panelType == PanelType.INVENTORY)
		{
			buttonOk.isEnabled = flag;
			labelItemCost.gameObject.SetActiveRecursively(false);
			labelCostValue.gameObject.SetActiveRecursively(false);
			spriteCoin.gameObject.SetActiveRecursively(false);
		}
		else if (panelType == PanelType.SELL || panelType == PanelType.FUSION_BASEITEM || panelType == PanelType.FUSION)
		{
			buttonOk.isEnabled = true;
		}
		sliderXp.gameObject.SetActiveRecursively(flag);
		XpLabelTitle.gameObject.SetActiveRecursively(flag);
		labelXpValue.gameObject.SetActiveRecursively(flag);
		XpPercent.gameObject.SetActiveRecursively(flag);
		labelXpNextValue.gameObject.SetActiveRecursively(flag);
		if (panelType == PanelType.SELL)
		{
			int num = 0;
			foreach (InventoryCell selectedItem in _selectedItems)
			{
				num += selectedItem.data.Cost;
			}
			labelCostValue.text = num + string.Empty;
		}
		else if (panelType == PanelType.FUSION)
		{
			InventoryCell inventoryCell = null;
			int num2 = 0;
			int num3 = 0;
			if (_disabledItems.Count > 0)
			{
				inventoryCell = _disabledItems[0];
				num2 = inventoryCell.data.Cost;
				num3 = num2 * _selectedItems.Count;
			}
			labelCostValue.text = num3 + string.Empty;
			if (allowMultipleSelection)
			{
				int num4 = 0;
				float num5 = 0f;
				int num6 = 0;
				int num7 = 0;
				if ((bool)inventoryCell)
				{
					num4 = inventoryCell.data.XP;
					foreach (InventoryCell selectedItem2 in _selectedItems)
					{
						num4 += selectedItem2.data.XPForFusion;
					}
					num5 = (float)num4 * 1f / (float)inventoryCell.data.fusion.xpToLevelUp;
					num6 = inventoryCell.data.fusion.xpToLevelUp;
					num7 = inventoryCell.data.XP;
				}
				sliderXpFusion.sliderValue = num5;
				XpPercent.text = string.Format("{0:0}%", num5 * 100f);
				labelXpNextValue.text = num4 + "/" + num6;
				sliderXp.sliderValue = (float)num7 * 1f / (float)num6;
			}
		}
		else
		{
			labelCostValue.text = data.Cost.ToString();
		}
		if (!allowMultipleSelection || (allowMultipleSelection && panelType == PanelType.SELL))
		{
			XpPercent.text = data.XPPercent + "%";
			sliderXp.sliderValue = data.XPPercent / 100f;
			sliderXpFusion.sliderValue = 0f;
			labelXpNextValue.text = data.XP + "/" + data.fusion.xpToLevelUp;
		}
	}

	private void OnItemSelected(GameObject source)
	{
		InventoryCell component = source.GetComponent<InventoryCell>();
		SelectCell(component);
	}

	private void OnClose(bool _success = true)
	{
		ItemSelectEvent itemSelectEvent = new ItemSelectEvent();
		itemSelectEvent.eventType = ItemSelectEventType.ITEM_CANCEL;
		eventTarget.SendMessage("OnItemSelectEvent", itemSelectEvent, SendMessageOptions.DontRequireReceiver);
		Messenger.Invoke("inventory_cancel");
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_cancel");
	}

	private void OnButtonClick(GameObject source)
	{
		if (source == buttonCancel.gameObject)
		{
			if (SingletonComponent<InventoryManager>.Instance != null)
			{
				if (SingletonComponent<TutorialManager>.Instance != null && SingletonComponent<TutorialManager>.Instance.enabled && !SingletonComponent<TutorialManager>.Instance.IsCompleted(Tutorial.State.Inventory))
				{
					Dbg.Log("Change equipment in tutorial");
					SingletonComponent<InventoryManager>.Instance.SaveEquipmentToServer(null, true);
				}
				else
				{
					SingletonComponent<InventoryManager>.Instance.SaveEquipmentToServer();
				}
			}
			OnClose();
			Messenger<PanelType>.Invoke("inventory_closed", panelType);
		}
		else if (source == buttonOk.gameObject)
		{
			if (_selectedItems.Count == 0)
			{
				Dbg.LogWarn("No items are selected.");
				return;
			}
			ItemSelectEvent itemSelectEvent = new ItemSelectEvent();
			itemSelectEvent.selectedItems = new List<InventoryCell>(_selectedItems);
			itemSelectEvent.multipleSelection = allowMultipleSelection;
			switch (panelType)
			{
			case PanelType.EVOLVE:
				itemSelectEvent.eventType = ItemSelectEventType.ITEM_EVOLVE;
				break;
			case PanelType.FUSION:
				itemSelectEvent.eventType = ItemSelectEventType.ITEM_FUSION;
				break;
			case PanelType.SELL:
				itemSelectEvent.eventType = ItemSelectEventType.ITEM_SELL;
				break;
			case PanelType.EQUIP:
			case PanelType.INVENTORY:
				itemSelectEvent.eventType = ItemSelectEventType.ITEM_EQUIP;
				ShowEquipNotification(itemSelectEvent.selectedItems[0].data);
				SingletonComponent<StageManager>.Instance.NeedUpdatePlayerTemp = true;
				break;
			}
			if (allowMultipleSelection && (itemSelectEvent.eventType == ItemSelectEventType.ITEM_FUSION || itemSelectEvent.eventType == ItemSelectEventType.ITEM_SELL))
			{
				int count = _selectedItems.Count;
				string headerKey = string.Empty;
				string empty = string.Empty;
				m_showWarning = false;
				List<ItemDetails> list = new List<ItemDetails>();
				foreach (InventoryCell selectedItem in _selectedItems)
				{
					if (selectedItem.data.IsEquip || selectedItem.data.Tier >= 3)
					{
						list.Add(selectedItem.data);
						m_showWarning = true;
						m_lastEvent = itemSelectEvent;
						switch (itemSelectEvent.eventType)
						{
						case ItemSelectEventType.ITEM_FUSION:
							headerKey = "item_warning_header_fusion";
							break;
						case ItemSelectEventType.ITEM_SELL:
							itemSelectEvent.eventType = ItemSelectEventType.ITEM_SELL_NO_POPUP;
							headerKey = "item_warning_header_sell";
							break;
						}
					}
				}
				if (m_showWarning)
				{
					UIPopupController.Instance.ShowItemWarningPopup(headerKey, empty, list, count == 1, OnItemWarningYes, OnItemWarningNo);
					SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_ok");
					return;
				}
			}
			Messenger<PanelType>.Invoke("inventory_ok", panelType);
			Messenger<PanelType>.Invoke("inventory_closed", panelType);
			eventTarget.SendMessage("OnItemSelectEvent", itemSelectEvent, SendMessageOptions.DontRequireReceiver);
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_ok");
		}
		else if (source == buttonSort.gameObject)
		{
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_ok");
			if (buttonOk != null)
			{
				buttonOk.isEnabled = false;
			}
			if (buttonCancel != null)
			{
				buttonCancel.isEnabled = false;
			}
			buttonSort.isEnabled = false;
			if (filter == Equipment.Part.Max)
			{
				grid.ShuffleTopCategory();
			}
			else
			{
				grid.ShuffleWithinCategory(filter);
			}
			SelectFirstActiveCell();
			MarkExcessCells();
			if (buttonCancel != null)
			{
				buttonCancel.isEnabled = true;
			}
			buttonSort.isEnabled = true;
			m_isGrouped = true;
		}
		else if (source == buttonBuySlots.gameObject)
		{
			if (SingletonComponent<StoreManager>.Instance != null)
			{
				SingletonComponent<StoreManager>.Instance.ShowBagSlotsShop();
			}
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_ok");
		}
	}

	public void ShowEquipNotification(ItemDetails p_item)
	{
		if (p_item == null)
		{
			return;
		}
		if ((bool)m_equipNotification)
		{
			UnityEngine.Object.Destroy(m_equipNotification.gameObject);
		}
		m_equipNotification = UnityEngine.Object.Instantiate(m_equipNotificationPrefab) as EquipNotification;
		if ((bool)m_equipNotification)
		{
			m_equipNotification.transform.parent = base.transform;
			m_equipNotification.transform.localScale = Vector3.one;
			UILabel componentInChildren = MBHier.GetComponentInChildren<UILabel>(m_equipNotification);
			string text = string.Empty;
			switch (p_item.Element)
			{
			case Element.Type.Earth:
				text = "[E]" + NBColorTags.TAG_GREEN;
				break;
			case Element.Type.Fire:
				text = "[F]" + NBColorTags.TAG_RED;
				break;
			case Element.Type.Lightning:
				text = "[L][FFCC00]";
				break;
			case Element.Type.Metal:
				text = "[M][FF00FF]";
				break;
			case Element.Type.Water:
				text = "[W][5998FF]";
				break;
			}
			string text2 = p_item.Name;
			string text3 = "equipped.";
			if ((bool)SingletonComponent<NBLocalization>.Instance)
			{
				text3 = SingletonComponent<NBLocalization>.Instance.Get("inventory_equip_notification_label");
				text2 = SingletonComponent<NBLocalization>.Instance.Get(text2);
			}
			componentInChildren.text = text + text2 + NBColorTags.TAG_END + " " + text3;
		}
	}

	private void OnItemWarningYes()
	{
		if (m_lastEvent != null)
		{
			Dbg.LogWarn(string.Concat("++++ ", m_lastEvent.eventType, " --> ", eventTarget.name));
			if (SingletonComponent<StageManager>.Instance != null)
			{
				SingletonComponent<StageManager>.Instance.NeedUpdatePlayerTemp = true;
			}
			Messenger<PanelType>.Invoke("inventory_ok", panelType);
			eventTarget.SendMessage("OnItemSelectEvent", m_lastEvent, SendMessageOptions.DontRequireReceiver);
		}
	}

	private void OnItemWarningNo()
	{
	}
}
