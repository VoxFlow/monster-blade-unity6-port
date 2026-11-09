using UnityEngine;

public abstract class FriendAssistBuff : FriendAssistSkill
{
	public float m_buffDuration = 15f;

	public GameObject m_buffFxPrefab;

	private ParticleSystemController m_buffFx;

	public NBTimer buffTimer { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		buffTimer = base.gameObject.AddComponent<NBTimer>();
		if (m_buffFxPrefab != null)
		{
			GameObject gameObject = Object.Instantiate(m_buffFxPrefab) as GameObject;
			if (gameObject != null)
			{
				m_buffFx = gameObject.GetComponent<ParticleSystemController>();
				if (m_buffFx != null)
				{
					m_buffFx.gameObject.SetActiveRecursively(false);
				}
			}
		}
		Messenger.AddListener("battle_end", OnBattleEnd);
		Messenger<CutsceneController.CutsceneType>.AddListener("cutscene_start", OnCutsceneStart);
		Messenger<CutsceneController.CutsceneType>.AddListener("cutscene_end", OnCutsceneFinish);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Messenger.RemoveListener("battle_end", OnBattleEnd);
		Messenger<CutsceneController.CutsceneType>.RemoveListener("cutscene_start", OnCutsceneStart);
		Messenger<CutsceneController.CutsceneType>.RemoveListener("cutscene_end", OnCutsceneFinish);
	}

	public override void OnSkillTrigger()
	{
		if (base.player != null && base.player.Data != null)
		{
			ApplyBuff(base.player.Data);
			buffTimer.CountDown(m_buffDuration, OnBuffExpire);
			if (m_buffFx != null)
			{
				m_buffFx.transform.Copy(base.player.transform);
				m_buffFx.transform.parent = base.player.transform;
				m_buffFx.Play();
			}
		}
	}

	protected abstract void ApplyBuff(PlayerData p_playerData);

	protected abstract void ResetBuff(PlayerData p_playerData);

	protected void OnBuffExpire()
	{
		if (base.player != null && base.player.Data != null)
		{
			ResetBuff(base.player.Data);
			if (m_buffFx != null)
			{
				m_buffFx.transform.parent = null;
				m_buffFx.Stop(false);
			}
		}
	}

	protected virtual void OnCutsceneStart(CutsceneController.CutsceneType p_type)
	{
		if (base.enabled)
		{
			buffTimer.Pause(true);
			if (m_buffFx != null && m_buffFx.IsPlaying())
			{
				m_buffFx.gameObject.active = false;
			}
		}
	}

	protected virtual void OnCutsceneFinish(CutsceneController.CutsceneType p_type)
	{
		if (base.enabled)
		{
			buffTimer.Pause(false);
			if (m_buffFx != null && m_buffFx.IsPlaying())
			{
				m_buffFx.gameObject.active = true;
			}
		}
	}

	protected virtual void OnBattleEnd()
	{
		if (base.enabled || !(buffTimer.Counter > 0f))
		{
			buffTimer.Stop();
		}
	}
}
