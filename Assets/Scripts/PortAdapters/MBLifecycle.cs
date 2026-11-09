// Runs the game's pause save when the player quits, as Android pauses an app before closing it. Register CD-030.
using UnityEngine;

public static class MBLifecycle
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Boot()
	{
		Application.quitting += OnQuitting;
	}

	private static void OnQuitting()
	{
		SaveData save = Object.FindAnyObjectByType<SaveData>();
		if (save == null)
		{
			return;
		}
		save.SaveDataToPlayerPrefs();
		save.SaveGamePauseTimeStamp();
		MBPrefs.Save();
		Debug.Log("[MBLifecycle] quit: saved like Android's pause (SaveData.OnApplicationPause)");
	}
}
