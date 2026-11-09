using System;
using System.Collections;
using UnityEngine;

public class SecondCombatTutorial : Tutorial
{
	private Player m_player;

	private PlayerController m_playerController;

	private Monster m_monster;

	private TutorialMonsterAi m_monsterAI;

	private TutorialSlashHint m_slashHint;

	private GameObject m_invisibleButtonPrefab;

	private UIButtonMessage m_invisibleButton;

	private bool m_isSuperAttackFull;

	private BattleScreenUI m_battleUI;

	private SuperAttackData m_superData;

	protected override void OnEnable()
	{
		base.OnEnable();
		Messenger.AddListener("player_super_atk", OnPlayerSuper);
		Messenger.AddListener("battle_start_ui", OnBattleStartUI);
		Messenger.AddListener("battle_announce", OnBattleAnnounce);
		Messenger.AddListener("battle_init", OnBattleInit);
		Messenger.AddListener("battle_start", OnBattleStart);
		Messenger.AddListener("battle_end", OnBattleEnd);
		Messenger.AddListener("battle_exit", OnBattleExit);
		Messenger<CutsceneController.CutsceneType>.AddListener(CutsceneMessage.CUTSCENE_START, OnCutsceneStart);
		Messenger<CutsceneController.CutsceneType>.AddListener(CutsceneMessage.CUTSCENE_END, OnCutsceneEnd);
		Messenger<PlayerCollisionData>.AddListener("monster_get_hit", OnMonsterGetHit);
		Messenger<DamageData>.AddListener("player_receive_dmg", OnPlayerReceivedDamage);
		Messenger.AddListener("tutorial_dungeon_end", OnTutorialDungeonEnd);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Messenger.RemoveListener("player_super_atk", OnPlayerSuper);
		Messenger.RemoveListener("battle_start_ui", OnBattleStartUI);
		Messenger.RemoveListener("battle_announce", OnBattleAnnounce);
		Messenger.RemoveListener("battle_init", OnBattleInit);
		Messenger.RemoveListener("battle_start", OnBattleStart);
		Messenger.RemoveListener("battle_end", OnBattleEnd);
		Messenger.RemoveListener("battle_exit", OnBattleExit);
		Messenger<CutsceneController.CutsceneType>.RemoveListener(CutsceneMessage.CUTSCENE_START, OnCutsceneStart);
		Messenger<CutsceneController.CutsceneType>.RemoveListener(CutsceneMessage.CUTSCENE_END, OnCutsceneEnd);
		Messenger<PlayerCollisionData>.RemoveListener("monster_get_hit", OnMonsterGetHit);
		Messenger<DamageData>.RemoveListener("player_receive_dmg", OnPlayerReceivedDamage);
		Messenger.RemoveListener("tutorial_dungeon_end", OnTutorialDungeonEnd);
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
		m_invisibleButtonPrefab = Resources.Load("Tutorial/Prefabs/Invisible Button") as GameObject;
		Dbg.Assert(m_invisibleButtonPrefab);
	}

	private void CreateInvisibleButton()
	{
		if ((bool)m_invisibleButtonPrefab)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(m_invisibleButtonPrefab) as GameObject;
			Dbg.Assert(gameObject);
			gameObject.transform.parent = UIPopupController.Instance.Panel;
			gameObject.layer = UIPopupController.Instance.gameObject.layer;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localScale = Vector3.one;
			m_invisibleButton = gameObject.GetComponent<UIButtonMessage>();
			Dbg.Assert(m_invisibleButton);
			m_invisibleButton.target = base.gameObject;
			m_invisibleButton.functionName = "OnScreenTapped";
		}
	}

	public override void Start()
	{
		base.Start();
		m_battleUI = SingletonComponent<BattleScreenUI>.Instance;
		Dbg.Assert(m_battleUI);
		m_player = SingletonComponent<StageManager>.Instance.PlayerObject.GetComponent<Player>();
		Dbg.Assert(m_player);
		m_playerController = m_player.GetComponent<PlayerController>();
		Dbg.Assert(m_playerController);
		m_monster = SingletonComponent<StageManager>.Instance.EnemyObject.GetComponent<Monster>();
		Dbg.Assert(m_monster);
		m_monsterAI = m_monster.GetComponent<TutorialMonsterAi>();
		Dbg.Assert(m_monsterAI);
		m_monsterAI.enabled = false;
		m_monsterAI.isImmortal = false;
		if (null != m_battleUI)
		{
			m_battleUI.Initialize();
			Dbg.Assert(m_battleUI.m_assistButton);
			m_battleUI.ShowBattleUiButtons(true);
			m_battleUI.EnableAlertText = true;
		}
		SkillSystem instance = SingletonComponent<SkillSystem>.Instance;
		if (null != instance && null != instance.friendSkill)
		{
			instance.friendSkill.skillData.DisableSkill(this);
		}
		TutorialManager instance2 = SingletonComponent<TutorialManager>.Instance;
		if (null != instance2)
		{
			instance2.showDodgeInstructions = true;
		}
	}

	protected override void OnPopupClose()
	{
		base.OnPopupClose();
	}

	private void OnPlayerAttack(PlayerAttack p_attack)
	{
		string text = "slash_" + p_attack.direction;
		Dbg.Log("TRIGGER ATTACK: " + text);
		if (TryAdvance(text))
		{
			m_slashHint.Hide();
		}
	}

	private void OnPlayerSuper()
	{
		string triggerKey = "super_move";
		TryAdvance(triggerKey);
	}

	private void OnPlayerSuperReady()
	{
		string triggerKey = "super_move_ready";
		TryAdvance(triggerKey);
	}

	private void OnBattleStartUI()
	{
		string triggerKey = "battle_start_ui";
		TryAdvance(triggerKey);
	}

	private void OnBattleAnnounce()
	{
		string triggerKey = "battle_announce";
		if (TryAdvance(triggerKey))
		{
		}
		DungeonManager instance = SingletonComponent<DungeonManager>.Instance;
		if (!(null != instance))
		{
			return;
		}
		BattleStruct currentBattle = SingletonComponent<DungeonManager>.Instance.GetCurrentBattle();
		if (currentBattle != null)
		{
			switch (currentBattle.battleNumber)
			{
			case 0:
				AnalyticsManager.FirstInstallEvents("tut_woodstorm");
				break;
			case 1:
				AnalyticsManager.FirstInstallEvents("tut_wingserpent");
				break;
			}
		}
	}

	private void OnBattleInit()
	{
		if ((bool)SingletonComponent<DungeonManager>.Instance)
		{
			BattleStruct currentBattle = SingletonComponent<DungeonManager>.Instance.GetCurrentBattle();
			if (currentBattle != null)
			{
				int battleNumber = currentBattle.battleNumber;
				if (battleNumber == 1)
				{
					TryAdvance("battle_continue");
					SingletonComponent<BattleManager>.Instance.m_tutorialMusic = "list_e_miniboss_music";
				}
			}
		}
		if (null != SingletonComponent<SuperAttackSystem>.Instance)
		{
			m_superData = SingletonComponent<SuperAttackSystem>.Instance.superAttackLogic.data;
			SuperAttackData superData = m_superData;
			superData.onMomentumFullCallback = (System.Action)Delegate.Combine(superData.onMomentumFullCallback, new System.Action(OnPlayerSuperReady));
		}
	}

	private void OnPlayerReceivedDamage(DamageData p_damage)
	{
		if (TryAdvance("player_hit"))
		{
			AnalyticsManager.FirstInstallEvents("tut_wingserpentattack");
		}
	}

	private void OnBattleStart()
	{
		TryAdvance("battle_start");
	}

	private void OnScreenTapped()
	{
		TryAdvance("click");
	}

	private void OnBattleEnd()
	{
		string triggerKey = "battle_end";
		TryAdvance(triggerKey);
		BattleManager instance = SingletonComponent<BattleManager>.Instance;
		DungeonManager instance2 = SingletonComponent<DungeonManager>.Instance;
		if (!(null != instance2) || !(null != instance))
		{
			return;
		}
		BattleStruct currentBattle = SingletonComponent<DungeonManager>.Instance.GetCurrentBattle();
		if (currentBattle == null)
		{
			return;
		}
		int battleNumber = currentBattle.battleNumber;
		if (instance.IsPlayerWin)
		{
			switch (battleNumber)
			{
			case 0:
				AnalyticsManager.FirstInstallEvents("tut_freebattle2_end");
				break;
			case 1:
				AnalyticsManager.FirstInstallEvents("tut_freebattle3_end");
				break;
			}
		}
	}

	private void OnBattleContinue(int battleNumber)
	{
		if (battleNumber == 0)
		{
			AnalyticsManager.FirstInstallEvents("tut_battleresult2");
		}
	}

	private void OnBattleExit()
	{
		string triggerKey = "battle_exit";
		if (TryAdvance(triggerKey))
		{
			AnalyticsManager.FirstInstallEvents("tut_battleresult3");
		}
	}

	private void OnCutsceneStart(CutsceneController.CutsceneType p_type)
	{
		if (CutsceneController.IsFriendAssistCutscene(p_type))
		{
			string triggerKey = "friend_assist";
			if (TryAdvance(triggerKey))
			{
				AnalyticsManager.FirstInstallEvents("tut_friendassist");
			}
		}
	}

	private void OnCutsceneEnd(CutsceneController.CutsceneType p_type)
	{
		if (CutsceneController.IsFriendAssistCutscene(p_type))
		{
			string triggerKey = "friend_assist_end";
			TryAdvance(triggerKey);
		}
	}

	private void OnMonsterGetHit(PlayerCollisionData collisionData)
	{
		if ((bool)m_monster && m_monster.Data.HpPercent <= 0.5f)
		{
			TryAdvance("monster_health_half");
		}
	}

	private void OnTutorialDungeonEnd()
	{
		TryAdvance("dungeon_end");
	}

	public override void ShowSubtitle(Action p_action)
	{
		base.ShowSubtitle(p_action);
		SingletonComponent<TutorialManager>.Instance.HidePointer();
	}

	private void ShowTapMonster(Action action)
	{
		HideSubtitle();
		SingletonComponent<TutorialManager>.Instance.ShowInstruction(action.text, InstructionBox.Anchor.Bottom);
	}

	private void ShowSubtitleBattleContinue(Action p_action)
	{
		ShowSubtitle(p_action);
		SingletonComponent<BattleManager>.Instance.ChangeState(BattleManager.State.None);
		m_monster = SingletonComponent<StageManager>.Instance.EnemyObject.GetComponent<Monster>();
		UnityEngine.Object.Destroy(m_monster.GetComponent<AIController>());
		m_monsterAI = m_monster.gameObject.AddComponent<TutorialMonsterAi>();
	}

	private void StartNextBattle(Action p_action)
	{
		HideSubtitle();
		SingletonComponent<BattleManager>.Instance.ChangeState(BattleManager.State.Init);
		if (null != m_battleUI)
		{
			m_battleUI.EnablePauseButton(false);
		}
		TutorialManager instance = SingletonComponent<TutorialManager>.Instance;
		if (null != instance)
		{
			instance.showDodgeInstructions = false;
		}
	}

	private void ShowInstructionFreezeTime(Action p_action)
	{
		if (p_action != null)
		{
			SingletonComponent<TutorialManager>.Instance.ShowInstruction(p_action.text);
		}
		CreateInvisibleButton();
		if (m_superData != null && m_superData.momentum >= 1f)
		{
			m_superData.momentum = 0.95f;
			m_isSuperAttackFull = true;
		}
		Time.timeScale = 0f;
	}

	private void AttackPlayer(Action p_action)
	{
		HideInstruction();
		Time.timeScale = 1f;
		if ((bool)m_invisibleButton)
		{
			UnityEngine.Object.Destroy(m_invisibleButton.gameObject);
		}
		if ((bool)m_monsterAI)
		{
			m_monsterAI.Attack();
		}
		if ((bool)m_playerController)
		{
			m_playerController.enabled = false;
		}
	}

	private void DropPlayerHealthDelayTime(Action p_action)
	{
		if ((bool)m_player && m_player.Data.HpPercent > 0.5f)
		{
			m_player.Data.HpPercent = 0.5f;
		}
		if ((bool)m_monsterAI)
		{
			m_monsterAI.allowAttack = false;
		}
		DelayTime(p_action);
	}

	private void DoNothing()
	{
		SingletonComponent<TutorialManager>.Instance.HidePointer();
		SingletonComponent<TutorialManager>.Instance.HideInstruction();
		HideSubtitle();
	}

	private void ShowFingerToFriendAssist(Action p_action)
	{
		HideSubtitle();
		StartCoroutine(DoEnableFriendAssist(p_action));
	}

	private IEnumerator DoEnableFriendAssist(Action p_action)
	{
		yield return null;
		if ((bool)SingletonComponent<FaderGui>.Instance)
		{
			SingletonComponent<FaderGui>.Instance.ignoreTimeScale = true;
		}
		Time.timeScale = 0f;
		m_playerController.AttackEnable = true;
		SkillSystem skillSystem = SingletonComponent<SkillSystem>.Instance;
		if (null != skillSystem && null != skillSystem.friendSkill)
		{
			skillSystem.friendSkill.skillData.EnableSkill(this);
		}
		if (m_superData != null && m_superData.momentum >= 1f)
		{
			m_superData.momentum = 0.95f;
			m_isSuperAttackFull = true;
		}
		if (null != m_battleUI)
		{
			m_battleUI.EnableDodgeButton(0, true);
			m_battleUI.EnableDodgeButton(1, true);
			m_battleUI.EnableBlockButton(true);
		}
		Vector3 fingerPos = Vector3.zero;
		if (null != m_battleUI)
		{
			SingletonComponent<TutorialManager>.Instance.ShowPointerWithDelay(fingerPos, PointingHand.Direction.DownLeft, m_battleUI.m_assistButton.transform, 0.2f);
		}
		if (p_action != null)
		{
			SingletonComponent<TutorialManager>.Instance.ShowInstruction(p_action.text);
		}
	}

	private void EnableFriendAssistButton(Action action)
	{
		HideSubtitle();
	}

	private void ShowSubtitleBattlePause(Action action)
	{
		ShowSubtitle(action);
		SingletonComponent<BattleManager>.Instance.ChangeState(BattleManager.State.None);
	}

	private void FreeBattle()
	{
		Time.timeScale = 1f;
		if ((bool)SingletonComponent<FaderGui>.Instance)
		{
			SingletonComponent<FaderGui>.Instance.ignoreTimeScale = false;
		}
		if (m_isSuperAttackFull && m_superData != null)
		{
			m_superData.momentum = 1f;
		}
		SingletonComponent<TutorialManager>.Instance.EndSubtitle();
		SingletonComponent<TutorialManager>.Instance.showDodgeInstructions = true;
		SingletonComponent<BattleManager>.Instance.enabled = true;
		m_monster.enabled = true;
		m_monsterAI.enabled = true;
		m_monsterAI.isImmortal = false;
		StartCoroutine(DoRefreshAssistButton());
		m_monsterAI.allowAttack = true;
		SingletonComponent<TutorialManager>.Instance.HidePointer();
		if (null != m_battleUI)
		{
			m_battleUI.EnablePauseButton(true);
		}
		AnalyticsManager.FirstInstallEvents("tut_freebattle3");
	}

	private IEnumerator DoRefreshAssistButton()
	{
		yield return null;
		if (null != m_battleUI && null != m_battleUI.m_assistButton)
		{
			m_battleUI.m_assistButton.Refresh();
		}
	}

	private void BattleExit(Action action)
	{
		HideSubtitle();
		SingletonComponent<BattleManager>.Instance.ChangeState(BattleManager.State.Exit);
		AnalyticsManager.FirstInstallEvents("tut_battleend3_loading");
	}

	private void End(Action action)
	{
		Messenger<State>.Invoke("tutorial_completed", State.Combat2);
	}
}
