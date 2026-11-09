using System;
using System.Collections;
using UnityEngine;

public class FirstCombatTutorial : Tutorial
{
	public enum Task
	{
		DodgeLeft = 0,
		DodgeRight = 1,
		Block = 2
	}

	private Player m_player;

	private PlayerController m_playerController;

	private Monster m_monster;

	private TutorialMonsterAi m_monsterAI;

	private TutorialSlashHint m_slashHint;

	private bool m_isTimeToDodge;

	private bool m_isDodgeTutorial;

	private bool m_isSuperReady;

	private bool m_disableControlAfterAttack;

	private NBTimer m_endTimer;

	private int m_slashCount;

	private BattleScreenUI m_battleUI;

	private SuperAttackData m_superData;

	private MonsterAttack m_currentAttack;

	protected override void OnEnable()
	{
		base.OnEnable();
		Messenger<PlayerAttack>.AddListener("player_begin_atk", OnPlayerAttack);
		Messenger.AddListener("player_block", OnPlayerBlock);
		Messenger.AddListener("battle_start", OnBattleStart);
		Messenger.AddListener("battle_end", OnBattleEnd);
		Messenger.AddListener("battle_exit", OnBattleExit);
		Messenger.AddListener("perfect_block", OnPerfectBlock);
		Messenger<MonsterCollisionData>.AddListener("player_get_hit", OnPlayerGetHit);
		Messenger<Player.DodgeDirection>.AddListener("perfect_dodge_start", OnPlayerStrafe);
		Messenger.AddListener("player_super_atk_begin", OnPlayerSuper);
		Messenger.AddListener("player_super_atk_end", OnPlayerSuperEnd);
		Messenger<SkillEnum.AssistSkill>.AddListener("friend_assist", OnFriendAssist);
		Messenger<bool>.AddListener("player_controller_enabled", OnPlayerControllerEnabled);
		Messenger.AddListener("monster_stun", OnMonsterStun);
		Messenger.AddListener("tutorial_dungeon_end", OnTutorialDungeonEnd);
		Messenger<MonsterAttack>.AddListener("monster_begin_atk", OnMonsterBeginAttack);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Messenger<PlayerAttack>.RemoveListener("player_begin_atk", OnPlayerAttack);
		Messenger.RemoveListener("player_block", OnPlayerBlock);
		Messenger.RemoveListener("battle_start", OnBattleStart);
		Messenger.RemoveListener("battle_end", OnBattleEnd);
		Messenger.RemoveListener("battle_exit", OnBattleExit);
		Messenger.RemoveListener("perfect_block", OnPerfectBlock);
		Messenger<MonsterCollisionData>.RemoveListener("player_get_hit", OnPlayerGetHit);
		Messenger<Player.DodgeDirection>.RemoveListener("perfect_dodge_start", OnPlayerStrafe);
		Messenger.RemoveListener("player_super_atk_begin", OnPlayerSuper);
		Messenger.RemoveListener("player_super_atk_end", OnPlayerSuperEnd);
		Messenger<SkillEnum.AssistSkill>.RemoveListener("friend_assist", OnFriendAssist);
		Messenger<bool>.RemoveListener("player_controller_enabled", OnPlayerControllerEnabled);
		Messenger.RemoveListener("monster_stun", OnMonsterStun);
		Messenger.RemoveListener("tutorial_dungeon_end", OnTutorialDungeonEnd);
		Messenger<MonsterAttack>.RemoveListener("monster_begin_atk", OnMonsterBeginAttack);
	}

	protected override void Awake()
	{
		base.Awake();
		GameObject gameObject = Resources.Load("Tutorial/Prefabs/Tutorial Slash Hint") as GameObject;
		Dbg.Assert(gameObject);
		GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject) as GameObject;
		Dbg.Assert(gameObject2);
		m_slashHint = gameObject2.GetComponent<TutorialSlashHint>();
		Dbg.Assert(m_slashHint);
		Dbg.Assert(SingletonComponent<BattleScreenUI>.Instance);
		m_slashHint.transform.parent = SingletonComponent<BattleScreenUI>.Instance.transform;
		m_slashHint.transform.localScale = Vector3.one;
		m_slashHint.Hide();
		m_endTimer = base.gameObject.AddComponent<NBTimer>();
		Dbg.Assert(m_endTimer);
	}

	public override void Start()
	{
		base.Start();
		Dbg.Assert(SingletonComponent<StageManager>.Instance);
		Dbg.Assert((bool)SingletonComponent<StageManager>.Instance.PlayerObject && (bool)SingletonComponent<StageManager>.Instance.EnemyObject);
		m_player = SingletonComponent<StageManager>.Instance.PlayerObject.GetComponent<Player>();
		m_monster = SingletonComponent<StageManager>.Instance.EnemyObject.GetComponent<Monster>();
		m_monsterAI = m_monster.GetComponent<TutorialMonsterAi>();
		Dbg.Assert(m_monster);
		Dbg.Assert(m_monsterAI);
		Dbg.Assert(m_player);
		m_monsterAI.allowAttack = false;
		m_monsterAI.m_eventTarget = base.gameObject;
		m_playerController = m_player.GetComponent<PlayerController>();
		Dbg.Assert(m_playerController);
		m_player.SetVisible(true);
		m_player.GetComponent<PlayerController>().enabled = false;
		m_player.m_regenerateHpAndImmortal = true;
		m_monster.SetVisible(true);
		m_monster.hasDeathCutscene = false;
		m_battleUI = SingletonComponent<BattleScreenUI>.Instance;
		if (null != m_battleUI)
		{
			m_battleUI.EnableBattleHudCamera();
			m_battleUI.ShowBattleUi(true);
			m_battleUI.Initialize();
			m_battleUI.m_dodgeButtons[0].target.enabled = false;
			m_battleUI.m_dodgeButtons[1].target.enabled = false;
			m_battleUI.m_blockButton.target.enabled = false;
		}
		SingletonComponent<BattleManager>.Instance.ClearBattleScore();
		SingletonComponent<BattleManager>.Instance.m_monsterHp = m_monster.Data.Hp;
		SingletonComponent<BattleManager>.Instance.enabled = true;
		SingletonComponent<BattleManager>.Instance.ChangeState(BattleManager.State.Battle);
		if (null != m_battleUI)
		{
			m_battleUI.EnablePauseButton(false);
			m_battleUI.EnableAlertText = false;
		}
		m_superData = ((!(null != SingletonComponent<SuperAttackSystem>.Instance) || !(null != SingletonComponent<SuperAttackSystem>.Instance.superAttackLogic)) ? null : SingletonComponent<SuperAttackSystem>.Instance.superAttackLogic.data);
		if (m_superData != null)
		{
			m_superData.DisableSkill(this);
			SuperAttackData superData = m_superData;
			superData.onMomentumFullCallback = (System.Action)Delegate.Combine(superData.onMomentumFullCallback, new System.Action(OnPlayerSuperReady));
		}
		SkillSystem instance = SingletonComponent<SkillSystem>.Instance;
		if (null != instance && null != instance.friendSkill)
		{
			instance.friendSkill.skillData.DisableSkill(this);
		}
		PlayerAttackLogic.m_blockSlashEnabled = false;
		m_disableControlAfterAttack = true;
		SingletonComponent<TutorialManager>.Instance.showDodgeInstructions = false;
		if (!(null != m_monster))
		{
			return;
		}
		string[] array = new string[2] { "Att_Righttop", "Att_Lefttop" };
		foreach (string p_attackAnim in array)
		{
			AnimationState animationState = m_monster.GetComponent<Animation>()[p_attackAnim];
			if (null != animationState)
			{
				AnimationEvent animationEvent = new AnimationEvent();
				animationEvent.functionName = "OnFreezeAttack";
				MonsterAttackParameter attackParameter = m_monster.GetAttackParameter(p_attackAnim);
				float time = 0f;
				if (attackParameter != null)
				{
					time = attackParameter.perfectDodgeWindowStart[0] * 0.2f + attackParameter.dodgeWindowEnd[0] * 0.8f;
				}
				animationEvent.time = time;
				animationState.clip.AddEvent(animationEvent);
			}
		}
	}

	private void EnableBattleUIButtons(bool p_enable)
	{
		bool locked = !p_enable;
		if (null != m_battleUI)
		{
			m_battleUI.m_dodgeButtons[0].m_locked = locked;
			m_battleUI.m_dodgeButtons[1].m_locked = locked;
			m_battleUI.m_blockButton.m_locked = locked;
		}
	}

	private void EnablePlayerAttack(bool p_enable)
	{
		if ((bool)m_playerController)
		{
			m_playerController.AttackEnable = p_enable;
		}
	}

	protected override void OnPopupClose()
	{
		base.OnPopupClose();
	}

	private void OnPlayerGetHit(MonsterCollisionData p_collData)
	{
		if (!(null != m_monsterAI) || !m_monsterAI.allowAttack)
		{
			Time.timeScale = 1f;
		}
	}

	private void OnPlayerAttack(PlayerAttack p_attack)
	{
		string text = "slash_" + p_attack.direction;
		Dbg.Log("TRIGGER ATTACK: " + text);
		if (TryAdvance(text))
		{
			if (m_disableControlAfterAttack)
			{
				m_playerController.enabled = false;
			}
			m_slashHint.Hide();
			SingletonComponent<TutorialManager>.Instance.HideInstruction();
			Time.timeScale = 1f;
			m_slashCount++;
			if (m_slashCount == 1)
			{
				AnalyticsManager.FirstInstallEvents("tut_slash1");
			}
			else if (m_slashCount == 2)
			{
				AnalyticsManager.FirstInstallEvents("tut_slash2");
			}
		}
		else
		{
			if (TryAdvance("slash"))
			{
				return;
			}
			if (Time.timeScale.Equals(0f))
			{
				m_player.IsReady = true;
				m_player.Idle();
			}
			else if (p_attack.type != PlayerAttack.Type.Repel && p_attack.type != PlayerAttack.Type.Ripost)
			{
				m_slashCount++;
				if (m_slashCount == 3)
				{
					AnalyticsManager.FirstInstallEvents("tut_slash3");
				}
				else if (m_slashCount == 4)
				{
					AnalyticsManager.FirstInstallEvents("tut_slash4");
				}
			}
		}
	}

	private void OnPlayerStrafe(Player.DodgeDirection p_dir)
	{
		string text = "button_" + p_dir;
		Dbg.Log("TRIGGER BUTTON: " + text);
		if (TryAdvance(text))
		{
			Time.timeScale = 1f;
			m_playerController.enabled = false;
			SingletonComponent<TutorialManager>.Instance.HidePointer();
			SingletonComponent<TutorialManager>.Instance.HideInstruction();
			EnableBattleUIButtons(false);
			switch (p_dir)
			{
			case Player.DodgeDirection.Left:
				AnalyticsManager.FirstInstallEvents("tut_dodgeleft");
				break;
			case Player.DodgeDirection.Right:
				AnalyticsManager.FirstInstallEvents("tut_dodgeright");
				break;
			}
		}
	}

	private void OnPlayerBlock()
	{
		string triggerKey = "button_Block";
		if (TryAdvance(triggerKey))
		{
			AnalyticsManager.FirstInstallEvents("tut_block");
			Time.timeScale = 1f;
		}
	}

	private void OnPerfectBlock()
	{
		string triggerKey = "perfect_block";
		if (TryAdvance(triggerKey))
		{
			EnableBattleUIButtons(false);
			if (m_currentAttack != null)
			{
				(m_monster.transform.FindInHierarchy(m_currentAttack.attackPoint).gameObject.GetComponent<Collider>() as SphereCollider).radius *= 0.5f;
			}
		}
	}

	private void OnBattleStart()
	{
		EnableBattleUIButtons(false);
		EnablePlayerAttack(false);
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_e_battle_music");
	}

	private void OnBattleEnd()
	{
		BattleManager instance = SingletonComponent<BattleManager>.Instance;
		if (null != instance && instance.IsPlayerWin)
		{
			AnalyticsManager.FirstInstallEvents("tut_freebattle1end");
		}
	}

	private void OnBattleExit()
	{
		string triggerKey = "battle_exit";
		if (TryAdvance(triggerKey))
		{
			AnalyticsManager.FirstInstallEvents("tut_battleresult");
		}
	}

	private void OnPlayerSuper()
	{
		string triggerKey = "super_move";
		if (TryAdvance(triggerKey))
		{
			AnalyticsManager.FirstInstallEvents("tut_super");
		}
	}

	private void OnPlayerSuperReady()
	{
		string triggerKey = "super_move_ready";
		if (TryAdvance(triggerKey))
		{
		}
		m_battleUI.m_blockButton.m_locked = true;
		m_battleUI.UnpressedButton();
		m_isSuperReady = true;
	}

	private void OnPlayerSuperEnd()
	{
		string triggerKey = "super_move_end";
		TryAdvance(triggerKey);
	}

	private void OnFriendAssist(SkillEnum.AssistSkill p_skill)
	{
		string triggerKey = "friend_assist";
		TryAdvance(triggerKey);
	}

	private void OnPlayerControllerEnabled(bool val)
	{
	}

	private void OnMonsterStun()
	{
	}

	private void OnMonsterBeginAttack(MonsterAttack p_attack)
	{
		m_currentAttack = p_attack;
	}

	private void OnTutorialDungeonEnd()
	{
		TryAdvance("dungeon_end");
	}

	private void OnFreezeAttack()
	{
		Time.timeScale = 0f;
		m_isTimeToDodge = true;
	}

	public override void DoAction(Action action)
	{
		base.DoAction(action);
	}

	public override void ShowSubtitle(Action action)
	{
		base.ShowSubtitle(action);
		if ((bool)m_playerController)
		{
			m_playerController.enabled = false;
		}
		SingletonComponent<TutorialManager>.Instance.HidePointer();
	}

	private void ShowSlashBLeftToTRight(Action action)
	{
		EnablePlayerAttack(true);
		m_playerController.enabled = true;
		SingletonComponent<TutorialManager>.Instance.EndSubtitle();
		m_monsterAI.enabled = true;
		m_monsterAI.allowAttack = false;
		m_slashHint.m_direction = PlayerAttack.Direction.TopRightToBottomLeft;
		m_slashHint.Show();
		SingletonComponent<TutorialManager>.Instance.ShowInstruction(action.text);
		Time.timeScale = 0f;
	}

	private void ShowSlashBRightToTLeft(Action action)
	{
		SingletonComponent<TutorialManager>.Instance.EndSubtitle();
		m_playerController.enabled = true;
		m_monsterAI.enabled = true;
		m_monsterAI.allowAttack = false;
		m_slashHint.m_direction = PlayerAttack.Direction.TopLeftToBottomRight;
		m_slashHint.Show();
		SingletonComponent<TutorialManager>.Instance.ShowInstruction(action.text);
		Time.timeScale = 0f;
		m_disableControlAfterAttack = false;
	}

	private void ShowTapLeftDodge(Action action)
	{
		SingletonComponent<TutorialManager>.Instance.EndSubtitle();
		DodgeSystem.alwaysPerfectDodge = true;
		StartCoroutine(PerformAttack(Task.DodgeLeft, action.text));
	}

	private void ShowTapRightDodge(Action action)
	{
		SingletonComponent<TutorialManager>.Instance.EndSubtitle();
		DodgeSystem.alwaysPerfectDodge = true;
		StartCoroutine(PerformAttack(Task.DodgeRight, action.text));
	}

	private void ShowTapBlock(Action action)
	{
		SingletonComponent<TutorialManager>.Instance.EndSubtitle();
		BlockSystem.alwaysPerfectBlock = true;
		StartCoroutine(PerformAttack(Task.Block, action.text));
		if (null != m_battleUI)
		{
			m_battleUI.EnableAlertText = true;
		}
	}

	private void FillSuperGauge()
	{
		StartCoroutine(DoFillSuperGauge());
	}

	private IEnumerator DoFillSuperGauge()
	{
		m_playerController.enabled = false;
		yield return new MBWaitForSeconds(2f);   // PORT (CD-059)
		m_playerController.enabled = true;
		if (m_isSuperReady)
		{
			TryAdvance("super_move_ready");
		}
		else
		{
			SingletonComponent<SuperAttackSystem>.Instance.superAttackLogic.data.momentum = 1f;
		}
	}

	private void ShowSubtitlePointToSuper(Action action)
	{
		SingletonComponent<TutorialManager>.Instance.EndSubtitle();
		Vector3 zero = Vector3.zero;
		if (null != m_battleUI)
		{
			SingletonComponent<TutorialManager>.Instance.ShowPointerWithDelay(zero, PointingHand.Direction.DownLeft, m_battleUI.m_superButton.transform, 0.2f);
		}
		SingletonComponent<TutorialManager>.Instance.ShowInstruction(action.text);
		Time.timeScale = 0f;
		if (m_superData != null)
		{
			m_superData.EnableSkill(this);
		}
		m_slashHint.Hide();
	}

	private void DoNothing()
	{
		SingletonComponent<TutorialManager>.Instance.HidePointer();
		SingletonComponent<TutorialManager>.Instance.HideInstruction();
	}

	private void BeforeFreeBattle(Action action)
	{
		SingletonComponent<TutorialManager>.Instance.ShowInstruction(action.text);
		m_monsterAI.m_eventTarget = null;
	}

	private void FreeBattle()
	{
		if (null != m_battleUI)
		{
			m_battleUI.EnableAlertText = true;
			m_battleUI.EnablePauseButton(true);
		}
		SingletonComponent<TutorialManager>.Instance.EndSubtitle();
		m_slashHint.Hide();
		SingletonComponent<TutorialManager>.Instance.HideInstruction();
		SingletonComponent<TutorialManager>.Instance.showDodgeInstructions = true;
		m_monster.hasDeathCutscene = true;
		SingletonComponent<BattleManager>.Instance.enabled = true;
		m_monsterAI.allowAttack = true;
		PlayerAttackLogic.m_blockSlashEnabled = true;
		BlockSystem.alwaysPerfectBlock = false;
		DodgeSystem.alwaysPerfectDodge = false;
		m_playerController.enabled = true;
		EnableBattleUIButtons(true);
		EnablePlayerAttack(true);
		AnalyticsManager.FirstInstallEvents("tut_freebattle1");
		Messenger.Invoke("tutorial_free_battle");
	}

	private void ShowSubtitleBattlePause(Action action)
	{
		ShowSubtitle(action);
		SingletonComponent<BattleManager>.Instance.ChangeState(BattleManager.State.None);
	}

	private void OnEndTimer()
	{
		UIPopupController.Instance.EnableUI();
	}

	private void BattleExit()
	{
		HideSubtitle();
		SingletonComponent<BattleManager>.Instance.ChangeState(BattleManager.State.Exit);
	}

	private void End()
	{
		AnalyticsManager.FirstInstallEvents("tut_battleend1_loading");
		Messenger<State>.Invoke("tutorial_completed", State.Combat1);
	}

	private IEnumerator WaitForMonsterTurning()
	{
		while (m_monster.AnimPlayer.CurrentAnimName == "Stun")
		{
			if (!m_disableControlAfterAttack && !m_playerController.enabled)
			{
				m_playerController.enabled = true;
			}
			yield return new WaitForFixedUpdate();
		}
		while (true)
		{
			if (m_monster.IsTurning())
			{
				yield return new WaitForFixedUpdate();
				continue;
			}
			if (m_monster.GetRelativeDirection(m_player.transform.position, 10f) != 0)
			{
				m_monster.Turn(m_player.transform.position);
				continue;
			}
			break;
		}
	}

	private IEnumerator WaitForMonsterIdle()
	{
		while (m_monster.CurrentState != Monster.State.Idle)
		{
			m_monster.ChangeState(Monster.State.Idle);
			yield return new WaitForFixedUpdate();
		}
	}

	private IEnumerator WaitForMonsterAttack(string p_anim)
	{
		m_isTimeToDodge = false;
		while (true)
		{
			if (m_monster.AnimPlayer.CurrentAnimName == "Stun")
			{
				if (!m_disableControlAfterAttack && !m_playerController.enabled)
				{
					m_playerController.enabled = true;
				}
			}
			else if (m_monster.AnimPlayer.CurrentAnimName != p_anim)
			{
				MonsterAttack attack = new MonsterAttack
				{
					type = MonsterAttack.Type.Weak,
					animationName = p_anim
				};
				m_monster.Attack(attack);
				m_player.EnableCollision(false);
				m_playerController.enabled = false;
			}
			else if (m_isTimeToDodge)
			{
				break;
			}
			yield return null;
		}
	}

	private IEnumerator PerformAttack(Task p_task, string p_message = "Empty")
	{
		m_playerController.enabled = false;
		string anim = string.Empty;
		switch (p_task)
		{
		case Task.DodgeLeft:
			anim = "Att_Righttop";
			break;
		case Task.DodgeRight:
			anim = "Att_Lefttop";
			break;
		case Task.Block:
			anim = "Att_Righttop";
			break;
		default:
			anim = "Att_Righttop";
			break;
		}
		yield return StartCoroutine(WaitForMonsterTurning());
		yield return StartCoroutine(WaitForMonsterIdle());
		yield return StartCoroutine(WaitForMonsterAttack(anim));
		m_slashHint.Hide();
		m_playerController.enabled = true;
		switch (p_task)
		{
		case Task.DodgeLeft:
			if (null != m_battleUI)
			{
				m_battleUI.m_dodgeButtons[0].m_locked = false;
				m_battleUI.m_dodgeButtons[1].m_locked = true;
				m_battleUI.m_blockButton.m_locked = true;
				m_battleUI.m_dodgeButtons[0].target.enabled = true;
				Vector3 fingerPos3 = Vector3.zero;
				SingletonComponent<TutorialManager>.Instance.ShowPointerWithDelay(fingerPos3, PointingHand.Direction.DownLeft, m_battleUI.m_dodgeButtons[0].transform, 0.2f);
				SingletonComponent<TutorialManager>.Instance.ShowInstruction(p_message);
			}
			break;
		case Task.DodgeRight:
			if (null != m_battleUI)
			{
				m_battleUI.m_dodgeButtons[0].m_locked = true;
				m_battleUI.m_dodgeButtons[1].m_locked = false;
				m_battleUI.m_blockButton.m_locked = true;
				m_battleUI.m_dodgeButtons[1].target.enabled = true;
				Vector3 fingerPos2 = Vector3.zero;
				SingletonComponent<TutorialManager>.Instance.ShowPointerWithDelay(fingerPos2, PointingHand.Direction.DownRight, m_battleUI.m_dodgeButtons[1].transform, 0.2f);
				SingletonComponent<TutorialManager>.Instance.ShowInstruction(p_message);
			}
			break;
		case Task.Block:
			if (null != m_battleUI)
			{
				m_battleUI.m_dodgeButtons[0].m_locked = true;
				m_battleUI.m_dodgeButtons[1].m_locked = true;
				m_battleUI.m_blockButton.m_locked = false;
				m_battleUI.m_blockButton.target.enabled = true;
				Vector3 fingerPos = Vector3.zero;
				SingletonComponent<TutorialManager>.Instance.ShowPointerWithDelay(fingerPos, PointingHand.Direction.DownRight, m_battleUI.m_blockButton.transform, 0.2f);
				SingletonComponent<TutorialManager>.Instance.ShowInstruction(p_message);
				if (m_currentAttack != null)
				{
					(m_monster.transform.FindInHierarchy(m_currentAttack.attackPoint).gameObject.GetComponent<Collider>() as SphereCollider).radius *= 2f;
				}
			}
			break;
		}
	}
}
