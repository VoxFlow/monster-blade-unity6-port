using System;
using UnityEngine;

public class CameraRecoilFx : CameraFx
{
	private float m_angularVel;

	private float m_rad;

	private float m_startFov;

	public float m_amplitude;

	public float Amplitude
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

	public float Period
	{
		set
		{
			m_angularVel = (float)Math.PI / value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_type = 4;
	}

	public override bool IsFinished()
	{
		return m_rad >= (float)Math.PI;
	}

	protected override void OnInterrupted()
	{
		base.gameObject.GetComponent<Camera>().fieldOfView = m_startFov;
	}

	protected override void OnPlay()
	{
		m_rad = 0f;
		m_startFov = base.gameObject.GetComponent<Camera>().fieldOfView;
	}

	protected override void OnUpdate(float p_deltaTime)
	{
		m_rad += m_angularVel * p_deltaTime;
		m_rad = Mathf.Min(m_rad, (float)Math.PI);
		base.gameObject.GetComponent<Camera>().fieldOfView = m_startFov + Mathf.Sin(m_rad) * m_amplitude;
	}
}
