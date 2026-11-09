using UnityEngine;

public class MonsterPetrifiedLogic : PetrifiedLogic
{
	protected NBTimer m_shakeTimer;

	protected NBTimer[] m_monsterTimers;

	protected float m_pausedTime;

	protected bool m_hasDeathCutscene;

	public float shakeInterval { get; set; }

	public bool enableDeathCutscene { get; set; }

	protected override void Awake()
	{
		base.Awake();
		m_shakeTimer = base.gameObject.AddComponent<NBTimer>();
		m_shakeTimer.Name = "Shake Timer";
		enableDeathCutscene = false;
		Messenger<PlayerCollisionData>.AddListener("monster_get_hit", OnMonsterGetHit);
		Messenger.AddListener("player_super_atk_end", OnSuperAttackEnd);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Messenger<PlayerCollisionData>.RemoveListener("monster_get_hit", OnMonsterGetHit);
		Messenger.RemoveListener("player_super_atk_end", OnSuperAttackEnd);
	}

	public override void Init()
	{
		base.Init();
		m_shaker = m_monster.gameObject.GetComponent<MonsterShakeScript>();
		if (null != m_shaker)
		{
			m_shaker.m_amplitude = 0.01f;
			m_shaker.m_frequency = 4f;
			m_shaker.m_duration = 1f;
		}
		ShadowProjector componentInChildren = MBHier.GetComponentInChildren<ShadowProjector>(m_monster.gameObject);
		base.petrifyFx.shakeParticleRoot = componentInChildren.Reference;
		base.petrifyFx.targetRenderers = MBHier.GetComponentsInChildren<SkinnedMeshRenderer>(m_monster.gameObject);
		base.charData = m_monster.Data;
		m_monsterTimers = MBHier.GetComponentsInChildren<NBTimer>(m_monster.gameObject);
	}

	protected void OnMonsterGetHit(PlayerCollisionData p_collisionData)
	{
	}

	protected void OnSuperAttackEnd()
	{
		if (base.petrified && m_monster.Data.Hp != 0f)
		{
			base.timer.Pause(false);
			base.timer.ResetCounterTo(base.timer.Counter - m_pausedTime);
		}
	}

	public override void OnPetrified(float p_duration, float p_armourMod, Callback p_onBreak)
	{
		base.OnPetrified(p_duration, p_armourMod, p_onBreak);
		if (!(null == m_monster) && !(null == m_monsterAI))
		{
			if (null != m_shakeTimer)
			{
				m_shakeTimer.CountDown(shakeInterval, Shake);
			}
			m_monster.GetComponent<Animation>().enabled = false;
			m_monsterAI.enabled = false;
			if (!enableDeathCutscene)
			{
				m_monster.hasDeathCutscene = false;
				m_hasDeathCutscene = m_monster.hasDeathCutscene;
			}
			for (int i = 0; i < m_monsterTimers.Length; i++)
			{
				m_monsterTimers[i].Pause(true);
			}
			EnableFriendAssist(false);
			StunLogic instance = SingletonComponent<StunLogic>.Instance;
			if (null != instance)
			{
				instance.enabled = false;
			}
			Messenger.Invoke("monster_end_atk");
		}
	}

	public override void OnBreak()
	{
		base.OnBreak();
		if (!(null == m_monster) && !(null == m_monsterAI))
		{
			m_monsterAI.enabled = true;
			m_monster.GetComponent<Animation>().enabled = true;
			if (null != m_shaker)
			{
				m_shaker.Stop();
			}
			m_shakeTimer.Reset();
			if (!enableDeathCutscene)
			{
				m_monster.hasDeathCutscene = m_hasDeathCutscene;
			}
			for (int i = 0; i < m_monsterTimers.Length; i++)
			{
				m_monsterTimers[i].Pause(false);
			}
			StunLogic instance = SingletonComponent<StunLogic>.Instance;
			if (null != instance)
			{
				instance.enabled = true;
			}
			EnableFriendAssist(true);
		}
	}

	public override void OnShattered()
	{
		base.OnShattered();
		if (!(null == m_monster))
		{
			if (null != m_shaker)
			{
				m_shaker.Stop();
			}
			m_shakeTimer.Reset();
			m_monster.GetComponent<Animation>().enabled = true;
			if (m_monster.AnimPlayer.CurrentAnimName != "Death")
			{
				m_monster.AnimPlayer.Play("Death");
			}
			if (SingletonComponent<SlowMotionSystem>.Instance != null)
			{
				SingletonComponent<SlowMotionSystem>.Instance.LinearFade(4f, 1.5f);
			}
			StunLogic instance = SingletonComponent<StunLogic>.Instance;
			if (null != instance)
			{
				instance.enabled = true;
			}
			EnableFriendAssist(true);
		}
	}

	protected override void Shake()
	{
		base.Shake();
		m_shakeTimer.CountDown(shakeInterval, Shake);
	}

	protected override void OnUpdate()
	{
		if (!base.timer.enabled)
		{
			m_pausedTime += Time.deltaTime;
		}
		for (int i = 0; i < m_monsterTimers.Length; i++)
		{
			if (m_monsterTimers[i].enabled)
			{
				m_monsterTimers[i].Pause(true);
			}
		}
	}

	private void EnableFriendAssist(bool p_enable)
	{
		SkillSystem instance = SingletonComponent<SkillSystem>.Instance;
		if (null != instance && null != instance.friendSkill)
		{
			if (p_enable)
			{
				instance.friendSkill.skillData.EnableSkill(this);
			}
			else
			{
				instance.friendSkill.skillData.DisableSkill(this);
			}
		}
	}
}
