using System.Collections;
using System.Collections.Generic;
using Models;
using ServiceCalls;
using UnityEngine;

public class GachaElementOptionsPanel : GachaOptionPanel
{
	public static class ElementColor
	{
		public static Color Fire = new Color(0.9254902f, 0f, 0f);

		public static Color Lightning = new Color(1f, 66f / 85f, 0f);

		public static Color Metal = new Color(19f / 85f, 0.10980392f, 38f / 51f);

		public static Color Nature = new Color(0.003921569f, 0.76862746f, 0f);

		public static Color Water = new Color(0f, 39f / 85f, 0.9254902f);
	}

	public enum ActionState
	{
		Default = 0,
		TryAgainLastGacha = 1,
		TryAgainShowOptions = 2
	}

	public enum ElementOptions
	{
		NONE = -1,
		GACHA = 0,
		GACHA_10_1 = 1
	}

	[HideInInspector]
	public GameObject premiumCamera1;

	[HideInInspector]
	public ActionState prevState;

	private GameObject m_lastButtonPressed;

	public UILabel m_labelFreeCounterSingle;

	public UILabel m_labelFreeCounterMulti;

	public UILabel m_labelDiamondCostSingle;

	public UILabel m_labelDiamondCostMulti;

	public UISprite m_spriteFreeBannerSingle;

	public UISprite m_spriteBackgroundSingle;

	public UISprite m_spriteBackgroundMulti;

	public UISprite m_spriteElementIconSingle;

	public UISprite m_spriteELementIconMulti;

	public UIButton m_buttonPlaySingle;

	public UIButton m_buttonPlayMulti;

	private ElementOptions m_currentOption = ElementOptions.NONE;

	private new void OnDisable()
	{
		Reset();
	}

	private new void OnEnable()
	{
		Init();
	}

	private new void Reset()
	{
		if ((bool)premiumCamera1)
		{
			premiumCamera1.SendMessage("Reset", null, SendMessageOptions.DontRequireReceiver);
		}
		prevState = ActionState.Default;
	}

	private void Init()
	{
		UIPopupController.Instance.HideLoadingPopup();
		if (prevState == ActionState.Default)
		{
			UIPopupController.SetAllInputsEnabled(false);
			StartCoroutine(DoAnimation());
			premiumCamera1.SendMessage("PlayAnimation", null, SendMessageOptions.DontRequireReceiver);
		}
		else
		{
			List<Vector3> nodes = premiumCamera1.GetComponent<iTweenPath>().nodes;
			premiumCamera1.transform.position = nodes[nodes.Count - 1];
			premiumCamera1.transform.rotation = GachaCave.Instance.premiumCamera2.transform.rotation;
			if (prevState == ActionState.TryAgainLastGacha)
			{
				OnButtonClick(m_lastButtonPressed);
			}
		}
		UpdateBackgroundsAndIcons();
		UpdateGachacounter();
		UpdateGachaPrice();
		m_currentOption = ElementOptions.NONE;
	}

	private void UpdateBackgroundsAndIcons()
	{
		GachaManager instance = SingletonComponent<GachaManager>.Instance;
		if ((bool)instance)
		{
			GachaEventsInfo eventById = instance.GetEventById(5);
			string element = eventById.element;
			Color color = Color.white;
			switch (element)
			{
			case "fire":
				color = ElementColor.Fire;
				break;
			case "lightning":
				color = ElementColor.Lightning;
				break;
			case "metal":
				color = ElementColor.Metal;
				break;
			case "nature":
				color = ElementColor.Nature;
				break;
			case "water":
				color = ElementColor.Water;
				break;
			}
			if ((bool)m_spriteBackgroundSingle)
			{
				m_spriteBackgroundSingle.color = color;
			}
			if ((bool)m_spriteBackgroundMulti)
			{
				m_spriteBackgroundMulti.color = color;
			}
			string spriteName = "Icon_" + element;
			if ((bool)m_spriteElementIconSingle)
			{
				m_spriteElementIconSingle.spriteName = spriteName;
			}
			if ((bool)m_spriteELementIconMulti)
			{
				m_spriteELementIconMulti.spriteName = spriteName;
			}
		}
	}

	private void UpdateGachacounter()
	{
		if ((bool)m_labelFreeCounterSingle)
		{
			m_labelFreeCounterSingle.transform.parent.gameObject.SetActiveRecursively(false);
		}
		if ((bool)m_labelFreeCounterMulti)
		{
			m_labelFreeCounterMulti.transform.parent.gameObject.SetActiveRecursively(false);
		}
		if ((bool)m_spriteFreeBannerSingle)
		{
			m_spriteFreeBannerSingle.gameObject.SetActiveRecursively(false);
		}
	}

	public void UpdateDisplayDiamondCost(bool p_show)
	{
	}

	private void UpdateGachaPrice()
	{
		GachaManager instance = SingletonComponent<GachaManager>.Instance;
		if ((bool)instance)
		{
			if ((bool)m_labelDiamondCostSingle)
			{
				m_labelDiamondCostSingle.text = instance.SingleElementPrice + string.Empty;
			}
			if ((bool)m_labelDiamondCostMulti)
			{
				m_labelDiamondCostMulti.text = instance.MultiElementPrice + string.Empty;
			}
		}
	}

	private void UpdatePremGachaFreeTex(bool p_show)
	{
	}

	protected override void Awake()
	{
		base.Awake();
		premiumCamera1 = GachaCave.Instance.premiumCamera1;
	}

	private IEnumerator DoAnimation()
	{
		yield return new MBWaitForSeconds(enterTransitionDuration);   // PORT (CD-059)
		if (parentScreen.backButton.transform.localPosition.y > -100f || parentScreen.backButton.transform.localPosition.y < -102f)
		{
			Vector3 pos = parentScreen.backButton.transform.localPosition;
			pos.y = -101f;
			parentScreen.backButton.transform.localPosition = pos;
		}
		UIPopupController.Instance.EnableUI();
	}

	protected override void Start()
	{
	}

	private void OnButtonClick(GameObject p_source)
	{
		m_lastButtonPressed = p_source;
		parentScreen.backButton.SetActiveRecursively(false);
		UIButtonMessage component = p_source.GetComponent<UIButtonMessage>();
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_premium_currency_purchase");
		switch ((ElementOptions)component.id)
		{
		case ElementOptions.GACHA:
			m_currentOption = ElementOptions.GACHA;
			UIPopupController.Instance.ShowLoadingPopupNetWait();
			SingletonComponent<GachaManager>.Instance.PlayElementSingleGacha(OnPlayGachaSuccess, OnPlayGachaFail, false, false);
			break;
		case ElementOptions.GACHA_10_1:
			m_currentOption = ElementOptions.GACHA_10_1;
			UIPopupController.Instance.ShowLoadingPopupNetWait();
			SingletonComponent<GachaManager>.Instance.PlayElementMultiGacha(OnPlayGachaSuccess, OnPlayGachaFail, false, false);
			break;
		}
	}

	private void OnPlayGachaSuccess(Services.ServiceHandle serviceHandle)
	{
		UIPopupController.Instance.HideLoadingPopup(false);
		UIPopupController.SetAllInputsEnabled(false);
		Models.GachaResult gachaResult = (Models.GachaResult)serviceHandle.returnValue;
		if (gachaResult.characterDetails != null)
		{
			SingletonComponent<PlayerDataManager>.Instance.SetPlayerData(gachaResult.characterDetails);
			MainMenu.Instance.SetCurrencies(gachaResult.characterDetails.currencies);
		}
		MainMenu.Instance.Refresh(100f);
		MainMenu.Instance.inventory.SetDirty();
		MainMenu.Instance.EnableHeader(false, string.Empty);
		float delay = 0.5f;
		if (gachaResult.items.Count > 1)
		{
			delay = 1.2f;
		}
		StartCoroutine(DoRevealAnimation(delay));
	}

	private IEnumerator DoRevealAnimation(float delay)
	{
		yield return new MBWaitForSeconds(delay);   // PORT (CD-059)
		MainMenu.Instance.ShowTopAndBottomBar(false);
		if ((bool)parentScreen)
		{
			parentScreen.SendMessage("OnElementGachaPlaySuccess", null, SendMessageOptions.DontRequireReceiver);
		}
	}

	private void RetryMultiGacha()
	{
		UIPopupController.Instance.ShowLoadingPopupNetWait();
		SingletonComponent<GachaManager>.Instance.PlayElementMultiGacha(OnPlayGachaSuccess, OnPlayGachaFail, false, true);
	}

	private void RetrySingleGacha()
	{
		UIPopupController.Instance.ShowLoadingPopupNetWait();
		SingletonComponent<GachaManager>.Instance.PlayElementSingleGacha(OnPlayGachaSuccess, OnPlayGachaFail, false, true);
	}

	private void OnPlayGachaFail(Services.ServiceHandle serviceHandle)
	{
		UIPopupController.Instance.HideLoadingPopup();
		Dbg.LogWarn("GachaElementOptionsPanel::OnPlayGachaFail: " + ServiceCallUtils.GetErrorMessage(serviceHandle));
		if (serviceHandle != null)
		{
			if (serviceHandle.result == Services.ConnectionResults.Timeout)
			{
				if (serviceHandle is PlayMultiGacha)
				{
					UIPopupController.Instance.ShowTimeoutForceRetry(RetryMultiGacha);
				}
				else
				{
					UIPopupController.Instance.ShowTimeoutForceRetry(RetrySingleGacha);
				}
				return;
			}
			if (serviceHandle.IsNoInternet())
			{
				if (serviceHandle is PlayMultiGacha)
				{
					UIPopupController.Instance.ShowNoNetForceRetry(RetryMultiGacha);
				}
				else
				{
					UIPopupController.Instance.ShowNoNetForceRetry(RetrySingleGacha);
				}
				return;
			}
			ServiceResponseCodes nReturnCode = (ServiceResponseCodes)serviceHandle.nReturnCode;
			if (nReturnCode == ServiceResponseCodes.EXCEPTION_NOT_ENOUGH_CURRENCY)
			{
				GotoPurchaseScreen();
			}
			else
			{
				UIPopupController.Instance.ShowOKPopupMSG("common_prompt_gacha_failed_header", "[Error:" + serviceHandle.nReturnCode + "] " + SingletonComponent<NBLocalization>.Instance.Get("common_prompt_gacha_failed_content"));
			}
		}
		parentScreen.backButton.SetActiveRecursively(true);
		if (parentScreen.backButton.transform.localPosition.y > -100f || parentScreen.backButton.transform.localPosition.y < -102f)
		{
			Vector3 localPosition = parentScreen.backButton.transform.localPosition;
			localPosition.y = -101f;
			parentScreen.backButton.transform.localPosition = localPosition;
		}
		if (prevState == ActionState.TryAgainLastGacha)
		{
			prevState = ActionState.Default;
			OnEnterTransition();
		}
	}

	private void OnRetryPlayLastGacha()
	{
		OnButtonClick(m_lastButtonPressed);
	}

	public void GotoPurchaseScreen()
	{
		if (SingletonComponent<StoreManager>.Instance != null)
		{
			int p_minimumDiamond = 1;
			GachaManager instance = SingletonComponent<GachaManager>.Instance;
			PlayerDataManager instance2 = SingletonComponent<PlayerDataManager>.Instance;
			if (null != instance && null != instance2)
			{
				int previousGachaPrice = instance.PreviousGachaPrice;
				PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
				p_minimumDiamond = previousGachaPrice - playerData.m_userData.premiumCurrency;
			}
			SingletonComponent<StoreManager>.Instance.ShowDiamondsShop(OnRetryPlayLastGacha, null, "common_prompt_gacha_need_diamonds", p_minimumDiamond);
			m_currentOption = ElementOptions.NONE;
		}
		else
		{
			Dbg.LogError("StoreManager.Instance is null");
		}
	}

	protected override void Update()
	{
		if (null != UIPopupController.Instance && UIPopupController.Instance.currentPopup != null)
		{
			m_backButtonMapper.enabled = true;
		}
		else if (m_currentOption == ElementOptions.NONE)
		{
			base.Update();
		}
		else
		{
			m_backButtonMapper.enabled = false;
		}
	}
}
