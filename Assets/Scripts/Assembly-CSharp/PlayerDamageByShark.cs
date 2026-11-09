using System;

public class PlayerDamageByShark : PlayerDamageLogic
{
	private static readonly string[] knockBackAnims = new string[1] { "Att_Special_01" };

	private static readonly float knockSpeed = 5f;

	protected override void DeterminePlayerReaction()
	{
		MonsterAttack currentAttack = m_monster.CurrentAttack;
		if (currentAttack != null && Array.IndexOf(knockBackAnims, currentAttack.animationName) >= 0)
		{
			currentAttack.knockSpeed = knockSpeed;
		}
		base.DeterminePlayerReaction();
	}
}
