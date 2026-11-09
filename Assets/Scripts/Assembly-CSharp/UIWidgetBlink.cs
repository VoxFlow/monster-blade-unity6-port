using UnityEngine;

[RequireComponent(typeof(UIWidget))]
public class UIWidgetBlink : MonoBehaviour
{
	public float m_rate = 1.5f;

	public bool m_useDefaultColors = true;

	public Color m_from;

	public Color m_to;

	private UIWidget m_widget;

	private Color m_originalColor;

	private TweenColor m_tween;

	private float m_duration;

	private float m_onDuration;

	private float m_offDuration;

	private float m_time;

	private void OnDisable()
	{
		Reset();
	}

	private void OnEnable()
	{
		Init();
	}

	private void Reset()
	{
		if ((bool)m_tween)
		{
			m_tween.enabled = false;
		}
		if ((bool)m_widget)
		{
			m_widget.color = m_originalColor;
		}
	}

	private void Awake()
	{
		m_widget = GetComponent<UIWidget>();
		m_originalColor = m_widget.color;
	}

	private void Init()
	{
		if (m_widget.enabled)
		{
			m_duration = 1f / m_rate;
			Color color;
			Color color2;
			if (m_useDefaultColors)
			{
				color = m_originalColor;
				color2 = color;
				color2.a = 0f;
			}
			else
			{
				color = m_from;
				color2 = m_to;
			}
			m_tween = TweenColor.Begin(base.gameObject, m_duration, color2);
			m_tween.from = color;
			m_tween.method = UITweener.Method.EaseIn;
			m_tween.style = UITweener.Style.PingPong;
		}
	}

	private void Start()
	{
		Init();
	}
}
