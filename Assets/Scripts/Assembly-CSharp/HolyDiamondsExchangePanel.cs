using System;
using System.Collections.Generic;
using Models;
using ServiceCalls;
using UnityEngine;

public class HolyDiamondsExchangePanel : NBUIMenuPanel
{
	public DragonPointsExchangeGrid m_grid;

	public UILabel m_currentPointsLabel;

	public UIButton m_closeButton;

	public UIButton m_openTreasureButton;

	private MainMenu m_main;

	private DragonPointsExchangeCell m_selectedCell;

	private uint m_requestId;

	private void OnEnable()
	{
		Init();
	}

	private void OnDisable()
	{
		Reset();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void Init()
	{
		if (null != m_main)
		{
			m_main.ShowInventoryOverlay(true);
		}
		UpdateCurrentHolyDiamonds();
		UIPopupController.Instance.ShowLoadingPopupNetWait();
		Services instance = SingletonComponent<Services>.Instance;
		instance.GetHolyShop(instance.NubeeId, instance.DeviceId, OnGetDataSuccess, OnGetDataFail);
	}

	public override void PreHidePanel()
	{
	}

	private void UpdateCurrentHolyDiamonds()
	{
		PlayerDataManager instance = SingletonComponent<PlayerDataManager>.Instance;
		if (null != instance && null != m_currentPointsLabel)
		{
			PlayerData playerData = instance.GetPlayerData();
			if (playerData != null && playerData.details != null && playerData.details.currencies != null)
			{
				int holyDiamonds = playerData.details.currencies.holyDiamonds;
				m_currentPointsLabel.text = holyDiamonds + string.Empty;
				Messenger<int>.Invoke("exchange_update_points", holyDiamonds);
			}
		}
	}

	private void OnGetDataSuccess(Services.ServiceHandle p_handle)
	{
		UIPopupController.Instance.HideLoadingPopup();
		if (p_handle == null)
		{
			return;
		}
		List<ShopData> p_shopData = p_handle.returnValue as List<ShopData>;
		AddLuckyGacha(ref p_shopData);
		if (!(null != m_grid) || p_shopData == null)
		{
			return;
		}
		m_grid.Clear();
		int count = p_shopData.Count;
		for (int i = 0; i < count; i++)
		{
			ShopData shopData = p_shopData[i];
			if (shopData.IsValid)
			{
				m_grid.AddData(shopData);
				DragonPointsExchangeCell dragonPointsExchangeCell = m_grid.AddCell(0f, 0f);
				if (null != dragonPointsExchangeCell)
				{
					dragonPointsExchangeCell.SetExchangeButtonMessageTarget(base.gameObject);
				}
			}
		}
		if (count > 0)
		{
			UpdateCurrentHolyDiamonds();
		}
	}

	private void AddLuckyGacha(ref List<ShopData> p_shopData)
	{
		if (p_shopData != null)
		{
			GachaManager instance = SingletonComponent<GachaManager>.Instance;
			if (!(null == instance))
			{
				GachaEventsInfo eventById = instance.GetEventById(11);
				int p_outResult = -1;
				NBGeneric.TryParse<int>(eventById.id, out p_outResult);
				string p_title = SingletonComponent<NBLocalization>.Instance.Get("lucky_gacha_title");
				string p_description = SingletonComponent<NBLocalization>.Instance.Get("lucky_gacha_description");
				ShopDataGacha item = new ShopDataGacha(eventById.id, p_title, p_description, (int)eventById.price, CurrencyEnum.HD, -1, p_outResult);
				p_shopData.Add(item);
			}
		}
	}

	private void OnGetDataFail(Services.ServiceHandle p_handle, string p_errorMessage)
	{
		UIPopupController.Instance.HideLoadingPopup();
		if (p_handle == null)
		{
			UIPopupController.Instance.ShowServerGenericError(OnFailPopupClosed);
		}
		else if (p_handle.IsNoInternet())
		{
			UIPopupController.Instance.ShowNoNetMessage(OnFailPopupClosed);
		}
		else
		{
			int p_error = ((p_handle != null) ? p_handle.nReturnCode : 0);
			UIPopupController.Instance.ShowServerError("holydiamonds_getdata_fail", OnFailPopupClosed, p_error);
		}
		base.show = false;
	}

	private new void Reset()
	{
		if (null != m_main)
		{
			m_main.ShowInventoryOverlay(false);
		}
	}

	private new void Awake()
	{
		m_main = MainMenu.Instance;
	}

	private void OnClose()
	{
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_cancel");
		base.show = false;
	}

	private void OnButtonClick(GameObject p_sender)
	{
		if (null != m_closeButton && m_closeButton.gameObject == p_sender)
		{
			OnClose();
			return;
		}
		if (null != m_openTreasureButton && m_openTreasureButton.gameObject == p_sender)
		{
			base.show = false;
			if (null != m_main)
			{
				SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_ok");
				m_main.ShowGachaPage(true);
			}
			return;
		}
		m_selectedCell = p_sender.GetComponent<DragonPointsExchangeCell>();
		if (null != m_selectedCell)
		{
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_ok");
			string txt = string.Format(SingletonComponent<NBLocalization>.Instance.Get("shop_exchange"), m_selectedCell.data.Title, m_selectedCell.data.Price, "[HD]");
			UIPopupController.Instance.ShowMediumPopupMSG("holydiamonds_exchange_panel_header", txt, OnExchangeYes, null);
			return;
		}
		InventoryCell component = p_sender.GetComponent<InventoryCell>();
		if (null != component)
		{
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_secondary_ui_tap");
			UIPopupController.Instance.ShowItemStats(component.data);
		}
	}

	private void OnExchangeYes()
	{
		Services instance = SingletonComponent<Services>.Instance;
		if (null != instance && null != m_selectedCell)
		{
			UIPopupController.Instance.ShowLoadingPopupNetWait();
			if (!PlayGacha())
			{
				m_requestId = (uint)(UnityEngine.Random.value * 4.2949673E+09f);
				instance.ExchangeHolyDiamonds(instance.NubeeId, instance.DeviceId, m_selectedCell.data.ID, m_requestId.ToString(), OnExchangeSuccess, OnExchangeFail);
			}
		}
	}

	private bool PlayGacha()
	{
		if (null == m_selectedCell || m_selectedCell.data == null)
		{
			return false;
		}
		if (m_selectedCell.data.Reward != RewardsType.GACHA)
		{
			return false;
		}
		ShopDataGacha shopDataGacha = m_selectedCell.data as ShopDataGacha;
		if (shopDataGacha.GachaID != -1)
		{
			GachaManager instance = SingletonComponent<GachaManager>.Instance;
			if (null == instance)
			{
				return false;
			}
			instance.PlayLuckyGacha(ReceiveGachaItem, new GachaManager.GachaCallback(OnExchangeFail), false, false);
			return true;
		}
		return false;
	}

	private void ReceiveGachaItem(Services.ServiceHandle p_handle)
	{
		UIPopupController.Instance.HideLoadingPopup();
		if (p_handle == null || null == m_selectedCell)
		{
			return;
		}
		ItemDetails lastItemResult = SingletonComponent<GachaManager>.Instance.GetLastItemResult();
		if (lastItemResult != null)
		{
			UIPopupController.Instance.ShowPromoClaimPopup(lastItemResult);
			UpdateCurrentHolyDiamonds();
			MainMenu instance = MainMenu.Instance;
			if (null != instance)
			{
				instance.CheckExcessItem(OnLockedInTown);
			}
		}
	}

	private void OnExchangeSuccess(Services.ServiceHandle p_handle)
	{
		UIPopupController.Instance.HideLoadingPopup();
		if (p_handle == null)
		{
			return;
		}
		if (null != SingletonComponent<PlayerDataManager>.Instance)
		{
			CommonRewards commonRewards = p_handle.returnValue as CommonRewards;
			if (commonRewards != null)
			{
				if (commonRewards.pet != null)
				{
					Singleton<PetSetup>.Instance.GetExtraInfo(ref commonRewards.pet);
					AddPet(commonRewards.pet);
					UIPopupController.Instance.ShowPromoClaimPopup(null, commonRewards, m_selectedCell.GetCustomSprite());
				}
				else
				{
					AddItem(commonRewards.item);
				}
			}
			SingletonComponent<PlayerDataManager>.Instance.SetCurrency(commonRewards.currencies);
		}
		UpdateCurrentHolyDiamonds();
		MainMenu instance = MainMenu.Instance;
		if (null != instance)
		{
			instance.CheckExcessItem(OnLockedInTown);
		}
	}

	private void AddPet(PetDetails p_pet)
	{
		if (null == SingletonComponent<PlayerDataManager>.Instance)
		{
			return;
		}
		PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		if (playerData.details != null && !playerData.details.unlockedPets.ContainsKey(p_pet.m_petId))
		{
			playerData.details.unlockedPets.Add(p_pet.m_petId, p_pet);
			if (playerData.details.activePet == string.Empty)
			{
				playerData.details.activePet = p_pet.m_petId;
			}
		}
	}

	private void AddItem(ServerInventoryItem p_item)
	{
		InventoryManager instance = SingletonComponent<InventoryManager>.Instance;
		if (null != instance)
		{
			ItemDetails p_data = instance.AddItemFromServer(p_item);
			UIPopupController.Instance.ShowPromoClaimPopup(p_data);
		}
		if (null != SingletonComponent<PlayerDataManager>.Instance)
		{
			PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
			playerData.m_userData.inventoryUsedSize++;
		}
		MainMenu.Instance.inventory.SetDirty();
	}

	private void OnLockedInTown()
	{
		if (null != this && null != base.gameObject)
		{
			base.show = false;
		}
	}

	private void RetryGacha()
	{
		GachaManager instance = SingletonComponent<GachaManager>.Instance;
		instance.PlayLuckyGacha(ReceiveGachaItem, new GachaManager.GachaCallback(OnExchangeFail), false, true);
	}

	private void RetryExchange()
	{
		Services instance = SingletonComponent<Services>.Instance;
		instance.ExchangeHolyDiamonds(instance.NubeeId, instance.DeviceId, m_selectedCell.data.ID, m_requestId.ToString(), OnExchangeSuccess, OnExchangeFail);
	}

	private void OnExchangeFail(Services.ServiceHandle p_handle)
	{
		OnExchangeFail(p_handle, "");
	}

	private void OnExchangeFail(Services.ServiceHandle p_handle, string p_errorMessage = "")
	{
		UIPopupController.Instance.HideLoadingPopup();
		if (p_handle == null)
		{
			UIPopupController.Instance.ShowServerGenericError(OnFailPopupClosed);
			return;
		}
		if (p_handle.result == Services.ConnectionResults.Timeout)
		{
			if (p_handle is ExchangeHolyDiamonds)
			{
				UIPopupController.Instance.ShowTimeoutForceRetry(RetryExchange);
			}
			else
			{
				UIPopupController.Instance.ShowTimeoutForceRetry(RetryGacha);
			}
			return;
		}
		if (p_handle.IsNoInternet())
		{
			if (p_handle is ExchangeHolyDiamonds)
			{
				UIPopupController.Instance.ShowNoNetForceRetry(RetryExchange);
			}
			else
			{
				UIPopupController.Instance.ShowNoNetForceRetry(RetryGacha);
			}
			return;
		}
		int num = ((p_handle != null) ? p_handle.nReturnCode : 0);
		if (num == 700)
		{
			int p_amount = 0;
			int p_currentAmount = 0;
			if (null != m_selectedCell && m_selectedCell.data != null)
			{
				p_amount = m_selectedCell.data.Price;
			}
			PlayerDataManager instance = SingletonComponent<PlayerDataManager>.Instance;
			if (null != instance)
			{
				PlayerData playerData = instance.GetPlayerData();
				if (playerData != null && playerData.details != null && playerData.details.currencies != null)
				{
					p_currentAmount = playerData.details.currencies.holyDiamonds;
				}
			}
			UIPopupController.Instance.ShowHolyDiamondsPopup(HolyDiamondsPopup.Type.WARNING, p_amount, p_currentAmount, OnTreasure);
		}
		else
		{
			UIPopupController.Instance.ShowServerError("holydiamonds_exchange_fail", OnFailPopupClosed, num);
		}
	}

	private void OnTreasure()
	{
		base.show = false;
	}

	private void OnFailPopupClosed()
	{
	}
}
