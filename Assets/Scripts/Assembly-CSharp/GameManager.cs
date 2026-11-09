using System;
using UnityEngine;

public class GameManager : SingletonComponent<GameManager>
{
	private enum LoadSceneStates
	{
		Nothing = 0,
		NewSceneRequest = 1,
		FadingIn = 2,
		Loading = 3,
		Finished = 4,
		Max = 5
	}

	public enum QualitySetting
	{
		High = 0,
		Mid = 1,
		Low = 2
	}

	private bool m_paused;

	private bool m_quit;

	private string m_currentScene = string.Empty;

	private string m_sceneLoading = string.Empty;

	private LoadSceneStates m_sceneLoaderState;

	private AsyncOperation m_asyncOp;

	private bool m_async = true;

	private bool m_additive;

	private bool m_restartingApp;

	private NBTimer m_staminaTimer;

	private PlayerData m_playerData;

	public static QualitySetting qualitySetting { get; set; }

	public static bool autoDetectQualitySetting { get; set; }

	public static bool IsDeviceSupported { get; private set; }

	public string CurrentScene
	{
		get
		{
			return m_currentScene;
		}
	}

	private void Awake()
	{
		IsDeviceSupported = true;
		autoDetectQualitySetting = false;
		m_staminaTimer = GetComponent<NBTimer>();
	}

	private void SuggestTargetFrameRate(int p_frameRate)
	{
		Application.targetFrameRate = MBFramePacing.Cap(p_frameRate);   // PORT (CD-049)
	}

	private void Quit()
	{
		if (!m_quit)
		{
			m_quit = true;
			Application.Quit();
		}
	}

	private bool IsQuit()
	{
		return m_quit;
	}

	public void Pause()
	{
		if (!m_paused)
		{
			Dbg.Log("Game is paused.");
			m_paused = true;
			Time.timeScale = 0f;
		}
	}

	public void Resume()
	{
		if (m_paused)
		{
			if (null != SingletonComponent<ItemVisualManager>.Instance && null != MainMenu.Instance)
			{
				SingletonComponent<ItemVisualManager>.Instance.ReloadAssetBundles();
			}
			Dbg.Log("Game is unpaused.");
			m_paused = false;
			Time.timeScale = 1f;
		}
	}

	private bool IsPaused()
	{
		return m_paused;
	}

	public bool IsLoadingScene()
	{
		return m_sceneLoaderState != LoadSceneStates.Nothing;
	}

	private void Update()
	{
		MBLoadingHold.Tick();   // PORT (CD-063)
		if (m_sceneLoaderState == LoadSceneStates.Nothing)
		{
			return;
		}
		if (m_sceneLoaderState == LoadSceneStates.NewSceneRequest)
		{
			// PORT (CD-063)
			if (!MBLoadingHold.MayCover())
			{
				return;
			}
			SendMessage("OnLoadSceneBegin", m_sceneLoading, SendMessageOptions.DontRequireReceiver);
			m_sceneLoaderState = LoadSceneStates.FadingIn;
		}
		if (m_sceneLoaderState == LoadSceneStates.FadingIn)
		{
		}
		if (m_sceneLoaderState == LoadSceneStates.Loading)
		{
			if (m_async && m_asyncOp != null)
			{
				// PORT (CD-063)
				if (!m_asyncOp.allowSceneActivation && MBLoadingHold.MayActivate())
				{
					m_asyncOp.allowSceneActivation = true;
				}
				if (m_asyncOp.isDone)
				{
					m_sceneLoaderState = LoadSceneStates.Finished;
				}
			}
			else
			{
				m_sceneLoaderState = LoadSceneStates.Finished;
			}
		}
		if (m_sceneLoaderState == LoadSceneStates.Finished)
		{
			if (m_sceneLoading == "MainMenu" || m_sceneLoading == "NameInput")
			{
				UIPopupController.Instance.HideFullScreenLoading();
			}
			Dbg.Log("Scene Load Complete: " + m_sceneLoading);
			m_currentScene = m_sceneLoading;
			m_sceneLoading = string.Empty;
			m_sceneLoaderState = LoadSceneStates.Nothing;
			if (!(m_currentScene == "BattleView"))
			{
				SendMessage("OnLoadSceneEnd", m_currentScene, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	public void LoadScene(string p_levelname)
	{
		LoadScene(p_levelname, true, false);
	}

	public bool LoadScene(string p_levelname, bool p_async, bool p_additive)
	{
		Dbg.Log("Try loading scene: " + p_levelname);
		if (m_sceneLoaderState != LoadSceneStates.Nothing)
		{
			Dbg.LogWarn("Is already loading this scene : " + m_sceneLoading);
			return false;
		}
		m_sceneLoaderState = LoadSceneStates.NewSceneRequest;
		m_sceneLoading = p_levelname;
		m_asyncOp = null;
		m_async = p_async;
		m_additive = p_additive;
		return true;
	}

	public float GetSceneLoadingProgress()
	{
		if (m_asyncOp != null)
		{
			return m_asyncOp.progress;
		}
		return 0f;
	}

	public void OnFadeOutEnd()
	{
		if (!(m_sceneLoading == string.Empty))
		{
			Resources.UnloadUnusedAssets();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			DestroyPopupWhenRestart();
			OnCleanUpEnd();
		}
	}

	public void OnCleanUpEnd()
	{
		UIPopupController.Instance.ShowFullScreenLoading();
		if (m_async && m_additive)
		{
			m_asyncOp = Application.LoadLevelAdditiveAsync(m_sceneLoading);
		}
		else if (!m_async && m_additive)
		{
			Application.LoadLevelAdditive(m_sceneLoading);
		}
		else if (m_async && !m_additive)
		{
			m_asyncOp = Application.LoadLevelAsync(m_sceneLoading);
		}
		else
		{
			Application.LoadLevel(m_sceneLoading);
		}
		if (m_asyncOp != null && !MBLoadingHold.MayActivate())
		{
			m_asyncOp.allowSceneActivation = false;   // PORT (CD-063)
		}
		m_sceneLoaderState = LoadSceneStates.Loading;
	}

	public NBTimer GetStaminaTimer()
	{
		return m_staminaTimer;
	}

	public void StopStaminaTimer()
	{
		if (m_staminaTimer == null)
		{
			Dbg.LogWarn("Unable to get stamina timer in GameManager");
		}
		else
		{
			m_staminaTimer.Stop();
		}
	}

	public void StartStaminaTimer()
	{
		if (GLOBAL.GAME_SETTINGS == null || SingletonComponent<PlayerDataManager>.Instance == null)
		{
			Dbg.LogWarn("game settings or player data manager is null");
			return;
		}
		m_playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		if (m_playerData == null)
		{
			Dbg.LogWarn("PlayerData is null");
			return;
		}
		float num = m_playerData.m_userData.timeNextStamina;
		if (m_staminaTimer == null)
		{
			Dbg.LogWarn("Unable to get stamina timer in GameManager");
			return;
		}
		if (num > 0f)
		{
			m_staminaTimer.CountDown(num, "StaminaTick");
		}
		else
		{
			m_staminaTimer.Stop();
		}
		Dbg.Log("GameManager.StartStaminaTimer called, timer : " + num);
	}

	public void StartStaminaTimer(float p_timer)
	{
		if (m_staminaTimer == null)
		{
			Dbg.LogWarn("Unable to get stamina timer in GameManager");
			return;
		}
		if (p_timer > 0f)
		{
			m_staminaTimer.CountDown(p_timer, "StaminaTick");
		}
		else
		{
			m_staminaTimer.Stop();
		}
		Dbg.Log("GameManager.StartStaminaTimer called, timer : " + p_timer);
	}

	private void StaminaTick()
	{
		if (m_staminaTimer == null)
		{
			Dbg.LogWarn("Unable to get stamina timer in GameManager");
			return;
		}
		if (m_playerData == null)
		{
			m_playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		}
		if (MainMenu.Instance != null && m_playerData != null && m_playerData.m_userData != null && m_playerData.m_userData.stamina < m_playerData.m_userData.maxStamina)
		{
			MainMenu.Instance.IncreaseStaminaValue(1);
			MainMenu.Instance.Refresh();
		}
		m_staminaTimer.CountDown(GLOBAL.GAME_SETTINGS.stamina_reset_time, "StaminaTick");
	}

	private void ApplicationDidReceiveMemoryWarning()
	{
		Resources.UnloadUnusedAssets();
		GC.Collect();
		SingletonComponent<SoundManager>.Instance.RequestUnloadResources();
	}

	private void DestroyPopupWhenRestart()
	{
		if (m_restartingApp)
		{
			GameObject gameObject = ((!(UIPopupController.Instance != null)) ? null : UIPopupController.Instance.gameObject);
			if (null != gameObject)
			{
				UnityEngine.Object.Destroy(gameObject);
				gameObject = null;
			}
		}
	}

	private void OnLevelWasLoaded()
	{
		if (m_restartingApp)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			m_restartingApp = false;
		}
	}

	public void applicationWillEnterForeground()
	{
		Dbg.Log("applicationWillEnterForeground  BattleScreenUI.disableInputsAfterFB : " + BattleScreenUI.disableInputsAfterFB);
		if (BattleScreenUI.disableInputsAfterFB)
		{
			UIPopupController.SetEnableLayer("PopUpLayer", true);
			BattleScreenUI.disableInputsAfterFB = false;
		}
		UIPopupController.PrintCameraAndLayers();
	}

	public void RestartApplication()
	{
		if (m_restartingApp)
		{
			Dbg.Log("The application is already restarting.");
			return;
		}
		m_restartingApp = true;
		SingletonComponent<DLC>.Instance.UnloadAll();
		GameObject[] array = GameObject.FindGameObjectsWithTag("DontDestroy");
		if (array != null)
		{
			GameObject gameObject = ((!(UIPopupController.Instance != null)) ? null : UIPopupController.Instance.gameObject);
			for (int i = 0; i < array.Length; i++)
			{
				GameObject gameObject2 = array[i];
				if (null != gameObject2 && base.gameObject != gameObject2 && gameObject != gameObject2)
				{
					UnityEngine.Object.Destroy(gameObject2);
					gameObject2 = null;
				}
			}
		}
		LoadScene("Startup", true, false);
	}
}
