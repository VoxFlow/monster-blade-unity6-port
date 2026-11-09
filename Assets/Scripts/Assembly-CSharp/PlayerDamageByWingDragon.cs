using UnityEngine;

public class PlayerDamageByWingDragon : PlayerDamageLogic
{
	private static readonly float explosionDamagePercent = 0.3f;

	protected override void CalculatePlayerDamage()
	{
		if (m_collData.attack.type == MonsterAttack.Type.Signature && m_collData.attack.multiplier == 100f)
		{
			if (m_player.Data.HpPercent > explosionDamagePercent)
			{
				m_damage.rawDamage = (m_damage.finalDamage = m_player.Data.MaxHp * explosionDamagePercent);
			}
			else
			{
				m_damage.rawDamage = (m_damage.finalDamage = 0f);
			}
			m_damage.damageType = DamageType.Normal;
		}
		else
		{
			base.CalculatePlayerDamage();
		}
	}

	protected override void DeterminePlayerReaction()
	{
		if (m_player.Data.Hp > 0f)
		{
			if (m_collData.attack.type == MonsterAttack.Type.Signature && m_collData.attack.multiplier == 100f)
			{
				m_player.PlayHitHeavyAnim(Player.HitReact.Bottom);
				m_player.ChangeState(Player.State.Hit);
				return;
			}
			if (m_collData.attack.type == MonsterAttack.Type.Heavy && Mathf.Approximately(0.05f, m_collData.attack.multiplier))
			{
				return;
			}
		}
		base.DeterminePlayerReaction();
	}
}
