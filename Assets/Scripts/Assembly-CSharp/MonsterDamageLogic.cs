using System.Collections;
using UnityEngine;

public class MonsterDamageLogic : BattleLogic
{
	protected float m_stunTime = 4f;

	protected PlayerCollisionData m_collData;

	protected DamageData m_damage = new DamageData();

	protected DamageBarBehavior m_healthBar;

	protected bool m_retreating;

	protected static float m_animationTreshold = 0.6f;

	protected static float m_criticalInteruptChance = 0.25f;

	protected override void Awake()
	{
		base.Awake();
		m_healthBar = AddDamageBarComponent();
		Messenger.AddListener("battle_exit", OnPlayerRetreat);
		Messenger<PlayerCollisionData>.AddListener("monster_get_hit", OnMonsterGetHit);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Messenger.RemoveListener("battle_exit", OnPlayerRetreat);
		Messenger<PlayerCollisionData>.RemoveListener("monster_get_hit", OnMonsterGetHit);
	}

	protected override void OnBattleStart()
	{
		base.OnBattleStart();
		GameObject[] array = GameObject.FindGameObjectsWithTag("HealthBar");
		if (!(m_healthBar != null) || array == null || m_healthBar.WatchingData != null)
		{
			return;
		}
		GameObject[] array2 = array;
		foreach (GameObject gameObject in array2)
		{
			if (gameObject.name == "monsterHealthBar")
			{
				m_healthBar.ForegroundSlider = gameObject.GetComponent<SliderSmoothExpand>();
			}
			if (gameObject.name == "monsterHealthBarBG")
			{
				m_healthBar.BackgroundSlider = gameObject.GetComponent<SliderSmoothExpand>();
			}
		}
		m_healthBar.WatchingData = m_monster.Data;
	}

	protected override void OnPause(bool p_flag)
	{
		base.OnPause(p_flag);
		m_healthBar.enabled = !p_flag;
	}

	protected virtual DamageBarBehavior AddDamageBarComponent()
	{
		return base.gameObject.AddComponent<DamageBarBehavior>();
	}

	protected virtual void OnPlayerRetreat()
	{
		m_retreating = true;
	}

	protected virtual void OnMonsterGetHit(PlayerCollisionData p_collData)
	{
		if (m_monster.Data.Hp != 0f)
		{
			Dbg.Log("MonsterDamageLogic:OnMonsterGetHit");
			m_collData = p_collData;
		}
	}

	protected override void LateUpdate()
	{
		if (!m_retreating && m_monster != null && m_collData != null && m_player.Data.Hp > 0f)
		{
			CalculateMonsterDamage();
			m_damage.finalDamage = (int)m_damage.finalDamage;
			DealDamageToMonster(m_damage);
			DetermineMonsterReaction();
			BroadcastEvent();
			m_collData = null;
		}
	}

	protected virtual void CalculateMonsterDamage()
	{
		bool p_isCritical = false;
		bool flag = m_monster.IsBlocking() || m_monster.Invincible;
		PlayerAttack attack = m_collData.attack;
		if (attack == null)
		{
			Dbg.LogWarn("m_collData.attack is null");
			m_damage.rawDamage = (m_damage.finalDamage = 0f);
			return;
		}
		float num = attack.multiplier;
		if (Mathf.Approximately(m_collData.directDamage, 0f))
		{
			bool p_ignoreCritical = !attack.CanCritical();
			BattleDamageCalculator.CalculateDamage(m_collData.attackerData, m_monster.Data, p_ignoreCritical, m_collData.ignoreArmour, out m_damage.rawDamage, out m_damage.finalDamage, out p_isCritical);
			if (flag)
			{
				num = 0f;
			}
			m_damage.rawDamage *= num;
			num = 1f;
		}
		else
		{
			m_damage.rawDamage = m_collData.directDamage;
		}
		if (attack.type != PlayerAttack.Type.Special)
		{
			num /= m_monster.Data.m_monsterData.resistances[(int)m_collData.attackerData.AttackElement];
		}
		if (m_collData.attack.type == PlayerAttack.Type.None)
		{
			m_damage.damageType = DamageType.None;
		}
		else if (m_collData.attack.type == PlayerAttack.Type.Skill)
		{
			m_damage.damageType = DamageType.Special;
		}
		else if (flag)
		{
			m_damage.damageType = DamageType.Block;
		}
		else if (p_isCritical)
		{
			m_damage.damageType = DamageType.Critical;
		}
		else if (attack.type != PlayerAttack.Type.Normal)
		{
			m_damage.damageType = DamageType.Heavy;
		}
		else if (m_collData.attack.type != PlayerAttack.Type.None)
		{
			m_damage.damageType = DamageType.Normal;
		}
		m_damage.finalDamage = m_damage.rawDamage * num;
		m_damage.attacker = m_collData.attackerData;
		m_damage.defender = m_monster.Data;
	}

	protected virtual void DetermineMonsterReaction()
	{
		PlayerAttack attack = m_collData.attack;
		if (attack == null || m_damage.damageType == DamageType.Block)
		{
			return;
		}
		bool flag = m_damage.damageType == DamageType.Critical || attack.type == PlayerAttack.Type.Ripost || attack.animation == "Cutscene_HeavyAttack";
		if (m_monster.Data.Hp == 0f)
		{
			AnimationState animationState = m_monster.AnimPlayer.Play("Death");
			animationState.speed = 0.5f;
		}
		else if (NeedToPlayHitAnimation(attack, m_monster.Data.m_monsterData.isBoss))
		{
			m_monster.ChangeState(Monster.State.Hit);
			if (flag)
			{
				m_monster.PlayHitHeavyAnim(attack.direction);
			}
			else if (attack.type == PlayerAttack.Type.Heavy)
			{
				m_monster.PlayHitHeavyAnim(attack.direction);
			}
			else
			{
				m_monster.PlayHitAnim(attack.direction);
			}
		}
	}

	public virtual void DealDamageToMonster(DamageData p_damage)
	{
		if (m_player.Data.Hp != 0f)
		{
			m_monster.TakeDamage(p_damage.finalDamage);
		}
	}

	protected virtual void BroadcastEvent()
	{
		Messenger<DamageData>.Invoke("monster_receive_dmg", m_damage);
	}

	protected virtual IEnumerator MakeMonsterStun()
	{
		Dbg.Log("MonsterDamageLogic:MakeMonsterStun()");
		while (m_monster.AnimPlayer.IsPlaying && m_monster.AnimPlayer.GetCurrentAnimNormalizeTime() < m_animationTreshold)
		{
			yield return null;
		}
		if (m_monster.Data.Hp > 0f)
		{
			m_monster.Idle();
			m_monster.AnimPlayer.Play("Idle_01");
			SingletonComponent<StunLogic>.Instance.StunMonster(m_stunTime);
		}
	}

	protected virtual bool NeedToPlayHitAnimation(PlayerAttack p_attack, bool p_isBoss)
	{
		if (!m_monster.AnimPlayer.enabled)
		{
			return false;
		}
		float num = Random.Range(0f, 1f);
		bool flag = m_damage.damageType == DamageType.Critical && num < m_criticalInteruptChance;
		StunLogic instance = SingletonComponent<StunLogic>.Instance;
		bool flag2 = instance != null && instance.IsMonsterStunning();
		bool flag3 = false;
		flag3 = ((!p_isBoss) ? (m_monster.IsReachableFromCurrent(Monster.State.Hit) && !m_monster.IsTurning() && m_monster.CurrentState != Monster.State.Attack && (m_monsterAI.IdleTimer.Counter > 0f || m_monster.CurrentAttack == null || m_monster.CurrentAttack.IsInteruptable() || flag2)) : flag2);
		return flag3 || p_attack.DoesMonsterNeedToReact() || flag;
	}
}
