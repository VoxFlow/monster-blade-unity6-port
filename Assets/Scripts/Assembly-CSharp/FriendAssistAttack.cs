public abstract class FriendAssistAttack : FriendAssistSkill
{
	protected void PlayHitReaction()
	{
		base.monster.Idle();
		MonsterSpecialFx component = base.monster.GetComponent<MonsterSpecialFx>();
		if (component != null)
		{
			component.PlayRandomHitFX();
		}
		if (!base.monster.PlayHitHeavyAnim(PlayerAttack.Direction.BottomToTop))
		{
			base.monster.PlayHitAnim(PlayerAttack.Direction.BottomToTop);
		}
	}
}
