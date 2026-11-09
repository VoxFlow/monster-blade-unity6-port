using UnityEngine;

// PORT (CD-021)
public class LandDragonRakeFx : MonoBehaviour
{
	public float m_duration = 2f;

	public float m_distance = 16f;

	private float m_rakeTime;

	private float m_speed;

	private ParticleSystem[] m_particles;

	private void Awake()
	{
		m_particles = MBHier.GetComponentsInChildren<ParticleSystem>(this);
	}

	private void Start()
	{
		m_speed = m_distance / m_duration;
	}

	private void StopParticles()
	{
		ParticleSystem[] particles = m_particles;
		foreach (ParticleSystem particleSystem in particles)
		{
			if (particleSystem != null)
			{
				particleSystem.Stop();
			}
		}
	}

	private void Update()
	{
		if (m_rakeTime > m_duration)
		{
			StopParticles();
			return;
		}
		m_rakeTime += Time.deltaTime;
		base.transform.Translate(Vector3.forward * Time.deltaTime * m_speed);
	}
}
