using System.Collections.Generic;
using UnityEngine;

public class StoneDragonAI : AIController
{
	public enum Phase
	{
		Calm = 0,
		Agressive = 1,
		Brutal = 2,
		Extreme = 3
	}

	private Phase m_phase;

	private float m_petrifyAttackChance = 0.5f;

	private bool m_timeToPetrify;

	private float m_defaultMinAtk;

	private float m_defaultMaxAtk;

	private List<MonsterAttack> m_attackSets = new List<MonsterAttack>();

	private PlayerPetrifiedLogic m_playerPetrifyScript;

	public Phase phase
	{
		get
		{
			return m_phase;
		}
		set
		{
			m_phase = value;
			switch (m_phase)
			{
			case Phase.Calm:
				m_monster.Data.MinAttack = m_defaultMinAtk;
				m_monster.Data.MaxAttack = m_defaultMaxAtk;
				m_monster.SpeedModifier = 1.75f;
				break;
			case Phase.Agressive:
				break;
			case Phase.Brutal:
				m_monster.Data.MinAttack = m_defaultMinAtk * 2f;
				m_monster.Data.MaxAttack = m_defaultMaxAtk * 2f;
				m_monster.SpeedModifier = 2f;
				break;
			case Phase.Extreme:
				break;
			}
		}
	}

	private PlayerPetrifiedLogic playerPetrifyScript
	{
		get
		{
			if (null == m_playerPetrifyScript)
			{
				m_playerPetrifyScript = Object.FindObjectOfType(typeof(PlayerPetrifiedLogic)) as PlayerPetrifiedLogic;
			}
			return m_playerPetrifyScript;
		}
	}

	public NBTimer idleTimer
	{
		get
		{
			return m_idleTimer;
		}
	}

	public List<MonsterAttack> attackSets
	{
		get
		{
			return m_attackSets;
		}
	}

	public MonsterAttack currentAttack
	{
		get
		{
			return m_currentAttack;
		}
	}

	private void PickAttackSets(int n)
	{
		m_attackSets.Clear();
		StoneDragon stoneDragon = m_monster as StoneDragon;
		for (int i = 0; i < n; i++)
		{
			int index = Random.Range(0, stoneDragon.attackAnimationSets.Count);
			KeyValuePair<string, MonsterAttack.Type> keyValuePair = stoneDragon.attackAnimationSets[index];
			if (!string.IsNullOrEmpty(keyValuePair.Key))
			{
				MonsterAttack monsterAttack = new MonsterAttack();
				monsterAttack.animationName = keyValuePair.Key;
				monsterAttack.type = keyValuePair.Value;
				m_attackSets.Add(monsterAttack);
			}
		}
		Dbg.Log("Pick: " + m_attackSets.Count + " attack sets.");
	}

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	protected override void Start()
	{
		base.Start();
		m_defaultMinAtk = m_monster.Data.MinAttack;
		m_defaultMaxAtk = m_monster.Data.MaxAttack;
		phase = Phase.Calm;
		PickAttackSets(7);
	}

	protected override void PrepareComboString(int p_comboCount)
	{
		if (m_attackSets.Count <= 0)
		{
			switch (m_phase)
			{
			case Phase.Calm:
				PickAttackSets(7);
				break;
			case Phase.Agressive:
				PickAttackSets(5);
				break;
			case Phase.Brutal:
				PickAttackSets(3);
				break;
			case Phase.Extreme:
				PickAttackSets(3);
				break;
			}
		}
	}

	protected override MonsterAttack PickAttackCommand()
	{
		MonsterAttack monsterAttack = null;
		if (m_timeToPetrify)
		{
			float num = Random.Range(0f, 1f);
			if (num <= m_petrifyAttackChance && playerPetrifyScript != null && !playerPetrifyScript.petrified)
			{
				monsterAttack = new MonsterAttack();
				monsterAttack.animationName = ((StoneDragon)m_monster).petrifyAttackAnimation;
				monsterAttack.type = MonsterAttack.Type.Ranged;
				Dbg.Log("PETRIFY ATTACK");
			}
			m_timeToPetrify = false;
		}
		else if (m_attackSets.Count > 0)
		{
			int index = Random.Range(0, m_attackSets.Count);
			monsterAttack = m_attackSets[index];
			Dbg.Log(monsterAttack.ToString());
			Dbg.Log("Attack sets count: " + m_attackSets.Count);
			m_attackSets.RemoveAt(index);
			if (m_attackSets.Count == 1)
			{
				m_timeToPetrify = true;
			}
		}
		if (monsterAttack == null)
		{
			Dbg.LogWarn("FAILED TO PICK VALID ATTACK!!");
			m_attackSets.Clear();
			m_timeToPetrify = false;
		}
		return monsterAttack;
	}

	protected override void TrackPlayerWhileAttacking()
	{
		if (m_currentAttack != null && m_monster.AnimPlayer.CurrentAnim != null && m_monster.AnimPlayer.CurrentAnimName != ((StoneDragon)m_monster).petrifyAttackAnimation)
		{
			base.TrackPlayerWhileAttacking();
		}
	}

	protected override void UpdateGlobal()
	{
		base.UpdateGlobal();
		switch (m_phase)
		{
		case Phase.Calm:
			if (m_monster.Data.HpPercent <= 0.75f)
			{
				phase = Phase.Agressive;
			}
			break;
		case Phase.Agressive:
			if (m_monster.Data.HpPercent <= 0.5f)
			{
				phase = Phase.Brutal;
			}
			break;
		case Phase.Brutal:
			if (m_monster.Data.HpPercent <= 0.25f)
			{
				phase = Phase.Extreme;
			}
			break;
		case Phase.Extreme:
			break;
		}
	}

	protected override MonsterDebugGUI InitDebugGUI()
	{
		return base.gameObject.AddComponent<StoneDragonDebugGUI>();
	}
}
