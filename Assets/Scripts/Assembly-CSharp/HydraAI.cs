using System;
using System.Collections.Generic;
using UnityEngine;

public class HydraAI : AIController
{
	public enum Mode
	{
		Normal = 0,
		Agressive = 1,
		Brutal = 2,
		Extreme = 3
	}

	private Mode m_mode;

	private Hydra m_hydra;

	private PlayerController m_playerController;

	private NBTimer m_timer;

	private bool m_comboAttack;

	public Mode mode
	{
		get
		{
			return m_mode;
		}
		set
		{
			m_mode = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_hydra = m_monster as Hydra;
		m_timer = base.gameObject.AddComponent<NBTimer>();
		m_timer.Name = "Recover Timer";
	}

	protected override void Start()
	{
		base.Start();
		base.gameObject.AddComponent<HydraDebugGUI>();
		m_playerController = m_player.GetComponent<PlayerController>();
		mode = Mode.Normal;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	protected override MonsterDebugGUI InitDebugGUI()
	{
		return base.gameObject.AddComponent<HydraDebugGUI>();
	}

	protected override void FixedUpdate()
	{
		if (!m_hydra.IsRecovering)
		{
			base.FixedUpdate();
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Keypad0))
		{
			m_monster.Data.HpPercent = 1f;
		}
		else if (Input.GetKeyDown(KeyCode.Keypad1))
		{
			m_monster.Data.HpPercent = 0.75f;
		}
		else if (Input.GetKeyDown(KeyCode.Keypad2))
		{
			m_monster.Data.HpPercent = 0.5f;
		}
		else if (Input.GetKeyDown(KeyCode.Keypad3))
		{
			m_monster.Data.HpPercent = 0.25f;
		}
		if (Input.GetKeyDown(KeyCode.Keypad9))
		{
			m_hydra.Heads[0].Hp = 0f;
		}
		else if (Input.GetKeyDown(KeyCode.Keypad7))
		{
			m_hydra.Heads[1].Hp = 0f;
		}
		else if (Input.GetKeyDown(KeyCode.Keypad8))
		{
			m_hydra.Heads[2].Hp = 0f;
		}
		else if (Input.GetKeyDown(KeyCode.P))
		{
			for (int i = 0; i < 3; i++)
			{
				m_hydra.Heads[i].Hp = 0f;
			}
		}
	}

	protected override void UpdateBehavior(float p_hp)
	{
		if (p_hp > 0f && p_hp <= 0.25f)
		{
			m_mode = Mode.Extreme;
			m_behavior.comboCount = 7;
			m_behavior.hasAdvanceAttack = true;
			m_behavior.hasSpecialAttack = true;
		}
		else if (p_hp > 0.25f && p_hp <= 0.5f)
		{
			m_mode = Mode.Brutal;
			m_behavior.comboCount = 6;
			m_behavior.hasAdvanceAttack = true;
			m_behavior.hasSpecialAttack = true;
		}
		else if (p_hp > 0.5f && p_hp <= 0.75f)
		{
			m_mode = Mode.Agressive;
			m_behavior.comboCount = 5;
			m_behavior.hasAdvanceAttack = true;
			m_behavior.hasSpecialAttack = false;
		}
		else if (p_hp > 0.75f && p_hp <= 1f)
		{
			m_mode = Mode.Normal;
			m_behavior.comboCount = 4;
			m_behavior.hasAdvanceAttack = false;
			m_behavior.hasSpecialAttack = false;
		}
		else
		{
			ChangeBehavior("DEAD");
		}
	}

	protected override void UpdateGlobal()
	{
		base.UpdateGlobal();
		if (m_monster.Data.Hp == 0f)
		{
			if (m_timer.Counter > 0f)
			{
				m_timer.Reset();
			}
		}
		else if (m_hydra.IsAllHeadCutOff() && m_hydra.CurrentState != Monster.State.Fall)
		{
			for (int i = 0; i < 3; i++)
			{
				m_hydra.Heads[i].enabled = false;
			}
			m_comboList.Clear();
			m_timer.CountDown(5f, "StartRecover");
			m_hydra.Fall("Falldown");
		}
	}

	protected override void PrepareComboString(int p_comboCount)
	{
		if (!(m_behavior.name == "DEAD"))
		{
			bool hasSpecialAttack = m_behavior.hasSpecialAttack;
			m_behavior.hasSpecialAttack = false;
			base.PrepareComboString(m_behavior.comboCount);
			m_behavior.hasSpecialAttack = hasSpecialAttack;
			Mode mode = m_mode;
			if ((mode == Mode.Brutal || mode == Mode.Extreme) && UnityEngine.Random.Range(0, 2) == 0)
			{
				m_comboList[m_comboList.Count - 1] = MonsterAttack.Type.Signature;
			}
			if (m_comboAttack)
			{
				m_comboList[0] = MonsterAttack.Type.Combo;
			}
		}
	}

	protected override string DetermineAttackAnim(MonsterAttack.Type p_attackType)
	{
		if (p_attackType == MonsterAttack.Type.Signature && m_hydra.GetNumHeadLeft() == 3)
		{
			return NBContainerUtil.RandomElement(m_monster.SpecialAttackAnimations);
		}
		string text = base.DetermineAttackAnim(p_attackType);
		bool flag = false;
		bool flag2 = Array.IndexOf(m_hydra.ComboAttackAnims, text) >= 0;
		if (m_comboAttack && !flag2)
		{
			text = NBContainerUtil.RandomElement(m_hydra.ComboAttackAnims);
			m_comboAttack = false;
			flag = true;
		}
		if (!flag)
		{
			List<string> availableAttacks = m_hydra.GetAvailableAttacks();
			if (availableAttacks.IndexOf(text) < 0)
			{
				text = NBContainerUtil.RandomElement(availableAttacks);
			}
		}
		Dbg.Log("DetermineAttackAnim: " + text);
		return text;
	}

	protected override void _Attack(MonsterAttack p_attack)
	{
		if (p_attack.type == MonsterAttack.Type.Signature)
		{
			p_attack = CreateSignatureAttackData();
			p_attack.animationName = DetermineAttackAnim(p_attack.type);
		}
		else
		{
			p_attack.knockSpeed = 0f;
		}
		m_monster.Attack(p_attack, 1f);
	}

	protected override void RandomIdleTime(string p_callback = null)
	{
		float num = (float)m_monster.Data.m_monsterData.idleBeforeAttack - 1f;
		float t = (m_monster.SpeedModifier - 1f) / 1.5f;
		float p_limit = Mathf.Lerp(num + 3f, num, t);
		m_idleTimer.CountDown(p_limit, p_callback);
		Debug.Log("--------- RandomIdleTime: " + m_idleTimer.Counter);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (m_monster.Data.Hp > 0f)
		{
			m_timer.Pause(false);
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		m_timer.Pause(true);
	}

	protected override void OnAnimEnded(string p_anim)
	{
		if (!m_hydra.IsRecovering)
		{
			base.OnAnimEnded(p_anim);
		}
	}

	protected override void OnWindDown()
	{
		if (m_currentAttack == null || m_currentAttack.comboIndex == m_currentAttack.comboLength)
		{
			base.OnFinishAttack();
		}
	}

	protected override void OnCutsceneStart(CutsceneController.CutsceneType p_type)
	{
		base.OnCutsceneStart(p_type);
		m_timer.Pause(true);
	}

	protected override void OnCutsceneEnd(CutsceneController.CutsceneType p_type)
	{
		base.OnCutsceneEnd(p_type);
		if (m_monster.Data.Hp > 0f)
		{
			m_timer.Pause(false);
		}
	}

	protected override void OnDead()
	{
		base.OnDead();
		m_timer.Reset();
	}

	private MonsterAttack CreateSignatureAttackData()
	{
		MonsterAttack monsterAttack = new MonsterAttack();
		monsterAttack.type = MonsterAttack.Type.Signature;
		monsterAttack.knockSpeed = 8f;
		return monsterAttack;
	}

	private void StartRecover()
	{
		base.enabled = false;
		StartCoroutine(m_hydra.RecoverRoar());
	}

	private void OnRage()
	{
		Dbg.Log("Hydra :: OnRage");
		m_player.IsReady = true;
		m_player.Idle();
		m_player.KnockBack(10f, OnPlayerFinishKnockBack);
		m_playerController.enabled = false;
		m_timer.CountDown(1f, "OnRageEnd");
		SingletonComponent<BattleScreenUI>.Instance.ShowFeedbackFx(true, null);
	}

	private void OnRageEnd()
	{
		SingletonComponent<BattleScreenUI>.Instance.ShowFeedbackFx(false, null);
	}

	private void OnFullyRecover()
	{
		m_comboAttack = true;
		base.enabled = true;
	}

	private void OnPlayerFinishKnockBack()
	{
		Vector3 p_targetPos = SingletonComponent<BattleManager>.Instance.BattleReferencePoint - m_player.transform.forward * SingletonComponent<BattleManager>.Instance.BattleReferenceDistance;
		m_player.RunTo(p_targetPos, 10f, OnPlayerFinishReturn);
	}

	private void OnPlayerFinishReturn()
	{
		m_playerController.enabled = true;
		m_monster.Idle();
	}
}
