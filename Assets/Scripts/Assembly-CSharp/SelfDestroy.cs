using UnityEngine;

public class SelfDestroy : MonoBehaviour
{
	public float m_duration = 1f;

	private ParticleSystem[] m_particlesSystem;


	private void Start()
	{
		// PORT (CD-019)
		m_particlesSystem = MBLegacyEmitter.Shuriken(MBHier.GetComponentsInChildren<ParticleSystem>(base.gameObject));
		if (m_particlesSystem != null)
		{
			base.enabled = true;
			return;
		}
		MBDestroy.Destroy(base.gameObject, m_duration);   // PORT (CD-053)
		base.enabled = false;
	}

	private void Update()
	{
		if (m_particlesSystem == null)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		bool flag = false;
		if (m_particlesSystem != null)
		{
			ParticleSystem[] particlesSystem = m_particlesSystem;
			foreach (ParticleSystem particleSystem in particlesSystem)
			{
				if (particleSystem != null && particleSystem.gameObject.active && particleSystem.IsAlive())
				{
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			Object.Destroy(base.gameObject);
		}
	}
}
