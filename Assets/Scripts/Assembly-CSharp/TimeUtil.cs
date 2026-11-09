using System;
using System.Globalization;

public class TimeUtil
{
	public static int GetCurrentYear()
	{
		return DateTime.UtcNow.Year;
	}

	public static int GetCurrentMonth()
	{
		return DateTime.UtcNow.Month;
	}

	public static int GetCurrentDay()
	{
		return DateTime.UtcNow.Day;
	}

	public static DayOfWeek GetCurrentDayOfWeek()
	{
		return DateTime.UtcNow.DayOfWeek;
	}

	public static long GetUnixEpochTime()
	{
		return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
	}

	public static DateTime GetFromUnixEpochTime(long p_unixEpochTime)
	{
		TimeSpan value = TimeSpan.FromSeconds(p_unixEpochTime);
		return new DateTime(1970, 1, 1).Add(value);
	}

	public static int GetWeekNumber(DateTime p_value)
	{
		return CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(p_value, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
	}

	public static int GetWeekNumber(long p_unixEpochTime)
	{
		return GetWeekNumber(GetFromUnixEpochTime(p_unixEpochTime));
	}

	public static DayOfWeek GetDayOfTheWeek(DateTime p_value)
	{
		return CultureInfo.CurrentCulture.Calendar.GetDayOfWeek(p_value);
	}

	public static DayOfWeek GetDayOfTheWeek(long p_unixEpochTime)
	{
		return GetDayOfTheWeek(GetFromUnixEpochTime(p_unixEpochTime));
	}
}
