public class StoneDragonBlockLogic : PetrifyBlockLogic
{
	public override void Init()
	{
		base.Init();
		StoneDragon stoneDragon = (StoneDragon)m_monster;
		base.duration = stoneDragon.petrifyDuration;
		base.armourMod = stoneDragon.petrifiedArmourMod;
	}

	protected override bool IsPetrifyAttack(MonsterAttack p_attack)
	{
		return p_attack != null && p_attack.animationName == "Att_Petrified";
	}

	protected override void OnNormalBlock(MonsterAttack p_attack)
	{
		StoneDragon stoneDragon = (StoneDragon)m_monster;
		if (stoneDragon.performingPetrifyAttack)
		{
			StartCoroutine(PlayMonsterHitReact());
		}
		base.OnNormalBlock(p_attack);
	}
}
