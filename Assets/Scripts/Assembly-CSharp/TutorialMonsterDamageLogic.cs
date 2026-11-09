public class TutorialMonsterDamageLogic : MonsterDamageLogic
{
	protected TutorialMonsterAi m_tutorialAI;

	protected override void OnBattleReady()
	{
		base.OnBattleReady();
		if (m_monsterAI is TutorialMonsterAi)
		{
			m_tutorialAI = m_monsterAI as TutorialMonsterAi;
		}
	}

	public override void DealDamageToMonster(DamageData p_damage)
	{
		if ((bool)m_tutorialAI)
		{
			if (m_tutorialAI != null && m_tutorialAI.isImmortal && p_damage.finalDamage >= m_monster.Data.Hp)
			{
				m_monster.Data.Hp = 1f;
			}
			else
			{
				base.DealDamageToMonster(p_damage);
			}
		}
		else
		{
			base.DealDamageToMonster(p_damage);
		}
	}
}
