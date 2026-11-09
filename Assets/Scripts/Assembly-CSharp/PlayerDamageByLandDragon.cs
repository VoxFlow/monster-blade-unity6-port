using UnityEngine;

public class PlayerDamageByLandDragon : PlayerDamageLogic
{
	protected override void CalculatePlayerDamage()
	{
		if (m_collData.attack.animationName == LandDragon.SLAM_ATK_ANIM)
		{
			m_damage.rawDamage = (m_damage.finalDamage = 999999f);
			m_damage.damageType = DamageType.Critical;
		}
		else if (m_collData.attack.animationName == LandDragon.RAKE_ATK_ANIM && m_blockSucceed)
		{
			m_damage.rawDamage = (m_damage.finalDamage = 0f);
			m_damage.damageType = DamageType.Block;
		}
		else
		{
			base.CalculatePlayerDamage();
		}
	}

	protected override void DeterminePlayerReaction()
	{
		LandDragon landDragon = (LandDragon)m_monster;
		if (m_collData.attack.animationName == LandDragon.SLAM_ATK_ANIM)
		{
			m_collData.attack.knockSpeed = landDragon.slamKnockSpeed;
		}
		if (m_player.Data.Hp > 0f && m_collData.attack.animationName != LandDragon.GEYSER_ATK_ANIM && landDragon.diving)
		{
			if (m_collData.attack.animationName == LandDragon.RAKE_ATK_ANIM)
			{
				if (!m_blockSucceed)
				{
					m_player.AnimPlayer.Play("Tumble");
				}
			}
			else if (m_collData.attack.animationName == LandDragon.SNAP_ATK_ANIM)
			{
				m_player.AnimPlayer.Play("HitFly");
			}
			if (m_player.CurrentState != Player.State.Strafe)
			{
				Vector3 p_targetPos = m_player.transform.position - m_player.transform.forward * 4f;
				m_player.GetComponent<CharacterMovement>().MoveTo(p_targetPos, 10f, 2f, 2.5f);
			}
			if (m_blockSucceed)
			{
				m_player.IsReady = true;
			}
			else
			{
				m_player.ChangeState(Player.State.Hit);
			}
		}
		else
		{
			base.DeterminePlayerReaction();
		}
	}

	protected override void OnPlayerFinishKnockback()
	{
		LandDragon landDragon = (LandDragon)m_monster;
		LandDragonAI landDragonAI = (LandDragonAI)m_monsterAI;
		if (landDragonAI.phase == LandDragonAI.Phase.BodySlam)
		{
			if (m_player.Data.Hp > 0f)
			{
				m_player.IsReady = true;
				m_player.Idle();
			}
		}
		else if (!landDragon.diving)
		{
			base.OnPlayerFinishKnockback();
		}
	}
}
