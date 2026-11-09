using System;
using UnityEngine;

public class SuperAttackSystem : SingletonComponent<SuperAttackSystem>
{
	private SuperAttack m_activeSuperLogic;

	private SuperAttack[] m_superAttackLogics;

	private bool m_enableSkill = true;

	public SuperAttack superAttackLogic
	{
		get
		{
			return m_activeSuperLogic;
		}
	}

	public void ChangeSuperAttack(Type p_superAttack)
	{
		if (!typeof(SuperAttack).IsAssignableFrom(p_superAttack) || (null != m_activeSuperLogic && p_superAttack == m_activeSuperLogic.GetType()) || NBContainerUtil.IsNullOrEmpty(m_superAttackLogics))
		{
			return;
		}
		for (int i = 0; i < m_superAttackLogics.Length; i++)
		{
			if (null != m_superAttackLogics[i] && p_superAttack == m_superAttackLogics[i].GetType())
			{
				m_activeSuperLogic.enabled = false;
				m_superAttackLogics[i].enabled = true;
				m_superAttackLogics[i].Init();
				m_activeSuperLogic = m_superAttackLogics[i];
				break;
			}
		}
	}

	private void Awake()
	{
		m_superAttackLogics = base.gameObject.GetComponents<SuperAttack>();
		m_activeSuperLogic = base.gameObject.GetComponent<DefaultSuperAttack>();
		m_activeSuperLogic.enabled = true;
		if (null != SingletonComponent<PlayerDataManager>.Instance)
		{
			PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
			for (int i = 0; i < m_superAttackLogics.Length; i++)
			{
				m_superAttackLogics[i].PrebattleInit(playerData);
			}
		}
		Messenger.AddListener("battle_ready", OnBattleReady);
		Messenger.AddListener("player_super_atk", OnTriggerSuperAttack);
		Messenger.AddListener("battle_exit", OnExitBattle);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Messenger.RemoveListener("battle_ready", OnBattleReady);
		Messenger.RemoveListener("player_super_atk", OnTriggerSuperAttack);
		Messenger.RemoveListener("battle_exit", OnExitBattle);
	}

	private void OnBattleReady()
	{
		if (null != m_activeSuperLogic)
		{
			m_activeSuperLogic.Init();
		}
	}

	private void OnTriggerSuperAttack()
	{
		if (m_enableSkill && null != m_activeSuperLogic)
		{
			m_activeSuperLogic.OnTrigger();
		}
	}

	private void OnExitBattle()
	{
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
