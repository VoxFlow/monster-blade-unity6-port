using System.Collections;
using System.Collections.Generic;
using Models;
using ServiceCalls;
using UnityEngine;

public class SystemInitScene : MonoBehaviour
{
	private int m_itemMgrCounter;

	private GameInit m_gameInit;

	private bool m_needToUpdateDeviceID;

	private string m_nubeeId = string.Empty;

	private string m_deviceId = string.Empty;

	[SerializeField]
	private ServerCheck m_serverCheck;

	[SerializeField]
	private GameObject m_titleScreen;

	[SerializeField]
	private TitleScreenUiReceiver m_titleScreenScript;

	private bool m_patchDownloaded;

	private bool m_bStartLoading;

	private bool m_bNubeePlatformConnected;

	private void Awake()
	{
		m_bStartLoading = false;
		m_bNubeePlatformConnected = false;
	}

	public void ShowTitleScreen()
	{
		SingletonComponent<SoundManager>.Instance.InitManager();
		SingletonComponent<SoundManager>.Instance.LoadTitleSounds();
		SingletonComponent<SoundManager>.Instance.PlayTitleMusic();
		SingletonComponent<FaderGui>.Instance.FadeIn();
		if (m_titleScreen != null)
		{
			m_titleScreen.SetActiveRecursively(true);
		}
		StartupCheck();
	}

	private void EnableTitleScreen()
	{
		if (m_titleScreenScript != null)
		{
			m_titleScreenScript.enabled = true;
		}
	}

	private void StartupCheck()
	{
		if (null != m_serverCheck)
		{
			m_serverCheck.StartupCheck(QueryNubeeID);
		}
		else
		{
			Dbg.LogError("m_serverCheck script reference missing!");
		}
	}

	private void QueryNubeeID()
	{
		Log(SingletonComponent<NBLocalization>.Instance.Get("common_loading_loginnubee"));
		ConnectToNubeePlatform();
		EnableTitleScreen();
	}

	public void DoInitialize()
	{
		EtceteraAndroid.initTTS();
		UIPopupController.Instance.ShowFullScreenLoading(UIPopupController.LoadingScreenMode.FULLSCREEN_STARTUP);
		Log(SingletonComponent<NBLocalization>.Instance.Get("common_loading_preparesystem"));
		UIPopupController.Instance.ShowLoadingPopup(string.Empty, string.Empty);
		m_bStartLoading = true;
		StartCoroutine(InitializeManagers());
		Messenger<LoadingProgressManager.LoadProgress>.Invoke("progress_bar", LoadingProgressManager.LoadProgress.StartInit);
		Messenger<LoadingProgressManager.LoadProgress>.Invoke("progress_bar", LoadingProgressManager.LoadProgress.EndFirstInit);
	}

	private void Log(string p_text)
	{
		if (m_bStartLoading)
		{
			UIPopupController.Instance.ShowLoadingPopup(string.Empty, p_text);
		}
		Dbg.Log(p_text, Dbg.NBLogType.SYSTEM_INIT);
	}

	private void LogError(string p_text)
	{
		Dbg.LogError(p_text, Dbg.NBLogType.SYSTEM_INIT);
	}

	private string ToNumberString(string p_string)
	{
		int num = 0;
		int num2 = 0;
		p_string.Trim();
		p_string = p_string.Replace(" ", "0");
		for (int i = 0; i < p_string.Length; i++)
		{
			if (i >= p_string.Length / 2)
			{
				num += p_string[i];
			}
			else
			{
				num2 += p_string[i];
			}
		}
		if (num2 <= 0)
		{
			num2 = 1;
		}
		return (num % num2).ToString();
	}

	private void ConnectToNubeePlatform()
	{
		Dbg.Log("Connecting to Nubee Platform", Dbg.NBLogType.SYSTEM_INIT);
		NBProfiler.BeginTask("CONNECT_NUBEE_PLATFORM");
		if (m_bStartLoading)
		{
			UIPopupController.Instance.ShowLoadingPopupNetWait();
		}
		NubeePlatformManager.OnConnectionSuccess -= OnConnectNbpSuccess;
		NubeePlatformManager.OnConnectionFailed -= OnConnectNbpFail;
		NubeePlatformManager.OnConnectionSuccess += OnConnectNbpSuccess;
		NubeePlatformManager.OnConnectionFailed += OnConnectNbpFail;
		NubeePlatformBinding.TryConnect(GLOBAL.SERVER_NUBEE_PLATFORM_APPID, GLOBAL.SERVER_NUBEE_PLATFORM_HASHSALT);
	}

	private void OnConnectNbpSuccess()
	{
		NubeePlatformManager.OnConnectionSuccess -= OnConnectNbpSuccess;
		NubeePlatformManager.OnConnectionFailed -= OnConnectNbpFail;
		Messenger<LoadingProgressManager.LoadProgress>.Invoke("progress_bar", LoadingProgressManager.LoadProgress.NBPSuccess);
		UIPopupController.Instance.HideLoadingPopup();
		Dbg.Log("Successfully connected to Nubee Platform!");
		Dbg.Log("Received nubee ID and device ID from Nubee Platform", Dbg.NBLogType.SYSTEM_INIT);
		m_nubeeId = NubeePlatformBinding.GetNubeeID();
		m_deviceId = NubeePlatformBinding.GetDeviceInfo();
		Dbg.Log(" [OnConnectNbpSuccess] Nubee Id is: " + m_nubeeId + "\nDevice Id is: " + m_deviceId, Dbg.NBLogType.SYSTEM_INIT);
		m_needToUpdateDeviceID = SingletonComponent<SaveData>.Instance.GameData.NeedToUpdateDeviceID(m_nubeeId, m_deviceId);
		Services.SetNubeeAccount(m_nubeeId, m_deviceId);
		SingletonComponent<SaveData>.Instance.GameData.SetNubeeAccount(m_nubeeId, m_deviceId);
		SingletonComponent<SaveData>.Instance.SaveDataToPlayerPrefs();
		RegisterNubeeAccount();
		SingletonComponent<SocialNetworkManager>.Instance.InitSocialNetwork();
		NBProfiler.EndTask("CONNECT_NUBEE_PLATFORM");
	}

	private void OnConnectNbpFail()
	{
		NubeePlatformManager.OnConnectionSuccess -= OnConnectNbpSuccess;
		NubeePlatformManager.OnConnectionFailed -= OnConnectNbpFail;
		NBProfiler.EndTask("CONNECT_NUBEE_PLATFORM");
		Dbg.LogWarn("Failed to connect to Nubee Platform! Retrying...");
		if (UIPopupController.Instance != null && m_bStartLoading)
		{
			UIPopupController.Instance.ShowServerGenericError(ConnectToNubeePlatform);
		}
		else
		{
			ConnectToNubeePlatform();
		}
	}

	private void RegisterNubeeAccount()
	{
		NBProfiler.BeginTask("REGISTER_NUBEE_ACCOUNT");
		Dbg.Log("Registering for Nubee Account.", Dbg.NBLogType.SYSTEM_INIT);
		CheckDeviceID();
	}

	private void CheckDeviceID()
	{
		if (m_needToUpdateDeviceID)
		{
			SingletonComponent<Services>.Instance.UpdateDeviceId(m_nubeeId, m_deviceId, OnUpdateDeviceIDSuccess, OnUpdateDeviceIDFail);
		}
		else
		{
			CreateOrUpdateNubeeAccountServiceCall();
		}
	}

	private void OnUpdateDeviceIDSuccess(Services.ServiceHandle p_handle)
	{
		Dbg.Log("OnUpdateDeviceIDSuccess", Dbg.NBLogType.SYSTEM_INIT);
		CreateOrUpdateNubeeAccountServiceCall();
	}

	private void OnUpdateDeviceIDFail(Services.ServiceHandle p_handle, string p_error)
	{
		Dbg.LogError("OnUpdateDeviceIDFail Code : " + ServiceCallUtils.GetErrorMessage(p_handle), Dbg.NBLogType.SYSTEM_INIT);
		if (m_bStartLoading)
		{
			if (p_handle == null)
			{
				UIPopupController.Instance.ShowServerGenericError(CheckDeviceID);
			}
			else if (p_handle.IsNoInternet())
			{
				UIPopupController.Instance.ShowNoNetMessage(CheckDeviceID);
			}
			else
			{
				UIPopupController.Instance.ShowServerGenericError(CheckDeviceID, p_handle.nReturnCode);
			}
		}
		else
		{
			CheckDeviceID();
		}
	}

	private void CreateOrUpdateNubeeAccountServiceCall(bool p_isTutorial = false)
	{
		if (!SingletonComponent<SaveData>.Instance.GameData.CharacterCreated || SingletonComponent<SaveData>.Instance.GameData.IsDeviceLanguageChanged || SingletonComponent<SaveData>.Instance.GameData.NeedUpdateFBID || SingletonComponent<SaveData>.Instance.GameData.NeedUpdateFBToken || SingletonComponent<SaveData>.Instance.GameData.NeedUpdateFBName)
		{
			Services instance = SingletonComponent<Services>.Instance;
			SaveData instance2 = SingletonComponent<SaveData>.Instance;
			instance.CreateOrUpdateNubeeAccount(m_nubeeId, m_deviceId, instance2.GameData.CharacterName, NBUnity.ExpandedToIso2(instance2.GameData.Language), instance2.GameData.FBID, instance2.GameData.FBToken, string.Empty, p_isTutorial, OnRegisterNubeeAccountSuccess, OnRegisterNubeeAccountFail);
		}
		else
		{
			Dbg.Log("Character already register with MS Server.");
			OnRegisterNubeeAccountSuccess(null);
		}
	}

	private void OnRegisterNubeeAccountSuccess(Services.ServiceHandle p_handle)
	{
		if (!SingletonComponent<SaveData>.Instance.GameData.CharacterCreated)
		{
			SingletonComponent<SaveData>.Instance.GameData.CharacterCreated = true;
		}
		SingletonComponent<SaveData>.Instance.GameData.SaveDeviceLanguage();
		SingletonComponent<SaveData>.Instance.GameData.NeedUpdateFBID = false;
		SingletonComponent<SaveData>.Instance.GameData.NeedUpdateFBToken = false;
		SingletonComponent<SaveData>.Instance.GameData.NeedUpdateFBName = false;
		GoToInitSequence();
		Messenger<LoadingProgressManager.LoadProgress>.Invoke("progress_bar", LoadingProgressManager.LoadProgress.NubeeAccountRegisterSuccess);
	}

	private void OnRegisterNubeeAccountFail(Services.ServiceHandle p_handle, string p_error)
	{
		UIPopupController.Instance.HideLoadingPopup();
		if (m_bStartLoading)
		{
			if (p_handle == null)
			{
				UIPopupController.Instance.ShowServerGenericError(OnRegisterNubeeAccountRetry);
				return;
			}
			Dbg.LogWarn("Register for Nubee Account fail: " + ServiceCallUtils.GetErrorMessage(p_handle), Dbg.NBLogType.SYSTEM_INIT);
			if (p_handle.IsNoInternet())
			{
				UIPopupController.Instance.ShowNoNetMessage(OnRegisterNubeeAccountRetry);
			}
			else if (p_handle.nReturnCode == 301)
			{
				OnRegisterNubeeAccountAlreadyHasAccount(p_handle);
			}
			else
			{
				UIPopupController.Instance.ShowServerGenericError(OnRegisterNubeeAccountRetry, p_handle.nReturnCode);
			}
		}
		else
		{
			OnRegisterNubeeAccountRetry();
		}
	}

	private void OnRegisterNubeeAccountAlreadyHasAccount(Services.ServiceHandle p_handle)
	{
		Dbg.Log("Register for Nubee Account already has account, continuing.", Dbg.NBLogType.SYSTEM_INIT);
		UIPopupController.Instance.HideLoadingPopup();
		GoToInitSequence();
	}

	private void OnRegisterNubeeAccountRetry()
	{
		if (m_bStartLoading)
		{
			UIPopupController.Instance.ShowLoadingPopupNetWait();
		}
		RegisterNubeeAccount();
	}

	private void GoToInitSequence()
	{
		Dbg.Log("Starting Init Sequence", Dbg.NBLogType.SYSTEM_INIT);
		NBProfiler.EndTask("REGISTER_NUBEE_ACCOUNT");
		GameInitialization();
	}

	private void GameInitialization()
	{
		NBProfiler.BeginTask("STARTUP_SEQUENCE");
		string empty = string.Empty;
		string empty2 = string.Empty;
		string empty3 = string.Empty;
		string text = "Unknown";
		Dbg.Log("Init Game for Android");
		empty = "android";
		empty3 = "android";
		text = SystemInfo.deviceModel;
		string p_language = NBUnity.ExpandedToIso2(SingletonComponent<SaveData>.Instance.GameData.Language);
		string deviceOsVersion = NBUnity.GetDeviceOsVersion();
		string p_gender = "M";
		if (!SingletonComponent<SaveData>.Instance.GameData.IsMale())
		{
			p_gender = "F";
		}
		Dbg.Log("Device Name : " + text);
		SingletonComponent<Services>.Instance.GameInitialization(m_nubeeId, m_deviceId, empty, GLOBAL.PATCH_VERSION, empty2, p_language, p_gender, empty3, deviceOsVersion, text, OnGameInitializationSuccess, OnGameInitializationFailed);
		StartCoroutine(InitOfflineManagers());
	}

	private IEnumerator InitOfflineManagers()
	{
		SingletonComponent<SaveData>.Instance.LoadDataFromPlayerPrefs();
		Messenger<LoadingProgressManager.LoadProgress>.Invoke("progress_bar", LoadingProgressManager.LoadProgress.LoadDataFromPref);
		yield return null;
		SingletonComponent<DailyManager>.Instance.InitManager();
		Messenger<LoadingProgressManager.LoadProgress>.Invoke("progress_bar", LoadingProgressManager.LoadProgress.DailyManagerInit);
		yield return null;
		SingletonComponent<NotifyManager>.Instance.InitManager();
		Messenger<LoadingProgressManager.LoadProgress>.Invoke("progress_bar", LoadingProgressManager.LoadProgress.NotifyManagerInit);
		yield return null;
		Messenger<LoadingProgressManager.LoadProgress>.Invoke("progress_bar", LoadingProgressManager.LoadProgress.SoundManagerInit);
		yield return null;
		NBCacheManager.Instance.Initialize();
		Messenger<LoadingProgressManager.LoadProgress>.Invoke("progress_bar", LoadingProgressManager.LoadProgress.NBCacheManagerInit);
		yield return null;
		yield return null;
	}

	private void OnGameInitializationSuccess(Services.ServiceHandle p_handle)
	{
		Messenger<LoadingProgressManager.LoadProgress>.Invoke("progress_bar", LoadingProgressManager.LoadProgress.GameNetInit);
		m_gameInit = (GameInit)p_handle.returnValue;
		GLOBAL.GAME_SETTINGS = m_gameInit.gameSettings;
		if (null != m_titleScreenScript)
		{
			m_titleScreenScript.UpdateDisplayInfo(SingletonComponent<Services>.Instance.NubeeId);
		}
		PatchInfoDownload();
		if (SingletonComponent<TutorialManager>.Instance != null)
		{
			Dbg.Log("Load tutorial progresss");
			SingletonComponent<TutorialManager>.Instance.LoadProgress();
		}
		SingletonComponent<SocialManager>.Instance.SaveGiftSenders(m_gameInit.giftSenders);
		if (m_gameInit.characterAccountDetails.battleScore <= 0)
		{
			MBPrefs.SetInt("Ms:LastRank:" + m_nubeeId.ToString(), 0);
		}
		if (SingletonComponent<GameManager>.Instance != null)
		{
			SingletonComponent<GameManager>.Instance.StartStaminaTimer(m_gameInit.characterAccountDetails.timeTillNextStamina);
		}
		else
		{
			Dbg.LogWarn("Unable to find GameManager");
		}
		if (SingletonComponent<DailyManager>.Instance != null)
		{
			SingletonComponent<DailyManager>.Instance.m_BattleHelpData = m_gameInit.m_BattleHelpData;
			SingletonComponent<DailyManager>.Instance.m_promoCodeDatas = m_gameInit.m_promoCodeDatas;
			Dbg.Log("Battle help UserNum : " + SingletonComponent<DailyManager>.Instance.m_BattleHelpData.UserNum + " SocialPointsGained: " + SingletonComponent<DailyManager>.Instance.m_BattleHelpData.SocialPointsGained);
			if (SingletonComponent<DailyManager>.Instance.m_BattleHelpData.UserNum > 0 || SingletonComponent<DailyManager>.Instance.m_BattleHelpData.SocialPointsGained > 0)
			{
				SingletonComponent<DailyManager>.Instance.NeedToShowBattleHelp = true;
			}
			SingletonComponent<DailyManager>.Instance.NeedToShowBattleHelp = false;
		}
		Dbg.Log("---------> OnGameInitializationSuccess <---------");
		NBProfiler.EndTask("STARTUP_SEQUENCE");
	}

	private void OnGameInitializationFailed(Services.ServiceHandle p_handle, string p_error)
	{
		LogError("---------> OnGameInitializationFailed <--------- : [" + ServiceCallUtils.GetErrorMessage(p_handle));
		if (UIPopupController.Instance == null)
		{
			LogError("[OnGameInitializationFailed] UIPopupManager is null, unable to display messages");
			return;
		}
		if (p_handle == null)
		{
			UIPopupController.Instance.ShowServerGenericError(GameInitialization);
			return;
		}
		if (p_handle.IsNoInternet())
		{
			UIPopupController.Instance.ShowNoNetMessage(GameInitialization);
			return;
		}
		int nReturnCode = p_handle.nReturnCode;
		if (nReturnCode == 112)
		{
			Time.timeScale = 0f;
			UIPopupController.Instance.ShowOKPopupMSG("common_error", "error_force_update", GoToAppStore);
		}
		else
		{
			UIPopupController.Instance.ShowServerGenericError(GameInitialization, p_handle.nReturnCode);
		}
	}

	private void GoToAppStore()
	{
		Application.OpenURL(GLOBAL.APP_STORE_LINK);
		Time.timeScale = 1f;
		GameInitialization();
	}

	private void PatchInfoDownload()
	{
		NBProfiler.BeginTask("PATCH_INFO_DOWNLOAD");
		if (m_gameInit == null)
		{
			Dbg.LogError("The Game Initialization Sequence was not called.", Dbg.NBLogType.SYSTEM_INIT);
			return;
		}
		List<PatchFileInfo> files = m_gameInit.patchInfo.files;
		if (files.Count == 0)
		{
			m_patchDownloaded = true;
			Log(SingletonComponent<NBLocalization>.Instance.Get("common_loading_updatenotneeded"));
			StartCoroutine(InitializeManagers());
			return;
		}
		Log(SingletonComponent<NBLocalization>.Instance.Get("common_loading_updatedownloading"));
		if (!SingletonComponent<DLC>.Instance.CompareAndDownload(files, OnDownloadUpdateSuccess, OnDownloadUpdateFail))
		{
			m_patchDownloaded = true;
			Log(SingletonComponent<NBLocalization>.Instance.Get("common_loading_updatenotneeded"));
			StartCoroutine(InitializeManagers());
		}
		else
		{
			Dbg.Log("Waiting for Download DLC callbacks.", Dbg.NBLogType.SYSTEM_INIT);
		}
	}

	private void OnDownloadUpdateSuccess()
	{
		string p_text = SingletonComponent<NBLocalization>.Instance.Get("common_loading_updatedownloading") + " (" + SingletonComponent<DLC>.Instance.FilesDownloadedCount + "/" + SingletonComponent<DLC>.Instance.FilesToDownloadCount + ")";
		Log(p_text);
		if (SingletonComponent<DLC>.Instance.FilesDownloadedCount == SingletonComponent<DLC>.Instance.FilesToDownloadCount)
		{
			m_patchDownloaded = true;
			Messenger<LoadingProgressManager.LoadProgress>.Invoke("progress_bar", LoadingProgressManager.LoadProgress.DLCInit);
			Log(SingletonComponent<NBLocalization>.Instance.Get("common_loading_updatenotneeded"));
			StartCoroutine(InitializeManagers());
		}
	}

	private void OnDownloadUpdateFail()
	{
		UIPopupController.Instance.HideLoadingPopup();
		LogError("Failed Downloading. Try Again Later.");
		if (m_bStartLoading)
		{
			UIPopupController.Instance.ShowOKPopupMSG("common_error", "common_prompt_patchfail_retry", PatchInfoDownload);
		}
		else
		{
			PatchInfoDownload();
		}
	}

	private void InitInventory()
	{
		NBProfiler.BeginTask("INITIALIZE_INVENTORY");
		SingletonComponent<InventoryManager>.Instance.InitManager(m_gameInit.inventoryItems, OnInventoryCreated);
		StartCoroutine(ProceedToGame());
	}

	private void OnInventoryCreated(bool _success, string _error)
	{
		if (_success)
		{
			Messenger<LoadingProgressManager.LoadProgress>.Invoke("progress_bar", LoadingProgressManager.LoadProgress.InventoryCreate);
		}
		else
		{
			Dbg.LogError("OnInventoryCreated failed", Dbg.NBLogType.SYSTEM_INIT);
			UIPopupController.Instance.ShowServerGenericError(InitInventory);
		}
		NBProfiler.EndTask("INITIALIZE_INVENTORY");
	}

	private IEnumerator InitializeManagers()
	{
		if (!m_patchDownloaded || !m_bStartLoading)
		{
			m_bNubeePlatformConnected = true;
			yield break;
		}
		SingletonComponent<DLC>.Instance.UnloadAll();
		NBProfiler.EndTask("PATCH_INFO_DOWNLOAD");
		NBProfiler.BeginTask("INITIALIZE_MANAGERS_1");
		SingletonComponent<NBLocalization>.Instance.SetCurrentLanguage(SingletonComponent<SaveData>.Instance.GameData.Language);
		Messenger<LoadingProgressManager.LoadProgress>.Invoke("progress_bar", LoadingProgressManager.LoadProgress.LocalizationInit);
		Log(SingletonComponent<NBLocalization>.Instance.Get("common_loading_systeminit"));
		SingletonComponent<ParameterManager>.Instance.InitManager();
		Messenger<LoadingProgressManager.LoadProgress>.Invoke("progress_bar", LoadingProgressManager.LoadProgress.ParameterManager);
		yield return null;
		SingletonComponent<PlayerDataManager>.Instance.InitManager();
		Messenger<LoadingProgressManager.LoadProgress>.Invoke("progress_bar", LoadingProgressManager.LoadProgress.PlayerDataManager);
		yield return null;
		Messenger<LoadingProgressManager.LoadProgress>.Invoke("progress_bar", LoadingProgressManager.LoadProgress.PetManager);
		yield return null;
		SingletonComponent<LeaderboardManager>.Instance.InitManager(m_gameInit.characterAccountDetails);
		Messenger<LoadingProgressManager.LoadProgress>.Invoke("progress_bar", LoadingProgressManager.LoadProgress.LeaderBoardManagerInit);
		yield return null;
		SingletonComponent<TutorialManager>.Instance.InitManager();
		Messenger<LoadingProgressManager.LoadProgress>.Invoke("progress_bar", LoadingProgressManager.LoadProgress.TutorialManager);
		yield return null;
		SingletonComponent<DungeonManager>.Instance.InitManager(m_gameInit.unlockedCitiesJson, m_gameInit.dungeonEvents);
		SingletonComponent<DungeonManager>.Instance.MainEventID = m_gameInit.mainEventID;
		Messenger<LoadingProgressManager.LoadProgress>.Invoke("progress_bar", LoadingProgressManager.LoadProgress.DungeonManager);
		m_gameInit.unlockedCitiesJson = null;
		m_gameInit.dungeonEvents = null;
		m_gameInit.mainEventID = null;
		yield return null;
		SingletonComponent<CharacterElementManager>.Instance.InitManager(ItemVisualMgrCB);
		yield return null;
		SingletonComponent<ItemVisualManager>.Instance.InitManager(ItemVisualMgrCB);
		yield return null;
	}

	private IEnumerator InitializeManagers2()
	{
		NBProfiler.EndTask("INITIALIZE_MANAGERS_1");
		NBProfiler.BeginTask("INITIALIZE_MANAGERS_2");
		SingletonComponent<ItemManager>.Instance.InitManager();
		Messenger<LoadingProgressManager.LoadProgress>.Invoke("progress_bar", LoadingProgressManager.LoadProgress.ItemManagerInit);
		yield return null;
		Messenger<LoadingProgressManager.LoadProgress>.Invoke("progress_bar", LoadingProgressManager.LoadProgress.LoadTitleSounds);
		yield return null;
		SingletonComponent<SoundManager>.Instance.LoadPlayList("PlayList_Global");
		SingletonComponent<SoundManager>.Instance.LoadPlayList("PlayList_Menu");
		Messenger<LoadingProgressManager.LoadProgress>.Invoke("progress_bar", LoadingProgressManager.LoadProgress.LoadPlaylist);
		yield return null;
		SingletonComponent<AchievementManager>.Instance.InitManager(AchievementMgrCb);
		SingletonComponent<PushManager>.Instance.InitManager(OnPushManagerResult);
		yield return null;
		if (SingletonComponent<LoadingTipsManager>.Instance != null)
		{
			SingletonComponent<LoadingTipsManager>.Instance.Init();
			yield return null;
		}
		GameInitAfterManagerInit();
		yield return null;
	}

	private void AchievementMgrCb(bool p_success)
	{
		Messenger<LoadingProgressManager.LoadProgress>.Invoke("progress_bar", LoadingProgressManager.LoadProgress.AchievementManager);
	}

	private void ItemVisualMgrCB(bool p_succeed)
	{
		if (p_succeed)
		{
			Messenger<LoadingProgressManager.LoadProgress>.Invoke("progress_bar", LoadingProgressManager.LoadProgress.ItemVisualManager);
			m_itemMgrCounter++;
			if (m_itemMgrCounter == 2)
			{
				StartCoroutine(InitializeManagers2());
			}
		}
		else
		{
			UIPopupController.Instance.ShowServerGenericError(PatchInfoDownload);
		}
	}

	private void OnPushManagerResult(bool p_succeed)
	{
		Messenger<LoadingProgressManager.LoadProgress>.Invoke("progress_bar", LoadingProgressManager.LoadProgress.OnPushManager);
		Dbg.LogWarn("PushManager Register result: " + p_succeed);
	}

	private void SoundMgrCb(bool p_success)
	{
		if (p_success)
		{
			Messenger<LoadingProgressManager.LoadProgress>.Invoke("progress_bar", LoadingProgressManager.LoadProgress.SoundManager);
		}
		else
		{
			LogError("SoundManager had problems initializing. Please check integrity of audio files. This should not happen!");
		}
	}

	private void GameInitAfterManagerInit()
	{
		NBProfiler.EndTask("INITIALIZE_MANAGERS_2");
		NBProfiler.BeginTask("INITIALIZE_MANAGERS_3");
		SingletonComponent<PlayerDataManager>.Instance.SetPlayerData(m_gameInit.characterAccountDetails);
		Messenger<LoadingProgressManager.LoadProgress>.Invoke("progress_bar", LoadingProgressManager.LoadProgress.InitPlayerData);
		SingletonComponent<StoreManager>.Instance.InitManager(m_gameInit.serverStoreItems);
		Messenger<LoadingProgressManager.LoadProgress>.Invoke("progress_bar", LoadingProgressManager.LoadProgress.StoreManager);
		SingletonComponent<GachaManager>.Instance.InitGachaEvents(m_gameInit.gachaEvents);
		Messenger<LoadingProgressManager.LoadProgress>.Invoke("progress_bar", LoadingProgressManager.LoadProgress.GachaManager);
		SingletonComponent<AchievementManager>.Instance.InitManager(m_gameInit.achievements);
		Messenger<LoadingProgressManager.LoadProgress>.Invoke("progress_bar", LoadingProgressManager.LoadProgress.AchievementDataInit);
		SingletonComponent<AnnouncementManager>.Instance.GetAnnoucementsFromServer();
		Messenger<LoadingProgressManager.LoadProgress>.Invoke("progress_bar", LoadingProgressManager.LoadProgress.AchievementDataInit);
		NBProfiler.EndTask("INITIALIZE_MANAGERS_3");
		InitInventory();
	}

	private IEnumerator ProceedToGame()
	{
		SingletonComponent<TexturesManager>.Instance.Initialize();
		SingletonComponent<TexturesManager>.Instance.LoadAllMenuAtlas();
		Messenger<LoadingProgressManager.LoadProgress>.Invoke("progress_bar", LoadingProgressManager.LoadProgress.End);
		UIPopupController popup = UIPopupController.Instance;
		if (popup != null)
		{
			while (popup.GetProgressBarValue() < 1f)
			{
				yield return null;
			}
			popup.HideLoadingPopup();
		}
		NBProfiler.EndTask("SYSTEM_INIT");
		if (SingletonComponent<TutorialManager>.Instance != null)
		{
			if (!SingletonComponent<TutorialManager>.Instance.IsCompleted(Tutorial.State.Combat1))
			{
				Dbg.Log(" Go To Tutorial ");
				GoToTutorial();
				Object.DestroyObject(base.gameObject);
				yield break;
			}
			if (SingletonComponent<TutorialManager>.Instance.IsCompleted(Tutorial.State.Combat3) && !SingletonComponent<TutorialManager>.Instance.IsCompleted(Tutorial.State.AfterEnterName))
			{
				SingletonComponent<GameManager>.Instance.LoadScene("NameInput", true, false);
				Object.DestroyObject(base.gameObject);
				yield break;
			}
			if (!SingletonComponent<TutorialManager>.Instance.enabled)
			{
				CallDailyLoginBonus();
			}
		}
		SingletonComponent<StageManager>.Instance.CreatePlayerMainMenuTemplate(GoToMainMenu);
		if (SingletonComponent<NotifyManager>.Instance != null)
		{
			SingletonComponent<NotifyManager>.Instance.ClearLocalNotification();
			SingletonComponent<NotifyManager>.Instance.RegisterAllLocalNotifications();
		}
	}

	private void GoToScreen(string p_screen)
	{
		if (SingletonComponent<GameManager>.Instance != null)
		{
			SingletonComponent<GameManager>.Instance.LoadScene(p_screen, true, false);
		}
		Object.DestroyObject(base.gameObject);
	}

	private void GoToMainMenu()
	{
		GoToScreen("MainMenu");
	}

	private void OnPrepareBattlesSuccess2(Services.ServiceHandle p_handle)
	{
		bool flag = false;
		if (SingletonComponent<TutorialManager>.Instance != null && SingletonComponent<TutorialManager>.Instance.enabled)
		{
			if (SingletonComponent<TutorialManager>.Instance.IsCompleted(Tutorial.State.AllCombats))
			{
				flag = true;
			}
		}
		else
		{
			flag = true;
		}
		if (flag)
		{
			SaveData.MsData gameData = SingletonComponent<SaveData>.Instance.GameData;
			if (!gameData.HasEnteredAnEvent && SingletonComponent<TutorialManager>.Instance != null && SingletonComponent<TutorialManager>.Instance.IsCompleted(Tutorial.State.All))
			{
				gameData.HasEnteredAnEvent = true;
				gameData.SaveAchievementData();
			}
		}
		AnalyticsManager.FirstInstallEvents("tut_afterloading");
		GameObject gameObject = new GameObject();
		gameObject.AddComponent<PreloadBattle>();
	}

	private void OnPrepareBattlesFail2(Services.ServiceHandle p_handle, string p_error)
	{
		UIPopupController.Instance.HideLoadingPopup();
		Dbg.LogError(" Failed to start Battle : " + ServiceCallUtils.GetErrorMessage(p_handle));
		if (p_handle == null)
		{
			UIPopupController.Instance.ShowServerGenericError(GoToTutorial);
			return;
		}
		if (p_handle.IsNoInternet())
		{
			UIPopupController.Instance.ShowNoNetMessage(GoToTutorial);
			return;
		}
		switch ((ServiceResponseCodes)p_handle.nReturnCode)
		{
		case ServiceResponseCodes.EXCEPTION_DUNGEON_NOT_ENOUGH_STAMINA:
			UIPopupController.Instance.ShowOKPopupMSG("common_info", "dungeon_prompt_not_enough_stamina");
			if (null != MainMenu.Instance)
			{
				MainMenu.Instance.RefreshStamina();
			}
			break;
		case ServiceResponseCodes.EXCEPTION_DUNGEON_NOT_ENOUGH_SHIELD:
			UIPopupController.Instance.ShowInsufficientShield(GoToTutorial);
			break;
		default:
			UIPopupController.Instance.ShowServerGenericError(GoToTutorial, p_handle.nReturnCode);
			break;
		}
	}

	private void GoToTutorial()
	{
		SingletonComponent<TutorialManager>.Instance.enabled = true;
		SingletonComponent<DungeonManager>.Instance.StartDungeonEventOnline(OnPrepareBattlesSuccess2, OnPrepareBattlesFail2, 0, string.Empty);
	}

	private void CallDailyLoginBonus()
	{
		if (UIPopupController.LoginCounter == 0)
		{
			SingletonComponent<DailyManager>.Instance.DailyPromotion();
			SingletonComponent<DailyManager>.Instance.CheckLocalTimeFlagToShowDailyBonus();
			if (SingletonComponent<DailyManager>.Instance.NeedToShowDailyBonus)
			{
				SingletonComponent<DailyManager>.Instance.DailyBonus();
			}
		}
		UIPopupController.LoginCounter++;
	}
}
