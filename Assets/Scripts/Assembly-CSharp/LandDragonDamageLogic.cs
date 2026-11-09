public class LandDragonDamageLogic : MonsterDamageLogic
{
	private LandDragon m_dragon;

	protected override void OnBattleReady()
	{
		base.OnBattleReady();
		m_dragon = (LandDragon)m_monster;
	}

	protected override void OnMonsterGetHit(PlayerCollisionData p_collData)
	{
		if (!m_dragon.fullyUnderGround)
		{
			base.OnMonsterGetHit(p_collData);
		}
	}

	protected override void DetermineMonsterReaction()
	{
		PlayerAttack attack = m_collData.attack;
		bool flag = attack.type == PlayerAttack.Type.Ripost || attack.type == PlayerAttack.Type.Super;
		if (!m_dragon.diving && flag)
		{
			base.DetermineMonsterReaction();
		}
	}
}
