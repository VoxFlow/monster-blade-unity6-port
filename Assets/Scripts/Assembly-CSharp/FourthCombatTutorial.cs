public class FourthCombatTutorial : Tutorial
{
	private NBTimer m_endTimer;

	protected override void OnEnable()
	{
		base.OnEnable();
		Messenger.AddListener("battle_init", OnBattleInit);
		Messenger.AddListener("battle_announce", OnBattleAnnounce);
		Messenger.AddListener("battle_intro", OnBattleIntro);
		Messenger.AddListener("battle_end", OnBattleEnd);
		Messenger.AddListener("battle_start_ui", OnBattleStartUI);
		Messenger.AddListener("tutorial_dungeon_end", OnTutorialDungeonEnd);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Messenger.RemoveListener("battle_init", OnBattleInit);
		Messenger.RemoveListener("battle_announce", OnBattleAnnounce);
		Messenger.RemoveListener("battle_intro", OnBattleIntro);
		Messenger.RemoveListener("battle_end", OnBattleEnd);
		Messenger.RemoveListener("battle_start_ui", OnBattleStartUI);
		Messenger.RemoveListener("tutorial_dungeon_end", OnTutorialDungeonEnd);
	}

	protected override void Awake()
	{
		base.Awake();
		m_endTimer = base.gameObject.AddComponent<NBTimer>();
		Dbg.Assert(m_endTimer);
	}

	public override void Start()
	{
		base.Start();
		SingletonComponent<BattleScreenUI>.Instance.Initialize();
		BattleScreenUI instance = SingletonComponent<BattleScreenUI>.Instance;
		if (instance != null)
		{
			instance.EnableAlertText = true;
		}
		SingletonComponent<TutorialManager>.Instance.showDodgeInstructions = true;
	}

	private void OnBattleInit()
	{
		if (!SingletonComponent<DungeonManager>.Instance)
		{
			return;
		}
		BattleStruct currentBattle = SingletonComponent<DungeonManager>.Instance.GetCurrentBattle();
		if (currentBattle != null)
		{
			int battleNumber = currentBattle.battleNumber;
			if (battleNumber == 1)
			{
				TryAdvance("battle_intro");
			}
		}
	}

	private void OnBattleAnnounce()
	{
		DungeonManager instance = SingletonComponent<DungeonManager>.Instance;
		if (!(null != instance))
		{
			return;
		}
		BattleStruct currentBattle = SingletonComponent<DungeonManager>.Instance.GetCurrentBattle();
		if (currentBattle != null)
		{
			switch (currentBattle.battleNumber)
			{
			case 0:
				AnalyticsManager.FirstInstallEvents("tut_raptor");
				break;
			case 1:
				SingletonComponent<TutorialManager>.Instance.showDodgeInstructions = false;
				AnalyticsManager.FirstInstallEvents("tut_landdragon");
				break;
			}
		}
	}

	private void OnBattleStartUI()
	{
		TryAdvance("battle_start_ui");
	}

	private void OnBattleReady()
	{
		int battleNumber = SingletonComponent<DungeonManager>.Instance.GetCurrentBattle().battleNumber;
		if (battleNumber == 1)
		{
			TryAdvance("battle_ready");
		}
	}

	private void OnBattleIntro()
	{
	}

	private void OnBattleEnd()
	{
		switch (SingletonComponent<DungeonManager>.Instance.GetCurrentBattle().battleNumber)
		{
		case 1:
			TryAdvance("battle_end");
			break;
		case 0:
		{
			BattleManager instance = SingletonComponent<BattleManager>.Instance;
			if (null != instance && instance.IsPlayerWin)
			{
				AnalyticsManager.FirstInstallEvents("tut_freebattle4_end");
			}
			break;
		}
		}
	}

	private void OnBattleContinue(int battleNumber)
	{
		switch (battleNumber)
		{
		case 0:
			AnalyticsManager.FirstInstallEvents("tut_battleresult4");
			break;
		}
	}

	private void OnEndTimer()
	{
		TryAdvance("intro_movie_start");
	}

	private void OnTutorialDungeonEnd()
	{
		TryAdvance("dungeon_end");
	}

	private void DoNothing()
	{
		HideSubtitle();
	}

	private void StartBattle()
	{
		HideSubtitle();
		SingletonComponent<BattleManager>.Instance.ChangeState(BattleManager.State.Intro);
	}

	private void ShowSubtitlePauseBattleManager(Action action)
	{
		ShowSubtitle(action);
		AnalyticsManager.FirstInstallEvents("tut_ochatalk");
		SingletonComponent<BattleManager>.Instance.ChangeState(BattleManager.State.None);
	}

	private void BattleEnd(Action action)
	{
		m_endTimer.CountDown(5f, "OnEndTimer");
		SingletonComponent<SoundManager>.Instance.StopMusic(true);
		AnalyticsManager.FirstInstallEvents("tut_death");
		SingletonComponent<BattleManager>.Instance.ChangeState(BattleManager.State.None);
	}

	private void BattleExit()
	{
		SingletonComponent<BattleManager>.Instance.EndLastBattle();
	}

	private void FourthCombatEnd()
	{
		AnalyticsManager.FirstInstallEvents("tut_deathloading");
	}

	private void End()
	{
		HideSubtitle();
		Messenger<State>.Invoke("tutorial_completed", State.Combat3);
	}
}
