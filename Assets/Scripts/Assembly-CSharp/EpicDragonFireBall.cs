using UnityEngine;

public class EpicDragonFireBall : Projectile
{
	[SerializeField]
	protected SelfDestroy m_explosionPrefab;

	private SoundManager m_soundMgr;

	private SoundManager soundMgr
	{
		get
		{
			if (m_soundMgr == null)
			{
				m_soundMgr = SingletonComponent<SoundManager>.Instance;
			}
			return m_soundMgr;
		}
	}

	public bool playExplosion { get; set; }

	public void Explode()
	{
		if (null != m_explosionPrefab)
		{
			SelfDestroy selfDestroy = Object.Instantiate(m_explosionPrefab) as SelfDestroy;
			if (null != selfDestroy)
			{
				selfDestroy.transform.position = base.transform.position;
				selfDestroy.transform.rotation = base.transform.rotation;
			}
		}
		MBDestroy.Destroy(base.gameObject, 0.5f);   // PORT (CD-053)
		base.gameObject.SetActiveRecursively(false);
		if (null != soundMgr)
		{
			soundMgr.PlaySoundFx("list_explosion");
		}
	}

	protected override void _OnTriggerEnter(Collider p_other)
	{
		playExplosion = true;
	}

	protected void LateUpdate()
	{
		if (playExplosion)
		{
			Explode();
		}
	}
}
