// Keeps a loading screen in clear view (not under the black transition fade) for at least 4.5 seconds, so its tip can be read
// (quality fix for fast loading). Register CD-063.
using UnityEngine;

public static class MBLoadingHold
{
	public const float MinSeconds = 4.5f;

	private static bool s_up;

	private static float s_visible;

	private static float s_last = -1f;

	private static bool s_held;

	private static float s_shownAt;

	private const float MaxHoldSeconds = 15f;

	public static void Shown()
	{
		if (s_up) return;
		s_up = true;
		s_shownAt = Time.realtimeSinceStartup;
		s_visible = 0f;
		s_last = -1f;
	}

	public static void Hidden()
	{
		s_up = false;
		s_visible = 0f;
		s_last = -1f;
	}

	public static void Tick()
	{
		float now = Time.realtimeSinceStartup;
		if (s_up && Clear())
		{
			if (s_last >= 0f) s_visible += now - s_last;
			s_last = now;
		}
		else
		{
			s_last = -1f;
		}
	}

	private static bool Clear()
	{
		FaderGui f = SingletonComponent<FaderGui>.Instance;
		return f == null || !f.enabled || f.m_faderValue <= 0.01f;
	}

	private static bool Done()
	{
		return !s_up || s_visible >= MinSeconds || Time.realtimeSinceStartup - s_shownAt >= MaxHoldSeconds;
	}

	public static bool MayCover()
	{
		return Release(Done());
	}

	public static bool MayActivate()
	{
		return Release(Done() || !Clear());
	}

	private static bool Release(bool may)
	{
		if (!may) s_held = true;
		else if (s_held)
		{
			s_held = false;
			Debug.Log("[LoadingHold] released after the loading screen was in clear view " + (s_visible * 1000f).ToString("F0") + " ms");
		}
		return may;
	}
}
