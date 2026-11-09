using System.Collections;
using UnityEngine;

public class WingChickenArmorPlayerSkill : PlayerSkill, ArmorSetSkill
{
	public PlayerBuffFx m_weaponBuffFxPrefab;

	private WingChickenArmorSkillLogic m_skillLogic;

	private float m_igniteChance = 0.12f;

	private float m_flameDamageMul = 0.25f;

	private float m_duration = 6f;

	private float m_interval = 1f;

	private float m_explosionDamageMul = 0.09f;

	private float m_skillTriggerParam = 0.18f;

	private bool m_isSkillTriggered;

	private PlayerBuffFx m_buffFx;

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
			return SkillEnum.ArmorSetSkill.WingChicken;
		}
	}

	public bool skillTriggered
	{
		get
		{
			return m_isSkillTriggered;
		}
	}

	public override bool DoesCharacterHaveSkill(PlayerData p_data)
	{
		return p_data.IsEquipingFullSet(GLOBAL.WINGCHICKEN_SETNAME);
	}

	protected override void Awake()
	{
		base.Awake();
		Messenger<PlayerCollisionData>.AddListener("monster_get_hit", OnMonsterGetHit);
		Messenger<CutsceneController.CutsceneType>.AddListener("cutscene_end", OnCutsceneEnd);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Messenger<PlayerCollisionData>.RemoveListener("monster_get_hit", OnMonsterGetHit);
		Messenger<CutsceneController.CutsceneType>.RemoveListener("cutscene_end", OnCutsceneEnd);
	}

	protected override void OnBattleReady()
	{
		base.OnBattleReady();
		if (base.enabled)
		{
			if (null == m_skillLogic)
			{
				m_skillLogic = base.gameObject.GetComponent<WingChickenArmorSkillLogic>();
			}
			if (null != base.monster && null != m_skillLogic)
			{
				m_skillLogic.enabled = true;
				m_skillLogic.explosionDamage = base.monster.Data.MaxHp * m_explosionDamageMul;
			}
		}
	}

	protected void OnMonsterGetHit(PlayerCollisionData p_collisionData)
	{
		if (base.enabled && m_isSkillTriggered && !(null == m_skillLogic) && !m_skillLogic.isBurning && !(null == p_collisionData) && p_collisionData.attack != null && p_collisionData.attack.type != PlayerAttack.Type.None && Random.Range(0f, 1f).CompareTo(m_igniteChance) <= 0)
		{
			m_skillLogic.Burn(base.monster.gameObject, base.player.Data, m_flameDamageMul, m_interval, m_duration);
			m_skillLogic.explosionDamage = base.monster.Data.MaxHp * m_explosionDamageMul;
		}
	}

	protected void OnCutsceneEnd(CutsceneController.CutsceneType p_cutscene)
	{
		if (base.enabled && p_cutscene == CutsceneController.CutsceneType.SKILL_WINGCHICKEN_ARMOR_SELF && null != base.player && null != m_weaponBuffFxPrefab)
		{
			m_buffFx = PlayerBuffFx.InstantiateFx(m_weaponBuffFxPrefab.gameObject, base.player, "Sword_Bone_01");
		}
	}

	protected void Update()
	{
		if (!(null == base.player) && base.player.Data != null && base.player.Data.Hp != 0f && !m_isSkillTriggered && base.player.Data.HpPercent <= m_skillTriggerParam)
		{
			CutsceneController instance = SingletonComponent<CutsceneController>.Instance;
			if (null != instance && instance.enabled)
			{
				StartCoroutine(TriggerSkill());
			}
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
			m_isSkillTriggered = true;
			StartCoroutine(SingletonComponent<CutsceneController>.Instance.PlayCutscene(CutsceneController.CutsceneType.SKILL_WINGCHICKEN_ARMOR_SELF, OnCutsceneFailed));
		}
	}

	private void OnCutsceneFailed(CutsceneController.CutsceneType p_cutscene)
	{
		if (p_cutscene == CutsceneController.CutsceneType.SKILL_WINGCHICKEN_ARMOR_SELF)
		{
			m_isSkillTriggered = false;
		}
	}
}
