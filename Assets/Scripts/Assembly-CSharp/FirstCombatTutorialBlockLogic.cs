public class FirstCombatTutorialBlockLogic : BlockSystem
{
	private bool m_isFreeBattle;

	protected override void Awake()
	{
		base.Awake();
		Messenger.AddListener("tutorial_free_battle", OnTutorialFreeBattle);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Messenger.RemoveListener("tutorial_free_battle", OnTutorialFreeBattle);
	}

	private void OnTutorialFreeBattle()
	{
		m_isFreeBattle = true;
	}

	protected override void OnPlayerBlock()
	{
		if (m_isFreeBattle)
		{
			base.OnPlayerBlock();
		}
		else
		{
			m_perfectBlockTimer.CountDown(100f);
		}
	}
}
