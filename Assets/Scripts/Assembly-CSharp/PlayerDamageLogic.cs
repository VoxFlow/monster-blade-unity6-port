using UnityEngine;

public class PlayerDamageLogic : BattleLogic
{
	protected MonsterCollisionData m_collData;

	protected DamageData m_damage = new DamageData();

	protected bool m_blockSucceed;

	protected bool m_perfectDefense;

	protected bool m_heavyBlock;

	protected bool m_cutscenePlaying;

	protected bool m_retreating;

	protected NBTimer m_invincibleTimer;

	protected DamageBarBehavior m_healthBar;

	protected float m_blockDamageMod = 0.25f;

	protected override void Awake()
	{
		base.Awake();
		m_healthBar = base.gameObject.AddComponent<DamageBarBehavior>();
		m_invincibleTimer = base.gameObject.AddComponent<NBTimer>();
		m_invincibleTimer.Name = "Invincible Time";
		Messenger.AddListener("battle_exit", OnPlayerRetreat);
		Messenger.AddListener("normal_block", OnNormalBlock);
		Messenger.AddListener("perfect_block", OnPerfectBlock);
		Messenger.AddListener("block_failed", OnBlockFailed);
		Messenger<Player.DodgeDirection>.AddListener("perfect_dodge_start", OnPerfectDodge);
		Messenger<MonsterCollisionData>.AddListener("player_get_hit", OnPlayerGetHit);
		Messenger<PlayerCollisionData>.AddListener("monster_get_hit", OnMonsterGetHit);
		Messenger.AddListener("perfect_block_end", OnPerfectBlockEnd);
		Messenger.AddListener("perfect_dodge_end", OnPerfectDodgeEnd);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Messenger.RemoveListener("battle_exit", OnPlayerRetreat);
		Messenger.RemoveListener("normal_block", OnNormalBlock);
		Messenger.RemoveListener("perfect_block", OnPerfectBlock);
		Messenger.RemoveListener("block_failed", OnBlockFailed);
		Messenger<Player.DodgeDirection>.RemoveListener("perfect_dodge_start", OnPerfectDodge);
		Messenger<MonsterCollisionData>.RemoveListener("player_get_hit", OnPlayerGetHit);
		Messenger<PlayerCollisionData>.RemoveListener("monster_get_hit", OnMonsterGetHit);
		Messenger.RemoveListener("perfect_block_end", OnPerfectBlockEnd);
		Messenger.RemoveListener("perfect_dodge_end", OnPerfectDodgeEnd);
	}

	protected override void OnBattleStart()
	{
		base.OnBattleStart();
		GameObject[] array = GameObject.FindGameObjectsWithTag("HealthBar");
		if (m_healthBar != null && array != null)
		{
			GameObject[] array2 = array;
			foreach (GameObject gameObject in array2)
			{
				if (gameObject.name == "playerHealthBar")
				{
					m_healthBar.ForegroundSlider = gameObject.GetComponent<SliderSmoothExpand>();
				}
				if (gameObject.name == "playerHealthBarBG")
				{
					m_healthBar.BackgroundSlider = gameObject.GetComponent<SliderSmoothExpand>();
				}
			}
			m_healthBar.WatchingData = m_player.Data;
		}
		m_perfectDefense = false;
	}

	protected override void OnCutsceneStart(CutsceneController.CutsceneType p_type)
	{
		base.OnCutsceneStart(p_type);
		m_cutscenePlaying = true;
		m_perfectDefense = false;
	}

	protected override void OnCutsceneFinish(CutsceneController.CutsceneType p_type)
	{
		base.OnCutsceneFinish(p_type);
		m_cutscenePlaying = false;
	}

	protected override void OnPause(bool p_flag)
	{
		base.OnPause(p_flag);
		m_healthBar.enabled = !p_flag;
	}

	protected virtual void OnPlayerGetHit(MonsterCollisionData p_collData)
	{
		if (m_player.Data.Hp != 0f)
		{
			m_collData = p_collData;
		}
	}

	protected virtual void OnMonsterGetHit(PlayerCollisionData p_collData)
	{
		m_perfectDefense = false;
	}

	protected virtual void OnPlayerRetreat()
	{
		m_retreating = true;
	}

	protected virtual void OnNormalBlock()
	{
		m_blockSucceed = true;
		m_perfectDefense = false;
		m_heavyBlock = false;
	}

	protected virtual void OnPerfectBlock()
	{
		m_blockSucceed = true;
		m_perfectDefense = true;
		m_heavyBlock = false;
	}

	protected virtual void OnBlockFailed()
	{
		if (m_collData != null && m_collData.attack.type == MonsterAttack.Type.Heavy)
		{
			m_heavyBlock = true;
		}
	}

	protected virtual void OnPerfectDodge(Player.DodgeDirection p_dir)
	{
		if (m_player.Data.Hp != 0f)
		{
			m_perfectDefense = true;
		}
	}

	protected virtual void OnPerfectBlockEnd()
	{
		m_perfectDefense = false;
	}

	protected virtual void OnPerfectDodgeEnd()
	{
		m_perfectDefense = false;
	}

	protected override void LateUpdate()
	{
		if (!m_retreating && m_collData != null && m_monster.Data.Hp > 0f && !m_cutscenePlaying)
		{
			CalculatePlayerDamage();
			m_damage.finalDamage = (int)m_damage.finalDamage;
			DealDamageToPlayer();
			DeterminePlayerReaction();
			BroadcastEvent();
			m_collData = null;
			m_blockSucceed = false;
			m_heavyBlock = false;
		}
	}

	protected virtual void CalculatePlayerDamage()
	{
		float num = ((!m_player.Invincible) ? m_collData.attack.multiplier : 0f);
		if (m_perfectDefense)
		{
			num = 0f;
			m_damage.damageType = DamageType.Block;
		}
		else if (Mathf.Approximately(m_collData.directDamage, 0f))
		{
			bool p_isCritical = false;
			BattleDamageCalculator.CalculateDamage(m_collData.attackerData, m_player.Data, true, m_collData.ignoreArmour, out m_damage.rawDamage, out m_damage.finalDamage, out p_isCritical);
			m_player.ComboRecognitionTimer.Stop();
			if (m_blockSucceed)
			{
				num = ((m_collData.attack.type != MonsterAttack.Type.Roar) ? (num * m_blockDamageMod) : 0f);
				m_damage.damageType = DamageType.Block;
			}
			else
			{
				if (p_isCritical)
				{
					m_damage.damageType = DamageType.Critical;
				}
				else
				{
					m_damage.damageType = DamageType.Normal;
				}
				if (m_heavyBlock)
				{
					num *= m_blockDamageMod;
				}
			}
		}
		else
		{
			m_damage.rawDamage = (m_damage.finalDamage = m_collData.directDamage);
			m_damage.damageType = DamageType.Critical;
		}
		m_damage.rawDamage *= num;
		m_damage.finalDamage *= num;
	}

	protected virtual void DeterminePlayerReaction()
	{
		if (m_perfectDefense)
		{
			return;
		}
		m_blockSucceed |= m_player.Invincible;
		if (m_blockSucceed)
		{
			m_player.IsReady = true;
		}
		else if (m_collData.attack.knockSpeed <= 0f)
		{
			if (m_player.Data.Hp > 0f)
			{
				string currentAnimName = m_player.AnimPlayer.CurrentAnimName;
				Player.HitReact p_hit = Player.HitReact.Bottom;
				bool flag = currentAnimName != "Tumble";
				flag &= m_damage.damageType == DamageType.Critical || m_damage.finalDamage >= 0.25f * m_player.Data.MaxHp || m_player.AnimPlayer.CurrentAnimName.Contains("Strafe") || m_collData.attack.zone == MonsterAttack.Zone.Center || m_collData.attack.type == MonsterAttack.Type.Signature;
				if (m_collData.attack.zone != MonsterAttack.Zone.None && m_collData.attack.zone != MonsterAttack.Zone.Area)
				{
					p_hit = (Player.HitReact)m_collData.attack.zone;
				}
				if (flag)
				{
					m_player.AnimPlayer.Play("Tumble");
				}
				else if (m_collData.attack.type == MonsterAttack.Type.Heavy)
				{
					m_player.PlayHitHeavyAnim(p_hit);
				}
				else
				{
					m_player.PlayHitAnim(p_hit);
				}
				if (currentAnimName == "Tumble")
				{
					m_player.Invincible = true;
					m_invincibleTimer.CountDown(m_player.AnimPlayer.CurrentAnim.time, "OnInvincibleTimeEnd");
				}
				m_player.ChangeState(Player.State.Hit);
			}
		}
		else
		{
			KnockPlayer(m_collData.attack.knockSpeed);
		}
	}

	protected virtual void DealDamageToPlayer()
	{
		m_player.TakeDamage(m_damage.finalDamage);
	}

	protected virtual void BroadcastEvent()
	{
		m_damage.attacker = m_collData.attackerData;
		m_damage.defender = m_player.Data;
		Messenger<DamageData>.Invoke("player_receive_dmg", m_damage);
	}

	protected void OnInvincibleTimeEnd()
	{
		m_player.Invincible = false;
	}

	protected virtual void OnPlayerFinishKnockback()
	{
		Vector3 vector = SingletonComponent<BattleManager>.Instance.BattleReferencePoint - m_player.transform.forward * SingletonComponent<BattleManager>.Instance.BattleReferenceDistance;
		if (m_player.transform.position != vector)
		{
			m_player.RunTo(vector, 10f, OnPlayerFinishReturn);
		}
		else
		{
			OnPlayerFinishReturn();
		}
	}

	protected virtual void OnPlayerFinishReturn()
	{
		m_playerController.enabled = true;
		m_monsterAI.enabled = true;
		m_monster.Idle();
	}

	protected virtual void KnockPlayer(float p_speed)
	{
		m_monsterAI.enabled = false;
		m_playerController.enabled = false;
		m_player.KnockBack(p_speed, OnPlayerFinishKnockback);
		m_player.ChangeState(Player.State.Hit);
	}
}
