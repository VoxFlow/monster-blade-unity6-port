using UnityEngine;

public class OrbitAroundTarget3D : MonoBehaviour
{
	private Vector3 m_orbitCenter = Vector3.zero;

	private Vector3 m_axisOfRotation = Vector3.zero;

	private float m_orbitVelocity;

	private float m_period;

	private void Awake()
	{
		base.enabled = false;
	}

	public bool IsOrbitFinished()
	{
		return m_orbitVelocity == 0f;
	}

	public void StopOrbit()
	{
		Dbg.Log("Orbit Stop");
		m_orbitVelocity = 0f;
		m_period = 0f;
		m_orbitCenter = Vector3.zero;
		m_axisOfRotation = Vector3.zero;
		base.enabled = false;
	}

	public void Orbit(Vector3 p_center, float p_velocity, float p_period)
	{
		Orbit(p_center, p_velocity, p_period, Vector3.up);
	}

	public void Orbit(Vector3 p_center, float p_velocity, float p_period, Vector3 p_axisOfRotation)
	{
		Dbg.Log("Orbit Start");
		m_orbitCenter = p_center;
		m_axisOfRotation = p_axisOfRotation;
		m_orbitVelocity = p_velocity;
		m_period = p_period;
		base.enabled = true;
	}

	public void Update()
	{
		if (m_period >= 0f)
		{
			if (Time.deltaTime > m_period)
			{
				base.transform.RotateAround(m_orbitCenter, m_axisOfRotation, m_period * m_orbitVelocity);
				StopOrbit();
			}
			else
			{
				base.transform.RotateAround(m_orbitCenter, m_axisOfRotation, m_orbitVelocity * Time.deltaTime);
				m_period -= Time.deltaTime;
			}
		}
		else
		{
			base.transform.RotateAround(m_orbitCenter, m_axisOfRotation, m_orbitVelocity * Time.deltaTime);
		}
	}
}
