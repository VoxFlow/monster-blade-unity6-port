using System.Collections;
using System.Collections.Generic;
using Models;
using UnityEngine;

public class DebugBattle : MonoBehaviour
{
	private ReturnToMonsterSelector returner;

	[SerializeField]
	private MonsterSelector m_monsterSelector;

	[SerializeField]
	private EntryPointScene m_entryPointScene;

	[SerializeField]
	private TestEnvironment m_testEnv;

	private GameInit m_gameInit;

	private int screenWidth = 960;

	private int screenHeight = 640;

	private void Awake()
	{
		screenWidth = Screen.width;
		screenHeight = Screen.height;
		GameObject gameObject = GameObject.Find("ReturnToMonsterSelector");
		if (gameObject != null)
		{
			returner = gameObject.GetComponent<ReturnToMonsterSelector>();
		}
	}

	private void OnGameInitializationSuccess(Services.ServiceHandle p_handle)
	{
		Messenger<LoadingProgressManager.LoadProgress>.Invoke("progress_bar", LoadingProgressManager.LoadProgress.GameNetInit);
		m_gameInit = (GameInit)p_handle.returnValue;
		GLOBAL.GAME_SETTINGS = m_gameInit.gameSettings;
		PatchInfoDownload();
	}

	private void OnGameInitializationFailed(Services.ServiceHandle p_handle, string p_error)
	{
	}

	private void PatchInfoDownload()
	{
		if (m_gameInit == null)
		{
			Dbg.LogError("The Game Initialization Sequence was not called.", Dbg.NBLogType.SYSTEM_INIT);
			return;
		}
		List<PatchFileInfo> files = m_gameInit.patchInfo.files;
		if (files.Count == 0)
		{
			StartCoroutine(InitializeManagers());
		}
		else if (!SingletonComponent<DLC>.Instance.CompareAndDownload(files, OnDownloadUpdateSuccess, OnDownloadUpdateFail))
		{
			StartCoroutine(InitializeManagers());
		}
	}

	private void OnDownloadUpdateSuccess()
	{
	}

	private void OnDownloadUpdateFail()
	{
	}

	private IEnumerator InitializeManagers()
	{
		SingletonComponent<DLC>.Instance.UnloadAll();
		SingletonComponent<NBLocalization>.Instance.SetCurrentLanguage(SingletonComponent<SaveData>.Instance.GameData.Language);
		SingletonComponent<ParameterManager>.Instance.InitManager();
		yield return null;
	}

	private void OnGUI()
	{
		if (!GUI.Button(new Rect((float)screenWidth - (float)screenWidth * 0.1f, (float)screenHeight - (float)screenHeight * 0.1f, (float)screenWidth * 0.1f, (float)screenHeight * 0.1f), "Start Battle") || !(m_monsterSelector != null))
		{
			return;
		}
		m_monsterSelector.gameObject.SetActiveRecursively(false);
		MBPrefs.SetFloat("Ms:PlayerRetryHp", 0f);
		MBPrefs.SetFloat("Ms:ItemProgress", 0f);
		MBPrefs.SetString("Ms:Rankings", string.Empty);
		MBPrefs.SetString("Ms:BattleScores", string.Empty);
		SingletonComponent<TutorialManager>.Instance.enabled = false;
		SingletonComponent<TutorialManager>.Instance.currentState = Tutorial.State.None;
		SingletonComponent<ParameterManager>.Instance.InitManager();
		Singleton<MonsterFactory>.Instance.m_debugCurrentMonsterPrefabPath = m_monsterSelector.currentMonsterPrefabPath;
		string text = "NatureLevel_Prefab_2";
		if (m_testEnv != null)
		{
			GameObject[] levels = m_testEnv.m_levels;
			foreach (GameObject gameObject in levels)
			{
				if (gameObject.active)
				{
					text = gameObject.name;
					break;
				}
			}
		}
		Debug.Log("Try to load env: " + text);
		Singleton<MonsterFactory>.Instance.m_debugLevel = text;
		Dbg.s_writeToLogFile = true;
		m_entryPointScene.Exit();
		base.enabled = false;
	}

	private void OnDestroy()
	{
		if (returner != null)
		{
			returner.enabled = true;
			returner.gameObject.SetActiveRecursively(true);
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
