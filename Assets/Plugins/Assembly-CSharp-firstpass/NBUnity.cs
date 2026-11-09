using UnityEngine;

public class NBUnity
{
	public enum RemoteNotificationType
	{
		None = 0,
		Badge = 1,
		Sound = 2,
		Alert = 4
	}

	public static string GetPreferredLanguage()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		string text = "en";
		AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.nubee.nbunity.NBUnity");
		text = androidJavaClass.CallStatic<string>("_getPreferredLanguage", new object[0]);
		string text2 = IsoToExpanded(text);
		Debug.Log("-->DEVICE LANGUAGE : " + text + " Expanded : " + text2);
		return text2;
#else
		switch (Application.systemLanguage)
		{
		case SystemLanguage.Japanese:
			return IsoToExpanded("ja");
		case SystemLanguage.ChineseSimplified:
		case SystemLanguage.Chinese:
			return IsoToExpanded("zh-hans");
		case SystemLanguage.ChineseTraditional:
			return IsoToExpanded("zh-hant");
		case SystemLanguage.Korean:
			return IsoToExpanded("ko");
		default:
			return IsoToExpanded("en");
		}
#endif
	}

	public static string GetDeviceOsName()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		string text = "Unspecified";
		AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.nubee.nbunity.NBUnity");
		return androidJavaClass.CallStatic<string>("_getDeviceOsName", new object[0]);
#else
		return "Unspecified";
#endif
	}

	public static string GetDeviceOsVersion()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		Debug.Log("GetDevicesOsVersion");
		string text = "0.0.0";
		AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.nubee.nbunity.NBUnity");
		return androidJavaClass.CallStatic<string>("_getDeviceOsVersion", new object[0]);
#else
		return "0.0.0";
#endif
	}

	private static string IsoToExpanded(string p_shortLanguage)
	{
		p_shortLanguage = p_shortLanguage.ToLower();
		switch (p_shortLanguage)
		{
		case "ja":
		case "jpn":
			return "Japanese";
		case "zh-hans":
			return "Chinese_s";
		case "zh-hant":
		case "zho":
			return "Chinese_t";
		case "ko":
		case "kor":
			return "Korean";
		default:
			return "English";
		}
	}

	public static string ExpandedToIso2(string p_longLanguage)
	{
		Debug.Log("ExpandedToIso2. Language:" + p_longLanguage);
		p_longLanguage = p_longLanguage.ToLower();
		switch (p_longLanguage)
		{
		case "japanese":
			return "ja";
		case "chinese_s":
			return "zh-Hans";
		case "chinese_t":
			return "zh-Hant";
		case "korean":
			return "ko";
		default:
			return "en";
		}
	}

	private static string ExpandedToIso3(string p_shortLanguage)
	{
		p_shortLanguage = p_shortLanguage.ToLower();
		if (p_shortLanguage == "japanese")
		{
			return "jpn";
		}
		if (p_shortLanguage == "chinese" || p_shortLanguage.Contains("chinese"))
		{
			return "zho";
		}
		if (p_shortLanguage == "korean")
		{
			return "kor";
		}
		return "eng";
	}

	public static void RegisterForRemoteNotifications(RemoteNotificationType p_notifyTypes)
	{
	}
}
