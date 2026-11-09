using System.Collections;
using UnityEngine;

public class EpicDragonBlockLogic : BlockSystem
{
	[SerializeField]
	private float m_deflectDelayIdleTime = 0.2f;

	private EpicDragon m_dragon;

	private EpicDragonAI m_dragonAI;

	private EpicDragonFireBall m_fireBall;

	protected override void OnBattleReady()
	{
		base.OnBattleReady();
		m_dragon = (EpicDragon)m_monster;
		m_dragonAI = (EpicDragonAI)m_monsterAI;
	}

	protected override void OnPlayerGetHit(MonsterCollisionData p_collData)
	{
		if (null != p_collData && p_collData.attack != null && EpicDragon.IsFireBallAttack(p_collData.attack))
		{
			m_fireBall = p_collData.gameObject.GetComponent<EpicDragonFireBall>();
		}
		else
		{
			m_fireBall = null;
		}
		base.OnPlayerGetHit(p_collData);
	}

	protected override void OnPerfectBlock(MonsterAttack p_attack)
	{
		Dbg.Log("Perfect Block");
		if (null != m_fireBall)
		{
			m_perfectBlock = true;
			m_fireBall.playExplosion = false;
			m_fireBall.target = m_dragon.flyingTarget.position;
			m_fireBall.gameObject.tag = "Player";
			m_fireBall.GetComponent<Collider>().enabled = true;
			PlayerCollisionData playerCollisionData = m_fireBall.playerCollisionData;
			if (null != playerCollisionData)
			{
				playerCollisionData.attacker = m_player.gameObject;
				playerCollisionData.attackerData = m_player.Data;
				playerCollisionData.attack = new PlayerAttack();
				playerCollisionData.attack.type = PlayerAttack.Type.Special;
			}
			StartCoroutine(m_player.DelayIdle(m_deflectDelayIdleTime, true));
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_player_perfect_block", 1f);
			Messenger.Invoke("perfect_block");
		}
		else if (m_dragonAI.phase == EpicDragonAI.Phase.FINAL)
		{
			m_perfectBlock = true;
			StartCoroutine(PlayFallingRepelAttack());
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_player_perfect_block", 1f);
		}
		else if (!m_dragon.flying)
		{
			base.OnPerfectBlock(p_attack);
		}
	}

	protected override void OnPlayerFinishBlock()
	{
		if (m_perfectBlock && null != m_fireBall)
		{
			Messenger.Invoke("perfect_block_end");
		}
		m_fireBall = null;
	}

	private IEnumerator PlayFallingRepelAttack()
	{
		Messenger.Invoke("perfect_block");
		m_player.ChangeState(Player.State.Idle);
		m_player.IsReady = false;
		if (m_player.ComboRecognitionTimer != null)
		{
			m_player.ComboRecognitionTimer.Pause(true);
		}
		m_player.enabled = false;
		m_playerController.enabled = false;
		if (!m_monster.PlayHitHeavyAnim(PlayerAttack.Direction.BottomToTop))
		{
			m_monster.PlayHitAnim(PlayerAttack.Direction.BottomToTop);
		}
		yield return new MBWaitForSeconds(m_deflectDelayIdleTime);   // PORT (CD-059)
		m_player.IsReady = true;
		m_player.enabled = true;
		m_playerController.enabled = true;
		Messenger.Invoke("perfect_block_end");
	}
}
