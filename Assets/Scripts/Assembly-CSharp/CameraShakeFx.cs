using System;
using UnityEngine;

public class CameraShakeFx : CameraFx
{
	public Vector3 m_amplitude = Vector3.zero;

	private Vector3 m_currentAmplitude = Vector3.zero;

	private Vector3 m_variant = Vector3.zero;

	public float m_frequency;

	public float m_duration;

	public bool m_smoothFade = true;

	private float m_rad;

	private float m_angleStep;

	private float m_time;

	private float m_timeSinceLastRand;

	private Vector3 m_startPos = Vector3.zero;

	private Vector3 m_targetPos = Vector3.zero;

	public Vector3 Amplitude
	{
		get
		{
			return m_amplitude;
		}
		set
		{
			m_amplitude = value;
		}
	}

	public float Duration
	{
		get
		{
			return m_duration;
		}
		set
		{
			m_duration = value;
		}
	}

	public float Frequency
	{
		get
		{
			return m_frequency;
		}
		set
		{
			m_frequency = value;
			m_angleStep = m_frequency * (float)Math.PI;
		}
	}

	public bool SmoothFade
	{
		get
		{
			return m_smoothFade;
		}
		set
		{
			m_smoothFade = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_type = 2;
	}

	private void RandomNextPos()
	{
		m_targetPos.x = UnityEngine.Random.Range(0f - m_currentAmplitude.x, m_currentAmplitude.x);
		m_targetPos.y = UnityEngine.Random.Range(0f - m_currentAmplitude.y, m_currentAmplitude.y);
		m_targetPos.z = UnityEngine.Random.Range(0f - m_currentAmplitude.z, m_currentAmplitude.z);
	}

	public override bool IsFinished()
	{
		return m_time >= m_duration;
	}

	private void OnEnable()
	{
		Frequency = m_frequency;
	}

	protected override void OnInterrupted()
	{
		base.transform.localPosition = m_startPos;
	}

	protected override void OnPlay()
	{
		m_time = 0f;
		m_timeSinceLastRand = 0f;
		m_startPos = base.transform.localPosition;
		m_variant = m_amplitude / m_duration;
		m_currentAmplitude = m_amplitude;
		RandomNextPos();
	}

	protected override void OnUpdate(float p_deltaTime)
	{
		m_rad += m_angleStep * p_deltaTime;
		m_rad = Mathf.Min(m_rad, (float)Math.PI);
		Vector3 localPosition = base.transform.localPosition;
		float num = Mathf.Sin(m_rad);
		localPosition.x = m_startPos.x + num * m_targetPos.x;
		localPosition.y = m_startPos.y + num * m_targetPos.y;
		localPosition.z = m_startPos.z + num * m_targetPos.z;
		base.transform.localPosition = localPosition;
		m_time += p_deltaTime;
		m_timeSinceLastRand += p_deltaTime;
		if (m_rad >= (float)Math.PI)
		{
			if (m_smoothFade)
			{
				m_currentAmplitude -= m_variant * m_timeSinceLastRand;
			}
			base.transform.localPosition = m_startPos;
			m_rad = 0f;
			m_timeSinceLastRand = 0f;
			RandomNextPos();
		}
		if (IsFinished())
		{
			base.transform.localPosition = m_startPos;
		}
	}
}
