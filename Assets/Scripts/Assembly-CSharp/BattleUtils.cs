using UnityEngine;

public static class BattleUtils
{
	private static SuperAttackSystem m_superAttackSystem;

	private static SkillSystem m_skillSystem;

	private static SuperAttackSystem superAttackSystem
	{
		get
		{
			if (null == m_superAttackSystem)
			{
				m_superAttackSystem = SingletonComponent<SuperAttackSystem>.Instance;
			}
			return m_superAttackSystem;
		}
	}

	private static SkillSystem skillSystem
	{
		get
		{
			if (null == m_skillSystem)
			{
				m_skillSystem = SingletonComponent<SkillSystem>.Instance;
			}
			return m_skillSystem;
		}
	}

	public static void EnableSkills(bool p_enable, Object p_requester)
	{
		if (null != superAttackSystem && null != superAttackSystem.superAttackLogic)
		{
			if (p_enable)
			{
				superAttackSystem.superAttackLogic.data.EnableSkill(p_requester);
			}
			else
			{
				superAttackSystem.superAttackLogic.data.DisableSkill(p_requester);
			}
		}
		if (null != skillSystem && null != skillSystem.friendSkill)
		{
			if (p_enable)
			{
				skillSystem.friendSkill.skillData.EnableSkill(p_requester);
			}
			else
			{
				skillSystem.friendSkill.skillData.DisableSkill(p_requester);
			}
		}
	}
}
