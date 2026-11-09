using System.Collections.Generic;
using Models;
using ServiceCalls;
using UnityEngine;

public class LeaderboardManager : SingletonComponent<LeaderboardManager>
{
	public enum MEDAL_RANK
	{
		NONE = 0,
		WOOD = 1,
		BRONZE = 2,
		SILVER = 3,
		GOLD = 4
	}

	public delegate void LbInitCB();

	public Dictionary<int, LeaderPlayer> m_Leaderboard = new Dictionary<int, LeaderPlayer>();

	public LeaderPlayer m_player = new LeaderPlayer(0, string.Empty, 0);

	public Dictionary<int, LeaderPlayer> m_socialLeaderboard = new Dictionary<int, LeaderPlayer>();

	public LeaderPlayer m_socialPlayer = new LeaderPlayer(0, string.Empty, 0);

	public string ExpiryTimeStr = string.Empty;

	public LeaderboardMenuScreen m_LeaderboardMenuScreenRef;

	public LeaderboardRankPanel m_LeaderboardRankPanelRef;

	public bool m_NeedToShowResult;

	public Dictionary<int, LeaderPlayer> m_lastTop3Players = new Dictionary<int, LeaderPlayer>();

	public LeaderPlayer m_lastPlayer = new LeaderPlayer(0, string.Empty, 0);

	public long LastResultTimeStamp;

	public float m_TotalScore;

	public float m_TotalMaxScore;

	private float m_playerColumnPosY;

	private Services m_services;

	public float m_TotalDamage;

	public float m_TotalTakenDamage;

	public float m_TotalTimeScore;

	public int m_highestMonsterLevel = 1;

	public float m_TotoalMonsterHp;

	public float m_maxTimeScore;

	public int m_battleTime;

	public float m_CurrentBattleDamage;

	public float m_CurrentBattleTakenDamage;

	public int m_CurrentPowerMoveCount;

	public int m_CurrentPerfectBlockCount;

	public int m_CurrentPerfectDodgeCount;

	public float m_PlayerColumnPosY
	{
		get
		{
			return m_playerColumnPosY;
		}
		set
		{
			m_playerColumnPosY = value;
			float num = (float)Screen.height / (float)Screen.width;
			if (num - 1.5f < 0.1f && num - 1.5f > -0.1f)
			{
				if (m_PlayerColumnPosY > 0f)
				{
					m_LeaderboardRankPanelRef.ShowFakeColumn(true, true);
				}
				else if (m_PlayerColumnPosY > -270f)
				{
					m_LeaderboardRankPanelRef.ShowFakeColumn(false);
				}
				else
				{
					m_LeaderboardRankPanelRef.ShowFakeColumn(true);
				}
				return;
			}
			float num2 = -300f;
			num2 = ((Screen.height != 1536) ? (-300f) : (-600f));
			if (m_PlayerColumnPosY > 0f)
			{
				m_LeaderboardRankPanelRef.ShowFakeColumn(true, true);
			}
			else if (m_PlayerColumnPosY > num2)
			{
				m_LeaderboardRankPanelRef.ShowFakeColumn(false);
			}
			else
			{
				m_LeaderboardRankPanelRef.ShowFakeColumn(true);
			}
		}
	}

	public long GetRemainingTime()
	{
		if (!string.IsNullOrEmpty(ExpiryTimeStr))
		{
			long result = 0L;
			long.TryParse(ExpiryTimeStr, out result);
			long num = result - TimeUtil.GetUnixEpochTime();
			Dbg.LogEx("Local GetUnixEpochTime() : {0}, ExpiryTime: {1}", Dbg.NBLogType.COMMON_LOG, TimeUtil.GetUnixEpochTime().ToString(), ExpiryTimeStr);
			if (num < 0)
			{
				num = 0L;
			}
			return num;
		}
		return TimeUtil.GetUnixEpochTime();
	}

	private void OnEnable()
	{
		Messenger<DamageData>.AddListener("player_receive_dmg", OnPlayerReceivedDamage);
		Messenger<DamageData>.AddListener("monster_receive_dmg", OnMonsterReceivedDamage);
		Messenger<PowerMove.MoveEnum>.AddListener("power_move", OnPowerMoveSuccess);
		Messenger<Player.DodgeDirection>.AddListener("perfect_dodge_start", OnPerfectDodgeSuccess);
		Messenger.AddListener("perfect_block", OnPerfectBlockSuccess);
	}

	private void OnDisable()
	{
		Messenger<DamageData>.RemoveListener("player_receive_dmg", OnPlayerReceivedDamage);
		Messenger<DamageData>.RemoveListener("monster_receive_dmg", OnMonsterReceivedDamage);
		Messenger<PowerMove.MoveEnum>.RemoveListener("power_move", OnPowerMoveSuccess);
		Messenger<Player.DodgeDirection>.RemoveListener("perfect_dodge_start", OnPerfectDodgeSuccess);
		Messenger.RemoveListener("perfect_block", OnPerfectBlockSuccess);
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void InitManager(CharacterAccountDetails p_accountData)
	{
		m_services = SingletonComponent<Services>.Instance;
		m_player.CharaAccountData = p_accountData;
		m_player.CharaName = p_accountData.name;
		m_player.Score = p_accountData.battleScore;
		m_player.Rank = p_accountData.leaderboardRank;
	}

	public void GetGlobalLeaderboard(int p_offset)
	{
		if (m_services != null)
		{
			m_services.GetGlobal(m_services.NubeeId, m_services.DeviceId, p_offset, OnGetGlobalLeaderboardSuccess, OnGetGlobalLeaderboardFailed);
		}
	}

	private void OnGetGlobalLeaderboardSuccess(Services.ServiceHandle p_handle)
	{
		UIPopupController.Instance.HideLoadingPopup();
		MainMenu.Instance.ShowTopAndBottomBar(false);
		MainMenu.Instance.StartLoadNextScreen(MainMenu.Menu.LeaderBoard);
		MainMenu.Instance.FadeOutFullScreen();
		if (m_LeaderboardMenuScreenRef != null)
		{
			m_LeaderboardMenuScreenRef.ShowRankPanel(true);
		}
		else
		{
			Dbg.LogWarn("m_LeaderboardMenuScreenRef is null");
		}
	}

	private void OnGetGlobalLeaderboardFailed(Services.ServiceHandle p_handle, string p_errMsg)
	{
		Dbg.LogWarn("OnGetGlobalLeaderboardFailed: " + ServiceCallUtils.GetErrorMessage(p_handle));
		UIPopupController.Instance.HideLoadingPopup();
		if (p_handle == null)
		{
			UIPopupController.Instance.ShowServerGenericError();
		}
		else if (p_handle.IsNoInternet())
		{
			UIPopupController.Instance.ShowNoNetMessage();
		}
		else
		{
			UIPopupController.Instance.ShowServerGenericError(null, p_handle.nReturnCode);
		}
	}

	public void GetSocailLeaderboard(int p_offset)
	{
		Dbg.Log("[Info] GetSocailLeaderboard() called");
		if (m_services != null)
		{
			m_services.GetSocial(m_services.NubeeId, m_services.DeviceId, p_offset, OnGetSocialLeaderboardSuccess, OnGetSocialLeaderboardFailed);
		}
	}

	private void OnGetSocialLeaderboardSuccess(Services.ServiceHandle p_handle)
	{
		UIPopupController.Instance.HideLoadingPopup();
		MainMenu.Instance.ShowTopAndBottomBar(false);
		m_LeaderboardMenuScreenRef.ShowRankPanel(false);
	}

	private void OnGetSocialLeaderboardFailed(Services.ServiceHandle p_handle, string p_errMsg)
	{
		Dbg.Log("OnGetSocialLeaderboardFailed error: " + ServiceCallUtils.GetErrorMessage(p_handle));
		UIPopupController.Instance.HideLoadingPopup();
		if (p_handle == null)
		{
			UIPopupController.Instance.ShowServerGenericError();
		}
		else if (p_handle.IsNoInternet())
		{
			UIPopupController.Instance.ShowNoNetMessage();
		}
		else
		{
			UIPopupController.Instance.ShowServerGenericError(null, p_handle.nReturnCode);
		}
	}

	public void SaveScore(int p_score)
	{
		Dbg.Log("[Info] SaveScore() called, score: " + p_score);
		if (m_services != null)
		{
			m_services.SaveScore(m_services.NubeeId, m_services.DeviceId, p_score, OnSaveScoreSuccess, OnSaveScoreFailed);
		}
	}

	private void OnSaveScoreSuccess(Services.ServiceHandle p_handle)
	{
		UIPopupController.Instance.HideLoadingPopup();
	}

	private void OnSaveScoreFailed(Services.ServiceHandle p_handle, string p_errMsg)
	{
		Dbg.LogWarn("OnSaveScoreFailed: " + ServiceCallUtils.GetErrorMessage(p_handle));
		UIPopupController.Instance.HideLoadingPopup();
	}

	public int GetLastRankInLocal()
	{
		int num = 1;
		return MBPrefs.GetInt("Ms:LastRank:" + m_services.NubeeId.ToString(), 1);
	}

	public void SetLastRankInLocal(int p_rank)
	{
		MBPrefs.SetInt("Ms:LastRank:" + m_services.NubeeId.ToString(), p_rank);
		Dbg.LogEx("Set player {0} last rank {1}", Dbg.NBLogType.COMMON_LOG, m_services.NubeeId.ToString(), p_rank.ToString());
	}

	public void ClearDungeonScoreParams()
	{
		m_TotalDamage = 0f;
		m_TotalTakenDamage = 0f;
		m_TotalTimeScore = 0f;
		m_highestMonsterLevel = 1;
		m_TotoalMonsterHp = 0f;
		m_maxTimeScore = 0f;
	}

	public void ClearPreviousBattleParams()
	{
		m_CurrentBattleDamage = 0f;
		m_CurrentBattleTakenDamage = 0f;
		m_CurrentPowerMoveCount = 0;
		m_CurrentPerfectBlockCount = 0;
		m_CurrentPerfectDodgeCount = 0;
	}

	public int CalculatePowerMoveScore(int playerLevel)
	{
		return m_CurrentPowerMoveCount * GLOBAL.GAME_SETTINGS.multiplier_specials * playerLevel;
	}

	public int CalculateSpecialsScore(int monsterLevel)
	{
		int num = m_CurrentPowerMoveCount + m_CurrentPerfectBlockCount + m_CurrentPerfectDodgeCount;
		if (num > 1000)
		{
			Dbg.LogWarn("Impossible, the special count is over 1000! specialCount: " + num);
		}
		return num * GLOBAL.GAME_SETTINGS.multiplier_specials * monsterLevel;
	}

	public float CalculateTimeScore(string p_monsterId, int timeSpend)
	{
		Dbg.Log(string.Format("[Info] Calculate the score for monster id: {0}, total time: {1}", p_monsterId, timeSpend.ToString()));
		m_battleTime = timeSpend;
		m_maxTimeScore = MaxTimeScore(p_monsterId);
		if ((float)timeSpend > m_maxTimeScore)
		{
			return 0f;
		}
		if (timeSpend < 0)
		{
			Dbg.LogWarn("The time spend is minus!! Cheater!, timeSpend : " + timeSpend);
			timeSpend = 0;
		}
		return m_maxTimeScore - (float)timeSpend;
	}

	public void PrintBattleScore()
	{
		Dbg.Log("******************************");
		Dbg.Log("Damage Dealt: " + m_TotalDamage);
		Dbg.Log("Damage Received: " + m_TotalTakenDamage);
		long num = m_battleTime;
		long num2 = num / 3600;
		long num3 = num % 3600 / 60;
		long num4 = num3 % 60;
		Dbg.Log("Time Taken: " + num2 + "h " + num3 + "m " + num4 + "s");
	}

	public float MaxTimeScore(string p_monsterId)
	{
		float num = 0f;
		num = ((!p_monsterId.Contains("_BOSS_")) ? 60f : (p_monsterId.Contains("CITY") ? 240f : ((!p_monsterId.Contains("ACT")) ? 120f : 580f)));
		Dbg.Log("[Info] LeaderboardManager::MaxTimeScore, return " + num);
		return num;
	}

	public int CalculateDungeonScore(int p_playerLevel)
	{
		Dbg.Log(string.Format("[Info] Player level: {0}, highest monster level: {1}", p_playerLevel, m_highestMonsterLevel));
		if (p_playerLevel - m_highestMonsterLevel > 5)
		{
			Dbg.Log("[Info] The player level is too high, socre 0");
			return 0;
		}
		float totalScore = SingletonComponent<LeaderboardManager>.Instance.m_TotalScore;
		return (int)totalScore;
	}

	public string CalculateRank(float p_score, float p_maxScore)
	{
		Dbg.Log("[Info] The total score: " + p_score + " total score max: " + p_maxScore);
		float num = p_score / p_maxScore;
		if (num >= 1f)
		{
			return "AAA";
		}
		if (num >= 0.9f && num < 1f)
		{
			return "AA";
		}
		if (num >= 0.8f && num < 0.9f)
		{
			return "A";
		}
		if (num >= 0.7f && num < 0.8f)
		{
			return "B";
		}
		if (num >= 0.6f && num < 0.7f)
		{
			return "C";
		}
		if (num >= 0.5f && num < 0.6f)
		{
			return "D";
		}
		if (num >= 0.4f && num < 0.5f)
		{
			return "E";
		}
		return "F";
	}

	public MEDAL_RANK CalculateMedalRank(float p_score, float p_maxScore)
	{
		float percentage = p_score / p_maxScore;
		return CalculateMedalRank(percentage);
	}

	public MEDAL_RANK CalculateMedalRank(float percentage)
	{
		Dbg.LogEx("CalculateMedalRank, percentage : {0}", Dbg.NBLogType.COMMON_LOG, percentage.ToString());
		if (percentage >= GLOBAL.GAME_SETTINGS.medal_criteria_gold)
		{
			Dbg.Log("CalculateMedalRank, medal : " + MEDAL_RANK.GOLD);
			return MEDAL_RANK.GOLD;
		}
		if (percentage >= GLOBAL.GAME_SETTINGS.medal_criteria_silver)
		{
			Dbg.Log("CalculateMedalRank, medal : " + MEDAL_RANK.SILVER);
			return MEDAL_RANK.SILVER;
		}
		if (percentage >= GLOBAL.GAME_SETTINGS.medal_criteria_bronze)
		{
			Dbg.Log("CalculateMedalRank, medal : " + MEDAL_RANK.BRONZE);
			return MEDAL_RANK.BRONZE;
		}
		if (percentage >= GLOBAL.GAME_SETTINGS.medal_criteria_wood)
		{
			Dbg.Log("CalculateMedalRank, medal : " + MEDAL_RANK.WOOD);
			return MEDAL_RANK.WOOD;
		}
		Dbg.Log("CalculateMedalRank, medal : " + MEDAL_RANK.NONE);
		return MEDAL_RANK.NONE;
	}

	private void OnPlayerReceivedDamage(DamageData p_damage)
	{
		m_TotalTakenDamage += p_damage.finalDamage;
		m_CurrentBattleTakenDamage += p_damage.finalDamage;
	}

	private void OnMonsterReceivedDamage(DamageData p_damage)
	{
		m_TotalDamage += p_damage.finalDamage;
		if (m_TotalDamage > p_damage.defender.MaxHp)
		{
			m_TotalDamage = p_damage.defender.MaxHp;
			m_TotalDamage += p_damage.finalDamage - (p_damage.defender.MaxHp - p_damage.defender.Hp);
		}
		if (Mathf.Approximately(p_damage.finalDamage, 999999f))
		{
			m_CurrentBattleDamage = 0f;
		}
		else
		{
			m_CurrentBattleDamage = p_damage.finalDamage;
		}
	}

	private void OnPowerMoveSuccess(PowerMove.MoveEnum p_move)
	{
		m_CurrentPowerMoveCount++;
	}

	private void OnPerfectDodgeSuccess(Player.DodgeDirection p_dir)
	{
		m_CurrentPerfectDodgeCount++;
	}

	private void OnPerfectBlockSuccess()
	{
		m_CurrentPerfectBlockCount++;
	}

	public int GetLowestMedalFromDungeon()
	{
		string text = MBPrefs.GetString("Ms:Medals", string.Empty);
		Dbg.LogEx("GetLowestMedalFromDungeon, medals string : {0}", Dbg.NBLogType.COMMON_LOG, text);
		if (text == string.Empty)
		{
			return 0;
		}
		string[] array = text.Split('|');
		if (array.Length < 2)
		{
			return 0;
		}
		int[] array2 = new int[array.Length - 1];
		for (int i = 0; i < array.Length - 1; i++)
		{
			array2[i] = int.Parse(array[i]);
		}
		int num = array2[0];
		int[] array3 = array2;
		foreach (int num2 in array3)
		{
			if (num2 < num)
			{
				num = num2;
			}
		}
		Dbg.LogEx("GetLowestMedalFromDungeon, lowest : {0}", Dbg.NBLogType.COMMON_LOG, num.ToString());
		return num;
	}
}
