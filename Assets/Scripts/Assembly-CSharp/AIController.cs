using System;
using System.Collections.Generic;
using UnityEngine;

public class AIController : MonoBehaviour
{
	protected Player m_player;

	protected Monster m_monster;

	protected MonsterAttack m_currentAttack;

	protected NBAnimation m_anim;

	protected NBTimer m_idleTimer;

	protected List<MonsterAttack.Type> m_comboList = new List<MonsterAttack.Type>();

	public bool m_enableSpecialAttack = true;

	protected MonsterBehavior m_behavior = new MonsterBehavior();

	protected MonsterBehaviorDB m_behaviorDB;

	protected MonsterBehavior.Trigger[] m_triggers;

	protected string[] m_swipeLeftAnims;

	protected string[] m_swipeRightAnims;

	protected float m_idleTrackingTime = 1f;

	public float m_playerAngle;

	protected int m_playerDir;

	protected static readonly float playerAngleMax = 130f;

	protected static readonly float maxRotSpeed = playerAngleMax / 0.4f;

	public MonsterBehavior CurrentBehavior
	{
		get
		{
			return m_behavior;
		}
	}

	public List<MonsterAttack.Type> ComboList
	{
		get
		{
			return m_comboList;
		}
	}

	public NBTimer IdleTimer
	{
		get
		{
			return m_idleTimer;
		}
	}

	public MonsterAttackParameter CurrentAttackParam { get; protected set; }

	protected virtual void Awake()
	{
		m_monster = base.gameObject.GetComponent<Monster>();
		m_idleTimer = base.gameObject.AddComponent<NBTimer>();
		m_idleTimer.Name = "Idle Time";
		m_anim = MBHier.GetComponentInChildren<NBAnimation>(this);
		CurrentAttackParam = null;
		Messenger.AddListener("battle_init", OnBattleInit);
		Messenger.AddListener("battle_start", OnBattleStart);
		Messenger.AddListener("battle_end", OnBattleEnd);
		Messenger.AddListener("monster_stun_end", OnStunEnd);
		Messenger.AddListener("monster_stuck_end", OnStuckEnd);
		Messenger<CutsceneController.CutsceneType>.AddListener(CutsceneMessage.CUTSCENE_START, OnCutsceneStart);
		Messenger<CutsceneController.CutsceneType>.AddListener(CutsceneMessage.CUTSCENE_END, OnCutsceneEnd);
		base.enabled = false;
	}

	protected virtual void Start()
	{
		m_behaviorDB = Singleton<MonsterBehaviorDB>.Instance;
		m_triggers = m_behaviorDB.GetTriggers(m_monster.Data.Level, m_monster.Data.m_monsterData.isBoss);
		UpdateBehavior(1f);
		MonsterDebugGUI monsterDebugGUI = InitDebugGUI();
		if (monsterDebugGUI != null && Application.loadedLevelName != "BattleView")
		{
			monsterDebugGUI.enabled = false;
		}
	}

	protected virtual void OnDestroy()
	{
		Messenger.RemoveListener("battle_init", OnBattleInit);
		Messenger.RemoveListener("battle_start", OnBattleStart);
		Messenger.RemoveListener("battle_end", OnBattleEnd);
		Messenger.RemoveListener("monster_stun_end", OnStunEnd);
		Messenger.RemoveListener("monster_stuck_end", OnStuckEnd);
		Messenger<CutsceneController.CutsceneType>.RemoveListener(CutsceneMessage.CUTSCENE_START, OnCutsceneStart);
		Messenger<CutsceneController.CutsceneType>.RemoveListener(CutsceneMessage.CUTSCENE_END, OnCutsceneEnd);
	}

	protected virtual string DetermineAttackAnim(MonsterAttack.Type p_attackType)
	{
		float lookAngle = base.transform.GetLookAngle(m_player.transform);
		string empty = string.Empty;
		Dbg.Log(string.Concat("BeginAttack: ", p_attackType, ". Angle between player and enemy: ", lookAngle));
		empty = ((p_attackType == MonsterAttack.Type.Signature) ? NBContainerUtil.RandomElement(m_monster.SpecialAttackAnimations) : ((lookAngle >= 60f && m_swipeLeftAnims != null) ? NBContainerUtil.RandomElement(m_swipeLeftAnims) : ((lookAngle >= 10f) ? m_monster.RandomAttackAnimation(p_attackType, MonsterAttack.Zone.Right) : ((lookAngle <= -60f && m_swipeRightAnims != null) ? NBContainerUtil.RandomElement(m_swipeRightAnims) : ((!(lookAngle <= -10f)) ? m_monster.RandomAttackAnimation(p_attackType, MonsterAttack.Zone.None) : m_monster.RandomAttackAnimation(p_attackType, MonsterAttack.Zone.Left))))));
		bool flag = Array.IndexOf(m_monster.AdvanceAttackAnimations, empty) >= 0;
		bool flag2 = Array.IndexOf(m_monster.SpecialAttackAnimations, empty) >= 0;
		if (empty == null || (flag && !m_behavior.hasAdvanceAttack) || (flag2 && !m_behavior.hasSpecialAttack))
		{
			empty = NBContainerUtil.RandomElement(m_monster.NormalAttackAnimations);
		}
		return empty;
	}

	protected virtual void PrepareComboString(int p_comboCount)
	{
		if (m_behavior.name == "DEAD")
		{
			return;
		}
		m_comboList.Clear();
		for (int i = 0; i < p_comboCount; i++)
		{
			MonsterAttack.Type type = MonsterAttack.Type.None;
			int num = 0;
			do
			{
				type = MonsterAttack.RandomAttack(m_behavior.hasSpecialAttack);
				num++;
			}
			while ((type == MonsterAttack.Type.Ranged || type == MonsterAttack.Type.Roar) && num < 10);
			m_comboList.Add(type);
		}
	}

	protected virtual MonsterAttack PickAttackCommand()
	{
		if (m_comboList.Count == 0)
		{
			return null;
		}
		MonsterAttack monsterAttack = new MonsterAttack();
		monsterAttack.animationName = DetermineAttackAnim(m_comboList[0]);
		m_comboList.RemoveAt(0);
		return monsterAttack;
	}

	public virtual void Attack()
	{
		if (!base.enabled)
		{
			Dbg.LogWarn(string.Concat("Unable to commence an attack. The ", GetType(), " component was disabled."));
			m_comboList.Clear();
			return;
		}
		MonsterAttack monsterAttack = PickAttackCommand();
		if (monsterAttack == null)
		{
			Dbg.LogWarn("Unable to commence an attack. Attack command is null.");
			m_monster.Idle();
			return;
		}
		bool flag = false;
		CurrentAttackParam = m_monster.AttackParameterDB[monsterAttack.animationName];
		if (CurrentAttackParam != null)
		{
			for (int i = 0; i < CurrentAttackParam.comboCount; i++)
			{
				flag |= CanAttackReachPlayer(CurrentAttackParam.zones[i]);
			}
		}
		if (!flag || string.IsNullOrEmpty(monsterAttack.animationName))
		{
			monsterAttack = null;
			TrackPlayer();
		}
		else
		{
			_Attack(monsterAttack);
		}
	}

	protected virtual void _Attack(MonsterAttack p_attack)
	{
		m_monster.Attack(p_attack);
	}

	protected virtual void OnBeginAttack(MonsterAttack p_attack)
	{
		m_currentAttack = p_attack;
	}

	protected virtual void OnDamageWindow()
	{
	}

	protected virtual void OnWindDown()
	{
	}

	protected virtual void OnFinishAttack()
	{
		m_currentAttack = null;
		if (base.enabled)
		{
			if (m_comboList.Count > 0)
			{
				Attack();
			}
			else
			{
				m_monster.Idle();
			}
		}
	}

	protected void OnExitAttack()
	{
		m_currentAttack = null;
	}

	protected virtual MonsterDebugGUI InitDebugGUI()
	{
		return base.gameObject.AddComponent<MonsterDebugGUI>();
	}

	protected virtual bool ChangeBehavior(string newBehavior)
	{
		if (m_behavior == null || m_behavior.name != newBehavior)
		{
			if (m_behavior != null)
			{
				Dbg.LogWarn("Monster changed behavior from: " + m_behavior.name + " to: " + newBehavior);
			}
			else
			{
				Dbg.LogWarn("Monster changed behavior to: " + newBehavior);
			}
			m_behavior = Singleton<MonsterBehaviorDB>.Instance.GetBehavior(newBehavior);
			return true;
		}
		return false;
	}

	protected void AssignSwipeAttackAnim(MonsterAttack.Zone p_direction, string[] p_list)
	{
		if (p_direction != MonsterAttack.Zone.Left || p_direction != MonsterAttack.Zone.Right)
		{
			Dbg.LogWarn("Invalid attack direction");
			return;
		}
		string[] array = new string[p_list.Length];
		for (int i = 0; i < p_list.Length; i++)
		{
			array[i] = p_list[i];
		}
		if (p_direction == MonsterAttack.Zone.Left)
		{
			m_swipeLeftAnims = array;
		}
		else
		{
			m_swipeRightAnims = array;
		}
	}

	protected virtual void RandomIdleTime(string p_callback = null)
	{
		m_idleTimer.CountDown(m_monster.Data.m_monsterData.idleBeforeAttack, p_callback);
		Dbg.Log("RandomIdleTime: " + m_idleTimer.Counter);
	}

	protected virtual void TrackPlayer()
	{
		m_monster.Idle();
		m_idleTimer.CountDown(UnityEngine.Random.Range(m_idleTrackingTime, m_idleTrackingTime + 1f), "OnIdleTimeEnd");
	}

	protected virtual void TrackPlayerWhileIdling()
	{
		if (m_playerDir != 0 && !m_monster.IsTurning() && base.GetComponent<Animation>().enabled)
		{
			m_playerAngle = Mathf.Clamp(m_playerAngle, 0f - playerAngleMax, playerAngleMax);
			m_monster.Turn(m_playerAngle);
		}
	}

	protected virtual void TrackPlayerWhileAttacking()
	{
		float num = 0f;
		if (!(m_monster.AnimPlayer.CurrentAnim != null))
		{
			return;
		}
		num = m_monster.AnimPlayer.CurrentAnim.time;
		int currentTrackableWindow = GetCurrentTrackableWindow(num);
		if (currentTrackableWindow != -1 && CanAttackReachPlayer(m_currentAttack.zone))
		{
			float num2 = Mathf.Abs(CurrentAttackParam.endTrackingWindows[currentTrackableWindow] - num);
			if (!(num2 <= 0.01f))
			{
				_TrackPlayerWhileAttacking(num2);
			}
		}
	}

	protected virtual void _TrackPlayerWhileAttacking(float p_trackabkePeriod)
	{
		float num = Mathf.Clamp(m_playerAngle, 0f - playerAngleMax, playerAngleMax);
		float num2 = ((!(num < 0f)) ? 1f : (-1f));
		if (m_playerDir == 0 && Mathf.Abs(m_playerAngle) > 15f)
		{
			num = ((!(m_playerAngle > 0f)) ? (-15f) : 15f);
		}
		float f = num / p_trackabkePeriod;
		if (Mathf.Abs(f) > maxRotSpeed)
		{
			num = maxRotSpeed * num2;
		}
		Rotor component = m_monster.GetComponent<Rotor>();
		if (component != null)
		{
			component.BeginRotate(num, p_trackabkePeriod);
		}
	}

	protected int GetCurrentTrackableWindow(float p_animTime)
	{
		if (m_currentAttack == null || m_monster.AnimPlayer.CurrentAnim == null || CurrentAttackParam == null)
		{
			return -1;
		}
		if (CurrentAttackParam.startTrackingWindows.Length != CurrentAttackParam.endTrackingWindows.Length)
		{
			return -1;
		}
		int num = CurrentAttackParam.startTrackingWindows.Length;
		for (int i = 0; i < num; i++)
		{
			if (p_animTime >= CurrentAttackParam.startTrackingWindows[i] && p_animTime <= CurrentAttackParam.endTrackingWindows[i])
			{
				return i;
			}
		}
		return -1;
	}

	protected bool CanAttackReachPlayer(MonsterAttack.Zone p_attackZone)
	{
		bool result = true;
		if (p_attackZone != MonsterAttack.Zone.Area)
		{
			result = p_attackZone != MonsterAttack.Zone.None && ((m_playerDir <= 0 && p_attackZone == MonsterAttack.Zone.Left) || (m_playerDir >= 0 && p_attackZone == MonsterAttack.Zone.Right) || p_attackZone == MonsterAttack.Zone.Center);
		}
		return result;
	}

	protected virtual void OnEnable()
	{
		if (m_monster.Data.Hp == 0f)
		{
			Dbg.LogWarn("Cannot enable MonsterAI after dead.");
			base.enabled = false;
		}
		else if (m_player == null)
		{
			Dbg.LogWarn("Player was not successfully initialized.");
			base.enabled = false;
		}
		else if (m_monster.CurrentState != Monster.State.Cutscene)
		{
			m_idleTimer.Pause(false);
		}
	}

	protected virtual void OnDisable()
	{
		m_idleTimer.Pause(true);
		m_comboList.Clear();
	}

	protected virtual void OnIdle()
	{
		if (!base.enabled)
		{
			return;
		}
		if (m_player != null && m_player.Data.Hp <= 0f)
		{
			m_monster.Wait();
			return;
		}
		m_currentAttack = null;
		if (m_idleTimer.Counter == 0f)
		{
			RandomIdleTime("OnIdleTimeEnd");
		}
	}

	protected virtual void OnDead()
	{
		Dbg.LogWarn("OnDead");
		base.enabled = false;
	}

	protected virtual void OnStun()
	{
		Dbg.Log("OnStun");
		base.enabled = false;
	}

	protected virtual void OnStunEnd()
	{
		Dbg.Log("OnStunEnd");
		base.enabled = true;
		m_comboList.Clear();
		m_monster.Idle();
	}

	protected virtual void OnStuckEnd()
	{
		m_monster.ChangeState(Monster.State.Idle);
		m_monster.Recover(m_player.transform.position);
	}

	public virtual void Pause(bool p_pause)
	{
		m_monster.GetComponent<Animation>().enabled = !p_pause;
		m_monster.AnimPlayer.enabled = !p_pause;
		m_monster.enabled = !p_pause;
		m_monster.StopTurn();
		base.enabled = !p_pause;
	}

	protected virtual void OnTauntBegan()
	{
		if (base.enabled)
		{
			m_idleTimer.enabled = false;
		}
	}

	protected virtual void OnWait()
	{
		base.enabled = false;
	}

	protected virtual void OnExitWait()
	{
		base.enabled = true;
	}

	protected virtual void OnHit()
	{
	}

	protected virtual void OnIdleTimeEnd()
	{
	}

	protected virtual void OnBattleInit()
	{
		base.enabled = false;
		m_player = SingletonComponent<StageManager>.Instance.PlayerObject.GetComponent<Player>();
	}

	protected virtual void OnBattleStart()
	{
		base.enabled = true;
	}

	protected virtual void OnBattleEnd()
	{
		m_idleTimer.Reset();
		m_comboList.Clear();
		base.enabled = false;
	}

	protected virtual void OnCutsceneStart(CutsceneController.CutsceneType p_type)
	{
		Dbg.Log("aicontroller.OnCutsceneStart");
		base.enabled = false;
	}

	protected virtual void OnCutsceneEnd(CutsceneController.CutsceneType p_type)
	{
		if (!CutsceneController.IsMobDeathCutscene(p_type) && p_type != CutsceneController.CutsceneType.TUTORIAL1)
		{
			base.enabled = true;
		}
	}

	protected virtual void OnAnimEnded(string p_anim)
	{
		if (m_monster == null || m_monster.CurrentState == Monster.State.Cutscene || m_monster.Data == null || m_monster.Data.Hp == 0f)
		{
			return;
		}
		if (!base.enabled)
		{
			if (m_player.Data.Hp == 0f || m_player.IsMoving())
			{
				m_monster.AnimPlayer.Play("Idle_01");
			}
			return;
		}
		Monster.State currentState = m_monster.CurrentState;
		if (currentState == Monster.State.Idle && NeedToPlayIdleAnim())
		{
			m_monster.RandomIdleAnim(0.3f, false);
		}
	}

	protected virtual bool NeedToPlayIdleAnim()
	{
		return m_idleTimer.Counter > 0f;
	}

	protected virtual void FixedUpdate()
	{
		if (m_monster.Data.Hp == 0f)
		{
			base.enabled = false;
		}
		UpdateBehavior(m_monster.Data.HpPercent);
		UpdateGlobal();
		switch (m_monster.CurrentState)
		{
		case Monster.State.Idle:
			UpdateIdle();
			break;
		case Monster.State.Attack:
			UpdateAttack();
			break;
		case Monster.State.Hit:
			UpdateHit();
			break;
		case Monster.State.Block:
			break;
		}
	}

	protected virtual void LateUpdate()
	{
	}

	protected virtual void UpdateGlobal()
	{
		if (m_player != null)
		{
			m_playerAngle = base.transform.GetLookAngle(m_player.transform);
			m_playerDir = m_monster.GetRelativeDirection(m_playerAngle, 15f);
		}
		if (m_monster.CurrentState != Monster.State.Attack && m_idleTimer.Counter == 0f && m_comboList.Count == 0)
		{
			int p_comboCount = UnityEngine.Random.Range(1, m_behavior.comboCount);
			PrepareComboString(p_comboCount);
		}
	}

	protected virtual void UpdateIdle()
	{
		bool flag = Mathf.Abs(m_playerAngle) >= playerAngleMax;
		if (m_idleTimer.Counter == 0f && !flag)
		{
			AnimationState currentAnim = m_monster.AnimPlayer.CurrentAnim;
			if ((currentAnim != null && currentAnim.wrapMode == WrapMode.Loop) || !m_monster.AnimPlayer.IsPlaying)
			{
				Attack();
				return;
			}
		}
		TrackPlayerWhileIdling();
	}

	protected virtual void UpdateAttack()
	{
		TrackPlayerWhileAttacking();
	}

	protected virtual void UpdateHit()
	{
		if (!m_monster.AnimPlayer.IsPlaying)
		{
			if (m_comboList.Count > 0)
			{
				Attack();
			}
			else
			{
				m_monster.Idle();
			}
		}
	}

	protected virtual void UpdateBehavior(float p_hp)
	{
		string text = string.Empty;
		MonsterBehavior.Trigger[] triggers = m_triggers;
		foreach (MonsterBehavior.Trigger trigger in triggers)
		{
			if (p_hp <= trigger.parameter)
			{
				text = trigger.behavior;
				break;
			}
		}
		if (text == string.Empty)
		{
			MonsterBehavior.Trigger[] triggers2 = m_triggers;
			foreach (MonsterBehavior.Trigger trigger2 in triggers2)
			{
				Dbg.Log(trigger2.ToString());
			}
			Dbg.Log("Monster HP: " + p_hp);
			Dbg.LogError("Invalid monster behavior.");
		}
		else
		{
			ChangeBehavior(text);
		}
	}
}
