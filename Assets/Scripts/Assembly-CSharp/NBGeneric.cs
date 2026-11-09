using System;
using System.Reflection;

public static class NBGeneric
{
	public static void Swap<T>(ref T p_lhs, ref T p_rhs)
	{
		T val = p_lhs;
		p_lhs = p_rhs;
		p_rhs = val;
	}

	public static bool TryParse<T>(string p_value, out int p_outResult)
	{
		p_outResult = 0;
		if (string.IsNullOrEmpty(p_value))
		{
			return false;
		}
		return int.TryParse(p_value, out p_outResult);
	}

	public static bool TryParse<T>(string p_value, out float p_outResult)
	{
		p_outResult = 0f;
		if (string.IsNullOrEmpty(p_value))
		{
			return false;
		}
		return float.TryParse(p_value, out p_outResult);
	}

	public static bool TryParse<T>(string p_value, out long p_outResult)
	{
		p_outResult = 0L;
		if (string.IsNullOrEmpty(p_value))
		{
			return false;
		}
		return long.TryParse(p_value, out p_outResult);
	}

	public static bool TryParse<T>(string p_value, out T p_outResult)
	{
		p_outResult = default(T);
		object[] array = new object[2] { p_value, p_outResult };
		Type[] types = new Type[2]
		{
			typeof(string),
			typeof(T).MakeByRefType()
		};
		MethodInfo method = typeof(T).GetMethod("TryParse", BindingFlags.Static | BindingFlags.Public, null, types, null);
		if (method != null)
		{
			if (!(bool)method.Invoke(null, array))
			{
				Dbg.LogWarn("Parser return an error. Cannot parse: " + p_value);
				return false;
			}
			p_outResult = (T)array[1];
			return true;
		}
		Dbg.LogWarn("No suitable parser function implemented.");
		return false;
	}

	public static bool ContainsNonAsciiChar(string p_string)
	{
		foreach (char c in p_string)
		{
			int num = c;
			if (num > 127)
			{
				return true;
			}
		}
		return false;
	}

	public static string ConvertTimeToDDHHMM(long p_time)
	{
		if (p_time < 0)
		{
			return string.Empty;
		}
		TimeSpan timeSpan = TimeSpan.FromSeconds(p_time);
		string text = "d";
		string text2 = "h";
		string text3 = "m";
		NBLocalization instance = SingletonComponent<NBLocalization>.Instance;
		if (null != instance)
		{
			text = instance.Get("leaderboard_timer_day");
			text2 = instance.Get("leaderboard_timer_hour");
			text3 = instance.Get("leaderboard_timer_minute");
		}
		string text4 = "{0}{1}:{2}{3}:{4}{5}";
		if (timeSpan.Days <= 0)
		{
			text4 = text4.Substring(text4.IndexOf(":") + 1);
		}
		return string.Format(text4, timeSpan.Days, text, timeSpan.Hours, text2, timeSpan.Minutes, text3);
	}
}
