using System.Collections.Generic;

public class DisableArmorSkillsInEpicDragonBattle : BattleLogic
{
	private Skill[] m_armourSetSkills;

	private EpicDragonAI.Phase m_phase = EpicDragonAI.Phase.NONE;

	protected override void OnBattleStart()
	{
		base.OnBattleStart();
		if (!(m_monster is EpicDragon))
		{
			return;
		}
		SkillSystem instance = SingletonComponent<SkillSystem>.Instance;
		if (m_armourSetSkills == null && null != SingletonComponent<SkillSystem>.Instance)
		{
			List<Skill> list = new List<Skill>();
			for (int i = 0; i < instance.playerSkills.Length; i++)
			{
				if (instance.playerSkills[i] is ArmorSetSkill)
				{
					list.Add(instance.playerSkills[i]);
				}
			}
			m_armourSetSkills = list.ToArray();
		}
		if (m_armourSetSkills.Length == 0)
		{
			base.enabled = false;
		}
		else if (!base.enabled)
		{
			base.enabled = true;
		}
		OnPhaseChange(m_phase);
	}

	protected override void OnBattleEnd()
	{
		base.OnBattleEnd();
	}

	private void OnEnable()
	{
		Messenger<EpicDragonAI.Phase>.AddListener("epicdragon_phase_change", OnPhaseChange);
	}

	private void OnDisable()
	{
		Messenger<EpicDragonAI.Phase>.RemoveListener("epicdragon_phase_change", OnPhaseChange);
	}

	private void OnPhaseChange(EpicDragonAI.Phase p_phase)
	{
		if (p_phase == EpicDragonAI.Phase.FINAL)
		{
			for (int i = 0; i < m_armourSetSkills.Length; i++)
			{
				ArmorSetSkill armorSetSkill = (ArmorSetSkill)m_armourSetSkills[i];
				m_armourSetSkills[i].enabled = armorSetSkill.skillTriggered;
			}
		}
		m_phase = p_phase;
	}
}
