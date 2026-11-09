using System;
using System.Collections.Generic;

public class Wolf : Monster
{
	protected override List<string> RetreiveIdleAnimList()
	{
		List<string> list = base.RetreiveIdleAnimList();
		list.Remove("Idle_Exhaust");
		list.Remove("Idle_04");
		return list;
	}

	protected override void InitAttackAnimations()
	{
		base.InitAttackAnimations();
		m_attackAnimations[0].Remove("Att_Exhaust_01");
	}

	protected override void OnExit(Enum p_current, Enum p_next)
	{
		State state = (State)(object)p_current;
		if (state == State.Attack)
		{
			base.GetComponent<UnityEngine.Collider>().enabled = true;
		}
		base.OnExit(p_current, p_next);
	}

	protected override void Update()
	{
		base.Update();
		if (base.CurrentState == State.Attack && base.CurrentAttack != null && base.CurrentAttack.animationName == "Signature" && m_anim.CurrentAnim != null)
		{
			int frame = m_anim.CurrentAnim.GetFrame();
			if (frame > 15 && frame <= 69)
			{
				base.GetComponent<UnityEngine.Collider>().enabled = false;
			}
			else
			{
				base.GetComponent<UnityEngine.Collider>().enabled = true;
			}
		}
	}
}
