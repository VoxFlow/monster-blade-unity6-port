using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SellPanel : NBUIMenuPanel
{
	public Component eventTarget;

	private bool _init;

	private bool _first = true;

	public InventoryPanel m_inventory;

	public CharacterInfoPanel m_characterInfoPanel;

	private List<InventoryCell> _items = new List<InventoryCell>();

	private void OnEnable()
	{
		if (_first)
		{
			_first = false;
		}
		else
		{
			Init();
		}
	}

	private void OnDisable()
	{
	}

	private new void Awake()
	{
		m_inventory = MainMenu.Instance.inventory;
	}

	private void Start()
	{
		Init();
	}

	public override void Reset()
	{
		_items.Clear();
		_init = false;
	}

	private void Init()
	{
		if (!_init)
		{
			UIPopupController.SetAllInputsEnabled(false);
			StartCoroutine(DoOpenInventory(0f));
			_init = true;
		}
	}

	private IEnumerator DoOpenInventory(float delay)
	{
		SellEvent evt = new SellEvent
		{
			type = SellEventType.OPEN_INVENTORY
		};
		eventTarget.SendMessage("OnSellEvent", evt, SendMessageOptions.DontRequireReceiver);
		yield return null;
	}

	private new void Update()
	{
	}

	private void OnItemSelectEvent(ItemSelectEvent evt)
	{
		if (evt.eventType == ItemSelectEventType.ITEM_CANCEL)
		{
			Reset();
			SellEvent sellEvent = new SellEvent
			{
				type = SellEventType.CLOSE_INVENTORY
			};
			eventTarget.SendMessage("OnSellEvent", sellEvent, SendMessageOptions.DontRequireReceiver);
		}
		else if (evt.eventType == ItemSelectEventType.ITEM_SELL)
		{
			_items.Clear();
			_items.AddRange(evt.selectedItems);
			string title = SingletonComponent<NBLocalization>.Instance.Get("inventory_sell_header");
			string format = SingletonComponent<NBLocalization>.Instance.Get("inventory_sell_desc");
			string txt = string.Format(format, _items.Count);
			UIPopupController.Instance.ShowMediumPopupMSG(title, txt, OnPopupYes, OnPopupNo, OnPopupClose);
		}
		else if (evt.eventType == ItemSelectEventType.ITEM_SELL_NO_POPUP)
		{
			_items.Clear();
			_items.AddRange(evt.selectedItems);
			OnPopupYes();
		}
	}

	private void OnPopupYes()
	{
		Dbg.LogWarn("SELLING = " + _items.Count + " items.");
		if (_items.Count > 0)
		{
			SingletonComponent<InventoryManager>.Instance.SellItems(_items, OnSellCallback);
			UIPopupController.Instance.ShowLoadingPopup(string.Empty, string.Empty);
		}
	}

	private void OnSellCallback(bool _success, int _num, int _invalidItems, Services.ServiceHandle _handle, bool _regenerateCharacter)
	{
		UIPopupController.Instance.HideLoadingPopup();
		if (_success)
		{
			string text = "equipment_sellsuccess_prompt";
			string text2 = text;
			NBLocalization instance = SingletonComponent<NBLocalization>.Instance;
			if (null != instance)
			{
				text2 = string.Format(instance.Get(text), _num);
			}
			if (_invalidItems == 0)
			{
				UIPopupController.Instance.ShowOKPopupMSG("equipment_sellsuccess_prompt_header", text2, OnSoldPopupClose);
			}
			else
			{
				string text3 = "equipment_sell_invaliditems_prompt";
				string text4 = text3;
				if (null != instance)
				{
					text4 = string.Format(instance.Get(text3), _invalidItems);
				}
				UIPopupController.Instance.ShowOKPopupMSG("equipment_sellsuccess_prompt_header", text2 + "\n" + text4, OnSoldPopupClose);
			}
			m_inventory.RemoveCells(_items);
			if (_regenerateCharacter)
			{
				Dbg.Log("Sell an equipped item, need to regenerate character model");
				if (MainMenu.Instance != null && MainMenu.Instance.m_sharedCharacterInfoPanel != null)
				{
					MainMenu.Instance.m_sharedCharacterInfoPanel.ResetCharacterModel();
				}
			}
			else
			{
				Dbg.Log("No equipped item sold, no need to regenerate character model");
			}
			_items.Clear();
			m_inventory.ClearSelectedItems();
		}
		else if (_handle == null || _handle.IsNoInternet())
		{
			UIPopupController.Instance.ShowNoNetMessage(OnSoldPopupClose);
		}
		else
		{
			ServiceResponseCodes nReturnCode = (ServiceResponseCodes)_handle.nReturnCode;
			UIPopupController.Instance.ShowOKPopupMSG("common_error", "[Error:" + _handle.nReturnCode + "]" + SingletonComponent<NBLocalization>.Instance.Get("sell_bag_item_fail_desc"), OnSoldPopupClose);
		}
	}

	private void OnPopupNo()
	{
		_items.Clear();
	}

	private void OnPopupClose()
	{
	}

	private void OnSoldPopupClose()
	{
	}
}
