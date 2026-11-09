public class SecondCombatTutorialMonsterDamageLogic : TutorialMonsterDamageLogic
{
	public override void DealDamageToMonster(DamageData p_damage)
	{
		StageManager instance = SingletonComponent<StageManager>.Instance;
		if (instance != null && p_damage.attacker == instance.GetFriendData(0))
		{
			m_tutorialAI.isImmortal = true;
		}
		else
		{
			m_tutorialAI.isImmortal = false;
		}
		base.DealDamageToMonster(p_damage);
	}
}
