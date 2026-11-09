using System.Collections;
using System.Collections.Generic;
using ServiceCalls;
using UnityEngine;

public class FusionPanel : NBUIMenuPanel
{
	public GameObject buttonBase;

	public GameObject[] buttonMaterials = new GameObject[6];

	public Transform m_cellsRoot;

	public InventoryCell cellPrefab;

	public InventoryCell baseCell;

	public InventoryCell[] itemCells = new InventoryCell[5];

	public TiersStars tiersTemplate;

	public Transform tiersTransform;

	private GameObject tiersDisplayObj;

	private TiersStars tiersDisplay;

	public UISprite healthItemLabel;

	public UISprite armourItemLabel;

	public UISprite attackItemLabel;

	public UISprite critItemLabel;

	public UILabel itemTitle;

	public UILabel healthItemValue;

	public UILabel armourItemValue;

	public UILabel attackItemValue;

	public UILabel critItemValue;

	public UILabel currentLevelItemValue;

	public UILabel percentage;

	public UILabel sellValue;

	public UISlider percentageBar;

	public UILabel currentXp;

	public UISlider m_sliderFusionXp;

	public UISprite m_spriteMax;

	public UIButton buttonAction;

	public Component eventTarget;

	public CharacterInfoPanel m_characterInfoPanel;

	private FusionEvent m_lastFusionEvent;

	private int m_retryCount;

	private BackButtonMapping m_backButtonMapper;

	private void OnEnable()
	{
		Init();
	}

	private void OnDisable()
	{
		Reset();
	}

	public override void OnPush()
	{
	}

	public override void OnPop()
	{
	}

	public override void Reset()
	{
		if ((bool)baseCell)
		{
			baseCell.m_hidden = true;
			baseCell.Empty = true;
		}
		InventoryCell[] array = itemCells;
		foreach (InventoryCell inventoryCell in array)
		{
			inventoryCell.m_hidden = true;
			inventoryCell.Empty = true;
		}
		SetFuseButtonEnabled(false);
		UpdateBaseInfo();
		UISprite componentInChildren = MBHier.GetComponentInChildren<UISprite>(buttonBase);
		if (componentInChildren != null)
		{
			if (baseCell.data == null)
			{
				componentInChildren.enabled = true;
			}
			else
			{
				Dbg.Log("Something is still on the base cell, no need to enable the background");
			}
		}
		GameObject[] array2 = buttonMaterials;
		foreach (GameObject gameObject in array2)
		{
			UISprite componentInChildren2 = MBHier.GetComponentInChildren<UISprite>(gameObject);
			if (componentInChildren2 != null)
			{
				componentInChildren2.enabled = true;
			}
		}
	}

	private IEnumerator DoReset()
	{
		yield return null;
		Reset();
	}

	private void SetFuseButtonEnabled(bool p_enabled)
	{
		UISprite componentInChildren = MBHier.GetComponentInChildren<UISprite>(buttonAction);
		buttonAction.isEnabled = p_enabled;
		if (!componentInChildren)
		{
			Dbg.LogWarn("Cannot find image for buttonAction");
			return;
		}
		if (p_enabled)
		{
			if (componentInChildren != null)
			{
				componentInChildren.spriteName = "buttons/green_btn";
			}
			TweenScale tweenScale = TweenScale.Begin(buttonAction.gameObject, 0.5f, Vector3.one * 1.1f);
			tweenScale.from = Vector3.one;
			tweenScale.method = UITweener.Method.EaseIn;
			tweenScale.style = UITweener.Style.PingPong;
			return;
		}
		if (componentInChildren != null)
		{
			componentInChildren.spriteName = "buttons/grey_btn";
		}
		TweenScale component = buttonAction.GetComponent<TweenScale>();
		if ((bool)component)
		{
			component.enabled = false;
			buttonAction.transform.localScale = Vector3.one;
		}
	}

	private new void Awake()
	{
		tiersDisplayObj = (GameObject)Object.Instantiate(tiersTemplate.gameObject, tiersTransform.position, tiersTransform.rotation);
		tiersDisplayObj.transform.parent = tiersTransform.parent;
		tiersDisplay = tiersDisplayObj.GetComponent<TiersStars>();
		tiersDisplay.SetEmptyTier();
		cellPrefab.m_hidden = true;
		baseCell = Object.Instantiate(cellPrefab) as InventoryCell;
		baseCell.transform.parent = m_cellsRoot;
		Vector3 localPosition = buttonBase.transform.localPosition;
		localPosition.z = -10f;
		baseCell.transform.localPosition = localPosition;
		baseCell.transform.localScale = Vector3.one * 1.3f;
		baseCell.SetLayerForAllChildren(base.gameObject.layer);
		UIButtonMessage component = baseCell.GetComponent<UIButtonMessage>();
		component.target = base.gameObject;
		Object.Destroy(baseCell.GetComponent<UIDragPanelContents>());
		for (int i = 0; i < itemCells.Length; i++)
		{
			itemCells[i] = Object.Instantiate(cellPrefab) as InventoryCell;
			itemCells[i].transform.parent = m_cellsRoot;
			localPosition = buttonMaterials[i].transform.localPosition;
			localPosition.z = -10f;
			itemCells[i].transform.localPosition = localPosition;
			itemCells[i].transform.localScale = Vector3.one * 0.72f;
			itemCells[i].SetLayerForAllChildren(base.gameObject.layer);
			component = itemCells[i].GetComponent<UIButtonMessage>();
			component.target = base.gameObject;
			Object.Destroy(itemCells[i].GetComponent<UIDragPanelContents>());
		}
		m_spriteMax.enabled = false;
		m_backButtonMapper = Object.FindObjectOfType(typeof(BackButtonMapping)) as BackButtonMapping;
	}

	private void Start()
	{
	}

	private void Init()
	{
		if (null != baseCell && baseCell.Empty)
		{
			baseCell.m_hidden = true;
			for (int i = 0; i < itemCells.Length; i++)
			{
				itemCells[i].m_hidden = true;
			}
			if (tiersDisplay != null)
			{
				tiersDisplay.SetEmptyTier();
			}
			SetFuseButtonEnabled(false);
			UpdateBaseInfo();
			UISprite componentInChildren = MBHier.GetComponentInChildren<UISprite>(buttonBase);
			if (componentInChildren != null)
			{
				componentInChildren.enabled = true;
			}
		}
	}

	private IEnumerator DoOpenInventory(float delay)
	{
		yield return new MBWaitForSeconds(delay);   // PORT (CD-059)
		OnButtonClick(baseCell.gameObject);
	}

	private new void Update()
	{
	}

	public void SetBaseCell(ItemDetails data)
	{
		baseCell.m_size = InventoryCell.Size.Large;
		baseCell.data = data;
		baseCell.m_hidden = false;
		baseCell.Empty = false;
		InventoryCell[] array = itemCells;
		foreach (InventoryCell inventoryCell in array)
		{
			if (!inventoryCell.Empty && inventoryCell.data.DB_ID == baseCell.data.DB_ID)
			{
				inventoryCell.m_hidden = true;
				inventoryCell.Empty = true;
			}
		}
		string p_reason = string.Empty;
		SetFuseButtonEnabled(CanFuse(out p_reason));
		UpdateBaseInfo();
	}

	public void SetBaseCell(InventoryCell cell)
	{
		if (cell == null)
		{
			baseCell.Empty = true;
			return;
		}
		baseCell.cellIndex = cell.cellIndex;
		UISprite componentInChildren = MBHier.GetComponentInChildren<UISprite>(buttonBase);
		if (componentInChildren != null)
		{
			componentInChildren.enabled = false;
		}
		SetBaseCell(cell.data);
	}

	private void UpdateBaseInfo()
	{
		if (baseCell.Empty)
		{
			itemTitle.text = string.Empty;
			healthItemValue.text = string.Empty;
			armourItemValue.text = string.Empty;
			currentLevelItemValue.text = string.Empty;
			percentage.text = "0%";
			sellValue.text = "0";
			percentageBar.sliderValue = 0f;
			m_sliderFusionXp.sliderValue = 0f;
			currentXp.text = string.Empty;
			currentXp.text = "0/0";
			healthItemValue.text = "0";
			armourItemValue.text = "0";
			attackItemValue.text = "0";
			critItemValue.text = "0.00%";
			m_spriteMax.enabled = false;
			tiersDisplay.SetEmptyTier();
			return;
		}
		ItemDetails data = baseCell.data;
		itemTitle.text = SingletonComponent<NBLocalization>.Instance.Get(data.Name);
		tiersDisplay.SetTier(data);
		int num = Mathf.RoundToInt(data.itemTypeWithSkill.hp);
		int num2 = Mathf.RoundToInt(data.itemTypeWithSkill.armour);
		int num3 = Mathf.RoundToInt(data.itemTypeWithSkill.minDamage);
		int num4 = Mathf.RoundToInt(data.itemTypeWithSkill.maxDamage);
		float critical = data.itemTypeWithSkill.critical;
		healthItemValue.text = ((!data.IsWeapon || num <= 0) ? string.Empty : "+") + num;
		armourItemValue.text = ((!data.IsWeapon || num2 <= 0) ? string.Empty : "+") + num2;
		attackItemValue.text = ((!data.IsArmour || num3 <= 0) ? string.Empty : "+") + num3 + ((!data.IsArmour) ? ("-" + num4) : string.Empty);
		critItemValue.text = ((!data.IsArmour || !(critical > 0f)) ? string.Empty : "+") + string.Format("{0:0.00}%", critical);
		int num5 = 0;
		int num6 = 0;
		InventoryCell[] array = itemCells;
		foreach (InventoryCell inventoryCell in array)
		{
			if (!inventoryCell.Empty && inventoryCell.data != null)
			{
				num5 += inventoryCell.data.XPForFusion;
				num6++;
			}
		}
		num5 += data.XP;
		sellValue.text = data.Cost * num6 + string.Empty;
		currentXp.text = num5 + "/" + data.fusion.xpToLevelUp;
		float num7 = (float)num5 / (float)data.fusion.xpToLevelUp;
		percentage.text = string.Format("{0:0}%", num7 * 100f);
		percentageBar.sliderValue = (float)data.XP * 1f / (float)data.fusion.xpToLevelUp;
		currentLevelItemValue.text = "LVL " + data.Level + "/" + data.MaxLevelInTier;
		m_sliderFusionXp.sliderValue = ((!(num7 > 1f)) ? num7 : 1f);
		m_spriteMax.enabled = data.Level >= data.MaxLevelInTier && data.IsMaxXP;
	}

	public int CalculateFusionCost()
	{
		int num = 0;
		InventoryCell[] array = itemCells;
		foreach (InventoryCell inventoryCell in array)
		{
			if (!inventoryCell.Empty && inventoryCell.data != null)
			{
				num++;
			}
		}
		ItemDetails data = baseCell.data;
		return data.Cost * num;
	}

	public bool CanFuse(out string p_reason)
	{
		PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		int num = CalculateFusionCost();
		if (num > playerData.Igc)
		{
			p_reason = "fusion_not_enough_gold_header";
			return false;
		}
		int num2 = 0;
		for (int i = 0; i < itemCells.Length; i++)
		{
			if (!itemCells[i].Empty)
			{
				num2++;
			}
		}
		if (num2 > 0 && baseCell.data != ItemManager.s_NULL_DATA && (baseCell.data.Level < baseCell.data.MaxLevelInTier || !baseCell.data.IsMaxXP))
		{
			p_reason = string.Empty;
			return true;
		}
		if ("-5" == baseCell.data.DB_ID)
		{
			p_reason = "fusion_no_base_item";
			return false;
		}
		p_reason = "fusion_max_level_desc";
		return false;
	}

	public void SetMaterialCells(List<InventoryCell> cells)
	{
		int num = Mathf.Clamp(cells.Count, 0, itemCells.Length);
		for (int i = 0; i < itemCells.Length; i++)
		{
			itemCells[i].m_hidden = true;
			itemCells[i].Empty = true;
		}
		for (int j = 0; j < num; j++)
		{
			itemCells[j].m_hidden = false;
			itemCells[j].m_size = InventoryCell.Size.Small;
			itemCells[j].data = cells[j].data;
			itemCells[j].cellIndex = cells[j].cellIndex;
			UISprite componentInChildren = MBHier.GetComponentInChildren<UISprite>(buttonMaterials[j]);
			if (componentInChildren != null)
			{
				componentInChildren.enabled = false;
			}
		}
		for (int k = num; k < buttonMaterials.Length; k++)
		{
			UISprite componentInChildren2 = MBHier.GetComponentInChildren<UISprite>(buttonMaterials[k]);
			if (componentInChildren2 != null)
			{
				componentInChildren2.enabled = true;
			}
		}
		string p_reason = string.Empty;
		SetFuseButtonEnabled(CanFuse(out p_reason));
		if (!string.IsNullOrEmpty(p_reason))
		{
			Dbg.Log("Need to disable the start fusion button");
			if (UIPopupController.Instance != null)
			{
				UIPopupController.Instance.ShowOKPopupMSG("common_info", p_reason);
			}
		}
		UpdateBaseInfo();
	}

	public void SetCurrentXP(float _newXP)
	{
		currentXp.text = _newXP.ToString();
	}

	private void OnButtonClick(GameObject source)
	{
		if (!eventTarget)
		{
			eventTarget = parentScreen;
		}
		if (source == baseCell.gameObject)
		{
			FusionEvent fusionEvent = new FusionEvent();
			fusionEvent.baseCell = ((!baseCell.Empty) ? baseCell : null);
			fusionEvent.eventType = FusionEventType.SELECT_BASE;
			eventTarget.SendMessage("OnFusionEvent", fusionEvent, SendMessageOptions.DontRequireReceiver);
			Messenger.Invoke("fusion_select_base");
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_secondary_ui_tap");
			return;
		}
		if (source == buttonAction.gameObject)
		{
			if (m_lastFusionEvent != null)
			{
				return;
			}
			if (baseCell.Empty)
			{
				Dbg.LogWarn("Base item is empty, cant FUSE!");
				return;
			}
			int num = 0;
			InventoryCell[] array = itemCells;
			foreach (InventoryCell inventoryCell in array)
			{
				if (!inventoryCell.Empty)
				{
					num++;
				}
			}
			if (num == 0)
			{
				Dbg.LogWarn("Materials are empty, cant FUSE!");
				return;
			}
			UIPopupController.Instance.ShowLoadingPopup(string.Empty, string.Empty);
			FusionEvent fusionEvent2 = new FusionEvent();
			fusionEvent2.eventType = FusionEventType.START_FUSION;
			fusionEvent2.baseItem = baseCell.data;
			fusionEvent2.otherItems = new List<ItemDetails>();
			List<string> list = new List<string>();
			InventoryCell[] array2 = itemCells;
			foreach (InventoryCell inventoryCell2 in array2)
			{
				if (!inventoryCell2.Empty)
				{
					fusionEvent2.otherItems.Add(inventoryCell2.data);
					list.Add(inventoryCell2.data.DB_ID);
				}
			}
			m_lastFusionEvent = fusionEvent2;
			if (null != m_backButtonMapper)
			{
				m_backButtonMapper.enabled = false;
			}
			SingletonComponent<InventoryManager>.Instance.Fuse(fusionEvent2.baseItem.DB_ID, list, OnFuseCallback);
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_ok");
			return;
		}
		if (baseCell.data.Level < baseCell.data.MaxLevelInTier || !baseCell.data.IsMaxXP)
		{
			Dbg.Log("Base item level is still able to level up");
			ShowMaterialSelectionInventory();
		}
		else if (baseCell.data.DB_ID == "-5" || baseCell.data == null)
		{
			Dbg.Log("There is no base item, just show the inventory");
			if (UIPopupController.Instance != null)
			{
				UIPopupController.Instance.ShowOKPopupMSG("common_info", "fusion_no_base_item");
			}
		}
		else
		{
			Dbg.Log("Base item level reaches max level, unable to level up");
			if (UIPopupController.Instance != null && SingletonComponent<NBLocalization>.Instance != null)
			{
				UIPopupController.Instance.ShowOKPopupMSG("fusion_max_level", "fusion_max_level_desc");
			}
		}
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_secondary_ui_tap");
	}

	private void ShowMaterialSelectionInventory()
	{
		FusionEvent fusionEvent = new FusionEvent();
		fusionEvent.eventType = FusionEventType.SELECT_ITEMS;
		fusionEvent.baseCell = baseCell;
		fusionEvent.materialCells = new List<InventoryCell>();
		InventoryCell[] array = itemCells;
		foreach (InventoryCell inventoryCell in array)
		{
			if (!inventoryCell.Empty)
			{
				fusionEvent.materialCells.Add(inventoryCell);
			}
		}
		Messenger.Invoke("fusion_select_material");
		eventTarget.SendMessage("OnFusionEvent", fusionEvent, SendMessageOptions.DontRequireReceiver);
		Dbg.Log("ShowMaterialSelectionInventory()");
	}

	private void OnFusionLevelupDone()
	{
		Reset();
		SetBaseCell(m_lastFusionEvent.resultItem);
		UIPopupController.Instance.HidePopup();
		MainMenu.Instance.Refresh();
		m_lastFusionEvent = null;
		if (null != m_backButtonMapper)
		{
			m_backButtonMapper.enabled = true;
		}
		Messenger.Invoke("fusion_done");
	}

	private void OnFuseCallback(ItemDetails item, Services.ServiceHandle _handle, bool _regenerateCharacter)
	{
		if (item != null && m_lastFusionEvent != null)
		{
			m_lastFusionEvent.resultItem = item;
			m_lastFusionEvent.regenerateCharacter = _regenerateCharacter;
			if (SingletonComponent<TutorialManager>.Instance.IsCompleted(Tutorial.State.All))
			{
				SaveData.MsData gameData = SingletonComponent<SaveData>.Instance.GameData;
				gameData.FusionSuccessCount++;
				gameData.SaveAchievementData();
			}
			m_retryCount = 0;
			UIPopupController.Instance.HideLoadingPopup();
			UIPopupController.Instance.ShowFusionLevelup(m_lastFusionEvent, OnFusionLevelupDone);
			SetFuseButtonEnabled(false);
			MainMenu.Instance.inventory.SetDirty();
			Messenger.Invoke("fusion_start");
		}
		else
		{
			UIPopupController.Instance.HideLoadingPopup();
			Dbg.LogWarn(" --> Fusion Failed UICallback <-- " + ServiceCallUtils.GetErrorMessage(_handle));
			if (_handle == null || _handle.IsNoInternet())
			{
				UIPopupController.Instance.ShowNoNetMessage(OnGiveup);
			}
			else
			{
				ServiceResponseCodes nReturnCode = (ServiceResponseCodes)_handle.nReturnCode;
				UIPopupController.Instance.ShowOKPopupMSG("common_error", ServiceCallUtils.FormatMessage("server_generic_error", _handle), OnGiveup);
			}
			if (null != m_backButtonMapper)
			{
				m_backButtonMapper.enabled = true;
			}
		}
	}

	private void OnRetry()
	{
		m_retryCount++;
		List<string> list = new List<string>();
		foreach (ItemDetails otherItem in m_lastFusionEvent.otherItems)
		{
			list.Add(otherItem.DB_ID);
		}
		SingletonComponent<InventoryManager>.Instance.Fuse(m_lastFusionEvent.baseItem.DB_ID, list, OnFuseCallback);
	}

	private void OnGiveup()
	{
		m_lastFusionEvent = null;
		UIPopupController.Instance.HideLoadingPopup();
	}

	public void ForceReset()
	{
		StartCoroutine(DoReset());
	}

	public void ForceResetAndSetBaseCell(InventoryCell cell)
	{
		StartCoroutine(DoResetAndSetBaseCell(cell));
	}

	private IEnumerator DoResetAndSetBaseCell(InventoryCell cell)
	{
		ItemDetails data = cell.data;
		yield return null;
		Reset();
		SetBaseCell(data);
	}

	private IEnumerator DoSetMaterialCells(List<InventoryCell> cells)
	{
		List<InventoryCell> temp = new List<InventoryCell>(cells);
		yield return null;
		SetMaterialCells(temp);
	}

	private IEnumerator DoSetBaseCell(InventoryCell cell)
	{
		yield return null;
		SetBaseCell(cell);
	}

	private void OnItemSelectEvent(ItemSelectEvent evt)
	{
		if (evt.eventType != ItemSelectEventType.ITEM_CANCEL && evt.eventType == ItemSelectEventType.ITEM_FUSION)
		{
			if (!evt.multipleSelection)
			{
				StartCoroutine(DoSetBaseCell(evt.selectedItems[0]));
			}
			else
			{
				StartCoroutine(DoSetMaterialCells(evt.selectedItems));
			}
		}
		if (!eventTarget)
		{
			eventTarget = parentScreen;
		}
		eventTarget.SendMessage("OnItemSelectEvent", evt, SendMessageOptions.DontRequireReceiver);
	}
}
