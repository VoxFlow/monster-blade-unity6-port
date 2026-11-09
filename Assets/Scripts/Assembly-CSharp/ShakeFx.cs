using System;
using UnityEngine;

public class ShakeFx : MonoBehaviour
{
	[SerializeField]
	private Vector3 m_amplitude = Vector3.zero;

	private Vector3 m_currentAmplitude = Vector3.zero;

	private Vector3 m_variant = Vector3.zero;

	[SerializeField]
	private float m_frequency;

	[SerializeField]
	private float m_duration;

	[SerializeField]
	private bool m_smoothFade = true;

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

	private void Awake()
	{
		base.enabled = false;
		m_startPos = base.transform.localPosition;
	}

	private void RandomNextPos()
	{
		m_targetPos.x = UnityEngine.Random.Range(0f - m_currentAmplitude.x, m_currentAmplitude.x);
		m_targetPos.y = UnityEngine.Random.Range(0f - m_currentAmplitude.y, m_currentAmplitude.y);
		m_targetPos.z = UnityEngine.Random.Range(0f - m_currentAmplitude.z, m_currentAmplitude.z);
	}

	public bool IsFinished()
	{
		return m_time >= m_duration;
	}

	private void OnEnable()
	{
		Frequency = m_frequency;
	}

	private void OnDisable()
	{
		base.transform.localPosition = Vector3.zero;
		base.transform.localRotation = Quaternion.identity;
	}

	public void Play()
	{
		m_time = 0f;
		m_timeSinceLastRand = 0f;
		base.transform.localPosition = m_startPos;
		m_variant = m_amplitude / m_duration;
		m_currentAmplitude = m_amplitude;
		RandomNextPos();
		base.enabled = true;
	}

	protected void Update()
	{
		float deltaTime = Time.deltaTime;
		if (Time.timeScale.Equals(0f))
		{
			return;
		}
		deltaTime /= Time.timeScale;
		m_rad += m_angleStep * deltaTime;
		m_rad = Mathf.Min(m_rad, (float)Math.PI);
		Vector3 localPosition = base.transform.localPosition;
		float num = Mathf.Sin(m_rad);
		localPosition.x = m_startPos.x + num * m_targetPos.x;
		localPosition.y = m_startPos.y + num * m_targetPos.y;
		localPosition.z = m_startPos.z + num * m_targetPos.z;
		base.transform.localPosition = localPosition;
		m_time += deltaTime;
		m_timeSinceLastRand += deltaTime;
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
			base.enabled = false;
		}
	}
}
