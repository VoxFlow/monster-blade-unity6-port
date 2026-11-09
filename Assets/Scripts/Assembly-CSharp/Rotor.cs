using UnityEngine;

public class Rotor : MonoBehaviour
{
	private float m_turnSpeed;

	private Quaternion m_startRotation = Quaternion.identity;

	private Quaternion m_targetRotation = Quaternion.identity;

	private float m_lerp;

	private VoidCallback m_endCallback;

	private void Awake()
	{
		base.enabled = false;
	}

	public bool IsRotating()
	{
		return m_turnSpeed != 0f;
	}

	public void BeginRotate(Vector3 p_lookTarget, float p_period, VoidCallback p_endCallback = null)
	{
		float lookAngle = base.transform.GetLookAngle(p_lookTarget);
		BeginRotate(lookAngle, p_period, p_endCallback);
	}

	public void BeginRotate(float p_angle, float p_period, VoidCallback p_endCallback = null)
	{
		Quaternion identity = Quaternion.identity;
		Vector3 eulerAngles = base.transform.rotation.eulerAngles;
		eulerAngles.y += p_angle;
		identity.eulerAngles = eulerAngles;
		BeginRotate(identity, p_period, p_endCallback);
	}

	public void BeginRotate(Quaternion p_targetRot, float p_period, VoidCallback p_endCallback = null)
	{
		if (p_period <= 0f)
		{
			Debug.LogWarning("Turn period cannot be less than or equal to 0: " + p_period);
			return;
		}
		m_startRotation = base.transform.rotation;
		m_targetRotation = p_targetRot;
		m_turnSpeed = 1f / p_period;
		m_lerp = 0f;
		m_endCallback = p_endCallback;
		base.enabled = true;
	}

	public void StopRotate()
	{
		m_turnSpeed = 0f;
		m_startRotation = Quaternion.identity;
		m_targetRotation = Quaternion.identity;
		m_lerp = 0f;
		m_endCallback = null;
		base.enabled = false;
	}

	private void Update()
	{
		float num = m_turnSpeed * Time.deltaTime;
		m_lerp += num;
		base.transform.rotation = Quaternion.Slerp(m_startRotation, m_targetRotation, m_lerp);
		if (m_lerp >= 1f)
		{
			if (m_endCallback != null)
			{
				m_endCallback();
			}
			StopRotate();
		}
	}
}
