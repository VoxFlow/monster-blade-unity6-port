public class WolfDamageLogic : MonsterDamageLogic
{
	private WolfAI m_wolfAI;

	protected override void OnBattleReady()
	{
		base.OnBattleReady();
		m_wolfAI = (WolfAI)m_monsterAI;
	}

	public override void DealDamageToMonster(DamageData p_damage)
	{
		if (m_monster.Data.Hp <= p_damage.finalDamage && !m_wolfAI.wasRage)
		{
			m_monster.Data.Hp = 1f;
		}
		else
		{
			base.DealDamageToMonster(p_damage);
		}
	}

	protected override void DetermineMonsterReaction()
	{
		if (m_wolfAI != null && m_wolfAI.interuptable)
		{
			base.DetermineMonsterReaction();
		}
	}
}
