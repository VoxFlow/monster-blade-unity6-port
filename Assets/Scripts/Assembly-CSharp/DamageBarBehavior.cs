using UnityEngine;

public class DamageBarBehavior : MonoBehaviour
{
	protected SliderSmoothExpand m_foregroundSlider;

	protected SliderSmoothExpand m_backgroundSlider;

	protected PlayerData m_watchingData;

	protected float m_prevValue;

	protected static float m_smallHealth = 0.0275f;

	public SliderSmoothExpand ForegroundSlider
	{
		set
		{
			m_foregroundSlider = value;
			if (m_foregroundSlider != null)
			{
				m_foregroundSlider.enabled = true;
			}
		}
	}

	public SliderSmoothExpand BackgroundSlider
	{
		set
		{
			m_backgroundSlider = value;
			if (m_backgroundSlider != null)
			{
				m_backgroundSlider.enabled = true;
			}
		}
	}

	public PlayerData WatchingData
	{
		get
		{
			return m_watchingData;
		}
		set
		{
			m_watchingData = value;
			if (value != null)
			{
				m_prevValue = m_watchingData.HpPercent;
				if (m_foregroundSlider != null)
				{
					SliderSmoothExpand foregroundSlider = m_foregroundSlider;
					float prevValue = m_prevValue;
					m_foregroundSlider.TargetSliderValue = prevValue;
					foregroundSlider.SliderValue = prevValue;
				}
				if (m_backgroundSlider != null)
				{
					SliderSmoothExpand backgroundSlider = m_backgroundSlider;
					float prevValue = m_prevValue;
					m_backgroundSlider.TargetSliderValue = prevValue;
					backgroundSlider.SliderValue = prevValue;
				}
			}
		}
	}

	protected virtual void Awake()
	{
	}

	protected virtual void OnDestroy()
	{
	}

	protected virtual void OnEnable()
	{
		if (m_foregroundSlider != null)
		{
			m_foregroundSlider.enabled = true;
		}
		if (m_backgroundSlider != null)
		{
			m_backgroundSlider.enabled = true;
		}
	}

	protected virtual void OnDisable()
	{
		if (m_foregroundSlider != null)
		{
			m_foregroundSlider.enabled = false;
		}
		if (m_backgroundSlider != null)
		{
			m_backgroundSlider.enabled = false;
		}
	}

	protected virtual void LateUpdate()
	{
		if (m_watchingData == null || m_foregroundSlider == null || m_backgroundSlider == null)
		{
			return;
		}
		float num = m_watchingData.HpPercent;
		if (num.CompareTo(m_smallHealth) < 0 && num > 0f)
		{
			num = m_smallHealth;
		}
		if (Mathf.Abs(m_prevValue - num) > SliderSmoothExpand.epsilon)
		{
			if (m_prevValue > m_watchingData.HpPercent)
			{
				SliderSmoothExpand backgroundSlider = m_backgroundSlider;
				float num2 = num;
				m_foregroundSlider.TargetSliderValue = num2;
				num2 = num2;
				m_foregroundSlider.SliderValue = num2;
				backgroundSlider.TargetSliderValue = num2;
			}
			else if (m_prevValue < m_watchingData.HpPercent)
			{
				SliderSmoothExpand foregroundSlider = m_foregroundSlider;
				float num2 = num;
				m_backgroundSlider.TargetSliderValue = num2;
				num2 = num2;
				m_backgroundSlider.SliderValue = num2;
				foregroundSlider.TargetSliderValue = num2;
			}
		}
		m_prevValue = num;
	}
}
