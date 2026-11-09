// Holds the title fade-in until the offline server's start-up reply (quality fix). Register CD-062.
using UnityEngine;

public static class MBStartupFade
{
	private static bool s_done;

	private static int s_releaseFrame = -1;

	private static float s_start = -1f;

	private static bool s_covered;

	public static bool CoverFirstScene()
	{
		if (s_covered) return false;
		s_covered = true;
		return true;
	}

	public static void StartupAnswered()
	{
		if (s_releaseFrame < 0) s_releaseFrame = Time.frameCount + 2;
	}

	public static bool Hold()
	{
		if (s_done) return false;
		if (s_start < 0f) s_start = Time.realtimeSinceStartup;
		bool ready = s_releaseFrame >= 0 && Time.frameCount >= s_releaseFrame;
		if (ready || Time.realtimeSinceStartup - s_start > 3f)
		{
			s_done = true;
			Debug.Log("[StartupFade] title fade-in released after " + ((Time.realtimeSinceStartup - s_start) * 1000f).ToString("F0") + " ms"
				+ (ready ? "" : " (3 s cap)"));
			return false;
		}
		return true;
	}
}
