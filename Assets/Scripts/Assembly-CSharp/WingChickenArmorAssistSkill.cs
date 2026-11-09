public class WingChickenArmorAssistSkill : FriendAssistNormalAttack, ArmorSetSkill
{
	private WingChickenArmorSkillLogic m_skillLogic;

	private FriendAssistSkill m_friendAssistLogic;

	private float m_flameDamageMul = 0.5f;

	private float m_duration = 12f;

	private float m_interval = 1f;

	private float m_explosionDamageMul = 0.15f;

	private bool m_skillTriggered;

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
			return SkillEnum.ArmorSetSkill.WingChicken;
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
		return p_data.IsEquipingFullSet(GLOBAL.WINGCHICKEN_SETNAME);
	}

	protected override void OnBattleReady()
	{
		base.OnBattleReady();
		if (!base.enabled)
		{
			return;
		}
		if (null == m_skillLogic)
		{
			m_skillLogic = base.gameObject.GetComponent<WingChickenArmorSkillLogic>();
		}
		if (null != m_skillLogic)
		{
			m_skillLogic.enabled = true;
		}
		SkillSystem instance = SingletonComponent<SkillSystem>.Instance;
		if (!(instance != null))
		{
			return;
		}
		Skill[] skills = instance.skills;
		foreach (Skill skill in skills)
		{
			if (!(skill == null) && !(skill is ArmorSetSkill) && base.friendData != null && skill is FriendAssistSkill && skill.DoesCharacterHaveSkill(base.friendData))
			{
				m_friendAssistLogic = (FriendAssistSkill)skill;
				break;
			}
		}
	}

	public override void OnSkillTrigger()
	{
		if (null == m_skillLogic)
		{
			return;
		}
		if (m_friendAssistLogic != null)
		{
			if (m_friendAssistLogic.assistSkillType != SkillEnum.AssistSkill.Default)
			{
				m_friendAssistLogic.OnSkillTrigger();
			}
			else
			{
				base.OnSkillTrigger();
			}
		}
		m_skillLogic.Burn(base.monster.gameObject, base.friendData, m_flameDamageMul, m_interval, m_duration);
		m_skillLogic.explosionDamage = base.monster.Data.MaxHp * m_explosionDamageMul;
		m_skillTriggered = true;
	}
}
