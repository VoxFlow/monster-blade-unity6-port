using System;
using UnityEngine;

public class MenuBackground3D : MonoBehaviour
{
	public string[] m_backgrounds;

	public Camera m_camera;

	private Background3D m_currBackground;

	private Element.Type m_currBackgroundType;

	private static MenuBackground3D s_instance;

	public static MenuBackground3D Instance
	{
		get
		{
			return s_instance;
		}
	}

	public Background3D CurrentBackground
	{
		get
		{
			return m_currBackground;
		}
	}

	private void OnDestroy()
	{
		s_instance = null;
		if (null != m_currBackground)
		{
			UnityEngine.Object.DestroyImmediate(m_currBackground);
		}
	}

	private void Awake()
	{
		if (s_instance == null)
		{
			s_instance = this;
		}
		if (Instance == null)
		{
			Debug.LogError("No Menu Background 3D found.");
		}
	}

	public void ShowDefaultBackGround(bool scrolling = false)
	{
		Dbg.Log("[Info] ShowDefaultBackGround.");
		Element.Type type = Element.Type.Earth;
		ShowBackground(type, scrolling);
		m_camera.gameObject.SetActiveRecursively(true);
	}

	public void ShowLastVisitedDungeonBg(bool scrolling)
	{
		string text = MBPrefs.GetString("Ms:LastVisitDungeon");
		Dbg.Log("[Info] The element of Last visited dungeon: " + text);
		Element.Type type = Element.Type.Fire;
		if (!string.IsNullOrEmpty(text))
		{
			type = (Element.Type)(int)Enum.Parse(typeof(Element.Type), text);
		}
		ShowBackground(type, scrolling);
	}

	public void ShowBackground(Element.Type type, bool scrolling = true)
	{
		if (type != m_currBackgroundType && m_currBackground != null)
		{
			UnityEngine.Object.DestroyImmediate(m_currBackground.gameObject);
			m_currBackground = null;
		}
		if (m_currBackground == null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(Resources.Load(m_backgrounds[(int)type])) as GameObject;
			m_currBackground = gameObject.GetComponent<Background3D>();
		}
		if ((bool)m_currBackground)
		{
			m_currBackgroundType = type;
			m_camera.gameObject.active = true;
			m_currBackground.EnableRolling(scrolling);
			m_currBackground.gameObject.SetActiveRecursively(true);
		}
	}

	public void HideBackground()
	{
		if (m_camera != null)
		{
			m_camera.gameObject.active = false;
		}
		if (m_currBackground != null)
		{
			m_currBackground.gameObject.SetActiveRecursively(false);
		}
	}
}
