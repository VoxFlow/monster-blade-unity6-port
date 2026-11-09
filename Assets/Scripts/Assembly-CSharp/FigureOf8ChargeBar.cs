using UnityEngine;

public class FigureOf8ChargeBar : MonoBehaviour
{
	public UISlider[] m_chargeBars;

	public UISprite[] m_superTexts;

	public float m_blinkTime = 0.06f;

	private NBTimer m_blinkTimer;

	private int m_barBlinkIndex;

	private int m_textBlinkIndex;

	private float m_oldSliderValue;

	public float SliderValue
	{
		set
		{
			if (m_chargeBars != null)
			{
				m_oldSliderValue = m_chargeBars[0].sliderValue;
				UISlider[] chargeBars = m_chargeBars;
				foreach (UISlider uISlider in chargeBars)
				{
					uISlider.sliderValue = Mathf.Min(value, 1f);
				}
			}
		}
	}

	private void OnBlinkTimeEnded()
	{
		m_barBlinkIndex++;
		m_barBlinkIndex %= m_chargeBars.Length;
		ShowBar(m_barBlinkIndex);
		m_textBlinkIndex++;
		m_textBlinkIndex %= m_superTexts.Length;
		ShowText(m_textBlinkIndex);
		m_blinkTimer.CountDown(m_blinkTime, "OnBlinkTimeEnded");
	}

	private void ShowBar(int p_index)
	{
		for (int i = 0; i < m_chargeBars.Length; i++)
		{
			if (i == p_index)
			{
				m_chargeBars[i].gameObject.SetActiveRecursively(true);
			}
			else
			{
				m_chargeBars[i].gameObject.SetActiveRecursively(false);
			}
		}
	}

	private void ShowText(int p_index)
	{
		for (int i = 0; i < m_superTexts.Length; i++)
		{
			if (i == p_index)
			{
				m_superTexts[i].gameObject.SetActiveRecursively(true);
			}
			else
			{
				m_superTexts[i].gameObject.SetActiveRecursively(false);
			}
		}
	}

	private void Awake()
	{
		m_blinkTimer = base.gameObject.AddComponent<NBTimer>();
	}

	private void Start()
	{
		if (m_chargeBars == null || m_superTexts == null)
		{
			base.enabled = false;
		}
	}

	private void Update()
	{
		if (m_oldSliderValue < 1f && m_chargeBars[0].sliderValue == 1f)
		{
			m_blinkTimer.CountDown(m_blinkTime, "OnBlinkTimeEnded");
			ShowText(0);
		}
		else if (m_oldSliderValue >= 1f && m_chargeBars[0].sliderValue < 1f)
		{
			m_blinkTimer.Reset();
			ShowBar(0);
			ShowText(-1);
		}
	}
}
