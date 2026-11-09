using UnityEngine;

public class FaderGui : SingletonComponent<FaderGui>
{
	public Texture2D m_texture;

	public Color m_color = new Color(0f, 0f, 0f, 1f);

	public float m_fadeInTime = 1f;

	public float m_fadeOutTime = 1f;

	private Color m_cColor = new Color(0f, 0f, 0f, 1f);

	private float m_cFadeInRate = 0.3f;

	private float m_cFadeOutRate = 0.3f;

	public FaderModes m_mode;

	public float m_faderValue = 1f;

	public Texture2D m_textureOverlay;

	public float m_fadeInTimeOverlay = 2f;

	public float m_fadeOutTimeOverlay = 2f;

	private float m_animationValue;

	private OverlayAnimationStates m_animationState;

	private float m_lastTime;

	private float m_accumTime;

	public bool ignoreTimeScale { get; set; }

	public void Awake()
	{
		m_cColor = m_color;
		m_cFadeInRate = m_fadeInTime;
		m_cFadeOutRate = m_fadeOutTime;
		m_animationState = OverlayAnimationStates.None;
		ignoreTimeScale = false;
		base.enabled = false;
		// PORT (CD-062)
		if (MBStartupFade.CoverFirstScene())
		{
			m_faderValue = 1f;
			base.enabled = true;
		}
	}

	public void Update()
	{
		// PORT (CD-062)
		if (m_mode == FaderModes.FadeIn && MBStartupFade.Hold())
		{
			m_lastTime = UnityEngine.Time.realtimeSinceStartup;
			return;
		}
		float num = 0f;
		if (ignoreTimeScale)
		{
			float realtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
			float b = realtimeSinceStartup - m_lastTime;
			m_accumTime += Mathf.Max(0f, b);
			num = 0.001f * Mathf.Round(m_accumTime * 1000f);
			m_accumTime -= num;
			if (num > 1f)
			{
				num = 1f;
			}
			m_lastTime = UnityEngine.Time.realtimeSinceStartup;
		}
		else
		{
			num = Time.deltaTime;
		}
		if (m_mode == FaderModes.FadeIn)
		{
			m_faderValue -= num * m_cFadeInRate;
			if (m_faderValue <= 0f)
			{
				m_faderValue = 0f;
				m_mode = FaderModes.None;
				SendMessage("OnFadeInEnd", null, SendMessageOptions.DontRequireReceiver);
				base.enabled = false;
			}
		}
		else if (m_mode == FaderModes.FadeOut)
		{
			m_faderValue += num * m_cFadeOutRate;
			if (m_faderValue >= 1f)
			{
				m_faderValue = 1f;
				m_mode = FaderModes.None;
				SendMessage("OnFadeOutEnd", null, SendMessageOptions.DontRequireReceiver);
			}
		}
		if (m_animationState == OverlayAnimationStates.FadeIn)
		{
			m_animationValue += num * m_fadeInTimeOverlay;
			if (m_animationValue >= 1f)
			{
				m_animationValue = 1f;
				m_animationState = OverlayAnimationStates.FadeOut;
				SendMessage("OnOverlayFadeInEnd", null, SendMessageOptions.DontRequireReceiver);
			}
		}
		else if (m_animationState == OverlayAnimationStates.FadeOut)
		{
			m_animationValue -= num * m_fadeOutTimeOverlay;
			if (m_animationValue <= 0f)
			{
				m_animationValue = 0f;
				m_animationState = OverlayAnimationStates.FadeIn;
				SendMessage("OnOverlayFadeOutEnd", null, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	public void OnGUI()
	{
		if (base.enabled)
		{
			if ((bool)m_texture)
			{
				Color color = GUI.color;
				GUI.color = m_cColor;
				Color color2 = new Color(m_cColor.r, m_cColor.g, m_cColor.b, m_faderValue);
				GUI.color = color2;
				GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), m_texture);
				GUI.color = color;
			}
			if ((bool)m_textureOverlay && m_animationState != OverlayAnimationStates.None)
			{
				Color color3 = GUI.color;
				GUI.color = Color.white;
				Color color4 = new Color(Color.white.r, Color.white.g, Color.white.b, m_animationValue);
				GUI.color = color4;
				GUI.DrawTexture(new Rect(Screen.width - m_textureOverlay.width - 10, Screen.height - m_textureOverlay.height - 10, m_textureOverlay.width, m_textureOverlay.height), m_textureOverlay);
				GUI.color = color3;
			}
		}
	}

	public void ShowLoadOverlay()
	{
		m_animationState = OverlayAnimationStates.FadeIn;
		m_animationValue = 0f;
		SendMessage("OnFadeInBegin", null, SendMessageOptions.DontRequireReceiver);
	}

	public void HideLoadOverlay()
	{
		m_animationState = OverlayAnimationStates.None;
		SendMessage("OnFadeOutBegin", null, SendMessageOptions.DontRequireReceiver);
	}

	public void FadeIn()
	{
		FadeIn(m_color, m_fadeInTime);
	}

	public void FadeOut()
	{
		FadeOut(m_color, m_fadeOutTime);
	}

	public void FadeIn(Color p_color, float p_fadeInTime)
	{
		base.enabled = true;
		m_cColor = p_color;
		m_cFadeInRate = ((p_fadeInTime != 0f) ? (1f / p_fadeInTime) : 0f);
		m_faderValue = ((p_fadeInTime != 0f) ? m_faderValue : 0f);
		m_mode = FaderModes.FadeIn;
		SendMessage("OnFadeInBegin", null, SendMessageOptions.DontRequireReceiver);
	}

	public void FadeOut(Color p_color, float p_fadeOutTime)
	{
		base.enabled = true;
		m_cColor = p_color;
		m_cFadeOutRate = ((p_fadeOutTime != 0f) ? (1f / p_fadeOutTime) : 0f);
		m_faderValue = ((p_fadeOutTime != 0f) ? m_faderValue : 1f);
		m_mode = FaderModes.FadeOut;
		SendMessage("OnFadeOutBegin", null, SendMessageOptions.DontRequireReceiver);
	}
}
