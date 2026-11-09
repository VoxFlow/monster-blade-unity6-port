public class EpicDragonDamageLogic : MonsterDamageLogic
{
	private EpicDragonAI m_dragonAI;

	private float m_nextPhaseHp;

	protected override void OnBattleReady()
	{
		base.OnBattleReady();
		if (null == m_dragonAI)
		{
			m_dragonAI = (EpicDragonAI)m_monsterAI;
		}
	}

	protected override void CalculateMonsterDamage()
	{
		base.CalculateMonsterDamage();
		if (m_collData.attack.type == PlayerAttack.Type.Special)
		{
			m_damage.finalDamage /= m_monster.Data.m_monsterData.resistances[(int)m_collData.attackerData.AttackElement];
		}
	}

	public override void DealDamageToMonster(DamageData p_damage)
	{
		DamageData damageData = p_damage;
		if (m_dragonAI.phase != EpicDragonAI.Phase.FINAL)
		{
			int num = (int)m_monster.Data.Hp;
			if (num <= (int)p_damage.finalDamage)
			{
				damageData = new DamageData(p_damage);
				damageData.finalDamage = num - 1;
				m_dragonAI.phase = EpicDragonAI.Phase.FINAL;
				m_dragonAI.enabled = false;
			}
		}
		base.DealDamageToMonster(damageData);
	}

	protected override void BroadcastEvent()
	{
		if (m_collData.gameObject.GetComponent<EpicDragonFireBall>() != null)
		{
			m_damage.damageType = DamageType.Special;
		}
		base.BroadcastEvent();
	}
}
