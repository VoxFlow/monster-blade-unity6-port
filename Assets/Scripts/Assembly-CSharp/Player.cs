using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NBAnimation))]
public class Player : StateMachine
{
	public enum State
	{
		None = -1,
		Init = 0,
		Intro = 1,
		Idle = 2,
		Block = 3,
		Dodge = 4,
		SideStep = 5,
		Strafe = 6,
		RipostDodge = 7,
		Exhaust = 8,
		Hit = 9,
		Attack = 10,
		Stun = 11,
		Death = 12,
		NoMore = 13,
		Cutscene = 14,
		Max = 15
	}

	public enum HitReact
	{
		Bottom = 0,
		Top = 1,
		Left = 2,
		Right = 3
	}

	public enum DodgeDirection
	{
		None = -1,
		Left = 0,
		Right = 1
	}

	public enum ChargePose
	{
		None = -1,
		Top = 0,
		Bottom = 1,
		Left = 2,
		Right = 3,
		LeftTop = 4,
		RightTop = 5,
		LeftBottom = 6,
		RightBottom = 7
	}

	private static State[][] m_transitionList = new State[15][]
	{
		new State[0],
		new State[2]
		{
			State.Init,
			State.Idle
		},
		new State[13]
		{
			State.Init,
			State.Intro,
			State.Idle,
			State.Attack,
			State.Block,
			State.Strafe,
			State.RipostDodge,
			State.SideStep,
			State.Exhaust,
			State.Hit,
			State.Stun,
			State.Cutscene,
			State.NoMore
		},
		new State[4]
		{
			State.Idle,
			State.Attack,
			State.Strafe,
			State.Hit
		},
		new State[2]
		{
			State.Idle,
			State.Attack
		},
		new State[3]
		{
			State.Idle,
			State.Attack,
			State.Strafe
		},
		new State[4]
		{
			State.Idle,
			State.Attack,
			State.SideStep,
			State.Strafe
		},
		new State[4]
		{
			State.Idle,
			State.Attack,
			State.SideStep,
			State.Strafe
		},
		new State[6]
		{
			State.Attack,
			State.Strafe,
			State.SideStep,
			State.Idle,
			State.Hit,
			State.Block
		},
		new State[7]
		{
			State.Idle,
			State.Attack,
			State.Block,
			State.Dodge,
			State.Strafe,
			State.SideStep,
			State.Exhaust
		},
		new State[5]
		{
			State.Idle,
			State.Attack,
			State.Block,
			State.Strafe,
			State.Cutscene
		},
		new State[1] { State.Block },
		new State[9]
		{
			State.Idle,
			State.Hit,
			State.Attack,
			State.Block,
			State.Dodge,
			State.Strafe,
			State.Exhaust,
			State.SideStep,
			State.Cutscene
		},
		new State[1] { State.Death },
		new State[10]
		{
			State.Init,
			State.Intro,
			State.Idle,
			State.Attack,
			State.Block,
			State.Strafe,
			State.SideStep,
			State.Exhaust,
			State.Hit,
			State.Stun
		}
	};

	private BattleManager m_battleManager;

	private PlayerAttack m_attack;

	private bool m_ready;

	public bool m_invincible;

	public bool m_regenerateHpAndImmortal;

	private NBTimer m_regenTimer;

	private float m_regenDelay = 3f;

	private ShadowProjector m_shadow;

	protected ModelFader m_fader;

	private float m_ripostAtkModifier = 1.5f;

	private float m_repelAtkModifier = 1.5f;

	private VoidCallback m_finishMoveCallback;

	private Rotor m_rotor;

	private bool m_readyToAttack = true;

	public PlayerAttack[] m_atkString;

	private float[] m_atkExpTimes;

	private int m_atkCount;

	public int m_maxAtkCount = 5;

	public float m_atkExpirationTime = 0.65f;

	private Collider m_bodyCollider;

	private Collider m_swordCollider;

	private int m_combo;

	private int m_comboMove;

	private float m_comboTime = 1.25f;

	private bool m_figureOfEight;

	private NBTimer m_comboRecognitionTimer;

	private bool m_enableTrail;

	private TrailRenderer m_swordTrail;

	private PlayerAttackParameterDB m_attackParameterDB;

	private NBTimer m_blockTimer;

	private readonly float m_minBlockPeriod = 0.5f;

	public float m_figureOfEightSpeedModifier = 0.2f;

	private bool m_dodgeSuccess;

	private NBAnimation m_anim;

	private PlayerData m_data;

	private OrbitAroundTarget3D m_orbit;

	public float m_sideStepSpeed = 2f;

	public float m_strafeDistance = 5f;

	private float m_stamina = 1f;

	private float m_staminaRecharge = 0.18f;

	private float m_exhaustRecovery = 0.4f;

	public string[] strafeAnimations = new string[2] { "StrafeL", "StrafeR" };

	private string m_idleAnim = "Idle";

	private string m_deadAnim = "Death";

	private string m_blockAnim = "Block";

	public State m_currentState = State.None;

	private static string[] m_lightAtkAnims = new string[8] { "Att_Top_Light", "Att_Down_Light", "Att_Left_Light", "Att_Right_Light", "Att_Lefttop_Light", "Att_Righttop_Light", "Att_Leftdown_Light", "Att_Rightdown_Light" };

	private static string[] m_atkAnims = new string[8] { "Att_Top", "Att_Down", "Att_Left", "Att_Right", "Att_Lefttop", "Att_Righttop", "Att_Leftdown", "Att_Rightdown" };

	private static string[] m_heavyAtkAnims = new string[8] { "Att_Top_Heavy", "Att_Down_Heavy", "Att_Left_Heavy", "Att_Right_Heavy", "Att_Lefttop_Heavy", "Att_Righttop_Heavy", "Att_Leftdown_Heavy", "Att_Rightdown_Heavy" };

	private static string[] m_coolAtkAnims = new string[8] { "Att_Top_Cool", "Att_Down_Cool", "Att_Left_Cool", "Att_Right_Cool", "Att_Lefttop_Cool", "Att_Righttop_Cool", "Att_Leftdown_Cool", "Att_Rightdown_Cool" };

	public bool IsPlayerRecovered = true;

	protected CharacterMovement m_moveScript;

	public new State CurrentState
	{
		get
		{
			return (State)(object)base.CurrentState;
		}
	}

	public ModelFader fader
	{
		get
		{
			return m_fader;
		}
	}

	public string IdleAnim
	{
		get
		{
			return m_idleAnim;
		}
		set
		{
			m_idleAnim = value;
		}
	}

	public string DeadAnim
	{
		get
		{
			return m_deadAnim;
		}
		set
		{
			m_deadAnim = value;
		}
	}

	public string BlockAnim
	{
		get
		{
			return m_blockAnim;
		}
		set
		{
			m_blockAnim = value;
		}
	}

	public PlayerData Data
	{
		get
		{
			return m_data;
		}
		set
		{
			CollisionData component = m_swordCollider.gameObject.GetComponent<CollisionData>();
			component.attackerData = value;
			m_data = value;
		}
	}

	public bool IsReady
	{
		get
		{
			return m_ready;
		}
		set
		{
			m_ready = value;
		}
	}

	public bool IsReadyToAttack
	{
		get
		{
			return m_readyToAttack;
		}
	}

	public PlayerAttack CurrentAttack
	{
		get
		{
			return m_attack;
		}
		set
		{
			m_attack = value;
		}
	}

	public bool FigureOfEight
	{
		get
		{
			return m_figureOfEight;
		}
	}

	public bool DodgeSuccess
	{
		get
		{
			return m_dodgeSuccess;
		}
		set
		{
			m_dodgeSuccess = value;
		}
	}

	public NBAnimation AnimPlayer
	{
		get
		{
			return m_anim;
		}
	}

	public float StrafeDistance
	{
		get
		{
			return m_strafeDistance;
		}
	}

	public float SideStepSpeed
	{
		get
		{
			return m_sideStepSpeed;
		}
	}

	public float Stamina
	{
		get
		{
			return m_stamina;
		}
		set
		{
			m_stamina = Mathf.Clamp01(value);
		}
	}

	public int ComboCount
	{
		get
		{
			m_combo = Mathf.Clamp(m_combo, 0, 99);
			return m_combo;
		}
		set
		{
			m_combo = Mathf.Clamp(value, 0, 99);
		}
	}

	public float ComboRecognitionTime
	{
		get
		{
			return m_comboTime;
		}
	}

	public NBTimer ComboRecognitionTimer
	{
		get
		{
			return m_comboRecognitionTimer;
		}
	}

	public bool Invincible
	{
		get
		{
			return m_invincible;
		}
		set
		{
			m_invincible = value;
		}
	}

	public static string[] LightAttacks
	{
		get
		{
			return m_lightAtkAnims;
		}
	}

	public static string[] NormalAttacks
	{
		get
		{
			return m_atkAnims;
		}
	}

	public static string[] HeavyAttacks
	{
		get
		{
			return m_heavyAtkAnims;
		}
	}

	public static string[] CoolAttacks
	{
		get
		{
			return m_coolAtkAnims;
		}
	}

	public float AttackSpeedModifier { get; set; }

	public Collider SwordCollider
	{
		get
		{
			return m_swordCollider;
		}
	}

	public Collider BodyCollider
	{
		get
		{
			return m_bodyCollider;
		}
	}

	public CutsceneController.CutsceneType DeadCutscene { get; set; }

	public Player()
	{
		List<Enum>[] array = new List<Enum>[15];
		for (int i = 0; i < m_transitionList.Length; i++)
		{
			array[i] = new List<Enum>();
			State[] array2 = m_transitionList[i];
			foreach (State state in array2)
			{
				array[i].Add(state);
			}
		}
		InitTransitionTable(array);
	}

	public override bool ChangeState(Enum p_next)
	{
		if (p_next.Equals(State.Death) && m_regenerateHpAndImmortal)
		{
			return false;
		}
		if (m_data.Hp == 0f && !p_next.Equals(State.Cutscene) && !p_next.Equals(State.Death) && !p_next.Equals(State.NoMore))
		{
			return false;
		}
		return base.ChangeState(p_next);
	}

	protected override void OnEnter(Enum p_previous, Enum p_current)
	{
		switch ((State)(object)p_current)
		{
		case State.Idle:
			IsReady = true;
			m_readyToAttack = true;
			EnableCollision(true);
			m_dodgeSuccess = false;
			if (m_attack != null)
			{
				m_attack.type = PlayerAttack.Type.None;
			}
			break;
		case State.Attack:
			m_comboRecognitionTimer.Reset();
			break;
		case State.Exhaust:
			EnableCollision(true);
			break;
		case State.Block:
			IsReady = false;
			EnableCollision(true);
			break;
		case State.Hit:
			IsReady = false;
			break;
		case State.RipostDodge:
			EnableCollision(false);
			break;
		case State.Death:
			m_data.Hp = 0f;
			m_swordCollider.enabled = false;
			EnableCollision(false);
			m_anim.enabled = true;
			m_anim.Play(m_deadAnim);
			m_rotor.StopRotate();
			m_moveScript.StopMove();
			Messenger.Invoke("player_dead");
			break;
		case State.Cutscene:
			IsReady = true;
			m_readyToAttack = true;
			m_moveScript.StopMove();
			m_orbit.StopOrbit();
			break;
		case State.NoMore:
			IsReady = true;
			break;
		case State.Dodge:
		case State.SideStep:
		case State.Strafe:
		case State.Stun:
			break;
		}
	}

	protected override void OnExit(Enum p_current, Enum p_next)
	{
		switch ((State)(object)p_current)
		{
		case State.Intro:
			EnableCollision(true);
			break;
		case State.Attack:
			m_readyToAttack = true;
			m_swordCollider.enabled = false;
			m_anim.TimeScale = 1f;
			m_comboRecognitionTimer.CountDown(m_comboTime, OnComboTimerEnd);
			break;
		case State.SideStep:
		case State.Strafe:
			IsReady = true;
			m_orbit.StopOrbit();
			break;
		case State.RipostDodge:
			EnableCollision(true);
			m_comboRecognitionTimer.Pause(false);
			break;
		case State.Block:
			Messenger.Invoke("player_finish_block");
			break;
		case State.Hit:
			IsReady = true;
			break;
		case State.Idle:
		case State.Dodge:
		case State.Exhaust:
			break;
		}
	}

	public void Awake()
	{
		SkinnedMeshRenderer componentInChildren = MBHier.GetComponentInChildren<SkinnedMeshRenderer>(base.gameObject);
		componentInChildren.updateWhenOffscreen = true;
		m_anim = base.gameObject.GetComponent<NBAnimation>();
		m_regenTimer = base.gameObject.AddComponent<NBTimer>();
		m_regenTimer.Name = "Regen Timer";
		m_blockTimer = base.gameObject.AddComponent<NBTimer>();
		m_blockTimer.Name = "Block Timer";
		m_comboRecognitionTimer = base.gameObject.AddComponent<NBTimer>();
		m_comboRecognitionTimer.Name = "Combo Timer";
		m_comboRecognitionTimer.IsRealtime = false;
		Transform transform = base.transform.FindInHierarchy("Male_ Spine1");
		CollisionDetector collisionDetector = transform.gameObject.AddComponent<CollisionDetector>();
		collisionDetector.collisionCallback = OnCollide;
		BoxCollider boxCollider = transform.gameObject.AddComponent<BoxCollider>();
		boxCollider.center = new Vector3(-0.1f, 0f, 0f);
		boxCollider.size = new Vector3(1.35f, 1.1f, 1.1f);
		boxCollider.isTrigger = true;
		boxCollider.gameObject.tag = "Player";
		boxCollider.enabled = false;
		m_bodyCollider = boxCollider;
		Transform transform2 = base.transform.FindInHierarchy("Sword_Bone");
		boxCollider = transform2.gameObject.AddComponent<BoxCollider>();
		boxCollider.center = new Vector3(-2f, 0f, 0f);
		boxCollider.size = new Vector3(3f, 0.3f, 0.3f);
		boxCollider.gameObject.tag = "Player";
		boxCollider.enabled = false;
		m_swordCollider = boxCollider;
		CollisionData collisionData = m_swordCollider.gameObject.AddComponent<PlayerCollisionData>();
		collisionData.attacker = base.gameObject;
		Rigidbody rigidbody = transform2.gameObject.AddComponent<Rigidbody>();
		rigidbody.isKinematic = true;
		rigidbody.useGravity = false;
		rigidbody.angularDamping = 0f;
		rigidbody.freezeRotation = true;
		rigidbody.interpolation = RigidbodyInterpolation.None;
		rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
		UnityEngine.Object original = Resources.Load("Prefabs/BattleView/TrailRenderer");
		GameObject gameObject = UnityEngine.Object.Instantiate(original) as GameObject;
		gameObject.transform.parent = transform2;
		m_swordTrail = gameObject.GetComponent<TrailRenderer>();
		m_swordTrail.enabled = false;
		m_atkString = new PlayerAttack[m_maxAtkCount];
		m_atkExpTimes = new float[m_maxAtkCount];
		m_atkCount = 0;
		base.gameObject.AddComponent<SmoothLookAt>();
		m_moveScript = base.gameObject.AddComponent<CharacterMovement>();
		m_rotor = base.gameObject.AddComponent<Rotor>();
		m_orbit = base.gameObject.AddComponent<OrbitAroundTarget3D>();
		m_fader = base.gameObject.AddComponent<ModelFader>();
		m_fader.fadeShader = Shader.Find("Mobile/Monster Slayer/Alpha Blend For Textured Faders");
		AttackSpeedModifier = 1f;
		DeadCutscene = CutsceneController.CutsceneType.NONE;
		Messenger.AddListener("battle_start", OnBattleStart);
		Messenger.AddListener("battle_end", OnBattleEnd);
		Messenger<CutsceneController.CutsceneType>.AddListener(CutsceneMessage.CUTSCENE_START, OnCutsceneStart);
		Messenger<CutsceneController.CutsceneType>.AddListener(CutsceneMessage.CUTSCENE_END, OnCutsceneEnd);
		base.gameObject.AddComponent<PlayerDebugGUI>();
	}

	public void Start()
	{
		m_shadow = MBHier.GetComponentInChildren<ShadowProjector>(base.gameObject);
		if (m_shadow == null)
		{
			Dbg.LogError("Player doesn't have 'ShadowProjector'.");
		}
		else
		{
			InitAttackParameters();
		}
	}

	private void OnDestroy()
	{
		Messenger.RemoveListener("battle_start", OnBattleStart);
		Messenger.RemoveListener("battle_end", OnBattleEnd);
		Messenger<CutsceneController.CutsceneType>.RemoveListener(CutsceneMessage.CUTSCENE_START, OnCutsceneStart);
		Messenger<CutsceneController.CutsceneType>.RemoveListener(CutsceneMessage.CUTSCENE_END, OnCutsceneEnd);
	}

	public bool CanAttack()
	{
		return IsReachableFromCurrent(State.Attack) && IsReady && m_readyToAttack && Stamina > 0f;
	}

	public bool CanBecomeIdle()
	{
		return IsReachableFromCurrent(State.Idle) && IsReady && !m_moveScript.IsMoving();
	}

	public bool CanPlayCutscene()
	{
		return IsReachableFromCurrent(State.Cutscene);
	}

	public bool CanStrafe()
	{
		return (IsReachableFromCurrent(State.Strafe) || IsReachableFromCurrent(State.RipostDodge)) && Stamina > 0f && !IsMoving() && IsReady;
	}

	public void Exhaust()
	{
		if (ChangeState(State.Exhaust))
		{
			m_anim.Play("Exhaustion");
		}
	}

	public void FixedUpdate()
	{
		m_currentState = CurrentState;
		if (m_currentState == State.Init && !IsReady)
		{
			m_battleManager = SingletonComponent<BattleManager>.Instance;
			m_anim.Play("Idle");
			IsReady = true;
			m_readyToAttack = true;
		}
	}

	public void EnableCollision(bool p_enable)
	{
		m_bodyCollider.enabled = p_enable;
	}

	public void PerformAttackSequence(PlayerAttack p_atk, float p_blendTime = 0.15f)
	{
		ChangeState(State.Attack);
		m_readyToAttack = false;
		m_swordCollider.enabled = false;
		float num = 1f;
		m_figureOfEight = m_attack != null && PlayerAttack.IsFigureOfEight(m_attack.direction, p_atk.direction);
		if (p_atk.animation == string.Empty)
		{
			switch (m_comboMove)
			{
			case 0:
				p_atk.animation = m_lightAtkAnims[(int)p_atk.direction];
				break;
			case 1:
				p_atk.animation = m_atkAnims[(int)p_atk.direction];
				break;
			case 2:
				if (m_figureOfEight)
				{
					p_atk.animation = m_coolAtkAnims[(int)p_atk.direction];
				}
				else
				{
					p_atk.animation = m_lightAtkAnims[(int)p_atk.direction];
				}
				break;
			case 3:
				p_atk.animation = m_lightAtkAnims[(int)p_atk.direction];
				break;
			case 4:
				if (m_figureOfEight)
				{
					p_atk.animation = m_atkAnims[(int)p_atk.direction];
				}
				else
				{
					p_atk.animation = m_coolAtkAnims[(int)p_atk.direction];
				}
				break;
			case 5:
				if (m_figureOfEight)
				{
					p_atk.animation = m_coolAtkAnims[(int)p_atk.direction];
				}
				else
				{
					p_atk.animation = m_atkAnims[(int)p_atk.direction];
				}
				break;
			case 6:
				p_atk.animation = m_coolAtkAnims[(int)p_atk.direction];
				SingletonComponent<BattleManager>.Instance.m_DamageToMonster += 5f;
				break;
			default:
				p_atk.animation = m_atkAnims[(int)p_atk.direction];
				break;
			}
			int num2 = 7;
			if (++m_comboMove >= num2)
			{
				m_comboMove = 1;
			}
		}
		if (p_atk.type == PlayerAttack.Type.None)
		{
			if (IsLightAttackAnimation(p_atk.animation))
			{
				p_atk.multiplier *= 0.5f;
				p_atk.type = PlayerAttack.Type.Normal;
			}
			else if (IsNormalAttackAnimation(p_atk.animation))
			{
				if (m_figureOfEight)
				{
					p_atk.multiplier *= 0.75f;
				}
				else
				{
					p_atk.multiplier *= 1f;
				}
				p_atk.type = PlayerAttack.Type.Normal;
			}
			else
			{
				if (!m_figureOfEight)
				{
					p_atk.multiplier *= 1.25f;
				}
				p_atk.type = PlayerAttack.Type.Heavy;
			}
			if (m_figureOfEight)
			{
				num += m_figureOfEightSpeedModifier;
			}
		}
		m_anim.TimeScale = num * AttackSpeedModifier;
		m_attack = p_atk;
		PlayerCollisionData component = m_swordCollider.gameObject.GetComponent<PlayerCollisionData>();
		component.attack = p_atk;
		StartCoroutine(m_anim.PlaySequence(p_atk.animation, p_blendTime));
		Messenger<PlayerAttack>.Invoke("player_begin_atk", p_atk);
	}

	public void RepelAttack(PlayerAttack.Direction p_dir)
	{
		ChangeState(State.Attack);
		PlayerAttack playerAttack = new PlayerAttack();
		playerAttack.direction = p_dir;
		playerAttack.type = PlayerAttack.Type.Repel;
		playerAttack.animation = CoolAttacks[(int)p_dir];
		playerAttack.multiplier = m_repelAtkModifier;
		m_attack = playerAttack;
		PlayerCollisionData component = m_swordCollider.gameObject.GetComponent<PlayerCollisionData>();
		component.attack = playerAttack;
		float p_fadeLength = 0.15f;
		m_anim.Play(playerAttack.animation, p_fadeLength);
		m_anim.CurrentAnim.time = 0.2f;
		Messenger<PlayerAttack>.Invoke("player_begin_atk", playerAttack);
	}

	public void RegisterAttackCommand(PlayerAttack p_atk)
	{
		if (m_atkCount < m_maxAtkCount)
		{
			m_atkString[m_atkCount] = p_atk;
			m_atkExpTimes[m_atkCount] = m_atkExpirationTime;
			m_atkCount++;
		}
	}

	public void ClearAttackList()
	{
		m_atkCount = 0;
	}

	private void PopAttackCommand()
	{
		for (int i = 0; i < m_atkCount - 1; i++)
		{
			m_atkString[i] = m_atkString[i + 1];
			m_atkExpTimes[i] = m_atkExpTimes[i + 1];
		}
		m_atkCount--;
	}

	public bool IsLightAttackAnimation(string p_anim)
	{
		string[] lightAtkAnims = m_lightAtkAnims;
		foreach (string text in lightAtkAnims)
		{
			if (text == p_anim)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsNormalAttackAnimation(string p_anim)
	{
		string[] atkAnims = m_atkAnims;
		foreach (string text in atkAnims)
		{
			if (text == p_anim)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsHeavyAttackAnimation(string p_anim)
	{
		string[] heavyAtkAnims = m_heavyAtkAnims;
		foreach (string text in heavyAtkAnims)
		{
			if (text == p_anim)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsCoolAttackAnimation(string p_anim)
	{
		string[] coolAtkAnims = m_coolAtkAnims;
		foreach (string text in coolAtkAnims)
		{
			if (text == p_anim)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsAttackAnimation(string p_anim)
	{
		return IsLightAttackAnimation(p_anim) || IsNormalAttackAnimation(p_anim) || IsHeavyAttackAnimation(p_anim) || IsCoolAttackAnimation(p_anim);
	}

	public void Idle()
	{
		if (CanBecomeIdle())
		{
			ChangeState(State.Idle);
			m_anim.Play(m_idleAnim, 0.2f);
			SendMessage("OnIdle", SendMessageOptions.DontRequireReceiver);
		}
	}

	public IEnumerator DelayIdle(float p_seconds, bool p_forceReady)
	{
		yield return new MBWaitForSeconds(p_seconds);   // PORT (CD-059)
		if (p_forceReady)
		{
			IsReady = true;
		}
		Idle();
	}

	public IEnumerator Revive()
	{
		m_anim.Play("Recovery");
		Messenger.Invoke("player_revived");
		Waiter waiter = new Waiter();
		yield return StartCoroutine(waiter.WaitUntil(2.6f));
		SendMessage("OnHeal", m_data.MaxHp);
		while (m_anim.CurrentAnimName == "Recovery" && m_anim.IsPlaying)
		{
			yield return new WaitForFixedUpdate();
		}
		m_data.ResetHp();
		Stamina = 1f;
		IsReady = true;
		Idle();
	}

	public void Init()
	{
		ChangeState(State.Init);
	}

	protected void InitAttackParameters()
	{
		m_attackParameterDB = Singleton<PlayerAttackParameterDB>.Instance;
		if (m_attackParameterDB == null)
		{
			Dbg.LogError("Player attack parameter database cannot be loaded.");
			return;
		}
		ParameterManager instance = SingletonComponent<ParameterManager>.Instance;
		NBBinaryFormat parametersFor = instance.GetParametersFor("MaleAttack");
		Dbg.Log("Initialize player attack parameters.");
		foreach (string rowsName in parametersFor.RowsNames)
		{
			string[] array = new string[5] { "OnDamageWindow,void,,", "OnWindDown,void,,", "OnAttackEnd,void,,", "OnTrailStart,void,,", "OnTrailEnd,void,," };
			string[] array2 = new string[5] { "DAMAGE_WINDOW", "WIND_DOWN", "COMBO_END", "TRAIL_START", "TRAIL_END" };
			Dbg.Log("Initialize attack parameters for: " + rowsName);
			for (int i = 0; i < array.Length; i++)
			{
				for (int j = 0; j < instance.GetNumberItemInCell("MaleAttack", rowsName, array2[i]); j++)
				{
					string parameter = instance.GetParameter("MaleAttack", rowsName, array2[i], j);
					m_anim.AddAnimEvent(rowsName, "[" + array[i] + parameter + "]");
				}
			}
			if (null != m_anim[rowsName])
			{
				m_anim[rowsName].wrapMode = WrapMode.ClampForever;
			}
		}
	}

	public bool IsBlocking()
	{
		return CurrentState == State.Block;
	}

	public bool IsMinBlockPeriodEnded()
	{
		return m_blockTimer.Counter == 0f;
	}

	public bool IsDead()
	{
		State currentState = CurrentState;
		return currentState == State.Death || currentState == State.NoMore;
	}

	public void OnKnockBackEnd()
	{
		if (m_data.Hp == 0f)
		{
			ChangeState(State.Death);
			m_anim.CurrentAnim.time = 1.8f;
		}
		else
		{
			m_anim.Play("Idle", 0.3f, true);
			if (m_finishMoveCallback != null)
			{
				m_finishMoveCallback();
			}
		}
		IsReady = true;
	}

	public void OnAnimEnded(string p_anim)
	{
		switch (CurrentState)
		{
		case State.Attack:
			Messenger<PlayerAttack>.Invoke("player_finish_atk", m_attack);
			if (m_atkCount > 0)
			{
				PerformAttackSequence(m_atkString[0]);
				PopAttackCommand();
			}
			else
			{
				Idle();
			}
			break;
		case State.Strafe:
			Idle();
			break;
		case State.RipostDodge:
			Messenger<PlayerAttack>.Invoke("player_finish_atk", m_attack);
			Idle();
			break;
		case State.Death:
			ChangeState(State.NoMore);
			break;
		case State.Intro:
			Idle();
			break;
		}
	}

	private void OnBattleStart()
	{
		EnableCollision(true);
	}

	private void OnBattleEnd()
	{
		EnableCollision(false);
	}

	public void OnCutsceneStart(CutsceneController.CutsceneType p_type)
	{
		base.enabled = false;
		m_atkCount = 0;
		EnableCollision(false);
	}

	public void OnCutsceneEnd(CutsceneController.CutsceneType p_type)
	{
		if (CutsceneController.IsMobDeathCutscene(p_type))
		{
			StartCoroutine(PlayWinningAnim());
			return;
		}
		EnableCollision(true);
		base.enabled = true;
	}

	private void OnDamageWindow()
	{
		if (CurrentState != State.Attack || m_attack == null || m_attack.animation == null)
		{
			return;
		}
		PlayerAttackParameter playerAttackParameter = m_attackParameterDB[m_attack.animation];
		if (playerAttackParameter != null)
		{
			if (m_attack.comboIndex < 0 || m_attack.comboIndex >= playerAttackParameter.directions.Length)
			{
				Dbg.LogWarn("Combo index out of range: " + m_attack.comboIndex);
				return;
			}
			m_attack.direction = m_attackParameterDB[m_attack.animation].directions[m_attack.comboIndex];
			m_swordCollider.enabled = true;
			m_attack.comboIndex++;
			ComboCount++;
			Messenger<PlayerAttack>.Invoke("player_damage_window", m_attack);
		}
	}

	private void OnWindDown()
	{
		m_swordCollider.enabled = false;
	}

	private void OnAttackEnd()
	{
		m_readyToAttack = true;
	}

	private void OnTrailStart()
	{
		if (m_enableTrail)
		{
			m_swordTrail.enabled = true;
		}
	}

	private void OnTrailEnd()
	{
		m_swordTrail.enabled = false;
	}

	private void OnCollide(Collider p_other)
	{
		if (!p_other.enabled || p_other.gameObject.tag != "Enemy")
		{
			return;
		}
		p_other.enabled = false;
		if (!m_dodgeSuccess)
		{
			MonsterCollisionData component = p_other.GetComponent<MonsterCollisionData>();
			Messenger<MonsterCollisionData>.Invoke("player_get_hit", component);
			if (CurrentState == State.Strafe)
			{
				SingletonComponent<BattleScreenUI>.Instance.ShowTextEffect(BattleScreenUI.TextEffect.DodgeFailed);
			}
		}
		else if (CurrentState == State.Strafe)
		{
			SingletonComponent<BattleScreenUI>.Instance.ShowTextEffect(BattleScreenUI.TextEffect.Dodge);
		}
	}

	private void OnLandOnGround()
	{
		m_orbit.StopOrbit();
		IsReady = true;
		Messenger.Invoke("player_land");
	}

	private void OnJump()
	{
	}

	private void OnMove()
	{
		m_atkCount = 0;
	}

	private void OnFinishRun()
	{
		if (m_finishMoveCallback != null)
		{
			m_finishMoveCallback();
		}
		IsReady = true;
		Idle();
	}

	private void OnComboTimerEnd()
	{
		ComboCount = 0;
		m_comboMove = 0;
	}

	public void PlayHitAnim(HitReact p_hit)
	{
		string[] array = new string[4] { "HitB", "HitR", "HitL", "HitT" };
		string p_anim = array[(int)p_hit];
		m_anim.Play(p_anim);
	}

	public void PlayHitHeavyAnim(HitReact p_hit)
	{
		string[] array = new string[4] { "HitBHeavy", "HitRHeavy", "HitLHeavy", "HitTHeavy" };
		string p_anim = array[(int)p_hit];
		m_anim.Play(p_anim);
	}

	public void PlayIntro()
	{
		if (ChangeState(State.Intro))
		{
			m_anim.Play("Intro", 0f);
		}
	}

	public IEnumerator PlayWinningAnim()
	{
		m_atkCount = 0;
		if (CurrentState == State.Block)
		{
			IsReady = true;
		}
		while (!CanBecomeIdle())
		{
			yield return null;
		}
		Idle();
		while (m_anim.CurrentAnimName == "Idle" && m_anim.CurrentAnim != null && m_anim.CurrentAnim.weight < 1f)
		{
			yield return null;
		}
		yield return StartCoroutine(m_anim.PlayAndWait("VictoryPose"));
		m_anim.Play("Victory_Idle");
	}

	public void SideStep(DodgeDirection p_dir)
	{
		if (!IsReady || !(Stamina > 0f) || !ChangeState(State.SideStep))
		{
			return;
		}
		float magnitude = (base.transform.position - SingletonComponent<BattleManager>.Instance.BattleReferencePoint).magnitude;
		if (magnitude > float.Epsilon)
		{
			string empty = string.Empty;
			float num = 0f;
			if (p_dir == DodgeDirection.Right)
			{
				num = -1f;
				empty = "SidestepR";
			}
			else
			{
				num = 1f;
				empty = "SidestepL";
			}
			float num2 = 57.29578f * (m_sideStepSpeed / (SingletonComponent<BattleManager>.Instance.BattleReferencePoint - base.transform.position).magnitude);
			m_orbit.Orbit(SingletonComponent<BattleManager>.Instance.BattleReferencePoint, num * num2, -1f);
			m_anim.Play(empty);
		}
		else
		{
			Dbg.LogWarn("Player and monster are being too close.");
		}
	}

	public void Strafe(DodgeDirection p_dir)
	{
		if (CanStrafe() && ChangeState(State.Strafe))
		{
			string text = strafeAnimations[(int)p_dir];
			float p_jumpTime = m_anim.GetEventTime(text, "OnLandOnGround") / m_anim[text].speed;
			BeginJump(p_dir, p_jumpTime);
			m_anim.Play(text, 0.05f);
			Messenger<DodgeDirection>.Invoke("player_strafe", p_dir);
		}
	}

	public void RipostDodge(DodgeDirection p_dir)
	{
		if (IsReady && ChangeState(State.RipostDodge))
		{
			StartCoroutine(PerformRipostDodge(p_dir));
			Messenger<DodgeDirection>.Invoke("perfect_dodge_start", p_dir);
		}
	}

	private IEnumerator PerformRipostDodge(DodgeDirection p_dir)
	{
		IsReady = false;
		m_comboRecognitionTimer.Pause(true);
		EnableCollision(false);
		Waiter waiter = new Waiter();
		if (p_dir == DodgeDirection.Left)
		{
			StartCoroutine(m_anim.PlaySequence("Dodge_Left_Perfect", 0.05f));
			yield return StartCoroutine(waiter.WaitUntil(0.25f));
			SingletonComponent<SlowMotionSystem>.Instance.LinearFade(10f, 0.4f);
			BeginJump(p_dir, 0.8f);
			yield return StartCoroutine(waiter.WaitUntil(1.1f));
		}
		else
		{
			StartCoroutine(m_anim.PlaySequence("Dodge_Right_Perfect", 0.05f));
			yield return StartCoroutine(waiter.WaitUntil(0.15f));
			SingletonComponent<SlowMotionSystem>.Instance.LinearFade(10f, 0.3f);
			BeginJump(p_dir, 0.6f);
			yield return StartCoroutine(waiter.WaitUntil(0.9f));
		}
		PlayerAttack attack = new PlayerAttack
		{
			type = PlayerAttack.Type.Ripost,
			direction = PlayerAttack.Direction.LeftToRight,
			multiplier = m_ripostAtkModifier,
			forceCleanHit = true
		};
		m_swordCollider.enabled = true;
		PlayerCollisionData collisionData = m_swordCollider.gameObject.GetComponent<PlayerCollisionData>();
		collisionData.attack = attack;
		ComboCount++;
		Messenger<PlayerAttack>.Invoke("player_begin_atk", attack);
		if (p_dir == DodgeDirection.Left)
		{
			yield return StartCoroutine(waiter.WaitUntil(0.4f));
		}
		else
		{
			yield return StartCoroutine(waiter.WaitUntil(0.6f));
		}
		OnLandOnGround();
		EnableCollision(true);
		Messenger.Invoke("perfect_dodge_end");
	}

	private void BeginJump(DodgeDirection p_dir, float p_jumpTime)
	{
		float magnitude = (base.transform.position - m_battleManager.BattleReferencePoint).magnitude;
		float num = 0f;
		if (p_jumpTime != 0f)
		{
			if (magnitude > float.Epsilon)
			{
				float num2 = m_strafeDistance / p_jumpTime;
				num = ((p_dir != DodgeDirection.Right) ? 1f : (-1f));
				num2 = num * 57.29578f * (num2 / magnitude);
				m_orbit.Orbit(m_battleManager.BattleReferencePoint, num2, p_jumpTime);
			}
			else
			{
				num = ((p_dir != DodgeDirection.Right) ? (-1f) : 1f);
				Vector3 vector = base.transform.right * num;
				Vector3 vector2 = base.transform.position + vector * m_battleManager.BattleReferenceDistance;
				float p_startSpeed = m_battleManager.BattleReferenceDistance / p_jumpTime;
				Vector3 normalized = (m_battleManager.BattleReferencePoint - vector2).normalized;
				m_moveScript.MoveTo(vector2, p_startSpeed, 0f);
				m_rotor.BeginRotate(Quaternion.LookRotation(normalized), p_jumpTime);
			}
			IsReady = false;
			Messenger.Invoke("player_jump");
		}
	}

	public void RefillHp(float p_life)
	{
		if (m_data.Hp != 0f && !IsDead())
		{
			m_data.Hp += p_life;
			SendMessage("OnHeal", p_life, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void DrainLife(float p_life)
	{
		if (m_data.Hp != 0f && !IsDead())
		{
			m_data.Hp += p_life;
			SendMessage("OnLifeSteal", p_life, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void TakeDamage(float p_dmg)
	{
		m_data.Hp -= p_dmg;
		m_attack = null;
		m_enableTrail = false;
		m_swordTrail.enabled = false;
		m_atkCount = 0;
	}

	public bool IsMoving()
	{
		return m_moveScript.IsMoving();
	}

	public void RunTo(Vector3 p_targetPos, float p_speed, VoidCallback p_endCallback = null)
	{
		if (Data == null || Data.Hp == 0f)
		{
			Dbg.LogWarn("Player was already dead. Cannot run.");
			return;
		}
		IsReady = false;
		m_finishMoveCallback = p_endCallback;
		m_moveScript.MoveTo(p_targetPos, 0f, p_speed, 0.65f, OnFinishRun);
		StartCoroutine(PrepareToRun());
	}

	public void KnockBack(float p_speed, VoidCallback p_endCallback = null)
	{
		KnockBack(p_speed, -base.transform.forward, p_endCallback);
	}

	public void KnockBack(float p_speed, Vector3 p_dir, VoidCallback p_endCallback = null)
	{
		string p_anim = "Knockback_01";
		float num = (base.GetComponent<Animation>()[p_anim].length - 0.5f) / base.GetComponent<Animation>()[p_anim].speed;
		float num2 = p_speed * num / 2f + 0.5f;
		Vector3 p_targetPos = base.transform.position + p_dir.normalized * num2;
		m_finishMoveCallback = p_endCallback;
		m_anim.Play(p_anim);
		m_moveScript.MoveTo(p_targetPos, p_speed, 1f, num, OnKnockBackEnd);
		m_orbit.StopOrbit();
	}

	public void KnockTo(Vector3 p_targetPos, VoidCallback p_endCallback = null)
	{
		string p_anim = "Knockback_01";
		float num = (base.GetComponent<Animation>()[p_anim].length - 0.5f) / base.GetComponent<Animation>()[p_anim].speed;
		float magnitude = (base.transform.position - p_targetPos).magnitude;
		float p_startSpeed = (magnitude - 0.5f) * 2f / num;
		m_finishMoveCallback = p_endCallback;
		m_anim.Play(p_anim);
		m_moveScript.MoveTo(p_targetPos, p_startSpeed, 1f, num, OnKnockBackEnd);
	}

	private IEnumerator PrepareToRun()
	{
		m_moveScript.enabled = false;
		yield return StartCoroutine(WaitTilIdle());
		m_moveScript.enabled = true;
		m_anim.Play("Run");
	}

	public void Block()
	{
		if (IsReachableFromCurrent(State.Block) && IsReady && Stamina > 0f)
		{
			ChangeState(State.Block);
			m_anim.Play(m_blockAnim, 0.15f);
			if (m_blockTimer != null)
			{
				m_blockTimer.CountDown(m_minBlockPeriod);
			}
			Messenger.Invoke("player_block");
		}
	}

	protected override void Update()
	{
		UpdateGlobal();
		switch (CurrentState)
		{
		case State.Attack:
			UpdateAttack();
			break;
		case State.Block:
			UpdateBlock();
			break;
		case State.SideStep:
			UpdateSideStep();
			break;
		case State.Strafe:
			UpdateStrafe();
			break;
		case State.RipostDodge:
			UpdateRipostDodge();
			break;
		case State.Hit:
			UpdateHit();
			break;
		case State.Dodge:
		case State.Exhaust:
			break;
		}
	}

	private void LateUpdate()
	{
		if (m_regenerateHpAndImmortal)
		{
			if (m_data.Hp <= 0f)
			{
				m_data.Hp = 1f;
			}
			if (m_data.Hp <= 1f && m_regenTimer.Counter == 0f)
			{
				m_regenTimer.CountDown(m_regenDelay, "OnRegen");
			}
		}
	}

	private void OnRegen()
	{
		m_data.HpPercent = 1f;
	}

	protected void UpdateGlobal()
	{
		if (Stamina <= 0f && (CurrentState == State.Idle || CurrentState == State.Block))
		{
			Exhaust();
			return;
		}
		if (CurrentState == State.Exhaust && Stamina >= m_exhaustRecovery)
		{
			IsReady = true;
			Idle();
		}
		bool flag = CurrentState == State.Idle || CurrentState == State.Exhaust || CurrentState == State.Hit;
		if (flag | (CurrentState == State.Attack && PowerMove.GetMoveEnum(m_anim.CurrentAnimName) == PowerMove.MoveEnum.None))
		{
			float num = ((CurrentState != State.Attack) ? m_staminaRecharge : (0.5f * m_staminaRecharge));
			Stamina += num * Time.deltaTime;
		}
		for (int i = 0; i < m_atkCount; i++)
		{
			m_atkExpTimes[i] -= Time.deltaTime;
			if (m_atkExpTimes[i] <= 0f)
			{
				PopAttackCommand();
			}
		}
		if (CanAttack() && m_atkCount > 0)
		{
			PerformAttackSequence(m_atkString[0]);
			PopAttackCommand();
		}
	}

	protected void UpdateAttack()
	{
	}

	protected void UpdateBlock()
	{
		if (!m_anim.IsPlaying)
		{
			m_anim.Play("Block");
		}
	}

	protected void UpdateSideStep()
	{
	}

	protected void UpdateStrafe()
	{
	}

	protected void UpdateRipostDodge()
	{
	}

	protected void UpdateHit()
	{
		if (m_anim.CurrentAnimName == "Tumble")
		{
			float num = 0.738f;
			if (m_anim.CurrentAnim.normalizedTime > num && !IsReady)
			{
				IsReady = true;
				Idle();
			}
		}
		if (!m_anim.IsPlaying)
		{
			IsReady = true;
			Idle();
		}
	}

	public IEnumerator WaitTilIdle()
	{
		while (m_anim.CurrentAnimName != "Idle")
		{
			yield return null;
		}
	}

	public void SetVisible(bool p_visible)
	{
		MBHier.GetComponentInChildren<SkinnedMeshRenderer>(base.gameObject).enabled = p_visible;
		if (m_shadow != null)
		{
			m_shadow.gameObject.active = p_visible;
		}
	}

	public bool IsVisible()
	{
		return base.gameObject.GetComponent<Renderer>().enabled && fader.currentAlpha > 0f;
	}
}
