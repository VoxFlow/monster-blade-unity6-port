// One seam for the game's touch and mouse reads, so input delivered as touch reaches the game as on a touch screen. Register
// CD-025.
using UnityEngine;

public static class MBInput
{
	private static readonly Touch[] s_none = new Touch[0];

	private static Touch[] s_touches = s_none;

	public static bool TouchPlatform;

	public static bool Injecting
	{
		get { return s_touches.Length > 0; }
	}

	public static void SetTouches(Touch[] touches)
	{
		s_touches = touches ?? s_none;
	}

	public static int touchCount
	{
		get { return TouchPlatform ? s_touches.Length : Input.touchCount; }
	}

	public static Touch[] touches
	{
		get { return TouchPlatform ? s_touches : Input.touches; }
	}

	public static Touch GetTouch(int index)
	{
		return TouchPlatform ? s_touches[index] : Input.GetTouch(index);
	}

	public static bool GetMouseButton(int button)
	{
		if (!TouchPlatform)
		{
			return Input.GetMouseButton(button);
		}
		return button == 0 && s_touches.Length > 0;
	}

	public static bool GetMouseButtonDown(int button)
	{
		if (!TouchPlatform)
		{
			return Input.GetMouseButtonDown(button);
		}
		return button == 0 && s_touches.Length > 0 && s_touches[0].phase == TouchPhase.Began;
	}

	public static bool GetMouseButtonUp(int button)
	{
		if (!TouchPlatform)
		{
			return Input.GetMouseButtonUp(button);
		}
		return button == 0 && s_touches.Length > 0 && (s_touches[0].phase == TouchPhase.Ended || s_touches[0].phase == TouchPhase.Canceled);
	}

	public static Vector3 mousePosition
	{
		get
		{
			if (!TouchPlatform)
			{
				return Input.mousePosition;
			}
			return s_touches.Length > 0 ? (Vector3)s_touches[0].position : Vector3.zero;
		}
	}

	public static bool anyKey
	{
		get { return Input.anyKey || (TouchPlatform && s_touches.Length > 0); }
	}

	public static bool anyKeyDown
	{
		get { return Input.anyKeyDown || (TouchPlatform && s_touches.Length > 0 && s_touches[0].phase == TouchPhase.Began); }
	}
}
