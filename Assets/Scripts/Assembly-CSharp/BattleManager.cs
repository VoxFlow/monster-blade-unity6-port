using System;
using System.Collections;
using ServiceCalls;
using UnityEngine;

public class BattleManager : SingletonComponent<BattleManager>
{
	public enum State
	{
		None = -1,
		Init = 0,
		BattleAnouncement = 1,
		Intro = 2,
		Battle = 3,
		BattleEnd = 4,
		Outro = 5,
		PostBattle = 6,
		Tutorial = 7,
		StartNextBattle = 8,
		Exit = 9,
		PostExit = 10,
		Error = 11
	}

	private State m_state = State.None;

	private bool m_playerWin;

	private bool m_playPlayerIntro;

	[SerializeField]
	private SuperAttackSystem m_superAttackPrefab;

	private Player m_player;

	private Monster m_enemy;

	private PlayerController m_playerController;

	private AIController m_enemyAI;

	private GameObject[] m_friends;

	private BattleStruct m_battleData;

	private GameObject m_battleLogic;

	private bool m_battleEnd;

	private Vector3 m_battleReferencePoint = Vector3.zero;

	private float m_battleReferenceDist;

	public bool IsRepeatedTutorialBattle;

	private string m_lastMusicName = string.Empty;

	private GameManager.QualitySetting m_qualitySetting = GameManager.QualitySetting.Mid;

	public static readonly float arenaRadius = 22f;

	public static readonly float squaredArenaRadius = 484f;

	public float m_DamageToMonster;

	public float m_monsterHp;

	public LeaderboardManager.MEDAL_RANK m_medal;

	public float m_battleScore;

	public float m_TimeScore;

	public float m_battleTimeAll;

	public float m_PowerMoveScore;

	public float m_DamageToPlayer;

	public int m_dodoboIgcBattle;

	public string m_tutorialMusic;

	public int RetryCost = 1;

	private BattleCamera m_cameraController;

	public bool m_disableAI;

	public bool m_playerInvincible;

	public bool m_monsterInvincible;

	public float progressBarProgressTargetValue;

	public State state
	{
		get
		{
			return m_state;
		}
	}

	public bool IsPlayerWin
	{
		get
		{
			return m_playerWin;
		}
	}

	public float BattleReferenceDistance
	{
		get
		{
			return m_battleReferenceDist;
		}
		set
		{
			m_battleReferenceDist = Mathf.Abs(value);
		}
	}

	public Vector3 BattleReferencePoint
	{
		get
		{
			return m_battleReferencePoint;
		}
		set
		{
			m_battleReferencePoint = value;
		}
	}

	public void ClearBattleScore()
	{
		m_DamageToMonster = 0f;
		m_monsterHp = 0f;
		m_battleScore = 0f;
		m_TimeScore = 0f;
	}

	public void Awake()
	{
		if (null == SingletonComponent<SuperAttackSystem>.Instance)
		{
			UnityEngine.Object.Instantiate(m_superAttackPrefab);
		}
		m_qualitySetting = GameManager.qualitySetting;
		GameManager.autoDetectQualitySetting = true;
		StageManager instance = SingletonComponent<StageManager>.Instance;
		DungeonManager instance2 = SingletonComponent<DungeonManager>.Instance;
		DungeonDbStruct dungeonDbStruct = instance2.CurrentDungeon;
		if (instance2.BattleType == Battle.Type.DungeonEvent)
		{
			dungeonDbStruct = instance2.CurrentDungeonEvent;
		}
		Dbg.Log("Inializing battle. Dungeon: " + dungeonDbStruct.id + " Number of Fight: " + dungeonDbStruct.battleCount);
		instance.SetStage(dungeonDbStruct.stage);
		MonsterBehaviorDB instance3 = Singleton<MonsterBehaviorDB>.Instance;
		if (instance3 != null)
		{
			Dbg.Log("Finished initializing monster behavior database");
		}
		InitPlayer();
		InitFriend();
		m_cameraController = GameObject.Find("Camera_Root").GetComponent<BattleCamera>();
		m_cameraController.gameObject.SetActiveRecursively(true);
		m_cameraController.transform.parent = m_player.transform;
		SingletonComponent<BattleScreenUI>.Instance.ShowBattleUi(false);
		SingletonComponent<BattleTextDisplay>.Instance.gameObject.SetActiveRecursively(false);
		progressBarProgressTargetValue = MBPrefs.GetFloat("Ms:ItemProgress", 0f);
		SingletonComponent<LeaderboardManager>.Instance.m_TotalScore = 0f;
		SingletonComponent<LeaderboardManager>.Instance.m_TotalMaxScore = 0f;
		Messenger<DamageData>.AddListener("monster_receive_dmg", OnMonsterReceivedDamage);
		Messenger<CutsceneController.CutsceneType>.AddListener(CutsceneMessage.CUTSCENE_START, OnCutsceneStart);
		Messenger<CutsceneController.CutsceneType>.AddListener(CutsceneMessage.CUTSCENE_END, OnCutsceneEnd);
		Messenger<bool>.AddListener("battle_pause", OnPause);
		Messenger.AddListener("retry_battle", OnRetryBattle);
	}

	private void Start()
	{
		if (UIPopupController.Instance != null)
		{
			UIPopupController.Instance.HideFullScreenLoading();
			UIPopupController.Instance.HideLoadingPopup();
		}
		if (SingletonComponent<TutorialManager>.Instance != null && SingletonComponent<TutorialManager>.Instance.enabled && SingletonComponent<TutorialManager>.Instance.IsCompleted(SingletonComponent<TutorialManager>.Instance.currentState))
		{
			Dbg.Log("User played this tutorial before, show the gold and exp 0");
			IsRepeatedTutorialBattle = true;
		}
		else
		{
			IsRepeatedTutorialBattle = false;
		}
		if (SingletonComponent<TutorialManager>.Instance.enabled && SingletonComponent<TutorialManager>.Instance.currentState == Tutorial.State.Combat1)
		{
			m_cameraController.MainCameraNode.GetComponent<Camera>().enabled = false;
		}
		StartBattle();
		StartCoroutine(DoFadeIn());
	}

	private IEnumerator DoFadeIn()
	{
		yield return new MBWaitForSeconds(0.5f);   // PORT (CD-059)
		SingletonComponent<FaderGui>.Instance.FadeIn();
	}

	private new void OnDestroy()
	{
		GameManager.qualitySetting = m_qualitySetting;
		GameManager.autoDetectQualitySetting = false;
		Messenger<DamageData>.RemoveListener("monster_receive_dmg", OnMonsterReceivedDamage);
		Messenger<CutsceneController.CutsceneType>.RemoveListener(CutsceneMessage.CUTSCENE_START, OnCutsceneStart);
		Messenger<CutsceneController.CutsceneType>.RemoveListener(CutsceneMessage.CUTSCENE_END, OnCutsceneEnd);
		Messenger<bool>.RemoveListener("battle_pause", OnPause);
		Messenger.RemoveListener("retry_battle", OnRetryBattle);
		if (SingletonComponent<StageManager>.Instance != null)
		{
			SingletonComponent<StageManager>.Instance.UnloadStage();
		}
	}

	private void InitPlayer()
	{
		m_player = SingletonComponent<StageManager>.Instance.CreatePlayer();
		if (m_player == null)
		{
			OnError("Failed to create player on the scene.");
			return;
		}
		m_player.Init();
		m_playerController = m_player.gameObject.GetComponent<PlayerController>();
	}

	private void InitMonster()
	{
		m_battleData = SingletonComponent<DungeonManager>.Instance.GetCurrentBattle();
		Dbg.Log("Commencing Battle: " + m_battleData.battleNumber);
		Dbg.Log("Monster ID: " + m_battleData.monsterId);
		DbPlayerDataStruct monsterDbData = SingletonComponent<PlayerDataManager>.Instance.GetMonsterDbData(m_battleData.monsterId);
		m_enemy = SingletonComponent<StageManager>.Instance.CreateMonster(monsterDbData);
		if (m_enemy == null)
		{
			OnError("Failed to create monster on the scene.");
			return;
		}
		m_enemy.enabled = true;
		m_enemy.Init();
		m_enemy.SetVisible(false);
		m_enemyAI = m_enemy.gameObject.GetComponent<AIController>();
		if (m_enemy is Dodobo)
		{
			Dodobo dodobo = (Dodobo)m_enemy;
			DbPlayerDataStruct monsterDbData2 = SingletonComponent<PlayerDataManager>.Instance.GetMonsterDbData(m_battleData.monsterId);
			if (monsterDbData2.species == MonsterSpecies.Dodobo)
			{
				dodobo.IsGold = true;
			}
		}
		BattleReferenceDistance = m_enemy.transform.position.z;
		m_battleReferencePoint = m_enemy.transform.position;
	}

	private void InitFriend()
	{
		StageManager instance = SingletonComponent<StageManager>.Instance;
		instance.CreateFriend(0);
		m_friends = instance.FriendObjects;
		if (m_friends[0] == null)
		{
			Dbg.LogWarn("Failed to create allies on the scene. Do you have dummy friend?");
			return;
		}
		BattleScreenUI instance2 = SingletonComponent<BattleScreenUI>.Instance;
		PlayerData friendData = instance.GetFriendData(0);
		GameObject gameObject = GameObject.Find("FriendLabel");
		if (instance2 != null && instance2.FriendLabel != null && friendData != null && gameObject != null)
		{
			gameObject.transform.position += m_friends[0].transform.position;
			gameObject.transform.parent = m_friends[0].transform.FindInHierarchy("Male_ Head");
			instance2.FriendLabel.CharacterName = friendData.Name;
			instance2.FriendLabel.ReferencePoint = gameObject.transform;
			instance2.FriendLabel.UiCam = Camera.main;
		}
	}

	public void DestroyMonster()
	{
		if (m_enemy != null)
		{
			SingletonComponent<StageManager>.Instance.DestroyMonster(m_enemy);
			UnityEngine.Object.Destroy(m_battleLogic);
			m_enemy = null;
		}
	}

	private void ShowEnemyEnterUi()
	{
		if (SingletonComponent<BattleScreenUI>.Instance != null)
		{
			SingletonComponent<BattleScreenUI>.Instance.ShowEnterEnemyUi(true);
			SingletonComponent<BattleScreenUI>.Instance.SetMonsterName(m_enemy.Data.Name);
			SingletonComponent<BattleScreenUI>.Instance.SetMonsterLevel(m_enemy.Data.Level, m_player.Data.Level, m_enemy.Data.m_monsterData.monsterId.Contains("BOSS"));
			SingletonComponent<BattleScreenUI>.Instance.SetMonsterElement(m_enemy.Data.MonsterElement);
			m_enemy.SetVisible(true);
			Messenger.Invoke("battle_start_ui");
		}
	}

	private void PlayIntro()
	{
		Messenger.Invoke("battle_intro");
		if (m_enemy.hasIntroCutscene)
		{
			StartCoroutine(PlayMonsterIntroCutscene());
			return;
		}
		if (!m_playPlayerIntro)
		{
			m_player.PlayIntro();
			m_playPlayerIntro = true;
			m_cameraController.ChangeCamera(BattleCamera.s_battleCameraName);
		}
		else
		{
			m_cameraController.InterpolateCamera(BattleCamera.s_battleCameraName, 2.5f, false);
		}
		if (!m_enemy.PlayIntro())
		{
			m_enemy.Wait();
		}
		ShowEnemyEnterUi();
	}

	public void ResetPlayerAndMonsterTransform()
	{
		m_enemy.transform.position = Vector3.zero;
		m_enemy.transform.rotation = Quaternion.Euler(new Vector3(0f, 180f, 0f));
		m_player.transform.position = new Vector3(0f, 0f, 0f - m_battleReferenceDist);
		m_player.transform.rotation = Quaternion.identity;
		m_battleReferencePoint = Vector3.zero;
	}

	public Vector3 GetPlayerStartPosition()
	{
		Vector3 normalized = (m_player.transform.position - m_battleReferencePoint).normalized;
		return m_battleReferencePoint + normalized * m_battleReferenceDist;
	}

	private void OnError(string p_errorMsg)
	{
		SingletonComponent<BattleScreenUI>.Instance.ShowErrorPopup("ERROR", p_errorMsg);
		ChangeState(State.Error);
		Dbg.LogError(p_errorMsg);
	}

	public void ChangeState(State p_next)
	{
		Dbg.Log(string.Concat("BattleManager: changing from state ", m_state, " to ", p_next));
		switch (m_state)
		{
		case State.Init:
			if (m_friends[0] != null)
			{
				m_friends[0].SetActiveRecursively(false);
			}
			Messenger.Invoke("battle_ready");
			break;
		case State.Intro:
			m_player.Idle();
			break;
		case State.BattleAnouncement:
			SingletonComponent<BattleScreenUI>.Instance.ShowBattleUi(true);
			if (m_enemy.Data.m_monsterData.level > SingletonComponent<LeaderboardManager>.Instance.m_highestMonsterLevel)
			{
				SingletonComponent<LeaderboardManager>.Instance.m_highestMonsterLevel = m_enemy.Data.m_monsterData.level;
			}
			SingletonComponent<LeaderboardManager>.Instance.m_TotoalMonsterHp += m_enemy.Data.Hp;
			break;
		case State.BattleEnd:
			if (m_playerWin)
			{
				SingletonComponent<LeaderboardManager>.Instance.PrintBattleScore();
				float num = SingletonComponent<LeaderboardManager>.Instance.CalculateTimeScore(m_enemy.Data.m_monsterData.monsterId, (int)m_battleTimeAll);
				SingletonComponent<BattleManager>.Instance.m_TimeScore = num;
				SingletonComponent<LeaderboardManager>.Instance.m_TotalTimeScore += num;
				Dbg.Log("[Info] The accum time score: " + SingletonComponent<LeaderboardManager>.Instance.m_TotalTimeScore);
				m_battleTimeAll = 0f;
			}
			break;
		}
		m_state = p_next;
		switch (p_next)
		{
		case State.Error:
			if (m_player != null)
			{
				m_player.gameObject.SetActiveRecursively(false);
			}
			if (m_enemy != null)
			{
				m_enemy.gameObject.SetActiveRecursively(false);
			}
			base.enabled = false;
			break;
		case State.Battle:
			m_battleEnd = false;
			m_player.enabled = true;
			m_player.Stamina = 1f;
			m_enemy.enabled = true;
			if (null != m_playerController)
			{
				m_playerController.enabled = true;
			}
			m_cameraController.ChangeCamera(BattleCamera.s_battleCameraName);
			Messenger.Invoke("battle_start");
			break;
		case State.BattleEnd:
			m_player.ClearAttackList();
			if (m_playerController != null)
			{
				m_playerController.enabled = false;
			}
			if (m_enemyAI != null)
			{
				m_enemyAI.enabled = false;
			}
			m_cameraController.InterpolateCamera(BattleCamera.s_battleCameraName, 1f, false);
			Messenger.Invoke("battle_end");
			if (m_playerWin && SingletonComponent<DungeonManager>.Instance.IsLastBattle())
			{
				SingletonComponent<SoundManager>.Instance.StopMusic(true);
				SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_jingle_victory_1");
			}
			break;
		case State.PostBattle:
		{
			Dbg.Log("Change to PostBattle, time : " + DateTime.Now.ToString("HH:mm:ss.ffffff"));
			CameraFx.StopPlayingEffects();
			if (!m_playerWin)
			{
				SingletonComponent<BattleScreenUI>.Instance.ShowPopup(BattleScreenUI.Popup.Retry);
				break;
			}
			DungeonManager instance = SingletonComponent<DungeonManager>.Instance;
			EndBattleData endBattleData = ProcessEndBattleData();
			string text = MBPrefs.GetString("Ms:Medals", string.Empty);
			string[] array = text.Split('|');
			int[] array2 = new int[array.Length - 1];
			for (int i = 0; i < array.Length - 1; i++)
			{
				array2[i] = int.Parse(array[i]);
			}
			int prevScore = (int)SingletonComponent<LeaderboardManager>.Instance.m_TotalScore;
			UIPopupController.Instance.ShowBattleClearScreen(endBattleData, array2, (int)m_DamageToMonster, (int)SingletonComponent<LeaderboardManager>.Instance.m_CurrentBattleTakenDamage, SingletonComponent<LeaderboardManager>.Instance.m_battleTime, SingletonComponent<LeaderboardManager>.Instance.CalculatePowerMoveScore(m_enemy.Data.Level), (int)SingletonComponent<LeaderboardManager>.Instance.m_TotalScore, prevScore, SingletonComponent<TutorialManager>.Instance.enabled || endBattleData.totalNumBattle == endBattleData.numBattle);
			SingletonComponent<DungeonManager>.Instance.m_dodoboIgcDungeon += m_dodoboIgcBattle;
			Dbg.LogEx("Dodobo Currency in battle:{0} ,In Dungeon: {1}", Dbg.NBLogType.COMMON_LOG, m_dodoboIgcBattle.ToString(), SingletonComponent<DungeonManager>.Instance.m_dodoboIgcDungeon.ToString());
			m_dodoboIgcBattle = 0;
			SingletonComponent<DungeonManager>.Instance.m_dungeonCompScore = SingletonComponent<LeaderboardManager>.Instance.CalculateDungeonScore(SingletonComponent<PlayerDataManager>.Instance.GetPlayerData().Level);
			int gainedXP = SingletonComponent<DungeonManager>.Instance.GetGainedXP();
			int originalExp = SingletonComponent<DungeonManager>.Instance.m_originalExp;
			Dbg.Log("XPGained: " + gainedXP + " and currXP: " + originalExp);
			DestroyMonster();
			m_enemy = null;
			break;
		}
		case State.Init:
			ClearBattleScore();
			SingletonComponent<LeaderboardManager>.Instance.ClearPreviousBattleParams();
			m_playerWin = false;
			m_dodoboIgcBattle = 0;
			m_tutorialMusic = null;
			Messenger.Invoke("battle_init");
			if (SingletonComponent<TutorialManager>.Instance.enabled && SingletonComponent<TutorialManager>.Instance.currentState == Tutorial.State.Combat1)
			{
				SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_e_tutorial_ambience");
			}
			else if (m_tutorialMusic != null)
			{
				if (m_lastMusicName != m_tutorialMusic)
				{
					SingletonComponent<SoundManager>.Instance.PlayPlaylist(m_tutorialMusic);
				}
				m_lastMusicName = m_tutorialMusic;
			}
			else
			{
				PlayMusicByMonster();
			}
			break;
		case State.Intro:
		{
			bool flag = false;
			if (SingletonComponent<TutorialManager>.Instance.enabled && SingletonComponent<TutorialManager>.Instance.currentState == Tutorial.State.Combat1 && !flag)
			{
				ChangeState(State.Tutorial);
			}
			else
			{
				PlayIntro();
			}
			MBPrefs.SetString("Ms:LastVisitDungeon", SingletonComponent<DungeonManager>.Instance.CurrentDungeon.element.ToString());
			MBPrefs.Save();
			break;
		}
		case State.BattleAnouncement:
		{
			m_monsterHp = m_enemy.Data.Hp;
			DungeonDbStruct dungeonDbStruct = SingletonComponent<DungeonManager>.Instance.CurrentDungeon;
			if (SingletonComponent<DungeonManager>.Instance.BattleType == Battle.Type.DungeonEvent)
			{
				dungeonDbStruct = SingletonComponent<DungeonManager>.Instance.CurrentDungeonEvent;
			}
			SingletonComponent<BattleTextDisplay>.Instance.Show(m_battleData.battleNumber + 1, dungeonDbStruct.battleCount, m_battleData.isBoss, OnFinishAnnounce);
			Messenger.Invoke("battle_announce");
			break;
		}
		case State.StartNextBattle:
		{
			Vector3 playerStartPosition = GetPlayerStartPosition();
			if ((playerStartPosition - m_player.transform.position).magnitude >= 0.01f)
			{
				m_player.RunTo(playerStartPosition, 10f, OnPlayerReachMonster);
			}
			else
			{
				ChangeState(State.BattleAnouncement);
			}
			break;
		}
		case State.Tutorial:
			if (SingletonComponent<TutorialManager>.Instance.currentState == Tutorial.State.Combat1)
			{
				AnalyticsManager.FirstInstallEvents("tut_ochastart");
				StartCoroutine(SingletonComponent<CutsceneController>.Instance.PlayCutscene(CutsceneController.CutsceneType.TUTORIAL1));
			}
			break;
		case State.Exit:
			if (m_player != null)
			{
				if (m_player.Data.Hp != 0f)
				{
					m_player.AnimPlayer.Play("Idle");
				}
				if (m_playerController != null)
				{
					m_playerController.enabled = false;
				}
			}
			if (m_enemy != null)
			{
				if (m_enemy.Data.Hp != 0f)
				{
					m_enemy.AnimPlayer.Play("Idle_01");
				}
				if (m_enemyAI != null)
				{
					m_enemyAI.enabled = false;
				}
			}
			Messenger.Invoke("battle_exit");
			break;
		case State.Outro:
		case State.PostExit:
			break;
		}
	}

	public EndBattleData ProcessEndBattleData()
	{
		m_DamageToMonster += m_enemy.Data.MaxHp;
		m_battleScore = CalculateBattleScore(m_player.Data.Level, m_enemy.Data.Level);
		SingletonComponent<LeaderboardManager>.Instance.m_TotalScore += m_battleScore;
		Dbg.Log("[Info] The battle score: " + m_battleScore + " ,current total score: " + SingletonComponent<LeaderboardManager>.Instance.m_TotalScore);
		float battleMaxScore = GetBattleMaxScore(m_enemy.Data.Level);
		SingletonComponent<LeaderboardManager>.Instance.m_TotalMaxScore += battleMaxScore;
		float num = m_battleScore / battleMaxScore;
		string text = MBPrefs.GetString("Ms:Medals", string.Empty);
		m_medal = SingletonComponent<LeaderboardManager>.Instance.CalculateMedalRank(num);
		string text2 = text;
		int medal = (int)m_medal;
		text = text2 + medal + "|";
		MBPrefs.SetString("Ms:Medals", text);
		Dbg.LogEx("Set medals in local : {0}", Dbg.NBLogType.COMMON_LOG, text);
		Dbg.Log("[Info] Get rating: " + num);
		string text3 = MBPrefs.GetString("Ms:BattleScores", string.Empty);
		MBPrefs.SetString("Ms:BattleScores", text3 + num + "|");
		EndBattleData endBattleData = new EndBattleData();
		endBattleData.monsterLevel = m_enemy.Data.Level;
		endBattleData.playerLevel = m_player.Data.Level;
		endBattleData.score = (int)m_battleScore;
		endBattleData.scorePerc = num;
		DungeonManager instance = SingletonComponent<DungeonManager>.Instance;
		if (instance != null)
		{
			if (instance.BattleType == Battle.Type.DungeonEvent)
			{
				endBattleData.totalNumBattle = instance.CurrentDungeonEvent.battleCount;
			}
			else
			{
				endBattleData.totalNumBattle = instance.CurrentDungeon.battleCount;
			}
			endBattleData.numBattle = instance.GetCurrentBattleCounter() + 1;
		}
		else
		{
			Dbg.Assert(false, "something screwed up, DungeonManager.Instance MIA");
			endBattleData.totalNumBattle = 0;
		}
		return endBattleData;
	}

	public void EndLastBattle()
	{
		ChangeState(State.Exit);
	}

	private void Update()
	{
		if (!SingletonComponent<CutsceneController>.Instance.Playing)
		{
			switch (m_state)
			{
			case State.Battle:
				UpdateBattle();
				break;
			case State.BattleEnd:
				UpdateBattleEnd();
				break;
			case State.PostBattle:
				UpdatePostBattle();
				break;
			case State.Init:
				UpdateInit();
				break;
			case State.Tutorial:
				UpdateTutorial();
				break;
			case State.Intro:
				UpdateIntro();
				break;
			case State.Exit:
				UpdateExit();
				break;
			case State.BattleAnouncement:
			case State.Outro:
			case State.StartNextBattle:
				break;
			}
		}
	}

	private void LateUpdate()
	{
		if (m_state == State.Battle)
		{
			if (m_battleEnd)
			{
				ChangeState(State.BattleEnd);
			}
			return;
		}
		if (m_playerController != null && m_playerController.enabled)
		{
			m_playerController.enabled = false;
		}
		if (m_enemyAI != null && m_enemyAI.enabled)
		{
			m_enemyAI.enabled = false;
		}
	}

	private void UpdateTutorial()
	{
	}

	private void UpdateInit()
	{
		bool isReady = m_player.IsReady;
		if (isReady & m_enemy.IsReady)
		{
			ChangeState(State.Intro);
		}
	}

	private void UpdateIntro()
	{
		if (m_player.CurrentState.Equals(Player.State.Idle) && m_enemy.CurrentState.Equals(Monster.State.Wait) && SingletonComponent<BattleScreenUI>.Instance.UserPressedOnMonster())
		{
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_event_notification_new");
			SingletonComponent<BattleScreenUI>.Instance.ResetUserPressedOnMonsterFlag();
			SingletonComponent<BattleScreenUI>.Instance.ShowEnterEnemyUi(false);
			ChangeState(State.BattleAnouncement);
		}
	}

	private void UpdateBattle()
	{
		if (SingletonComponent<CutsceneController>.Instance.Playing)
		{
			return;
		}
		if (m_player.Data.Hp == 0f && m_enemy.Data.Hp == 0f)
		{
			m_player.Data.Hp = 1f;
		}
		m_battleTimeAll += Time.deltaTime;
		if (m_enemy.Data.Hp == 0f)
		{
			if (m_enemy.hasDeathCutscene)
			{
				StartCoroutine(PlayMonsterDeadCutscene());
			}
			else
			{
				DungeonManager instance = SingletonComponent<DungeonManager>.Instance;
				if (instance != null && instance.IsLastBattle())
				{
					StartCoroutine(m_player.PlayWinningAnim());
				}
				else if (m_player.IsBlocking())
				{
					m_player.IsReady = true;
					m_player.Idle();
				}
				m_enemy.AnimPlayer.enabled = true;
				m_enemy.AnimPlayer.Play("Death");
				m_enemy.ChangeState(Monster.State.Death);
			}
			m_battleEnd = true;
			m_playerWin = true;
		}
		else
		{
			if (m_player.Data.Hp != 0f)
			{
				return;
			}
			m_playerWin = false;
			if (m_player.DeadCutscene != CutsceneController.CutsceneType.NONE)
			{
				StartCoroutine(PlayPlayerDeadCutscene());
				m_battleEnd = true;
			}
			else if (m_player.CurrentState != Player.State.Death && m_player.CurrentState != Player.State.NoMore)
			{
				if (!m_player.IsMoving() || m_player.AnimPlayer.CurrentAnimName == "Run")
				{
					m_player.ChangeState(Player.State.Death);
				}
			}
			else
			{
				m_battleEnd = true;
			}
		}
	}

	private void UpdateBattleEnd()
	{
		bool flag = m_playerWin && (m_enemy == null || m_enemy.CurrentState == Monster.State.NoMore);
		flag |= !m_playerWin && m_player.CurrentState == Player.State.NoMore;
		if (flag & !SingletonComponent<BattleScreenUI>.Instance.IsPostBattleTextShowing())
		{
			Dbg.Log("ChangeState to PostBattle, time : " + DateTime.Now.ToString("HH:mm:ss.ffffff"));
			ChangeState(State.PostBattle);
		}
	}

	private void UpdatePostBattle()
	{
	}

	private void UpdateExit()
	{
		UIPopupController.Instance.HideLoadingPopup();
		DungeonManager instance = SingletonComponent<DungeonManager>.Instance;
		if (instance == null)
		{
			return;
		}
		UIPopupController.Instance.ShowLoadingPopupNetWait();
		ChangeState(State.PostExit);
		instance.EndDungeon(m_playerWin, OnEndDungeonSuccess, OnEndDungeonFail);
		if (m_battleData.dungeonRef == "TUTORIAL_3")
		{
			Dbg.Log("It's the last tutorial dungeon, then we should show the daily bonus popup");
			if (SingletonComponent<DailyManager>.Instance != null)
			{
				SingletonComponent<DailyManager>.Instance.NeedToShowDialyBonusAfterTutorial = true;
				SingletonComponent<DailyManager>.Instance.DailyBonus();
			}
		}
	}

	public void StartBattle()
	{
		InitMonster();
		m_cameraController.InitForCurrentBattle();
		InitBattleLogic();
		ResetPlayerAndMonsterTransform();
		ChangeState(State.Init);
	}

	private void InitBattleLogic()
	{
		UnityEngine.Object obj = null;
		obj = ((m_enemy is Bull || m_enemy is BullBoss) ? Resources.Load("Prefabs/BattleView/BullBattleLogic_Prefab") : ((m_enemy is SharkBoss) ? Resources.Load("Prefabs/BattleView/SharkBattleLogic_Prefab") : ((m_enemy is Wolf) ? Resources.Load("Prefabs/BattleView/WolfBattleLogic_Prefab") : ((m_enemy is LandDragon) ? Resources.Load("Prefabs/BattleView/LandDragonBattleLogic_Prefab") : ((m_enemy is StoneDragon) ? Resources.Load("Prefabs/BattleView/StoneDragonBattleLogic_Prefab") : ((m_enemy is Hydra) ? Resources.Load("Prefabs/BattleView/HydraBattleLogic_Prefab") : ((m_enemy is WingDragon) ? Resources.Load("Prefabs/BattleView/WingDragonBattleLogic_Prefab") : ((SingletonComponent<TutorialManager>.Instance.enabled && SingletonComponent<TutorialManager>.Instance.currentState == Tutorial.State.Combat1) ? Resources.Load("Tutorial/Prefabs/FirstCombatTutorialLogic_Prefab") : ((SingletonComponent<TutorialManager>.Instance.enabled && SingletonComponent<TutorialManager>.Instance.currentState == Tutorial.State.Combat2) ? Resources.Load("Tutorial/Prefabs/SecondCombatTutorialLogic_Prefab") : ((!(m_enemy is EpicDragon)) ? Resources.Load("Prefabs/BattleView/BattleLogic_Prefab") : Resources.Load("Prefabs/BattleView/EpicDragonBattleLogic_Prefab")))))))))));
		if (obj != null)
		{
			m_battleLogic = UnityEngine.Object.Instantiate(obj) as GameObject;
			m_battleLogic.name = "BattleLogic";
		}
	}

	private IEnumerator PlayReviveAnim()
	{
		yield return StartCoroutine(m_player.Revive());
		ChangeState(State.StartNextBattle);
	}

	private IEnumerator PlayMonsterIntroCutscene()
	{
		Dbg.Log("PlayMonsterIntroCutscene");
		CutsceneController cutsceneController = SingletonComponent<CutsceneController>.Instance;
		if (!(cutsceneController == null))
		{
			CutsceneController.CutsceneType cutsceneType = CutsceneController.CutsceneType.NONE;
			switch (m_enemy.Data.m_monsterData.species)
			{
			case MonsterSpecies.Dodobo:
			case MonsterSpecies.Bobodo:
				cutsceneType = CutsceneController.CutsceneType.INTRO_DODOBO;
				break;
			case MonsterSpecies.LandDragon:
				cutsceneType = CutsceneController.CutsceneType.INTRO_LANDDRAGON;
				break;
			case MonsterSpecies.StoneDragon:
				cutsceneType = CutsceneController.CutsceneType.INTRO_STONEDRAGON;
				break;
			case MonsterSpecies.WingChicken:
				cutsceneType = CutsceneController.CutsceneType.INTRO_WINGCHICKEN;
				break;
			case MonsterSpecies.Hydra:
				cutsceneType = CutsceneController.CutsceneType.INTRO_HYDRA;
				break;
			case MonsterSpecies.Wolf:
				cutsceneType = CutsceneController.CutsceneType.INTRO_WOLF;
				break;
			case MonsterSpecies.Bull:
			case MonsterSpecies.Rhino:
				cutsceneType = CutsceneController.CutsceneType.INTRO_CUTSCENE_BEGIN;
				break;
			case MonsterSpecies.WingSerpent:
			case MonsterSpecies.MantaRay:
				cutsceneType = CutsceneController.CutsceneType.INTRO_WINGSERPENT;
				break;
			case MonsterSpecies.Raptor:
				cutsceneType = CutsceneController.CutsceneType.INTRO_RAPTOR;
				break;
			case MonsterSpecies.Tricep:
				cutsceneType = CutsceneController.CutsceneType.INTRO_TRICEP;
				break;
			case MonsterSpecies.Shark:
				cutsceneType = CutsceneController.CutsceneType.INTRO_SHARK;
				break;
			case MonsterSpecies.Lizard:
				cutsceneType = CutsceneController.CutsceneType.INTRO_LIZARD;
				break;
			case MonsterSpecies.EpicDragon:
				cutsceneType = CutsceneController.CutsceneType.INTRO_EPICDRAGON;
				break;
			}
			m_player.Idle();
			yield return StartCoroutine(SingletonComponent<CutsceneController>.Instance.PlayCutscene(cutsceneType));
			Time.timeScale = 1f;
			m_enemy.Wait(0f);
			m_cameraController.ChangeCamera(BattleCamera.s_battleCameraName);
			ShowEnemyEnterUi();
		}
	}

	private IEnumerator PlayMonsterDeadCutscene()
	{
		Dbg.Log("PlayMonsterDeadCutscene");
		Waiter waiter = new Waiter();
		float waitTime = 0.8f;
		SingletonComponent<SlowMotionSystem>.Instance.SlowTheTime(0.4f, waitTime);
		yield return StartCoroutine(waiter.WaitUntil(waitTime));
		CutsceneController.CutsceneType cutsceneType = CutsceneController.CutsceneType.NONE;
		switch (m_enemy.Data.m_monsterData.species)
		{
		case MonsterSpecies.Bull:
			cutsceneType = CutsceneController.CutsceneType.DEATH_CUTSCENE_BEGIN;
			break;
		case MonsterSpecies.Tricep:
			cutsceneType = CutsceneController.CutsceneType.DEATH_TRICEP;
			break;
		case MonsterSpecies.Wolf:
			cutsceneType = CutsceneController.CutsceneType.DEATH_WOLF;
			break;
		case MonsterSpecies.Hydra:
			cutsceneType = CutsceneController.CutsceneType.DEATH_HYDRA;
			break;
		case MonsterSpecies.LandDragon:
			cutsceneType = CutsceneController.CutsceneType.DEATH_LANDDRAGON;
			break;
		case MonsterSpecies.WingChicken:
			cutsceneType = CutsceneController.CutsceneType.DEATH_WINGCHICKEN;
			break;
		case MonsterSpecies.Raptor:
			cutsceneType = CutsceneController.CutsceneType.DEATH_RAPTOR;
			break;
		case MonsterSpecies.WingSerpent:
			cutsceneType = CutsceneController.CutsceneType.DEATH_WINGSERPENT;
			break;
		case MonsterSpecies.StoneDragon:
			cutsceneType = CutsceneController.CutsceneType.DEATH_STONEDRAGON;
			break;
		case MonsterSpecies.Shark:
			cutsceneType = CutsceneController.CutsceneType.DEATH_SHARK;
			break;
		case MonsterSpecies.Rhino:
			cutsceneType = CutsceneController.CutsceneType.DEATH_RHINO;
			break;
		case MonsterSpecies.Lizard:
			cutsceneType = CutsceneController.CutsceneType.DEATH_LIZARD;
			break;
		case MonsterSpecies.MantaRay:
			cutsceneType = CutsceneController.CutsceneType.DEATH_MANTA_RAY;
			break;
		case MonsterSpecies.EpicDragon:
			cutsceneType = CutsceneController.CutsceneType.DEATH_EPICDRAGON;
			break;
		}
		if (cutsceneType != CutsceneController.CutsceneType.NONE)
		{
			yield return StartCoroutine(SingletonComponent<CutsceneController>.Instance.PlayCutscene(cutsceneType));
		}
		Time.timeScale = 1f;
		m_player.transform.position = Vector3.zero;
		m_player.transform.rotation = Quaternion.identity;
		m_enemy.transform.position.Set(0f, 0f, BattleReferenceDistance);
		m_enemy.transform.rotation.SetLookRotation(-Vector3.forward);
		ResetPlayerAndMonsterTransform();
		m_enemy.ChangeState(Monster.State.Death);
	}

	private IEnumerator PlayPlayerDeadCutscene()
	{
		Dbg.Log("PlayPlayerDeadCutscene");
		Waiter waiter = new Waiter();
		float waitTime = 0.4f;
		SingletonComponent<SlowMotionSystem>.Instance.SlowTheTime(0.4f, waitTime);
		yield return StartCoroutine(waiter.WaitUntil(waitTime));
		yield return StartCoroutine(SingletonComponent<CutsceneController>.Instance.PlayCutscene(m_player.DeadCutscene));
		Time.timeScale = 1f;
		m_player.transform.position = Vector3.zero;
		m_player.transform.rotation = Quaternion.identity;
		m_enemy.transform.position.Set(0f, 0f, BattleReferenceDistance);
		m_enemy.transform.rotation.SetLookRotation(-Vector3.forward);
		ResetPlayerAndMonsterTransform();
		m_player.ChangeState(Player.State.Death);
		m_player.AnimPlayer.CurrentAnim.time = m_player.AnimPlayer.CurrentAnim.length;
	}

	private void OnMonsterReceivedDamage(DamageData p_damage)
	{
		if (Mathf.Approximately(p_damage.finalDamage, 999999f))
		{
			m_DamageToMonster = 0f;
		}
		else
		{
			m_DamageToMonster = p_damage.finalDamage;
		}
	}

	private void OnPowerMoveSuccess()
	{
		m_PowerMoveScore += GLOBAL.GAME_SETTINGS.power_move_score_multiplier;
	}

	public void OnSlomoEnded()
	{
		Time.timeScale = 1f;
	}

	public void OnVulnerableTimeEnded()
	{
	}

	private void OnPause(bool p_flag)
	{
	}

	private void OnCutsceneStart(CutsceneController.CutsceneType p_type)
	{
		ResetPlayerAndMonsterTransform();
	}

	private void OnCutsceneEnd(CutsceneController.CutsceneType p_type)
	{
		ResetPlayerAndMonsterTransform();
		if (p_type == CutsceneController.CutsceneType.TUTORIAL1)
		{
			m_cameraController.MainCameraNode.GetComponent<Camera>().enabled = true;
		}
	}

	private void OnEndDungeonSuccess(Services.ServiceHandle p_handle = null)
	{
		UIPopupController.Instance.HideLoadingPopup();
		bool flag = false;
		SaveData.MsData gameData = SingletonComponent<SaveData>.Instance.GameData;
		DungeonManager instance = SingletonComponent<DungeonManager>.Instance;
		if (instance.BattleType == Battle.Type.Dungeon)
		{
			int result = 0;
			if (int.TryParse(instance.CurrentDungeon.act, out result))
			{
				if (gameData.LastAct < result)
				{
					gameData.LastAct = result;
					flag = true;
				}
				if (instance.CurrentDungeon.id == "DUNGEON_171")
				{
					gameData.LastAct = result + 1;
					flag = true;
				}
			}
			if (instance.HasDungeonComplete())
			{
				int p_outResult = 0;
				string p_value = instance.CurrentDungeon.id.Replace("DUNGEON_", string.Empty);
				if (NBGeneric.TryParse<int>(p_value, out p_outResult))
				{
					int num = p_outResult % 3;
					if (p_outResult > 0 && num == 0)
					{
						Dbg.Log("[Info] The dungeon index is power of 3, it's the last dungeon of a City");
						int num2 = p_outResult / 3;
						if (!gameData.IsCityDone(num2 - 1))
						{
							gameData.SetIsCityDone(num2 - 1, true);
							flag = true;
						}
					}
				}
			}
			else
			{
				Dbg.Log("Player lose or exit the dungeon, no need to update the achievement progress");
			}
		}
		else if (instance.BattleType != Battle.Type.DungeonEvent)
		{
		}
		if (flag)
		{
			gameData.SaveAchievementData();
		}
		GameManager instance2 = SingletonComponent<GameManager>.Instance;
		TutorialManager instance3 = SingletonComponent<TutorialManager>.Instance;
		if ((bool)instance3 && instance3.enabled)
		{
			Messenger.Invoke("tutorial_dungeon_end");
		}
		else if (instance2 != null)
		{
			instance2.LoadScene("MainMenuInit", true, false);
		}
	}

	private void OnEndDungeonFail(Services.ServiceHandle p_handle, string p_error)
	{
		Dbg.LogWarn("Failed to end dungeon: " + ServiceCallUtils.GetErrorMessage(p_handle));
		UIPopupController.Instance.HideLoadingPopup();
		if (p_handle == null)
		{
			UIPopupController.Instance.ShowMediumPopupMSG("common_error", "server_generic_error", EndLastBattle, OnIgnoreEndDungeon);
		}
		else if (p_handle.result == Services.ConnectionResults.Timeout)
		{
			UIPopupController.Instance.ShowTimeoutForceRetry(EndLastBattle);
		}
		else if (p_handle.IsNoInternet())
		{
			UIPopupController.Instance.ShowNoNetForceRetry(EndLastBattle);
		}
		else
		{
			UIPopupController.Instance.ShowMediumPopupMSG("common_error", "common_prompt_unableprogress", EndLastBattle, OnIgnoreEndDungeon);
		}
	}

	private void OnRetryBattle()
	{
		StartCoroutine(PlayReviveAnim());
		PlayMusicByMonster();
	}

	private void OnIgnoreEndDungeon()
	{
		SingletonComponent<GameManager>.Instance.LoadScene("MainMenuInit", true, false);
	}

	private void OnFinishAnnounce()
	{
		ChangeState(State.Battle);
	}

	private void OnPlayerReachMonster()
	{
		ChangeState(State.BattleAnouncement);
	}

	private void OnCameraFinishedInterpolate()
	{
		State state = m_state;
		if (state == State.Intro)
		{
			PlayIntro();
		}
	}

	public float CalculateBattleScore(int p_playerLevel, int p_monsterLevel)
	{
		Dbg.Log(string.Format("[Info] Player level: {0}, monster level: {1}, player max hp: {2}, player hp: {3}", p_playerLevel, p_monsterLevel, m_player.Data.MaxHp, m_player.Data.Hp));
		float num = SingletonComponent<LeaderboardManager>.Instance.m_CurrentBattleTakenDamage;
		if (num < 0f)
		{
			num = 0f;
		}
		Dbg.Log("[Info] ATK : " + m_DamageToMonster + ", DEF: " + num + ", TIME score: " + m_TimeScore);
		if (p_playerLevel - p_monsterLevel > 5)
		{
			Dbg.Log("[Info] The player level is too high, socre 0");
			return 0f;
		}
		float num2 = m_DamageToMonster - num + m_TimeScore;
		if (num2 < 0f)
		{
			return 0f;
		}
		return num2 + (float)SingletonComponent<LeaderboardManager>.Instance.CalculateSpecialsScore(p_monsterLevel);
	}

	public float GetBattleMaxScore(int monsterLevel = 1)
	{
		float num = m_monsterHp + SingletonComponent<LeaderboardManager>.Instance.MaxTimeScore(m_enemy.Data.m_monsterData.monsterId);
		int num2 = Mathf.CeilToInt((float)monsterLevel * GLOBAL.GAME_SETTINGS.expected_specials_ratio);
		num += (float)(num2 * GLOBAL.GAME_SETTINGS.multiplier_specials * monsterLevel);
		Dbg.Log(string.Format("[Info] Max score of monster {0} is : {1}", m_enemy.Data.m_monsterData.monsterId, num.ToString()));
		return num;
	}

	private void PlayMusicByMonster()
	{
		DbPlayerDataStruct monsterDbData = SingletonComponent<PlayerDataManager>.Instance.GetMonsterDbData(m_battleData.monsterId);
		string bgm = Singleton<MonsterFactory>.Instance.GetBgm(monsterDbData.name);
		if (!(bgm == m_lastMusicName))
		{
			SingletonComponent<SoundManager>.Instance.PlayPlaylist(bgm);
			m_lastMusicName = bgm;
		}
	}
}
