using System;
using System.Collections.Generic;
using Models;
using UnityEngine;

public class DragonPointsExchangePanel : NBUIMenuPanel
{
	public DragonPointsExchangeGrid m_grid;

	public UIButton m_closeButton;

	public UIButton m_huntButton;

	public UILabel m_currentPointsLabel;

	private DragonPointsExchangeCell m_selectedCell;

	[SerializeField]
	private NBUILocalize m_expiryDateLabel;

	private MainMenu m_main;

	private long m_expiryDate;

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
		UpdateCurrentDragonPoints();
		UIPopupController.Instance.ShowLoadingPopupNetWait();
		Services instance = SingletonComponent<Services>.Instance;
		instance.GetDragonShop(instance.NubeeId, instance.DeviceId, OnGetDataSuccess, OnGetDataFail);
	}

	public override void PreHidePanel()
	{
	}

	private void UpdateCurrentDragonPoints()
	{
		PlayerDataManager instance = SingletonComponent<PlayerDataManager>.Instance;
		if (null != instance && null != m_currentPointsLabel)
		{
			PlayerData playerData = instance.GetPlayerData();
			if (playerData != null && playerData.details != null && playerData.details.currencies != null)
			{
				int dragonPoints = playerData.details.currencies.dragonPoints;
				m_currentPointsLabel.text = dragonPoints + string.Empty;
				Messenger<int>.Invoke("exchange_update_points", dragonPoints);
			}
		}
	}

	private void SetTime()
	{
		if (m_expiryDate == 0L || !(null != m_expiryDateLabel))
		{
			return;
		}
		string text = SingletonComponent<NBLocalization>.Instance.Get("dragon_point_expiry_date");
		long num = m_expiryDate - TimeUtil.GetUnixEpochTime();
		if (num < 0)
		{
			return;
		}
		TimeSpan timeSpan = new TimeSpan(0, 0, (int)num);
		if (timeSpan.Days < 0 || timeSpan.Hours < 0 || timeSpan.Minutes < 0)
		{
			return;
		}
		string text2 = SingletonComponent<NBLocalization>.Instance.Get("leaderboard_timer_day");
		string text3 = SingletonComponent<NBLocalization>.Instance.Get("leaderboard_timer_hour");
		string text4 = SingletonComponent<NBLocalization>.Instance.Get("leaderboard_timer_minute");
		m_expiryDateLabel.Key = string.Format(text + "{0}{1} {2}{3} {4}{5}", timeSpan.Days, text2, timeSpan.Hours, text3, timeSpan.Minutes, text4);
		if (timeSpan.Days <= 0 && timeSpan.Hours <= 0 && timeSpan.Minutes <= 0 && !(null == SingletonComponent<PlayerDataManager>.Instance))
		{
			PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
			if (playerData.details != null)
			{
				playerData.details.currencies.dragonPoints = 0;
				Dbg.Log("Dragon points expired!");
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
		DragonShop dragonShop = p_handle.returnValue as DragonShop;
		if (dragonShop == null)
		{
			return;
		}
		m_expiryDate = dragonShop.ExpiryDate;
		SetTime();
		List<ShopData> shopData = dragonShop.ShopData;
		if (!(null != m_grid) || shopData == null)
		{
			return;
		}
		m_grid.Clear();
		int count = shopData.Count;
		for (int i = 0; i < count; i++)
		{
			ShopData shopData2 = shopData[i];
			if (shopData2.IsValid)
			{
				m_grid.AddData(shopData2);
				DragonPointsExchangeCell dragonPointsExchangeCell = m_grid.AddCell(0f, 0f);
				if (null != dragonPointsExchangeCell)
				{
					dragonPointsExchangeCell.SetExchangeButtonMessageTarget(base.gameObject);
				}
			}
		}
		if (count > 0)
		{
			UpdateCurrentDragonPoints();
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
			UIPopupController.Instance.ShowServerError("dragonpoints_getdata_fail", OnFailPopupClosed, p_error);
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
		if (null != m_huntButton && m_huntButton.gameObject == p_sender)
		{
			base.show = false;
			MainMenu instance = MainMenu.Instance;
			if (null != instance && null != instance.currScreen && !(instance.currScreen.currentPanel is FriendSelectPanel))
			{
				SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_ok");
				instance.ShowEventPage();
			}
			return;
		}
		m_selectedCell = p_sender.GetComponent<DragonPointsExchangeCell>();
		if (null != m_selectedCell)
		{
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_ok");
			if (null != UIPopupController.Instance)
			{
				string txt = string.Format(SingletonComponent<NBLocalization>.Instance.Get("shop_exchange"), m_selectedCell.data.Title, m_selectedCell.data.Price, "[DP]");
				UIPopupController.Instance.ShowMediumPopupMSG("dragonpoints_exchange_panel_header", txt, OnExchangeYes, null);
			}
			return;
		}
		InventoryCell component = p_sender.GetComponent<InventoryCell>();
		if (null != component)
		{
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_secondary_ui_tap");
			ItemEnum.Skill type = component.data.skill.type;
			bool showSkillDescriptions = type != ItemEnum.Skill.ARMOUR_GHOUL && type != ItemEnum.Skill.WEAPON_GHOUL;
			UIPopupController.Instance.ShowItemStats(component.data, null, true, showSkillDescriptions);
		}
	}

	private void OnExchangeYes()
	{
		Services instance = SingletonComponent<Services>.Instance;
		if (null != instance && null != m_selectedCell)
		{
			UIPopupController.Instance.ShowLoadingPopupNetWait();
			m_requestId = (uint)(UnityEngine.Random.value * 4.2949673E+09f);
			instance.ExchangeDragonPoints(instance.NubeeId, instance.DeviceId, m_selectedCell.data.ID, m_requestId.ToString(), OnExchangeSuccess, OnExchangeFail);
		}
	}

	private void OnExchangeSuccess(Services.ServiceHandle p_handle)
	{
		SetTime();
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
		UpdateCurrentDragonPoints();
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
		base.show = false;
	}

	private void OnExchangeFail(Services.ServiceHandle p_handle, string p_errorMessage)
	{
		SetTime();
		UIPopupController.Instance.HideLoadingPopup();
		if (p_handle == null)
		{
			UIPopupController.Instance.ShowServerGenericError(OnFailPopupClosed);
			return;
		}
		if (p_handle.result == Services.ConnectionResults.Timeout)
		{
			UIPopupController.Instance.ShowTimeoutForceRetry(RetryExchange);
			return;
		}
		if (p_handle.IsNoInternet())
		{
			UIPopupController.Instance.ShowNoNetRetry(RetryExchange);
			return;
		}
		int num = ((p_handle != null) ? p_handle.nReturnCode : 0);
		if (num == 700)
		{
			UIPopupController.Instance.ShowServerError("dragonpoints_exchange_not_enough", OnFailPopupClosed, num);
		}
		else
		{
			UIPopupController.Instance.ShowServerError("dragonpoints_exchange_fail", OnFailPopupClosed, num);
		}
	}

	private void RetryExchange()
	{
		Services instance = SingletonComponent<Services>.Instance;
		instance.ExchangeDragonPoints(instance.NubeeId, instance.DeviceId, m_selectedCell.data.ID, m_requestId.ToString(), OnExchangeSuccess, OnExchangeFail);
	}

	private void OnFailPopupClosed()
	{
	}
}
