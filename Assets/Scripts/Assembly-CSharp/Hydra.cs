using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hydra : Monster
{
	private bool m_recovering;

	private HydraHead[] m_heads = new HydraHead[3];

	private List<HydraHead.HeadEnum> m_availableHeads = new List<HydraHead.HeadEnum>();

	private List<string> m_availableAttacks = new List<string>();

	private string m_bodyMesh = "Mob05_Body";

	private bool m_battleEnd;

	public static readonly float maxSpeedModifier = 1.75f;

	private string[] m_headAttackPoints = new string[3] { "Mob05 Jaw Left", "Mob05 Jaw Right", "Mob05 Jaw" };

	private readonly string[] m_comboAttackAnims = new string[3] { "Att_Combo_01", "Att_Combo_02", "Att_Combo_03" };

	public HydraHead[] Heads
	{
		get
		{
			return m_heads;
		}
	}

	public bool IsRecovering
	{
		get
		{
			return m_recovering;
		}
	}

	public string[] ComboAttackAnims
	{
		get
		{
			return m_comboAttackAnims;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_anim.TimeScale = 1f;
		m_heads[0] = base.transform.FindInHierarchy("Mob05_LHead").GetComponent<HydraHead>();
		m_heads[1] = base.transform.FindInHierarchy("Mob05_RHead").GetComponent<HydraHead>();
		m_heads[2] = base.transform.FindInHierarchy("Mob05_MHead").GetComponent<HydraHead>();
	}

	protected override void OnEnter(Enum p_previous, Enum p_current)
	{
		State state = (State)(object)p_current;
		if (state == State.Death)
		{
			for (int i = 0; i < 3; i++)
			{
				m_heads[i].OnMonsterDead();
				if (m_faders[i].name == m_bodyMesh)
				{
					m_faders[i].Fade(1f, 0f, m_fadeSpeed, null);
				}
			}
			ShadowProjector componentInChildren = MBHier.GetComponentInChildren<ShadowProjector>(base.gameObject);
			componentInChildren.gameObject.active = false;
			StopCoroutine("RecoverRoar");
		}
		base.OnEnter(p_previous, p_current);
	}

	protected override void OnBattleStart()
	{
		base.OnBattleStart();
		m_battleEnd = false;
	}

	protected override void OnBattleEnd()
	{
		base.OnBattleEnd();
		m_battleEnd = true;
	}

	protected override void OnDamageWindow()
	{
		base.OnDamageWindow();
		int num = -1;
		for (int i = 0; i < 3; i++)
		{
			if (base.CurrentAttack.attackPoint == m_headAttackPoints[i])
			{
				num = i;
				break;
			}
		}
		if (num != -1 && m_heads[num].IsCutOff)
		{
			EnableAttackPoint(m_headAttackPoints[num], false);
		}
	}

	protected override void OnCutsceneStart(CutsceneController.CutsceneType p_type)
	{
		base.OnCutsceneStart(p_type);
		HydraHead[] heads = m_heads;
		foreach (HydraHead hydraHead in heads)
		{
			hydraHead.enabled = false;
		}
	}

	protected override void OnCutsceneEnd(CutsceneController.CutsceneType p_type)
	{
		base.OnCutsceneEnd(p_type);
		if (base.Data.Hp != 0f && base.CurrentState != State.Fall)
		{
			HydraHead[] heads = m_heads;
			foreach (HydraHead hydraHead in heads)
			{
				hydraHead.enabled = true;
			}
		}
	}

	public override void Fall(string p_fallAnim)
	{
		base.Fall(p_fallAnim);
		for (int i = 0; i < 3; i++)
		{
			m_heads[i].enabled = false;
		}
	}

	public override void Idle()
	{
		if (ChangeState(State.Idle))
		{
			RandomIdleAnim(1f, true);
			SendMessage("OnIdle");
		}
	}

	public override bool CanPlayCutscene()
	{
		return base.CanPlayCutscene() && !m_recovering;
	}

	public override bool CanStun()
	{
		return base.CanStun() && !m_recovering;
	}

	public bool IsAllHeadCutOff()
	{
		for (int i = 0; i < m_heads.Length; i++)
		{
			if (!m_heads[i].IsCutOff)
			{
				return false;
			}
		}
		return true;
	}

	public int GetNumHeadLeft()
	{
		int num = 0;
		for (int i = 0; i < 3; i++)
		{
			num += ((!m_heads[i].IsCutOff) ? 1 : 0);
		}
		return num;
	}

	public List<HydraHead.HeadEnum> GetAvailableHeads()
	{
		m_availableHeads.Clear();
		for (int i = 0; i < 3; i++)
		{
			if (!m_heads[i].IsCutOff)
			{
				m_availableHeads.Add((HydraHead.HeadEnum)i);
			}
		}
		return m_availableHeads;
	}

	public List<string> GetAvailableAttacks()
	{
		m_availableAttacks.Clear();
		int num = 0;
		for (int i = 0; i < 3; i++)
		{
			if (!m_heads[i].IsCutOff)
			{
				num++;
			}
		}
		for (int j = 0; j < m_allAttacks.Length; j++)
		{
			string text = m_allAttacks[j];
			MonsterAttackParameter monsterAttackParameter = base.AttackParameterDB[text];
			bool flag = true;
			if (monsterAttackParameter.types[0] == MonsterAttack.Type.Signature && num != 3)
			{
				continue;
			}
			for (int k = 0; k < monsterAttackParameter.attackPoints.Length; k++)
			{
				int num2 = Array.IndexOf(m_headAttackPoints, monsterAttackParameter.attackPoints[k]);
				if (num2 < 0 || m_heads[num2].IsCutOff)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				m_availableAttacks.Add(text);
			}
		}
		return m_availableAttacks;
	}

	private void ResetAllHeadHp()
	{
		float maxHp = m_data.Hp / 6f;
		for (int i = 0; i < 3; i++)
		{
			m_heads[i].MaxHp = maxHp;
		}
	}

	public float GetAllHeadHpPercent()
	{
		float num = 0f;
		for (int i = 0; i < 3; i++)
		{
			num += m_heads[i].HpPercent;
		}
		return num / 3f;
	}

	public IEnumerator RecoverRoar()
	{
		Dbg.Log("RecoverRoar");
		m_recovering = true;
		ResetAllHeadHp();
		for (int i = 0; i < 3; i++)
		{
			m_heads[i].Recover();
		}
		HydraSpecialFx vfx = base.gameObject.GetComponent<HydraSpecialFx>();
		if (vfx != null)
		{
			StartCoroutine(vfx.PlayExplosionEffect());
		}
		Waiter waiter = new Waiter();
		yield return StartCoroutine(waiter.WaitUntil(1.5f));
		if (m_data.Hp == 0f)
		{
			m_recovering = false;
			yield break;
		}
		base.Invincible = true;
		m_anim.Play("Rage");
		yield return StartCoroutine(waiter.WaitUntil(1f));
		SendMessage("OnRage", SendMessageOptions.DontRequireReceiver);
		while (m_anim.IsPlaying && m_anim.CurrentAnimName == "Rage")
		{
			yield return null;
		}
		for (int j = 0; j < 3; j++)
		{
			m_heads[j].enabled = true;
		}
		m_recovering = false;
		base.Invincible = false;
		m_anim.Play("Idle_01");
		SendMessage("OnFullyRecover", SendMessageOptions.DontRequireReceiver);
	}

	public void AttackOnHead(HydraHead.HeadEnum p_head, float p_dmg)
	{
		if (p_head != HydraHead.HeadEnum.None)
		{
			HydraHead hydraHead = m_heads[(int)p_head];
			hydraHead.TakeDamage(p_dmg);
			Dbg.Log(string.Concat(p_head, " is attacked for ", p_dmg, " and has ", hydraHead.Hp, " left."));
			if (hydraHead.IsCutOff)
			{
				EnableAttackPoint(m_headAttackPoints[(int)p_head], false);
			}
		}
	}

	protected override void UpdateGlobal()
	{
		base.UpdateGlobal();
		if (!m_battleEnd)
		{
			for (int i = 0; i < 3; i++)
			{
				m_heads[i].enabled = m_anim.enabled;
			}
		}
		m_speedModifier = Mathf.Lerp(maxSpeedModifier, 1f, m_data.HpPercent);
	}
}
