using System.Collections;
using UnityEngine;

public class WolfAI : AIController
{
	public enum Phase
	{
		Normal = 0,
		Exhaust = 1,
		Extreme = 2
	}

	private Phase m_phase;

	private float m_defaultMinAtk;

	private float m_defaultMaxAtk;

	private float m_defaultDef;

	private bool m_hitWindowPassed = true;

	private Transform m_headBone;

	private bool m_playerFinishKB;

	private NBTimer m_phaseTimer;

	public static float extremeHpPercent = 0.25f;

	public static float exhaustHpPercent = 0.5f;

	public static float extremeModeTime = 60f;

	public static float exhaustModeTime = 30f;

	public static float knockbackSpeed = 10f;

	public static float criticalAngle = 40f;

	private static readonly float speedModifier = 1.5f;

	private static readonly float maxCoroutineWaitTime = 20f;

	private NBTimer m_coroutineTimer;

	private bool m_interuptable = true;

	private bool m_wasRage;

	private WolfSpecialFx m_fx;

	private bool m_cutscenePlaying;

	private Job m_customSequence;

	public Phase phase
	{
		get
		{
			return m_phase;
		}
	}

	public float phaseTime
	{
		get
		{
			return m_phaseTimer.Counter;
		}
	}

	public bool interuptable
	{
		get
		{
			return m_interuptable;
		}
		private set
		{
			Dbg.Log("WolfAI:interuptable " + value);
			m_interuptable = value;
		}
	}

	public bool wasRage
	{
		get
		{
			return m_wasRage;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_fx = base.gameObject.GetComponent<WolfSpecialFx>();
		m_headBone = base.transform.FindInHierarchy("Mob03 Head");
		m_phaseTimer = base.gameObject.AddComponent<NBTimer>();
		m_phaseTimer.IsRealtime = false;
		m_coroutineTimer = base.gameObject.AddComponent<NBTimer>();
		m_coroutineTimer.IsRealtime = false;
	}

	protected override void Start()
	{
		base.Start();
		m_defaultMinAtk = m_monster.Data.MinAttack;
		m_defaultMaxAtk = m_monster.Data.MaxAttack;
		m_defaultDef = m_monster.Data.Def;
		ChangePhase(Phase.Normal);
	}

	public override void Attack()
	{
		if (interuptable)
		{
			base.Attack();
		}
	}

	protected override void PrepareComboString(int p_comboCount)
	{
		m_behavior.hasSpecialAttack = m_phase == Phase.Extreme;
		base.PrepareComboString(p_comboCount);
	}

	protected override string DetermineAttackAnim(MonsterAttack.Type p_attackType)
	{
		string result = string.Empty;
		float num = Random.Range(0f, 1f);
		if (Mathf.Abs(m_playerAngle) >= criticalAngle && num < 0.75f)
		{
			if (m_playerAngle >= 40f)
			{
				result = "Elusive_01";
			}
			else if (m_playerAngle <= -40f)
			{
				result = "Elusive_02";
			}
		}
		else
		{
			result = base.DetermineAttackAnim(p_attackType);
		}
		return result;
	}

	protected override MonsterDebugGUI InitDebugGUI()
	{
		return base.gameObject.AddComponent<WolfDebugGUI>();
	}

	private void UpdateCustomBehavior()
	{
		if (m_player.Data.Hp != 0f)
		{
			switch (m_phase)
			{
			case Phase.Normal:
				UpdateNormalPhase();
				break;
			case Phase.Exhaust:
				UpdateExhaustPhase();
				break;
			case Phase.Extreme:
				UpdateExtremePhase();
				break;
			}
		}
	}

	protected override void LateUpdate()
	{
		if (interuptable)
		{
			base.LateUpdate();
			if (m_player != null)
			{
				float lookAngle = base.transform.GetLookAngle(m_player.transform);
				Vector3 localEulerAngles = m_headBone.transform.localEulerAngles;
				localEulerAngles.y = Mathf.Clamp(0f - lookAngle, -25f, 25f);
				m_headBone.transform.localEulerAngles = localEulerAngles;
			}
			UpdateCustomBehavior();
		}
	}

	protected virtual IEnumerator PeriodicUpdate()
	{
		StunLogic stunLogic = SingletonComponent<StunLogic>.Instance;
		while (!base.enabled)
		{
			if (stunLogic != null && !stunLogic.IsMonsterStunning() && !m_monster.AnimPlayer.IsPlaying)
			{
				m_monster.AnimPlayer.Play("Idle_01");
			}
			yield return null;
		}
	}

	private void UpdateNormalPhase()
	{
		float hpPercent = m_monster.Data.HpPercent;
		if (hpPercent <= extremeHpPercent)
		{
			BecomeExtreme();
		}
		else if (hpPercent <= exhaustHpPercent)
		{
			BecomeExhaust();
		}
	}

	private void UpdateExhaustPhase()
	{
		if (!m_wasRage && m_monster.Data.HpPercent <= extremeHpPercent)
		{
			m_phaseTimer.Stop();
		}
		if (m_phaseTimer.Counter == 0f)
		{
			BecomeExtreme();
		}
	}

	private void UpdateExtremePhase()
	{
		if (IsReadyToChangePhase())
		{
			BecomeExhaust();
		}
	}

	protected override void FixedUpdate()
	{
		if (interuptable)
		{
			base.FixedUpdate();
		}
	}

	public override void Pause(bool p_pause)
	{
		base.Pause(p_pause);
		m_coroutineTimer.Pause(p_pause);
		m_phaseTimer.Pause(p_pause);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		StopCoroutine("PeriodicUpdate");
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		StartCoroutine("PeriodicUpdate");
	}

	protected override void OnAnimEnded(string p_anim)
	{
		if (interuptable)
		{
			base.OnAnimEnded(p_anim);
		}
	}

	protected override void OnIdle()
	{
		if (interuptable)
		{
			base.OnIdle();
		}
		m_hitWindowPassed = true;
	}

	protected override void OnBeginAttack(MonsterAttack p_attack)
	{
		base.OnBeginAttack(p_attack);
		m_hitWindowPassed = false;
	}

	protected override void OnWindDown()
	{
		m_hitWindowPassed = true;
	}

	protected override void OnBattleStart()
	{
		base.OnBattleStart();
		m_phaseTimer.Pause(false);
	}

	protected override void OnBattleEnd()
	{
		base.OnBattleEnd();
		m_phaseTimer.Pause(true);
	}

	protected override void OnCutsceneStart(CutsceneController.CutsceneType p_type)
	{
		base.OnCutsceneStart(p_type);
		m_phaseTimer.Pause(true);
		m_cutscenePlaying = true;
	}

	protected override void OnCutsceneEnd(CutsceneController.CutsceneType p_type)
	{
		base.OnCutsceneEnd(p_type);
		m_phaseTimer.Pause(false);
		m_cutscenePlaying = false;
	}

	protected override void OnStun()
	{
		base.OnStun();
		m_phaseTimer.Pause(true);
	}

	protected override void OnStunEnd()
	{
		base.OnStunEnd();
		m_phaseTimer.Pause(false);
	}

	private void OnRage()
	{
		if (!m_cutscenePlaying)
		{
			Dbg.Log("OnRage");
			MonsterAttack monsterAttack = new MonsterAttack();
			monsterAttack.multiplier = 0f;
			m_player.IsReady = true;
			m_player.Idle();
			m_player.KnockBack(knockbackSpeed, OnPlayerFinishKnockBack);
			m_player.gameObject.GetComponent<PlayerController>().enabled = false;
			m_playerFinishKB = false;
			if (m_fx != null)
			{
				m_fx.SwapTexture(1);
				m_fx.EnableParticle("FX_Fire");
			}
			Messenger.Invoke("wolf_rage");
			SingletonComponent<BattleScreenUI>.Instance.ShowFeedbackFx(true, null);
		}
	}

	private void OnPlayerFinishKnockBack()
	{
		m_playerFinishKB = true;
	}

	private void OnPhaseEnd()
	{
		m_comboList.Clear();
	}

	private void ChangePhase(Phase p_next)
	{
		switch (p_next)
		{
		case Phase.Normal:
			m_monster.Data.MinAttack = m_defaultMinAtk;
			m_monster.Data.MaxAttack = m_defaultMaxAtk;
			m_monster.Data.Def = m_defaultDef;
			m_monster.SpeedModifier = speedModifier;
			if (m_fx != null)
			{
				m_fx.SwapTexture(0);
			}
			break;
		case Phase.Exhaust:
			m_monster.Data.MinAttack = m_defaultMinAtk * 0.5f;
			m_monster.Data.MaxAttack = m_defaultMaxAtk * 0.5f;
			m_monster.Data.Def = m_defaultDef;
			m_monster.SpeedModifier = 0.75f * speedModifier;
			if (m_phase == Phase.Extreme)
			{
				m_monster.Data.HpPercent = extremeHpPercent;
			}
			break;
		case Phase.Extreme:
			m_monster.Data.MinAttack = m_defaultMinAtk * 2f;
			m_monster.Data.MaxAttack = m_defaultMaxAtk * 2f;
			m_monster.Data.Def = m_defaultDef * 2f;
			m_monster.SpeedModifier = 1.15f * speedModifier;
			m_monster.Data.HpPercent = 1f;
			break;
		}
		m_phase = p_next;
		m_comboList.Clear();
		m_phaseTimer.Reset();
	}

	private void BecomeExhaust()
	{
		m_customSequence = new Job(Exhaust());
		m_customSequence.OnJobCompleted += OnFinishExhaustSequence;
		m_customSequence.Start();
	}

	private void BecomeExtreme()
	{
		m_customSequence = new Job(TriggerExtremeMode());
		m_customSequence.OnJobCompleted += OnFinishExtremeSequence;
		m_customSequence.Start();
	}

	private IEnumerator Exhaust()
	{
		interuptable = false;
		Dbg.Log("Exhaust");
		m_coroutineTimer.CountDown(maxCoroutineWaitTime);
		while (!IsReadyToChangePhase())
		{
			if (m_coroutineTimer.Counter == 0f)
			{
				m_customSequence.Kill();
			}
			yield return null;
		}
		if (m_player.Data.Hp == 0f)
		{
			m_customSequence.Kill();
			yield return null;
		}
		ChangePhase(Phase.Exhaust);
		string fallDownAnim = "Falldown_02";
		m_phaseTimer.CountDown(exhaustModeTime);
		m_monster.Fall(fallDownAnim);
		if (m_fx != null)
		{
			m_fx.SwapTexture(0);
		}
		m_coroutineTimer.CountDown(maxCoroutineWaitTime);
		while (m_monster.AnimPlayer.IsPlaying && m_monster.AnimPlayer.CurrentAnimName == fallDownAnim)
		{
			if ((!m_wasRage && m_monster.Data.HpPercent <= extremeHpPercent) || m_coroutineTimer.Counter == 0f)
			{
				m_customSequence.Kill();
			}
			yield return null;
		}
	}

	private IEnumerator TriggerExtremeMode()
	{
		interuptable = false;
		m_wasRage = true;
		m_monster.Invincible = true;
		m_comboList.Clear();
		m_phaseTimer.Reset();
		Dbg.Log("TriggerExtremeMode");
		m_coroutineTimer.CountDown(maxCoroutineWaitTime);
		while (!IsReadyToChangePhase())
		{
			if (m_coroutineTimer.Counter == 0f)
			{
				m_customSequence.Kill();
			}
			if (m_player.Data.Hp == 0f)
			{
				AnimationState monsterAnim = m_monster.AnimPlayer.CurrentAnim;
				while (monsterAnim != null && monsterAnim.wrapMode != WrapMode.Loop && m_monster.AnimPlayer.IsPlaying)
				{
					yield return null;
				}
				m_customSequence.Kill();
			}
			if (m_monster.CurrentState == Monster.State.Cutscene)
			{
				m_customSequence.Kill();
			}
			yield return null;
		}
		Dbg.Log("Ready to start extreme mode");
		ChangePhase(Phase.Extreme);
		m_monster.ChangeState(Monster.State.Cutscene);
		PlayerController playerController = m_player.gameObject.GetComponent<PlayerController>();
		playerController.enabled = false;
		m_monster.AnimPlayer.Play("Rage");
		m_coroutineTimer.CountDown(maxCoroutineWaitTime);
		while (m_monster.AnimPlayer.CurrentAnimName == "Rage" && m_monster.AnimPlayer.IsPlaying)
		{
			if (m_coroutineTimer.Counter == 0f)
			{
				m_customSequence.Kill();
			}
			yield return null;
		}
		SingletonComponent<BattleScreenUI>.Instance.ShowFeedbackFx(false, null);
		if (m_monster.GetRelativeDirection(m_player.transform.position, 10f) == 0)
		{
			yield break;
		}
		m_monster.Turn(m_player.transform.position);
		Dbg.Log("Begin turn.");
		m_coroutineTimer.CountDown(maxCoroutineWaitTime);
		while (!m_playerFinishKB || m_monster.IsTurning())
		{
			if (m_coroutineTimer.Counter == 0f)
			{
				m_customSequence.Kill();
			}
			yield return null;
		}
		Dbg.Log("Finsihed turn.");
	}

	private void OnFinishExhaustSequence(bool p_wasTerminated)
	{
		interuptable = true;
		m_monster.Idle();
		if (p_wasTerminated)
		{
			m_phaseTimer.Stop();
		}
		Dbg.Log("Exhaust sequences have finished.");
	}

	private void OnFinishExtremeSequence(bool p_wasTerminated)
	{
		m_monster.Invincible = false;
		interuptable = true;
		if (p_wasTerminated)
		{
			if (m_monster.CurrentState == Monster.State.Cutscene)
			{
				Dbg.Log("Extreme Mode sequence was terminated because cutscene is playing!");
			}
			else if (m_player.Data.Hp == 0f)
			{
				Dbg.Log("Extreme Mode sequence was terminated because player was already dead!");
			}
			else if (m_coroutineTimer.Counter == 0f)
			{
				Dbg.Log("Extreme Mode sequence was terminated because the timer was finished!");
			}
			else
			{
				Dbg.Log("Extreme Mode sequence was terminated without specific reason!");
			}
			if (m_monster.Data.Hp != 0f)
			{
				m_monster.AnimPlayer.Play("Idle_01");
			}
			SingletonComponent<BattleScreenUI>.Instance.ShowFeedbackFx(false, null);
		}
		else
		{
			Dbg.Log("Extreme sequences have finished.");
			m_phaseTimer.CountDown(extremeModeTime, "OnPhaseEnd");
			if (m_player.Data.Hp == 0f)
			{
				m_phaseTimer.Pause(true);
			}
		}
	}

	private bool IsReadyToChangePhase()
	{
		return base.enabled && m_monster.Data.Hp > 0f && m_monster.CanBecomeIdle() && m_phaseTimer.Counter == 0f && m_hitWindowPassed;
	}
}
