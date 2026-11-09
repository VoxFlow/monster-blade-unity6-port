using UnityEngine;

public class SlowMotionSystem : SingletonComponent<SlowMotionSystem>
{
	private float m_period;

	private float m_lerpStep;

	private VoidCallback m_endCallback;

	private void Start()
	{
		base.enabled = false;
	}

	public void SlowTheTime(float p_scale, float p_period, VoidCallback p_endCallback = null)
	{
		Time.timeScale = 1f / p_scale;
		m_period = p_period;
		m_lerpStep = 0f;
		m_endCallback = p_endCallback;
		base.enabled = true;
	}

	public void LinearFade(float p_scale, float p_period, VoidCallback p_endCallback = null)
	{
		Time.timeScale = 1f / p_scale;
		m_period = p_period;
		m_lerpStep = (1f - Time.timeScale) / p_period;
		m_endCallback = p_endCallback;
		base.enabled = true;
	}

	public void Stop()
	{
		m_period = 0f;
		m_lerpStep = 0f;
		Time.timeScale = 1f;
		base.enabled = false;
		if (m_endCallback != null)
		{
			m_endCallback();
		}
	}

	private void Update()
	{
		m_period -= Time.deltaTime / Time.timeScale;
		Time.timeScale += Time.deltaTime * m_lerpStep;
		if (m_period <= 0f)
		{
			Stop();
		}
	}
}
