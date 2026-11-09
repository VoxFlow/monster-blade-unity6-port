using System;
using System.Collections;
using UnityEngine;

public class StunLogic : SingletonComponent<StunLogic>
{
	private Monster m_monster;

	private AIController m_monsterAI;

	private NBTimer m_stunTimer;

	private string m_stunAnim;

	public bool IsMonsterStunning()
	{
		return m_stunTimer != null && m_stunTimer.Counter > 0f;
	}

	public void Pause(bool p_pause)
	{
		if (IsMonsterStunning())
		{
			m_stunTimer.Pause(p_pause);
		}
	}

	private void Start()
	{
		m_stunTimer = base.gameObject.AddComponent<NBTimer>();
		m_stunTimer.Name = "Stun Timer";
		Messenger.AddListener("battle_ready", OnBattleReady);
		Messenger.AddListener("battle_end", OnBattleEnd);
		Messenger<CutsceneController.CutsceneType>.AddListener("cutscene_start", OnCutsceneStart);
		Messenger<CutsceneController.CutsceneType>.AddListener("cutscene_end", OnCutsceneEnd);
	}

	private new void OnDestroy()
	{
		Messenger.RemoveListener("battle_ready", OnBattleReady);
		Messenger.RemoveListener("battle_end", OnBattleEnd);
		Messenger<CutsceneController.CutsceneType>.RemoveListener("cutscene_start", OnCutsceneStart);
		Messenger<CutsceneController.CutsceneType>.RemoveListener("cutscene_end", OnCutsceneEnd);
	}

	private void OnEnable()
	{
		Pause(true);
	}

	private void OnDisable()
	{
		Pause(false);
	}

	private void OnBattleReady()
	{
		GameObject enemyObject = SingletonComponent<StageManager>.Instance.EnemyObject;
		m_monster = enemyObject.GetComponent<Monster>();
		m_monsterAI = enemyObject.GetComponent<AIController>();
	}

	private void OnBattleEnd()
	{
		m_stunTimer.Reset();
	}

	private void OnCutsceneStart(CutsceneController.CutsceneType p_cutscene)
	{
		base.enabled = false;
	}

	private void OnCutsceneEnd(CutsceneController.CutsceneType p_cutscene)
	{
		base.enabled = true;
	}

	public bool StunMonster(float p_time, string p_anim = null)
	{
		if (!base.enabled || !m_monster.CanStun())
		{
			return false;
		}
		Dbg.Log("StunMonster: " + p_time);
		m_stunAnim = p_anim;
		m_monsterAI.enabled = false;
		m_stunTimer.CountDown(p_time, OnMonsterStunEnd);
		Messenger.Invoke("monster_stun");
		return true;
	}

	public IEnumerator StunMonsterAfterHit(float p_stunTime, Action p_onFailed = null)
	{
		Dbg.Log("StunLogic:StunMonsterAfterHit()");
		if (!base.enabled)
		{
			yield break;
		}
		if (m_monster == null || m_monster.CurrentState != Monster.State.Hit || m_monster.Data.Hp == 0f)
		{
			if (p_onFailed != null)
			{
				p_onFailed();
			}
		}
		else if (!StunMonster(p_stunTime) && p_onFailed != null)
		{
			p_onFailed();
		}
	}

	private void OnMonsterStunEnd()
	{
		if (!SingletonComponent<CutsceneController>.Instance.Playing && !SuperAttack.Playing)
		{
			Dbg.Log("OnMonsterStunEnd");
			m_monsterAI.enabled = true;
			Messenger.Invoke("monster_stun_end");
		}
	}

	private void Update()
	{
		if (IsMonsterStunning())
		{
			string p_animName = ((m_stunAnim != null) ? m_stunAnim : "Stun");
			if (m_monster.Data.Hp == 0f)
			{
				m_stunTimer.Stop();
			}
			else if (!m_monster.AnimPlayer.IsPlaying || m_monster.IsPlayingIdleAnimation())
			{
				m_monster.TryPlayAnim(p_animName);
			}
		}
	}
}
