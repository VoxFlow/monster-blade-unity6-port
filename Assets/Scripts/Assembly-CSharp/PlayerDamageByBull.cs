public class PlayerDamageByBull : PlayerDamageLogic
{
	private static readonly float knockSpeed = 5f;

	protected override void DeterminePlayerReaction()
	{
		MonsterAttack currentAttack = m_monster.CurrentAttack;
		if (currentAttack != null && currentAttack.zone == MonsterAttack.Zone.Center)
		{
			currentAttack.knockSpeed = knockSpeed;
		}
		base.DeterminePlayerReaction();
	}
}
