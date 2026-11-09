using System.Collections;
using System.Collections.Generic;
using ServiceCalls;
using UnityEngine;

public class EvolvePanel : NBUIMenuPanel
{
	public ItemInfo m_templateInfoItem;

	public Transform m_baseItemStatsPosition;

	public Transform m_resultItemStatsPosition;

	private ItemInfo m_baseItemInfo;

	private ItemInfo m_resultItemInfo;

	private InventoryCell m_baseIC;

	private InventoryCell m_resultIC;

	private ItemDetails m_baseItem;

	private ItemDetails m_resultItem;

	public string m_noButton;

	public string m_yesButton;

	public float m_itemRequiredScale = 1f;

	public UILabel XpPercent;

	public UILabel labelXpValue;

	public UISlider sliderXp;

	public UILabel labelCostValue;

	public InventoryCell m_template;

	public Transform[] m_templateListPosition;

	private List<InventoryCell> m_evolveList;

	private List<string> m_evolveListIDs;

	public UIButton m_evolveButton;

	public UISprite m_imageButton;

	public UILabel m_evolveLabel;

	private bool m_haveAllCatalyst;

	public Component eventTarget;

	public CharacterInfoPanel m_characterInfoPanel;

	private bool _init;

	private List<string> m_catalysts = new List<string>();

	private ItemSelectEvent m_lastItemEvent;

	private EvolveEvent m_lastEvolveEvent;

	private int m_retryCount;

	private void OnEnable()
	{
		Init();
	}

	private void OnDisable()
	{
	}

	private void UpdateMembers()
	{
		m_baseIC = m_baseItemInfo.InventoryCell;
		m_baseItem = m_baseIC.data;
		m_resultIC = m_resultItemInfo.InventoryCell;
		m_resultItem = m_resultIC.data;
	}

	private new void Awake()
	{
		m_baseItemInfo = (ItemInfo)Object.Instantiate(m_templateInfoItem, m_baseItemStatsPosition.position, m_baseItemStatsPosition.rotation);
		m_resultItemInfo = (ItemInfo)Object.Instantiate(m_templateInfoItem, m_resultItemStatsPosition.position, m_resultItemStatsPosition.rotation);
		m_baseItemInfo.gameObject.layer = base.gameObject.layer;
		m_resultItemInfo.gameObject.layer = base.gameObject.layer;
		m_baseItemInfo.transform.parent = base.transform;
		m_resultItemInfo.transform.parent = base.transform;
		m_baseItemInfo.InventoryCell.m_size = InventoryCell.Size.Medium;
		m_resultItemInfo.InventoryCell.m_size = InventoryCell.Size.Medium;
		UpdateMembers();
		m_baseItemInfo.transform.parent = m_baseItemStatsPosition.transform.parent;
		m_resultItemInfo.transform.parent = m_resultItemStatsPosition.transform.parent;
		UIButtonMessage component = m_baseIC.GetComponent<UIButtonMessage>();
		component.target = base.gameObject;
		component = m_resultIC.GetComponent<UIButtonMessage>();
		component.target = base.gameObject;
		m_evolveList = new List<InventoryCell>();
		Transform[] templateListPosition = m_templateListPosition;
		foreach (Transform transform in templateListPosition)
		{
			InventoryCell inventoryCell = Object.Instantiate(m_template, transform.position, transform.rotation) as InventoryCell;
			inventoryCell.transform.parent = transform.parent;
			inventoryCell.transform.localScale = transform.localScale * m_itemRequiredScale;
			inventoryCell.SetLayerRecursively(base.gameObject.layer);
			component = inventoryCell.GetComponent<UIButtonMessage>();
			component.target = base.gameObject;
			m_evolveList.Add(inventoryCell);
			inventoryCell.m_size = InventoryCell.Size.Small;
			inventoryCell.data = ItemManager.s_NULL_DATA;
		}
		DisableEvolution();
	}

	public override void OnPush()
	{
		PublicReset();
	}

	public override void Reset()
	{
		m_baseIC.m_hidden = true;
		m_resultIC.m_hidden = true;
		m_baseIC.Empty = true;
		m_resultIC.Empty = true;
		foreach (InventoryCell evolve in m_evolveList)
		{
			evolve.m_hidden = true;
			evolve.Empty = true;
		}
		m_baseItemInfo.ShowItem(m_baseItem, true, false);
		m_resultItemInfo.ShowItem(m_resultItem, true, false);
		UpdateStats();
		DisableEvolution();
		_init = false;
	}

	public void PublicReset()
	{
		m_baseIC.m_hidden = true;
		m_resultIC.m_hidden = true;
		m_baseIC.Empty = true;
		m_resultIC.Empty = true;
		foreach (InventoryCell evolve in m_evolveList)
		{
			evolve.m_hidden = true;
			evolve.Empty = true;
		}
		m_baseItem = ItemManager.s_NULL_DATA;
		m_resultItem = ItemManager.s_NULL_DATA;
		m_baseItemInfo.ShowItem(m_baseItem, true, false);
		m_resultItemInfo.ShowItem(m_resultItem, true, false);
		UpdateStats();
		DisableEvolution();
		_init = false;
	}

	private void Init()
	{
		if (!_init)
		{
			PublicReset();
			UIPopupController.SetAllInputsEnabled(false);
			StartCoroutine(DoOpenInventory(0.3f));
			_init = true;
		}
	}

	private IEnumerator DoOpenInventory(float delay)
	{
		yield return new MBWaitForSeconds(delay);   // PORT (CD-059)
		OnButtonClick(m_baseIC.gameObject);
	}

	private bool CanEvolve(bool _showPopUp = true)
	{
		PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		int cost = m_baseItem.Cost * m_catalysts.Count;
		if (!playerData.HasEnoughIGC(cost))
		{
			Dbg.LogWarn("Not Enough IGC.");
			if (_showPopUp)
			{
				UIPopupController.Instance.ShowOKPopupMSG("error_evolve", "not_enough_igc", OnPopUpClose);
			}
			return false;
		}
		if (!m_haveAllCatalyst)
		{
			Dbg.LogWarn("Doesn't have all the Catalysts.");
			if (_showPopUp)
			{
				UIPopupController.Instance.ShowOKPopupMSG("error_evolve", "not_all_catalysts", OnPopUpClose);
			}
			return false;
		}
		return true;
	}

	private bool CheckEvolve(InventoryCell _cell)
	{
		if (_cell == null || !_cell.IsEquipment())
		{
			Dbg.LogError("The selected Item doesn't have inventoryCell script.");
			return false;
		}
		ItemDetails data = _cell.data;
		if (data == null || data.IsBaseItem)
		{
			Dbg.LogWarn("Cannot evolve BaseItem.");
			UIPopupController.Instance.ShowOKPopupMSG("Evolve failed", "Base item cannot evolve", OnPopUpClose);
			return false;
		}
		if (!data.equipment.canEvolve)
		{
			Dbg.Log("This item cannot evolve.");
			UIPopupController.Instance.ShowOKPopupMSG("Evolve failed", "This item cannot evolve", OnPopUpClose);
			return false;
		}
		EnableEvolution();
		return true;
	}

	private void DisableEvolution()
	{
		m_imageButton.spriteName = m_noButton;
		m_evolveLabel.alpha = 0.5f;
		m_evolveButton.isEnabled = false;
	}

	private void EnableEvolution()
	{
		m_imageButton.spriteName = m_yesButton;
		m_evolveLabel.alpha = 1f;
		m_evolveButton.isEnabled = true;
	}

	private void SetupTheEvolveList()
	{
		if (m_baseItem == null)
		{
			return;
		}
		string empty = string.Empty;
		m_catalysts = m_baseItem.equipment.catalysts;
		string text = empty;
		empty = text + "This item has " + m_catalysts.Count + " catalysts.\n";
		if (m_catalysts.Count > m_evolveList.Count)
		{
			Dbg.LogWarn("More Catalyst found than slot for evolution. Cancel Evolution.");
			return;
		}
		int num = 0;
		int num2 = 0;
		m_haveAllCatalyst = false;
		Dictionary<string, List<ItemDetails>> dictionary = new Dictionary<string, List<ItemDetails>>();
		for (num = 0; num < m_catalysts.Count; num++)
		{
			string text2 = m_catalysts[num];
			if (!(text2 == string.Empty))
			{
				ItemDetails itemDetails = null;
				bool flag = false;
				List<ItemDetails> list = null;
				if (!dictionary.ContainsKey(text2))
				{
					list = SingletonComponent<InventoryManager>.Instance.GetItemsByItemID(text2);
					dictionary.Add(text2, list);
				}
				else
				{
					list = dictionary[text2];
				}
				if (list != null && list.Count > 0)
				{
					itemDetails = list[0];
					list.RemoveAt(0);
				}
				else
				{
					itemDetails = SingletonComponent<InventoryManager>.Instance.CreateItem(text2);
					flag = true;
				}
				m_evolveList[num].data = itemDetails;
				m_evolveList[num].Enable = !flag;
				if (!flag)
				{
					num2++;
					m_evolveList[num].Enable = true;
				}
				empty = empty + text2 + "\n";
			}
		}
		for (; num < m_evolveList.Count; num++)
		{
			m_evolveList[num].Empty = true;
		}
		if (num2 == m_catalysts.Count)
		{
			m_haveAllCatalyst = true;
			if (CanEvolve(false))
			{
				Dbg.LogWarn(" ** This item can Evolve ** ");
				GetEvolveListData();
				EnableEvolution();
			}
		}
	}

	private void GetEvolveListData()
	{
		m_evolveListIDs = new List<string>();
		foreach (InventoryCell evolve in m_evolveList)
		{
			if (evolve.data == ItemManager.s_NULL_DATA)
			{
				break;
			}
			m_evolveListIDs.Add(evolve.data.DB_ID);
		}
	}

	private void GetResult()
	{
		if (m_baseItem != null && !(m_resultIC == null))
		{
			int tier = m_baseItem.Tier + 1;
			ItemDetails item = SingletonComponent<InventoryManager>.Instance.CreateItem(m_baseItem.ItemID, tier);
			m_resultItemInfo.ShowItem(item, true, false);
			UpdateMembers();
			m_resultIC.Enable = true;
		}
	}

	private void UpdateStats()
	{
		if (m_baseItem == ItemManager.s_NULL_DATA)
		{
			XpPercent.text = "0%";
			labelXpValue.text = string.Format(SingletonComponent<NBLocalization>.Instance.Get("evolve_to_next_level_label"), 0);
			sliderXp.sliderValue = 0f;
			labelCostValue.text = "0";
		}
		else
		{
			XpPercent.text = m_baseItem.XPPercent + "%";
			labelXpValue.text = string.Format(SingletonComponent<NBLocalization>.Instance.Get("evolve_to_next_level_label"), m_baseItem.XPForFusion);
			sliderXp.sliderValue = m_baseItem.XPPercent / 100f;
			int num = m_baseItem.Cost * m_catalysts.Count;
			labelCostValue.text = num + string.Empty;
		}
	}

	public bool EvolveOf(InventoryCell cell)
	{
		if (!CheckEvolve(cell))
		{
			return false;
		}
		m_baseItemInfo.ShowItem(cell.data, true, false);
		UpdateMembers();
		SetupTheEvolveList();
		GetResult();
		UpdateStats();
		return true;
	}

	private void OnPopUpClose()
	{
	}

	private void OnButtonClick(GameObject source)
	{
		if (source == m_baseIC.gameObject)
		{
			EvolveEvent evolveEvent = new EvolveEvent();
			evolveEvent.type = EvolveEventType.SELECT_BASE;
			evolveEvent.baseCell = m_baseIC;
			eventTarget.SendMessage("OnEvolveEvent", evolveEvent, SendMessageOptions.DontRequireReceiver);
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_secondary_ui_tap");
			return;
		}
		if (source == m_evolveButton.gameObject)
		{
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_evolution_activation");
			if (m_lastEvolveEvent != null)
			{
				return;
			}
			if (!CanEvolve())
			{
				Dbg.LogWarn("Can't evolve!");
				return;
			}
			EvolveEvent evolveEvent2 = new EvolveEvent();
			evolveEvent2.itemDetails = m_baseItem;
			evolveEvent2.otherDetails = new List<ItemDetails>();
			evolveEvent2.type = EvolveEventType.START_EVOLVE;
			foreach (InventoryCell evolve in m_evolveList)
			{
				if (!evolve.Empty)
				{
					evolveEvent2.otherDetails.Add(evolve.data);
				}
			}
			m_lastEvolveEvent = evolveEvent2;
			List<string> list = new List<string>();
			foreach (ItemDetails otherDetail in evolveEvent2.otherDetails)
			{
				list.Add(otherDetail.DB_ID);
			}
			SingletonComponent<InventoryManager>.Instance.Evolve(evolveEvent2.itemDetails.DB_ID, list, OnEvolveCallback);
			return;
		}
		foreach (InventoryCell evolve2 in m_evolveList)
		{
			if (source == evolve2.gameObject && !evolve2.m_hidden)
			{
				UIPopupController.Instance.ShowItemStats(evolve2.data);
				break;
			}
		}
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_secondary_ui_tap");
	}

	private void OnEvolveCallback(ItemDetails _data, Services.ServiceHandle _handle, bool _regenerateCharacter)
	{
		if (_data == null || _data == ItemManager.s_NULL_DATA)
		{
			Dbg.LogWarn(" --> Evolve Failed UICallback, message = " + ServiceCallUtils.GetErrorMessage(_handle));
			if (_handle == null)
			{
				UIPopupController.Instance.ShowServerGenericError();
			}
			else
			{
				UIPopupController.Instance.ShowServerGenericError(null, _handle.nReturnCode);
			}
			return;
		}
		m_lastEvolveEvent.resultItem = _data;
		m_lastEvolveEvent.regenerateCharacter = _regenerateCharacter;
		m_retryCount = 0;
		if (SingletonComponent<TutorialManager>.Instance != null && SingletonComponent<TutorialManager>.Instance.IsCompleted(Tutorial.State.All))
		{
			SaveData.MsData gameData = SingletonComponent<SaveData>.Instance.GameData;
			bool flag = false;
			if (!gameData.HasEvolvedAnItem)
			{
				gameData.HasEvolvedAnItem = true;
				flag = true;
			}
			if (m_lastEvolveEvent.resultItem.Tier > gameData.HighestEvolvedTier)
			{
				gameData.HighestEvolvedTier = m_lastEvolveEvent.resultItem.Tier;
				flag = true;
			}
			if (flag)
			{
				gameData.HasEvolvedAnItem = true;
			}
		}
		Dbg.LogWarn("last evolve event = " + m_lastEvolveEvent.otherDetails.Count);
		UIPopupController.Instance.ShowEvolveLevelup(m_lastEvolveEvent, OnEvolveLevelupDone);
		MainMenu.Instance.inventory.SetDirty();
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_evolution_complete");
	}

	private void OnRetry()
	{
		m_retryCount++;
		List<string> list = new List<string>();
		foreach (ItemDetails otherDetail in m_lastEvolveEvent.otherDetails)
		{
			list.Add(otherDetail.DB_ID);
		}
		SingletonComponent<InventoryManager>.Instance.Evolve(m_lastEvolveEvent.itemDetails.DB_ID, list, OnEvolveCallback);
	}

	private void OnGiveup()
	{
		m_lastEvolveEvent = null;
	}

	private void OnEvolveLevelupDone()
	{
		PublicReset();
		UIPopupController.Instance.HidePopup();
		if (m_lastEvolveEvent.regenerateCharacter)
		{
			m_characterInfoPanel.RegenerateCharacter();
		}
		MainMenu.Instance.Refresh();
		m_lastEvolveEvent = null;
	}

	private void OnItemSelectEvent(ItemSelectEvent evt)
	{
		if (evt.selectedItems != null && evt.selectedItems.Count > 0)
		{
			InventoryCell cell = evt.selectedItems[0];
			if (EvolveOf(cell))
			{
				int count = m_evolveList.Count;
				string headerKey = "item_warning_header_evolve";
				string footerKey = "item_warning_footer";
				bool flag = false;
				List<ItemDetails> list = new List<ItemDetails>();
				foreach (InventoryCell evolve in m_evolveList)
				{
					if (evolve.data.IsEquip || evolve.data.Tier >= 3)
					{
						list.Add(evolve.data);
						m_lastItemEvent = evt;
						flag = true;
					}
				}
				if (flag)
				{
					UIPopupController.Instance.ShowItemWarningPopup(headerKey, footerKey, list, count == 1, OnItemWarningYes, OnItemWarningNo);
					return;
				}
			}
		}
		eventTarget.SendMessage("OnItemSelectEvent", evt, SendMessageOptions.DontRequireReceiver);
	}

	private void OnItemWarningYes()
	{
		eventTarget.SendMessage("OnItemSelectEvent", m_lastItemEvent, SendMessageOptions.DontRequireReceiver);
	}

	private void OnItemWarningNo()
	{
	}
}
