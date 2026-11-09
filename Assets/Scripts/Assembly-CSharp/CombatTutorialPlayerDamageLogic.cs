public class CombatTutorialPlayerDamageLogic : PlayerDamageLogic
{
	protected override void DealDamageToPlayer()
	{
		if (m_player.Data.Hp > m_damage.finalDamage)
		{
			base.DealDamageToPlayer();
		}
	}
}
