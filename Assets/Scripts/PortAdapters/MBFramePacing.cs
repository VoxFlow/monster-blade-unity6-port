// Caps the frame rate at the original hardware's 60 frames per second and synchronises to displays at multiples of 60 Hz.
// Register CD-049.
using System;
using UnityEngine;

public static class MBFramePacing
{
	public const int OriginalDisplayHz = 60;

	public static int Cap(int requested)
	{
		if (Application.platform == RuntimePlatform.Android) return requested;
		SyncToDisplay();
		return requested <= 0 ? OriginalDisplayHz : Mathf.Min(requested, OriginalDisplayHz);
	}

	private static void SyncToDisplay()
	{
		double hz = Screen.currentResolution.refreshRateRatio.value;
		int k = (int)Math.Round(hz / OriginalDisplayHz);
		bool multiple = k >= 1 && k <= 4 && Math.Abs(hz - k * OriginalDisplayHz) < 1.0;
		int want = multiple ? k : 0;
		if (QualitySettings.vSyncCount != want)
		{
			QualitySettings.vSyncCount = want;
			Debug.Log("[FramePacing] display " + hz.ToString("F2") + " Hz: vSyncCount " + want + (multiple ? " (even 60 fps)" : " (not a multiple of 60 Hz: frames cannot be even at 60 fps)"));
		}
	}
}
