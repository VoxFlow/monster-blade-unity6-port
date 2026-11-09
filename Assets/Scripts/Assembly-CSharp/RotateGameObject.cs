using UnityEngine;

public class RotateGameObject : MonoBehaviour
{
	public bool m_clockwise = true;

	public float m_degreesPerSecond = 0.01f;

	private Transform m_transform;

	private void Awake()
	{
		m_transform = base.transform;
	}

	public void Reset()
	{
		m_transform.rotation = Quaternion.identity;
	}

	private void Update()
	{
		m_transform.RotateAroundLocal(Vector3.forward, (!m_clockwise) ? m_degreesPerSecond : (0f - m_degreesPerSecond));
	}
}
