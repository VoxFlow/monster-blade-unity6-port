using UnityEngine;

public class CameraInterpolator : CameraFx
{
	private Vector3 m_startPos = Vector3.zero;

	private Vector3 m_targetPos = Vector3.zero;

	private Quaternion m_startRot = Quaternion.identity;

	private Quaternion m_targetRot = Quaternion.identity;

	private float m_startFov;

	private float m_targetFov;

	private float m_lerp;

	private float m_step;

	private bool m_transformLocal;

	public Vector3 TargetPos
	{
		get
		{
			return m_targetPos;
		}
	}

	public Quaternion TargetRot
	{
		get
		{
			return m_targetRot;
		}
	}

	public Transform TargetTransform
	{
		set
		{
			if (m_transformLocal)
			{
				m_targetPos = value.localPosition;
				m_targetRot = value.localRotation;
			}
			else
			{
				m_targetPos = value.position;
				m_targetRot = value.rotation;
			}
		}
	}

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

	public float Speed
	{
		get
		{
			return m_step;
		}
		set
		{
			m_step = value;
		}
	}

	public bool TransformLocal
	{
		get
		{
			return m_transformLocal;
		}
		set
		{
			m_transformLocal = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		interuptable = false;
		m_type = 3;
	}

	public override bool IsFinished()
	{
		return m_lerp >= 1f;
	}

	protected override void OnInterrupted()
	{
		if (m_transformLocal)
		{
			base.transform.localPosition = m_targetPos;
			base.transform.localRotation = m_targetRot;
		}
		else
		{
			base.transform.position = m_targetPos;
			base.transform.rotation = m_targetRot;
		}
		base.GetComponent<Camera>().fieldOfView = m_targetFov;
	}

	protected override void OnPlay()
	{
		if (m_transformLocal)
		{
			m_startPos = base.transform.localPosition;
			m_startRot = base.transform.localRotation;
		}
		else
		{
			m_startPos = base.transform.position;
			m_startRot = base.transform.rotation;
		}
		m_startFov = base.GetComponent<Camera>().fieldOfView;
		m_lerp = 0f;
		Dbg.Log(ToString());
	}

	protected override void OnUpdate(float p_deltaTime)
	{
		m_lerp += m_step * p_deltaTime;
		m_lerp = Mathf.Min(m_lerp, 1f);
		if (m_transformLocal)
		{
			base.transform.localPosition = Vector3.Lerp(m_startPos, m_targetPos, m_lerp);
			base.transform.localRotation = Quaternion.Slerp(m_startRot, m_targetRot, m_lerp);
		}
		else
		{
			base.transform.position = Vector3.Lerp(m_startPos, m_targetPos, m_lerp);
			base.transform.rotation = Quaternion.Slerp(m_startRot, m_targetRot, m_lerp);
		}
		base.GetComponent<Camera>().fieldOfView = Mathf.Lerp(m_startFov, m_targetFov, m_lerp);
	}

	public override string ToString()
	{
		return string.Format("[CameraInterpolator: TargetPos={0}, TargetRot={1}, TargetFov={2}, Speed={3}, TransformLocal={4}, StartPos={5}, StartRot={6}, StartFov={7}]", TargetPos, TargetRot, TargetFov, Speed, TransformLocal, m_startPos, m_startRot, m_startFov);
	}
}
