using UnityEngine;

[RequireComponent(typeof(UISlider))]
public class SliderSmoothExpand : MonoBehaviour
{
	protected UISlider m_slider;

	protected float m_timeDelayToMove = 0.3f;

	protected float m_nextTime;

	protected float m_maxExpandRate = 0.4f;

	protected float m_expandRate;

	protected float m_expandAccel;

	protected float m_defaultExpandAccel = 0.2f;

	protected float m_targetSliderValue = 1f;

	protected float m_sign;

	protected bool m_expanding;

	public static readonly float epsilon = 1E-06f;

	public float ExpandAccel
	{
		set
		{
			m_expandAccel = value;
		}
	}

	public float SliderValue
	{
		get
		{
			if (m_slider == null)
			{
				return 0f;
			}
			return m_slider.sliderValue;
		}
		set
		{
			if (m_slider != null)
			{
				m_slider.sliderValue = value;
			}
		}
	}

	public float TargetSliderValue
	{
		get
		{
			return m_targetSliderValue;
		}
		set
		{
			m_targetSliderValue = value;
			BeginUpdate();
		}
	}

	public float TimeDelayToMove
	{
		set
		{
			m_timeDelayToMove = value;
		}
	}

	protected virtual void Awake()
	{
		m_slider = base.gameObject.GetComponent<UISlider>();
	}

	protected void BeginUpdate()
	{
		float num = m_targetSliderValue - m_slider.sliderValue;
		m_sign = ((!(num > 0f)) ? (-1f) : 1f);
	}

	protected virtual void Update()
	{
		if (m_sign == 0f)
		{
			return;
		}
		if (!m_expanding)
		{
			m_nextTime += Time.deltaTime;
			if (m_nextTime >= m_timeDelayToMove)
			{
				m_nextTime = 0f;
				m_expandRate = 0f;
				if (m_expandAccel.Equals(0f))
				{
					m_expandRate = m_maxExpandRate;
				}
				m_expanding = true;
			}
			return;
		}
		float num = m_expandRate * m_sign * Time.deltaTime;
		bool flag = m_sign > 0f;
		SliderValue += num;
		if (!m_expandAccel.Equals(0f))
		{
			m_expandRate += m_sign * m_expandAccel * Time.deltaTime;
			m_expandRate = Mathf.Max(m_expandRate, 0f - m_maxExpandRate);
			m_expandRate = Mathf.Min(m_expandRate, m_maxExpandRate);
		}
		if ((flag && SliderValue >= m_targetSliderValue) || (!flag && SliderValue <= m_targetSliderValue))
		{
			m_slider.sliderValue = m_targetSliderValue;
			m_expanding = false;
			m_nextTime = 0f;
			m_expandRate = 0f;
			m_sign = 0f;
		}
	}
}
