public class PlayerAttackEpicDragon : PlayerAttackLogic
{
	private const string FALLING_ATTACK_LEFT_ANIM = "Falling_Att_Left";

	private const string FALLING_ATTACK_RIGHT_ANIM = "Falling_Att_Right";

	private static readonly string[] ms_fallingAttacks = new string[2] { "Falling_Att_Left", "Falling_Att_Right" };

	private EpicDragonAI m_dragonAI;

	protected override void OnBattleReady()
	{
		base.OnBattleReady();
		m_dragonAI = (EpicDragonAI)m_monsterAI;
	}

	protected override void OnPlayerSwipe(PlayerAttack.Direction p_atkDir)
	{
		string[] array = new string[8]
		{
			string.Empty,
			string.Empty,
			"Falling_Att_Left",
			"Falling_Att_Right",
			"Falling_Att_Left",
			"Falling_Att_Right",
			"Falling_Att_Left",
			"Falling_Att_Right"
		};
		if (p_atkDir == PlayerAttack.Direction.None)
		{
			return;
		}
		if (m_dragonAI.phase == EpicDragonAI.Phase.FINAL)
		{
			string value = array[(int)p_atkDir];
			if (string.IsNullOrEmpty(value))
			{
				value = NBContainerUtil.RandomElement(ms_fallingAttacks);
			}
			PlayerAttack playerAttack = new PlayerAttack();
			playerAttack.animation = value;
			playerAttack.type = PlayerAttack.Type.Normal;
			playerAttack.direction = p_atkDir;
			m_player.RegisterAttackCommand(playerAttack);
			if (PlayerAttackLogic.m_blockSlashEnabled && m_player.IsBlocking())
			{
				m_player.IsReady = true;
			}
		}
		else
		{
			base.OnPlayerSwipe(p_atkDir);
		}
	}
}
