using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class EntryPointScene : MonoBehaviour
{
	public SystemInitScene m_systemInitScript;

	public int m_secsReminderPlay = 604800;

	private void Awake()
	{
		GLOBAL.Init();
		if (m_systemInitScript == null)
		{
			Debug.LogError("No System Init script given. Cannot start the game.");
			base.enabled = false;
		}
	}

	private void Start()
	{
		NBProfiler.BeginTask("ENTRY_POINT_TO_MENU");
		if (HaveAllManagers())
		{
			StartCoroutine(DoInitialize());
		}
	}

	private bool HaveAllManagers()
	{
		NBProfiler.BeginTask("HAVE_ALL_MANAGERS");
		if (SingletonComponent<SaveData>.Instance == null)
		{
			Dbg.LogError("SaveData instance not ready.");
			return false;
		}
		if (SingletonComponent<NBLocalization>.Instance == null)
		{
			Dbg.LogError("NBLocalization not ready.");
			return false;
		}
		if (SingletonComponent<Services>.Instance == null)
		{
			Dbg.LogError("Services (Service Calls) instance not ready. Game Cannot start.");
			return false;
		}
		if (SingletonComponent<GameManager>.Instance == null)
		{
			Dbg.LogError("GameManager instance not ready. Game Cannot start.");
			return false;
		}
		if (SingletonComponent<ParameterManager>.Instance == null)
		{
			Dbg.LogError("ParameterManager instance not ready. Game Cannot start.");
			return false;
		}
		if (SingletonComponent<DLC>.Instance == null)
		{
			Dbg.LogError("DLC not ready. Game Cannot start.");
			return false;
		}
		if (SingletonComponent<SocialManager>.Instance == null)
		{
			Dbg.LogError("SocialManager not ready. Game Cannot start.");
			return false;
		}
		if (SingletonComponent<GachaManager>.Instance == null)
		{
			Dbg.LogError("GachaManager not ready. Game Cannot start.");
			return false;
		}
		if (SingletonComponent<DungeonManager>.Instance == null)
		{
			Dbg.LogError("DungeonManager instance not ready. Game Cannot start.");
			return false;
		}
		if (SingletonComponent<PlayerDataManager>.Instance == null)
		{
			Dbg.LogError("PlayerDataManager instance not ready. Game Cannot start.");
			return false;
		}
		if (SingletonComponent<ItemManager>.Instance == null)
		{
			Dbg.LogError("ItemManager instance not ready. Game Cannot start.");
			return false;
		}
		if (SingletonComponent<CameraConfiguration>.Instance == null)
		{
			Dbg.LogError("CameraConfiguration not ready. Game Cannot start.");
			return false;
		}
		if (SingletonComponent<StageManager>.Instance == null)
		{
			Dbg.LogError("StageManager not ready. Game Cannot start.");
			return false;
		}
		if (SingletonComponent<SubtitleManager>.Instance == null)
		{
			Dbg.LogError("SubtitleManager not ready. Game Cannot start.");
			return false;
		}
		if (SingletonComponent<SoundManager>.Instance == null)
		{
			Dbg.LogError("SoundManager instance not ready. Game Cannot start.");
			return false;
		}
		if (SingletonComponent<StoreManager>.Instance == null)
		{
			Dbg.LogError("StoreManager instance not ready. Game Cannot start.");
			return false;
		}
		if (SingletonComponent<PushManager>.Instance == null)
		{
			Dbg.LogError("PushManager instance not ready. Game Cannot start.");
			return false;
		}
		if (SingletonComponent<AnalyticsManager>.Instance == null)
		{
			Dbg.LogError("AnalyticsManager instance not ready. Game Cannot start.");
			return false;
		}
		if (SingletonComponent<AchievementManager>.Instance == null)
		{
			Dbg.LogError("AchievementManager instance not ready. Game Cannot start.");
			return false;
		}
		if (SingletonComponent<NotifyManager>.Instance == null)
		{
			Dbg.LogError("NotifyManager instance not ready. Game Cannot start.");
			return false;
		}
		NBProfiler.EndTask("HAVE_ALL_MANAGERS");
		return true;
	}

	public IEnumerator DoInitialize()
	{
		SingletonComponent<SaveData>.Instance.LoadDataFromPlayerPrefs();
		SingletonComponent<SaveData>.Instance.GameData.IsFirstTimeToRun = SingletonComponent<SaveData>.Instance.GameData.NubeeId == string.Empty;
		AnalyticsManager.FirstInstallEvents("tut_nubeelogo");
		Apsalar.NBSendEvent("launch_app");
		SingletonComponent<SaveData>.Instance.GameData.LastAppRun = TimeUtil.GetUnixEpochTime();
		yield return null;
		Exit();
	}

	private void LoadLanguageSettings()
	{
		SingletonComponent<NBLocalization>.Instance.InitializeLocalization();
		string language = SingletonComponent<SaveData>.Instance.GameData.Language;
		string preferredLanguage = NBUnity.GetPreferredLanguage();
		SingletonComponent<NBLocalization>.Instance.LoadPartialLanguage(preferredLanguage);
		SingletonComponent<SaveData>.Instance.GameData.Language = preferredLanguage;
		SingletonComponent<SaveData>.Instance.SaveDataToPlayerPrefs();
	}

	public void Exit()
	{
		LoadLanguageSettings();
		SingletonComponent<SaveData>.Instance.SaveDataToPlayerPrefs();
		CheckDeviceCompatibility();
	}

	private void ChangeUIAtlasReferenceByDeviceType()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			return;
		}
		string text = "Assets/Resources/UI/Atlas";
		string text2 = "Assets/Resources/UI/Atlas_Compress";
		string path = "Assets/Resources/UI/REF_ATLAS/";
		string path2 = ((GameManager.qualitySetting != GameManager.QualitySetting.Low) ? text : text2);
		List<UIAtlas> list = CollectAll<UIAtlas>(path);
		List<UIAtlas> list2 = CollectAll<UIAtlas>(path2);
		Dictionary<string, UIAtlas> dictionary = new Dictionary<string, UIAtlas>();
		foreach (UIAtlas item in list2)
		{
			if (!(item == null) && item.name != null && !dictionary.ContainsKey(item.name))
			{
				dictionary.Add(item.name, item);
			}
		}
		foreach (UIAtlas item2 in list)
		{
			if (!(item2.replacement == null) && dictionary.ContainsKey(item2.replacement.name))
			{
				item2.replacement = dictionary[item2.replacement.name];
			}
		}
	}

	public static List<T> CollectAll<T>(string path) where T : Object
	{
		List<T> list = new List<T>();
		List<string> list2 = new List<string>();
		GetFilesInDirectory(path, "*.*", list2);
		foreach (string item in list2)
		{
			if (!item.Contains(".meta") && !item.Contains(".svn") && !item.Contains(".db"))
			{
#if UNITY_EDITOR
				T val = (T)UnityEditor.AssetDatabase.LoadAssetAtPath(item, typeof(T));
#else
				T val = null;
#endif
				if (!(val == null))
				{
					list.Add(val);
				}
			}
		}
		return list;
	}

	private static void GetFilesInDirectory(string path, string pattern, List<string> fileNameCollection)
	{
		string[] files = Directory.GetFiles(path, pattern);
		string[] array = files;
		foreach (string item in array)
		{
			fileNameCollection.Add(item);
		}
		string[] directories = Directory.GetDirectories(path);
		string[] array2 = directories;
		foreach (string path2 in array2)
		{
			string[] files2 = Directory.GetFiles(path2, pattern);
			GetFilesInDirectory(path2, pattern, fileNameCollection);
		}
	}

	private void GoToTitleScreen()
	{
		m_systemInitScript.ShowTitleScreen();
	}

	private void CheckDeviceCompatibility()
	{
		if (!GameManager.IsDeviceSupported && UIPopupController.Instance != null)
		{
			UIPopupController.Instance.ShowOKPopupMSG("common_info", "device_not_supported", GoToTitleScreen);
		}
		else
		{
			GoToTitleScreen();
		}
	}
}
