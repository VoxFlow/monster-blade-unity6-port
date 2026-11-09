using UnityEngine;

public class HydraDamageLogic : MonsterDamageLogic
{
	private HydraHead.HeadEnum m_damageHead = HydraHead.HeadEnum.None;

	private Hydra m_hydra;

	private float m_centerAngle = 20f;

	private float m_criticalAngle = 90f;

	protected override void OnBattleReady()
	{
		base.OnBattleReady();
		m_hydra = (Hydra)m_monster;
	}

	protected override void OnMonsterGetHit(PlayerCollisionData p_collData)
	{
		base.OnMonsterGetHit(p_collData);
		if (p_collData.attack.type == PlayerAttack.Type.Skill)
		{
			m_damageHead = HydraHead.HeadEnum.Max;
			return;
		}
		Transform transform = p_collData.attacker.transform;
		float lookAngle = m_monster.transform.GetLookAngle(transform.position);
		float num = Mathf.Abs(lookAngle);
		Dbg.Log("[Info] Attack angle: " + num);
		m_damageHead = HydraHead.HeadEnum.None;
		if (num > m_centerAngle && num < m_criticalAngle)
		{
			if (Vector3.Cross(m_monster.transform.forward, transform.position - m_monster.transform.position).y > 3f)
			{
				if (!m_hydra.Heads[1].IsCutOff)
				{
					m_damageHead = HydraHead.HeadEnum.Right;
				}
				else if (!m_hydra.Heads[2].IsCutOff)
				{
					m_damageHead = HydraHead.HeadEnum.Middle;
				}
			}
			else if (!m_hydra.Heads[0].IsCutOff)
			{
				m_damageHead = HydraHead.HeadEnum.Left;
			}
			else if (!m_hydra.Heads[2].IsCutOff)
			{
				m_damageHead = HydraHead.HeadEnum.Middle;
			}
		}
		else if (num <= m_centerAngle)
		{
			switch (p_collData.attack.direction)
			{
			case PlayerAttack.Direction.LeftToRight:
			case PlayerAttack.Direction.TopLeftToBottomRight:
			case PlayerAttack.Direction.BottomLeftToTopRight:
				AttackFromRightSide();
				return;
			case PlayerAttack.Direction.RightToLeft:
			case PlayerAttack.Direction.TopRightToBottomLeft:
			case PlayerAttack.Direction.BottomRightToTopLeft:
				AttackFromLeftSide();
				return;
			}
			if (!m_hydra.Heads[2].IsCutOff)
			{
				m_damageHead = HydraHead.HeadEnum.Middle;
			}
		}
		else
		{
			Dbg.Log("[Info] Attack from the back");
		}
	}

	protected override DamageBarBehavior AddDamageBarComponent()
	{
		return base.gameObject.AddComponent<HydraDamageBar>();
	}

	public override void DealDamageToMonster(DamageData p_damage)
	{
		if (m_hydra.IsAllHeadCutOff() || m_hydra.IsRecovering)
		{
			base.DealDamageToMonster(p_damage);
			return;
		}
		if (m_damageHead == HydraHead.HeadEnum.None)
		{
			p_damage.damageType = DamageType.Block;
			p_damage.rawDamage = (p_damage.finalDamage = 0f);
			return;
		}
		bool flag = m_collData == null || m_collData.attack.type == PlayerAttack.Type.Special || m_collData.attack.type == PlayerAttack.Type.Ripost || m_collData.attack.type == PlayerAttack.Type.Repel;
		if (flag | (p_damage.damageType == DamageType.Special))
		{
			float p_dmg = p_damage.finalDamage / (float)m_hydra.GetNumHeadLeft();
			for (int i = 0; i < 3; i++)
			{
				if (!m_hydra.Heads[i].IsCutOff)
				{
					m_hydra.AttackOnHead((HydraHead.HeadEnum)i, p_dmg);
				}
			}
		}
		else
		{
			m_hydra.AttackOnHead(m_damageHead, p_damage.finalDamage);
			m_damageHead = HydraHead.HeadEnum.None;
		}
	}

	protected override void DetermineMonsterReaction()
	{
		if (!m_hydra.IsRecovering)
		{
			base.DetermineMonsterReaction();
		}
	}

	private void AttackFromRightSide()
	{
		Dbg.Log("[Info] Attack from monster's right side");
		if (m_hydra.Heads[1].IsCutOff)
		{
			if (!m_hydra.Heads[2].IsCutOff)
			{
				m_damageHead = HydraHead.HeadEnum.Middle;
			}
			else
			{
				m_damageHead = HydraHead.HeadEnum.None;
			}
		}
		else
		{
			m_damageHead = HydraHead.HeadEnum.Right;
		}
	}

	private void AttackFromLeftSide()
	{
		Dbg.Log("[Info] Attack from monster's left side");
		if (m_hydra.Heads[0].IsCutOff)
		{
			if (!m_hydra.Heads[2].IsCutOff)
			{
				m_damageHead = HydraHead.HeadEnum.Middle;
			}
			else
			{
				m_damageHead = HydraHead.HeadEnum.None;
			}
		}
		else
		{
			m_damageHead = HydraHead.HeadEnum.Left;
		}
	}
}
