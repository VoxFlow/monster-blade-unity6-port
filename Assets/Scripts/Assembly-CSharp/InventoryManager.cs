using System.Collections.Generic;
using System.Text;
using Models;
using ServiceCalls;
using UnityEngine;

public class InventoryManager : SingletonComponent<InventoryManager>
{
	public delegate void CallbackForUI(ItemDetails _itemData, string _serverMsg);

	public delegate void SuccessfulCallback(bool _successful, string _serverMsg);

	public delegate void SellCallback(bool _success, int _numSold, int _numInvalid, Services.ServiceHandle _handle, bool _regenerateCharacter);

	public delegate void FuseEvolveCallBack(ItemDetails _itemData, Services.ServiceHandle _handle, bool _regenerateCharacter);

	public delegate void SaveEquipOnServer(bool _success);

	private PlayerData m_playerData;

	private PlayerDataManager m_playerDataMgr;

	private ItemManager m_itemManager;

	private Services m_services;

	private Dictionary<string, ItemDetails> m_inventory = new Dictionary<string, ItemDetails>();

	private int m_maxSpace;

	private int m_occupiedSpace;

	private int m_currentPage;

	private int m_pages;

	private int m_cost;

	private ItemDetails m_tempItem;

	private List<string> m_tempItemList;

	private SuccessfulCallback m_refreshCallback;

	private SellCallback m_sellCallbackNum;

	private FuseEvolveCallBack m_fuseEvolveCallback;

	private SaveEquipOnServer m_saveEquipmentOnServer;

	private uint m_nRequestId;

	private List<ItemDetails> m_baseItems = new List<ItemDetails>();

	private List<ItemDetails> m_temporaryItems = new List<ItemDetails>();

	private List<ItemDetails> m_newItems;

	private List<ItemDetails> m_newNameItems;

	private HashSet<string> m_newCollectionItems = new HashSet<string>();

	private HashSet<string> m_newSpecialCollectionItems = new HashSet<string>();

	private bool m_newCollectionItemsDirty;

	private bool m_newItemsListDirty;

	private bool m_isLoaded;

	private ItemVisualManager.CompletedCB m_onVisualLoaded;

	public Dictionary<string, ItemDetails> GetInventory
	{
		get
		{
			if (!IsLoaded)
			{
				Dbg.LogWarn("Inventory is not loaded.", Dbg.NBLogType.INVENTORY);
			}
			return m_inventory;
		}
	}

	public int OccupiedSpace
	{
		get
		{
			UpdateBagSpace();
			return m_occupiedSpace;
		}
	}

	public int MaxSpace
	{
		get
		{
			UpdateBagSpace();
			return m_maxSpace;
		}
	}

	public bool isMaxSpaceLimitReached
	{
		get
		{
			return MaxSpace >= GLOBAL.GAME_SETTINGS.slot_limitation;
		}
	}

	public int OccupiedSpaceOnDisplay
	{
		get
		{
			return OccupiedSpace - GLOBAL.NUM_BASE_ITEMS;
		}
	}

	public int MaxSpaceOnDisplay
	{
		get
		{
			return MaxSpace - GLOBAL.NUM_BASE_ITEMS;
		}
	}

	public bool IsLoaded
	{
		get
		{
			return m_isLoaded;
		}
	}

	private bool CheckInputs()
	{
		m_playerDataMgr = SingletonComponent<PlayerDataManager>.Instance;
		if (m_playerDataMgr == null)
		{
			return false;
		}
		m_playerData = m_playerDataMgr.GetPlayerData();
		if (m_playerData == null)
		{
			return false;
		}
		m_itemManager = SingletonComponent<ItemManager>.Instance;
		if (m_itemManager == null)
		{
			return false;
		}
		m_playerDataMgr = SingletonComponent<PlayerDataManager>.Instance;
		if (m_playerDataMgr == null)
		{
			return false;
		}
		m_services = SingletonComponent<Services>.Instance;
		if (m_services == null)
		{
			return false;
		}
		return true;
	}

	private void ResetCallBacksData()
	{
		m_tempItem = null;
		m_tempItemList = null;
		m_sellCallbackNum = null;
		m_refreshCallback = null;
	}

	public void InitManager(List<ServerInventoryItem> _serverInventoryItems, SuccessfulCallback p_callback)
	{
		if (!CheckInputs())
		{
			base.enabled = false;
			return;
		}
		Unload();
		ResetCallBacksData();
		CreateBaseItems();
		m_currentPage = 1;
		PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		m_maxSpace = playerData.m_userData.inventoryMaxSize;
		m_occupiedSpace = playerData.m_userData.inventoryUsedSize;
		m_pages = m_occupiedSpace / m_services.MaxPagination + 1;
		m_refreshCallback = p_callback;
		FillInventory(_serverInventoryItems);
		LoadNewCollectionItemsFromPlayerPrefs();
	}

	private void CreateBaseItems()
	{
		m_baseItems.Clear();
		for (int i = 0; i < 5; i++)
		{
			m_baseItems.Add(ItemManager.s_NULL_DATA);
		}
	}

	private void UpdateBagSpace()
	{
		if (m_playerData != null && m_playerData.m_userData != null)
		{
			m_maxSpace = m_playerData.m_userData.inventoryMaxSize;
			m_occupiedSpace = m_playerData.m_userData.inventoryUsedSize;
		}
	}

	public void FillInventory(List<ServerInventoryItem> _serverInventoryItem)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(" -- server Page : " + m_currentPage + " with itemCount : " + _serverInventoryItem.Count + " -- \n");
		for (int i = 0; i < _serverInventoryItem.Count; i++)
		{
			ItemDetails itemDetails = AddItemFromServer(_serverInventoryItem[i], false);
			stringBuilder.AppendLine(itemDetails.ItemID);
			if (itemDetails.IsBaseItem)
			{
				int part = (int)itemDetails.equipment.part;
				m_baseItems[part] = itemDetails;
				Dbg.Log("Base item Set for part : " + itemDetails.equipment.part, Dbg.NBLogType.INVENTORY);
			}
		}
		Dbg.Log(stringBuilder.ToString(), Dbg.NBLogType.INVENTORY);
		stringBuilder = null;
		if (m_refreshCallback != null)
		{
			m_refreshCallback(true, string.Empty);
		}
		m_refreshCallback = null;
		PrintInventory();
		m_isLoaded = true;
		LoadNewItemsState();
	}

	public void OnFillPlayerInventory(Services.ServiceHandle _handle)
	{
		FillInventory((List<ServerInventoryItem>)_handle.returnValue);
	}

	public void OnFillPlayerInventoryFailed(Services.ServiceHandle _handle, string _errorString)
	{
		Dbg.LogError("Could not retreive player inventory from server. Need to assign default inventory. NOT DONE!ErrMsg: " + _errorString + " - " + ServiceCallUtils.GetErrorMessage(_handle), Dbg.NBLogType.INVENTORY);
		if (m_refreshCallback != null)
		{
			m_refreshCallback(false, _errorString);
		}
		m_refreshCallback = null;
	}

	private void OnFuseSuccess(Services.ServiceHandle _handle)
	{
		FusionResult fusionResult = (FusionResult)_handle.returnValue;
		bool regenerateCharacter = false;
		Remove(m_tempItem.DB_ID);
		foreach (string tempItem in m_tempItemList)
		{
			ItemDetails byDBID = GetByDBID(tempItem);
			if (byDBID != null)
			{
				if (byDBID.IsEquip)
				{
					regenerateCharacter = true;
					UnequipItemFromPlayer(byDBID.equipment.part);
				}
				Remove(tempItem);
				m_playerData.m_userData.inventoryUsedSize--;
				if (m_playerData.m_userData.inventoryUsedSize <= 0)
				{
					m_playerData.m_userData.inventoryUsedSize = 0;
				}
			}
		}
		ItemDetails itemDetails = AddItemFromServer(fusionResult.item, false);
		m_playerData.Igc -= m_cost;
		if (m_tempItem.IsEquip)
		{
			regenerateCharacter = true;
			m_playerData.Equip(itemDetails);
		}
		SingletonComponent<PlayerDataManager>.Instance.SetPlayerData(fusionResult.characterDetails);
		if (m_fuseEvolveCallback != null)
		{
			m_fuseEvolveCallback(itemDetails, _handle, regenerateCharacter);
		}
		m_cost = 0;
	}

	private void OnFuseFailed(Services.ServiceHandle _handle, string _error)
	{
		if (_handle != null && _handle.result == Services.ConnectionResults.Timeout)
		{
			UIPopupController.Instance.HideLoadingPopup();
			UIPopupController.Instance.ShowTimeoutForceRetry(RetryFusion);
			return;
		}
		if (_handle != null && _handle.IsNoInternet())
		{
			UIPopupController.Instance.HideLoadingPopup();
			UIPopupController.Instance.ShowNoNetForceRetry(RetryFusion);
			return;
		}
		if (_handle != null && _handle.returnJson != null)
		{
			JSONObject field = _handle.returnJson.GetField("payload");
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
		if (m_fuseEvolveCallback != null)
		{
			m_fuseEvolveCallback(null, _handle, false);
		}
	}

	private void OnEvolveSuccess(Services.ServiceHandle _handle)
	{
		ServerInventoryItem serverInventoryItem = (ServerInventoryItem)_handle.returnValue;
		bool regenerateCharacter = false;
		if (m_tempItem.IsEquip)
		{
			regenerateCharacter = true;
			UnequipItemFromPlayer(m_tempItem.equipment.part);
		}
		Remove(m_tempItem.DB_ID);
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string tempItem in m_tempItemList)
		{
			ItemDetails byDBID = GetByDBID(tempItem);
			if (byDBID != null)
			{
				if (byDBID.IsEquipment && byDBID.IsEquip)
				{
					regenerateCharacter = true;
					UnequipItemFromPlayer(byDBID.equipment.part);
				}
				stringBuilder.AppendLine(tempItem + "  " + byDBID.ToString());
				Remove(tempItem);
				m_playerData.m_userData.inventoryUsedSize--;
				if (m_playerData.m_userData.inventoryUsedSize <= 0)
				{
					m_playerData.m_userData.inventoryUsedSize = 0;
				}
			}
		}
		ItemDetails itemDetails = AddItemFromServer(serverInventoryItem);
		m_playerData.Igc -= m_cost;
		if (MainMenu.Instance != null)
		{
			MainMenu.Instance.Refresh();
		}
		else
		{
			Dbg.LogWarn("The main menu instance is null, cannot refresh the UI");
		}
		Dbg.Log(string.Concat(" ****** Evolution Successful ****** base item : \n", m_tempItem, " with list : ", stringBuilder.ToString(), "\n obtain -> \n", itemDetails), Dbg.NBLogType.INVENTORY);
		stringBuilder = null;
		if (m_fuseEvolveCallback != null)
		{
			m_fuseEvolveCallback(itemDetails, _handle, regenerateCharacter);
		}
		m_cost = 0;
	}

	private void OnEvolveFailed(Services.ServiceHandle _handle, string _error)
	{
		if (m_fuseEvolveCallback != null)
		{
			m_fuseEvolveCallback(null, _handle, false);
		}
		Dbg.LogError(" ****** Evolution Failed ****** ", Dbg.NBLogType.INVENTORY);
	}

	private void OnMultiSellSuccess(Services.ServiceHandle _handle)
	{
		MultipleSell multipleSell = (MultipleSell)_handle.returnValue;
		int num = 0;
		int num2 = 0;
		bool regenerateCharacter = false;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(" ** Multi Sell Successful **");
		for (int i = 0; i < multipleSell.returnCode.Count; i++)
		{
			if (i >= m_tempItemList.Count)
			{
				Dbg.LogError("The m_tempItemList doesn't have the same size as the one return from server. Force Refresh Server.", Dbg.NBLogType.INVENTORY);
				break;
			}
			string text = m_tempItemList[i];
			int num3 = multipleSell.returnCode[i];
			if (num3 == MultipleSell.okCode || num3 == MultipleSell.missingCode)
			{
				if (num3 == MultipleSell.okCode)
				{
					num++;
				}
				else
				{
					num2++;
				}
				stringBuilder.AppendLine(text + " with return code : " + num3);
				ItemDetails byDBID = GetByDBID(text);
				if (byDBID != null)
				{
					if (byDBID.IsEquipment && byDBID.IsEquip)
					{
						regenerateCharacter = true;
						UnequipItemFromPlayer(byDBID.equipment.part);
					}
					m_inventory.Remove(text);
					m_playerData.m_userData.inventoryUsedSize--;
					if (m_playerData.m_userData.inventoryUsedSize <= 0)
					{
						m_playerData.m_userData.inventoryUsedSize = 0;
					}
				}
			}
			else
			{
				num2++;
			}
		}
		if (multipleSell.characterDetails != null)
		{
			m_playerDataMgr.SetPlayerData(multipleSell.characterDetails);
			if (multipleSell.characterDetails.currencies != null)
			{
				stringBuilder.AppendLine(multipleSell.characterDetails.currencies.social.ToString());
				stringBuilder.AppendLine(multipleSell.characterDetails.currencies.premium.ToString());
				stringBuilder.AppendLine(multipleSell.characterDetails.currencies.ingame.ToString());
			}
		}
		Dbg.LogWarn(stringBuilder.ToString(), Dbg.NBLogType.INVENTORY);
		stringBuilder = null;
		if (MainMenu.Instance != null)
		{
			MainMenu.Instance.Refresh();
		}
		if (m_sellCallbackNum != null)
		{
			m_sellCallbackNum(true, num, num2, null, regenerateCharacter);
		}
	}

	private void OnMultiSellFailed(Services.ServiceHandle _handle, string _error)
	{
		if (_handle != null && _handle.result == Services.ConnectionResults.Timeout)
		{
			UIPopupController.Instance.HideLoadingPopup();
			UIPopupController.Instance.ShowTimeoutForceRetry(RetrySellItems);
			return;
		}
		if (_handle != null && _handle.IsNoInternet())
		{
			UIPopupController.Instance.HideLoadingPopup();
			UIPopupController.Instance.ShowNoNetForceRetry(RetrySellItems);
			return;
		}
		if (_handle != null && _handle.returnJson != null)
		{
			JSONObject field = _handle.returnJson.GetField("payload");
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
		if (m_sellCallbackNum != null)
		{
			m_sellCallbackNum(false, 0, 0, _handle, false);
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string tempItem in m_tempItemList)
		{
			stringBuilder.AppendLine(tempItem);
		}
		Dbg.LogError(" ****** Sell Failed ****** items : \n" + stringBuilder.ToString() + ":\n" + _error, Dbg.NBLogType.INVENTORY);
		stringBuilder = null;
	}

	private void OnUpdateInventorySuccess(Services.ServiceHandle _handle)
	{
		Dbg.Log(" ---------------- Equipped Item Saved on Server  ---------------- ", Dbg.NBLogType.INVENTORY);
		if (m_saveEquipmentOnServer != null)
		{
			m_saveEquipmentOnServer(true);
			m_saveEquipmentOnServer = null;
			UIPopupController.Instance.HideLoadingPopup();
		}
	}

	private void OnUpdateInventoryFailed(Services.ServiceHandle _handle, string _error)
	{
		Dbg.LogError("Could not update equipment Error : \n" + _error + " - " + ServiceCallUtils.GetErrorMessage(_handle), Dbg.NBLogType.INVENTORY);
		if (m_saveEquipmentOnServer != null)
		{
			SaveEquipOnServer saveEquipmentOnServer = m_saveEquipmentOnServer;
			m_saveEquipmentOnServer = null;
			saveEquipmentOnServer(false);
			UIPopupController.Instance.HideLoadingPopup();
		}
	}

	private List<string> InventoryCellToIDS(List<InventoryCell> _cellList)
	{
		List<string> list = new List<string>();
		foreach (InventoryCell _cell in _cellList)
		{
			list.Add(_cell.data.DB_ID);
		}
		return list;
	}

	private void Remove(string _db_id)
	{
		m_inventory.Remove(_db_id);
	}

	private void PrintInventory()
	{
	}

	private void WarnIfNotLoaded()
	{
		if (!IsLoaded)
		{
			Dbg.LogWarn("The inventory is not Loaded", Dbg.NBLogType.INVENTORY);
		}
	}

	public ItemDetails GetByDBID(string _db_id)
	{
		ItemDetails value = ItemManager.s_NULL_DATA;
		if (!m_inventory.TryGetValue(_db_id, out value))
		{
			Dbg.LogWarn("Could not find item in inventory : " + _db_id + "\tLoaded : " + IsLoaded, Dbg.NBLogType.INVENTORY);
		}
		return value;
	}

	public ItemDetails GetByItemID(string _itemID)
	{
		string text = _itemID.ToLower();
		foreach (ItemDetails value in m_inventory.Values)
		{
			if (value.ItemID.ToLower() == text)
			{
				return value;
			}
		}
		Dbg.LogWarn("Could not find any item in inventory with itemID : " + _itemID + "\t\tLoaded : " + IsLoaded, Dbg.NBLogType.INVENTORY);
		return ItemManager.s_NULL_DATA;
	}

	public List<ItemDetails> GetItemsByItemID(string p_itemID)
	{
		List<ItemDetails> list = new List<ItemDetails>();
		string text = p_itemID.ToLower();
		foreach (ItemDetails value in m_inventory.Values)
		{
			if (value.ItemID.ToLower() == text)
			{
				list.Add(value);
			}
		}
		return list;
	}

	public ItemDetails AddItemFromServer(ServerInventoryItem _serverInventoryItem, bool p_isNew = true)
	{
		if (_serverInventoryItem == null || _serverInventoryItem.isPet)
		{
			Dbg.LogError("Try to add a null or pet item to inventory. Return.");
			return ItemManager.s_NULL_DATA;
		}
		ItemDetails itemDetails = m_itemManager.CreateItemDetails(_serverInventoryItem);
		if (itemDetails == null || itemDetails == ItemManager.s_NULL_DATA)
		{
			return ItemManager.s_NULL_DATA;
		}
		if (m_inventory.ContainsKey(_serverInventoryItem.db_id))
		{
			m_inventory.Remove(_serverInventoryItem.db_id);
		}
		if (p_isNew)
		{
			if (m_newItems == null)
			{
				m_newItems = new List<ItemDetails>();
			}
			m_newItems.Add(itemDetails);
			Messenger.Invoke("town_update_badge");
			itemDetails.isNew = true;
			m_newItemsListDirty = true;
			if (!IsInInventoryItemId(_serverInventoryItem.item_id))
			{
				if (m_newNameItems == null)
				{
					m_newNameItems = new List<ItemDetails>();
				}
				m_newNameItems.Add(itemDetails);
				Dbg.Log("[Info] Item : " + _serverInventoryItem.item_id + " is a new name item, add to list", Dbg.NBLogType.INVENTORY);
			}
			else
			{
				Dbg.Log("[Info] Item : " + _serverInventoryItem.item_id + " is NOT a new name item, add to list", Dbg.NBLogType.INVENTORY);
			}
			if (m_playerData != null && m_playerData.AddArmorToCollection(_serverInventoryItem))
			{
				string item_id = _serverInventoryItem.item_id;
				string collectionId = _serverInventoryItem.collectionId;
				ItemManager instance = SingletonComponent<ItemManager>.Instance;
				if (null != instance)
				{
					if (instance.IsSpecialItem(item_id, _serverInventoryItem.tier))
					{
						if (m_newSpecialCollectionItems != null)
						{
							m_newSpecialCollectionItems.Add(collectionId);
							m_newCollectionItemsDirty = true;
						}
					}
					else if (instance.IsNormalItem(item_id, _serverInventoryItem.tier) && m_newCollectionItems != null)
					{
						m_newCollectionItems.Add(collectionId);
						m_newCollectionItemsDirty = true;
					}
				}
			}
		}
		m_inventory.Add(_serverInventoryItem.db_id, itemDetails);
		if (_serverInventoryItem.isEquip && m_playerData != null)
		{
			m_playerData.Equip(itemDetails);
		}
		return itemDetails;
	}

	public ItemDetails CreateItem(string _itemID, int _tier = -1, int _baseLevel = -1)
	{
		ItemDetails itemDetails = m_itemManager.CreateItemDetails(_itemID, _tier, _baseLevel);
		if (itemDetails == null)
		{
			return ItemManager.s_NULL_DATA;
		}
		m_temporaryItems.Add(itemDetails);
		return itemDetails;
	}

	public ItemDetails GetOrCreate(string _itemID, out bool _created, int _tier = -1)
	{
		ItemDetails itemDetails = GetByItemID(_itemID);
		_created = false;
		if (itemDetails == null || itemDetails == ItemManager.s_NULL_DATA)
		{
			itemDetails = CreateItem(_itemID, _tier);
			Dbg.LogWarn(" ---- > You don't have this item in your inventory. Create it ... <----- " + itemDetails, Dbg.NBLogType.INVENTORY);
			_created = true;
		}
		return itemDetails;
	}

	public void FreeTempItems()
	{
		foreach (ItemDetails temporaryItem in m_temporaryItems)
		{
			temporaryItem.UnloadVisual();
		}
		m_temporaryItems.Clear();
	}

	public ItemDetails GetTempItem(string _itemID)
	{
		foreach (ItemDetails temporaryItem in m_temporaryItems)
		{
			if (_itemID == temporaryItem.ItemID)
			{
				return temporaryItem;
			}
		}
		Dbg.LogError("Could not find Temporary Item : " + _itemID, Dbg.NBLogType.INVENTORY);
		return ItemManager.s_NULL_DATA;
	}

	public void Fuse(string _base_db_id, List<InventoryCell> _fusionList, FuseEvolveCallBack _callbackForUI)
	{
		Fuse(_base_db_id, InventoryCellToIDS(_fusionList), _callbackForUI);
	}

	public void Fuse(string _base_db_id, List<string> _fusionListID, FuseEvolveCallBack _callbackForUI)
	{
		m_tempItem = GetByDBID(_base_db_id);
		m_tempItemList = _fusionListID;
		m_fuseEvolveCallback = _callbackForUI;
		if (m_tempItem == null)
		{
			if (m_fuseEvolveCallback != null)
			{
				m_fuseEvolveCallback(null, null, false);
			}
			return;
		}
		SetCost(_base_db_id, _fusionListID.Count);
		m_nRequestId = (uint)(Random.value * 4.2949673E+09f);
		if (SingletonComponent<TutorialManager>.Instance != null && SingletonComponent<TutorialManager>.Instance.enabled && !SingletonComponent<TutorialManager>.Instance.IsCompleted(Tutorial.State.Fusion))
		{
			Dbg.Log("Tutorial fusion");
			m_services.FusionStart(m_services.NubeeId, m_services.DeviceId, _base_db_id, _fusionListID, m_nRequestId.ToString(), OnFuseSuccess, OnFuseFailed, true);
		}
		else
		{
			m_services.FusionStart(m_services.NubeeId, m_services.DeviceId, _base_db_id, _fusionListID, m_nRequestId.ToString(), OnFuseSuccess, OnFuseFailed);
		}
	}

	private void RetryFusion()
	{
		if (SingletonComponent<TutorialManager>.Instance != null && SingletonComponent<TutorialManager>.Instance.enabled && !SingletonComponent<TutorialManager>.Instance.IsCompleted(Tutorial.State.Fusion))
		{
			m_services.FusionStart(m_services.NubeeId, m_services.DeviceId, m_tempItem.DB_ID, m_tempItemList, m_nRequestId.ToString(), OnFuseSuccess, OnFuseFailed, true);
		}
		else
		{
			m_services.FusionStart(m_services.NubeeId, m_services.DeviceId, m_tempItem.DB_ID, m_tempItemList, m_nRequestId.ToString(), OnFuseSuccess, OnFuseFailed);
		}
		UIPopupController.Instance.ShowLoadingPopup(string.Empty, string.Empty);
	}

	public void Evolve(string _base_db_id, List<InventoryCell> _evolveList, FuseEvolveCallBack _callbackForUI)
	{
		Evolve(_base_db_id, InventoryCellToIDS(_evolveList), _callbackForUI);
	}

	public void Evolve(string _base_db_id, List<string> _evolveListID, FuseEvolveCallBack _callbackForUI)
	{
		m_tempItem = GetByDBID(_base_db_id);
		m_tempItemList = _evolveListID;
		m_fuseEvolveCallback = _callbackForUI;
		if (m_tempItem == null)
		{
			Dbg.LogError("Could not find item to evolve in inventory.", Dbg.NBLogType.INVENTORY);
			if (m_fuseEvolveCallback != null)
			{
				m_fuseEvolveCallback(null, null, false);
			}
		}
		else
		{
			SetCost(_base_db_id, _evolveListID.Count);
			m_services.Evolve(m_services.NubeeId, m_services.DeviceId, _base_db_id, OnEvolveSuccess, OnEvolveFailed);
		}
	}

	private void SetCost(string _base_db_id, int _count)
	{
		ItemDetails byDBID = GetByDBID(_base_db_id);
		m_cost = 0;
		m_cost = byDBID.Cost;
		m_cost *= _count;
	}

	public void SellItems(List<InventoryCell> _itemIds, SellCallback _sellCallback)
	{
		SellItems(InventoryCellToIDS(_itemIds), _sellCallback);
	}

	public void SellItems(List<string> _itemIDS, SellCallback _sellCallback)
	{
		m_tempItemList = _itemIDS;
		m_sellCallbackNum = _sellCallback;
		m_nRequestId = (uint)(Random.value * 4.2949673E+09f);
		m_services.MultiSellItem(m_services.NubeeId, m_services.DeviceId, _itemIDS, m_nRequestId.ToString(), OnMultiSellSuccess, OnMultiSellFailed);
	}

	private void RetrySellItems()
	{
		m_services.MultiSellItem(m_services.NubeeId, m_services.DeviceId, m_tempItemList, m_nRequestId.ToString(), OnMultiSellSuccess, OnMultiSellFailed);
		UIPopupController.Instance.ShowLoadingPopup(string.Empty, string.Empty);
	}

	public void UnequipItemFromPlayer(Equipment.Part p_part)
	{
		if (m_playerData != null)
		{
			if (p_part >= Equipment.Part.Helm && (int)p_part < m_baseItems.Count)
			{
				m_playerData.Equip(m_baseItems[(int)p_part]);
			}
		}
		Dbg.Log(string.Concat(" --> Revert to base item for part : ", p_part, " with item\n", m_baseItems[(int)p_part]), Dbg.NBLogType.INVENTORY);
	}

	public void SaveEquipmentToServer(SaveEquipOnServer _cb = null, bool p_isTutorial = false)
	{
		m_saveEquipmentOnServer = _cb;
		if (m_playerData.Equipments.Length != 5)
		{
			Dbg.LogError("The local equipment is wrong. Do not send to server.", Dbg.NBLogType.INVENTORY);
			if (m_saveEquipmentOnServer != null)
			{
				m_saveEquipmentOnServer(false);
				m_saveEquipmentOnServer = null;
			}
			return;
		}
		List<string> list = new List<string>();
		Equipment.Data[] equipments = m_playerData.Equipments;
		foreach (Equipment.Data data in equipments)
		{
			if (data.itemDetails == null)
			{
				Dbg.LogError("This equipItem is null. Do not update the server.", Dbg.NBLogType.INVENTORY);
				if (m_saveEquipmentOnServer != null)
				{
					m_saveEquipmentOnServer(false);
					m_saveEquipmentOnServer = null;
				}
				return;
			}
			list.Add(data.itemDetails.DB_ID);
		}
		m_services.UpdateInventory(m_services.NubeeId, m_services.DeviceId, list, OnUpdateInventorySuccess, OnUpdateInventoryFailed, p_isTutorial);
	}

	public void RefreshFromServer(SuccessfulCallback _refreshInventory = null)
	{
		m_refreshCallback = _refreshInventory;
	}

	public bool CanEvolve(string _db_id)
	{
		ItemDetails byDBID = GetByDBID(_db_id);
		if (byDBID == null)
		{
			return false;
		}
		if (byDBID.IsBaseItem)
		{
			Dbg.LogWarn("Cannot evolve BaseItem : " + _db_id, Dbg.NBLogType.INVENTORY);
			return false;
		}
		return byDBID.equipment.canEvolve;
	}

	public bool CanBeSold(string _db_id)
	{
		ItemDetails byDBID = GetByDBID(_db_id);
		if (byDBID == null)
		{
			return false;
		}
		return !byDBID.IsBaseItem;
	}

	public void RemoveFromNewItems(ItemDetails p_item)
	{
		if (m_newItems == null || p_item == null)
		{
			Dbg.LogWarn("Could not remove new Item.", Dbg.NBLogType.INVENTORY);
			return;
		}
		m_newItems.Remove(p_item);
		Messenger.Invoke("town_update_badge");
		m_newItemsListDirty = true;
	}

	public void RemoveFromNewCollectionItems(ItemDetails p_item)
	{
		if (p_item != null && p_item.serverData != null)
		{
			string collectionId = p_item.serverData.collectionId;
			if (m_newCollectionItems != null && m_newCollectionItems.Contains(collectionId))
			{
				m_newCollectionItems.Remove(collectionId);
			}
			else if (m_newSpecialCollectionItems != null && m_newSpecialCollectionItems.Contains(collectionId))
			{
				m_newSpecialCollectionItems.Remove(collectionId);
			}
			m_newCollectionItemsDirty = true;
		}
	}

	public void ClearNewCollectionItemsFromPlayerPrefs()
	{
		ClearNewCollectionItemsFromPlayerPrefs(false);
		ClearNewCollectionItemsFromPlayerPrefs(true);
	}

	public void ClearNewCollectionItemsFromPlayerPrefs(bool p_special)
	{
		string text = ((!p_special) ? "normal" : "special");
		int num = MBPrefs.GetInt("itemcollections.new." + text + ".count");
		for (int i = 0; i < num; i++)
		{
			string key = "itemcollections.new." + text + "." + i;
			MBPrefs.DeleteKey(key);
		}
		MBPrefs.SetInt("itemcollections.new." + text + ".count", 0);
	}

	public void SaveNewCollectionItemsToPlayerPrefs()
	{
		if (m_newCollectionItemsDirty)
		{
			SaveNewCollectionItemsToPlayerPrefs(m_newCollectionItems);
			SaveNewCollectionItemsToPlayerPrefs(m_newSpecialCollectionItems, true);
			m_newCollectionItemsDirty = false;
		}
	}

	public void SaveNewCollectionItemsToPlayerPrefs(ICollection<string> p_collection, bool p_special = false)
	{
		if (p_collection == null)
		{
			return;
		}
		string text = ((!p_special) ? "normal" : "special");
		int num = MBPrefs.GetInt("itemcollections.new." + text + ".count");
		int count = p_collection.Count;
		int num2 = count - num;
		if (num2 < 0)
		{
			for (int i = count; i < num; i++)
			{
				string key = "itemcollections.new." + text + "." + i;
				MBPrefs.DeleteKey(key);
			}
		}
		int num3 = 0;
		foreach (string item in p_collection)
		{
			MBPrefs.SetString("itemcollections.new." + text + "." + num3, item);
			num3++;
		}
		MBPrefs.SetInt("itemcollections.new." + text + ".count", num3);
	}

	public bool IsNewCollectionItem(string p_collectionId)
	{
		if (m_newCollectionItems != null && m_newCollectionItems.Contains(p_collectionId))
		{
			return true;
		}
		if (m_newSpecialCollectionItems != null && m_newSpecialCollectionItems.Contains(p_collectionId))
		{
			return true;
		}
		return false;
	}

	public int GetNewCollectionItemsTotal()
	{
		int newCollectionItemsCount = GetNewCollectionItemsCount(false);
		int newCollectionItemsCount2 = GetNewCollectionItemsCount(true);
		return newCollectionItemsCount + newCollectionItemsCount2;
	}

	public int GetNewCollectionItemsCount(bool p_special)
	{
		if (p_special && m_newSpecialCollectionItems != null)
		{
			return m_newSpecialCollectionItems.Count;
		}
		if (m_newCollectionItems != null)
		{
			return m_newCollectionItems.Count;
		}
		return 0;
	}

	public void LoadNewCollectionItemsFromPlayerPrefs()
	{
		LoadNewCollectionItemsFromPlayerPrefs(m_newCollectionItems, false);
		LoadNewCollectionItemsFromPlayerPrefs(m_newSpecialCollectionItems, true);
	}

	public void LoadNewCollectionItemsFromPlayerPrefs(ICollection<string> p_collection, bool p_special)
	{
		if (p_collection == null)
		{
			return;
		}
		p_collection.Clear();
		string text = ((!p_special) ? "normal" : "special");
		int num = MBPrefs.GetInt("itemcollections.new." + text + ".count");
		for (int i = 0; i < num; i++)
		{
			string key = "itemcollections.new." + text + "." + i;
			string text2 = MBPrefs.GetString(key, string.Empty);
			if (text2 != string.Empty)
			{
				p_collection.Add(text2);
			}
		}
	}

	public void SaveNewItemsState()
	{
		if (m_newItems == null || !m_newItemsListDirty)
		{
			return;
		}
		int num = MBPrefs.GetInt("InventoryManager.m_newItems.Count");
		int count = m_newItems.Count;
		int num2 = count - num;
		if (num2 < 0)
		{
			for (int i = count; i < num; i++)
			{
				string key = "InventoryManager.m_newItems[" + i + "]";
				MBPrefs.DeleteKey(key);
			}
		}
		int num3 = 0;
		foreach (ItemDetails newItem in m_newItems)
		{
			MBPrefs.SetString("InventoryManager.m_newItems[" + num3 + "]", newItem.DB_ID);
			num3++;
		}
		MBPrefs.SetInt("InventoryManager.m_newItems.Count", num3);
		Dbg.Log(">>>>>>>>> Saving new items state = " + num3 + " new items", Dbg.NBLogType.INVENTORY);
		m_newItemsListDirty = false;
		SaveNewCollectionItemsToPlayerPrefs();
	}

	public void LoadNewItemsState()
	{
		if (m_inventory == null || !IsLoaded)
		{
			return;
		}
		int num = MBPrefs.GetInt("InventoryManager.m_newItems.Count");
		Dbg.Log(">>>>>>>>> Loading new items state = " + num + " new items", Dbg.NBLogType.INVENTORY);
		if (m_newItems != null)
		{
			m_newItems.Clear();
			Messenger.Invoke("town_update_badge");
		}
		else if (num > 0)
		{
			m_newItems = new List<ItemDetails>();
		}
		for (int i = 0; i < num; i++)
		{
			string key = "InventoryManager.m_newItems[" + i + "]";
			string key2 = MBPrefs.GetString(key);
			if (m_inventory.ContainsKey(key2))
			{
				ItemDetails itemDetails = m_inventory[key2];
				itemDetails.isNew = true;
				m_newItems.Add(itemDetails);
				Messenger.Invoke("town_update_badge");
			}
		}
	}

	public List<ItemDetails> GetNewItems()
	{
		return m_newItems;
	}

	public int GetNumOfNewItems()
	{
		if (m_newItems == null)
		{
			Dbg.Log("GetNumOfNewItems, count : 0, null");
			return 0;
		}
		Dbg.Log("GetNumOfNewItems, count : " + m_newItems.Count);
		return m_newItems.Count;
	}

	public bool IsNewItem(string p_itemId)
	{
		foreach (ItemDetails newItem in m_newItems)
		{
			if (newItem.ItemID == p_itemId)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsNewNameItem(string p_itemId)
	{
		if (m_newNameItems != null)
		{
			foreach (ItemDetails newNameItem in m_newNameItems)
			{
				if (newNameItem.ItemID == p_itemId)
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool IsInInventoryDbId(string p_dbId)
	{
		return ItemManager.s_NULL_DATA != GetByDBID(p_dbId);
	}

	public bool IsInInventoryItemId(string p_itemId)
	{
		return ItemManager.s_NULL_DATA != GetByItemID(p_itemId);
	}

	public void ClearNewNameItems()
	{
		if (m_newNameItems != null)
		{
			m_newNameItems.Clear();
		}
	}

	public void LoadVisual(ItemVisualManager.CompletedCB _visualLoaded = null)
	{
		m_onVisualLoaded = _visualLoaded;
		SingletonComponent<ItemVisualManager>.Instance.LoadAllAtlas(SetVisual);
	}

	public void LoadEquipVisual(ItemVisualManager.CompletedCB _visualLoaded = null)
	{
		m_onVisualLoaded = _visualLoaded;
		List<string> list = new List<string>();
		Equipment.Data[] equipments = m_playerData.Equipments;
		foreach (Equipment.Data data in equipments)
		{
			if (data.itemDetails == null)
			{
				Dbg.LogError("No item for this part !!! : " + data);
			}
			else
			{
				list.Add(data.itemDetails.BGAtlasName);
			}
		}
		SingletonComponent<ItemVisualManager>.Instance.LoadAtlasList(list, SetVisual);
		list.Clear();
		list = null;
	}

	private void SetEquipmentVisual(bool _loaded = true)
	{
		Equipment.Data[] equipments = m_playerData.Equipments;
		foreach (Equipment.Data data in equipments)
		{
			ItemDetails itemDetails = data.itemDetails;
			if (data.itemDetails == null)
			{
				Dbg.LogError("No item for this part !!! : " + data);
			}
			else
			{
				m_itemManager.SetItemVisual(itemDetails, itemDetails.BGSpriteName, itemDetails.Element);
			}
		}
		if (m_onVisualLoaded != null)
		{
			m_onVisualLoaded(_loaded);
		}
	}

	private void SetVisual(bool _loaded = true)
	{
		foreach (ItemDetails value in m_inventory.Values)
		{
			m_itemManager.SetItemVisual(value, value.OriginalBGSprite, value.Element);
		}
		if (m_onVisualLoaded != null)
		{
			m_onVisualLoaded(_loaded);
		}
	}

	public void UnloadAllVisual()
	{
		SingletonComponent<ItemVisualManager>.Instance.UnloadAllAtlas();
		foreach (ItemDetails value in m_inventory.Values)
		{
			value.UnloadVisual();
		}
		FreeTempItems();
	}

	public void Unload()
	{
		m_inventory.Clear();
		FreeTempItems();
		m_isLoaded = false;
	}
}
