using System;
using System.Collections;
using System.Collections.Generic;
using Models;
using ServiceCalls;
using UnityEngine;

public class GachaScreen : NBUIMenuScreen
{
	public enum Choice
	{
		FRIENDSHIP = 0,
		PREMIUM = 1,
		ELEMENT = 2
	}

	public enum PremiumOptions
	{
		WEAPON = 0,
		WEAPON_BUNDLE = 1,
		ARMOR = 2,
		ARMOR_BUNDLE = 3
	}

	private enum ScreenState
	{
		NONE = -1,
		HOME = 0,
		SOCIAL_CHEST = 1,
		PREMIUM_OPTIONS = 2,
		PREMIUM_CHEST = 3,
		ELEMENT_OPTIONS = 4
	}

	[HideInInspector]
	public GameObject premiumCamera1;

	[HideInInspector]
	public GameObject premiumCamera2;

	[HideInInspector]
	public GameObject socialCamera;

	[HideInInspector]
	public GameObject premiumSection;

	[HideInInspector]
	public GameObject socialSection;

	[SerializeField]
	private Camera m_petCamera;

	[SerializeField]
	private UILabel m_socialPoint;

	public UILabel m_premiumCounter;

	public UILabel m_socialCounter;

	public GameObject m_socialPointIconOnCommonGachaBtn;

	public UILabel m_socialPointsCost;

	public UISprite m_freeBannerOnCommonGachaBtn;

	public UIAtlas m_uiAtlasRef;

	private GameObject _currentCamera;

	private List<string> m_friendNames = new List<string>();

	private int m_retry;

	private int m_retryMax = 5;

	private Services m_services;

	public UIButton m_holyDiamondsExchangeButton;

	public UILabel m_holyDiamondsCurrentLabel;

	public UIButton m_freeDiamondBtn;

	public UILabel m_freeDiamondLbl;

	private BackButtonMapping m_backButtonMapper;

	private GachaMenuPanel m_gachaMenuPanel;

	private TutorialManager m_tutorialManager;

	private ScreenState _currState = ScreenState.NONE;

	private int _lastButtonId;

	private ScreenState currState
	{
		get
		{
			return _currState;
		}
		set
		{
			_currState = value;
			if (_currState == ScreenState.HOME)
			{
				backButton.SetActiveRecursively(false);
				if ((!(m_tutorialManager != null) || !m_tutorialManager.enabled || m_tutorialManager.IsCompleted(Tutorial.State.Gacha)) && menuPanels[0] != null && menuPanels[0] is StandardMenuPanel)
				{
					menuPanels[0].EnableButton(0, true);
					menuPanels[0].EnableButton(1, true);
				}
				UpdateRedIconNum();
				if (m_backButtonMapper != null)
				{
					m_backButtonMapper.enabled = true;
				}
			}
			else if (_currState != ScreenState.NONE && m_backButtonMapper != null)
			{
				m_backButtonMapper.enabled = false;
			}
			RefreshHDDisplay();
		}
	}

	private void RefreshHDDisplay()
	{
		bool flag = _currState == ScreenState.HOME || _currState == ScreenState.PREMIUM_OPTIONS;
		flag &= !SingletonComponent<TutorialManager>.Instance.enabled;
		if (null != m_holyDiamondsExchangeButton)
		{
			Transform parent = m_holyDiamondsExchangeButton.transform.parent;
			if (null != parent)
			{
				parent.gameObject.SetActiveRecursively(flag);
				if (flag)
				{
					UIAnchor component = parent.GetComponent<UIAnchor>();
					if (null != component)
					{
						UICamera uICamera = UICamera.FindCameraForLayer(MBLayers.NameToLayer("UI"));   // PORT (CD-050)
						if (null != uICamera)
						{
							component.uiCamera = uICamera.cachedCamera;
						}
					}
				}
			}
		}
		if (flag)
		{
			PlayerDataManager instance = SingletonComponent<PlayerDataManager>.Instance;
			PlayerData playerData = ((!(null == instance)) ? instance.GetPlayerData() : null);
			if (null != m_holyDiamondsCurrentLabel && playerData != null && playerData.details != null)
			{
				int holyDiamonds = playerData.details.currencies.holyDiamonds;
				m_holyDiamondsCurrentLabel.text = string.Format(SingletonComponent<NBLocalization>.Instance.Get("holydiamonds_you_have_label"), holyDiamonds);
			}
		}
		if (null != m_freeDiamondBtn && null != m_freeDiamondBtn.transform.parent)
		{
			m_freeDiamondBtn.transform.parent.gameObject.SetActiveRecursively(flag);
		}
		if (null != m_freeDiamondLbl)
		{
			m_freeDiamondLbl.text = SingletonComponent<NBLocalization>.Instance.Get("metaps_description_label");
		}
	}

	private void Awake()
	{
		if (m_uiAtlasRef != null)
		{
			switch (SingletonComponent<SaveData>.Instance.GameData.Language)
			{
			case "Japanese":
				m_uiAtlasRef.replacement = LoadUIAtlas("UI/Atlas/UI_ATLAS_MM_TREASURE_Prefab_JP");
				break;
			case "Chinese_s":
				m_uiAtlasRef.replacement = LoadUIAtlas("UI/Atlas/UI_ATLAS_MM_TREASURE_Prefab_CHS");
				break;
			case "Chinese_t":
				m_uiAtlasRef.replacement = LoadUIAtlas("UI/Atlas/UI_ATLAS_MM_TREASURE_Prefab_CHT");
				break;
			case "Korean":
				m_uiAtlasRef.replacement = LoadUIAtlas("UI/Atlas/UI_ATLAS_MM_TREASURE_Prefab_KR");
				break;
			default:
				m_uiAtlasRef.replacement = LoadUIAtlas("UI/Atlas/UI_ATLAS_MM_TREASURE_Prefab");
				break;
			}
			if (m_uiAtlasRef.replacement == null)
			{
				m_uiAtlasRef.replacement = LoadUIAtlas("UI/Atlas/UI_ATLAS_MM_TREASURE_Prefab");
			}
		}
		premiumCamera1 = GachaCave.Instance.premiumCamera1;
		premiumCamera2 = GachaCave.Instance.premiumCamera2;
		socialCamera = GachaCave.Instance.socialCamera1;
		premiumSection = GachaCave.Instance.premiumCave;
		socialSection = GachaCave.Instance.socialCave;
		m_tutorialManager = SingletonComponent<TutorialManager>.Instance;
		m_backButtonMapper = UnityEngine.Object.FindObjectOfType(typeof(BackButtonMapping)) as BackButtonMapping;
	}

	private void Update()
	{
		if ((!(m_tutorialManager != null) || !m_tutorialManager.enabled || !m_tutorialManager.IsAnyTutorialExisting()) && (_currState == ScreenState.PREMIUM_CHEST || _currState == ScreenState.SOCIAL_CHEST) && UIPopupController.Instance != null)
		{
			if (UIPopupController.Instance.currentPopup != null && m_backButtonMapper.alwaysShowExitConfirmation)
			{
				m_backButtonMapper.enabled = true;
			}
			else if (UIPopupController.Instance.currentPopup == null && !m_backButtonMapper.alwaysShowExitConfirmation)
			{
				m_backButtonMapper.enabled = false;
			}
		}
	}

	protected override void Init()
	{
		if (MainMenu.Instance != null && backButton == null && MainMenu.Instance.m_sharedBackButton != null)
		{
			backButton = MainMenu.Instance.m_sharedBackButton.gameObject;
			m_backButton = MainMenu.Instance.m_sharedBackButton;
		}
		if ((bool)backButton)
		{
			UIButtonMessage component = backButton.GetComponent<UIButtonMessage>();
			component.target = base.gameObject;
			backButton.gameObject.SetActiveRecursively(false);
		}
		CheckForGiftSenders();
		CheckForHolyDiamondsRewards();
		PushState(ScreenState.HOME);
		if (MenuBackground3D.Instance != null)
		{
			MenuBackground3D.Instance.HideBackground();
			MenuBackground3D.Instance.m_camera.gameObject.SetActiveRecursively(false);
		}
		if (_currentCamera == null)
		{
			SwitchCamera(socialCamera);
		}
		if (m_socialPointsCost != null)
		{
			if (null != SingletonComponent<GachaManager>.Instance && SingletonComponent<GachaManager>.Instance.SocialGachaPrice >= 0)
			{
				m_socialPointsCost.text = SingletonComponent<GachaManager>.Instance.SocialGachaPrice.ToString();
			}
			else
			{
				m_socialPointsCost.text = GLOBAL.GAME_SETTINGS.social_points_per_ticket.ToString();
			}
		}
		if (menuPanels[0] != null)
		{
			m_gachaMenuPanel = menuPanels[0] as GachaMenuPanel;
		}
		GameManager.qualitySetting = GameManager.QualitySetting.Mid;
	}

	protected override void Reset()
	{
		if (null != MainMenu.Instance)
		{
			MainMenu.Instance.ShowTopAndBottomBar(true);
		}
		ClearStack();
		socialSection.SetActiveRecursively(false);
		premiumSection.SetActiveRecursively(false);
		SwitchCamera(socialCamera);
		GameManager.qualitySetting = GameManager.QualitySetting.High;
		if (null != m_backButtonMapper && !m_backButtonMapper.enabled)
		{
			m_backButtonMapper.enabled = true;
		}
	}

	private void OnBack()
	{
		if (_currState == ScreenState.PREMIUM_OPTIONS || _currState == ScreenState.ELEMENT_OPTIONS)
		{
			UIPopupController.SetAllInputsEnabled(false);
			SwitchCamera(premiumCamera1);
			premiumCamera1.SendMessage("PlayAnimationReverse", null, SendMessageOptions.DontRequireReceiver);
			base.currentPanel.OnExitTransition();
			backButton.SetActiveRecursively(false);
			MainMenu.Instance.ShowBottomBar(true);
			PopState();
		}
		else if (_currState == ScreenState.PREMIUM_CHEST || _currState == ScreenState.SOCIAL_CHEST)
		{
			Home();
		}
		else
		{
			PopState();
		}
	}

	private void PushState(ScreenState nextState)
	{
		if (_currState != nextState)
		{
			if (nextState >= ScreenState.HOME && (int)nextState < menuPanels.Length && PushPanelWithTransitions(menuPanels[(int)nextState]))
			{
				currState = nextState;
			}
		}
	}

	private void PopState(bool useTransitions = true)
	{
		if (useTransitions)
		{
			PopPanelWithTransitions(true, OnPopPanelExit, OnPopPanelEnter);
			return;
		}
		PopPanel();
		ScreenState currentPanelIndex = (ScreenState)GetCurrentPanelIndex();
		currState = currentPanelIndex;
	}

	private void OnPopPanelExit()
	{
		ScreenState currentPanelIndex = (ScreenState)GetCurrentPanelIndex();
		currState = currentPanelIndex;
	}

	private void OnPopPanelEnter()
	{
		UIPopupController.Instance.EnableUI();
	}

	private UIAtlas LoadUIAtlas(string p_path)
	{
		UIAtlas result = null;
		GameObject gameObject = Resources.Load(p_path) as GameObject;
		if (gameObject != null)
		{
			result = gameObject.GetComponent<UIAtlas>();
		}
		else
		{
			Dbg.LogError("Unable to load: " + p_path + " as UIAtlas");
		}
		return result;
	}

	private void UpdatePremiumCounter(bool p_show)
	{
		PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		int premiumGachaCount = playerData.PremiumGachaCount;
		Dbg.Log(" You Have " + premiumGachaCount + " Free Premium Gacha.");
		MainMenu.Instance.UpdateFreeTicketsCounter(m_premiumCounter, premiumGachaCount, p_show);
	}

	private void UpdateSocialCounter(bool p_show)
	{
		PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		int socialGachaCount = playerData.SocialGachaCount;
		Dbg.Log(" You Have " + socialGachaCount + " Free Social Gacha.");
		MainMenu.Instance.UpdateFreeTicketsCounter(m_socialCounter, socialGachaCount, p_show);
		UpdateSocialGachaFreeTex(playerData.FreeSocialGachaTicketCount > 0);
		UpdateSocialCostIcon(playerData.FreeSocialGachaTicketCount <= 0);
		UpdateSocialPoints();
	}

	private void UpdateSocialGachaFreeTex(bool p_show)
	{
		Dbg.LogEx("UpdateSocialGachaFreeText, show: {0}", Dbg.NBLogType.GACHA, p_show.ToString());
		m_freeBannerOnCommonGachaBtn.enabled = p_show;
	}

	private void UpdateSocialCostIcon(bool p_show)
	{
		Dbg.LogEx("UpdateSocialCostIcon, show: {0}", Dbg.NBLogType.GACHA, p_show.ToString());
		if (!p_show)
		{
			m_socialPointsCost.text = string.Empty;
		}
	}

	private void UpdateSocialPoints()
	{
		if (null != m_socialPoint)
		{
			PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
			if (playerData != null)
			{
				m_socialPoint.text = playerData.m_userData.socialCurrency.ToString();
			}
		}
		else
		{
			Dbg.LogWarn("m_socialPoint is null, unable to update the social point text");
		}
	}

	private void SwitchCamera(GameObject newCamera)
	{
		if (!(_currentCamera == newCamera))
		{
			if (_currentCamera != null)
			{
				MBHier.GetComponentInChildren<Camera>(_currentCamera).enabled = false;
			}
			_currentCamera = newCamera;
			if (_currentCamera != null)
			{
				MBHier.GetComponentInChildren<Camera>(_currentCamera).enabled = true;
			}
		}
	}

	private void OnButtonClick(GameObject source)
	{
		if (source == backButton)
		{
			OnBack();
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_cancel");
			return;
		}
		if (null != m_holyDiamondsExchangeButton && m_holyDiamondsExchangeButton.gameObject == source)
		{
			MainMenu instance = MainMenu.Instance;
			if (null != instance)
			{
				instance.ShowHolyDiamondsExchangePanel();
			}
			return;
		}
		if (null != m_freeDiamondBtn && m_freeDiamondBtn.gameObject == source)
		{
			Metaps.LaunchOfferWall();
			return;
		}
		UIButtonMessage component = source.GetComponent<UIButtonMessage>();
		if (_currState == ScreenState.HOME)
		{
			switch ((Choice)component.id)
			{
			case Choice.FRIENDSHIP:
				PlaySingleSocial();
				break;
			case Choice.PREMIUM:
				EnableGachaButtons(false);
				UIPopupController.Instance.ShowLoadingPopupNetWait();
				SingletonComponent<GachaManager>.Instance.RefreshGachaEvents(OnRefreshPremiumGachaSuccess, OnRefreshPremiumGachaFail);
				break;
			}
		}
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_ok");
	}

	private void PlaySingleSocial()
	{
		TutorialManager instance = SingletonComponent<TutorialManager>.Instance;
		bool flag = null != instance && instance.enabled;
		PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		int socialGachaCount = playerData.SocialGachaCount;
		if (!flag && socialGachaCount > 1)
		{
			int num = Mathf.Min(10, socialGachaCount);
			string text = "gacha_social_claim_multi_label";
			string p_confirmTxt = text;
			if (null != SingletonComponent<NBLocalization>.Instance)
			{
				string format = SingletonComponent<NBLocalization>.Instance.Get(text);
				p_confirmTxt = string.Format(format, num);
			}
			UIPopupController.Instance.ShowMediumPopupMSG("common_prompt_gacha_social_multi_header", "common_prompt_gacha_social_multi", p_confirmTxt, "gacha_social_claim_single_label", OnSocialClaimSingle, OnSocialClaimMulti, OnSocialClaimCancel);
		}
		else
		{
			OnSocialClaimSingle();
		}
	}

	private void OnSocialClaimSingle()
	{
		EnableGachaButtons(false);
		UIPopupController.Instance.ShowLoadingPopupNetWait();
		bool p_isTutorial = m_tutorialManager != null && m_tutorialManager.enabled && !m_tutorialManager.IsCompleted(Tutorial.State.Gacha);
		SingletonComponent<GachaManager>.Instance.PlaySocialGacha(OnPlaySocialGachaSuccess, OnPlaySocialGachaFail, p_isTutorial, false);
	}

	private void OnSocialClaimMulti()
	{
		EnableGachaButtons(false);
		PlayMultiSocial();
	}

	private void OnSocialClaimCancel()
	{
	}

	private void PlayMultiSocial()
	{
		UIPopupController.Instance.ShowLoadingPopupNetWait();
		bool p_isTutorial = m_tutorialManager != null && m_tutorialManager.enabled && !m_tutorialManager.IsCompleted(Tutorial.State.Gacha);
		SingletonComponent<GachaManager>.Instance.PlaySocialMultiGacha(OnPlayMultiSocialGachaSuccess, OnPlaySocialGachaFail, p_isTutorial, false);
	}

	private void RetrySingleSocialGacha()
	{
		EnableGachaButtons(false);
		UIPopupController.Instance.ShowLoadingPopupNetWait();
		bool p_isTutorial = m_tutorialManager != null && m_tutorialManager.enabled && !m_tutorialManager.IsCompleted(Tutorial.State.Gacha);
		SingletonComponent<GachaManager>.Instance.PlaySocialGacha(OnPlaySocialGachaSuccess, OnPlaySocialGachaFail, p_isTutorial, true);
	}

	private void RetryMultiSocialGacha()
	{
		UIPopupController.Instance.ShowLoadingPopupNetWait();
		bool p_isTutorial = m_tutorialManager != null && m_tutorialManager.enabled && !m_tutorialManager.IsCompleted(Tutorial.State.Gacha);
		SingletonComponent<GachaManager>.Instance.PlaySocialMultiGacha(OnPlayMultiSocialGachaSuccess, OnPlaySocialGachaFail, p_isTutorial, true);
	}

	private void OnPlaySocialGachaSuccess(Services.ServiceHandle serviceHandle)
	{
		if (serviceHandle == null)
		{
			UIPopupController.Instance.HideLoadingPopup();
			UIPopupController.Instance.ShowServerGenericError();
			return;
		}
		if (null == UIPopupController.Instance || null == MainMenu.Instance || null == socialSection || null == GachaCave.Instance)
		{
			UIPopupController.Instance.HideLoadingPopup();
			Dbg.LogError("OnPlaySocialGachaSuccess null instance found");
			return;
		}
		Messenger<Choice>.Invoke("gacha_menu_selected", Choice.FRIENDSHIP);
		UIPopupController.Instance.HideLoadingPopup();
		UIPopupController.SetAllInputsEnabled(false);
		socialSection.SetActiveRecursively(true);
		Models.GachaResult gachaResult = (Models.GachaResult)serviceHandle.returnValue;
		if (gachaResult == null)
		{
			Dbg.LogError("OnPlaySocialGachaSuccess result is null");
			return;
		}
		if (gachaResult.characterDetails != null)
		{
			MainMenu.Instance.SetCurrencies(gachaResult.characterDetails.currencies);
		}
		MainMenu.Instance.inventory.SetDirty();
		MainMenu.Instance.EnableHeader(false, string.Empty);
		if (gachaResult.items.Count > 0)
		{
			if (m_tutorialManager != null && m_tutorialManager.IsCompleted(Tutorial.State.All))
			{
				SaveData.MsData gameData = SingletonComponent<SaveData>.Instance.GameData;
				gameData.OpenSocialTreasureCount++;
				gameData.SaveAchievementData();
			}
			int tier = gachaResult.items[0].tier;
			GachaCave.Instance.ShowChest(tier, Choice.FRIENDSHIP);
		}
		UpdateSocialCounter(true);
		PushState(ScreenState.SOCIAL_CHEST);
		SwitchCamera(socialCamera);
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_social_gacha_intro");
	}

	private void OnPlayMultiSocialGachaSuccess(Services.ServiceHandle serviceHandle)
	{
		UIPopupController.Instance.HideLoadingPopup();
		UIPopupController.SetAllInputsEnabled(false);
		socialSection.SetActiveRecursively(true);
		Models.GachaResult gachaResult = (Models.GachaResult)serviceHandle.returnValue;
		if (gachaResult.characterDetails != null)
		{
			MainMenu.Instance.SetCurrencies(gachaResult.characterDetails.currencies);
		}
		MainMenu.Instance.inventory.SetDirty();
		MainMenu.Instance.EnableHeader(false, string.Empty);
		if (gachaResult.items.Count > 0)
		{
			if (m_tutorialManager != null && m_tutorialManager.IsCompleted(Tutorial.State.All))
			{
				SaveData.MsData gameData = SingletonComponent<SaveData>.Instance.GameData;
				gameData.OpenSocialTreasureCount += 10;
				gameData.SaveAchievementData();
			}
			int tier = gachaResult.items[0].tier;
			GachaCave.Instance.ShowChest(tier, Choice.FRIENDSHIP);
		}
		UpdateSocialCounter(true);
		PushState(ScreenState.SOCIAL_CHEST);
		SwitchCamera(socialCamera);
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_social_gacha_intro");
	}

	private void OnPlaySocialGachaFail(Services.ServiceHandle serviceHandle)
	{
		UIPopupController.Instance.HideLoadingPopup();
		Dbg.LogWarn("GachaScreen::OnPlayGachaFail: " + ServiceCallUtils.GetErrorMessage(serviceHandle));
		if (serviceHandle == null)
		{
			UIPopupController.Instance.ShowServerGenericError(new PopupCallback(EnableGachaButtons));
			return;
		}
		if (serviceHandle.result == Services.ConnectionResults.Timeout)
		{
			if (serviceHandle is PlayMultiGacha)
			{
				UIPopupController.Instance.ShowTimeoutForceRetry(RetryMultiSocialGacha);
			}
			else
			{
				UIPopupController.Instance.ShowTimeoutForceRetry(RetrySingleSocialGacha);
			}
			return;
		}
		if (serviceHandle.IsNoInternet())
		{
			if (serviceHandle is PlayMultiGacha)
			{
				UIPopupController.Instance.ShowNoNetForceRetry(RetryMultiSocialGacha);
			}
			else
			{
				UIPopupController.Instance.ShowNoNetForceRetry(RetrySingleSocialGacha);
			}
			return;
		}
		ServiceResponseCodes nReturnCode = (ServiceResponseCodes)serviceHandle.nReturnCode;
		if (nReturnCode == ServiceResponseCodes.EXCEPTION_NOT_ENOUGH_CURRENCY)
		{
			UIPopupController.Instance.ShowOKPopupMSG("common_prompt_social_gacha_failed_header", "not_enough_social_ticket", new PopupCallback(EnableGachaButtons));
			return;
		}
		UIPopupController.Instance.ShowMediumPopupMSG("common_prompt_social_gacha_failed_header", "[Error:" + serviceHandle.nReturnCode + "] " + SingletonComponent<NBLocalization>.Instance.Get("common_prompt_social_gacha_failed_content"), RetrySingleSocialGacha, new PopupCallback(EnableGachaButtons));
	}

	private void EnableGachaButtons()
	{
		EnableGachaButtons(true);
	}

	private void EnableGachaButtons(bool p_enable = true)
	{
		if (m_gachaMenuPanel != null)
		{
			m_gachaMenuPanel.EnableGachaBtns(p_enable);
		}
	}

	private void OnRefreshSocialGachaFail(Services.ServiceHandle serviceHandle)
	{
		UIPopupController.Instance.HideLoadingPopup();
		Dbg.LogWarn("OnRefreshSocialGachaFail: " + ServiceCallUtils.GetErrorMessage(serviceHandle));
		if (serviceHandle == null || serviceHandle.IsNoInternet())
		{
			UIPopupController.Instance.ShowSmallPopupMSG("common_prompt_nonet_retry", OnRefreshSocialGachaFailPopupYes);
		}
		else
		{
			UIPopupController.Instance.ShowServerGenericError(null, serviceHandle.nReturnCode);
		}
	}

	private void OnRefreshSocialGachaSuccess(Services.ServiceHandle serviceHandle)
	{
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_social_gacha_selection");
		UpdateSocialPoints();
		PlaySingleSocial();
	}

	private void OnRefreshSocialGachaFailPopupYes()
	{
		Debug.LogError("OnRefreshSocialGachaFailPopupYes This is called!");
		UIPopupController.Instance.ShowLoadingPopupNetWait();
		SingletonComponent<GachaManager>.Instance.RefreshGachaEvents(OnRefreshSocialGachaSuccess, OnRefreshSocialGachaFail);
	}

	private IEnumerator DelayGotoEpicGacha()
	{
		yield return new MBWaitForSeconds(0.8f);   // PORT (CD-059)
		GotoEpicGacha();
	}

	private void GotoEpicGacha()
	{
		Dbg.Log("GotoEpicGacha");
		SingletonComponent<GachaManager>.Instance.RefreshGachaEvents(OnRefreshPremiumGachaSuccess, OnRefreshPremiumGachaFail);
	}

	private IEnumerator JumpToEpicTreasures()
	{
		if (currState == ScreenState.PREMIUM_OPTIONS)
		{
			yield break;
		}
		Home();
		GachaMenuPanel panel = menuPanels[0] as GachaMenuPanel;
		if (null != panel)
		{
			UIPopupController.Instance.ShowLoadingPopupNetWait();
			if (null != panel.GetComponent<Animation>() && null != panel.GetComponent<Animation>().clip)
			{
				yield return new MBWaitForSeconds(panel.GetComponent<Animation>().clip.length);   // PORT (CD-059)
			}
			OnButtonClick(panel.buttons[1]);
		}
	}

	private void OnRefreshPremiumGachaFailPopupYes()
	{
		UIPopupController.Instance.ShowLoadingPopupNetWait();
		SingletonComponent<GachaManager>.Instance.RefreshGachaEvents(OnRefreshPremiumGachaSuccess, OnRefreshPremiumGachaFail);
	}

	private void OnGachaPremiumTryAgain(bool again)
	{
		GachaPremiumOptionsPanel gachaPremiumOptionsPanel = menuPanels[2] as GachaPremiumOptionsPanel;
		Dbg.Log("panel = " + gachaPremiumOptionsPanel.name + ", again " + again);
		if (again)
		{
			gachaPremiumOptionsPanel.prevState = GachaPremiumOptionsPanel.ActionState.TryAgainLastGacha;
		}
		else
		{
			gachaPremiumOptionsPanel.prevState = GachaPremiumOptionsPanel.ActionState.TryAgainShowOptions;
			backButton.gameObject.SetActiveRecursively(true);
			if (backButton.transform.localPosition.y > -100f || backButton.transform.localPosition.y < -102f)
			{
				Vector3 localPosition = backButton.transform.localPosition;
				localPosition.y = -101f;
				backButton.transform.localPosition = localPosition;
			}
			if ((bool)MainMenu.Instance)
			{
				MainMenu.Instance.EnableHeader(true, "treasure_header");
			}
		}
		MainMenu.Instance.ShowTopBar(true);
		PopState(false);
	}

	private void OnPremiumGachaPlaySuccess()
	{
		premiumSection.SetActiveRecursively(true);
		PushState(ScreenState.PREMIUM_CHEST);
		SwitchCamera(premiumCamera2);
	}

	private void CheckForGiftSenders()
	{
		m_services = SingletonComponent<Services>.Instance;
		if (m_services == null)
		{
			return;
		}
		PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		if (playerData != null)
		{
			if (playerData.m_userData.freePremiumGacha == 0)
			{
				Dbg.Log("You don't need to Call GetGiftSenders ?? Do it anyway for now. PlayerData freeGacha : " + playerData.m_userData.freePremiumGacha);
			}
			m_retry = 0;
			m_friendNames.Clear();
			int num = MBPrefs.GetInt("PlayerDataManager.gifts.Count");
			for (int i = 0; i < num; i++)
			{
				string key = "PlayerDataManager.gifts[" + i + "]";
				string item = MBPrefs.GetString(key);
				MBPrefs.DeleteKey(key);
				m_friendNames.Add(item);
			}
			MBPrefs.SetInt("PlayerDataManager.gifts.Count", 0);
			ShowGiftPopup();
			MBPrefs.Save();
		}
	}

	private void ShowGiftPopup()
	{
		if (m_friendNames != null && m_friendNames.Count != 0)
		{
			string arg = m_friendNames[0] + "!";
			string format = SingletonComponent<NBLocalization>.Instance.Get("social_gotagift_content");
			string txt = string.Format(format, arg);
			UIPopupController.Instance.ShowOKPopupMSG("social_gotagift_header", txt, ShowGiftPopup);
			m_friendNames.RemoveAt(0);
		}
	}

	public override void OnEnable()
	{
		Messenger<int>.AddListener("exchange_update_points", OnHolyDiamondsUpdated);
		Init();
		if (SingletonComponent<GachaManager>.Instance != null && SingletonComponent<GachaManager>.Instance.IsFromPromotion)
		{
			SingletonComponent<GachaManager>.Instance.IsFromPromotion = false;
			UIPopupController.Instance.ShowLoadingPopupNetWait();
			if (MainMenu.Instance.currScreen is GachaScreen)
			{
				StartCoroutine(DelayGotoEpicGacha());
			}
		}
		RefreshHDDisplay();
	}

	public override void OnDisable()
	{
		Messenger<int>.RemoveListener("exchange_update_points", OnHolyDiamondsUpdated);
		Reset();
	}

	private void OnHolyDiamondsUpdated(int p_points)
	{
		RefreshHDDisplay();
	}

	public void OnRefreshPremiumGachaSuccess(Services.ServiceHandle serviceHandle)
	{
		UIPopupController.Instance.HideLoadingPopup();
		UIPopupController.SetAllInputsEnabled(false);
		PushState(ScreenState.PREMIUM_OPTIONS);
		SwitchCamera(premiumCamera1);
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_premium_gacha_selection");
		Messenger<Choice>.Invoke("gacha_menu_selected", Choice.PREMIUM);
	}

	public void OnRefreshPremiumGachaFail(Services.ServiceHandle serviceHandle)
	{
		UIPopupController.Instance.HideLoadingPopup();
		EnableGachaButtons();
		Dbg.LogWarn("OnRefreshSocialGachaFail: " + ServiceCallUtils.GetErrorMessage(serviceHandle));
		if (serviceHandle == null || serviceHandle.IsNoInternet())
		{
			UIPopupController.Instance.ShowSmallPopupMSG("common_prompt_nonet_retry", OnRefreshPremiumGachaFailPopupYes);
		}
		else
		{
			UIPopupController.Instance.ShowServerGenericError(null, serviceHandle.nReturnCode);
		}
	}

	public override void Home()
	{
		if (_currState != ScreenState.HOME)
		{
			if ((bool)MainMenu.Instance)
			{
				MainMenu.Instance.EnableHeader(true, "treasure_header");
			}
			base.Home();
		}
	}

	public override void UpdateRedIconNum()
	{
		UpdatePremiumCounter(_currState == ScreenState.HOME);
		UpdateSocialCounter(_currState == ScreenState.HOME);
	}

	public static void CheckForHolyDiamondsRewards()
	{
		TutorialManager instance = SingletonComponent<TutorialManager>.Instance;
		if (null != instance && instance.enabled)
		{
			return;
		}
		GachaManager instance2 = SingletonComponent<GachaManager>.Instance;
		UIPopupController instance3 = UIPopupController.Instance;
		PlayerDataManager instance4 = SingletonComponent<PlayerDataManager>.Instance;
		PlayerData playerData = ((!(null == instance4)) ? instance4.GetPlayerData() : null);
		if (null != instance2 && null != instance3 && playerData != null)
		{
			int lastHolyDiamondsRewards = instance2.lastHolyDiamondsRewards;
			if (lastHolyDiamondsRewards > 0)
			{
				int holyDiamonds = playerData.details.currencies.holyDiamonds;
				instance3.ShowHolyDiamondsPopup(HolyDiamondsPopup.Type.REWARD, lastHolyDiamondsRewards, holyDiamonds);
			}
		}
	}
}
