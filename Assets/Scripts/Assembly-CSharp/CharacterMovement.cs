using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
	private float m_moveSpeed;

	private float m_finalSpeed;

	private float m_moveAccel;

	private Vector3 m_moveVec = Vector3.zero;

	private Vector3 m_targetPos = Vector3.zero;

	private VoidCallback m_endCallback;

	public Vector3 MoveVector
	{
		get
		{
			return m_moveVec;
		}
	}

	public bool IsMoving()
	{
		return m_moveVec != Vector3.zero;
	}

	public void StopMove()
	{
		m_moveSpeed = 0f;
		m_finalSpeed = 0f;
		m_moveAccel = 0f;
		m_moveVec = Vector3.zero;
	}

	public void MoveTo(Vector3 p_targetPos, float p_startSpeed, float p_finalSpeed, float p_timeReachFinalSpeed = 0f, VoidCallback p_endCallback = null)
	{
		if (p_startSpeed == 0f && p_finalSpeed == 0f)
		{
			Dbg.LogWarn("Invalid parameter(s). p_startSpeed: " + p_startSpeed + ", p_finalSpeed: " + p_finalSpeed);
		}
		else
		{
			m_targetPos = p_targetPos;
			m_moveSpeed = p_startSpeed;
			m_finalSpeed = p_finalSpeed;
			m_moveVec = (p_targetPos - base.transform.position).normalized;
			m_moveAccel = ((!(p_timeReachFinalSpeed > 0f)) ? 0f : ((p_finalSpeed - p_startSpeed) / p_timeReachFinalSpeed));
			m_endCallback = p_endCallback;
			SendMessage("OnMove", SendMessageOptions.DontRequireReceiver);
		}
	}

	private void Update()
	{
		if (!IsMoving())
		{
			return;
		}
		float num = m_moveSpeed * Time.deltaTime;
		float sqrMagnitude = (base.transform.position - m_targetPos).sqrMagnitude;
		if (sqrMagnitude <= num * num)
		{
			base.transform.position = m_targetPos;
			StopMove();
			if (m_endCallback != null)
			{
				m_endCallback();
			}
			SendMessage("OnFinishMove", SendMessageOptions.DontRequireReceiver);
			return;
		}
		base.transform.position += m_moveVec * num;
		m_moveSpeed += m_moveAccel * Time.deltaTime;
		if (m_moveAccel < 0f && m_moveSpeed < m_finalSpeed)
		{
			m_moveSpeed = m_finalSpeed;
			m_moveAccel = 0f;
		}
		else if (m_moveAccel > 0f && m_moveSpeed > m_finalSpeed)
		{
			m_moveSpeed = m_finalSpeed;
			m_moveAccel = 0f;
		}
		float sqrMagnitude2 = (base.transform.position - m_targetPos).sqrMagnitude;
		if (m_moveSpeed.CompareTo(0f) == 0 && m_moveAccel.CompareTo(0f) == 0)
		{
			Dbg.LogWarn("Character is stopped prematurely.");
			base.transform.position = m_targetPos;
		}
		else if (sqrMagnitude2 > sqrMagnitude)
		{
			Dbg.LogWarn("Character is start moving away from the target position.");
			base.transform.position = m_targetPos;
		}
	}
}
