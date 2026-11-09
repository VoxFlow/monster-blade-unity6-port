using UnityEngine;

public class StoneDragonDamageLogic : MonsterDamageLogic
{
	private MonsterPetrifiedLogic m_petrifyLogic;

	protected override void OnBattleStart()
	{
		base.OnBattleStart();
		m_petrifyLogic = Object.FindObjectOfType(typeof(MonsterPetrifiedLogic)) as MonsterPetrifiedLogic;
	}

	protected override void DetermineMonsterReaction()
	{
		if (!m_petrifyLogic.petrified)
		{
			base.DetermineMonsterReaction();
		}
	}
}
