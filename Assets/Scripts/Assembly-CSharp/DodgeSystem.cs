using UnityEngine;

public class DodgeSystem : SingletonComponent<DodgeSystem>
{
	public enum DodgeResult
	{
		None = -1,
		Normal = 0,
		Focus = 1,
		Ripost = 2,
		Signature = 3
	}

	protected Player m_player;

	protected Monster m_monster;

	protected AIController m_monsterAI;

	protected DodgeResult m_dodgeResult = DodgeResult.None;

	protected MonsterAttack m_monsterAttack;

	protected bool m_ripostAttack;

	protected float m_strafeStaminaCost = 0.26f;

	public bool m_superTough;

	public float m_stunTime = 4f;

	public static readonly float defaultPerfectDodgePeriod = 0.25f;

	public static float perfectDodgePeriod = defaultPerfectDodgePeriod;

	public static bool alwaysPerfectDodge = false;

	protected virtual void Awake()
	{
		m_superTough = false;
		m_player = SingletonComponent<StageManager>.Instance.PlayerObject.GetComponent<Player>();
		Messenger.AddListener("battle_ready", OnBattleReady);
		Messenger.AddListener("battle_end", OnBattleEnd);
		Messenger.AddListener("player_land", OnPlayerLand);
		Messenger.AddListener("monster_attack_wind_down", OnMonsterWindDown);
		Messenger<MonsterAttack>.AddListener("monster_begin_atk", OnMonsterBeginAttack);
		Messenger<DamageData>.AddListener("monster_receive_dmg", OnMonsterReceiveDamage);
		Messenger<PlayerController.Button>.AddListener("battle_ui_button_pressed", OnUiButtonPressed);
		Messenger<CutsceneController.CutsceneType>.AddListener(CutsceneMessage.CUTSCENE_START, OnCutsceneStart);
		Messenger.AddListener("monster_end_atk", OnMonsterFinishAttack);
		Messenger<PlayerAttack>.AddListener("player_finish_atk", OnPlayerFinishAttack);
	}

	protected new virtual void OnDestroy()
	{
		Messenger.RemoveListener("battle_ready", OnBattleReady);
		Messenger.RemoveListener("battle_end", OnBattleEnd);
		Messenger.RemoveListener("player_land", OnPlayerLand);
		Messenger.RemoveListener("monster_attack_wind_down", OnMonsterWindDown);
		Messenger<MonsterAttack>.RemoveListener("monster_begin_atk", OnMonsterBeginAttack);
		Messenger<DamageData>.RemoveListener("monster_receive_dmg", OnMonsterReceiveDamage);
		Messenger<PlayerController.Button>.RemoveListener("battle_ui_button_pressed", OnUiButtonPressed);
		Messenger<CutsceneController.CutsceneType>.RemoveListener(CutsceneMessage.CUTSCENE_START, OnCutsceneStart);
		Messenger.RemoveListener("monster_end_atk", OnMonsterFinishAttack);
		Messenger<PlayerAttack>.RemoveListener("player_finish_atk", OnPlayerFinishAttack);
	}

	protected virtual void RipostDodgeLogic(Player.DodgeDirection p_dir)
	{
		Dbg.Log("Perfect Dodge!!");
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_player_perfect_dodge", 1f);
		m_player.RipostDodge(p_dir);
		EnableSkill(false);
		m_monster.bodyCollider.enabled = true;
		m_monster.ChangeState(Monster.State.Idle);
		m_monsterAI.enabled = false;
		m_ripostAttack = true;
	}

	protected virtual void StrafeLogic(Player.DodgeDirection p_dir)
	{
		Dbg.Log("Dodge!!");
		m_player.Strafe(p_dir);
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_player_roll");
	}

	protected bool IsDodgeSuccess(Player.DodgeDirection p_dodgeDir, MonsterAttack p_atk, MonsterAttackParameter p_atkParam, AnimationState p_atkAnim)
	{
		if (p_atk == null)
		{
			Dbg.LogWarn("DodgeSystem.IsDodgeSuccess. p_atk is null!");
			return false;
		}
		if (p_atk.comboIndex <= 0 || p_atk.comboIndex > p_atkParam.damageWindows.Length || p_atk.comboIndex > p_atkParam.dodgeWindowStart.Length || p_atk.comboIndex > p_atkParam.dodgeWindowEnd.Length)
		{
			Dbg.LogWarn("Combo index is out of damage and/or dodge window parameter length.");
			return false;
		}
		bool flag = false;
		int num = p_atk.comboIndex - 1;
		if (p_atk.zone != MonsterAttack.Zone.None && p_atk.zone != MonsterAttack.Zone.Area)
		{
			flag = p_atk.zone == MonsterAttack.Zone.Center || p_atk.zone == (MonsterAttack.Zone)(p_dodgeDir + 1);
			float num2 = p_atkParam.dodgeWindowStart[num] / p_atkAnim.length;
			float num3 = p_atkParam.dodgeWindowEnd[num] / p_atkAnim.length;
			flag &= p_atkAnim.normalizedTime >= num2 && p_atkAnim.normalizedTime <= num3;
		}
		return flag | alwaysPerfectDodge;
	}

	protected bool DetectPerfectDodge(MonsterAttack p_atk, MonsterAttackParameter p_atkParam, AnimationState p_atkAnim)
	{
		if (m_monster == null || p_atk == null || p_atkParam == null || p_atkAnim == null)
		{
			return false;
		}
		if (p_atk.comboIndex <= 0 || p_atk.comboIndex > p_atkParam.perfectDodgeWindowStart.Length || p_atk.comboIndex > p_atkParam.dodgeWindowEnd.Length)
		{
			Dbg.LogWarn("Combo index is out of perfect dodge window parameter length.");
			return false;
		}
		float lookAngle = m_monster.transform.GetLookAngle(m_player.transform);
		float num = Mathf.Abs(lookAngle);
		bool flag = false;
		if (num > 60f)
		{
			flag = false;
		}
		else if (Mathf.Abs(lookAngle) <= 7.5f)
		{
			flag = true;
		}
		else if (m_monsterAttack.zone == MonsterAttack.Zone.Left && lookAngle >= 0f && lookAngle < 60f)
		{
			flag = true;
		}
		else if (m_monsterAttack.zone == MonsterAttack.Zone.Right && lookAngle <= 0f && lookAngle > -60f)
		{
			flag = true;
		}
		int num2 = p_atk.comboIndex - 1;
		float num3 = (p_atkParam.dodgeWindowEnd[num2] - perfectDodgePeriod) / p_atkAnim.length;
		float num4 = p_atkParam.dodgeWindowEnd[num2] / p_atkAnim.length;
		return p_atkAnim.normalizedTime >= num3 && p_atkAnim.normalizedTime <= num4 && flag;
	}

	protected void OnUiButtonPressed(PlayerController.Button p_button)
	{
		if (m_player.Stamina == 0f)
		{
			m_player.Exhaust();
		}
		else
		{
			if ((p_button != PlayerController.Button.LeftArrow && p_button != PlayerController.Button.RightArrow) || !m_player.CanStrafe())
			{
				return;
			}
			Player.DodgeDirection dodgeDirection = ((p_button != PlayerController.Button.LeftArrow) ? Player.DodgeDirection.Right : Player.DodgeDirection.Left);
			MonsterAttackParameter p_atkParam = null;
			bool flag = false;
			if (m_monsterAttack != null)
			{
				p_atkParam = m_monster.GetAttackParameter(m_monsterAttack.animationName);
				flag = IsDodgeSuccess(dodgeDirection, m_monsterAttack, p_atkParam, m_monster.AnimPlayer.CurrentAnim);
			}
			m_dodgeResult = DodgeResult.None;
			if (flag)
			{
				if (DetectPerfectDodge(m_monsterAttack, p_atkParam, m_monster.AnimPlayer.CurrentAnim) || alwaysPerfectDodge)
				{
					m_dodgeResult = DodgeResult.Ripost;
				}
				else
				{
					m_dodgeResult = DodgeResult.Normal;
				}
				m_player.DodgeSuccess = true;
			}
			switch (m_dodgeResult)
			{
			case DodgeResult.Ripost:
				RipostDodgeLogic(dodgeDirection);
				break;
			case DodgeResult.Normal:
			case DodgeResult.Focus:
				StrafeLogic(dodgeDirection);
				break;
			case DodgeResult.None:
				StrafeLogic(dodgeDirection);
				break;
			}
			if (!m_superTough)
			{
				m_player.Stamina -= m_strafeStaminaCost;
			}
		}
	}

	protected virtual void OnMonsterFinishAttack()
	{
		if (!m_ripostAttack)
		{
			m_player.DodgeSuccess = false;
		}
		m_monsterAttack = null;
	}

	protected virtual void OnPlayerLand()
	{
		if (m_dodgeResult != DodgeResult.Ripost)
		{
			m_dodgeResult = DodgeResult.None;
			m_player.DodgeSuccess = false;
		}
		m_monsterAttack = null;
	}

	protected void OnPlayerGetHit(MonsterCollisionData p_collData)
	{
		if (m_ripostAttack)
		{
			m_monsterAI.enabled = true;
			m_ripostAttack = false;
		}
	}

	protected void OnMonsterBeginAttack(MonsterAttack p_attack)
	{
		m_monsterAttack = p_attack;
		if (!m_ripostAttack)
		{
			m_player.DodgeSuccess = false;
		}
	}

	protected void OnMonsterWindDown()
	{
		m_monsterAttack = null;
	}

	protected void OnMonsterReceiveDamage(DamageData p_dmg)
	{
		if (m_ripostAttack && p_dmg.damageType != DamageType.Special)
		{
			OnRipostAttackEnd();
		}
	}

	protected virtual void OnBattleReady()
	{
		m_monster = SingletonComponent<StageManager>.Instance.EnemyObject.GetComponent<Monster>();
		m_monsterAI = SingletonComponent<StageManager>.Instance.EnemyObject.GetComponent<AIController>();
	}

	protected virtual void OnBattleEnd()
	{
		m_ripostAttack = false;
		EnableSkill(true);
	}

	protected virtual void OnCutsceneStart(CutsceneController.CutsceneType p_type)
	{
		m_monsterAttack = null;
	}

	protected virtual void OnRipostAttackEnd()
	{
		if (m_monster != null && m_monster.Data.Hp > 0f)
		{
			if (m_monster.CurrentState == Monster.State.Hit)
			{
				StartCoroutine(SingletonComponent<StunLogic>.Instance.StunMonsterAfterHit(m_stunTime, delegate
				{
					m_monsterAI.enabled = true;
				}));
			}
			else if (m_monsterAI != null)
			{
				m_monsterAI.enabled = true;
			}
		}
		EnableSkill(true);
		m_player.DodgeSuccess = false;
		m_ripostAttack = false;
	}

	protected virtual void OnPlayerFinishAttack(PlayerAttack p_attack)
	{
		if (m_ripostAttack)
		{
			OnRipostAttackEnd();
		}
	}

	protected virtual void Update()
	{
		if (m_ripostAttack && !m_monster.AnimPlayer.IsPlaying)
		{
			m_monster.AnimPlayer.Play("Idle_01");
		}
	}

	private void EnableSkill(bool p_enable)
	{
		SuperAttackSystem instance = SingletonComponent<SuperAttackSystem>.Instance;
		if (null != instance && null != instance.superAttackLogic)
		{
			if (p_enable)
			{
				instance.superAttackLogic.data.EnableSkill(this);
			}
			else
			{
				instance.superAttackLogic.data.DisableSkill(this);
			}
		}
		SkillSystem instance2 = SingletonComponent<SkillSystem>.Instance;
		if (null != instance2 && null != instance2.friendSkill)
		{
			if (p_enable)
			{
				instance2.friendSkill.skillData.EnableSkill(this);
			}
			else
			{
				instance2.friendSkill.skillData.DisableSkill(this);
			}
		}
		BlockSystem.enablePerfectBlock = p_enable;
	}
}
