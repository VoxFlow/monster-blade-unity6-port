// The switch that routes the game's service calls to the in-process offline server. Register CD-005.
using UnityEngine;

public static class OfflineServerMode
{
	public static bool Enabled = true;

	private const string NubeeIdKey = "OfflineServer.NubeeId";

	private const string DeviceIdKey = "OfflineServer.DeviceId";

	public static string NubeeId
	{
		get { return Persisted(NubeeIdKey, "1"); }
	}

	public static string DeviceId
	{
		get { return Persisted(DeviceIdKey, SystemInfo.deviceUniqueIdentifier); }
	}

	public static void ConnectNubeePlatform()
	{
		NubeePlatformManager manager = Object.FindAnyObjectByType<NubeePlatformManager>();
		if (manager == null)
		{
			Debug.LogError("[OfflineServer] NubeePlatformManager not found; cannot complete the platform connection.");
			return;
		}
		manager.Invoke("ConnectionSuccess", 0f);
	}

	private static string Persisted(string key, string initial)
	{
		string value = MBPrefs.GetString(key, string.Empty);
		if (value == string.Empty)
		{
			value = initial;
			MBPrefs.SetString(key, value);
			MBPrefs.Save();
		}
		return value;
	}
}
