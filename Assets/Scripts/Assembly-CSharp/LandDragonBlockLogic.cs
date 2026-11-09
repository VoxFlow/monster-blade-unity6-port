public class LandDragonBlockLogic : BlockSystem
{
	protected override void OnPlayerBlock()
	{
		LandDragon landDragon = (LandDragon)m_monster;
		if (!landDragon.diving)
		{
			base.OnPlayerBlock();
		}
	}

	protected override void OnPerfectBlock(MonsterAttack p_attack)
	{
		LandDragon landDragon = (LandDragon)m_monster;
		if (landDragon.diving)
		{
			m_player.AnimPlayer.Play("BlockHit");
			m_player.ChangeState(Player.State.Hit);
			m_perfectBlock = false;
			Messenger.Invoke("normal_block");
		}
		else
		{
			base.OnPerfectBlock(p_attack);
		}
	}
}
