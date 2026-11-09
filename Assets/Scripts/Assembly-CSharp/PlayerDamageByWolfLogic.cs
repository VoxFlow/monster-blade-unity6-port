public class PlayerDamageByWolfLogic : PlayerDamageLogic
{
	protected override void DeterminePlayerReaction()
	{
		if (m_player.Data.Hp > 0f && m_collData.attack.animationName == "Signature")
		{
			if (m_collData.attack.attackPoint == "LRearTailTrailRenderer")
			{
				m_player.AnimPlayer.Play("Tumble");
				m_player.ChangeState(Player.State.Hit);
			}
			else
			{
				KnockPlayer(10f);
			}
		}
		else
		{
			base.DeterminePlayerReaction();
		}
	}
}
