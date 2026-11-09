using UnityEngine;

public class CameraZoomFx : CameraFx
{
	private float m_step;

	private float m_startFov;

	public float m_targetFov;

	private float m_time;

	public float TargetFov
	{
		get
		{
			return m_targetFov;
		}
		set
		{
			m_targetFov = value;
		}
	}

	public float Period
	{
		set
		{
			m_step = 1f / value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_type = 4;
	}

	public override bool IsFinished()
	{
		return m_time >= 1f;
	}

	protected override void OnInterrupted()
	{
		base.gameObject.GetComponent<Camera>().fieldOfView = m_startFov;
	}

	protected override void OnPlay()
	{
		m_startFov = base.gameObject.GetComponent<Camera>().fieldOfView;
		m_time = 0f;
	}

	protected override void OnUpdate(float p_deltaTime)
	{
		m_time += m_step * p_deltaTime;
		m_time = Mathf.Min(m_time, 1f);
		base.gameObject.GetComponent<Camera>().fieldOfView = Mathf.SmoothStep(m_startFov, m_targetFov, m_time);
	}
}
