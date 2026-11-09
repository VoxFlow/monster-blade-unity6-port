using System.Collections;
using Models;
using UnityEngine;

public class DailyBonusPopupNew : MonoBehaviour
{
	[SerializeField]
	private UISprite[] m_rewardSprites;

	[SerializeField]
	private NBUILocalize[] m_rewardAmountTexts;

	[SerializeField]
	private UISprite[] m_stampSprites;

	[SerializeField]
	private UILabel[] m_dayTexts;

	[SerializeField]
	private UISprite[] m_backgroundFrames;

	private bool IsAnimatingStamp;

	private void Start()
	{
	}

	public void SetupDisplay()
	{
		UISprite[] stampSprites = m_stampSprites;
		foreach (UISprite uISprite in stampSprites)
		{
			uISprite.enabled = false;
		}
		int num = 0;
		foreach (DailyBonusData dailyBonuse in SingletonComponent<DailyManager>.Instance.DailyBonuses)
		{
			SingletonComponent<DailyManager>.Instance.DisplayReward(dailyBonuse, ref m_rewardSprites[num], ref m_rewardAmountTexts[num]);
			if (num < SingletonComponent<DailyManager>.Instance.TodayIndex)
			{
				DarkerColor(ref m_rewardSprites[num], 0.5f);
				UILabel p_uiLabel = m_rewardAmountTexts[num].GetComponent<UILabel>();
				if (p_uiLabel != null)
				{
					DarkerColor(ref p_uiLabel, 0.5f);
				}
				DarkerColor(ref m_dayTexts[num], 0.5f);
				DarkerColor(ref m_backgroundFrames[num], 0.5f);
				m_stampSprites[num].enabled = true;
			}
			num++;
			if (num >= m_rewardSprites.Length)
			{
				Dbg.LogWarn("The number of daily bonus data is " + num + " , but we have only " + m_rewardSprites.Length + " of sprites");
				break;
			}
		}
	}

	public void OnClose()
	{
		if (!IsAnimatingStamp)
		{
			OnClaim();
		}
	}

	private void OnClaimSuccess(Services.ServiceHandle p_handle)
	{
		UIPopupController.Instance.HideLoadingPopup();
		if (p_handle == null)
		{
			return;
		}
		if (null != SingletonComponent<PlayerDataManager>.Instance)
		{
			DailyBonusData dailyBonusData = p_handle.returnValue as DailyBonusData;
			if (dailyBonusData == null)
			{
				return;
			}
			Dbg.Log("YOU GOT DAILY BONUS = " + dailyBonusData.Item, Dbg.NBLogType.UI_POPUP);
			DailyBonusData.Type typeId = (DailyBonusData.Type)dailyBonusData.TypeId;
			CurrencyDetails currencyDetails = null;
			CurrencyDetails currencyDetails2 = null;
			int result = 0;
			switch (typeId)
			{
			case DailyBonusData.Type.Item:
			{
				string text = string.Empty;
				int p_tier = 1;
				string p_db_id = string.Empty;
				string p_item_type = string.Empty;
				int p_itemLevel = -1;
				if (dailyBonusData.MyItemData != null)
				{
					text = dailyBonusData.MyItemData.ItemId;
					p_tier = dailyBonusData.MyItemData.ItemTier;
					p_db_id = dailyBonusData.MyItemData.dbId;
					p_item_type = dailyBonusData.MyItemData.ItemType;
					p_itemLevel = dailyBonusData.MyItemData.ItemLevel;
				}
				if (!string.IsNullOrEmpty(text))
				{
					ServerInventoryItem p_item = new ServerInventoryItem(text, p_tier, p_db_id, p_item_type, p_itemLevel, 0, string.Empty);
					AddItem(p_item);
				}
				break;
			}
			case DailyBonusData.Type.IGC:
				currencyDetails = new CurrencyDetails();
				if (int.TryParse(dailyBonusData.Item, out result))
				{
					currencyDetails.ingame = result;
				}
				break;
			case DailyBonusData.Type.SC:
				currencyDetails = new CurrencyDetails();
				if (int.TryParse(dailyBonusData.Item, out result))
				{
					currencyDetails.social = result;
				}
				break;
			case DailyBonusData.Type.PC:
				currencyDetails = new CurrencyDetails();
				if (int.TryParse(dailyBonusData.Item, out result))
				{
					currencyDetails.premium = result;
				}
				break;
			case DailyBonusData.Type.SocialGacha:
				currencyDetails = new CurrencyDetails();
				if (int.TryParse(dailyBonusData.Item, out result))
				{
					currencyDetails.freeSocialGacha = result;
				}
				break;
			case DailyBonusData.Type.PremiumGacha:
				currencyDetails = new CurrencyDetails();
				if (int.TryParse(dailyBonusData.Item, out result))
				{
					currencyDetails.freeGacha = result;
				}
				break;
			}
			if (currencyDetails != null)
			{
				currencyDetails2 = SingletonComponent<PlayerDataManager>.Instance.GetCurrency() + currencyDetails;
				SingletonComponent<PlayerDataManager>.Instance.SetCurrency(currencyDetails2);
			}
		}
		IsAnimatingStamp = true;
		StartCoroutine(AnimStamp(SingletonComponent<DailyManager>.Instance.TodayIndex, ClosePopupAndShowClaimReward));
	}

	private void OnClaimFail(Services.ServiceHandle p_handle, string p_errorMessage)
	{
		UIPopupController.Instance.HideLoadingPopup();
		UIPopupController.Instance.CloseCurrentPopup();
		if (p_handle == null)
		{
			UIPopupController.Instance.ShowServerErrorRetry("dailybonus_claim_fail", OnRetryClaim, OnFailPopupClosed);
		}
		else if (p_handle.result == Services.ConnectionResults.Timeout)
		{
			UIPopupController.Instance.ShowTimeoutForceRetry(OnRetryClaim);
		}
		else if (p_handle.IsNoInternet())
		{
			UIPopupController.Instance.ShowNoNetRetry(OnRetryClaim);
		}
		else
		{
			int p_error = ((p_handle != null) ? p_handle.nReturnCode : 0);
			UIPopupController.Instance.ShowServerErrorRetry("dailybonus_claim_fail", OnRetryClaim, OnFailPopupClosed, p_error);
		}
		UIPopupController.Instance.MakeFirstPriority();
	}

	private void OnRetryClaim()
	{
		if (this == null && null != UIPopupController.Instance)
		{
			UIPopupController.Instance.ShowDailyBonus();
			UIPopupController.Instance.MakeFirstPriority();
		}
	}

	private void OnClaim()
	{
		Services instance = SingletonComponent<Services>.Instance;
		if (null != instance)
		{
			UIPopupController.Instance.ShowLoadingPopupNetWait();
			instance.DailyBonusClaim(instance.NubeeId, instance.DeviceId, OnClaimSuccess, OnClaimFail);
		}
	}

	private void OnLockedInTown()
	{
	}

	private void OnFailPopupClosed()
	{
	}

	private void AddItem(ServerInventoryItem p_item)
	{
		InventoryManager instance = SingletonComponent<InventoryManager>.Instance;
		if (null != instance)
		{
			instance.AddItemFromServer(p_item);
		}
		if (null != SingletonComponent<PlayerDataManager>.Instance)
		{
			PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
			playerData.m_userData.inventoryUsedSize++;
		}
		MainMenu.Instance.inventory.SetDirty();
	}

	public void ClosePopupAndShowClaimReward()
	{
		UIPopupController.Instance.CloseCurrentPopup();
		MainMenu instance = MainMenu.Instance;
		if (null != instance)
		{
			instance.Refresh();
		}
	}

	public IEnumerator AnimStamp(int p_index, PopupCallback p_callback)
	{
		UISprite stamp = m_stampSprites[p_index];
		stamp.enabled = true;
		Vector3 s = stamp.transform.localScale;
		stamp.transform.localScale = 6f * s;
		iTween.ScaleTo(stamp.gameObject, iTween.Hash("scale", 0.5f * s, "time", 0.9f, "easetype", "easeInCubic"));
		yield return new MBWaitForSeconds(0.9f);   // PORT (CD-059)
		stamp.transform.localScale = 1.5f * s;
		iTween.ScaleTo(stamp.gameObject, iTween.Hash("scale", s, "time", 0.5f, "easetype", "easeOutBounce"));
		iTween.ShakePosition(base.gameObject, iTween.Hash("amount", Vector3.one * 10f, "time", 0.5f));
		yield return new MBWaitForSeconds(2f);   // PORT (CD-059)
		if (p_callback != null)
		{
			p_callback();
		}
	}

	private void DarkerColor(ref UISprite p_uiSprite, float p_colorMag)
	{
		p_uiSprite.color = new Color(p_uiSprite.color.r * p_colorMag, p_uiSprite.color.g * p_colorMag, p_uiSprite.color.b * p_colorMag, 1f);
	}

	private void DarkerColor(ref UILabel p_uiLabel, float p_colorMag)
	{
		p_uiLabel.color = new Color(p_uiLabel.color.r * p_colorMag, p_uiLabel.color.g * p_colorMag, p_uiLabel.color.b * p_colorMag, 1f);
	}
}
