using System;
using System.Collections;
using UnityEngine;

public class LandDragonAI : AIController
{
	public enum Phase
	{
		None = 0,
		Calm = 1,
		Underground = 2,
		Aggressive = 3,
		BodySlam = 4
	}

	public class Underground
	{
		public enum Phase
		{
			None = 0,
			GroundTrail = 1,
			AttackWait = 2,
			AttackSign = 3,
			Attack = 4,
			Rest = 5
		}
	}

	private float m_phaseCalmHpThreshold = 0.75f;

	private float m_phaseUndergroundDuration = 30f;

	private float m_phaseAggressiveDuration = 30f;

	private Phase m_phase;

	private Underground.Phase m_attackPhase;

	private float m_defaultMinAttack;

	private float m_defaultMaxAttack;

	private float m_defaultAgility;

	private bool m_slamHit;

	private float m_stunTime = 5f;

	private BattleCamera m_battleCamera;

	private Vector3 m_originalPosition;

	private Vector3 m_beforeDivingPosition;

	private Vector3 m_prevPosition;

	private LandDragonSpecialFx m_vfx;

	public LandDragonGeyserAttack[] m_geyserAttacks;

	private MonsterAttack m_undergroundAttack;

	private NBTimer m_phaseTimer;

	private LandDragon m_dragon;

	private bool m_interuptable = true;

	private float m_trailTime;

	private float m_trailDuration = 2.5f;

	private float m_trailMinAngle;

	private float m_trailMaxAngle = 80f;

	private float m_trailMinRadius = 5f;

	private float m_trailMaxRadius = 13.5f;

	private Vector3 m_trailPosition = Vector3.zero;

	private Vector3 m_snapPosition;

	private int m_attackDirection = 1;

	private float m_rakeTime;

	private float m_rakeDuration = 3f;

	private float m_waitTime;

	private float m_waitDuration = 2f;

	private float m_signTime;

	private float m_signDuration = 0.5f;

	private float m_restTime;

	private float m_restDuration = 2f;

	private float m_geyserDuration;

	private int m_attackType;

	public Phase phase
	{
		get
		{
			return m_phase;
		}
		set
		{
			Dbg.Log(string.Concat("Changing Phase From: ", m_phase, ", To: ", value));
			m_phase = value;
			switch (m_phase)
			{
			case Phase.Calm:
				m_monster.Data.MinAttack = m_defaultMinAttack;
				m_monster.Data.MaxAttack = m_defaultMaxAttack;
				m_monster.Data.Agi = m_defaultAgility;
				m_monster.SpeedModifier = 1.5f;
				break;
			case Phase.Underground:
				m_monster.Data.MinAttack = m_defaultMinAttack * 2f;
				m_monster.Data.MaxAttack = m_defaultMaxAttack * 2f;
				StartCoroutine(DoDive());
				break;
			case Phase.Aggressive:
				Emerge();
				break;
			}
			m_slamHit = false;
			m_interuptable = true;
			m_dragon.slamming = false;
			m_comboList.Clear();
			Dbg.Log("m_slamHit: " + m_slamHit);
			Dbg.Log("m_interuptable: " + m_interuptable);
			Dbg.Log("m_dragon.slamming: " + m_dragon.slamming);
			Messenger<Phase>.Invoke(LandDragonEvent.LAND_DRAGON_PHASE_CHANGE, value);
		}
	}

	public Underground.Phase attackPhase
	{
		get
		{
			return m_attackPhase;
		}
		set
		{
			OnExitAttackPhase(m_attackPhase);
			m_attackPhase = value;
			OnEnterAttackPhase(m_attackPhase);
		}
	}

	public NBTimer phaseTimer
	{
		get
		{
			return m_phaseTimer;
		}
	}

	public NBTimer idleTimer
	{
		get
		{
			return m_idleTimer;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_vfx = GetComponent<LandDragonSpecialFx>();
		GameObject gameObject = GameObject.Find("Camera_Root");
		if (gameObject != null)
		{
			m_battleCamera = gameObject.GetComponent<BattleCamera>();
		}
		m_phaseTimer = base.gameObject.AddComponent<NBTimer>();
		Messenger.AddListener("player_dead", OnPlayerDead);
		Messenger<MonsterCollisionData>.AddListener("player_get_hit", OnPlayerHit);
	}

	protected override void Start()
	{
		base.Start();
		m_dragon = (LandDragon)m_monster;
		m_originalPosition = base.transform.position;
		m_defaultMinAttack = m_monster.Data.MinAttack;
		m_defaultMaxAttack = m_monster.Data.MaxAttack;
		m_defaultAgility = m_monster.Data.Agi;
		phase = Phase.Calm;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		m_phaseTimer.Pause(false);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		m_phaseTimer.Pause(true);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Messenger.RemoveListener("player_dead", OnPlayerDead);
		Messenger<MonsterCollisionData>.RemoveListener("player_get_hit", OnPlayerHit);
	}

	private void OnPlayerHit(MonsterCollisionData p_collData)
	{
		if (m_phase == Phase.Underground)
		{
			if (m_player.transform.position.sqrMagnitude >= BattleManager.squaredArenaRadius && m_phaseTimer.Counter > 0f)
			{
				m_phaseTimer.Stop();
			}
		}
		else if (m_phase == Phase.BodySlam && p_collData.attack != null && p_collData.attack.animationName == LandDragon.SLAM_ATK_ANIM)
		{
			m_slamHit = true;
		}
	}

	private void OnPlayerDead()
	{
		if (m_phase == Phase.Underground)
		{
			StartCoroutine(WaitTilAttackFinishThenEmerge());
		}
		else if (m_phase == Phase.BodySlam)
		{
			StartCoroutine(DiveToOrigin());
		}
		base.enabled = false;
	}

	private void OnPhaseTimerEnd()
	{
		switch (phase)
		{
		case Phase.Underground:
			StartCoroutine(BecomeAggressive());
			break;
		case Phase.Aggressive:
			StartCoroutine(PerformBodySlam());
			break;
		}
	}

	private void OnPlayerFinishKnockBack()
	{
		SingletonComponent<BattleScreenUI>.Instance.ShowBattleUiButtons(true);
		m_player.transform.LookAt(base.transform.position);
		m_player.Idle();
	}

	private void OnDiveEnd()
	{
		if (phase == Phase.Underground)
		{
			m_beforeDivingPosition = base.transform.position;
			m_phaseTimer.CountDown(m_phaseUndergroundDuration, OnPhaseTimerEnd);
			m_comboList.Clear();
			m_monster.Idle();
			m_monster.GetComponent<Collider>().enabled = false;
			m_battleCamera.InterpolateCamera("Top Camera", 1f, true);
			SingletonComponent<CutsceneController>.Instance.enabled = true;
		}
	}

	private void OnEmergeDown()
	{
		Dbg.Log("OnEmergeDown");
		if (m_monster.CurrentState != Monster.State.Intro)
		{
			m_monster.Idle();
			m_monster.Invincible = false;
			m_idleTimer.CountDown(2.5f);
			if (m_player.Data.Hp == 0f)
			{
				m_idleTimer.Pause(true);
			}
		}
		if (phase == Phase.Aggressive)
		{
			m_phaseTimer.CountDown(m_phaseAggressiveDuration, OnPhaseTimerEnd);
			if (m_player.Data.Hp == 0f)
			{
				m_phaseTimer.Pause(true);
			}
		}
	}

	private void OnAnimBegin(string p_anim)
	{
		if (base.enabled && p_anim == LandDragon.SLAM_ATK_ANIM)
		{
			StartCoroutine("DoJump");
		}
	}

	private void OnStopFeedbackFX()
	{
		SingletonComponent<BattleScreenUI>.Instance.ShowFeedbackFx(false, null);
	}

	protected override void OnAnimEnded(string p_anim)
	{
		if (!(p_anim != "Dive"))
		{
			return;
		}
		if (m_player != null && m_dragon != null && m_player.Data.Hp == 0f && !m_dragon.fullyUnderGround)
		{
			m_dragon.AnimPlayer.Play("Idle_01");
		}
		if (base.enabled)
		{
			if (p_anim == LandDragon.SLAM_ATK_ANIM)
			{
				OnJumpEnd();
			}
			else if (!m_dragon.diving)
			{
				base.OnAnimEnded(p_anim);
			}
		}
	}

	protected override void OnBattleStart()
	{
		base.OnBattleStart();
		Dbg.Log("Monster Phase Before Start: " + phase);
		if (phase == Phase.Aggressive && Mathf.Approximately(m_phaseTimer.Counter, 0f))
		{
			m_phaseTimer.CountDown(2f, OnPhaseTimerEnd);
		}
		else if (phase == Phase.BodySlam)
		{
			phase = Phase.Underground;
		}
		Dbg.Log("Monster Phase After Started: " + phase);
		if (m_dragon != null)
		{
			Dbg.Log("Fully Undergound: " + m_dragon.fullyUnderGround);
		}
	}

	protected override void OnCutsceneEnd(CutsceneController.CutsceneType p_type)
	{
		base.OnCutsceneEnd(p_type);
		if (phase == Phase.BodySlam)
		{
			phase = Phase.Underground;
		}
	}

	protected override void OnStunEnd()
	{
		if (phase == Phase.BodySlam)
		{
			phase = Phase.Underground;
		}
		else
		{
			base.OnStunEnd();
		}
	}

	protected override void OnStuckEnd()
	{
		if (phase == Phase.BodySlam)
		{
			phase = Phase.Underground;
		}
		else
		{
			base.OnStunEnd();
		}
	}

	protected void OnJumpEnd()
	{
		StopCoroutine("DoJump");
		m_dragon.slamming = false;
		m_dragon.Idle();
		if (!m_slamHit)
		{
			StunLogic instance = SingletonComponent<StunLogic>.Instance;
			if (instance != null)
			{
				instance.StunMonster(m_stunTime, "Stumble_Idle");
			}
		}
		else if (m_player.Data.Hp > 0f)
		{
			StartCoroutine(WaitThenDive());
		}
		m_slamHit = false;
		m_monster.Invincible = false;
		Messenger.Invoke(LandDragonEvent.LAND_DRAGON_FINISH_JUMP);
	}

	public void Emerge()
	{
		m_monster.GetComponent<Collider>().enabled = true;
		m_comboList.Clear();
		m_battleCamera.InterpolateCamera("Fighting Camera", 1f, true);
		BattleManager instance = SingletonComponent<BattleManager>.Instance;
		Vector3 originalPosition = m_originalPosition;
		base.transform.position = originalPosition;
		instance.BattleReferencePoint = originalPosition;
		base.transform.LookAt(m_player.transform.position);
		m_dragon.Emerge();
	}

	private IEnumerator BecomeAggressive()
	{
		Dbg.Log("Wait until attack finish.");
		while (attackPhase != Underground.Phase.Rest)
		{
			yield return null;
		}
		attackPhase = Underground.Phase.None;
		m_comboList.Clear();
		m_vfx.SetTrailActive(false);
		Dbg.Log("Wait until can be idle.");
		while (!m_monster.CanBecomeIdle() || !base.enabled)
		{
			yield return null;
		}
		Debug.Log("Aggressive");
		phase = Phase.Aggressive;
	}

	private IEnumerator PerformBodySlam()
	{
		m_comboList.Clear();
		m_interuptable = false;
		Dbg.Log("Wait until can be idle.");
		while (!m_dragon.CanJump() || !base.enabled)
		{
			yield return null;
		}
		base.enabled = false;
		if (m_player.Data.Hp == 0f)
		{
			Dbg.Log("Player died before doing body-slam");
			yield break;
		}
		m_dragon.Idle();
		base.enabled = true;
		Dbg.Log("Body Slam Attack!");
		MonsterAttack attack = new MonsterAttack
		{
			type = MonsterAttack.Type.Ranged,
			animationName = LandDragon.SLAM_ATK_ANIM
		};
		m_dragon.Attack(attack);
		Debug.Log("Body Slam");
		m_dragon.slamming = true;
		m_interuptable = true;
		phase = Phase.BodySlam;
	}

	private IEnumerator WaitThenDive()
	{
		while (m_player.CurrentState == Player.State.Hit)
		{
			yield return null;
		}
		phase = Phase.Underground;
	}

	protected override void UpdateGlobal()
	{
		if (phase == Phase.Calm && m_monster.Data.HpPercent <= m_phaseCalmHpThreshold)
		{
			phase = Phase.Underground;
		}
		base.UpdateGlobal();
	}

	protected override void LateUpdate()
	{
		if (m_currentAttack != null && m_currentAttack.animationName != LandDragon.SLAM_ATK_ANIM && m_currentAttack.animationName != LandDragon.SNAP_ATK_ANIM && m_currentAttack.animationName != LandDragon.RAKE_ATK_ANIM)
		{
			base.LateUpdate();
		}
	}

	protected override void UpdateIdle()
	{
		if (phase == Phase.Underground)
		{
			if (m_monster.AnimPlayer.CurrentAnim == null)
			{
				AnimationState animationState = base.GetComponent<Animation>().PlayQueued("Dive", QueueMode.PlayNow);
				animationState.normalizedTime = 1f;
				Attack();
			}
			else
			{
				if (m_monster.AnimPlayer.CurrentAnim == null || !m_dragon.diving || !m_dragon.fullyUnderGround)
				{
					return;
				}
				if (m_idleTimer.Counter == 0f)
				{
					WrapMode wrapMode = m_monster.AnimPlayer.CurrentAnim.wrapMode;
					if (wrapMode == WrapMode.Loop || !m_monster.AnimPlayer.IsPlaying)
					{
						Attack();
					}
				}
				else if (m_playerDir != 0 && !m_monster.IsTurning())
				{
					m_monster.Turn(m_player.transform.position);
				}
			}
		}
		else if (phase != Phase.BodySlam)
		{
			if (m_monster.AnimPlayer.CurrentAnimName != "Emerge")
			{
				base.UpdateIdle();
			}
		}
		else
		{
			if (phase != Phase.BodySlam || m_dragon.slamming)
			{
				return;
			}
			float lookAngle = base.transform.GetLookAngle(m_player.transform.position);
			if (!m_monster.IsTurning())
			{
				float num = Mathf.Abs(lookAngle);
				if (num > 10f)
				{
					m_monster.Turn(m_player.transform.position);
				}
				else
				{
					Attack();
				}
			}
		}
	}

	public override void Attack()
	{
		if (phase == Phase.Calm || phase == Phase.Aggressive)
		{
			base.Attack();
		}
		else if (phase == Phase.Underground)
		{
			PrepareSignatureComboString();
			if (m_comboList.Count == 0)
			{
				return;
			}
			MonsterAttack monsterAttack = new MonsterAttack();
			monsterAttack.type = m_comboList[0];
			if (m_comboList.Count > 0)
			{
				m_comboList.RemoveAt(0);
			}
			string text = string.Empty;
			if (monsterAttack.type == MonsterAttack.Type.Ranged)
			{
				text = LandDragon.RAKE_ATK_ANIM;
			}
			else if (monsterAttack.type == MonsterAttack.Type.Signature)
			{
				text = LandDragon.SNAP_ATK_ANIM;
			}
			if (text != string.Empty)
			{
				monsterAttack.animationName = text;
				if (m_monster.ChangeState(Monster.State.Attack))
				{
					m_currentAttack = monsterAttack;
					attackPhase = Underground.Phase.GroundTrail;
				}
			}
		}
		else if (phase != Phase.BodySlam)
		{
		}
	}

	private void OnEnterAttackPhase(Underground.Phase p_next)
	{
		switch (p_next)
		{
		case Underground.Phase.GroundTrail:
		{
			m_attackType = UnityEngine.Random.Range(0, 2);
			float num = 0f;
			if (m_attackType == 0)
			{
				m_rakeTime = 0f;
				num = m_rakeDuration;
				m_currentAttack = CreateAttackFromAnimInfo(LandDragon.RAKE_ATK_ANIM, MonsterAttack.Type.Ranged);
				base.transform.LookAt(m_player.transform);
				Vector3 position = Vector3.right * m_attackDirection * 4f + Vector3.forward * 3f;
				base.transform.LookAt(base.transform.TransformPoint(position));
				m_monster.AnimPlayer.Play(LandDragon.RAKE_ATK_ANIM);
				m_rakeDuration = m_monster.AnimPlayer.CurrentAnim.length;
			}
			else
			{
				m_trailTime = 0f;
				num = m_trailDuration;
				m_attackDirection = -m_attackDirection;
				m_currentAttack = CreateAttackFromAnimInfo(LandDragon.SNAP_ATK_ANIM, MonsterAttack.Type.Heavy);
				m_vfx.SetTrailActive(true);
			}
			if (m_geyserAttacks != null)
			{
				m_geyserDuration = (num - 0.5f) / 4f;
			}
			break;
		}
		case Underground.Phase.AttackSign:
			m_signTime = 0f;
			m_vfx.SetTrailActive(false);
			if (m_attackType == 1)
			{
				m_snapPosition = m_player.transform.position;
				m_vfx.SetSignActive(true, m_snapPosition);
			}
			break;
		case Underground.Phase.AttackWait:
			m_waitTime = 0f;
			if (m_attackType == 0)
			{
				m_waitDuration = 0.5f;
			}
			else if (m_attackType == 1)
			{
				m_waitDuration = 0.2f;
			}
			break;
		case Underground.Phase.Attack:
			if (m_attackType == 0)
			{
				m_currentAttack.type = MonsterAttack.Type.Ranged;
				m_currentAttack.animationName = LandDragon.RAKE_ATK_ANIM;
				m_attackDirection = -m_attackDirection;
				Vector3 vector = m_player.transform.TransformPoint(Vector3.right * -m_attackDirection);
				base.transform.LookAt(vector);
				base.transform.position = vector - base.transform.forward * 7f;
				m_monster.Attack(m_currentAttack, 0f);
			}
			else if (m_attackType == 1)
			{
				base.transform.position = m_snapPosition;
				m_currentAttack.type = MonsterAttack.Type.Signature;
				m_currentAttack.animationName = LandDragon.SNAP_ATK_ANIM;
				m_monster.Attack(m_currentAttack, 0f);
			}
			break;
		case Underground.Phase.Rest:
			base.transform.position = m_beforeDivingPosition;
			m_restTime = 0f;
			break;
		}
	}

	private void OnExitAttackPhase(Underground.Phase p_prev)
	{
	}

	protected override void UpdateAttack()
	{
		if (phase == Phase.Underground)
		{
			switch (m_attackPhase)
			{
			case Underground.Phase.GroundTrail:
				if (m_attackType == 0)
				{
					m_rakeTime += Time.deltaTime;
					base.transform.Translate(Vector3.forward * Time.deltaTime * 5f);
					if (m_rakeTime >= m_rakeDuration)
					{
						attackPhase = Underground.Phase.AttackSign;
					}
				}
				else if (m_attackType == 1)
				{
					base.transform.LookAt(m_player.transform);
					m_trailTime += Time.deltaTime;
					float num = m_trailTime / m_trailDuration;
					float num2 = Mathf.Lerp(m_trailMinAngle, m_trailMaxAngle, num);
					float num3 = Mathf.Lerp(m_trailMinRadius, m_trailMaxRadius, num);
					float f = num2 * ((float)Math.PI / 180f);
					float num4 = Mathf.Cos(f) * num3;
					float z = Mathf.Sin(f) * num3;
					if (m_attackDirection < 0)
					{
						num4 = 0f - num4;
					}
					m_trailPosition.x = num4;
					m_trailPosition.z = z;
					m_trailPosition.y = 0f;
					m_trailPosition = base.transform.TransformPoint(m_trailPosition);
					m_vfx.SetTrailPosition(m_trailPosition);
					if (num >= 1f)
					{
						attackPhase = Underground.Phase.AttackSign;
					}
				}
				break;
			case Underground.Phase.AttackSign:
				m_signTime += Time.deltaTime;
				if (m_signTime >= m_signDuration)
				{
					attackPhase = Underground.Phase.AttackWait;
				}
				break;
			case Underground.Phase.AttackWait:
				m_waitTime += Time.deltaTime;
				if (m_waitTime >= m_waitDuration)
				{
					attackPhase = Underground.Phase.Attack;
				}
				break;
			case Underground.Phase.Attack:
				if (m_attackType == 0)
				{
					base.transform.Translate(Vector3.forward * Time.deltaTime * 5f);
				}
				if (!m_monster.AnimPlayer.IsPlaying)
				{
					attackPhase = Underground.Phase.Rest;
				}
				break;
			case Underground.Phase.Rest:
				m_restTime += Time.deltaTime;
				if (m_restTime >= m_restDuration)
				{
					attackPhase = Underground.Phase.GroundTrail;
				}
				break;
			}
			if (!(m_geyserDuration > 0f))
			{
				return;
			}
			m_geyserDuration -= Time.deltaTime;
			if (m_geyserDuration <= 0f)
			{
				Vector3 position = m_player.transform.position;
				Vector3 vector = position - base.transform.position;
				float num5 = 57.29578f * (m_player.StrafeDistance / vector.magnitude);
				Quaternion quaternion = Quaternion.AngleAxis(num5, Vector3.up);
				Vector3 p_pos = base.transform.position + quaternion * vector;
				quaternion = Quaternion.AngleAxis(0f - num5, Vector3.up);
				Vector3 p_pos2 = base.transform.position + quaternion * vector;
				int num6 = UnityEngine.Random.Range(0, 2);
				m_geyserAttacks[0].Explode(position, 0f);
				if (num6 == 0)
				{
					m_geyserAttacks[1].Explode(p_pos, 0.75f);
					m_geyserAttacks[2].Explode(p_pos2, 1.5f);
				}
				else
				{
					m_geyserAttacks[1].Explode(p_pos2, 0.75f);
					m_geyserAttacks[2].Explode(p_pos, 1.5f);
				}
				m_geyserDuration = 0f;
			}
		}
		else if (phase != Phase.BodySlam)
		{
			TrackPlayerWhileAttacking();
		}
	}

	private IEnumerator DoDive()
	{
		base.enabled = false;
		SingletonComponent<CutsceneController>.Instance.enabled = false;
		StunLogic stunLogic = SingletonComponent<StunLogic>.Instance;
		while (!m_dragon.CanDive() || (stunLogic != null && stunLogic.IsMonsterStunning()))
		{
			yield return null;
		}
		if (!m_player.IsDead())
		{
			base.enabled = true;
			m_monster.Invincible = true;
			m_dragon.Dive();
		}
	}

	private IEnumerator DoJump()
	{
		m_monster.Invincible = true;
		Messenger.Invoke(LandDragonEvent.LAND_DRAGON_JUMP);
		Rotor rotor = m_dragon.GetComponent<Rotor>();
		while (m_monster.CurrentAttack != null && m_monster.CurrentAttack.phase.CompareTo(MonsterAttack.Phase.DamageWindow) < 0)
		{
			if (rotor != null)
			{
				rotor.BeginRotate(m_player.transform.position, 0.5f);
			}
			yield return null;
		}
		if (m_monster.CurrentState != Monster.State.Attack || m_monster.AnimPlayer.CurrentAnimName != LandDragon.SLAM_ATK_ANIM)
		{
			m_monster.StopTurn();
			yield break;
		}
		SingletonComponent<BattleManager>.Instance.BattleReferencePoint = m_player.transform.position;
		SingletonComponent<BattleScreenUI>.Instance.ShowFeedbackFx(true, null);
		Invoke("OnStopFeedbackFX", 2f);
		CharacterMovement moveScript = m_monster.GetComponent<CharacterMovement>();
		Dbg.Log("m_dragon jump to: " + SingletonComponent<BattleManager>.Instance.BattleReferencePoint);
		MonsterAttackParameter attackParam = m_monster.AttackParameterDB[LandDragon.SLAM_ATK_ANIM];
		float dist = (base.transform.position - SingletonComponent<BattleManager>.Instance.BattleReferencePoint).magnitude;
		float jumpTime = (attackParam.windownWindows[0] - attackParam.damageWindows[0]) / m_monster.AnimPlayer.CurrentAnim.speed;
		float speed = dist / jumpTime;
		moveScript.MoveTo(SingletonComponent<BattleManager>.Instance.BattleReferencePoint, speed, speed, 0f, OnJumpEnd);
	}

	private IEnumerator WaitTilAttackFinishThenEmerge()
	{
		if (!(m_dragon == null))
		{
			m_dragon.IsReady = false;
			while (m_dragon.CurrentState == Monster.State.Attack && m_dragon.AnimPlayer.IsPlaying)
			{
				yield return null;
			}
			if (m_dragon.fullyUnderGround)
			{
				phase = Phase.Aggressive;
			}
			else
			{
				m_phase = Phase.Aggressive;
			}
			while (m_dragon.diving)
			{
				yield return true;
			}
			m_dragon.IsReady = true;
		}
	}

	private IEnumerator DiveToOrigin()
	{
		m_dragon.IsReady = false;
		m_dragon.Dive();
		Waiter waiter = new Waiter();
		yield return StartCoroutine(waiter.WaitUntil(4f));
		m_dragon.transform.position = Vector3.zero;
		if (SingletonComponent<BattleManager>.Instance != null)
		{
			SingletonComponent<BattleManager>.Instance.BattleReferencePoint = m_dragon.transform.position;
		}
		m_dragon.Emerge();
		while (m_dragon.diving)
		{
			yield return true;
		}
		m_dragon.slamming = false;
		m_dragon.IsReady = true;
	}

	protected override void PrepareComboString(int p_comboCount)
	{
		switch (phase)
		{
		case Phase.Calm:
			PrepareNormalComboString();
			break;
		case Phase.Underground:
			PrepareSignatureComboString();
			break;
		case Phase.Aggressive:
			PrepareNormalComboString();
			break;
		case Phase.BodySlam:
			m_comboList.Clear();
			break;
		}
	}

	private void PrepareNormalComboString()
	{
		m_comboList.Clear();
		int num = UnityEngine.Random.Range(1, 4);
		for (int i = 0; i < num; i++)
		{
			int num2 = UnityEngine.Random.Range(0, 2);
			m_comboList.Add((num2 != 0) ? MonsterAttack.Type.Heavy : MonsterAttack.Type.Weak);
		}
	}

	private void PrepareSignatureComboString()
	{
		m_comboList.Clear();
		int num = 1;
		for (int i = 0; i < num; i++)
		{
			int num2 = UnityEngine.Random.Range(0, 2);
			m_comboList.Add((num2 != 0) ? MonsterAttack.Type.Signature : MonsterAttack.Type.Ranged);
		}
	}

	private MonsterAttack CreateAttackFromAnimInfo(string animName, MonsterAttack.Type attackType)
	{
		MonsterAttack monsterAttack = new MonsterAttack();
		monsterAttack.animationName = animName;
		monsterAttack.type = attackType;
		monsterAttack.knockSpeed = 0f;
		return monsterAttack;
	}
}
