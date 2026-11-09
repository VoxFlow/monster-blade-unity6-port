using UnityEngine;

public class PlayerLifeStealFx : MonoBehaviour
{
	private ParticleSystemController m_particle;

	private void Awake()
	{
		Object original = Resources.Load("FX/Prefabs/FX_LifeSteal");
		GameObject gameObject = Object.Instantiate(original) as GameObject;
		gameObject.transform.parent = base.transform;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.transform.localRotation = Quaternion.identity;
		m_particle = gameObject.GetComponent<ParticleSystemController>();
		m_particle.Stop();
	}

	public void Play()
	{
		if (m_particle.IsPlaying())
		{
			m_particle.Stop();
		}
		m_particle.Play();
	}
}
