public class PlayerFlameLogic : FlameLogic
{
	protected override void Awake()
	{
		base.Awake();
		base.collisionData = base.gameObject.AddComponent<MonsterCollisionData>();
		((MonsterCollisionData)base.collisionData).attack.type = MonsterAttack.Type.Ranged;
	}

	public override void Burn(Callback p_onBurnOut = null)
	{
		base.Burn(p_onBurnOut);
		((MonsterCollisionData)base.collisionData).attack.multiplier = base.damageMultiplier;
	}

	protected override void DealFlameDamage()
	{
		Messenger<MonsterCollisionData>.Invoke("player_get_hit", (MonsterCollisionData)base.collisionData);
	}
}
