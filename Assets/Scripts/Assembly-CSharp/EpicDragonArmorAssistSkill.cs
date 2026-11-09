using UnityEngine;

public class EpicDragonArmorAssistSkill : FriendAssistNormalAttack, ArmorSetSkill
{
	[SerializeField]
	private string m_setName = string.Empty;

	[SerializeField]
	private float m_duration = 5f;

	[SerializeField]
	private float m_damageInterval = 1f;

	[SerializeField]
	private MonsterFlameFx m_flameFxPrefab;

	private FriendAssistSkill m_friendAssistLogic;

	private MonsterFlameLogic m_flameLogic;

	private MonsterFlameFx m_flameFx;

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
			return SkillEnum.ArmorSetSkill.EpicDragon;
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
		return !string.IsNullOrEmpty(m_setName) && p_data.IsEquipingFullSet(m_setName);
	}

	protected override void OnBattleReady()
	{
		base.OnBattleReady();
		if (!base.enabled)
		{
			return;
		}
		SkillSystem instance = SingletonComponent<SkillSystem>.Instance;
		if (instance != null)
		{
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
		if (null == m_flameLogic)
		{
			m_flameLogic = (MonsterFlameLogic)FlameLogic.FindOrCreateFlameLogic<MonsterFlameLogic>();
		}
		if (null != m_flameLogic)
		{
			m_flameLogic.attackerData = new PlayerData(base.friendData);
			m_flameLogic.attackerData.m_isMonster = true;
			m_flameLogic.attackerData.MonsterElement = base.monster.Data.MonsterElement;
			m_flameLogic.damageInterval = m_damageInterval;
			m_flameLogic.duration = m_duration;
			m_flameLogic.dealDirectDamage = true;
			m_flameLogic.damageMultiplier = (float)base.friendData.Level * m_damageInterval * 0.1f / m_duration;
		}
		if (null != base.monster && null == m_flameFx && null != m_flameFxPrefab)
		{
			m_flameFx = FlameFx.FindOrCreateFlameFx<MonsterFlameFx>(base.monster.gameObject, m_flameFxPrefab.gameObject);
			ShadowProjector componentInChildren = MBHier.GetComponentInChildren<ShadowProjector>(base.monster.gameObject);
			if (null != componentInChildren)
			{
				m_flameFx.transform.parent = componentInChildren.m_reference;
			}
			m_flameFx.transform.localPosition = Vector3.zero;
			m_flameFx.targetRenderers = MBHier.GetComponentsInChildren<SkinnedMeshRenderer>(base.monster.gameObject);
			m_flameFx.monster = base.monster.gameObject.GetComponent<Monster>();
		}
	}

	public override void OnSkillTrigger()
	{
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
		if (null != m_flameLogic && null != m_flameFx)
		{
			m_flameLogic.Burn(m_flameFx.OnBurnOut);
			m_flameFx.Burn();
		}
		m_skillTriggered = true;
	}
}
