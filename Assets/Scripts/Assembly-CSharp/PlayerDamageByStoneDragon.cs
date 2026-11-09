using UnityEngine;

public class PlayerDamageByStoneDragon : PlayerDamageLogic
{
	[SerializeField]
	private PlayerPetrifiedLogic m_playerPetrifyLogicPrefab;

	private PlayerPetrifiedLogic m_playerPetrifyLogic;

	private MonsterPetrifiedLogic m_monsterPetrifyLogic;

	private StoneDragon m_dragon;

	private int m_deathBlowCounter;

	private int m_deathBlowMaxCount = 3;

	public override void Init()
	{
		base.Init();
		m_dragon = (StoneDragon)m_monster;
		if (null == m_playerPetrifyLogic && null != m_playerPetrifyLogicPrefab)
		{
			m_playerPetrifyLogic = Object.Instantiate(m_playerPetrifyLogicPrefab) as PlayerPetrifiedLogic;
			if (null != m_playerPetrifyLogic)
			{
				m_playerPetrifyLogic.Init();
			}
		}
		if (null == m_monsterPetrifyLogic)
		{
			m_monsterPetrifyLogic = Object.FindObjectOfType(typeof(MonsterPetrifiedLogic)) as MonsterPetrifiedLogic;
			if (null != m_monsterPetrifyLogic)
			{
				m_monsterPetrifyLogic.Init();
			}
		}
	}

	protected override void CalculatePlayerDamage()
	{
		if (m_dragon.performingPetrifyAttack)
		{
			m_damage.rawDamage = 0f;
			m_damage.finalDamage = 0f;
		}
		else
		{
			base.CalculatePlayerDamage();
		}
	}

	protected override void DeterminePlayerReaction()
	{
		if (!m_dragon.performingPetrifyAttack && null != m_playerPetrifyLogic && !m_playerPetrifyLogic.petrified)
		{
			base.DeterminePlayerReaction();
		}
	}

	protected override void DealDamageToPlayer()
	{
		if (!m_dragon.performingPetrifyAttack)
		{
			if (m_playerPetrifyLogic.petrified)
			{
				m_deathBlowCounter++;
				if (m_deathBlowCounter >= m_deathBlowMaxCount)
				{
					m_player.Data.Hp = 0f;
				}
			}
			base.DealDamageToPlayer();
		}
		else
		{
			if (!m_perfectDefense && null != m_playerPetrifyLogic && (null == m_monsterPetrifyLogic || !m_monsterPetrifyLogic.petrified))
			{
				m_playerPetrifyLogic.OnPetrified(m_dragon.petrifyDuration, m_dragon.petrifiedArmourMod, null);
				m_deathBlowCounter = 0;
			}
			m_perfectDefense = false;
		}
	}

	protected override void BroadcastEvent()
	{
		if (m_dragon.performingPetrifyAttack)
		{
			m_damage.damageType = DamageType.None;
		}
		base.BroadcastEvent();
	}
}
