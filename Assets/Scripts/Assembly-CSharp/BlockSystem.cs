using System.Collections;
using UnityEngine;

public class BlockSystem : BattleLogic
{
	protected NBTimer m_perfectBlockTimer;

	protected bool m_perfectBlock;

	[SerializeField]
	protected float m_stunTime = 4f;

	[SerializeField]
	protected float m_blockStaminaCost = 0.4f;

	[SerializeField]
	protected float m_perfectBlockStaminaCost = 0.26f;

	public static bool alwaysPerfectBlock;

	public static float perfectBlockPeriod = 0.18f;

	public static bool enablePerfectBlock = true;

	protected override void Awake()
	{
		base.Awake();
		m_perfectBlockTimer = base.gameObject.AddComponent<NBTimer>();
		m_perfectBlockTimer.Name = "Perfect Block";
		enablePerfectBlock = true;
	}

	protected virtual void OnEnable()
	{
		Messenger.AddListener("player_block", OnPlayerBlock);
		Messenger.AddListener("player_finish_block", OnPlayerFinishBlock);
		Messenger<MonsterCollisionData>.AddListener("player_get_hit", OnPlayerGetHit);
	}

	protected virtual void OnDisable()
	{
		Messenger.RemoveListener("player_block", OnPlayerBlock);
		Messenger.RemoveListener("player_finish_block", OnPlayerFinishBlock);
		Messenger<MonsterCollisionData>.RemoveListener("player_get_hit", OnPlayerGetHit);
	}

	protected virtual void OnPlayerBlock()
	{
		if (base.enabled && enablePerfectBlock)
		{
			m_perfectBlockTimer.CountDown(perfectBlockPeriod);
		}
	}

	protected virtual void OnPlayerFinishBlock()
	{
	}

	protected virtual void OnPlayerGetHit(MonsterCollisionData p_collData)
	{
		bool flag = m_player.IsBlocking();
		bool flag2 = flag && p_collData.attack.IsBlockable();
		Dbg.Log("Blocking: " + m_player.IsBlocking() + ", Blockable: " + p_collData.attack.IsBlockable());
		if (!flag2)
		{
			if (flag)
			{
				Messenger.Invoke("block_failed");
			}
			return;
		}
		Dbg.Log("Block Timer: " + m_perfectBlockTimer.Counter);
		if (enablePerfectBlock && (m_perfectBlockTimer.Counter > 0f || alwaysPerfectBlock))
		{
			OnPerfectBlock(p_collData.attack);
		}
		else
		{
			OnNormalBlock(p_collData.attack);
		}
	}

	protected virtual void OnNormalBlock(MonsterAttack p_attack)
	{
		Dbg.Log("Normal Block");
		m_player.AnimPlayer.Play("BlockHit");
		m_player.Stamina -= m_blockStaminaCost;
		m_perfectBlock = false;
		Messenger.Invoke("normal_block");
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_player_block_effect");
	}

	protected virtual void OnPerfectBlock(MonsterAttack p_attack)
	{
		Dbg.Log("Perfect Block");
		m_perfectBlock = true;
		StartCoroutine(PlayPlayerRepelAttack());
		StartCoroutine(PlayMonsterHitReact());
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_player_perfect_block", 1f);
		Messenger.Invoke("perfect_block");
	}

	protected IEnumerator PlayPlayerRepelAttack()
	{
		m_player.ChangeState(Player.State.Idle);
		m_player.IsReady = false;
		m_player.Stamina -= m_perfectBlockStaminaCost;
		if (m_player.ComboRecognitionTimer != null)
		{
			m_player.ComboRecognitionTimer.Pause(true);
		}
		m_player.enabled = false;
		m_playerController.enabled = false;
		StartCoroutine(m_player.AnimPlayer.PlaySequence("BlockRepel", 0.15f));
		while (m_player.AnimPlayer.CurrentAnim.time <= 0.05f)
		{
			yield return null;
		}
		SingletonComponent<SlowMotionSystem>.Instance.LinearFade(15f, 0.6f);
		PlayerAttack.Direction[] attackDirections = new PlayerAttack.Direction[3]
		{
			PlayerAttack.Direction.TopToBottom,
			PlayerAttack.Direction.LeftToRight,
			PlayerAttack.Direction.BottomToTop
		};
		int animIndex = Random.Range(0, attackDirections.Length);
		m_player.RepelAttack(attackDirections[animIndex]);
		CutsceneController cutsceneController = SingletonComponent<CutsceneController>.Instance;
		while (null != m_player.AnimPlayer.CurrentAnim && m_player.AnimPlayer.CurrentAnim.normalizedTime < 0.67f)
		{
			if (cutsceneController.Playing)
			{
				Messenger.Invoke("perfect_block_end");
				yield break;
			}
			yield return null;
		}
		m_player.IsReady = true;
		if (m_player.CurrentState != Player.State.Cutscene)
		{
			if (m_monster != null && m_monster.Data != null && m_monster.Data.Hp > 0f)
			{
				m_player.enabled = true;
				m_playerController.enabled = true;
				m_player.Idle();
			}
			Messenger.Invoke("perfect_block_end");
		}
	}

	protected IEnumerator PlayMonsterHitReact()
	{
		m_monster.ChangeState(Monster.State.Idle);
		m_monsterAI.enabled = false;
		if (!m_monster.PlayHitHeavyAnim(PlayerAttack.Direction.BottomToTop))
		{
			m_monster.PlayHitAnim(PlayerAttack.Direction.BottomToTop);
		}
		while (m_monster.AnimPlayer.IsPlaying && m_monster.AnimPlayer.CurrentAnim.normalizedTime < 0.8f)
		{
			if (m_monster.Data.Hp == 0f || m_monster.CurrentState == Monster.State.Cutscene)
			{
				yield break;
			}
			yield return null;
		}
		if (m_monster.Data.Hp > 0f)
		{
			SingletonComponent<StunLogic>.Instance.StunMonster(m_stunTime);
		}
	}
}
