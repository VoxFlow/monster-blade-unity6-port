using UnityEngine;

public class ArmorSetPlayerSkill : PlayerSkill, ArmorSetSkill
{
	[SerializeField]
	protected string m_armourSetName = string.Empty;

	[SerializeField]
	protected SkillEnum.SelfSkill m_selfSkill = SkillEnum.SelfSkill.None;

	[SerializeField]
	protected SkillEnum.ArmorSetSkill m_armorSkill = SkillEnum.ArmorSetSkill.None;

	[SerializeField]
	protected CutsceneController.CutsceneType m_cutsceneEnum = CutsceneController.CutsceneType.NONE;

	protected bool m_skillTriggered;

	public override SkillEnum.SelfSkill selfSkillType
	{
		get
		{
			return m_selfSkill;
		}
	}

	public SkillEnum.ArmorSetSkill armorSkillType
	{
		get
		{
			return m_armorSkill;
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
		return !string.IsNullOrEmpty(m_armourSetName) && p_data.IsEquipingFullSet(m_armourSetName);
	}

	protected override void Awake()
	{
		base.Awake();
		Messenger<CutsceneController.CutsceneType>.AddListener("cutscene_end", OnCutsceneEnd);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Messenger<CutsceneController.CutsceneType>.RemoveListener("cutscene_end", OnCutsceneEnd);
	}

	public virtual void TriggerSkill()
	{
		if (base.enabled)
		{
			if (!m_skillTriggered && null != SingletonComponent<CutsceneController>.Instance)
			{
				PreCutsceneInitialization();
				StartCoroutine(SingletonComponent<CutsceneController>.Instance.PlayCutscene(m_cutsceneEnum));
			}
			else
			{
				_TriggerSkill();
			}
		}
	}

	protected virtual void _TriggerSkill()
	{
	}

	protected virtual void OnCutsceneEnd(CutsceneController.CutsceneType p_cutscene)
	{
		if (base.enabled && p_cutscene == m_cutsceneEnum)
		{
			m_skillTriggered = true;
			PostCutsceneInitialization();
		}
	}

	protected virtual void PreCutsceneInitialization()
	{
	}

	protected virtual void PostCutsceneInitialization()
	{
	}
}
