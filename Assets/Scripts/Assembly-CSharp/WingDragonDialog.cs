using System;
using System.Collections.Generic;
using Models;
using UnityEngine;

public class WingDragonDialog : Tutorial
{
	private System.Action m_dungeonClearedEndCallback;

	protected override void OnEnable()
	{
		Messenger.AddListener("battle_start_ui", OnBattleStartUI);
		Messenger.AddListener("battle_exit", OnBattleExit);
		Messenger<System.Action>.AddListener("dungeon_cleared_end", OnDungeonClearedEnd);
	}

	protected override void OnDisable()
	{
		Messenger.RemoveListener("battle_start_ui", OnBattleStartUI);
		Messenger.RemoveListener("battle_exit", OnBattleExit);
		Messenger<System.Action>.RemoveListener("dungeon_cleared_end", OnDungeonClearedEnd);
	}

	private void OnBattleStartUI()
	{
		TryAdvance("battle_start_ui");
	}

	private void OnBattleExit()
	{
		TryAdvance("battle_exit");
	}

	private void OnDungeonClearedEnd(System.Action p_callback)
	{
		m_dungeonClearedEndCallback = p_callback;
		TryAdvance("dungeon_cleared_end");
	}

	protected override void Awake()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	public override void Start()
	{
		base.Start();
	}

	private void ShowSubtitleBattleExit(Action p_action)
	{
		BattleManager instance = SingletonComponent<BattleManager>.Instance;
		if (null != instance && instance.IsPlayerWin)
		{
			ShowSubtitle(p_action);
			SingletonComponent<BattleManager>.Instance.ChangeState(BattleManager.State.None);
		}
		else
		{
			SingletonComponent<TutorialManager>.Instance.RemoveTutorial(State.WingDragon);
		}
	}

	private void DoNothing()
	{
		HideSubtitle();
	}

	private void ExitBattle()
	{
		HideSubtitle();
		SingletonComponent<BattleManager>.Instance.ChangeState(BattleManager.State.Exit);
	}

	private void ShowSubtitleHoldScreen(Action p_action)
	{
		DungeonManager instance = SingletonComponent<DungeonManager>.Instance;
		InventoryManager instance2 = SingletonComponent<InventoryManager>.Instance;
		if (null != instance && null != instance2)
		{
			List<ServerInventoryItem> chestRewards = instance.ChestRewards;
			foreach (ServerInventoryItem item in chestRewards)
			{
				ItemDetails byDBID = instance2.GetByDBID(item.db_id);
				if (byDBID != null)
				{
					string setID = byDBID.equipment.setID;
					if (setID.Contains(GLOBAL.FORESTSTRIDER_SETNAME))
					{
						ShowSubtitle(p_action);
						return;
					}
				}
			}
		}
		SingletonComponent<TutorialManager>.Instance.RemoveTutorial(State.WingDragon);
	}

	private void End()
	{
		HideSubtitle();
		if (m_dungeonClearedEndCallback != null)
		{
			m_dungeonClearedEndCallback();
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
