using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : StateMachine
{
	public enum State
	{
		None = -1,
		Init = 0,
		Intro = 1,
		Idle = 2,
		Block = 3,
		Hit = 4,
		Attack = 5,
		Fall = 6,
		Stuck = 7,
		Wait = 8,
		Death = 9,
		NoMore = 10,
		Cutscene = 11,
		Max = 12
	}

	protected static State[][] m_transitionList = new State[12][]
	{
		new State[0],
		new State[1],
		new State[9]
		{
			State.Init,
			State.Intro,
			State.Idle,
			State.Block,
			State.Hit,
			State.Attack,
			State.Fall,
			State.Wait,
			State.Cutscene
		},
		new State[1] { State.Idle },
		new State[4]
		{
			State.Idle,
			State.Attack,
			State.Hit,
			State.Wait
		},
		new State[4]
		{
			State.Idle,
			State.Attack,
			State.Hit,
			State.Cutscene
		},
		new State[5]
		{
			State.Idle,
			State.Block,
			State.Attack,
			State.Hit,
			State.Wait
		},
		new State[1] { State.Attack },
		new State[5]
		{
			State.Init,
			State.Intro,
			State.Idle,
			State.Attack,
			State.Hit
		},
		new State[7]
		{
			State.Idle,
			State.Attack,
			State.Hit,
			State.Stuck,
			State.Wait,
			State.Fall,
			State.Cutscene
		},
		new State[1] { State.Death },
		new State[11]
		{
			State.Init,
			State.Intro,
			State.Idle,
			State.Block,
			State.Hit,
			State.Attack,
			State.Fall,
			State.Wait,
			State.Death,
			State.NoMore,
			State.Cutscene
		}
	};

	protected BattleManager m_battleManager;

	protected PlayerData m_data;

	protected bool m_ready;

	protected ShadowProjector m_shadow;

	protected SkinnedMeshRenderer[] m_renderers;

	public bool hasDeathCutscene;

	public bool hasIntroCutscene;

	public string weightGroup = string.Empty;

	protected NBTimer m_timer;

	protected MonsterAttack m_currentAttack = new MonsterAttack();

	protected MonsterAttack m_pendingAttack = new MonsterAttack();

	protected string[] m_normalAttacks;

	protected string[] m_advanceAttacks;

	protected string[] m_specialAttacks;

	protected string[] m_allAttacks;

	public bool m_canSnap = true;

	public string m_attackParametersCSV = string.Empty;

	protected List<string>[] m_attackAnimations;

	protected MonsterAttackParameterDB m_attackParameterDB;

	protected Collider[] m_attackPoints;

	protected float m_attackSpeed = 1f;

	protected float m_anticipationTimeBase = 0.7f;

	protected float m_anticipationTimeMod = 0.1f;

	protected float m_anticipationTime = 1f;

	public bool m_invincible;

	protected float m_ragePoint;

	protected float m_rageGainFactor = 0.15f;

	protected float m_rageLossFactor = 0.05f;

	public bool m_canRage;

	protected NBAnimation m_anim;

	protected string[] m_idleAnimations;

	protected float m_speedModifier = 1f;

	public float m_fadeSpeed = 0.5f;

	protected ModelFader[] m_faders;

	protected bool m_fading;

	[SerializeField]
	protected CollisionDetector m_bodyCollider;

	public State m_currentState = State.None;

	protected Rotor m_rotor;

	protected CharacterMovement m_moveScript;

	public new State CurrentState
	{
		get
		{
			return (State)(object)base.CurrentState;
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
			if (value != null)
			{
				m_data = value;
				float atk = m_data.Atk;
				m_data.MinAttack = atk - 5f;
				m_data.MaxAttack = atk + 5f;
				Collider[] attackPoints = m_attackPoints;
				foreach (Collider collider in attackPoints)
				{
					CollisionData component = collider.gameObject.GetComponent<CollisionData>();
					component.attackerData = value;
				}
				m_attackSpeed = m_anticipationTimeBase + (float)value.m_monsterData.difficulty * m_anticipationTimeMod;
				m_speedModifier = value.m_monsterData.attackAnimSpeed;
				hasIntroCutscene = (hasDeathCutscene = value.m_monsterData.isBoss);
				if ((float)value.m_monsterData.difficulty > 0f)
				{
					m_anticipationTime = 0.5f + value.Agi / 70f;
				}
			}
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

	public NBAnimation AnimPlayer
	{
		get
		{
			return m_anim;
		}
	}

	public List<string>[] AttackAnimations
	{
		get
		{
			return m_attackAnimations;
		}
	}

	public string[] NormalAttackAnimations
	{
		get
		{
			return m_normalAttacks;
		}
	}

	public string[] AdvanceAttackAnimations
	{
		get
		{
			return m_advanceAttacks;
		}
	}

	public string[] SpecialAttackAnimations
	{
		get
		{
			return m_specialAttacks;
		}
	}

	public MonsterAttackParameterDB AttackParameterDB
	{
		get
		{
			return m_attackParameterDB;
		}
	}

	public float RagePoint
	{
		get
		{
			return Mathf.Min(m_ragePoint, 1f);
		}
		set
		{
			m_ragePoint = Mathf.Clamp(value, 0f, 1f);
		}
	}

	public float SpeedModifier
	{
		get
		{
			return m_speedModifier;
		}
		set
		{
			m_speedModifier = value;
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

	public float AttackSpeed
	{
		get
		{
			return m_attackSpeed * m_speedModifier;
		}
	}

	public float AnticipationSpeed
	{
		get
		{
			return m_anticipationTime;
		}
	}

	public MonsterAttack CurrentAttack
	{
		get
		{
			return m_currentAttack;
		}
	}

	public ModelFader[] Faders
	{
		get
		{
			return m_faders;
		}
	}

	public Collider bodyCollider
	{
		get
		{
			if (null != m_bodyCollider)
			{
				return m_bodyCollider.GetComponent<Collider>();
			}
			return base.GetComponent<Collider>();
		}
	}

	public Monster()
	{
		List<Enum>[] array = new List<Enum>[12];
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
		m_state = (m_prevState = State.None);
	}

	public override bool ChangeState(Enum p_next)
	{
		if (m_data == null)
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
		State state = (State)(object)p_current;
		State state2 = ((p_previous == null) ? State.None : ((State)(object)p_previous));
		switch (state)
		{
		case State.Hit:
			m_rotor.StopRotate();
			break;
		case State.Cutscene:
			EnableFootCollision(true);
			break;
		case State.Death:
		{
			m_fading = false;
			m_ragePoint = 0f;
			if (base.GetComponent<Collider>() != null)
			{
				base.GetComponent<Collider>().enabled = false;
			}
			float num = ((!(m_fadeSpeed > 0f)) ? 0f : (1f / m_fadeSpeed));
			float p_limit = m_anim.RemainingPlayTime + num + 1f;
			Dbg.LogEx("Wait Time: {0}", Dbg.NBLogType.COMMON_LOG, p_limit.ToString());
			m_timer.CountDown(p_limit);
			SendMessage("OnDead", SendMessageOptions.DontRequireReceiver);
			break;
		}
		case State.NoMore:
			SetVisible(false);
			break;
		case State.Intro:
			EnableFootCollision(true);
			break;
		}
	}

	protected override void OnExit(Enum p_current, Enum p_next)
	{
		switch ((State)(object)p_current)
		{
		case State.Init:
			if (base.GetComponent<Collider>() != null)
			{
				base.GetComponent<Collider>().enabled = true;
			}
			break;
		case State.Attack:
			m_currentAttack.type = MonsterAttack.Type.None;
			m_currentAttack.phase = MonsterAttack.Phase.None;
			m_currentAttack.comboIndex = 0;
			m_currentAttack.comboLength = 0;
			DisableAllAttackPoints();
			SendMessage("OnExitAttack", SendMessageOptions.DontRequireReceiver);
			Messenger.Invoke("monster_end_atk");
			break;
		case State.Wait:
			SendMessage("OnExitWait", SendMessageOptions.DontRequireReceiver);
			break;
		}
	}

	protected virtual void Awake()
	{
		base.GetComponent<Animation>().cullingType = AnimationCullingType.AlwaysAnimate;
		m_renderers = MBHier.GetComponentsInChildren<SkinnedMeshRenderer>(base.gameObject);
		SkinnedMeshRenderer[] renderers = m_renderers;
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in renderers)
		{
			skinnedMeshRenderer.updateWhenOffscreen = true;
		}
		m_faders = MBHier.GetComponentsInChildren<ModelFader>(base.gameObject);
		m_anim = base.gameObject.GetComponent<NBAnimation>();
		m_attackAnimations = new List<string>[6];
		for (int j = 0; j < 6; j++)
		{
			m_attackAnimations[j] = new List<string>();
		}
		Collider[] componentsInChildren = MBHier.GetComponentsInChildren<Collider>(base.gameObject, true);
		List<Collider> list = new List<Collider>();
		Collider[] array = componentsInChildren;
		foreach (Collider collider in array)
		{
			if (collider.gameObject.tag == "MonsterFoot")
			{
				collider.enabled = false;
			}
			else
			{
				if (!(collider != base.gameObject.GetComponent<Collider>()))
				{
					continue;
				}
				CollisionData component = collider.gameObject.GetComponent<CollisionData>();
				if (!(component == null))
				{
					component.attacker = base.gameObject;
					list.Add(collider);
					Rigidbody rigidbody = collider.gameObject.GetComponent<Rigidbody>();
					if (rigidbody != null)
					{
						rigidbody.isKinematic = true;
						rigidbody.useGravity = false;
						rigidbody.angularDamping = 0f;
						rigidbody.interpolation = RigidbodyInterpolation.None;
						rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
					}
					collider.gameObject.tag = "Enemy";
				}
			}
		}
		m_attackPoints = list.ToArray();
		DisableAllAttackPoints();
		if (base.gameObject.GetComponent<Collider>() != null)
		{
			base.gameObject.GetComponent<Collider>().enabled = false;
		}
		m_moveScript = base.gameObject.AddComponent<CharacterMovement>();
		m_rotor = base.gameObject.AddComponent<Rotor>();
		m_shadow = MBHier.GetComponentInChildren<ShadowProjector>(base.gameObject);
		if (m_shadow == null)
		{
			Dbg.LogError("This monster doesn't have 'ShadowProjector'.");
			return;
		}
		m_timer = base.gameObject.AddComponent<NBTimer>();
		m_timer.Name = "General";
		Messenger.AddListener("battle_start", OnBattleStart);
		Messenger.AddListener("battle_end", OnBattleEnd);
		Messenger<CutsceneController.CutsceneType>.AddListener(CutsceneMessage.CUTSCENE_START, OnCutsceneStart);
		Messenger<CutsceneController.CutsceneType>.AddListener(CutsceneMessage.CUTSCENE_END, OnCutsceneEnd);
	}

	protected virtual void Start()
	{
	}

	protected virtual void OnDestroy()
	{
		Messenger.Invoke("monster_destroy");
		Messenger.RemoveListener("battle_start", OnBattleStart);
		Messenger.RemoveListener("battle_end", OnBattleEnd);
		Messenger<CutsceneController.CutsceneType>.RemoveListener(CutsceneMessage.CUTSCENE_START, OnCutsceneStart);
		Messenger<CutsceneController.CutsceneType>.RemoveListener(CutsceneMessage.CUTSCENE_END, OnCutsceneEnd);
	}

	protected virtual List<string> RetreiveIdleAnimList()
	{
		List<string> list = new List<string>();
		string[] animationList = m_anim.AnimationList;
		foreach (string text in animationList)
		{
			if (text.StartsWith("Idle"))
			{
				list.Add(text);
			}
		}
		return list;
	}

	protected virtual bool InitIdleAnimationList()
	{
		if (m_anim.AnimationList == null)
		{
			Dbg.LogWarn("Animation list not yet ready");
			return false;
		}
		m_idleAnimations = RetreiveIdleAnimList().ToArray();
		return true;
	}

	public void SetVisible(bool p_visible)
	{
		if (m_renderers != null)
		{
			SkinnedMeshRenderer[] renderers = m_renderers;
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in renderers)
			{
				if (skinnedMeshRenderer != null)
				{
					Dbg.Log(string.Concat(skinnedMeshRenderer.gameObject, " is visible: ", p_visible));
					skinnedMeshRenderer.enabled = p_visible;
				}
			}
		}
		EnableShadow(p_visible);
	}

	public bool IsVisibile()
	{
		bool result = true;
		if (m_renderers != null)
		{
			SkinnedMeshRenderer[] renderers = m_renderers;
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in renderers)
			{
				if (skinnedMeshRenderer != null && !skinnedMeshRenderer.enabled)
				{
					result = false;
					break;
				}
			}
		}
		else
		{
			result = false;
		}
		return result;
	}

	public void EnableShadow(bool p_enable)
	{
		if (m_shadow != null)
		{
			m_shadow.enabled = p_enable;
		}
	}

	public virtual MonsterCollisionData EnableAttackPoint(string p_pointName, bool p_enable)
	{
		Collider[] attackPoints = m_attackPoints;
		foreach (Collider collider in attackPoints)
		{
			if (collider.name == p_pointName)
			{
				collider.enabled = p_enable;
				return collider.gameObject.GetComponent<MonsterCollisionData>();
			}
		}
		return null;
	}

	public void DisableAllAttackPoints()
	{
		Collider[] attackPoints = m_attackPoints;
		foreach (Collider collider in attackPoints)
		{
			collider.enabled = false;
		}
	}

	public void EnableFootCollision(bool p_enable)
	{
		Collider[] componentsInChildren = MBHier.GetComponentsInChildren<Collider>(base.gameObject, true);
		Collider[] array = componentsInChildren;
		foreach (Collider collider in array)
		{
			if (base.gameObject.GetComponent<Collider>() != null && collider != base.gameObject.GetComponent<Collider>() && collider.gameObject.tag == "MonsterFoot")
			{
				collider.enabled = p_enable;
			}
		}
	}

	public virtual bool IsBlocking()
	{
		return m_currentState == State.Block;
	}

	public virtual void Block()
	{
		if (ChangeState(State.Block))
		{
			TryPlayAnim("Block");
			SendMessage("OnBlock", SendMessageOptions.DontRequireReceiver);
		}
	}

	public virtual bool CanBecomeIdle()
	{
		return IsReachableFromCurrent(State.Idle) && ((m_anim.CurrentAnim != null && m_anim.CurrentAnim.wrapMode == WrapMode.Loop) || !m_anim.IsPlaying);
	}

	public virtual bool CanPlayCutscene()
	{
		return IsReachableFromCurrent(State.Cutscene) && base.GetComponent<Animation>().enabled;
	}

	public virtual bool CanStun()
	{
		return CurrentState != State.Cutscene && m_data.Hp > 0f;
	}

	public virtual void Idle()
	{
		if (ChangeState(State.Idle))
		{
			Dbg.LogWarn("Idle");
			if (m_anim.CurrentAnimName == "Stumble_Idle")
			{
				m_anim.Play("Recovery");
			}
			else
			{
				RandomIdleAnim(0.4f, false);
			}
			SendMessage("OnIdle", SendMessageOptions.DontRequireReceiver);
		}
	}

	public void Init()
	{
		ChangeState(State.Init);
	}

	public virtual void Fall(string p_fallAnim = "")
	{
		if (ChangeState(State.Fall))
		{
			if (p_fallAnim != string.Empty)
			{
				m_anim.Play(p_fallAnim);
			}
			else
			{
				TryPlayAnim("Fall");
			}
			SendMessage("OnFall", SendMessageOptions.DontRequireReceiver);
		}
	}

	public bool IsDead()
	{
		return CurrentState == State.Death || CurrentState == State.NoMore;
	}

	public void Stuck()
	{
		if (ChangeState(State.Stuck) && !TryPlayAnim("SAtkFail"))
		{
			TryPlayAnim("SAtkStun");
		}
	}

	public virtual void TakeDamage(float p_damage)
	{
		m_data.Hp -= p_damage;
	}

	public bool Taunt()
	{
		if (m_anim["Taunt"] != null && IsReachableFromCurrent(State.Idle) && m_anim.CurrentAnimName != "Taunt")
		{
			Dbg.Log("Taunt");
			m_anim.Play("Taunt", 0.1f);
			SendMessage("OnTaunt", SendMessageOptions.DontRequireReceiver);
			return true;
		}
		return false;
	}

	public virtual bool PlayIntro()
	{
		if (ChangeState(State.Intro))
		{
			if (m_anim["Intro"] != null)
			{
				m_anim.Play("Intro", 0f, true);
				m_anim.GetComponent<Animation>().Sample();
				return true;
			}
			return false;
		}
		return false;
	}

	public virtual void RandomIdleAnim(float p_fadeLength, bool p_loop)
	{
		string text = m_idleAnimations[UnityEngine.Random.Range(0, m_idleAnimations.Length)];
		if (p_loop)
		{
			int num = 0;
			for (num = 0; num < 10; num++)
			{
				if (m_anim[text].wrapMode == WrapMode.Loop)
				{
					break;
				}
				text = m_idleAnimations[UnityEngine.Random.Range(0, m_idleAnimations.Length)];
			}
			if (num == 10)
			{
				Dbg.LogWarn("Failed to randomize idle animation.");
			}
		}
		m_anim.Play(text, p_fadeLength);
	}

	public virtual bool IsPlayingIdleAnimation()
	{
		return Array.IndexOf(m_idleAnimations, m_anim.CurrentAnimName) >= 0;
	}

	public virtual bool PlayHitAnim(PlayerAttack.Direction p_dir)
	{
		string[] p_animList = new string[8] { "Hit_Up", "Hit_Down", "Hit_Right", "Hit_Left", "Hit_Right", "Hit_Left", "Hit_Right", "Hit_Left" };
		string text = PickHitAnimation(p_dir, p_animList);
		if (text == string.Empty)
		{
			return false;
		}
		StartCoroutine(m_anim.PlaySequence(text, 0.06f));
		return true;
	}

	public virtual bool PlayHitHeavyAnim(PlayerAttack.Direction p_dir)
	{
		string[] p_animList = new string[8] { "Hit_Up_Heavy", "Hit_Down_Heavy", "Hit_Right_Heavy", "Hit_Left_Heavy", "Hit_Right_Heavy", "Hit_Left_Heavy", "Hit_Right_Heavy", "Hit_Left_Heavy" };
		string text = PickHitAnimation(p_dir, p_animList);
		if (text == string.Empty)
		{
			return false;
		}
		StartCoroutine(m_anim.PlaySequence(text, 0.06f));
		return true;
	}

	protected string PickHitAnimation(PlayerAttack.Direction p_dir, string[] p_animList)
	{
		string text = string.Empty;
		if (p_dir == PlayerAttack.Direction.None)
		{
			Dbg.LogWarn("Invalid attack direction.");
		}
		else
		{
			text = p_animList[(int)p_dir];
			if (m_anim[text] == null)
			{
				text = string.Empty;
				foreach (string text2 in p_animList)
				{
					if (m_anim[text2] != null)
					{
						text = text2;
						break;
					}
				}
			}
		}
		return text;
	}

	public virtual void PlayStumbleAnim()
	{
		if (!TryPlayAnim("Stumble"))
		{
			PlayHitAnim(PlayerAttack.Direction.TopToBottom);
		}
	}

	public void PlayStuckAnim()
	{
		TryPlayAnim("SAtkStun");
		SendMessage("OnStuck", SendMessageOptions.DontRequireReceiver);
	}

	public bool TryPlayAnim(string p_animName)
	{
		return TryPlayAnim(p_animName, 0.3f);
	}

	public bool TryPlayAnim(string p_animName, float p_fadeLength)
	{
		if ((bool)m_anim[p_animName])
		{
			m_anim.Play(p_animName, p_fadeLength);
			return true;
		}
		RandomIdleAnim(0.3f, false);
		return false;
	}

	public void Fade(float p_from, float p_to)
	{
		if (m_faders != null && m_faders.Length != 0)
		{
			ModelFader[] faders = m_faders;
			foreach (ModelFader modelFader in faders)
			{
				modelFader.Fade(p_from, p_to, m_fadeSpeed, null);
			}
		}
		m_fading = true;
	}

	public void ChangeTexture(string p_path)
	{
		Texture texture = Resources.Load(p_path) as Texture;
		if (texture == null)
		{
			Dbg.LogError("Invalid texture path: " + p_path);
			return;
		}
		SkinnedMeshRenderer[] renderers = m_renderers;
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in renderers)
		{
			skinnedMeshRenderer.material.SetTexture("_MainTex", texture);
		}
	}

	protected virtual void FixedUpdate()
	{
		m_currentState = CurrentState;
	}

	protected override void Update()
	{
		UpdateGlobal();
		switch (CurrentState)
		{
		case State.Idle:
			UpdateIdle();
			break;
		case State.Intro:
			UpdateIntro();
			break;
		case State.Hit:
			UpdateHit();
			break;
		case State.Death:
			UpdateDeath();
			break;
		case State.Init:
			UpdateInit();
			break;
		case State.Block:
		case State.Attack:
		case State.Fall:
		case State.Stuck:
		case State.Wait:
			break;
		}
	}

	protected virtual void UpdateGlobal()
	{
	}

	protected virtual void UpdateInit()
	{
		if (m_ready)
		{
			return;
		}
		m_battleManager = SingletonComponent<BattleManager>.Instance;
		if (InitIdleAnimationList())
		{
			InitAttackAnimations();
			m_ready = true;
			if (!hasIntroCutscene)
			{
				RandomIdleAnim(0f, true);
			}
			else
			{
				m_anim.Play("Idle_01", 0f);
			}
			m_anim.GetComponent<Animation>().Sample();
		}
	}

	protected virtual void UpdateIntro()
	{
		Quaternion rotation = Quaternion.Euler(new Vector3(0f, 180f, 0f));
		base.transform.rotation = rotation;
		if (!base.GetComponent<Animation>().isPlaying)
		{
			base.GetComponent<Animation>().Play();
		}
	}

	protected virtual void UpdateIdle()
	{
	}

	protected virtual void UpdateHit()
	{
	}

	protected virtual void UpdateDeath()
	{
		bool flag = false;
		bool flag2 = true;
		if (m_faders != null && m_faders.Length > 0)
		{
			if (m_fading)
			{
				for (int i = 0; i < m_faders.Length; i++)
				{
					if (null != m_faders[i])
					{
						flag2 &= m_faders[i].currentAlpha == 0f;
					}
				}
				flag = flag2;
			}
			else if (!m_anim.IsPlaying)
			{
				for (int j = 0; j < m_faders.Length; j++)
				{
					if (null != m_faders[j])
					{
						m_faders[j].FadeFromCurrentAlpha(0f, m_fadeSpeed, null);
					}
				}
				m_fading = true;
			}
		}
		else
		{
			flag &= m_anim.IsPlaying;
		}
		if (m_timer.Counter == 0f)
		{
			Dbg.LogWarn("Monster death sequences did not finish playing properly.");
			flag = true;
		}
		if (flag)
		{
			ChangeState(State.NoMore);
		}
	}

	public void Wait(float p_fadeTime = 0.3f)
	{
		if (ChangeState(State.Wait))
		{
			RandomIdleAnim(p_fadeTime, true);
			SendMessage("OnWait", SendMessageOptions.DontRequireReceiver);
		}
	}

	public IEnumerator WaitTilIdle()
	{
		while (!m_anim.CurrentAnimName.Contains("Idle"))
		{
			yield return null;
		}
	}

	public MonsterAttack.Zone GetAttackDirection(string p_atkAnim, int p_comboIndex)
	{
		MonsterAttack.Zone result = MonsterAttack.Zone.None;
		MonsterAttackParameter monsterAttackParameter = m_attackParameterDB[p_atkAnim];
		if (monsterAttackParameter != null && p_comboIndex < monsterAttackParameter.comboCount && p_comboIndex >= 0)
		{
			result = monsterAttackParameter.zones[p_comboIndex];
		}
		return result;
	}

	public string RandomAttackAnimation(MonsterAttack.Type p_type, MonsterAttack.Zone p_direction)
	{
		if (p_type == MonsterAttack.Type.None || p_type == MonsterAttack.Type.Max)
		{
			return string.Empty;
		}
		List<string> list = m_attackAnimations[(int)p_type];
		List<string> list2 = new List<string>();
		if ((p_type == MonsterAttack.Type.Weak || p_type == MonsterAttack.Type.Heavy) && p_direction != MonsterAttack.Zone.None && p_direction != MonsterAttack.Zone.Max)
		{
			foreach (string item in list)
			{
				MonsterAttackParameter monsterAttackParameter = m_attackParameterDB[item];
				if (monsterAttackParameter != null && monsterAttackParameter.zones[0] == p_direction)
				{
					list2.Add(item);
				}
			}
		}
		else
		{
			list2 = list;
		}
		if (list2.Count == 0)
		{
			list2 = m_attackAnimations[0];
		}
		return NBContainerUtil.RandomElement(list2);
	}

	public float GetAttackSpeed(string p_anim)
	{
		return base.GetComponent<Animation>()[p_anim].speed * m_attackSpeed * m_speedModifier;
	}

	public bool IsAttacking()
	{
		return m_currentState == State.Attack;
	}

	public virtual MonsterAttack Attack(MonsterAttack p_attack, float p_fadeTime = 0.3f)
	{
		if (!ChangeState(State.Attack))
		{
			return null;
		}
		m_anim.PlayFixedFrameRate(p_attack.animationName, p_fadeTime, 30f);
		m_pendingAttack.knockSpeed = p_attack.knockSpeed;
		m_pendingAttack.animationName = p_attack.animationName;
		return m_pendingAttack;
	}

	protected virtual void InitAttackAnimations()
	{
		if (m_attackParametersCSV == string.Empty)
		{
			Dbg.LogError("Attack parameter database is not provided.");
			return;
		}
		ParameterManager instance = SingletonComponent<ParameterManager>.Instance;
		if (!instance.IsDataInitialized)
		{
			instance.InitManager();
		}
		NBBinaryFormat parametersFor = instance.GetParametersFor(m_attackParametersCSV);
		m_attackParameterDB = new MonsterAttackParameterDB(m_attackParametersCSV);
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		List<string> list3 = new List<string>();
		List<string> list4 = new List<string>();
		foreach (string rowsName in parametersFor.RowsNames)
		{
			int numberItemInCell = instance.GetNumberItemInCell(m_attackParametersCSV, rowsName, "ATTACK_TYPE");
			MonsterAttackParameter monsterAttackParameter = m_attackParameterDB[rowsName];
			string parameter = instance.GetParameter(m_attackParametersCSV, rowsName, "ATTACK_TYPE");
			string parameter2 = instance.GetParameter(m_attackParametersCSV, rowsName, "TYPE");
			string empty = string.Empty;
			int num = -1;
			if (parameter2.Contains("A_Attack"))
			{
				empty = parameter2.Replace("A_Attack_", string.Empty);
				num = -1;
				int.TryParse(empty, out num);
				num--;
				if (num < 0 || m_data == null || !m_data.m_monsterData.advanceAtkAbility[num])
				{
					continue;
				}
				list2.Add(rowsName);
			}
			else if (parameter2.Contains("SP_Attack") || parameter.Contains("Signature"))
			{
				empty = parameter2.Replace("SP_Attack_", string.Empty);
				num = -1;
				if (parameter.Contains("Signature"))
				{
					num = 0;
				}
				else
				{
					int.TryParse(empty, out num);
					num--;
				}
				if (num < 0 || m_data == null || !m_data.m_monsterData.specialAtkAbility[num])
				{
					continue;
				}
				list3.Add(rowsName);
			}
			else
			{
				list.Add(rowsName);
			}
			list4.Add(rowsName);
			for (int i = 0; i < numberItemInCell; i++)
			{
				string parameter3 = instance.GetParameter(m_attackParametersCSV, rowsName, "ATTACK_TYPE", i);
				string parameter4 = instance.GetParameter(m_attackParametersCSV, rowsName, "DIRECTION", i);
				MonsterAttack.Type typeFromString = MonsterAttack.GetTypeFromString(parameter3);
				MonsterAttack.Zone zoneFromString = MonsterAttack.GetZoneFromString(parameter4);
				if (m_anim[rowsName] == null)
				{
					continue;
				}
				if (i == 0)
				{
					if (numberItemInCell == 1 || typeFromString == MonsterAttack.Type.Signature)
					{
						m_attackAnimations[(int)typeFromString].Add(rowsName);
					}
					else
					{
						m_attackAnimations[5].Add(rowsName);
					}
					m_anim[rowsName].wrapMode = WrapMode.ClampForever;
				}
				string[] array = new string[3] { "OnWindUp,string,", "OnDamageWindow,void,,", "OnWindDown,void,," };
				string[] array2 = new string[3] { "WIND_UP", "DAMAGE_WINDOW", "WIND_DOWN" };
				for (int j = 0; j < array.Length; j++)
				{
					string text = instance.GetParameter(m_attackParametersCSV, rowsName, array2[j], i);
					if (j == 0)
					{
						text = parameter3 + '_' + parameter4 + '_' + (i + 1) + '_' + numberItemInCell + "," + text;
					}
					string p_eventString = "[" + array[j] + text + "]";
					m_anim.AddAnimEvent(rowsName, p_eventString);
				}
				if (monsterAttackParameter != null)
				{
					m_anim.AddAnimEvent(rowsName, "[OnPerfectDodgeWindowStart,void,," + monsterAttackParameter.perfectDodgeWindowStart[i] + "]");
					m_anim.AddAnimEvent(rowsName, "[OnDodgeWindowStart,void,," + monsterAttackParameter.dodgeWindowStart[i] + "]");
					m_anim.AddAnimEvent(rowsName, "[OnDodgeWindowEnd,void,," + monsterAttackParameter.dodgeWindowEnd[i] + "]");
				}
			}
		}
		m_normalAttacks = list.ToArray();
		m_advanceAttacks = list2.ToArray();
		m_specialAttacks = list3.ToArray();
		m_allAttacks = list4.ToArray();
	}

	protected void RemoveAttackAnimations(string[] p_attacks)
	{
		List<string> list = new List<string>(m_normalAttacks);
		List<string> list2 = new List<string>(m_advanceAttacks);
		List<string> list3 = new List<string>(m_specialAttacks);
		List<string> list4 = new List<string>(m_allAttacks);
		for (int i = 0; i < p_attacks.Length; i++)
		{
			list.Remove(p_attacks[i]);
			list2.Remove(p_attacks[i]);
			list3.Remove(p_attacks[i]);
			list4.Remove(p_attacks[i]);
		}
		for (int j = 0; j < m_attackAnimations.Length; j++)
		{
			List<string> list5 = m_attackAnimations[j];
			for (int k = 0; k < p_attacks.Length; k++)
			{
				list5.Remove(p_attacks[k]);
			}
		}
		m_normalAttacks = list.ToArray();
		m_advanceAttacks = list2.ToArray();
		m_specialAttacks = list3.ToArray();
		m_allAttacks = list4.ToArray();
	}

	protected virtual void OnWindUp(string p_attack)
	{
		if (CurrentState == State.Attack)
		{
			string[] array = p_attack.Split('_');
			if (array.Length != 4)
			{
				Dbg.LogError("Invalid attack information");
			}
			m_currentAttack.type = MonsterAttack.GetTypeFromString(array[0]);
			m_currentAttack.zone = MonsterAttack.GetZoneFromString(array[1]);
			m_currentAttack.phase = MonsterAttack.Phase.WindUp;
			m_currentAttack.comboIndex = int.Parse(array[2]);
			m_currentAttack.comboLength = int.Parse(array[3]);
			m_currentAttack.knockSpeed = m_pendingAttack.knockSpeed;
			m_currentAttack.animationName = m_pendingAttack.animationName;
			MonsterAttackParameter monsterAttackParameter = m_attackParameterDB[m_currentAttack.animationName];
			if (monsterAttackParameter != null && monsterAttackParameter.attackPoints.Length >= m_currentAttack.comboIndex)
			{
				m_currentAttack.attackPoint = monsterAttackParameter.attackPoints[m_currentAttack.comboIndex - 1];
			}
			switch (m_currentAttack.type)
			{
			case MonsterAttack.Type.Heavy:
				m_currentAttack.multiplier = 1.5f;
				break;
			case MonsterAttack.Type.Signature:
				m_currentAttack.multiplier = 2.5f;
				break;
			default:
				m_currentAttack.multiplier = 1f;
				break;
			}
			DisableAllAttackPoints();
			SendMessage("OnBeginAttack", m_currentAttack, SendMessageOptions.DontRequireReceiver);
			Messenger<MonsterAttack>.Invoke("monster_begin_atk", m_currentAttack);
		}
	}

	protected virtual void OnFinishAttack()
	{
		m_currentAttack.type = MonsterAttack.Type.None;
		m_currentAttack.phase = MonsterAttack.Phase.None;
	}

	protected virtual void OnDamageWindow()
	{
		if (CurrentState != State.Attack)
		{
			return;
		}
		if (m_currentAttack != null)
		{
			m_currentAttack.phase = MonsterAttack.Phase.DamageWindow;
			MonsterCollisionData monsterCollisionData = EnableAttackPoint(m_currentAttack.attackPoint, true);
			if (monsterCollisionData != null)
			{
				monsterCollisionData.attack = m_currentAttack;
			}
		}
		Messenger.Invoke("monster_damage_window");
	}

	protected virtual void OnWindDown()
	{
		if (CurrentState == State.Attack)
		{
			if (m_currentAttack != null)
			{
				m_currentAttack.type = MonsterAttack.Type.None;
				m_currentAttack.phase = MonsterAttack.Phase.WindDown;
				m_pendingAttack.type = MonsterAttack.Type.None;
				m_pendingAttack.phase = MonsterAttack.Phase.WindDown;
				EnableAttackPoint(m_currentAttack.attackPoint, false);
			}
			Messenger.Invoke("monster_attack_wind_down");
			SendMessage("OnAttackWindDown", m_currentAttack, SendMessageOptions.DontRequireReceiver);
		}
	}

	protected virtual void OnPerfectDodgeWindowStart()
	{
		Messenger.Invoke("perfect_dodge_window_start");
	}

	protected virtual void OnDodgeWindowStart()
	{
		Messenger.Invoke("dodge_window_start");
	}

	protected virtual void OnDodgeWindowEnd()
	{
		Messenger.Invoke("dodge_window_end");
	}

	protected void OnStunEnd()
	{
		Idle();
	}

	public MonsterAttackParameter GetAttackParameter(string p_attackAnim)
	{
		return m_attackParameterDB[p_attackAnim];
	}

	public bool IsTurning()
	{
		return m_rotor.IsRotating();
	}

	public virtual void Turn(float p_angle)
	{
		string p_anim = GetOrientAnimation(p_angle);
		float p_period = 1f;
		if (base.GetComponent<Animation>()[p_anim] == null)
		{
			p_anim = "Idle_01";
		}
		else
		{
			p_period = base.GetComponent<Animation>()[p_anim].length;
		}
		m_anim.Play(p_anim);
		m_rotor.BeginRotate(p_angle, p_period, OnFinishOrient);
	}

	public void Turn(Vector3 p_lookTarget)
	{
		float lookAngle = base.transform.GetLookAngle(p_lookTarget);
		Turn(lookAngle);
	}

	public virtual string GetOrientAnimation(float p_angle)
	{
		string empty = string.Empty;
		if (p_angle < 0f)
		{
			return "OrientLeft";
		}
		return "OrientRight";
	}

	public string GetOrientAnimation(Vector3 p_lookTarget)
	{
		return GetOrientAnimation(base.transform.GetLookAngle(p_lookTarget));
	}

	public void Recover(Vector3 p_lookTarget)
	{
		Dbg.Log("Recover");
		string p_anim = GetRecoverAnimation(p_lookTarget);
		float p_period = 1f;
		if (base.GetComponent<Animation>()[p_anim] == null)
		{
			p_anim = "Idle_01";
		}
		else
		{
			p_period = base.GetComponent<Animation>()[p_anim].length;
		}
		m_anim.Play(p_anim);
		m_rotor.BeginRotate(p_lookTarget, p_period, OnFinishOrient);
		SendMessage("OnRecover", SendMessageOptions.DontRequireReceiver);
	}

	public string GetRecoverAnimation(Vector3 p_lookTarget)
	{
		float lookAngle = base.transform.GetLookAngle(p_lookTarget);
		string empty = string.Empty;
		if (lookAngle < 0f)
		{
			return "SRecoverL";
		}
		return "SRecoverR";
	}

	public int GetRelativeDirection(Vector3 p_target, float p_forwardAngle)
	{
		float lookAngle = base.transform.GetLookAngle(p_target);
		return GetRelativeDirection(lookAngle, p_forwardAngle);
	}

	public int GetRelativeDirection(float p_lookAngle, float p_forwardAngle)
	{
		int result = 0;
		if (p_lookAngle < 0f - p_forwardAngle)
		{
			result = -1;
		}
		else if (p_lookAngle > p_forwardAngle)
		{
			result = 1;
		}
		return result;
	}

	public void StopTurn()
	{
		Dbg.Log("monster.StopTurn");
		m_rotor.StopRotate();
	}

	protected virtual void OnFinishOrient()
	{
		string currentAnimName = m_anim.CurrentAnimName;
		if (currentAnimName.Contains("Orient") || currentAnimName.Contains("Recover"))
		{
			RandomIdleAnim(0.3f, false);
		}
	}

	public void RunTo(Vector3 p_targetPos, float p_speed, VoidCallback p_endCallback = null)
	{
		TryPlayAnim("Run");
		m_moveScript.MoveTo(p_targetPos, 0f, p_speed, 0.65f, p_endCallback);
	}

	protected virtual void OnEnable()
	{
	}

	protected virtual void OnDisable()
	{
	}

	protected virtual void OnBattleStart()
	{
		base.enabled = true;
		Idle();
	}

	protected virtual void OnBattleEnd()
	{
	}

	protected virtual void OnCutsceneStart(CutsceneController.CutsceneType p_type)
	{
		Dbg.Log("monster.OnCutsceneStart");
		base.enabled = false;
		StopTurn();
	}

	protected virtual void OnCutsceneEnd(CutsceneController.CutsceneType p_type)
	{
		base.enabled = true;
	}

	protected virtual void OnTriggerEnter(Collider p_other)
	{
		if (p_other.enabled && !(p_other.gameObject.tag != "Player"))
		{
			PlayerCollisionData component = p_other.GetComponent<PlayerCollisionData>();
			Messenger<PlayerCollisionData>.Invoke("monster_get_hit", component);
			p_other.enabled = false;
		}
	}

	protected virtual void OnAnimBegin(string p_anim)
	{
	}

	protected virtual void OnAnimEnded(string p_anim)
	{
		State currentState = CurrentState;
		if (p_anim == "Stumble" && null != base.GetComponent<Animation>()["Stumble_Idle"])
		{
			m_anim.Play("Stumble_Idle");
		}
		else
		{
			if (!base.enabled)
			{
				return;
			}
			switch (currentState)
			{
			case State.Attack:
				SendMessage("OnFinishAttack");
				break;
			case State.Idle:
				break;
			case State.Wait:
				RandomIdleAnim(0.3f, true);
				break;
			case State.Stuck:
				if (p_anim == "SAtkFail")
				{
					TryPlayAnim("SAtkStun");
				}
				break;
			case State.Intro:
				Wait();
				break;
			case State.Block:
			case State.Hit:
			case State.Fall:
				break;
			}
		}
	}
}
