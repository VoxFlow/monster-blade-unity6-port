using UnityEngine;

public class WingDragon : Monster
{
	private bool m_isFlameOnPlayer;

	public static readonly string SIGNATURE_ATK_BEGIN_ANIM = "Signature_Attack";

	public static readonly string SIGNATURE_ATK_IDLE_ANIM = "Signature_Attack_Idle";

	public static readonly string SIGNATURE_ATK_LANDIG_ANIM = "Signature_Attack_Landing";

	public bool isFlameOnPlayer
	{
		get
		{
			return m_isFlameOnPlayer;
		}
		set
		{
			m_isFlameOnPlayer = value;
		}
	}

	public void SetAttackable(bool isAttackable)
	{
		base.gameObject.GetComponent<Collider>().enabled = isAttackable;
	}

	public GameObject GetAttackPointNode(string p_pointName)
	{
		Collider[] attackPoints = m_attackPoints;
		foreach (Collider collider in attackPoints)
		{
			if (collider.name == p_pointName)
			{
				return collider.gameObject;
			}
		}
		Dbg.LogWarn("WingDragon::GetAttackPointPosition(): Not found: " + p_pointName);
		return null;
	}

	public void PlayPreFireballAttack()
	{
		m_anim.Play(SIGNATURE_ATK_BEGIN_ANIM, 0.1f);
	}

	public void PlayFireballIdle()
	{
		m_anim.Play(SIGNATURE_ATK_IDLE_ANIM, 0.1f);
	}

	public void PlayFireballLanding()
	{
		m_anim.Play(SIGNATURE_ATK_LANDIG_ANIM, 0.1f);
	}

	public bool IsFlying()
	{
		if (m_anim == null || m_anim.CurrentAnim == null)
		{
			return false;
		}
		int frame = m_anim.CurrentAnim.GetFrame();
		bool flag = m_anim.CurrentAnimName == SIGNATURE_ATK_IDLE_ANIM;
		flag |= m_anim.CurrentAnimName == SIGNATURE_ATK_BEGIN_ANIM && frame >= 9;
		return flag | (m_anim.CurrentAnimName == SIGNATURE_ATK_LANDIG_ANIM && frame < 6);
	}

	protected override void Update()
	{
		base.Update();
		if (base.gameObject.GetComponent<Collider>() != null)
		{
			if (IsFlying())
			{
				base.gameObject.GetComponent<Collider>().enabled = false;
			}
			else
			{
				base.gameObject.GetComponent<Collider>().enabled = true;
			}
		}
	}

	protected override void OnAnimEnded(string p_anim)
	{
		if (base.enabled)
		{
			if (SIGNATURE_ATK_BEGIN_ANIM == p_anim)
			{
				base.AnimPlayer.Play(SIGNATURE_ATK_IDLE_ANIM, 0.1f);
			}
			else if (SIGNATURE_ATK_IDLE_ANIM == p_anim)
			{
				base.AnimPlayer.Play(SIGNATURE_ATK_LANDIG_ANIM, 0.1f);
			}
			else
			{
				base.OnAnimEnded(p_anim);
			}
		}
	}
}
