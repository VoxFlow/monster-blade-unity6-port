// Screen position of user-interface widgets, for the PC input. Register CD-025.
using UnityEngine;

public static class MBScreen
{
	public static bool Pos(GameObject go, out Vector2 pos, out string why)
	{
		pos = Vector2.zero;
		why = null;
		if (go == null || !go.activeInHierarchy)
		{
			why = "inactive";
			return false;
		}
		Camera cam = CameraFor(go);
		if (cam == null)
		{
			why = "no UICamera for layer " + go.layer;
			return false;
		}
		Collider c = go.GetComponent<Collider>();
		return ToScreen(cam, c != null ? c.bounds.center : go.transform.position, out pos, out why);
	}

	public static bool ToScreen(Camera cam, Vector3 world, out Vector2 pos, out string why)
	{
		Vector3 p = cam.WorldToScreenPoint(world);
		pos = new Vector2(p.x, p.y);
		bool on = (cam.orthographic || p.z >= 0f) && p.x >= 0f && p.y >= 0f && p.x <= Screen.width && p.y <= Screen.height;
		why = on ? null : "off screen " + p + " via " + cam.name;
		return on;
	}

	public static Camera CameraFor(GameObject go)
	{
		Camera cam = go.GetComponentInParent<Camera>();
		if (cam != null && cam.enabled)
		{
			return cam;
		}
		foreach (UICamera uc in Object.FindObjectsByType<UICamera>(FindObjectsSortMode.None))
		{
			Camera c0 = uc.GetComponent<Camera>();
			if (uc.enabled && c0 != null && c0.enabled && (c0.cullingMask & (1 << go.layer)) != 0)
			{
				return c0;
			}
		}
		return null;
	}
}
