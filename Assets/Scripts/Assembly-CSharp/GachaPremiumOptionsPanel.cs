using System.Collections;
using System.Collections.Generic;
using Models;
using ServiceCalls;
using UnityEngine;

public class GachaPremiumOptionsPanel : GachaOptionPanel
{
	public enum ActionState
	{
		Default = 0,
		TryAgainLastGacha = 1,
		TryAgainShowOptions = 2
	}

	public enum PremiumOptions
	{
		NONE = -1,
		GACHA = 0,
		GACHA_5_1 = 1
	}

	public GameObject premiumCamera1;

	public GameObject eventTarget;

	public ActionState prevState;

	public UILabel m_freeGachaArmour;

	public UILabel m_freeGachaWeapon;

	[SerializeField]
	private UISprite m_freeBanner;

	[SerializeField]
	private GameObject[] m_diamondCosts;

	[SerializeField]
	private UILabel[] m_diamondCostLabel;

	[SerializeField]
	private GameObject m_diamondsSingle;

	[SerializeField]
	private UILabel m_openText;

	[SerializeField]
	private UILabel m_singleBonusLabel;

	[SerializeField]
	private UILabel m_multiBonusLabel;

	[SerializeField]
	private GameObject m_viewCollectionMultiBtn;

	[SerializeField]
	private GameObject m_viewCollectionSingleBtn;

	private List<UIButtonMessage> m_uiButtonMessages = new List<UIButtonMessage>();

	private GameObject m_lastButtonPressed;

	private PremiumOptions m_currentOption = PremiumOptions.NONE;

	private new void OnDisable()
	{
		Reset();
	}

	private void EnableGachaBtns(bool p_enable)
	{
		foreach (UIButtonMessage uiButtonMessage in m_uiButtonMessages)
		{
			uiButtonMessage.enabled = p_enable;
		}
	}

	private void Init()
	{
		UIPopupController.Instance.HideLoadingPopup();
		MainMenu.Instance.ShowBottomBar(false);
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
			else
			{
				GachaScreen.CheckForHolyDiamondsRewards();
			}
		}
		UpdateGachacounter();
		m_currentOption = PremiumOptions.NONE;
	}

	private void UpdateGachacounter()
	{
		PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		int premiumGachaCount = playerData.PremiumGachaCount;
		MainMenu.Instance.UpdateFreeTicketsCounter(m_freeGachaArmour, premiumGachaCount);
		MainMenu.Instance.UpdateFreeTicketsCounter(m_freeGachaWeapon, 0);
		SetSinglePrice(premiumGachaCount > 0);
		SetMultiPrice();
		UpdatePremGachaFreeTex(premiumGachaCount > 0);
	}

	private void SetSinglePrice(bool p_free)
	{
		if (m_diamondCostLabel == null || m_diamondCostLabel.Length <= 1)
		{
			return;
		}
		UILabel uILabel = m_diamondCostLabel[0];
		if (!Dbg.Assert(uILabel != null))
		{
			int singlePremiumPrice = SingletonComponent<GachaManager>.Instance.SinglePremiumPrice;
			bool flag = p_free || singlePremiumPrice == 0;
			uILabel.gameObject.SetActiveRecursively(!flag);
			uILabel.text = singlePremiumPrice.ToString();
			if (null != m_diamondsSingle)
			{
				m_diamondsSingle.SetActiveRecursively(!flag);
			}
			if (null != m_openText)
			{
				m_openText.gameObject.SetActiveRecursively(flag);
				m_openText.text = SingletonComponent<NBLocalization>.Instance.Get("open_free_gacha");
			}
		}
	}

	private void SetMultiPrice()
	{
		if (m_diamondCostLabel != null && m_diamondCostLabel.Length > 1)
		{
			UILabel uILabel = m_diamondCostLabel[1];
			Dbg.Assert(uILabel != null);
			int multiPremiumPrice = SingletonComponent<GachaManager>.Instance.MultiPremiumPrice;
			if (uILabel != null && multiPremiumPrice >= 0)
			{
				uILabel.text = multiPremiumPrice.ToString();
			}
		}
	}

	private void UpdatePremGachaFreeTex(bool p_show)
	{
		if (m_freeBanner != null)
		{
			m_freeBanner.enabled = p_show;
		}
		else
		{
			Dbg.LogWarn("m_freeBanner is null, unable to show or hide it");
		}
	}

	private IEnumerator DoAnimation()
	{
		yield return new MBWaitForSeconds(enterTransitionDuration);   // PORT (CD-059)
		parentScreen.backButton.SetActiveRecursively(true);
		if (parentScreen.backButton.transform.localPosition.y > -100f || parentScreen.backButton.transform.localPosition.y < -102f)
		{
			Vector3 pos = parentScreen.backButton.transform.localPosition;
			pos.y = -101f;
			parentScreen.backButton.transform.localPosition = pos;
			Dbg.LogWarn("The back button position is incorrect, move it to proper position");
		}
		UIPopupController.Instance.EnableUI();
	}

	private void OnButtonClick(GameObject source)
	{
		m_lastButtonPressed = source;
		parentScreen.backButton.SetActiveRecursively(false);
		UIButtonMessage component = source.GetComponent<UIButtonMessage>();
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_premium_currency_purchase");
		switch ((PremiumOptions)component.id)
		{
		case PremiumOptions.GACHA:
		{
			m_currentOption = PremiumOptions.GACHA;
			EnableGachaBtns(false);
			bool p_isTutorial = false;
			if ((bool)SingletonComponent<TutorialManager>.Instance)
			{
				p_isTutorial = SingletonComponent<TutorialManager>.Instance.enabled;
			}
			UIPopupController.Instance.ShowLoadingPopupNetWait();
			SingletonComponent<GachaManager>.Instance.PlayPremiumSingleGacha(OnPlayGachaSuccess, OnPlayGachaFail, p_isTutorial, false);
			break;
		}
		case PremiumOptions.GACHA_5_1:
			m_currentOption = PremiumOptions.GACHA_5_1;
			EnableGachaBtns(false);
			UIPopupController.Instance.ShowLoadingPopupNetWait();
			SingletonComponent<GachaManager>.Instance.PlayPremiumMultiGacha(OnPlayGachaSuccess, OnPlayGachaFail, false, false);
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
		if (SingletonComponent<TutorialManager>.Instance != null && SingletonComponent<TutorialManager>.Instance.IsCompleted(Tutorial.State.Fusion))
		{
			SaveData.MsData gameData = SingletonComponent<SaveData>.Instance.GameData;
			int count = gachaResult.items.Count;
			gameData.OpenPremiumTreasureCount += count;
			gameData.SaveAchievementData();
		}
		UpdateGachacounter();
		float delay = 1.5f;
		Messenger<PremiumOptions>.Invoke("gacha_premiumtype_selected", m_currentOption);
		StartCoroutine(DoRevealAnimation(delay));
	}

	private IEnumerator DoRevealAnimation(float delay)
	{
		yield return new MBWaitForSeconds(delay);   // PORT (CD-059)
		MainMenu.Instance.ShowTopAndBottomBar(false);
		eventTarget.SendMessage("OnPremiumGachaPlaySuccess", null, SendMessageOptions.DontRequireReceiver);
	}

	private void RetryMultiGacha()
	{
		UIPopupController.Instance.ShowLoadingPopupNetWait();
		bool p_isTutorial = m_tutorialManager != null && m_tutorialManager.enabled && !m_tutorialManager.IsCompleted(Tutorial.State.Gacha);
		SingletonComponent<GachaManager>.Instance.PlayPremiumMultiGacha(OnPlayGachaSuccess, OnPlayGachaFail, p_isTutorial, true);
	}

	private void RetrySingleGacha()
	{
		UIPopupController.Instance.ShowLoadingPopupNetWait();
		bool p_isTutorial = m_tutorialManager != null && m_tutorialManager.enabled && !m_tutorialManager.IsCompleted(Tutorial.State.Gacha);
		SingletonComponent<GachaManager>.Instance.PlayPremiumSingleGacha(OnPlayGachaSuccess, OnPlayGachaFail, p_isTutorial, true);
	}

	private void OnPlayGachaFail(Services.ServiceHandle serviceHandle)
	{
		EnableGachaBtns(true);
		UIPopupController.Instance.HideLoadingPopup();
		Dbg.LogWarn("GachaPremiumOptionsPanel::OnPlayGachaFail: " + ServiceCallUtils.GetErrorMessage(serviceHandle));
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
			}
			else
			{
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
		}
		parentScreen.backButton.SetActiveRecursively(true);
		if (parentScreen.backButton.transform.localPosition.y > -100f || parentScreen.backButton.transform.localPosition.y < -102f)
		{
			Vector3 localPosition = parentScreen.backButton.transform.localPosition;
			localPosition.y = -101f;
			parentScreen.backButton.transform.localPosition = localPosition;
			Dbg.LogWarn("The back button position is incorrect, move it to proper position");
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

	protected override void Update()
	{
		if (null != UIPopupController.Instance && UIPopupController.Instance.currentPopup != null)
		{
			m_backButtonMapper.enabled = true;
		}
		else if (m_currentOption == PremiumOptions.NONE)
		{
			base.Update();
		}
		else
		{
			m_backButtonMapper.enabled = false;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		premiumCamera1 = GachaCave.Instance.premiumCamera1;
		GameObject[] array = buttons;
		foreach (GameObject gameObject in array)
		{
			UIButtonMessage component = gameObject.GetComponent<UIButtonMessage>();
			m_uiButtonMessages.Add(component);
		}
		SetBonusLabel(m_singleBonusLabel, GLOBAL.GAME_SETTINGS.gacha_premium_single_hd_bonus_min, GLOBAL.GAME_SETTINGS.gacha_premium_single_hd_bonus_max);
		SetBonusLabel(m_multiBonusLabel, GLOBAL.GAME_SETTINGS.gacha_premium_multi_hd_bonus_min, GLOBAL.GAME_SETTINGS.gacha_premium_multi_hd_bonus_max);
	}

	private void SetBonusLabel(UILabel p_label, int p_min, int p_max)
	{
		if (null != p_label)
		{
			NBLocalization instance = SingletonComponent<NBLocalization>.Instance;
			string text = string.Empty;
			if (null != instance)
			{
				string format = instance.Get("gacha_premium_bonus");
				text = string.Format(format, p_min, p_max);
			}
			p_label.text = text;
		}
	}

	protected override void Start()
	{
	}

	public override void Reset()
	{
		if ((bool)premiumCamera1)
		{
			premiumCamera1.SendMessage("Reset", null, SendMessageOptions.DontRequireReceiver);
		}
		prevState = ActionState.Default;
	}

	public override void OnEnterTransition()
	{
		if (prevState != ActionState.TryAgainLastGacha)
		{
			base.OnEnterTransition();
		}
	}

	public override void OnEnable()
	{
		Init();
		EnableGachaBtns(true);
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
			m_currentOption = PremiumOptions.NONE;
		}
		else
		{
			Dbg.LogError("StoreManager.Instance is null");
		}
	}

	public void OnFullSetViewDetailsClick()
	{
		UIPopupController.Instance.ShowCollectionViewerPopup(GachaManager.GachaID.SetMulti);
	}

	public void OnSingleSetViewDetailsClick()
	{
		UIPopupController.Instance.ShowCollectionViewerPopup(GachaManager.GachaID.PremiumSingle);
	}
}
