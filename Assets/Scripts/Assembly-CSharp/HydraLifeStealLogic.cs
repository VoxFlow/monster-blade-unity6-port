public class HydraLifeStealLogic : LifeStealLogic
{
	private Hydra m_hydra;

	private bool m_allHeadCutOff;

	protected override void Awake()
	{
		base.Awake();
		Messenger<PlayerCollisionData>.AddListener("monster_get_hit", OnMonsterGetHit);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Messenger<PlayerCollisionData>.RemoveListener("monster_get_hit", OnMonsterGetHit);
	}

	protected override void OnBattleReady()
	{
		base.OnBattleReady();
		m_hydra = (Hydra)base.monster;
	}

	protected void OnMonsterGetHit(PlayerCollisionData p_collData)
	{
		if (null != m_hydra)
		{
			m_allHeadCutOff = m_hydra.IsAllHeadCutOff();
		}
	}

	protected override void OnMonsterReceiveDamage(DamageData p_dmg)
	{
		if (m_allHeadCutOff)
		{
			base.OnMonsterReceiveDamage(p_dmg);
		}
	}
}
