using System;
using System.Collections;
using UnityEngine;

public abstract class SuperAttack : BattleLogic
{
	[SerializeField]
	private SuperAttackData m_data;

	[SerializeField]
	private float m_figureOfEightMomentum = 0.1f;

	[SerializeField]
	private float m_nonFigureOfEightMomentum = 0.0255f;

	protected bool m_monsterHit;

	protected bool m_chargableAttack;

	public static bool infiniteSuper;

	public static bool Playing { get; private set; }

	public SuperAttackData data
	{
		get
		{
			return m_data;
		}
	}

	protected float figureOfEightMomentum
	{
		get
		{
			return m_figureOfEightMomentum;
		}
	}

	protected float nonFigureOfEightMomentum
	{
		get
		{
			return m_nonFigureOfEightMomentum;
		}
	}

	protected virtual void OnEnable()
	{
		Messenger.AddListener("battle_exit", OnExitBattle);
		Messenger<PlayerCollisionData>.AddListener("monster_get_hit", OnMonsterGetHit);
		Messenger<DamageData>.AddListener("monster_receive_dmg", OnMonsterReceiveDamage);
	}

	protected virtual void OnDisable()
	{
		Messenger.RemoveListener("battle_exit", OnExitBattle);
		Messenger<PlayerCollisionData>.RemoveListener("monster_get_hit", OnMonsterGetHit);
		Messenger<DamageData>.RemoveListener("monster_receive_dmg", OnMonsterReceiveDamage);
	}

	protected virtual void OnExitBattle()
	{
		StopCoroutine("TriggerSuperAttack");
		StopCoroutine("PlayAttackSequences");
	}

	public virtual void PrebattleInit(PlayerData p_data)
	{
	}

	protected override void OnBattleReady()
	{
	}

	protected override void OnBattleEnd()
	{
		base.OnBattleEnd();
		m_monsterHit = false;
		Playing = false;
	}

	public void OnTrigger()
	{
		if (base.enabled)
		{
			StartCoroutine("TriggerSuperAttack");
		}
	}

	protected virtual void OnMonsterGetHit(PlayerCollisionData p_collisionData)
	{
		m_chargableAttack = false;
		if (p_collisionData != null && p_collisionData.attack != null)
		{
			if (p_collisionData.attack.type == PlayerAttack.Type.Super)
			{
				m_monsterHit = true;
			}
			else if (p_collisionData.attack.type == PlayerAttack.Type.Normal || p_collisionData.attack.type == PlayerAttack.Type.Heavy)
			{
				m_chargableAttack = true;
			}
		}
	}

	protected virtual void OnMonsterReceiveDamage(DamageData p_damage)
	{
		if (p_damage.damageType != DamageType.Block && m_chargableAttack)
		{
			ChargeMomentum();
		}
		if (m_monsterHit && p_damage.damageType != DamageType.Block)
		{
			if (m_monster.IsTurning())
			{
				m_monster.StopTurn();
				m_monster.GetComponent<Animation>().Stop();
			}
			m_monsterAI.enabled = false;
		}
	}

	public void ChargeMomentum()
	{
		if (!(null == m_player))
		{
			if (m_player.FigureOfEight)
			{
				data.momentum += figureOfEightMomentum;
			}
			else
			{
				data.momentum += nonFigureOfEightMomentum;
			}
			m_chargableAttack = false;
		}
	}

	public bool CanTriggerSuperAttack()
	{
		return m_player.Stamina > 0f && m_player.CanBecomeIdle() && (m_player.CurrentState == Player.State.Idle || m_player.CurrentState == Player.State.Attack);
	}

	protected virtual IEnumerator TriggerSuperAttack()
	{
		Dbg.Log("TriggerSuperAttack");
		if (m_player == null || m_monster == null)
		{
			yield break;
		}
		m_playerController.enabled = false;
		Playing = true;
		while (!CanTriggerSuperAttack() || !m_player.CanAttack())
		{
			yield return null;
		}
		if (m_player.Data.Hp == 0f || m_monster.Data.Hp == 0f)
		{
			Playing = false;
			yield break;
		}
		data.momentum = 0f;
		m_player.IsReady = false;
		m_player.ComboRecognitionTimer.Pause(true);
		Messenger.Invoke("player_super_atk_begin");
		yield return StartCoroutine("PlayAttackSequences");
		if (m_player.CurrentState == Player.State.Attack)
		{
			m_player.IsReady = true;
			if (SingletonComponent<StunLogic>.Instance != null && m_monster.Data.Hp > 0f)
			{
				Action onFailed = delegate
				{
					m_monsterAI.enabled = true;
				};
				StartCoroutine(SingletonComponent<StunLogic>.Instance.StunMonsterAfterHit(data.stunTime, onFailed));
			}
		}
		else
		{
			Dbg.Log("Super Attack was interrupted.");
			if (m_monster.Data.Hp > 0f)
			{
				m_monsterAI.enabled = true;
				m_monster.Idle();
			}
		}
		m_player.ComboRecognitionTimer.Pause(false);
		m_monsterHit = false;
		Playing = false;
		Messenger.Invoke("player_super_atk_end");
		if (!m_player.IsMoving() && m_monster.Data.Hp > 0f)
		{
			m_playerController.enabled = true;
		}
	}

	protected override void Update()
	{
		if (!(null == m_monster) && !(null == m_player))
		{
			if (infiniteSuper)
			{
				data.momentum = 1f;
			}
			if (m_monster.Data.Hp > 0f && m_monsterHit && m_monster != null && !m_monster.AnimPlayer.IsPlaying)
			{
				m_monster.AnimPlayer.Play("Stun");
			}
		}
	}

	protected abstract IEnumerator PlayAttackSequences();
}
