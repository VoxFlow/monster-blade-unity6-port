using System.Collections;
using UnityEngine;

public class LandDragonBattle : BattleLogic
{
	private bool m_slamHit;

	private LandDragon m_dragon;

	private BattleScreenUI m_battleUI;

	protected override void Awake()
	{
		base.Awake();
		m_battleUI = SingletonComponent<BattleScreenUI>.Instance;
		Messenger.AddListener("player_dead", OnPlayerDead);
		Messenger.AddListener(LandDragonEvent.LAND_DRAGON_DIVE, OnMonsterDive);
		Messenger.AddListener(LandDragonEvent.LAND_DRAGON_EMERGE, OnMonsterEmerge);
		Messenger.AddListener(LandDragonEvent.LAND_DRAGON_JUMP, OnLandDragonJump);
		Messenger.AddListener(LandDragonEvent.LAND_DRAGON_FINISH_JUMP, OnLandDragonFinishJump);
		Messenger<LandDragonAI.Phase>.AddListener(LandDragonEvent.LAND_DRAGON_PHASE_CHANGE, OnLandDragonChangePhase);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Messenger.RemoveListener("player_dead", OnPlayerDead);
		Messenger.RemoveListener(LandDragonEvent.LAND_DRAGON_DIVE, OnMonsterDive);
		Messenger.RemoveListener(LandDragonEvent.LAND_DRAGON_EMERGE, OnMonsterEmerge);
		Messenger.RemoveListener(LandDragonEvent.LAND_DRAGON_JUMP, OnLandDragonJump);
		Messenger.RemoveListener(LandDragonEvent.LAND_DRAGON_FINISH_JUMP, OnLandDragonFinishJump);
		Messenger<LandDragonAI.Phase>.RemoveListener(LandDragonEvent.LAND_DRAGON_PHASE_CHANGE, OnLandDragonChangePhase);
	}

	protected override void OnBattleStart()
	{
		base.OnBattleStart();
		m_dragon = (LandDragon)m_monster;
		if (!m_dragon.diving)
		{
			BattleUtils.EnableSkills(true, this);
			BlockSystem.enablePerfectBlock = true;
		}
	}

	protected void OnMonsterDive()
	{
		m_player.GetComponent<SmoothLookAt>().target = null;
		BattleUtils.EnableSkills(false, this);
		BlockSystem.enablePerfectBlock = false;
		StartCoroutine(PlayTumbleOnDive());
	}

	protected void OnMonsterEmerge()
	{
		m_player.GetComponent<SmoothLookAt>().target = m_monster.transform;
		if (!m_player.IsDead())
		{
			float f = (m_player.transform.position - m_monster.transform.position).sqrMagnitude - SingletonComponent<BattleManager>.Instance.BattleReferenceDistance * SingletonComponent<BattleManager>.Instance.BattleReferenceDistance;
			m_player.transform.LookAt(m_monster.transform.position);
			if (Mathf.Abs(f) > 0.001f)
			{
				StartCoroutine(PlayEmergeSequence());
			}
		}
	}

	protected void OnLandDragonJump()
	{
		m_playerController.AttackEnable = false;
		m_slamHit = false;
	}

	protected void OnLandDragonFinishJump()
	{
		m_playerController.AttackEnable = true;
		if (!m_slamHit)
		{
			m_playerController.DodgeEnable = false;
			BattleUtils.EnableSkills(false, this);
		}
	}

	private void OnPlayerDead()
	{
		BattleUtils.EnableSkills(true, this);
		BlockSystem.enablePerfectBlock = true;
	}

	private void OnPlayerHit(MonsterCollisionData p_collData)
	{
		if (p_collData.attack != null && p_collData.attack.animationName == LandDragon.SLAM_ATK_ANIM)
		{
			m_slamHit = true;
		}
	}

	private void OnLandDragonChangePhase(LandDragonAI.Phase p_phase)
	{
		switch (p_phase)
		{
		case LandDragonAI.Phase.Calm:
		case LandDragonAI.Phase.Aggressive:
			break;
		case LandDragonAI.Phase.Underground:
			break;
		}
	}

	private IEnumerator PlayEmergeSequence()
	{
		Vector3 toPos = SingletonComponent<BattleManager>.Instance.GetPlayerStartPosition();
		m_monsterAI.enabled = false;
		m_playerController.enabled = false;
		CharacterMovement moveScript = m_player.GetComponent<CharacterMovement>();
		Dbg.Log("Wait until can be idle.");
		while (!m_player.CanBecomeIdle())
		{
			yield return null;
		}
		m_player.Idle();
		m_player.RunTo(toPos, 10f);
		Dbg.Log("Wait until player finish running.");
		while (moveScript.IsMoving())
		{
			yield return null;
		}
		m_monsterAI.enabled = true;
		m_playerController.enabled = true;
		m_player.Idle();
		BattleUtils.EnableSkills(true, this);
		BlockSystem.enablePerfectBlock = true;
	}

	private IEnumerator PlayTumbleOnDive()
	{
		if (!m_player.IsDead() && !m_player.IsMoving())
		{
			m_playerController.enabled = false;
			yield return StartCoroutine(m_player.AnimPlayer.PlayAndWait("Tumble"));
			m_playerController.enabled = true;
			m_player.Idle();
		}
	}
}
