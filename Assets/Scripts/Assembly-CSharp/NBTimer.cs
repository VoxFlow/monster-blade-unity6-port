using System;
using UnityEngine;

public class NBTimer : MonoBehaviour
{
	private string m_endCallback;

	private Action m_endDelegate;

	[SerializeField]
	private float m_counter;

	private float m_period;

	private bool m_isRealtime = true;

	public bool m_ignoreTimeScale;

	private float m_lastTime;

	private float m_accumTime;

	public string Name
	{
		get
		{
			return string.Empty;
		}
		set
		{
		}
	}

	public float Counter
	{
		get
		{
			return m_counter;
		}
	}

	public float Period
	{
		get
		{
			return m_period;
		}
	}

	public float Ratio
	{
		get
		{
			float result = 0f;
			if (!m_period.Equals(0f))
			{
				result = m_counter / m_period;
			}
			return result;
		}
	}

	public bool IsRealtime
	{
		get
		{
			return m_isRealtime;
		}
		set
		{
			m_isRealtime = value;
		}
	}

	public void CountDown(float p_limit)
	{
		CountDown(p_limit, string.Empty);
	}

	public void CountDown(float p_limit, string p_endCallback)
	{
		OnCountDown(p_limit);
		m_endCallback = p_endCallback;
		m_endDelegate = null;
		base.enabled = true;
	}

	public void CountDown(float p_limit, Action p_endCallback)
	{
		OnCountDown(p_limit);
		m_endCallback = null;
		m_endDelegate = p_endCallback;
		base.enabled = true;
	}

	private void OnCountDown(float p_limit)
	{
		p_limit = Mathf.Max(p_limit, 0f);
		m_period = (m_counter = p_limit);
		base.enabled = true;
	}

	public void Reset()
	{
		m_counter = 0f;
		base.enabled = false;
	}

	public void ResetCounterTo(float p_newValue)
	{
		m_counter = Mathf.Max(p_newValue, 0f);
	}

	private void Start()
	{
		m_lastTime = UnityEngine.Time.realtimeSinceStartup;
	}

	private void OnEnable()
	{
		m_lastTime = UnityEngine.Time.realtimeSinceStartup;
	}

	public void Stop()
	{
		m_counter = 0f;
		base.enabled = false;
		if (m_endCallback != null && m_endCallback != string.Empty)
		{
			SendMessage(m_endCallback);
		}
		else if (m_endDelegate != null)
		{
			m_endDelegate();
		}
	}

	public void Pause(bool p_pause)
	{
		if (m_counter != 0f)
		{
			base.enabled = !p_pause;
		}
	}

	private void LateUpdate()
	{
		float num = 0f;
		if (m_ignoreTimeScale)
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
			if (m_isRealtime && !Time.timeScale.Equals(0f))
			{
				num /= Time.timeScale;
			}
		}
		m_counter -= num;
		if (m_counter <= 0f)
		{
			Stop();
		}
	}
}
