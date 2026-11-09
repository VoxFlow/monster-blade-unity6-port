using System.Collections;
using UnityEngine;

public class EpicDragonAI : AIController
{
	public enum Phase
	{
		NONE = -1,
		FLYING = 0,
		GROUND = 1,
		FINAL = 2
	}

	private const string PARAMETER_FILE = "Mob07Parameters";

	private const string LEVEL = "LEVEL";

	private const string RAPID_FIRE_CHANCE = "RAPID_FIRE_CHANCE";

	private const string FLYING_FLAME_THROWER_CHANCE = "FLYING_FLAME_THROWER_CHANCE";

	private const string GROUND_FLAME_THROWER_COMBO_TRIGGER = "GROUND_FLAME_THROWER_COMBO_TRIGGER";

	private const string FLYING_PHASE_HP = "FLYING_PHASE_HP";

	private const string FLYING_REST_TIME = "FLYING_REST_TIME";

	private const string FINAL_PHASE_HP = "FINAL_PHASE_HP";

	[SerializeField]
	private EpicDragonFireBall m_fireBallPrefab;

	[SerializeField]
	private float m_flyingIdleMin = 0.5f;

	[SerializeField]
	private float m_flyingIdleMax = 2f;

	[SerializeField]
	private float m_rapidFireChance = 0.65f;

	[SerializeField]
	private float m_flameThrowerChance = 0.15f;

	[SerializeField]
	private float m_flyingFireballRotatingSpeed = 10f;

	[SerializeField]
	private float m_flyingFireBreathRotatingSpeed = 3f;

	[SerializeField]
	private float m_groundFireBreathRotatingSpeed = 3f;

	[SerializeField]
	private float m_fallingCollisionRadius = 9.5f;

	[SerializeField]
	private DragonFireBreath m_flyingFireBreath;

	[SerializeField]
	private DragonFireBreath m_groundFireBreath;

	[SerializeField]
	private float m_groundFireBreathComboHpTrigger = 0.3f;

	[SerializeField]
	private float m_flyingHpPercent = 0.05f;

	[SerializeField]
	private float m_flyingRestTime = 45f;

	[SerializeField]
	private float m_finalPhaseHp = 0.25f;

	private static readonly float[] ms_phaseTriggers = new float[3] { 1f, 0.92f, 0.15f };

	private NBTimer m_flyingRestTimer;

	private float m_damageReceived;

	private bool m_interuptable = true;

	private float m_maxHp;

	private Phase m_phase = Phase.NONE;

	private EpicDragon m_dragon;

	public static float[] PHASE_HP_TRIGGERS
	{
		get
		{
			return ms_phaseTriggers;
		}
	}

	public Phase phase
	{
		get
		{
			return m_phase;
		}
		set
		{
			switch (value)
			{
			case Phase.FLYING:
				StartCoroutine(m_dragon.TakeOff());
				m_damageReceived = 0f;
				break;
			case Phase.GROUND:
				m_flyingRestTimer.CountDown(m_flyingRestTime);
				break;
			case Phase.FINAL:
				((SphereCollider)m_monster.bodyCollider).radius = m_fallingCollisionRadius;
				m_monster.Data.MaxHp = m_finalPhaseHp;
				m_monster.Data.Hp = m_finalPhaseHp;
				break;
			}
			m_phase = value;
			Messenger<Phase>.Invoke("epicdragon_phase_change", value);
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_dragon = (EpicDragon)m_monster;
		m_flyingRestTimer = base.gameObject.AddComponent<NBTimer>();
		m_flyingRestTimer.Name = "Flying Rest Timer";
		Messenger<DamageData>.AddListener("monster_receive_dmg", OnReceiveDamage);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Messenger<DamageData>.RemoveListener("monster_receive_dmg", OnReceiveDamage);
	}

	protected override void Start()
	{
		base.Start();
		phase = Phase.FLYING;
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (phase == Phase.GROUND)
		{
			m_flyingRestTimer.Pause(false);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		StopFireBreath();
		if (!m_interuptable)
		{
			m_interuptable = true;
			if (null != SingletonComponent<CutsceneController>.Instance)
			{
				SingletonComponent<CutsceneController>.Instance.enabled = true;
			}
		}
		if (phase == Phase.GROUND)
		{
			m_flyingRestTimer.Pause(true);
		}
	}

	protected override void OnBattleInit()
	{
		base.OnBattleInit();
		DragonFireBreath[] componentsInChildren = MBHier.GetComponentsInChildren<DragonFireBreath>(base.gameObject);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (null != componentsInChildren[i])
			{
				componentsInChildren[i].player = m_player;
				componentsInChildren[i].monster = m_monster;
			}
		}
		LoadMonsterParameters();
		m_maxHp = m_monster.Data.MaxHp;
		m_monster.Data.MaxHp -= m_finalPhaseHp;
		m_monster.Data.Hp = m_monster.Data.MaxHp;
	}

	protected override void OnCutsceneStart(CutsceneController.CutsceneType p_type)
	{
		base.OnCutsceneStart(p_type);
		StopFireBreath();
	}

	protected override void OnCutsceneEnd(CutsceneController.CutsceneType p_type)
	{
		base.OnCutsceneEnd(p_type);
		if (!(null == m_monster) && p_type == CutsceneController.CutsceneType.EPICDRAGON_FINAL_PHASE)
		{
			m_monster.AnimPlayer.Play("Glide_Idle");
		}
	}

	protected override void OnDead()
	{
		base.OnDead();
		m_monster.Data.MaxHp = m_maxHp;
		m_monster.Data.Hp = 0f;
	}

	protected override string DetermineAttackAnim(MonsterAttack.Type p_attackType)
	{
		string empty = string.Empty;
		if (phase == Phase.FLYING)
		{
			float num = Random.Range(0f, 1f);
			if (num < m_rapidFireChance)
			{
				return "Flight_Att_Fireball_Rapid";
			}
			num -= m_rapidFireChance;
			if (num < m_flameThrowerChance)
			{
				return "Flight_Att_FlameThrower";
			}
			return "Flight_Att_Fireball";
		}
		if (phase == Phase.FINAL)
		{
			return NBContainerUtil.RandomElement(EpicDragon.GLIDE_ATTACK_ANIMS);
		}
		return base.DetermineAttackAnim(p_attackType);
	}

	protected override void OnDamageWindow()
	{
		base.OnDamageWindow();
		if (m_monster.CurrentAttack != null)
		{
			MonsterAttack currentAttack = m_monster.CurrentAttack;
			if (EpicDragon.IsFireBreathAttack(currentAttack))
			{
				Transform p_root = base.transform.FindInHierarchy(currentAttack.attackPoint);
				BreathFire(p_root, m_player.BodyCollider.transform);
			}
			else if (EpicDragon.IsFireBallAttack(currentAttack))
			{
				SpitFireBall(m_monster.CurrentAttack);
			}
		}
	}

	protected override void OnWindDown()
	{
		base.OnWindDown();
		if (m_monster.CurrentAttack != null && EpicDragon.IsFireBreathAttack(m_monster.CurrentAttack))
		{
			StopFireBreath();
		}
	}

	private void OnReceiveDamage(DamageData p_damage)
	{
		if (phase == Phase.FLYING)
		{
			m_damageReceived += p_damage.finalDamage / m_monster.Data.MaxHp;
			if (m_damageReceived >= m_flyingHpPercent && m_monster.Data.Hp > p_damage.finalDamage)
			{
				phase = Phase.GROUND;
			}
		}
	}

	protected override void OnFinishAttack()
	{
		base.OnFinishAttack();
		StopFireBreath();
	}

	protected override void RandomIdleTime(string p_callback)
	{
		if (phase == Phase.FLYING)
		{
			float p_limit = Random.Range(m_flyingIdleMin, m_flyingIdleMax);
			m_idleTimer.CountDown(p_limit, p_callback);
		}
		else
		{
			base.RandomIdleTime(p_callback);
		}
	}

	protected override void OnAnimEnded(string p_anim)
	{
		if (m_monster == null || m_monster.CurrentState == Monster.State.Cutscene || m_monster.Data == null || m_monster.Data.Hp == 0f)
		{
			return;
		}
		if (!base.enabled)
		{
			if (m_player.Data.Hp == 0f || m_player.IsMoving())
			{
				RandomIdleAnim(0.3f);
			}
			return;
		}
		Monster.State currentState = m_monster.CurrentState;
		if (currentState == Monster.State.Idle && NeedToPlayIdleAnim() && !m_dragon.IsPlayingIdleAnimation())
		{
			RandomIdleAnim(0.3f);
		}
	}

	private IEnumerator Fly()
	{
		CutsceneController cutsceneController = SingletonComponent<CutsceneController>.Instance;
		if (null != cutsceneController)
		{
			if (cutsceneController.Playing)
			{
				yield break;
			}
			cutsceneController.enabled = false;
		}
		m_interuptable = false;
		m_monster.Invincible = true;
		BattleUtils.EnableSkills(false, this);
		while (!m_monster.CanBecomeIdle())
		{
			if (m_comboList.Count > 0)
			{
				m_comboList.Clear();
			}
			yield return null;
		}
		if (m_player.Data.Hp > 0f && phase == Phase.GROUND)
		{
			m_monster.Idle();
			m_monster.Invincible = false;
			phase = Phase.FLYING;
		}
		if (null != cutsceneController)
		{
			cutsceneController.enabled = true;
		}
		BattleUtils.EnableSkills(true, this);
		m_interuptable = true;
	}

	private void RandomIdleAnim(float p_fadeLength)
	{
		if (!(null != m_dragon) || !m_dragon.takingOff)
		{
			if (phase == Phase.FLYING)
			{
				m_dragon.AnimPlayer.Play(EpicDragon.RandomFlyingIdleAnim(), p_fadeLength);
			}
			else if (phase == Phase.FINAL)
			{
				m_dragon.AnimPlayer.Play("Glide_Idle", p_fadeLength);
			}
			else
			{
				m_dragon.RandomIdleAnim(p_fadeLength, false);
			}
		}
	}

	private void SpitFireBall(MonsterAttack p_attack)
	{
		if (p_attack != null && null != m_fireBallPrefab)
		{
			EpicDragonFireBall epicDragonFireBall = Object.Instantiate(m_fireBallPrefab) as EpicDragonFireBall;
			Transform transform = base.transform.FindInHierarchy(p_attack.attackPoint);
			if (null != transform)
			{
				epicDragonFireBall.transform.position = transform.position;
			}
			MonsterCollisionData monsterCollisionData = epicDragonFireBall.monsterCollisionData;
			monsterCollisionData.attackerData = m_monster.Data;
			monsterCollisionData.attacker = base.gameObject;
			monsterCollisionData.attack = new MonsterAttack(m_monster.CurrentAttack);
			Vector3 position = base.transform.InverseTransformPoint(m_player.BodyCollider.transform.position);
			position.x = 0f;
			epicDragonFireBall.target = base.transform.TransformPoint(position);
			epicDragonFireBall.gameObject.tag = "Enemy";
		}
	}

	private void BreathFire(Transform p_root, Transform p_target)
	{
		if (phase == Phase.FLYING && null != m_flyingFireBreath)
		{
			m_flyingFireBreath.Emit(p_root, p_target);
		}
		else if (phase == Phase.GROUND && null != m_groundFireBreath)
		{
			m_groundFireBreath.Emit(p_root, p_target);
		}
	}

	private void StopFireBreath()
	{
		if (null != m_flyingFireBreath)
		{
			m_flyingFireBreath.Stop();
		}
		if (null != m_groundFireBreath)
		{
			m_groundFireBreath.Stop();
		}
	}

	protected override void UpdateGlobal()
	{
		if (!m_interuptable)
		{
			return;
		}
		if (phase == Phase.GROUND)
		{
			if (m_flyingRestTimer.Counter == 0f)
			{
				StartCoroutine(Fly());
			}
		}
		else if (phase == Phase.FINAL)
		{
			base.transform.localPosition = Vector3.zero;
			base.transform.localRotation = Quaternion.identity;
		}
		base.UpdateGlobal();
	}

	protected override void TrackPlayer()
	{
		if (phase != Phase.FINAL)
		{
			base.TrackPlayer();
		}
	}

	protected override void _TrackPlayerWhileAttacking(float p_trackablePeriod)
	{
		if (phase == Phase.FINAL)
		{
			return;
		}
		float num = Mathf.Abs(m_playerAngle);
		if ((m_monster.CurrentAttack.animationName == "Flight_Att_FlameThrower" || m_monster.CurrentAttack.animationName == "Att_FlameThrower") && num > 0.001f)
		{
			float num2 = 0f;
			num2 = ((phase != Phase.FLYING) ? ((!EpicDragon.IsFireBallAttack(m_monster.CurrentAttack)) ? m_groundFireBreathRotatingSpeed : m_flyingFireballRotatingSpeed) : ((!EpicDragon.IsFireBallAttack(m_monster.CurrentAttack)) ? m_flyingFireBreathRotatingSpeed : m_flyingFireballRotatingSpeed));
			float num3 = num2 * Time.deltaTime * (m_playerAngle / Mathf.Abs(m_playerAngle));
			Vector3 eulerAngles = base.transform.eulerAngles;
			if (num < num3)
			{
				eulerAngles.y += m_playerAngle;
			}
			else
			{
				eulerAngles.y += num3;
			}
			if (Mathfx.IsValidVector(eulerAngles))
			{
				base.transform.eulerAngles = eulerAngles;
			}
		}
		else
		{
			base._TrackPlayerWhileAttacking(p_trackablePeriod);
		}
	}

	protected override bool ChangeBehavior(string newBehavior)
	{
		if (base.ChangeBehavior(newBehavior))
		{
			if (phase != Phase.FINAL && m_monster.Data.HpPercent <= m_groundFireBreathComboHpTrigger)
			{
				m_behavior.hasSpecialAttack = true;
			}
			else
			{
				m_behavior.hasSpecialAttack = false;
			}
			return true;
		}
		return false;
	}

	protected void LoadMonsterParameters()
	{
		ParameterManager instance = SingletonComponent<ParameterManager>.Instance;
		if (!(instance == null) && !(null == m_monster) && m_monster.Data != null)
		{
			NBBinaryFormat parametersFor = instance.GetParametersFor("Mob07Parameters");
			int level = m_monster.Data.Level;
			m_rapidFireChance = parametersFor.GetDataGeneric<float>(level, "RAPID_FIRE_CHANCE") * 0.01f;
			m_flameThrowerChance = parametersFor.GetDataGeneric<float>(level, "FLYING_FLAME_THROWER_CHANCE") * 0.01f;
			m_groundFireBreathComboHpTrigger = parametersFor.GetDataGeneric<float>(level, "GROUND_FLAME_THROWER_COMBO_TRIGGER") * 0.01f;
			m_flyingHpPercent = parametersFor.GetDataGeneric<float>(level, "FLYING_PHASE_HP") * 0.01f;
			m_flyingRestTime = parametersFor.GetDataGeneric<float>(level, "FLYING_REST_TIME");
			m_finalPhaseHp = parametersFor.GetDataGeneric<float>(level, "FINAL_PHASE_HP");
			m_finalPhaseHp *= m_monster.Data.MaxHp * 0.01f;
		}
	}
}
