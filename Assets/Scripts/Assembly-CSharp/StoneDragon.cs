using System.Collections.Generic;
using UnityEngine;

public class StoneDragon : Monster
{
	[SerializeField]
	private string m_petrifyAttackAnimation = "Att_Petrified";

	[SerializeField]
	private float m_petrifyDuration = 10f;

	[SerializeField]
	private float m_petrifyShakeDuration = 3f;

	[SerializeField]
	private float m_petrifiedArmourMod = 1.3f;

	[SerializeField]
	private float m_saveTimePerTap = 3f;

	[SerializeField]
	private float m_maxSaveTime = 5f;

	private List<KeyValuePair<string, MonsterAttack.Type>> m_attackAnimationSets;

	public string petrifyAttackAnimation
	{
		get
		{
			return m_petrifyAttackAnimation;
		}
	}

	public float petrifyDuration
	{
		get
		{
			return m_petrifyDuration;
		}
	}

	public float petrifyShakeDuration
	{
		get
		{
			return m_petrifyShakeDuration;
		}
	}

	public float petrifiedArmourMod
	{
		get
		{
			return m_petrifiedArmourMod;
		}
	}

	public float saveTimePerTap
	{
		get
		{
			return m_saveTimePerTap;
		}
	}

	public float maxSaveTime
	{
		get
		{
			return m_maxSaveTime;
		}
	}

	public bool performingPetrifyAttack
	{
		get
		{
			return m_currentAttack != null && m_currentAttack.animationName == petrifyAttackAnimation;
		}
	}

	public List<KeyValuePair<string, MonsterAttack.Type>> attackAnimationSets
	{
		get
		{
			return m_attackAnimationSets;
		}
	}

	protected override void InitAttackAnimations()
	{
		base.InitAttackAnimations();
		m_attackAnimations[0].Remove("Att_Up");
		m_attackAnimationSets = new List<KeyValuePair<string, MonsterAttack.Type>>();
		foreach (string item in m_attackAnimations[0])
		{
			Dbg.Log("Adding weak attack: " + item);
			m_attackAnimationSets.Add(new KeyValuePair<string, MonsterAttack.Type>(item, MonsterAttack.Type.Weak));
		}
		foreach (string item2 in m_attackAnimations[1])
		{
			Dbg.Log("Adding heavy attack: " + item2);
			m_attackAnimationSets.Add(new KeyValuePair<string, MonsterAttack.Type>(item2, MonsterAttack.Type.Heavy));
		}
	}

	protected override void UpdateDeath()
	{
		if (hasDeathCutscene)
		{
			base.UpdateDeath();
			return;
		}
		bool flag = m_faders != null && !m_faders[0].enabled;
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
}
