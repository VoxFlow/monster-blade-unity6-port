public class FirstCombatTutorialMonsterDamageLogic : TutorialMonsterDamageLogic
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

	public override void DealDamageToMonster(DamageData p_damage)
	{
		if (!m_isFreeBattle && m_monster.Data.Hp < p_damage.finalDamage)
		{
			p_damage.finalDamage = m_monster.Data.Hp - 1f;
		}
		base.DealDamageToMonster(p_damage);
	}
}
