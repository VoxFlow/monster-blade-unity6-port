public class FirstCombatTutorialAi : TutorialMonsterAi
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
		m_immortal = false;
	}

	protected override void Start()
	{
		base.Start();
		m_immortal = true;
	}

	protected override void PrepareComboString(int p_comboCount)
	{
		m_comboList.Clear();
		if (m_isFreeBattle)
		{
			base.PrepareComboString(p_comboCount);
		}
	}

	protected override void OnStunEnd()
	{
		base.enabled = true;
		m_comboList.Clear();
		if (m_monster.CurrentState != Monster.State.Attack)
		{
			m_monster.Idle();
		}
	}

	protected override void UpdateGlobal()
	{
		if (m_isFreeBattle)
		{
			base.UpdateGlobal();
		}
	}

	protected override void UpdateIdle()
	{
		if (m_isFreeBattle)
		{
			base.UpdateIdle();
		}
	}
}
