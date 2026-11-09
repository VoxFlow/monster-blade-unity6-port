using System;
using System.Collections.Generic;
using UnityEngine;

public class Monster_TrashBoss : Monster
{
	public enum eSPAttackFX
	{
		Roar = 0,
		Combo = 1,
		MAXCOUNT = 2
	}

	public float m_knockSpeed = 5f;

	private List<bool> m_SpecialAttackAbility;

	private int m_SpecialAttackCount;

	private string[] m_FXs;

	private MonsterSpecialFx m_FxScript;

	protected override void Awake()
	{
		base.Awake();
		m_FXs = new string[2];
		m_FXs[0] = "FX_TrashBossRoar";
		m_FXs[1] = "FX_TrashBossCombo";
		m_SpecialAttackCount = 0;
	}

	protected override void Start()
	{
		base.Start();
		m_FxScript = GetComponent<MonsterSpecialFx>();
	}

	protected override void OnWindUp(string p_attack)
	{
		base.OnWindUp(p_attack);
		if (base.CurrentState == State.Attack && base.CurrentAttack != null && (base.CurrentAttack.type == MonsterAttack.Type.Signature || base.CurrentAttack.type == MonsterAttack.Type.Roar))
		{
			AIController_TrashBoss component = base.gameObject.GetComponent<AIController_TrashBoss>();
			switch (component.CurrSPAttack)
			{
			case AIController_TrashBoss.eSPAttack.Heavy:
				base.CurrentAttack.type = MonsterAttack.Type.Heavy;
				break;
			case AIController_TrashBoss.eSPAttack.Combo:
				base.CurrentAttack.type = MonsterAttack.Type.Weak;
				break;
			case AIController_TrashBoss.eSPAttack.Roar:
				base.CurrentAttack.type = MonsterAttack.Type.Roar;
				base.CurrentAttack.knockSpeed = m_knockSpeed;
				break;
			}
		}
	}

	protected override void InitAttackAnimations()
	{
		base.InitAttackAnimations();
		List<string>[] array = new List<string>[6];
		for (int i = 0; i < 6; i++)
		{
			array[i] = new List<string>();
		}
		for (int j = 0; j < 6; j++)
		{
			if (j == 2)
			{
				continue;
			}
			foreach (string item in m_attackAnimations[j])
			{
				if (item.Contains("Atk_Special_") || item.Contains("Att_Special_"))
				{
					m_attackAnimations[2].Add(item);
					array[j].Add(item);
				}
			}
		}
		List<string> list = m_attackAnimations[2];
		MBSort.Sort(list);
		for (int k = 0; k < 6; k++)
		{
			foreach (string item2 in array[k])
			{
				m_attackAnimations[k].Remove(item2);
			}
		}
		InitSpecialAttackAbility();
	}

	private void InitSpecialAttackAbility()
	{
		if (m_data == null || m_data.m_monsterData == null)
		{
			return;
		}
		m_SpecialAttackAbility = new List<bool>();
		for (int i = 0; i < m_data.m_monsterData.specialAtkAbility.Length; i++)
		{
			if (!m_data.m_monsterData.specialAtkAbility[i])
			{
				m_SpecialAttackAbility.Add(false);
				continue;
			}
			m_SpecialAttackAbility.Add(true);
			m_SpecialAttackCount++;
		}
	}

	public int GetSpecialAttackMax()
	{
		return m_SpecialAttackAbility.Count;
	}

	public int GetSpecialAttackCount()
	{
		return m_SpecialAttackCount;
	}

	public bool GetSpecialAttackUseable(int type)
	{
		if (type >= m_SpecialAttackAbility.Count || type < 0)
		{
			Dbg.Log("Monster_TrashBoss::GetSpecialAttackAbility: type is out of range.");
		}
		return m_SpecialAttackAbility[type];
	}

	public MonsterCollisionData GetAttackCollisonData(string attackName)
	{
		Collider[] attackPoints = m_attackPoints;
		foreach (Collider collider in attackPoints)
		{
			if (collider.name == attackName)
			{
				return collider.gameObject.GetComponent<MonsterCollisionData>();
			}
		}
		Dbg.LogWarn("Attack point not found: " + attackName);
		return null;
	}

	public override bool ChangeState(Enum p_next)
	{
		SetAttackable(true);
		bool flag = base.ChangeState(p_next);
		if (!flag)
		{
			return flag;
		}
		if (m_currentState != State.Attack)
		{
			return flag;
		}
		ClearSPAttackFX();
		return flag;
	}

	public void SetAttackable(bool isAttackable)
	{
		base.gameObject.GetComponent<Collider>().enabled = isAttackable;
	}

	public void ClearSPAttackFX()
	{
		if (!(null == m_FxScript))
		{
			string[] fXs = m_FXs;
			foreach (string p_name in fXs)
			{
				m_FxScript.DisableParticle(p_name);
			}
		}
	}

	public void PlaySPAttackFX(eSPAttackFX type)
	{
		if (!(null == m_FxScript))
		{
			m_FxScript.EnableParticle(m_FXs[(int)type]);
		}
	}
}
