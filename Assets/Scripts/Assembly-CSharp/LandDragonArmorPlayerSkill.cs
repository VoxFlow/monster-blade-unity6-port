using UnityEngine;

public class LandDragonArmorPlayerSkill : ArmorSetPlayerSkill
{
	[SerializeField]
	protected PlayerBuffFx m_glowFxPrefab;

	[SerializeField]
	protected SelfDestroy m_geyserFxPrefab;

	[SerializeField]
	protected float m_particleScaleFactor = 1.1f;

	[SerializeField]
	protected float m_powerUpPerCharge = 0.2f;

	[SerializeField]
	protected float m_basePower = 1.5f;

	[SerializeField]
	protected int m_maxCharge = 4;

	protected ArmorSetTriggerPerfectDodge m_skillTriggerLogic;

	protected PlayerCollisionData m_playerCollisionData;

	protected PlayerBuffFx m_glowFx;

	protected float[] m_defaultParticleSizes;

	protected ParticleSystem[] m_fxParticles;

	protected bool m_specialAttack;

	protected bool m_bossBattle;

	protected float m_power = 1f;

	protected int m_powerUpCharge;

	protected ArmorSetTriggerPerfectDodge skillTriggerLogic
	{
		get
		{
			if (null == m_skillTriggerLogic)
			{
				m_skillTriggerLogic = base.gameObject.GetComponent<ArmorSetTriggerPerfectDodge>();
			}
			return m_skillTriggerLogic;
		}
	}

	protected void OnEnable()
	{
		Messenger<PlayerAttack>.AddListener("player_begin_atk", OnPlayerBeginAttack);
		Messenger<Player.DodgeDirection>.AddListener("perfect_dodge_start", OnPerfectDodgeStart);
		Messenger.AddListener("perfect_dodge_end", OnPerfectDodgeEnd);
		Messenger<DamageData>.AddListener("monster_receive_dmg", OnMonsterReceiveDamage);
		Messenger<MonsterCollisionData>.AddListener("player_get_hit", OnPlayerGetHit);
	}

	protected void OnDisable()
	{
		Messenger<PlayerAttack>.AddListener("player_begin_atk", OnPlayerBeginAttack);
		Messenger<Player.DodgeDirection>.RemoveListener("perfect_dodge_start", OnPerfectDodgeStart);
		Messenger.RemoveListener("perfect_dodge_end", OnPerfectDodgeEnd);
		Messenger<DamageData>.RemoveListener("monster_receive_dmg", OnMonsterReceiveDamage);
		Messenger<MonsterCollisionData>.RemoveListener("player_get_hit", OnPlayerGetHit);
	}

	protected override void OnBattleReady()
	{
		base.OnBattleReady();
		if (null != base.player && null != base.player.SwordCollider && null == m_playerCollisionData)
		{
			m_playerCollisionData = base.player.SwordCollider.gameObject.GetComponent<PlayerCollisionData>();
		}
		m_bossBattle = null != base.monster && base.monster.Data != null && base.monster.Data.m_monsterData.isBoss;
	}

	protected override void PreCutsceneInitialization()
	{
		if (null != base.monster)
		{
			base.monster.Invincible = true;
		}
	}

	protected override void PostCutsceneInitialization()
	{
		if (null != base.monster)
		{
			base.monster.Invincible = false;
		}
		Transform transform = base.player.transform.FindInHierarchy("Sword_Bone");
		if (!(null != m_glowFxPrefab) || !(null != transform))
		{
			return;
		}
		m_glowFx = PlayerBuffFx.InstantiateFx(m_glowFxPrefab.gameObject, base.player, "Sword_Bone", TransformAttachMode.RETAIN_LOCAL_TRANSFORM);
		m_fxParticles = MBHier.GetComponentsInChildren<ParticleSystem>(m_glowFx.gameObject);
		if (m_fxParticles != null)
		{
			m_defaultParticleSizes = new float[m_fxParticles.Length];
			for (int i = 0; i < m_fxParticles.Length; i++)
			{
				m_defaultParticleSizes[i] = m_fxParticles[i].startSize;
			}
		}
	}

	private void OnPerfectDodgeStart(Player.DodgeDirection p_dir)
	{
		if (!m_skillTriggered)
		{
			return;
		}
		m_powerUpCharge++;
		for (int i = 0; i < m_fxParticles.Length; i++)
		{
			if (null != m_fxParticles[i])
			{
				m_fxParticles[i].startSize = m_defaultParticleSizes[i] * (1f + m_particleScaleFactor * (float)m_powerUpCharge);
			}
		}
		if (m_bossBattle)
		{
			m_power *= m_basePower + m_powerUpPerCharge * (float)m_powerUpCharge * (float)m_powerUpCharge;
			m_specialAttack = true;
		}
		else
		{
			m_specialAttack = m_powerUpCharge >= m_maxCharge;
		}
	}

	private void OnPerfectDodgeEnd()
	{
		if (skillTriggered && null != skillTriggerLogic && null != m_playerCollisionData)
		{
			m_playerCollisionData.directDamage = 0f;
			m_playerCollisionData.attack.multiplier = 1f;
			if (m_powerUpCharge >= m_maxCharge)
			{
				ResetPowerUpCharge();
			}
			else
			{
				m_specialAttack = false;
			}
		}
	}

	private void OnPlayerBeginAttack(PlayerAttack p_attack)
	{
		if (m_specialAttack)
		{
			if (m_bossBattle)
			{
				m_playerCollisionData.attack.multiplier = m_power;
				return;
			}
			m_playerCollisionData.directDamage = 999999f;
			m_playerCollisionData.attack.multiplier = 1f;
		}
	}

	private void OnMonsterReceiveDamage(DamageData p_damage)
	{
		if (m_powerUpCharge < m_maxCharge)
		{
			return;
		}
		if (null != m_geyserFxPrefab)
		{
			SelfDestroy selfDestroy = Object.Instantiate(m_geyserFxPrefab) as SelfDestroy;
			if (null != selfDestroy)
			{
				selfDestroy.transform.AttachTo(base.player.transform, TransformAttachMode.RETAIN_LOCAL_TRANSFORM);
			}
		}
		ResetParticleSize();
	}

	private void OnPlayerGetHit(MonsterCollisionData p_collData)
	{
		if (skillTriggered)
		{
			ResetPowerUpCharge();
			ResetParticleSize();
		}
	}

	private void ResetPowerUpCharge()
	{
		m_powerUpCharge = 0;
		m_power = 1f;
		m_specialAttack = false;
	}

	private void ResetParticleSize()
	{
		if (m_fxParticles == null)
		{
			return;
		}
		for (int i = 0; i < m_fxParticles.Length; i++)
		{
			if (null != m_fxParticles[i])
			{
				m_fxParticles[i].startSize = m_defaultParticleSizes[i];
			}
		}
	}
}
