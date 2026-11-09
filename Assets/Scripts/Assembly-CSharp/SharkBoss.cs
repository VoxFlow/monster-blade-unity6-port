public class SharkBoss : Monster_TrashBoss
{
	protected override void Update()
	{
		base.Update();
		if (base.CurrentState == State.Attack && base.CurrentAttack != null && base.CurrentAttack.animationName == "Att_Special_01" && m_anim.CurrentAnim != null)
		{
			int frame = m_anim.CurrentAnim.GetFrame();
			if (frame > 15 && frame < 75)
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
