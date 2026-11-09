using UnityEngine;

public class StoneDragonSpecialFX : MonsterSpecialFx
{
	public GameObject m_fxEyeGazePrefab;

	public GameObject m_fxShatteredPrefab;

	private ParticleSystemController m_fxRightEyeGaze;

	private ParticleSystemController m_fxLeftEyeGaze;

	private ParticleSystemController m_fxShattered;

	private Transform m_head;

	protected override void Awake()
	{
		m_head = base.transform.FindInHierarchy("Mob12 Head");
		GameObject gameObject = Object.Instantiate(m_fxEyeGazePrefab) as GameObject;
		gameObject.transform.parent = m_head;
		gameObject.transform.localPosition = new Vector3(-0.47f, 0.57f, -0.3f);
		m_fxLeftEyeGaze = gameObject.GetComponent<ParticleSystemController>();
		GameObject gameObject2 = Object.Instantiate(m_fxEyeGazePrefab) as GameObject;
		gameObject2.transform.parent = m_head;
		gameObject2.transform.localPosition = new Vector3(-0.47f, 0.57f, 0.3f);
		m_fxRightEyeGaze = gameObject2.GetComponent<ParticleSystemController>();
		GameObject gameObject3 = Object.Instantiate(m_fxShatteredPrefab) as GameObject;
		m_fxShattered = gameObject3.GetComponent<ParticleSystemController>();
		base.Awake();
	}

	private void OnEyeGaze()
	{
		if ((bool)m_head)
		{
			if ((bool)m_fxRightEyeGaze)
			{
				m_fxRightEyeGaze.Play();
			}
			if ((bool)m_fxLeftEyeGaze)
			{
				m_fxLeftEyeGaze.Play();
			}
		}
	}

	public void OnExplode(Vector3 position)
	{
		m_fxShattered.transform.position = position;
		m_fxShattered.Play();
	}
}
