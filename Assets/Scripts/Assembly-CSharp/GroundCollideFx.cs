using UnityEngine;

public class GroundCollideFx : MonoBehaviour
{
	public GameObject m_fxFootDustPrefab;

	public string m_stepSoundFx = "list_mob12_dust";

	protected SoundManager m_sounMgr;

	protected void Awake()
	{
		m_sounMgr = SingletonComponent<SoundManager>.Instance;
		base.enabled = false;
	}

	protected virtual void OnTriggerEnter(Collider p_other)
	{
		if (p_other.tag == "MonsterFoot")
		{
			OnMonsterStep(p_other);
		}
	}

	protected void OnMonsterStep(Collider p_other)
	{
		if (m_fxFootDustPrefab != null && GameManager.qualitySetting != GameManager.QualitySetting.Low)
		{
			GameObject gameObject = Object.Instantiate(m_fxFootDustPrefab) as GameObject;
			if (gameObject != null)
			{
				gameObject.transform.position = p_other.bounds.center;
				ParticleSystem componentInChildren = MBHier.GetComponentInChildren<ParticleSystem>(gameObject);
			}
		}
		if (m_sounMgr != null && !string.IsNullOrEmpty(m_stepSoundFx))
		{
			m_sounMgr.PlayPlaylist(m_stepSoundFx);
		}
	}
}
