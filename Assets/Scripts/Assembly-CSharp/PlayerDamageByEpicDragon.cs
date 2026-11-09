public class PlayerDamageByEpicDragon : PlayerDamageLogic
{
	private const string FALLING_HIT_ANIM = "Falling_Hit";

	private const string FALLING_DEATH_ANIM = "Falling_Death";

	private bool m_fireAttack;

	private EpicDragonAI m_dragonAI;

	protected override void OnBattleReady()
	{
		base.OnBattleReady();
		m_dragonAI = (EpicDragonAI)m_monsterAI;
	}

	protected override void OnPlayerGetHit(MonsterCollisionData p_collData)
	{
		base.OnPlayerGetHit(p_collData);
		MonsterAttack attack = p_collData.attack;
		m_fireAttack = EpicDragon.IsFireBallAttack(attack) || EpicDragon.IsFireBreathAttack(attack);
	}

	protected override void DeterminePlayerReaction()
	{
		if (m_perfectDefense)
		{
			return;
		}
		m_blockSucceed |= m_player.Invincible;
		if (m_blockSucceed)
		{
			m_player.IsReady = true;
			return;
		}
		if (m_dragonAI.phase == EpicDragonAI.Phase.FINAL)
		{
			if (m_player.Data.Hp > 0f)
			{
				m_player.AnimPlayer.Play("Falling_Hit", 0.08f);
				m_player.ChangeState(Player.State.Hit);
			}
		}
		else
		{
			if (m_fireAttack && m_player.CurrentState != Player.State.Strafe)
			{
				StartCoroutine(m_player.DelayIdle(0.2f, true));
			}
			base.DeterminePlayerReaction();
		}
		m_fireAttack = false;
	}
}
