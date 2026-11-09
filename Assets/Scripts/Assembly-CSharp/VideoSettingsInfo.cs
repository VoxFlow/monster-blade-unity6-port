using UnityEngine;

public class VideoSettingsInfo : MonoBehaviour
{
	public bool m_enable = true;

	public int m_portraitScreenWidth = 320;

	public int m_portraitScreenHeight = 480;

	public bool m_fullScreenMode;

	public bool m_useBuiltInResolution;

	public bool m_useSmallSideAsReference;

	public bool m_showBlackBorders;

	public DesiredAspectRatio m_desiredAspectRatio = DesiredAspectRatio.SixteenToTen;

	private int m_startWidth;

	private int m_startHeight;

	private Camera m_cameraComponent;

	private Resolution[] m_resolutions;

	private int m_screenWidth;

	private int m_screenHeight;

	private float m_currentAspectRatio;

	private float m_expectedAspectRatio = 1.6f;

	private float m_defaultWidth;

	private float m_defaultHeight;

	private float m_windowAspectRatio;

	private float m_scaleHeight;

	public void Awake()
	{
		m_cameraComponent = base.gameObject.GetComponent<Camera>();
		m_screenWidth = m_portraitScreenWidth;
		m_screenHeight = m_portraitScreenHeight;
	}

	public void Start()
	{
		string empty = string.Empty;
		string text;
		if (!m_enable)
		{
			text = empty;
			empty = text + "Screen Resolution(WxH) : " + Screen.width + "x" + Screen.height + "\n";
			text = empty;
			empty = text + "Aspect Ratio : " + (float)Screen.width / ((float)Screen.height * 1f) + "\n";
			Dbg.Log(empty);
			return;
		}
		switch (m_desiredAspectRatio)
		{
		case DesiredAspectRatio.SixteenToTen:
			m_expectedAspectRatio = 1.6f;
			Dbg.Log("AspectRatio 16:10");
			break;
		case DesiredAspectRatio.TenToSixteen:
			m_expectedAspectRatio = 0.625f;
			Dbg.Log("AspectRatio 10:16");
			break;
		case DesiredAspectRatio.TwoToThree:
			m_expectedAspectRatio = 2f / 3f;
			Dbg.Log("AspectRatio 2:3");
			break;
		case DesiredAspectRatio.ThreeToTwo:
			m_expectedAspectRatio = 1.5f;
			Dbg.Log("AspectRatio 3:2");
			break;
		}
		if (m_useBuiltInResolution)
		{
			m_startWidth = Screen.width;
			m_startHeight = Screen.height;
			Dbg.Log("Use BuiltIn Screen Resolution");
		}
		else
		{
			m_startWidth = m_portraitScreenHeight;
			m_startHeight = m_portraitScreenWidth;
			Dbg.Log("Use Given Resolution");
		}
		m_windowAspectRatio = (float)m_startWidth / ((float)m_startHeight * 1f);
		text = empty;
		empty = text + "Screen Resolution(WxH) : " + m_startWidth + "x" + m_startHeight + "\n";
		text = empty;
		empty = text + "starting AspectRatio : " + m_windowAspectRatio + "\n";
		m_screenWidth = m_startWidth;
		if (m_screenWidth < m_startHeight)
		{
			m_screenWidth = m_startHeight;
		}
		if (m_useSmallSideAsReference)
		{
			m_screenHeight = m_startHeight;
			m_screenWidth = (int)((float)m_screenHeight * m_expectedAspectRatio);
		}
		else
		{
			m_screenHeight = (int)((float)m_screenWidth / m_expectedAspectRatio);
		}
		Screen.SetResolution(m_screenWidth, m_screenHeight, false);
		m_windowAspectRatio = (float)m_screenWidth / ((float)m_screenHeight * 1f);
		text = empty;
		empty = text + "NEW Screen Resolution(WxH) : " + m_screenWidth + "x" + m_screenHeight + "\n";
		text = empty;
		empty = text + "New AspectRatio : " + m_windowAspectRatio + "\n";
		if (m_showBlackBorders)
		{
			m_scaleHeight = (float)(m_startWidth / m_startHeight) / m_expectedAspectRatio;
			text = empty;
			empty = text + "ScaleHeight : " + m_scaleHeight + "\n";
			if (m_scaleHeight < 1f)
			{
				Rect rect = m_cameraComponent.rect;
				rect.width = 1f;
				rect.height = m_scaleHeight;
				rect.x = 0f;
				rect.y = (1f - m_scaleHeight) / 2f;
				m_cameraComponent.rect = rect;
			}
			else
			{
				float num = 1f / m_scaleHeight;
				Rect rect2 = m_cameraComponent.rect;
				rect2.width = num;
				rect2.height = 1f;
				rect2.x = (1f - num) / 2f;
				rect2.y = 0f;
				m_cameraComponent.rect = rect2;
			}
		}
		text = empty;
		empty = string.Concat(text, "Rect : ", m_cameraComponent.rect, "\n");
		Dbg.Log(empty);
	}
}
