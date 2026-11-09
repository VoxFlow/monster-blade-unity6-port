using UnityEngine;

public class StoneDragonArmorAssistSkill : FriendAssistNormalAttack, ArmorSetSkill
{
	[SerializeField]
	protected PetrifiedLogic m_skillLogicPrefab;

	[SerializeField]
	private float m_duration = 15f;

	[SerializeField]
	private float m_armourMultiplier = 1.05f;

	private MonsterPetrifiedLogic m_skillLogic;

	private FriendAssistSkill m_friendAssistLogic;

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
			return SkillEnum.ArmorSetSkill.StoneDragon;
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
		return p_data.IsEquipingFullSet(GLOBAL.STONEDRAGON_SETNAME);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (null != m_skillLogic)
		{
			Object.Destroy(m_skillLogic.gameObject);
		}
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
			m_skillLogic = Object.Instantiate(m_skillLogicPrefab) as MonsterPetrifiedLogic;
		}
		if (null != m_skillLogic)
		{
			m_skillLogic.Init();
			m_skillLogic.shakeInterval = 3f;
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
				if (m_friendAssistLogic.assistSkillType == SkillEnum.AssistSkill.Stun)
				{
					m_duration += base.friendData.FriendAssistValue;
				}
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
		m_skillLogic.OnPetrified(m_duration, m_armourMultiplier, null);
		m_skillTriggered = true;
	}
}
