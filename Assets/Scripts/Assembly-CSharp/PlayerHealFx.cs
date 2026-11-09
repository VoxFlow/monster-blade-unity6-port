using UnityEngine;

public class PlayerHealFx : MonoBehaviour
{
	private ParticleSystemController m_particle;

	private void Awake()
	{
		Object original = Resources.Load("FX/Prefabs/FX_Heal");
		GameObject gameObject = Object.Instantiate(original) as GameObject;
		gameObject.transform.parent = base.transform;
		m_particle = gameObject.GetComponent<ParticleSystemController>();
		m_particle.Stop();
	}

	private void OnHeal(float p_hp)
	{
		if (m_particle.IsPlaying())
		{
			m_particle.Stop();
		}
		m_particle.Play();
	}
}
