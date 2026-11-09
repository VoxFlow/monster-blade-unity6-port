using System;
using System.IO;
using System.Text;
using UnityEngine;

[ExecuteInEditMode]
public class Dbg : SingletonComponent<Dbg>
{
	public enum NBLogType
	{
		COMMON_LOG = 0,
		COMMON_WARNING = 1,
		COMMON_ERROR = 2,
		SYSTEM_INIT = 3,
		MEMORY_INFO = 4,
		INVENTORY = 5,
		DLC = 6,
		SERVICES = 7,
		SOUND = 8,
		PROFILER = 9,
		TEXTURES_MANAGER = 10,
		STORE = 11,
		UI_POPUP = 12,
		UI_SETTINGS = 13,
		CHARACTER_GENERATOR = 14,
		CHARACTER_INFO_PANEL = 15,
		GACHA = 16,
		PET = 17,
		SNS = 18,
		SOCIAL_FRIEND_SELECT = 19,
		SOCIAL_FRIEND_LIST = 20,
		SOCIAL_FRIEND_REQUEST = 21,
		SOCIAL_FRIEND_GIFT = 22,
		TUTORIAL = 23,
		MAX = 24
	}

	public static bool[] s_enableTypes = new bool[24];


	public bool m_previousEnable = true;

	public bool m_enableLogging = true;

	public bool[] m_enableTypes = new bool[24];

	public static bool s_saveProfilerFile = false;

	public static bool s_writeToLogFile = false;

	private static StreamWriter profilerSW = null;

	private static StreamWriter logFileSW = null;

	private static string s_logLocation = string.Empty;

	private static string s_allLogsPattern = "AllLogs-";

	private static string s_profilerPattern = "Profiler";

	private static int s_maxLogCount = 5;

	private void Awake()
	{
		s_logLocation = UnityEngine.Application.temporaryCachePath + "/";   // PORT (CD-007)
		s_enableTypes = new bool[24];
		for (int i = 0; i < 24; i++)
		{
			s_enableTypes[i] = m_enableTypes[i];
		}
	}

	private void DeleteOldLogs()
	{
		string[] files = Directory.GetFiles(s_logLocation, s_allLogsPattern + "*");
		string[] files2 = Directory.GetFiles(s_logLocation, s_profilerPattern + "*");
		MBSort.Sort(files);
		MBSort.Sort(files2);
		StringBuilder stringBuilder = new StringBuilder();
		if (files.Length >= s_maxLogCount)
		{
			for (int i = 0; i < files.Length - s_maxLogCount; i++)
			{
				string text = files[i];
				stringBuilder.AppendLine(text);
				File.Delete(text);
			}
			for (int j = 0; j < files2.Length - s_maxLogCount; j++)
			{
				string text2 = files2[j];
				stringBuilder.AppendLine(text2);
				File.Delete(text2);
			}
		}
		Debug.Log("Logs Deleted :\n" + stringBuilder.ToString());
	}

	public void EnableAll()
	{
		for (int i = 0; i < 24; i++)
		{
			m_enableTypes[i] &= m_enableLogging;
			if (!m_previousEnable && m_enableLogging)
			{
				m_enableTypes[i] = m_enableLogging;
			}
		}
		m_previousEnable = m_enableLogging;
	}

	public static void LogEx(string p_string, NBLogType _type = NBLogType.COMMON_LOG, params string[] p_otherStrings)
	{
	}

	public static void Log(string p_string, NBLogType _type = NBLogType.COMMON_LOG)
	{
	}

	public static void LogWarnEx(string p_string, NBLogType _type = NBLogType.COMMON_WARNING, params string[] p_otherStrings)
	{
	}

	public static void LogWarn(string p_string, NBLogType _type = NBLogType.COMMON_WARNING)
	{
	}

	public static void LogErrorEx(string p_string, NBLogType _type = NBLogType.COMMON_ERROR, params string[] p_otherStrings)
	{
	}

	public static void LogError(string p_string, NBLogType _type = NBLogType.COMMON_ERROR)
	{
	}

	public static bool Assert(bool condition, string message = "Missing reference.")
	{
		return !condition;
	}

	private new void OnDestroy()
	{
		if (profilerSW != null)
		{
			profilerSW.Close();
			profilerSW = null;
		}
		if (logFileSW != null)
		{
			logFileSW.Close();
			logFileSW = null;
		}
	}
}
