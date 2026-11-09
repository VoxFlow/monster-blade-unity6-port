using System;

public class Bull : Monster
{
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
		if (base.CurrentState != State.Attack || base.CurrentAttack == null || !(m_anim.CurrentAnim != null))
		{
			return;
		}
		int frame = m_anim.CurrentAnim.GetFrame();
		if (base.CurrentAttack.animationName == "Att_Special_01")
		{
			if (frame > 24)
			{
				base.GetComponent<UnityEngine.Collider>().enabled = false;
			}
			else
			{
				base.GetComponent<UnityEngine.Collider>().enabled = true;
			}
		}
		else if (base.CurrentAttack.animationName == "Att_Up")
		{
			if (frame > 6 && frame < 36)
			{
				base.GetComponent<UnityEngine.Collider>().enabled = false;
			}
			else
			{
				base.GetComponent<UnityEngine.Collider>().enabled = true;
			}
		}
		else if (base.CurrentAttack.animationName == "Att_Combo_01")
		{
			if (frame > 18 && frame < 66)
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
