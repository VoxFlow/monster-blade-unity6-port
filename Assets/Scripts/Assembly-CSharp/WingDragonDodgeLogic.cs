using UnityEngine;

public class WingDragonDodgeLogic : DodgeSystem
{
	protected override void RipostDodgeLogic(Player.DodgeDirection p_dir)
	{
		if (GameObject.Find("FX_Wing_Flame(Clone)") != null)
		{
			m_dodgeResult = DodgeResult.Normal;
			StrafeLogic(p_dir);
			m_monsterAI.enabled = true;
		}
		else
		{
			base.RipostDodgeLogic(p_dir);
		}
	}
}
