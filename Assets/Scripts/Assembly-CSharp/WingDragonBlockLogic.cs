using UnityEngine;

public class WingDragonBlockLogic : BlockSystem
{
	protected override void OnPlayerGetHit(MonsterCollisionData p_collData)
	{
		WingDragon wingDragon = m_monster as WingDragon;
		if (!wingDragon.isFlameOnPlayer || !(null != p_collData) || p_collData.attack.type != MonsterAttack.Type.Heavy || !Mathf.Approximately(0.05f, p_collData.attack.multiplier))
		{
			base.OnPlayerGetHit(p_collData);
		}
	}
}
