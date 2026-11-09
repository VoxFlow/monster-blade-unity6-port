using System.Collections;
using UnityEngine;

public class BagPanel : NBUIMenuPanel
{
	public GameObject eventTarget;

	private bool _init;

	private void OnEnable()
	{
		Init();
	}

	private void OnDisable()
	{
	}

	private void Start()
	{
	}

	public override void Reset()
	{
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
		BagEvent evt = new BagEvent
		{
			type = BagEventType.OPEN_INVENTORY
		};
		eventTarget.SendMessage("OnBagEvent", evt, SendMessageOptions.DontRequireReceiver);
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
			BagEvent bagEvent = new BagEvent
			{
				type = BagEventType.CLOSE_INVENTORY
			};
			eventTarget.SendMessage("OnBagEvent", bagEvent, SendMessageOptions.DontRequireReceiver);
		}
		else
		{
			if (evt.eventType != ItemSelectEventType.ITEM_EQUIP)
			{
				return;
			}
			Reset();
			if (evt.selectedItems == null || evt.selectedItems.Count <= 0)
			{
				return;
			}
			InventoryCell inventoryCell = evt.selectedItems[0];
			ItemDetails data = inventoryCell.data;
			PlayerDataManager instance = SingletonComponent<PlayerDataManager>.Instance;
			if (null != instance)
			{
				PlayerData playerData = instance.GetPlayerData();
				if (playerData != null)
				{
					playerData.Equip(data);
				}
			}
			BagEvent bagEvent2 = new BagEvent
			{
				type = BagEventType.EQUIP_ITEM,
				item = data
			};
			Messenger<ItemDetails>.Invoke("character_item_equipped", data);
			eventTarget.SendMessage("OnBagEvent", bagEvent2, SendMessageOptions.DontRequireReceiver);
		}
	}
}
