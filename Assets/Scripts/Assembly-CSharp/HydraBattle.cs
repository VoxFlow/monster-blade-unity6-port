public class HydraBattle : BattleLogic
{
	private bool m_hasLifeSteal;

	private SkillSystem m_skillSystem;

	protected override void OnBattleReady()
	{
		base.OnBattleReady();
		m_skillSystem = SingletonComponent<SkillSystem>.Instance;
		HydraLifeStealLogic component = base.gameObject.GetComponent<HydraLifeStealLogic>();
		if (m_skillSystem != null)
		{
			LifeStealLogic componentInChildren = MBHier.GetComponentInChildren<LifeStealLogic>(m_skillSystem.gameObject);
			if (componentInChildren != null && component != null)
			{
				m_hasLifeSteal = componentInChildren.enabled;
				if (m_hasLifeSteal)
				{
					componentInChildren.enabled = false;
				}
			}
		}
		component.enabled = m_hasLifeSteal;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (m_skillSystem != null)
		{
			LifeStealLogic component = m_skillSystem.gameObject.GetComponent<LifeStealLogic>();
			if (component != null && m_hasLifeSteal)
			{
				component.enabled = true;
			}
		}
	}
}
