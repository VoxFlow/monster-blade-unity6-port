using System.Collections.Generic;
using System.Runtime.InteropServices;
using Models;
using ServiceCalls;
using UnityEngine;

public class StoreManager : SingletonComponent<StoreManager>
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct ProductInfo
	{
		public string ProductIdentifier { get; private set; }

		public string Title { get; private set; }

		public string Description { get; private set; }

		public string Price { get; private set; }

		public string CurrencySymbol { get; private set; }
	}

	public enum ShopItemType
	{
		Premium = 1,
		InGame = 2,
		BagSlot = 3,
		Stamina = 4,
		BattlePoint = 5
	}

	public delegate void CompletedCB(bool _succeed);

	private const int m_defaultBagSlotIncrease = 5;

	private ServerStoreItem m_stamina;

	private int m_defaultStaminaPrice = 5;

	private string m_bagSlotsID = string.Empty;

	private ServerStoreItem m_battlePoint;

	private int m_defaultBattlePointPrice = 1;

	private ServerStoreItem m_bagSlotsServer;

	private int m_numberOfDiamonds;

	private string m_IDofSoldItem = string.Empty;

	private string m_receipt = string.Empty;

	private string m_json = string.Empty;

	private bool m_enableShop = true;

	private bool m_productReceived;

	private Services m_services;

	private PlayerData m_playerData;

	private List<string> m_prices;

	private List<string> m_productsId = new List<string>();

	private List<ServerStoreItem> m_storeItems;

	private List<ServerStoreItem> m_diamondItems;

	private List<ProductInfo> m_productInfo;

	private CompletedCB m_completeCB;

	private int retryCount;

	private static string ShopItemCSV = "ShopItem";

	private static string FreeDiamondColumn = "FREE_DIAMONDS";

	private static string PopularColumn = "MOST_POPULAR";

	private static string BestValueColumn = "BEST_VALUE";

	private static string PositionColumn = "POSITION";

	private int retryProductID;

	private int retryReceipt;

	private int retryCountMax = 5;

	private int m_bagSlotsToPurchase = 1;

	private PopupCallback m_onFail;

	private PopupCallback m_onBack;

	private PopupCallback m_onDiamondBuy;

	private PopupCallback m_onInventoryExcessPurchase;

	private PopupCallback m_onInventoryExcessCancel;

	private PopupCallback m_onBagSlotBuySuccess;

	private PopupCallback m_onBagSlotBuyFail;

	private string m_lastHintMsg = string.Empty;

	private int m_lastMinimumDiamond;

	private List<AndroidIAPOrder> m_orders = new List<AndroidIAPOrder>();

	private AndroidIAPOrder currentOrder;

	public bool IsEnabled
	{
		get
		{
			return m_enableShop;
		}
	}

	public int BagSlotsPrice
	{
		get
		{
			if (m_bagSlotsServer != null)
			{
				return (int)m_bagSlotsServer.price;
			}
			return 5;
		}
	}

	public int StaminaPrice
	{
		get
		{
			if (m_stamina == null)
			{
				return m_defaultStaminaPrice;
			}
			return (int)m_stamina.price;
		}
	}

	public int BattlePointPrice
	{
		get
		{
			if (m_battlePoint == null)
			{
				return m_defaultBattlePointPrice;
			}
			return (int)m_battlePoint.price;
		}
	}

	public List<ServerStoreItem> Diamonds
	{
		get
		{
			return m_diamondItems;
		}
	}

	public bool IsMakingAndroidPurchase
	{
		get
		{
			return currentOrder != null;
		}
	}

	private void Awake()
	{
		m_productInfo = new List<ProductInfo>();
	}

	public void InitManager(List<ServerStoreItem> _storeItems)
	{
		NBProfiler.BeginTask("[STORE_MANAGER][INIT_MANAGER]");
		m_services = SingletonComponent<Services>.Instance;
		if (SingletonComponent<PlayerDataManager>.Instance != null)
		{
			m_playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		}
		m_storeItems = _storeItems;
		GetItems();
		RequestProductData();
		m_enableShop = true;
		Dbg.Log(" STORE : --> OnGetStoreItemSuccessful <--", Dbg.NBLogType.STORE);
		NBProfiler.EndTask("[STORE_MANAGER][INIT_MANAGER]");
	}

	public bool CanMakePayment()
	{
		return NubeePlatformBinding.IsPaymentSupported();
	}

	public void ReEnableStore(CompletedCB _completeCB)
	{
		NBProfiler.BeginTask("[STORE_MANAGER][INIT_MANAGER]");
		m_completeCB = _completeCB;
		m_services = SingletonComponent<Services>.Instance;
		if (SingletonComponent<PlayerDataManager>.Instance != null)
		{
			m_playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		}
		if (m_services != null)
		{
			m_services.GetStoreItems(OnGetStoreItemSuccessful, OnGetStoreItemFailed);
			return;
		}
		Dbg.LogError(" --> Disable Shop <-- ", Dbg.NBLogType.STORE);
		m_enableShop = false;
	}

	private void RequestProductData()
	{
		Dbg.Log("STORE : Requesting ProductData.", Dbg.NBLogType.STORE);
	}

	private void SetDiamondsPrice()
	{
		if (!m_enableShop)
		{
			return;
		}
		string text = string.Empty;
		foreach (ProductInfo item in m_productInfo)
		{
			string text2 = item.CurrencySymbol + item.Price;
			foreach (ServerStoreItem diamondItem in m_diamondItems)
			{
				if (item.ProductIdentifier == diamondItem.productId)
				{
					diamondItem.priceLabel = text2;
					text = text + diamondItem.productId + " " + text2;
				}
			}
		}
		m_productReceived = true;
		Dbg.LogWarn(" --> Shop Enable ?price Set : " + text, Dbg.NBLogType.STORE);
	}

	private void GetItems()
	{
		string text = string.Empty;
		m_diamondItems = new List<ServerStoreItem>(m_storeItems.Count);
		NBBinaryFormat parametersFor = SingletonComponent<ParameterManager>.Instance.GetParametersFor(ShopItemCSV);
		foreach (ServerStoreItem storeItem in m_storeItems)
		{
			if (storeItem.type == 5 && m_battlePoint == null)
			{
				m_battlePoint = storeItem;
				Dbg.LogWarn(" Got the battle point ID : " + m_battlePoint.db_id, Dbg.NBLogType.STORE);
			}
			if (storeItem.type == 4 && m_stamina == null)
			{
				m_stamina = storeItem;
				Dbg.LogWarn(" Got the stamina ID : " + m_stamina.db_id, Dbg.NBLogType.STORE);
			}
			if (storeItem.type == 3 && m_bagSlotsID == string.Empty)
			{
				m_bagSlotsServer = storeItem;
				m_bagSlotsID = storeItem.db_id;
				Dbg.LogWarn(" Got the BagSlots ID : " + m_bagSlotsID, Dbg.NBLogType.STORE);
			}
			if (storeItem.type == 1)
			{
				storeItem.freeDiamonds = parametersFor.GetDataGeneric<int>(storeItem.name, FreeDiamondColumn);
				storeItem.popular = parametersFor.GetDataGeneric<int>(storeItem.name, PopularColumn) > 0;
				storeItem.bestValue = parametersFor.GetDataGeneric<int>(storeItem.name, BestValueColumn) > 0;
				storeItem.positionIndex = parametersFor.GetDataGeneric<int>(storeItem.name, PositionColumn);
				string text2 = text;
				text = text2 + storeItem.db_id + "  " + storeItem.productId + "\n";
				m_diamondItems.Add(storeItem);
				m_productsId.Add(storeItem.productId);
			}
		}
		MBSort.Sort(m_diamondItems, SortDiamondItems);
		Dbg.Log("STORE : --> Got items from MSServer <-- " + text, Dbg.NBLogType.STORE);
	}

	private static int SortDiamondItems(ServerStoreItem p_item1, ServerStoreItem p_item2)
	{
		if (p_item1 == null)
		{
			if (p_item2 == null)
			{
				return 0;
			}
			return -1;
		}
		if (p_item2 == null)
		{
			return 1;
		}
		return p_item1.positionIndex.CompareTo(p_item2.positionIndex);
	}

	private void OnGetStoreItemSuccessful(Services.ServiceHandle _handle)
	{
		m_storeItems = (List<ServerStoreItem>)_handle.returnValue;
		GetItems();
		if (m_completeCB != null)
		{
			m_completeCB(true);
		}
		RequestProductData();
		m_enableShop = true;
		Dbg.Log(" STORE : --> OnGetStoreItemSuccessful <--", Dbg.NBLogType.STORE);
		NBProfiler.EndTask("[STORE_MANAGER][INIT_MANAGER]");
	}

	private void OnGetStoreItemFailed(Services.ServiceHandle _handle, string _error)
	{
		if (retryCount <= retryCountMax)
		{
			if (m_services != null)
			{
				m_services.GetStoreItems(OnGetStoreItemSuccessful, OnGetStoreItemFailed);
			}
		}
		else
		{
			if (m_completeCB != null)
			{
				m_completeCB(false);
			}
			NBProfiler.EndTask("[STORE_MANAGER][INIT_MANAGER]");
			m_enableShop = false;
		}
		retryCount++;
		Dbg.LogError(" STORE : --> OnGetStoreItemFailed <-- " + ServiceCallUtils.GetErrorMessage(_handle), Dbg.NBLogType.STORE);
	}

	private void UpdatePlayerData(Services.ServiceHandle _handle)
	{
		PurchaseDetails purchaseDetails = (PurchaseDetails)_handle.returnValue;
		m_playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		if (m_playerData != null)
		{
			if (purchaseDetails.characterAccountDetails != null && purchaseDetails.characterAccountDetails.nubee_id != string.Empty)
			{
				purchaseDetails.characterAccountDetails.ToPlayerData(ref m_playerData);
			}
			if (null != MainMenu.Instance)
			{
				MainMenu.Instance.RefreshPlayerStats();
			}
		}
	}

	private void OnBuyDiamondFail()
	{
		ShowDiamondsShop(m_onDiamondBuy, m_onBack);
	}

	public void ShowDiamondsShop(PopupCallback _onBuy = null, PopupCallback _onBack = null, string p_hintMsg = null, int p_minimumDiamond = 1)
	{
		if (!CanMakePayment())
		{
			UIPopupController.Instance.ShowOKPopupMSG("shop_header", "shop_diamonds_not_available", _onBack);
		}
		else if (SingletonComponent<PlayerDataManager>.Instance.GetPlayerData().m_userData.isBanFromIAP)
		{
			UIPopupController.Instance.ShowServerError("cancelled_purchase", _onBack, 706);
		}
		else
		{
			if (!m_enableShop)
			{
				return;
			}
			ShowAllDiamonds();
			if (p_minimumDiamond > 1)
			{
				foreach (ServerStoreItem diamondItem in m_diamondItems)
				{
					if (diamondItem.denomination < p_minimumDiamond)
					{
						diamondItem.isHidden = true;
					}
				}
			}
			m_onDiamondBuy = _onBuy;
			m_onBack = _onBack;
			m_onFail = OnBuyDiamondFail;
			m_lastHintMsg = p_hintMsg;
			m_lastMinimumDiamond = p_minimumDiamond;
			UIPopupController.Instance.ShowDiamondShop(OnBack, p_hintMsg);
		}
	}

	public void SelectedDiamond(int _index)
	{
		if (!m_enableShop)
		{
			return;
		}
		if (_index < 0 || _index >= m_diamondItems.Count)
		{
			Dbg.LogError("Wrong item selected", Dbg.NBLogType.STORE);
			return;
		}
		ServerStoreItem serverStoreItem = m_diamondItems[_index];
		if (!CanMakePayment())
		{
			Dbg.LogWarn("Cannot make payment right now.", Dbg.NBLogType.STORE);
			return;
		}
		string productId = serverStoreItem.productId;
		string androidProductID = serverStoreItem.androidProductID;
		m_IDofSoldItem = serverStoreItem.db_id;
		UIPopupController.Instance.ShowLoadingPopup("shop_header", "shop_buying_item");
		Dbg.Log(" Purchasing product ... " + productId + " Android productID : " + androidProductID, Dbg.NBLogType.STORE);
		NubeePlatformBinding.RequestPurchaseV3(androidProductID);
		MBPrefs.SetInt("StoreManager.IsMakingPurchase", 1);
	}

	private void OnBack()
	{
		UIPopupController.Instance.HideLoadingPopup();
		if (m_onBack != null)
		{
			m_onBack();
			m_onBack = null;
		}
		Dbg.Log(" --> Close Diamonds PopUp <-- ", Dbg.NBLogType.STORE);
	}

	public void ShowInsufficientStaminaShop()
	{
		ShowStaminaShop("dungeon_prompt_not_enough_stamina");
	}

	public void ShowStaminaShop(string hintMsg = null)
	{
		if (m_enableShop)
		{
			UIPopupController.Instance.ShowShopStamina(hintMsg, OnStaminaYes, OnStaminaNo);
		}
	}

	public void AutoBuyStamina()
	{
		OnStaminaYes();
	}

	public void AutoBuyBattlePoint()
	{
		OnBattlePointYes();
	}

	private void OnStaminaYes()
	{
		retryCount = 0;
		if (m_stamina == null)
		{
			Dbg.LogError("Stamina storeItem was not initialized.", Dbg.NBLogType.STORE);
		}
		else if (m_services != null)
		{
			UIPopupController.Instance.ShowLoadingPopup("shop_header", "shop_buying_stamina");
			m_services.Purchase(m_services.NubeeId, m_services.DeviceId, m_stamina.db_id, string.Empty, 1, string.Empty, OnBuyStaminaSuccess, OnBuyStaminaFailed);
			Dbg.Log(" --> Buying Stamina <-- ", Dbg.NBLogType.STORE);
		}
	}

	private void OnStaminaNo()
	{
		Dbg.Log(" --> Close Stamina PopUp <-- ", Dbg.NBLogType.STORE);
	}

	private void OnBuyStaminaSuccess(Services.ServiceHandle _handle)
	{
		UIPopupController.Instance.HideLoadingPopup();
		UpdatePlayerData(_handle);
		if (m_playerData != null)
		{
			int stamina = m_playerData.m_userData.stamina;
			Dbg.Log(" --> Buying stamina from server SUCCEED New stamina : " + stamina + " <-- ", Dbg.NBLogType.STORE);
		}
		if (SingletonComponent<NotifyManager>.Instance != null)
		{
			SingletonComponent<NotifyManager>.Instance.ClearLocalNotification();
			SingletonComponent<NotifyManager>.Instance.RegisterAllLocalNotifications();
		}
		UIPopupController.Instance.ShowShopStaminaSuccess();
	}

	private void OnBuyStaminaFailed(Services.ServiceHandle _handle, string _error)
	{
		UIPopupController.Instance.HideLoadingPopup();
		Dbg.LogError("StoreManager::OnBuyStaminaFailed " + ServiceCallUtils.GetErrorMessage(_handle), Dbg.NBLogType.STORE);
		if (_handle == null)
		{
			return;
		}
		if (_handle.IsNoInternet())
		{
			UIPopupController.Instance.ShowNoNetMessage();
			return;
		}
		switch ((ServiceResponseCodes)_handle.nReturnCode)
		{
		case ServiceResponseCodes.EXCEPTION_SHOP_MAX_STAMINA:
		{
			UIPopupController.Instance.ShowShopStaminaFailed("shop_max_stamina");
			if (_handle == null || _handle.returnJson == null || !_handle.returnJson.HasField("data"))
			{
				break;
			}
			JSONObject jSONObject = _handle.returnJson["data"];
			if (jSONObject != null)
			{
				float num = GLOBAL.GAME_SETTINGS.stamina_reset_time;
				float num2 = num;
				int num3 = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData().m_userData.stamina;
				if (jSONObject.HasField("interval_increment"))
				{
					num = (float)jSONObject["interval_increment"].n;
				}
				if (jSONObject.HasField("current_stamina"))
				{
					num3 = (int)jSONObject["current_stamina"].n;
				}
				if (jSONObject.HasField("next_increment"))
				{
					num2 = (float)jSONObject["next_increment"].n;
				}
				Dbg.Log("new stamina values from server are - Interval_increment: " + num + " current_stamina: " + num3 + " next_increment: " + num2);
				GLOBAL.GAME_SETTINGS.stamina_reset_time = num;
				MainMenu.Instance.SetStaminaValue(num3);
				MainMenu.Instance.SyncStaminaCountDown();
			}
			break;
		}
		case ServiceResponseCodes.EXCEPTION_NOT_ENOUGH_CURRENCY:
			UIPopupController.Instance.ShowShopStaminaFailed("shop_buy_stamina_failed", true);
			break;
		default:
			UIPopupController.Instance.ShowServerGenericError(null, _handle.nReturnCode);
			break;
		}
	}

	public void ShowInsufficientBattlePointShop()
	{
		ShowBattlePointShop("dungeon_prompt_not_enough_battlepoint");
	}

	public void ShowBattlePointShop(string hintMsg = null)
	{
		if (m_enableShop)
		{
			UIPopupController.Instance.ShowShopBattlePoint(hintMsg, OnBattlePointYes, OnBattlePointNo);
		}
	}

	private void OnBattlePointYes()
	{
		retryCount = 0;
		if (m_battlePoint == null)
		{
			Dbg.LogError("BattlePoint storeItem was not initialized.", Dbg.NBLogType.STORE);
		}
		else if (m_services != null)
		{
			UIPopupController.Instance.ShowLoadingPopup("shop_header", "shop_buying_battlepoint");
			m_services.Purchase(m_services.NubeeId, m_services.DeviceId, m_battlePoint.db_id, string.Empty, 1, string.Empty, OnBuyBattlePointSuccess, OnBuyBattlePointFailed);
			Dbg.Log(" --> Buying BattlePoint <-- ", Dbg.NBLogType.STORE);
		}
	}

	private void OnBattlePointNo()
	{
		Dbg.Log(" --> Close BattlePoint PopUp <-- ", Dbg.NBLogType.STORE);
	}

	private void OnBuyBattlePointSuccess(Services.ServiceHandle _handle)
	{
		UIPopupController.Instance.HideLoadingPopup();
		UpdatePlayerData(_handle);
		if (m_playerData != null)
		{
			int shield = m_playerData.m_userData.shield;
			Dbg.Log(" --> Buying battle point from server SUCCEED New battle point : " + shield + " <-- ", Dbg.NBLogType.STORE);
		}
		if (SingletonComponent<NotifyManager>.Instance != null)
		{
			SingletonComponent<NotifyManager>.Instance.ClearLocalNotification();
			SingletonComponent<NotifyManager>.Instance.RegisterAllLocalNotifications();
		}
		UIPopupController.Instance.ShowShopBattlePointSuccess();
		Messenger.Invoke("battlepoints_update_points");
	}

	private void OnBuyBattlePointFailed(Services.ServiceHandle _handle, string _error)
	{
		UIPopupController.Instance.HideLoadingPopup();
		Dbg.LogError("StoreManager::OnBuyBattlePointFailed " + ServiceCallUtils.GetErrorMessage(_handle), Dbg.NBLogType.STORE);
		if (_handle == null)
		{
			return;
		}
		if (_handle.IsNoInternet())
		{
			UIPopupController.Instance.ShowNoNetMessage();
			return;
		}
		switch ((ServiceResponseCodes)_handle.nReturnCode)
		{
		case ServiceResponseCodes.EXCEPTION_STORE_MAX_SHIELDS:
		{
			UIPopupController.Instance.ShowShopBattlePointFailed("shop_max_battlepoint");
			if (_handle == null || _handle.returnJson == null || !_handle.returnJson.HasField("data"))
			{
				break;
			}
			JSONObject jSONObject = _handle.returnJson["data"];
			if (jSONObject != null)
			{
				int num = GLOBAL.GAME_SETTINGS.shields_reset_time;
				int num2 = num;
				int num3 = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData().m_userData.shield;
				if (jSONObject.HasField("interval_increment"))
				{
					num = (int)jSONObject["interval_increment"].n;
				}
				if (jSONObject.HasField("current_shields"))
				{
					num3 = (int)jSONObject["current_shields"].n;
				}
				if (jSONObject.HasField("next_increment"))
				{
					num2 = (int)jSONObject["next_increment"].n;
				}
				Dbg.Log("new battle point values from server are - Interval_increment: " + num + " current_shield: " + num3 + " next_increment: " + num2);
				GLOBAL.GAME_SETTINGS.shields_reset_time = num;
				SingletonComponent<PlayerDataManager>.Instance.GetPlayerData().m_userData.shield = num3;
				SingletonComponent<PlayerDataManager>.Instance.GetPlayerData().m_userData.timeNextShield = num2;
				Messenger.Invoke("battlepoints_update_points");
			}
			break;
		}
		case ServiceResponseCodes.EXCEPTION_NOT_ENOUGH_CURRENCY:
			UIPopupController.Instance.ShowShopBattlePointFailed("shop_buy_battlepoint_failed", true);
			break;
		default:
			UIPopupController.Instance.ShowServerGenericError(null, _handle.nReturnCode);
			break;
		}
	}

	public void ShowBagSlotsShop(PopupCallback p_customizedOnNoCb = null)
	{
		if (m_enableShop)
		{
			m_bagSlotsToPurchase = 1;
			if (SingletonComponent<InventoryManager>.Instance.MaxSpace + GLOBAL.GAME_SETTINGS.store_bag_slot_increase_amount > GLOBAL.GAME_SETTINGS.slot_limitation)
			{
				UIPopupController.Instance.ShowOKPopupMSG("common_info", "reach_slot_limitation");
			}
			else if (p_customizedOnNoCb != null)
			{
				UIPopupController.Instance.ShowShopBagSlots(OnBagSlotsYes, p_customizedOnNoCb);
			}
			else
			{
				UIPopupController.Instance.ShowShopBagSlots(OnBagSlotsYes, OnBagSlotsNo);
			}
		}
	}

	private void BuyBagSlots(PopupCallback _onBuy = null, PopupCallback _onFail = null)
	{
		retryCount = 0;
		if (m_bagSlotsID == string.Empty)
		{
			Dbg.LogError("BagSlot ID was not initialized.", Dbg.NBLogType.STORE);
			return;
		}
		m_onBagSlotBuySuccess = _onBuy;
		m_onBagSlotBuyFail = _onFail;
		if (m_services != null)
		{
			UIPopupController.Instance.ShowLoadingPopup("shop_header", "shop_buying_bagslots");
			m_services.Purchase(m_services.NubeeId, m_services.DeviceId, m_bagSlotsID, string.Empty, m_bagSlotsToPurchase, string.Empty, OnBuyBagSlotsSuccess, OnBuyBagSlotsFailed);
			Dbg.Log(" --> Buying BagSlots <-- ", Dbg.NBLogType.STORE);
		}
	}

	private void AutoBuyBagSlots()
	{
		OnBagSlotsYes();
	}

	private void OnBagSlotsYes()
	{
		if (MainMenu.Instance.inventory == null)
		{
			Dbg.LogWarn("The MainMenu.Instance.inventory is null, unable to update slot max number immediately");
			BuyBagSlots();
		}
		else
		{
			BuyBagSlots(UpdateSlotInfo);
		}
	}

	private void UpdateSlotInfo()
	{
		MainMenu.Instance.inventory.UpdateSlotsInfo();
	}

	private void OnBagSlotsNo()
	{
		Dbg.Log(" --> Close Buy bag slots PopUp <-- ", Dbg.NBLogType.STORE);
	}

	private void OnBuyBagSlotsSuccess(Services.ServiceHandle _handle)
	{
		UIPopupController.Instance.HideLoadingPopup();
		UpdatePlayerData(_handle);
		InventoryManager instance = SingletonComponent<InventoryManager>.Instance;
		Dbg.Log(" --> Buying Bag Slots from server SUCCEED\nInventoryMgr : " + instance.OccupiedSpace + "/" + instance.MaxSpace, Dbg.NBLogType.STORE);
		if (m_onBagSlotBuySuccess != null)
		{
			Dbg.Log(" Callback for Bag Slots ", Dbg.NBLogType.STORE);
			m_onBagSlotBuySuccess();
		}
		else
		{
			UIPopupController.Instance.ShowShopBagSlotsSuccess();
		}
	}

	private void OnBuyBagSlotsFailed(Services.ServiceHandle _handle, string _error)
	{
		UIPopupController.Instance.HideLoadingPopup();
		if (_handle != null)
		{
			if (_handle.IsNoInternet())
			{
				UIPopupController.Instance.ShowNoNetMessage(m_onBagSlotBuyFail);
			}
			else if (_handle.nReturnCode == 700)
			{
				UIPopupController.Instance.ShowShopBagSlotsFailed("not_enough_premium", AutoBuyBagSlots, m_onBagSlotBuyFail);
			}
			else if (_handle.nReturnCode == 1004)
			{
				UIPopupController.Instance.ShowOKPopupMSG("common_info", "reach_slot_limitation", m_onBagSlotBuyFail);
			}
			else if (m_onBagSlotBuyFail != null)
			{
				m_onBagSlotBuyFail();
			}
			else
			{
				UIPopupController.Instance.ShowShopBagSlotsFailed();
			}
		}
	}

	public void InventoryExcess(int _bagSlotsNeeded, PopupCallback _onPurchased, PopupCallback _onCancel)
	{
		if (m_enableShop)
		{
			int num = GLOBAL.GAME_SETTINGS.store_bag_slot_increase_amount;
			if (num == 0)
			{
				num = 5;
			}
			int num2 = SingletonComponent<InventoryManager>.Instance.MaxSpace + _bagSlotsNeeded;
			if (num2 > GLOBAL.GAME_SETTINGS.slot_limitation)
			{
				_bagSlotsNeeded = GLOBAL.GAME_SETTINGS.slot_limitation - SingletonComponent<InventoryManager>.Instance.MaxSpace;
			}
			int num3 = Mathf.CeilToInt((float)_bagSlotsNeeded / (float)num);
			int num4 = num3 * num;
			m_numberOfDiamonds = num3 * (int)m_bagSlotsServer.price;
			string format = SingletonComponent<NBLocalization>.Instance.Get("inventory_excess_content");
			string txt = string.Format(format, num4, m_numberOfDiamonds);
			m_onInventoryExcessPurchase = _onPurchased;
			m_onInventoryExcessCancel = _onCancel;
			m_bagSlotsToPurchase = num3;
			UIPopupController.Instance.ShowMediumPopupMSG("inventory_excess_header", txt, OnInventoryExcessBuy, m_onInventoryExcessCancel);
		}
	}

	private void OnInventoryExcessBuy()
	{
		if (m_playerData.m_userData.premiumCurrency < m_numberOfDiamonds)
		{
			ShowDiamondsShop(OnInventoryExcessBuySuccess, m_onInventoryExcessCancel, "shop_buy_bagslots_failed", m_numberOfDiamonds);
		}
		else
		{
			OnInventoryExcessBuySuccess();
		}
	}

	private void OnInventoryExcessBuySuccess()
	{
		BuyBagSlots(m_onInventoryExcessPurchase, m_onInventoryExcessCancel);
	}

	public void ShowAllDiamonds()
	{
		foreach (ServerStoreItem diamondItem in m_diamondItems)
		{
			diamondItem.isHidden = false;
		}
	}

	private void OnEnable()
	{
		CanMakePayment();
		NubeePlatformManager.OnPurchaseStateChanged += purchaseStateChanged;
		NubeePlatformManager.OnPurchaseUserCancelled += purchaseUserCancelled;
		NubeePlatformManager.OnPurchaseItemUnavailable += purchaseItemUnavailable;
		NubeePlatformManager.OnPurchaseOtherError += purchaseOtherError;
	}

	private void OnDisable()
	{
		NubeePlatformManager.OnPurchaseStateChanged -= purchaseStateChanged;
		NubeePlatformManager.OnPurchaseUserCancelled -= purchaseUserCancelled;
		NubeePlatformManager.OnPurchaseItemUnavailable -= purchaseItemUnavailable;
		NubeePlatformManager.OnPurchaseOtherError -= purchaseOtherError;
	}

	private void PurchaseSuccessful()
	{
		MBPrefs.SetInt("StoreManager.IsMakingPurchase", 0);
		UIPopupController.Instance.HideLoadingPopup();
		UIPopupController.Instance.ShowOKPopupMSG("shop_header", "shop_purchase_succeed");
		m_onFail = null;
		Dbg.Log(" --> Purchase Successfull <-- ", Dbg.NBLogType.STORE);
	}

	private void PurchaseCancelled(string _error)
	{
		MBPrefs.SetInt("StoreManager.IsMakingPurchase", 0);
		UIPopupController.Instance.HideLoadingPopup();
		UIPopupController.Instance.ShowOKPopupMSG("shop_header", "shop_purchase_cancelled");
		if (m_onBack != null)
		{
			m_onBack();
		}
		Dbg.LogWarn(" --> Purchase Cancelled <-- " + _error);
	}

	private void PurchaseFailed(string _error)
	{
		MBPrefs.SetInt("StoreManager.IsMakingPurchase", 0);
		UIPopupController.Instance.HideLoadingPopup();
		UIPopupController.Instance.ShowOKPopupMSG("shop_error", "shop_purchase_failed");
		if (m_onFail != null)
		{
			m_onFail();
		}
		Dbg.LogError(" --> Purchase failed <-- " + _error);
	}

	private void RetryPurchase()
	{
		UIPopupController.Instance.ShowLoadingPopup("shop_header", "shop_buying_item");
		m_services.Purchase(m_services.NubeeId, m_services.DeviceId, m_IDofSoldItem, m_receipt, 1, m_json, OnPurchaseSuccessful, OnPurchaseFailed);
	}

	private void CancelRetryPurchase()
	{
		ShowPreviousDiamondsShop();
	}

	public void ShowDiamondReceived(CurrencyDetails p_currencies, PopupCallback p_onBack = null)
	{
		if (p_currencies == null)
		{
			Dbg.LogWarn("No Currencies given.");
			return;
		}
		if (m_playerData == null)
		{
			m_playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		}
		int premiumCurrency = m_playerData.m_userData.premiumCurrency;
		if (null != MainMenu.Instance)
		{
			MainMenu.Instance.SetCurrencies(p_currencies);
		}
		int num = m_playerData.m_userData.premiumCurrency - premiumCurrency;
		Dbg.Log("New Diamonds : " + m_playerData.m_userData.premiumCurrency + "\t\tDiamond Receive : " + num);
		UIPopupController.Instance.ShowDiamondGot(num, p_onBack);
	}

	private void OnPurchaseSuccessful(Services.ServiceHandle _handle)
	{
		MBPrefs.SetInt("StoreManager.IsMakingPurchase", 0);
		int premiumCurrency = m_playerData.m_userData.premiumCurrency;
		UpdatePlayerData(_handle);
		int num = m_playerData.m_userData.premiumCurrency - premiumCurrency;
		Dbg.Log("New Diamonds : " + m_playerData.m_userData.premiumCurrency + "\t\tDiamond Receive : " + num);
		retryReceipt = 0;
		UIPopupController.Instance.HideLoadingPopup();
		UIPopupController.Instance.ShowDiamondGot(num, m_onDiamondBuy);
		Dbg.Log("STORE :  --> Receipt validation from MSServer Done <-- ");
		ConfirmPurchase();
	}

	private void OnPurchaseFailed(Services.ServiceHandle _handle, string _error)
	{
		MBPrefs.SetInt("StoreManager.IsMakingPurchase", 0);
		UIPopupController.Instance.HideLoadingPopup();
		Dbg.LogError("[StoreManager::OnPurchaseFailed] " + ServiceCallUtils.GetErrorMessage(_handle));
		HandleDiamondPurchaseFail(_handle, RetryPurchase, ShowPreviousDiamondsShop, CancelRetryPurchase);
	}

	public void HandleDiamondPurchaseFail(Services.ServiceHandle p_handle, PopupCallback p_retry, PopupCallback p_cancel, PopupCallback p_cancel_n_save_receipt)
	{
		if (p_handle != null && p_handle.returnJson != null)
		{
			JSONObject field = p_handle.returnJson.GetField("payload");
			if (field != null)
			{
				JSONObject field2 = field.GetField("character");
				if (field2 != null)
				{
					CharacterAccountDetails p_character = new CharacterAccountDetails();
					ServiceCallUtils.SetCharacterFromPayload(field2, ref p_character);
					SingletonComponent<PlayerDataManager>.Instance.SetPlayerData(p_character);
				}
			}
		}
		if (p_handle == null)
		{
			UIPopupController.Instance.ShowServerGenericErrorRetry(p_retry, p_cancel_n_save_receipt);
			return;
		}
		if (p_handle.IsNoInternet())
		{
			UIPopupController.Instance.ShowNoNetRetry(p_retry, p_cancel_n_save_receipt);
			return;
		}
		switch (p_handle.nReturnCode)
		{
		case 704:
		case 1003:
			UIPopupController.Instance.ShowOKPopupMSG("common_error", "receipt_hash_invalid", p_cancel);
			break;
		case 701:
		case 702:
		case 703:
		case 705:
			UIPopupController.Instance.ShowMediumPopupMSG("common_error", "receipt_verify_failed_retry", p_retry, p_cancel_n_save_receipt);
			break;
		case 706:
			Dbg.LogWarn("We received a Failed Transaction, Validate it with GooglePlayApp anyway.");
			ConfirmPurchase();
			UIPopupController.Instance.ShowServerError("cancelled_purchase", p_cancel, p_handle.nReturnCode);
			SingletonComponent<PlayerDataManager>.Instance.GetPlayerData().m_userData.isBanFromIAP = true;
			break;
		default:
			UIPopupController.Instance.ShowServerGenericErrorRetry(p_retry, p_cancel_n_save_receipt, p_handle.nReturnCode);
			break;
		}
	}

	private void ShowPreviousDiamondsShop()
	{
		currentOrder = null;
		ShowDiamondsShop(m_onDiamondBuy, m_onBack, m_lastHintMsg, m_lastMinimumDiamond);
	}

	private ServerStoreItem GetServerItemFromProductID(string p_productID)
	{
		if (m_diamondItems == null)
		{
			return null;
		}
		foreach (ServerStoreItem diamondItem in m_diamondItems)
		{
			if (diamondItem.androidProductID == p_productID)
			{
				return diamondItem;
			}
		}
		return null;
	}

	private string GetProductIDFromJSONRecord(string p_json)
	{
		if (p_json == null)
		{
			return string.Empty;
		}
		JSONObject jSONObject = new JSONObject(p_json);
		if (jSONObject != null)
		{
			JSONObject field = jSONObject.GetField("orders");
			if (field != null && field.Count > 0 && !field.IsEmptyArray())
			{
				JSONObject jSONObject2 = field[0];
				return (jSONObject2.GetField("productId") == null) ? string.Empty : jSONObject2.GetField("productId").str;
			}
		}
		return string.Empty;
	}

	private void purchaseStateChanged(GooglePaymentTransaction transaction)
	{
		Dbg.Log("STORE : purchased product: " + transaction);
		if (IsMakingAndroidPurchase)
		{
			Dbg.LogWarn("We are currently processing an order : " + currentOrder.productId + ". Please ignore all the incoming new purchaseStateChanged.");
			return;
		}
		m_json = transaction.signedData;
		m_receipt = transaction.signature;
		AndroidIAPOrder.GetOrdersFromJSON(m_json, ref m_orders);
		PurchaseAndroidOrder();
	}

	private void PurchaseAndroidOrder()
	{
		if (m_orders == null || m_orders.Count <= 0 || m_orders[0] == null)
		{
			Dbg.LogError("Received empty orders from GooglePlay.", Dbg.NBLogType.STORE);
			currentOrder = null;
			return;
		}
		currentOrder = m_orders[0];
		ServerStoreItem serverItemFromProductID = GetServerItemFromProductID(currentOrder.productId);
		if (serverItemFromProductID != null && serverItemFromProductID.db_id != string.Empty)
		{
			m_IDofSoldItem = serverItemFromProductID.db_id;
			Dbg.Log("SUCCESS : " + serverItemFromProductID.db_id + " PID : " + currentOrder.productId, Dbg.NBLogType.STORE);
			UIPopupController.Instance.ShowLoadingPopup("shop_header", "shop_buying_item");
			m_services.Purchase(m_services.NubeeId, m_services.DeviceId, m_IDofSoldItem, m_receipt, 1, m_json, OnPurchaseSuccessful, OnPurchaseFailed);
		}
		else
		{
			currentOrder = null;
			Dbg.LogError("Could not recognise server Item DBID from google play json. PID : " + currentOrder.productId, Dbg.NBLogType.STORE);
		}
	}

	private void ConfirmPurchase()
	{
		NubeePlatformBinding.ConfirmPurchaseV3(currentOrder.originalJson);
		if (IsMakingAndroidPurchase)
		{
			m_orders.Remove(currentOrder);
			if (m_orders.Count > 0)
			{
				Dbg.Log("Move to the next order : " + m_orders[0].productId);
				PurchaseAndroidOrder();
			}
			else
			{
				Dbg.Log("No More order to proceed.");
				currentOrder = null;
			}
		}
	}

	private void purchaseSignatureVerifiedEvent(string signedData, string signature)
	{
		Dbg.Log("STORE : purchaseSignatureVerifiedEvent. signedData: " + signedData + ", signature: " + signature);
	}

	private void purchaseSucceededEvent(string productId)
	{
		Dbg.Log("STORE : purchaseSucceededEvent: " + productId);
		PurchaseSuccessful();
	}

	private void purchaseUserCancelled(string productId)
	{
		Dbg.Log("STORE : purchaseCancelledEvent: " + productId);
		PurchaseCancelled(productId);
	}

	private void purchaseItemUnavailable(string productId)
	{
		Dbg.LogError("STORE : purchaseItemUnavailable: " + productId);
		PurchaseFailed(productId);
	}

	private void purchaseOtherError(string productId)
	{
		Dbg.LogError("STORE : purchaseOtherError: " + productId);
		PurchaseFailed(productId);
	}

	private void purchaseRefundedEvent(string productId)
	{
		Dbg.Log("STORE : purchaseRefundedEvent: " + productId);
	}

	private void purchaseFailedEvent(string productId)
	{
		Dbg.LogError("STORE : purchaseFailedEvent: " + productId);
		PurchaseFailed(productId);
	}

	private void transactionsRestoredEvent()
	{
		Dbg.Log("STORE : transactionsRestoredEvent");
	}

	private void transactionRestoreFailedEvent(string error)
	{
		Dbg.LogError("STORE : transactionRestoreFailedEvent: " + error);
	}

	public bool IsValidPetShopData(ShopDataPet p_petShopData)
	{
		if (p_petShopData == null)
		{
			return false;
		}
		if (p_petShopData != null)
		{
			PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
			bool result = false;
			if (playerData.details != null && playerData.details.unlockedPets != null)
			{
				result = !playerData.details.unlockedPets.ContainsKey(p_petShopData.PetID);
			}
			return result;
		}
		return false;
	}
}
