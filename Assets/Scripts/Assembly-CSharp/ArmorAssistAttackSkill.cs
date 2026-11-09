using UnityEngine;

public class ArmorAssistAttackSkill : FriendAssistNormalAttack, ArmorSetSkill
{
	[SerializeField]
	protected string m_armourSetName = string.Empty;

	[SerializeField]
	protected SkillEnum.ArmorSetSkill m_armorSkill = SkillEnum.ArmorSetSkill.None;

	protected FriendAssistSkill m_friendAssistLogic;

	protected bool m_skillTriggered;

	public override SkillEnum.AssistSkill assistSkillType
	{
		get
		{
			if (m_friendAssistLogic != null)
			{
				return m_friendAssistLogic.assistSkillType;
			}
			return SkillEnum.AssistSkill.None;
		}
	}

	public SkillEnum.ArmorSetSkill armorSkillType
	{
		get
		{
			return m_armorSkill;
		}
	}

	public override string spriteName
	{
		get
		{
			if (string.IsNullOrEmpty(m_spriteName))
			{
				return (!(null != m_friendAssistLogic)) ? string.Empty : m_friendAssistLogic.spriteName;
			}
			return m_spriteName;
		}
	}

	public override string darkSpriteName
	{
		get
		{
			if (string.IsNullOrEmpty(m_spriteName))
			{
				return (!(null != m_friendAssistLogic)) ? string.Empty : m_friendAssistLogic.darkSpriteName;
			}
			return m_darkSpriteName;
		}
	}

	public bool skillTriggered
	{
		get
		{
			return m_skillTriggered;
		}
	}

	public override bool DoesCharacterHaveSkill(PlayerData p_data)
	{
		return p_data.IsEquipingFullSet(m_armourSetName);
	}

	protected override void OnBattleReady()
	{
		base.OnBattleReady();
		if (!base.enabled)
		{
			return;
		}
		SkillSystem instance = SingletonComponent<SkillSystem>.Instance;
		StageManager instance2 = SingletonComponent<StageManager>.Instance;
		PlayerData playerData = null;
		if (instance2 != null)
		{
			playerData = instance2.GetFriendData(0);
		}
		if (!base.enabled || !(instance != null))
		{
			return;
		}
		Skill[] skills = instance.skills;
		foreach (Skill skill in skills)
		{
			if (!(skill == null) && !(skill is ArmorSetSkill) && playerData != null && skill is FriendAssistSkill && skill.DoesCharacterHaveSkill(playerData))
			{
				m_friendAssistLogic = (FriendAssistSkill)skill;
				break;
			}
		}
		if (m_friendAssistLogic != null && m_friendAssistLogic.assistSkillType == SkillEnum.AssistSkill.MassiveDamage)
		{
			m_collData.directDamage = playerData.FriendAssistValue;
			m_collData.attack.multiplier = 1f;
		}
	}

	public override void OnSkillTrigger()
	{
		base.OnSkillTrigger();
		if (m_friendAssistLogic != null && m_friendAssistLogic.assistSkillType != SkillEnum.AssistSkill.MassiveDamage && m_friendAssistLogic.assistSkillType != SkillEnum.AssistSkill.Default)
		{
			m_friendAssistLogic.OnSkillTrigger();
		}
		m_skillTriggered = true;
	}
}
