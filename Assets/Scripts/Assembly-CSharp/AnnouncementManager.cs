using Models;
using ServiceCalls;
using UnityEngine;

public class AnnouncementManager : SingletonComponent<AnnouncementManager>
{
	public delegate void ClaimRewardCallBackForCell();

	public readonly string[] m_CellIcons = new string[4] { "Icon_Hunt", "Icon_News", "Icon_Social", "Icon_Treasure" };

	public AnnounceAndFriendNotifyData Data = new AnnounceAndFriendNotifyData();

	public ClaimRewardCallBackForCell m_ClaimedCallBack;

	public AnnouncementData CurrentClaimedRewardData;

	private VoidCallback m_receivedAnnouncements;

	public void GetAnnoucementsFromServer(VoidCallback p_receivedAnnouncements = null)
	{
		m_receivedAnnouncements = p_receivedAnnouncements;
		if (SingletonComponent<Services>.Instance != null)
		{
			if (UIPopupController.Instance != null)
			{
				UIPopupController.Instance.ShowLoadingPopup(string.Empty, string.Empty);
			}
			SingletonComponent<Services>.Instance.GetAnnouncements(SingletonComponent<Services>.Instance.NubeeId, SingletonComponent<Services>.Instance.DeviceId, OnGetAnnouncementsSuccess, OnGetAnnouncementsFail);
		}
		else
		{
			Dbg.LogError("Services.Instance is null, the game should shut down");
		}
	}

	private void OnGetAnnouncementsSuccess(Services.ServiceHandle p_handle)
	{
		Dbg.Log("[Info] OnGetAnnouncementsSuccess");
		Data = p_handle.returnValue as AnnounceAndFriendNotifyData;
		UIPopupController.Instance.HideLoadingPopup();
		ClearExpiredData();
		GetNewAnnouncements();
		if (m_receivedAnnouncements != null)
		{
			m_receivedAnnouncements();
		}
	}

	private void OnGetAnnouncementsFail(Services.ServiceHandle p_handle, string p_errMsg)
	{
		UIPopupController.Instance.HideLoadingPopup();
		Dbg.LogWarn("OnGetAnnouncementsFail: " + ServiceCallUtils.GetErrorMessage(p_handle));
		if (p_handle == null)
		{
			UIPopupController.Instance.ShowServerGenericError();
			return;
		}
		if (p_handle.IsNoInternet())
		{
			UIPopupController.Instance.ShowNoNetMessage();
			return;
		}
		ServiceResponseCodes nReturnCode = (ServiceResponseCodes)p_handle.nReturnCode;
		UIPopupController.Instance.ShowServerGenericError(null, p_handle.nReturnCode);
	}

	public void ClaimAnnouncementReward(AnnouncementData p_announcementData)
	{
		if (SingletonComponent<Services>.Instance != null)
		{
			if (UIPopupController.Instance != null)
			{
				UIPopupController.Instance.ShowLoadingPopup(string.Empty, string.Empty);
			}
			foreach (int key in Data.Annoucements[p_announcementData.DbId].AnnouncementRewards.Keys)
			{
				Data.Annoucements[p_announcementData.DbId].AnnouncementRewards[key].Claimed = true;
			}
			CurrentClaimedRewardData = p_announcementData;
			SingletonComponent<Services>.Instance.ClaimAnnouncementReward(SingletonComponent<Services>.Instance.NubeeId, SingletonComponent<Services>.Instance.DeviceId, p_announcementData.DbId, OnClaimAnnouncementRewardSuccess, OnClaimAnnouncementRewardFail);
		}
		else
		{
			Dbg.LogError("Services.Instance is null, the game should shut down");
		}
	}

	private void OnClaimAnnouncementRewardSuccess(Services.ServiceHandle p_handle)
	{
		Dbg.Log("OnClaimAnnouncementRewardSuccess");
		CurrencyDetails currencyDetails = p_handle.returnValue as CurrencyDetails;
		int num = 0;
		int num2 = 0;
		if (currencyDetails != null)
		{
			if (null != SingletonComponent<PlayerDataManager>.Instance)
			{
				num = (int)currencyDetails.ingame - SingletonComponent<PlayerDataManager>.Instance.GetPlayerData().m_userData.inGameCurrency;
				num2 = (int)currencyDetails.premium - SingletonComponent<PlayerDataManager>.Instance.GetPlayerData().m_userData.premiumCurrency;
			}
			MainMenu.Instance.SetIgcValue((int)currencyDetails.ingame);
			MainMenu.Instance.SetPremiumValue((int)currencyDetails.premium);
			MainMenu.Instance.RefreshPlayerStats();
		}
		else
		{
			Dbg.LogWarn("OnClaimAnnouncementRewardSuccess return value is null!");
		}
		if (m_ClaimedCallBack != null)
		{
			m_ClaimedCallBack();
			m_ClaimedCallBack = null;
		}
		if (UIPopupController.Instance != null)
		{
			UIPopupController.Instance.HideLoadingPopup();
			UIPopupController.Instance.CloseCurrentPopup();
			if (num > 0 || num2 > 0)
			{
				string text = SingletonComponent<NBLocalization>.Instance.Get("announcement_reward");
				if (num > 0)
				{
					string text2 = text;
					text = text2 + "\n" + num + " [G] ";
				}
				if (num2 > 0)
				{
					string text2 = text;
					text = text2 + "\n" + num2 + " [D] ";
				}
				UIPopupController.Instance.ShowOKPopupMSG("common_info", text);
			}
		}
		else
		{
			Dbg.LogError("UIPopupController.Instance is null!");
		}
		if (null != MainMenu.Instance.m_AnnouncementPanel)
		{
			MainMenu.Instance.m_AnnouncementPanel.UpdateCells();
		}
	}

	private void OnClaimAnnouncementRewardFail(Services.ServiceHandle p_handle, string p_errMsg)
	{
		UIPopupController.Instance.HideLoadingPopup();
		UIPopupController.Instance.CloseCurrentPopup();
		Dbg.LogWarn("OnClaimAnnouncementRewardFail: " + ServiceCallUtils.GetErrorMessage(p_handle));
		if (p_handle == null)
		{
			UIPopupController.Instance.ShowServerGenericError();
			return;
		}
		if (p_handle.IsNoInternet())
		{
			UIPopupController.Instance.ShowNoNetMessage();
			return;
		}
		ServiceResponseCodes nReturnCode = (ServiceResponseCodes)p_handle.nReturnCode;
		UIPopupController.Instance.ShowServerGenericError(null, p_handle.nReturnCode);
	}

	public void DeleteAnnouncementMessage(int p_dbId)
	{
		if (SingletonComponent<Services>.Instance != null)
		{
			SingletonComponent<Services>.Instance.DeleteAnnouncementMessage(SingletonComponent<Services>.Instance.NubeeId, SingletonComponent<Services>.Instance.DeviceId, p_dbId, OnDeleteAnnouncementSuccess, OnDeleteAnnouncementFail);
		}
		else
		{
			Dbg.LogError("Services.Instance is null, the game should shut down");
		}
	}

	private void OnDeleteAnnouncementSuccess(Services.ServiceHandle p_handle)
	{
		Dbg.Log("OnDeleteAnnouncementSuccess", Dbg.NBLogType.SERVICES);
	}

	private void OnDeleteAnnouncementFail(Services.ServiceHandle p_handle, string p_errMsg)
	{
		Dbg.LogWarn("OnDeleteAnnouncementFail", Dbg.NBLogType.SERVICES);
	}

	public bool HasClaimed(AnnouncementData p_data)
	{
		foreach (int key in p_data.AnnouncementRewards.Keys)
		{
			if (p_data.AnnouncementRewards[key].Claimed)
			{
				return true;
			}
		}
		return false;
	}

	public void ClearExpiredData()
	{
		if (Data == null)
		{
			Dbg.LogError("No Announcement Data.");
			return;
		}
		int num = 0;
		foreach (int key in Data.Annoucements.Keys)
		{
			if (num < key)
			{
				num = key;
			}
		}
		Dbg.Log("Announcement last ID : " + num);
		for (int i = 0; i < num; i++)
		{
			if (!Data.Annoucements.ContainsKey(i))
			{
				string text = MBPrefs.GetString("AnnouncementData.DbId." + i + ".ViewDate", string.Empty);
				if (text != string.Empty)
				{
					MBPrefs.DeleteKey(text);
				}
			}
		}
	}

	public void GetNewAnnouncements()
	{
		if (Data == null || null == SingletonComponent<PlayerDataManager>.Instance || Data.Annoucements == null)
		{
			Dbg.LogError("No PlayerData or Announcement Data.");
			return;
		}
		PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		playerData.m_AnnouncementCount = 0;
		foreach (AnnouncementData value in Data.Annoucements.Values)
		{
			if (!value.IsViewed)
			{
				playerData.m_AnnouncementCount++;
			}
		}
		Dbg.Log(playerData.m_AnnouncementCount + " new Announcements!");
	}

	public void DownloadTexture(string p_url, string p_md5, NBCacheManager.DownloadCallback p_cb)
	{
		if (!p_url.Trim().EndsWith("\\"))
		{
			int num = p_url.LastIndexOf('/');
			if (num > 0)
			{
				string p_szFilename = p_url.Substring(num + 1);
				NBCacheManager.Instance.GetFile(p_url, "announcement", p_szFilename, p_md5, p_cb, null, null);
			}
			else
			{
				Dbg.LogError("filename not found in " + p_url);
			}
		}
		else
		{
			Dbg.LogError("url ends with no filename: " + p_url);
		}
	}
}
