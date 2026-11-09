public class MonsterFlameLogic : FlameLogic
{
	private PlayerCollisionData m_collDataCached;

	public bool dealDirectDamage { get; set; }

	protected override void Awake()
	{
		base.Awake();
		base.collisionData = base.gameObject.AddComponent<PlayerCollisionData>();
		m_collDataCached = base.collisionData as PlayerCollisionData;
		m_collDataCached.attack = new PlayerAttack();
		m_collDataCached.attack.type = PlayerAttack.Type.Skill;
		m_collDataCached.attack.direction = PlayerAttack.Direction.None;
		m_collDataCached.attack.forceCleanHit = true;
	}

	public override void Burn(Callback p_onBurnOut = null)
	{
		base.Burn(p_onBurnOut);
		((PlayerCollisionData)base.collisionData).attack.multiplier = base.damageMultiplier;
	}

	protected override void DealFlameDamage()
	{
		base.DealFlameDamage();
		if (dealDirectDamage)
		{
			m_collDataCached.directDamage = m_collDataCached.attackerData.Atk * base.damageMultiplier;
		}
		else
		{
			m_collDataCached.directDamage = 0f;
		}
		Messenger<PlayerCollisionData>.Invoke("monster_get_hit", (PlayerCollisionData)base.collisionData);
	}
}
