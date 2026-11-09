using UnityEngine;

public class CameraTiltFx : CameraFx
{
	private enum Phase
	{
		RiseUp = 0,
		Follow = 1,
		Return = 2
	}

	private Quaternion m_originalRotation = Quaternion.identity;

	private Quaternion m_startRotation = Quaternion.identity;

	private Quaternion m_targetRotation = Quaternion.identity;

	public Vector3 m_riseUpAngle = Vector3.zero;

	public Vector3 m_followAngle = Vector3.zero;

	public float m_riseUpStep;

	public float m_followStep;

	public float m_returnStep;

	private float m_time;

	private Phase m_phase;

	public Vector3 RiseUpAngle
	{
		get
		{
			return m_riseUpAngle;
		}
		set
		{
			m_riseUpAngle = value;
		}
	}

	public Vector3 FollowUpAngle
	{
		get
		{
			return m_followAngle;
		}
		set
		{
			m_followAngle = value;
		}
	}

	public float RiseUpPeriod
	{
		get
		{
			float result = 0f;
			if (m_riseUpStep != 0f)
			{
				result = 1f / m_riseUpStep;
			}
			return result;
		}
		set
		{
			m_riseUpStep = 1f / value;
		}
	}

	public float FollowUpPeriod
	{
		get
		{
			float result = 0f;
			if (m_followStep != 0f)
			{
				result = 1f / m_followStep;
			}
			return result;
		}
		set
		{
			m_followStep = 1f / value;
		}
	}

	public float ReturnPeriod
	{
		get
		{
			float result = 0f;
			if (m_returnStep != 0f)
			{
				result = 1f / m_returnStep;
			}
			return result;
		}
		set
		{
			m_returnStep = 1f / value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_type = 1;
	}

	public override bool IsFinished()
	{
		return m_phase == Phase.Return && m_time >= 1f;
	}

	protected override void OnUpdate(float p_deltaTime)
	{
		if (m_phase == Phase.RiseUp)
		{
			m_time += m_riseUpStep * p_deltaTime;
			if (m_time >= 1f)
			{
				m_time = 0f;
				m_startRotation = base.transform.localRotation;
				m_targetRotation = Quaternion.Euler(m_followAngle);
				m_phase = Phase.Follow;
			}
		}
		if (m_phase == Phase.Follow)
		{
			m_time += m_followStep * p_deltaTime;
			if (m_time >= 1f)
			{
				m_time = 0f;
				m_startRotation = base.transform.localRotation;
				m_targetRotation = m_originalRotation;
				m_phase = Phase.Return;
			}
		}
		else if (m_phase == Phase.Return)
		{
			m_time += m_returnStep * p_deltaTime;
		}
		base.transform.localRotation = Quaternion.Slerp(m_startRotation, m_targetRotation, m_time);
	}

	protected override void OnPlay()
	{
		m_targetRotation = Quaternion.Euler(m_riseUpAngle);
		m_originalRotation = (m_startRotation = base.transform.localRotation);
		m_phase = Phase.RiseUp;
		m_time = 0f;
	}

	protected override void OnInterrupted()
	{
		base.transform.localRotation = m_originalRotation;
	}
}
