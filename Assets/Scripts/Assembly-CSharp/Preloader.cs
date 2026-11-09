using System.Collections;
using UnityEngine;

public class Preloader : MonoBehaviour
{
	private enum LoadStates
	{
		InitManagers = 0,
		Patching = 1,
		Finished = 2,
		Error = 3
	}

	public bool m_usePatchServer = true;

	public string m_nextScene = string.Empty;

	private void Awake()
	{
		GLOBAL.Init();
	}

	private IEnumerator InitializeManagers()
	{
		bool pass = true;
		if (SingletonComponent<Services>.Instance == null)
		{
			Debug.LogError("Services (Service Calls) instance not ready.");
			pass = false;
		}
		yield return null;
		if (SingletonComponent<GameManager>.Instance == null)
		{
			Debug.LogError("GameManager instance not ready.");
			pass = false;
		}
		yield return null;
		if (SingletonComponent<ParameterManager>.Instance == null)
		{
			Debug.LogError("ParameterManager instance not ready.");
			pass = false;
		}
		yield return null;
		if (SingletonComponent<SaveData>.Instance == null)
		{
			Debug.LogError("SaveData instance not ready.");
			pass = false;
		}
		yield return null;
		if (SingletonComponent<SoundManager>.Instance == null)
		{
			Debug.LogError("SoundManager instance not ready.");
			pass = false;
		}
		yield return null;
		if (SingletonComponent<DungeonManager>.Instance == null)
		{
			Debug.LogError("DungeonManager instance not ready.");
			pass = false;
		}
		yield return null;
		if (SingletonComponent<PlayerDataManager>.Instance == null)
		{
			Debug.LogError("PlayerDataManager instance not ready.");
			pass = false;
		}
		yield return null;
		if (SingletonComponent<ItemManager>.Instance == null)
		{
			Debug.LogError("ItemManager instance not ready.");
			pass = false;
		}
		yield return null;
		if (SingletonComponent<StageManager>.Instance == null)
		{
			Debug.LogError("StageManager not ready.");
			pass = false;
		}
		yield return null;
		if (SingletonComponent<SubtitleManager>.Instance == null)
		{
			Debug.LogError("SubtitleManager not ready.");
			pass = false;
		}
		yield return null;
		if (SingletonComponent<DLC>.Instance == null)
		{
			Debug.LogError("DownloadController not ready.");
			pass = false;
		}
		if (!pass)
		{
			Debug.LogError("Initialization process not Completed!.");
			yield break;
		}
		Debug.Log("Loading new scene ->>>>>>>>");
		SingletonComponent<GameManager>.Instance.LoadScene(m_nextScene, true, false);
	}
}
