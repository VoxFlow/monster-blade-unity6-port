using System.Collections;
using UnityEngine;

public class HydraArmorPlayerSkill : PlayerSkill, ArmorSetSkill
{
	[SerializeField]
	protected PlayerBuffFx m_buffFxPrefab;

	private float m_duration = 65f;

	private float m_hpRegenerationRate;

	private bool m_skillTriggered;

	private bool m_active;

	private PlayerBuffFx m_buffFx;

	private NBTimer m_skillTimer;

	private static readonly float triggerCondition = 0.21f;

	private static readonly float maxSpeedModifier = 1.85f;

	private static readonly float maxHpRegeneratingRate = 0.2f;

	private static readonly float minHpRegeneratingRate = 0.004f;

	private bool m_skillEffectOn;

	public override SkillEnum.SelfSkill selfSkillType
	{
		get
		{
			return SkillEnum.SelfSkill.None;
		}
	}

	public SkillEnum.ArmorSetSkill armorSkillType
	{
		get
		{
			return SkillEnum.ArmorSetSkill.Hydra;
		}
	}

	private bool skillEffectOn
	{
		get
		{
			return m_skillEffectOn;
		}
		set
		{
			if (value)
			{
				if (null != m_buffFxPrefab)
				{
					m_buffFx = PlayerBuffFx.InstantiateFx(m_buffFxPrefab.gameObject, base.player, "Male_ Spine");
				}
			}
			else
			{
				if (null != m_buffFx)
				{
					Object.Destroy(m_buffFx.gameObject);
				}
				if (null != base.player)
				{
					base.player.AttackSpeedModifier = 1f;
				}
			}
			m_active = value;
			m_skillEffectOn = value;
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
		return p_data.IsEquipingFullSet(GLOBAL.HYDRA_SETNAME);
	}

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (skillEffectOn)
		{
			skillEffectOn = false;
		}
	}

	private void OnEnable()
	{
		m_skillTimer = base.gameObject.AddComponent<NBTimer>();
		m_skillTimer.Name = "Hydra Armour Timer";
		m_skillTimer.IsRealtime = false;
		Messenger<CutsceneController.CutsceneType>.AddListener("cutscene_start", OnCutsceneStart);
		Messenger<CutsceneController.CutsceneType>.AddListener("cutscene_end", OnCutsceneEnd);
		Messenger.AddListener("battle_start", OnBattleStart);
		Messenger.AddListener("battle_end", OnBattleEnd);
	}

	private void OnDisable()
	{
		if (null != m_skillTimer)
		{
			Object.Destroy(m_skillTimer);
		}
		Messenger<CutsceneController.CutsceneType>.RemoveListener("cutscene_start", OnCutsceneStart);
		Messenger<CutsceneController.CutsceneType>.RemoveListener("cutscene_end", OnCutsceneEnd);
		Messenger.RemoveListener("battle_start", OnBattleStart);
		Messenger.RemoveListener("battle_end", OnBattleEnd);
	}

	protected void OnCutsceneStart(CutsceneController.CutsceneType p_cutscene)
	{
		if (base.enabled)
		{
			m_active = false;
			if (null != m_skillTimer && m_skillTimer.Counter != 0f)
			{
				m_skillTimer.Pause(true);
			}
		}
	}

	protected void OnCutsceneEnd(CutsceneController.CutsceneType p_cutscene)
	{
		if (!base.enabled || null == m_skillTimer)
		{
			return;
		}
		m_active = skillEffectOn;
		if (p_cutscene == CutsceneController.CutsceneType.SKILL_HYDRA_ARMOR_SELF)
		{
			if (null != base.player && null != m_buffFxPrefab)
			{
				skillEffectOn = true;
				m_skillTimer.CountDown(m_duration);
			}
		}
		else if (m_skillTimer.Counter != 0f)
		{
			m_skillTimer.Pause(false);
		}
	}

	private void OnBattleStart()
	{
		if (base.enabled)
		{
			m_active = skillEffectOn;
		}
	}

	private void OnBattleEnd()
	{
		if (base.enabled)
		{
			m_active = false;
		}
	}

	private void Update()
	{
		if (null == base.player || base.player.Data == null)
		{
			return;
		}
		if (base.player.Data.Hp == 0f)
		{
			if (skillEffectOn)
			{
				skillEffectOn = false;
			}
		}
		else if (skillEffectOn)
		{
			if (m_active)
			{
				m_hpRegenerationRate = Mathf.Lerp(maxHpRegeneratingRate, minHpRegeneratingRate, base.player.Data.HpPercent);
				base.player.AttackSpeedModifier = Mathf.Lerp(maxSpeedModifier, 1f, base.player.Data.HpPercent);
				base.player.Data.HpPercent += m_hpRegenerationRate * Time.deltaTime;
				if (m_skillTimer.Counter == 0f)
				{
					skillEffectOn = false;
				}
			}
		}
		else if (!m_skillTriggered && triggerCondition >= base.player.Data.HpPercent && null != SingletonComponent<CutsceneController>.Instance)
		{
			StartCoroutine(TriggerSkill());
		}
	}

	private IEnumerator TriggerSkill()
	{
		while (!base.player.GetComponent<Animation>().enabled)
		{
			yield return null;
		}
		if (!(null == base.player) && base.player.Data != null && base.player.Data.Hp != 0f)
		{
			m_skillTriggered = true;
			StartCoroutine(SingletonComponent<CutsceneController>.Instance.PlayCutscene(CutsceneController.CutsceneType.SKILL_HYDRA_ARMOR_SELF, OnCutsceneFailed));
		}
	}

	private void OnCutsceneFailed(CutsceneController.CutsceneType p_cutscene)
	{
		if (p_cutscene == CutsceneController.CutsceneType.SKILL_HYDRA_ARMOR_SELF)
		{
			m_skillTriggered = false;
		}
	}
}
