using UnityEngine;

public class WolfBattle : BattleLogic
{
	private bool m_battleStart;

	protected override void Awake()
	{
		base.Awake();
		Messenger.AddListener("wolf_rage", OnRage);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Messenger.RemoveListener("wolf_rage", OnRage);
	}

	protected override void OnBattleReady()
	{
		base.OnBattleReady();
		base.enabled = false;
	}

	protected override void OnBattleStart()
	{
		m_battleStart = true;
	}

	protected override void OnBattleEnd()
	{
		m_battleStart = false;
	}

	private void OnRage()
	{
		base.enabled = true;
	}

	private void OnPlayerFinishReturn()
	{
		if (m_battleStart)
		{
			Dbg.Log("Re-enable player and monster controllers.");
			m_playerController.enabled = true;
			m_monsterAI.enabled = true;
			m_player.IsReady = true;
			m_player.Idle();
			m_monster.Idle();
		}
		base.enabled = false;
	}

	protected override void LateUpdate()
	{
		BattleManager instance = SingletonComponent<BattleManager>.Instance;
		Vector3 vector = instance.BattleReferencePoint - m_player.transform.forward * instance.BattleReferenceDistance;
		float num = Vector3.Distance(vector, m_player.transform.position);
		if (m_player.Data != null && m_player.Data.Hp != 0f && num > 0.001f && !m_player.IsMoving())
		{
			Dbg.Log("Waiting player to return");
			m_playerController.enabled = false;
			m_monsterAI.enabled = false;
			m_player.RunTo(vector, 10f, OnPlayerFinishReturn);
		}
		if (m_player.IsMoving() && m_monsterAI.enabled)
		{
			m_monsterAI.enabled = false;
		}
	}
}
