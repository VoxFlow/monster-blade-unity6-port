using Models;
using UnityEngine;

public class PromoCodePopup : MonoBehaviour
{
	[SerializeField]
	private UIButton m_closeButton;

	[SerializeField]
	private PromoCodeGrid m_grid;

	private UIPopupController m_popupController;

	private void OnEnable()
	{
		Init();
	}

	private void OnDisable()
	{
		Reset();
	}

	private void Init()
	{
		if (null != m_popupController)
		{
			m_popupController.EnableKeyboard(true);
		}
	}

	private void Reset()
	{
		if (null != m_popupController)
		{
			m_popupController.EnableKeyboard(false);
		}
	}

	private void Awake()
	{
		m_popupController = UIPopupController.Instance;
	}

	private void Start()
	{
		DailyManager instance = SingletonComponent<DailyManager>.Instance;
		if (!(null != m_grid) || !(null != instance) || instance.m_promoCodeDatas == null)
		{
			return;
		}
		int count = instance.m_promoCodeDatas.Count;
		for (int i = 0; i < count; i++)
		{
			PromoCodeData promoCodeData = instance.m_promoCodeDatas[i];
			if (promoCodeData != null && promoCodeData.IsPromoEnable())
			{
				m_grid.AddData(promoCodeData);
				PromoCodeCell promoCodeCell = m_grid.AddCell(0f, 0f);
				if (null != promoCodeCell)
				{
					promoCodeCell.m_sendCallback = OnClaimPromoCode;
				}
				promoCodeData.isNew = false;
			}
		}
	}

	private void OnClaimPromoCode(string p_code, string p_id)
	{
		if (p_code == string.Empty)
		{
			if (null != m_popupController)
			{
				m_popupController.SendMessage("OnYes", SendMessageOptions.DontRequireReceiver);
				m_popupController.ShowOKPopupMSG("common_error", "promo_claim_empty_code", OnFailPopupClosed);
			}
			return;
		}
		Services instance = SingletonComponent<Services>.Instance;
		if (null != instance)
		{
			if (null != m_popupController)
			{
				m_popupController.ShowLoadingPopupNetWait();
			}
			instance.SendPromoCode(instance.NubeeId, instance.DeviceId, p_id, p_code, OnClaimPromoCodeSuccess, OnClaimPromoCodeFail);
		}
	}

	private void OnButtonClick(GameObject p_sender)
	{
		if (null != m_closeButton && p_sender == m_closeButton.gameObject)
		{
			CloseWindow();
		}
	}

	private void CloseWindow()
	{
		if (null != m_popupController)
		{
			m_popupController.SendMessage("OnClose", SendMessageOptions.DontRequireReceiver);
		}
	}

	private void OnClaimPromoCodeSuccess(Services.ServiceHandle p_handle)
	{
		CommonRewards commonRewards = (CommonRewards)p_handle.returnValue;
		if (commonRewards == null)
		{
			return;
		}
		ServerInventoryItem item = commonRewards.item;
		InventoryManager instance = SingletonComponent<InventoryManager>.Instance;
		if (!(null != instance))
		{
			return;
		}
		ItemDetails itemDetails = instance.AddItemFromServer(item);
		if (itemDetails != ItemManager.s_NULL_DATA)
		{
			PlayerDataManager instance2 = SingletonComponent<PlayerDataManager>.Instance;
			if (null != instance2)
			{
				instance2.GetPlayerData().m_userData.inventoryUsedSize++;
			}
			MainMenu.Instance.inventory.SetDirty();
			if (null != m_popupController)
			{
				m_popupController.HideLoadingPopup();
				m_popupController.SendMessage("OnYes", SendMessageOptions.DontRequireReceiver);
				m_popupController.ShowPromoClaimPopup(itemDetails, null, null, OnSuccessPopupClosed);
			}
		}
		else if (null != m_popupController)
		{
			m_popupController.HideLoadingPopup();
			m_popupController.SendMessage("OnYes", SendMessageOptions.DontRequireReceiver);
			string txt = SingletonComponent<NBLocalization>.Instance.Get("promo_claim_add_item_failed");
			m_popupController.ShowOKPopupMSG("common_error", txt, OnFailPopupClosed);
		}
	}

	private void OnClaimPromoCodeFail(Services.ServiceHandle p_handle, string p_errorMessage)
	{
		if (null != m_popupController)
		{
			m_popupController.SendMessage("OnYes", SendMessageOptions.DontRequireReceiver);
			if (p_handle == null)
			{
				m_popupController.ShowServerGenericError(OnFailPopupClosed);
			}
			else if (p_handle.IsNoInternet())
			{
				m_popupController.ShowNoNetMessage(OnFailPopupClosed);
			}
			else
			{
				int p_error = ((p_handle != null) ? p_handle.nReturnCode : 0);
				m_popupController.ShowServerError("promo_claim_invalid_code", OnFailPopupClosed, p_error);
			}
			m_popupController.HideLoadingPopup();
		}
	}

	private void OnFailPopupClosed()
	{
		MainMenu instance = MainMenu.Instance;
		if (null != instance)
		{
			instance.ShowPromoCodePopup();
		}
	}

	private void OnSuccessPopupClosed()
	{
		MainMenu instance = MainMenu.Instance;
		if (null != instance)
		{
			instance.CheckExcessItem();
		}
	}
}
