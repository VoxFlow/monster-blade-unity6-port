using UnityEngine;

public class WingDragonSpecialFx : MonsterSpecialFx
{
	public GameObject m_GroundExplosion_Prefab;

	public GameObject m_HitExplosion_Prefab;

	public SelfDestroy m_FlyShockWaveLight_Prefab;

	public GameObject m_DelayExplosion_Prefab;

	private GameObject m_HitExplosionInstance;

	private GameObject m_DelayExplosionInstance;

	protected override void Start()
	{
		base.Start();
		m_HitExplosionInstance = Object.Instantiate(m_HitExplosion_Prefab) as GameObject;
		m_DelayExplosionInstance = Object.Instantiate(m_DelayExplosion_Prefab) as GameObject;
		m_HitExplosionInstance.transform.position = new Vector3(0f, -10000f, 0f);
		m_DelayExplosionInstance.transform.position = new Vector3(0f, -10000f, 0f);
		Messenger<Transform>.AddListener("fireball_PlayGroundExplosion", PlayGroundExplosion);
		Messenger<Transform>.AddListener("fireball_PlayHitExplosion", PlayHitExplosion);
		Messenger<Transform>.AddListener("fireball_PlayDelayExplosion", PlayDelayExplosion);
		Messenger<Transform>.AddListener("fireball_PlayShockWaveLight", PlayShockWaveLight);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Object.Destroy(m_HitExplosionInstance);
		Object.Destroy(m_DelayExplosionInstance);
		Messenger<Transform>.RemoveListener("fireball_PlayGroundExplosion", PlayGroundExplosion);
		Messenger<Transform>.RemoveListener("fireball_PlayHitExplosion", PlayHitExplosion);
		Messenger<Transform>.RemoveListener("fireball_PlayDelayExplosion", PlayDelayExplosion);
		Messenger<Transform>.RemoveListener("fireball_PlayShockWaveLight", PlayShockWaveLight);
	}

	public void PlayHitExplosion(Transform trans)
	{
		if (!Dbg.Assert(m_HitExplosionInstance != null, "WingDragonSpecialFx.PlayHitExplosion() don't have FXInstance"))
		{
			m_HitExplosionInstance.transform.parent = null;
			m_HitExplosionInstance.transform.localPosition = trans.position;
			m_HitExplosionInstance.GetComponent<ParticleSystemController>().Play();
		}
	}

	public void PlayDelayExplosion(Transform trans)
	{
		if (!Dbg.Assert(m_DelayExplosionInstance != null, "WingDragonSpecialFx.PlayDelayExplosion() don't have FXInstance"))
		{
			m_DelayExplosionInstance.transform.position = trans.position + new Vector3(0f, 1f, 0f);
			m_DelayExplosionInstance.transform.parent = trans;
			if (!Dbg.Assert(m_DelayExplosionInstance.GetComponent<ParticleSystemController>() != null, "WingDragonSpecialFx.PlayDelayExplosion() can't get ParticleSystemController"))
			{
				m_DelayExplosionInstance.GetComponent<ParticleSystemController>().Play();
			}
		}
	}

	public void PlayGroundExplosion(Transform trans)
	{
		if (!Dbg.Assert(m_GroundExplosion_Prefab != null, "WingDragonSpecialFx.PlayGroundExplosion(): m_GroundExplosion_Prefab is null"))
		{
			GameObject gameObject = Object.Instantiate(m_GroundExplosion_Prefab) as GameObject;
			if (gameObject != null)
			{
				gameObject.transform.parent = null;
				gameObject.transform.localPosition = trans.position;
			}
		}
	}

	public void PlayShockWaveLight(Transform trans)
	{
		if (!Dbg.Assert(m_FlyShockWaveLight_Prefab != null, "WingDragonSpecialFx.PlayShockWaveLight() don't have FXInstance"))
		{
			GameObject gameObject = Object.Instantiate((Object)m_FlyShockWaveLight_Prefab) as GameObject;
			gameObject.transform.AttachTo(base.transform, TransformAttachMode.PARENT_ORIGIN);
		}
	}

	public void PlayIntroShockWave()
	{
		if (!Dbg.Assert(m_FlyShockWaveLight_Prefab != null, "WingDragonSpecialFx.PlayIntroShockWave() don't have FXInstance"))
		{
			SelfDestroy selfDestroy = Object.Instantiate(m_FlyShockWaveLight_Prefab) as SelfDestroy;
			Vector3 position = base.transform.FindInHierarchy("Mob08 Pelvis").position;
			position.y = 0f;
			selfDestroy.gameObject.transform.position = position;
		}
	}
}
