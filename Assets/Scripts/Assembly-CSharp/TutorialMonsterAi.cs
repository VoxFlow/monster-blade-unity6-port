using UnityEngine;

public class TutorialMonsterAi : AIController
{
	public GameObject m_eventTarget;

	protected bool m_immortal;

	private bool m_allowAttack = true;

	public bool allowAttack
	{
		get
		{
			return m_allowAttack;
		}
		set
		{
			m_allowAttack = value;
		}
	}

	public bool isImmortal
	{
		get
		{
			return m_immortal;
		}
		set
		{
			m_immortal = value;
		}
	}

	protected override bool NeedToPlayIdleAnim()
	{
		return base.NeedToPlayIdleAnim() || !m_allowAttack;
	}

	private void OnMonsterGetHit(PlayerCollisionData collisionData)
	{
		if ((bool)m_monster)
		{
		}
	}

	private void OnRegen()
	{
		m_monster.Data.HpPercent = 1f;
	}

	private void OnSPAttack_Disable()
	{
	}

	private void OnSPAttack_Enable()
	{
	}

	protected override void PrepareComboString(int p_comboCount)
	{
		if (m_allowAttack)
		{
			m_behavior.hasSpecialAttack = false;
			base.PrepareComboString(p_comboCount);
		}
	}

	protected override void FixedUpdate()
	{
		if (m_immortal && m_monster.Data.Hp <= 0f)
		{
			m_monster.Data.Hp = 1f;
		}
		base.FixedUpdate();
	}

	private void OnFreezeAttack()
	{
		if (null != m_eventTarget)
		{
			m_eventTarget.SendMessage("OnFreezeAttack", SendMessageOptions.DontRequireReceiver);
		}
	}
}
