using System;
using UnityEngine;

public class Dodobo : Monster
{
	private DodoboHUD m_hud;

	private float m_lifeTime = 30f;

	private bool m_battleStart;

	private bool m_bIsGold;

	private int m_goldGet;

	private NBTimer m_timeLimit;

	public bool IsGold
	{
		get
		{
			return m_bIsGold;
		}
		set
		{
			m_bIsGold = value;
		}
	}

	public int MoneyPerHit
	{
		get
		{
			if (m_data != null)
			{
				if (m_bIsGold)
				{
					return base.Data.Level;
				}
				return (int)((float)base.Data.Level / 1.5f);
			}
			return 0;
		}
	}

	public float GetDodoboLifeTime()
	{
		return m_timeLimit.Counter;
	}

	protected override void Awake()
	{
		base.Awake();
		m_timeLimit = base.gameObject.AddComponent<NBTimer>();
		m_timeLimit.Name = "Dodobo Timer";
		m_timeLimit.Pause(true);
		Messenger.AddListener("player_dead", OnPlayerDead);
	}

	protected override void Start()
	{
		base.Start();
		hasDeathCutscene = false;
		if (!hasIntroCutscene)
		{
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_jingle_dodobo_1");
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Messenger.RemoveListener("player_dead", OnPlayerDead);
	}

	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!(m_hud == null))
		{
			return;
		}
		BattleScreenUI instance = SingletonComponent<BattleScreenUI>.Instance;
		if (!(instance != null))
		{
			return;
		}
		Transform transform = instance.transform.FindInHierarchy("DodoboHUD_Panel");
		if (transform != null)
		{
			m_hud = transform.gameObject.GetComponent<DodoboHUD>();
			if (m_hud != null)
			{
				m_hud.gameObject.SetActiveRecursively(false);
			}
		}
	}

	protected override void UpdateGlobal()
	{
		base.UpdateGlobal();
		if (m_battleStart)
		{
			if (m_hud != null && m_hud.gameObject.active)
			{
				m_hud.UpdateCountdownTime(m_timeLimit.Counter);
			}
			if (m_timeLimit.Counter == 0f && IsReachableFromCurrent(State.Death))
			{
				base.Data.Hp = 0f;
			}
		}
	}

	protected override void OnEnter(Enum p_current, Enum p_next)
	{
		if (p_next.Equals(State.Death))
		{
			m_timeLimit.Reset();
		}
		base.OnEnter(p_current, p_next);
	}

	public override void TakeDamage(float p_damage)
	{
		base.TakeDamage(p_damage);
		int moneyPerHit = MoneyPerHit;
		m_goldGet += moneyPerHit;
		m_hud.UpdateMoney(m_goldGet);
	}

	public override bool PlayHitAnim(PlayerAttack.Direction p_dir)
	{
		string[] p_list = new string[2] { "React_01", "React_02" };
		m_anim.Play(NBContainerUtil.RandomElement(p_list), 0.3f, true);
		return true;
	}

	public override bool PlayHitHeavyAnim(PlayerAttack.Direction p_dir)
	{
		return PlayHitAnim(p_dir);
	}

	protected override void OnBattleStart()
	{
		base.OnBattleStart();
		if (m_hud != null)
		{
			m_hud.gameObject.SetActiveRecursively(true);
			m_hud.StartCountDown(this);
			m_hud.UpdateMoney(m_goldGet);
		}
		m_timeLimit.CountDown(m_lifeTime);
		m_timeLimit.Pause(false);
		m_battleStart = true;
	}

	protected override void OnCutsceneStart(CutsceneController.CutsceneType p_type)
	{
		base.OnCutsceneStart(p_type);
		m_timeLimit.Pause(true);
		if (m_hud != null && p_type != CutsceneController.CutsceneType.INTRO_DODOBO)
		{
			m_hud.gameObject.SetActiveRecursively(false);
		}
	}

	protected override void OnCutsceneEnd(CutsceneController.CutsceneType p_type)
	{
		base.OnCutsceneEnd(p_type);
		if (m_battleStart)
		{
			m_timeLimit.Pause(false);
		}
		if (m_hud != null && p_type != CutsceneController.CutsceneType.INTRO_DODOBO)
		{
			m_hud.gameObject.SetActiveRecursively(true);
		}
	}

	private void OnPlayerDead()
	{
		m_timeLimit.Pause(true);
	}

	protected override void OnBattleEnd()
	{
		base.OnBattleEnd();
		m_timeLimit.Pause(true);
		if (m_hud != null)
		{
			m_hud.gameObject.SetActiveRecursively(false);
		}
		SingletonComponent<BattleManager>.Instance.m_dodoboIgcBattle = m_goldGet;
		m_battleStart = false;
	}
}
