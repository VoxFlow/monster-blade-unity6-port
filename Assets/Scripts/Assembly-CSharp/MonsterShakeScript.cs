using UnityEngine;

public class MonsterShakeScript : MonoBehaviour
{
	public float m_amplitude = 0.025f;

	public float m_duration = 0.2f;

	public float m_frequency = 4f;

	private float m_period;

	private float m_time;

	private float m_angularVec;

	private Quaternion m_defaultRot = Quaternion.identity;

	public void Start()
	{
		base.enabled = false;
	}

	public void Shake()
	{
		if (base.enabled)
		{
			Stop();
		}
		m_defaultRot = base.transform.rotation;
		if (m_frequency != 0f)
		{
			m_period = 1f / m_frequency;
		}
		m_defaultRot = base.transform.rotation;
		m_angularVec = 360f / m_period;
		m_time = 0f;
		base.enabled = true;
	}

	public void Stop()
	{
		base.transform.rotation = m_defaultRot;
		base.enabled = false;
	}

	public void Pause(bool p_pause)
	{
		if (IsShaking())
		{
			base.enabled = !p_pause;
		}
	}

	public bool IsShaking()
	{
		return m_defaultRot != base.transform.rotation;
	}

	private void Update()
	{
		m_time += Time.deltaTime;
		float angle = 0f;
		if (m_time < m_duration)
		{
			angle = Mathf.Sin(m_angularVec * m_time) * m_amplitude;
		}
		else
		{
			base.enabled = false;
		}
		base.transform.rotation = m_defaultRot;
		base.transform.RotateAroundLocal(Vector3.up, angle);
	}
}
