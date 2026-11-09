using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Models;
using ServiceCalls;
using UnityEngine;

public class DungeonManager : SingletonComponent<DungeonManager>
{
	public class DungeonStruct
	{
		public enum RankType
		{
			AAA = 0,
			AA = 1,
			A = 2,
			B = 3,
			C = 4,
			D = 5,
			E = 6,
			F = 7
		}

		public enum Medal
		{
			NONE = 0,
			WOOD = 1,
			BRONZE = 2,
			SILVER = 3,
			GOLD = 4
		}

		public bool visited;

		public bool cleared;

		public RankType rank = RankType.F;

		public Medal medal;

		public int number = 1;

		public DungeonDbStruct dungeon;
	}

	public class CityStruct
	{
		public bool visited;

		public bool cleared;

		public CityDbStruct city;

		public int minLevel;
	}

	public delegate void AsyncCallback(Services.ServiceHandle p_handle);

	public SetInformation m_demoSetInfo;

	private DungeonDbStruct m_currentDungeon;

	private DungeonDbStruct m_nextDungeon;

	private CityDbStruct m_currentCity;

	private DungeonEventStruct m_currentEventDungeon;

	private DungeonEventStruct m_nextEventDungeon;

	private int m_currentEventDifficulty;

	private int m_battleCounter;

	private DungeonDbStruct m_latestUnlockedDungeon;

	private List<CityDbStruct> m_citiesDb = new List<CityDbStruct>();

	private List<DungeonDbStruct> m_dungeonsDb = new List<DungeonDbStruct>();

	private List<DungeonEventStruct> m_dungeonEvents = new List<DungeonEventStruct>();

	private List<BattleStruct> m_battles = new List<BattleStruct>();

	private List<CityStruct> m_citiesStack = new List<CityStruct>();

	private List<DungeonStruct> m_dungeonStack = new List<DungeonStruct>();

	private AsyncCallback m_successRefreshCb;

	private AsyncCallback m_failRefreshCb;

	private AsyncCallback m_successGetEventsCb;

	private AsyncCallback m_failGetEventsCb;

	private Battle.Type m_type;

	private int m_numberOfCitiesToRefresh;

	private int m_unlockedDungeonsSuccessCount;

	private string m_dungeonKey = string.Empty;

	public string m_kingBossId = string.Empty;

	public int m_kingBossIndex = -1;

	public bool m_isKingBossDefeated;

	private MonsterSpecies m_previousSpecies = MonsterSpecies.Unknown;

	private string mainEventID = string.Empty;

	private uint m_sessionRequestID;

	private bool m_hasLevelUp;

	private bool m_hasDungeonComplete;

	private bool m_showDungeonResult;

	private bool m_cityUnlocked;

	private bool m_dungeonUnlocked;

	private bool m_eventCategoryUnlocked;

	private bool m_dungeonEventUnlocked;

	private bool m_petEventUnlocked;

	private int m_expGained;

	private int m_ingameGained;

	public int m_originalExp;

	public int m_dungeonCompScore;

	public int m_dodoboIgcDungeon;

	private bool m_isFromDungeon;

	public Dictionary<string, string> m_BossIdVsBossIcon = new Dictionary<string, string>();

	private string[] m_bossIcons = new string[18]
	{
		"Earth_wingchicken_boss", "Lightning_lizard_boss", "Water_hydra_boss", "Metal_mantaray_boss", "Fire_wolf_boss", "Earth_tricep_boss", "Water_shark_boss", "Metal_stonedragon_boss", "Fire_rhino_boss", "Lightning_landdragon_boss",
		"Fire_wolf_boss", "Dungeon_dodobo", "Dungeon_bodobo", "Fire_wolf_boss", "Water_hydra_boss", "Metal_stonedragon_boss", "Lightning_landdragon_boss", "Metal_epicdragon_boss"
	};

	private string[] m_bossIds = new string[18]
	{
		"EARTH_WINGCHICKEN_BOSS", "LIGHTNING_LIZARD_BOSS", "WATER_HYDRA_BOSS", "METAL_MANTARAY_BOSS", "FIRE_WOLF_BOSS", "EARTH_TRICEP_BOSS", "WATER_SHARK_BOSS", "METAL_STONEDRAGON_BOSS", "FIRE_RHINO_BOSS", "LIGHTNING_LANDDRAGON_BOSS",
		"FIRE_WOLF_BOSS_DUMMY_", "METAL_DODOBO", "METAL_BOBODO", "FIRE_WOLF_EVENT_BOSS", "WATER_HYDRA_EVENT_BOSS", "METAL_STONEDRAGON_EVENT_BOSS", "LIGHTNING_LANDDRAGON_EVENT_BOSS", "METAL_EPICDRAGON_EVENT_BOSS"
	};

	public PlayerData m_oldData;

	private List<ServerInventoryItem> m_chestRewards = new List<ServerInventoryItem>();

	private Services.ServiceHandle.ServiceCompleteCallback m_startDungeonSuccessCb;

	private Services.ServiceHandle.ServiceErrorCallback m_startDungeonFailCb;

	private Services.ServiceHandle.ServiceCompleteCallback m_endDungeonSuccessCb;

	private Services.ServiceHandle.ServiceErrorCallback m_endDungeonFailCb;

	private bool isSendingDungeonEnd;

	public int NumOfMonsterDefeated { get; set; }

	public Battle.Type BattleType
	{
		get
		{
			return m_type;
		}
		set
		{
			m_type = value;
		}
	}

	public string DungeonKey
	{
		get
		{
			return m_dungeonKey;
		}
	}

	public string MainEventID
	{
		get
		{
			return mainEventID;
		}
		set
		{
			mainEventID = value;
		}
	}

	public bool IsFromDungeon
	{
		get
		{
			return m_isFromDungeon;
		}
		set
		{
			m_isFromDungeon = value;
		}
	}

	public bool IsPlayerWeaponGoodElementForCurrentDungeon
	{
		get
		{
			PlayerDataManager instance = SingletonComponent<PlayerDataManager>.Instance;
			Equipment.Data data = null;
			DungeonDbStruct dungeonDbStruct = null;
			if (null != instance)
			{
				PlayerData playerData = instance.GetPlayerData();
				if (playerData != null)
				{
					data = playerData.GetEquipmentData(Equipment.Part.Weapon);
				}
			}
			dungeonDbStruct = CurrentDungeon;
			if (data == null || dungeonDbStruct == null)
			{
				return false;
			}
			Element.Type element = data.element;
			Element.Type element2 = dungeonDbStruct.element;
			bool flag = Element.GetWeakElement(element2) == element;
			bool flag2 = Element.GetWeakElement(element) == element2;
			return !flag || flag2;
		}
	}

	public bool IsPlayerWeaponGoodLevelForCurrentDungeon
	{
		get
		{
			PlayerDataManager instance = SingletonComponent<PlayerDataManager>.Instance;
			Equipment.Data data = null;
			DungeonDbStruct dungeonDbStruct = null;
			if (null != instance)
			{
				PlayerData playerData = instance.GetPlayerData();
				if (playerData != null)
				{
					data = playerData.GetEquipmentData(Equipment.Part.Weapon);
				}
			}
			dungeonDbStruct = CurrentDungeon;
			if (data == null || dungeonDbStruct == null)
			{
				return false;
			}
			return data.itemDetails.BaseLevel >= dungeonDbStruct.min;
		}
	}

	public List<ServerInventoryItem> ChestRewards
	{
		get
		{
			return m_chestRewards;
		}
	}

	public DungeonDbStruct CurrentDungeon
	{
		get
		{
			return m_currentDungeon;
		}
		set
		{
			m_currentDungeon = value;
		}
	}

	public DungeonDbStruct NextDungeon
	{
		get
		{
			return m_nextDungeon;
		}
		set
		{
			m_nextDungeon = value;
		}
	}

	public CityDbStruct CurrentCity
	{
		get
		{
			return m_currentCity;
		}
		set
		{
			m_currentCity = value;
		}
	}

	public DungeonEventStruct CurrentDungeonEvent
	{
		get
		{
			return m_currentEventDungeon;
		}
		set
		{
			m_currentEventDungeon = value;
		}
	}

	public DungeonEventStruct NextDungeonEvent
	{
		get
		{
			return m_nextEventDungeon;
		}
		set
		{
			m_nextEventDungeon = value;
		}
	}

	public bool IsPlayingEventDungeon
	{
		get
		{
			return m_type == Battle.Type.DungeonEvent;
		}
	}

	public int CurrentEventDifficulty
	{
		get
		{
			return m_currentEventDifficulty;
		}
		set
		{
			m_currentEventDifficulty = value;
		}
	}

	public DungeonDbStruct LatestUnlockedDungeon
	{
		get
		{
			return m_latestUnlockedDungeon;
		}
	}

	public List<DungeonEventStruct> AllDungeonEvents
	{
		get
		{
			return m_dungeonEvents;
		}
	}

	public bool IsSendingDungeonEnd
	{
		get
		{
			return isSendingDungeonEnd;
		}
	}

	public bool IsPlayingTutorial
	{
		get
		{
			if (CurrentDungeonEvent == null)
			{
				return false;
			}
			return BattleType == Battle.Type.DungeonEvent && SingletonComponent<DungeonManager>.Instance.CurrentDungeonEvent.id.Contains("TUTORIAL");
		}
	}

	public PlayerData GetOriginalStats()
	{
		return m_oldData;
	}

	public void InitManager(JSONObject p_unlockedCities, JSONObject p_dungeonEvents)
	{
		StartCoroutine(SetupWorldFromData(p_unlockedCities, p_dungeonEvents));
		PrepareBossIdVsBossIconMap();
	}

	public int GetIGCReward()
	{
		int num = 0;
		if (SingletonComponent<BattleManager>.Instance != null && SingletonComponent<BattleManager>.Instance.IsRepeatedTutorialBattle)
		{
			return num;
		}
		if (m_type == Battle.Type.Dungeon && m_currentDungeon != null)
		{
			num = m_currentDungeon.igcReward;
		}
		else if (m_type == Battle.Type.DungeonEvent && m_currentEventDungeon != null)
		{
			num = m_currentEventDungeon.igcReward;
		}
		return num + SingletonComponent<BattleManager>.Instance.m_dodoboIgcBattle;
	}

	public int GetXPReward()
	{
		int num = 0;
		if (SingletonComponent<BattleManager>.Instance != null && SingletonComponent<BattleManager>.Instance.IsRepeatedTutorialBattle)
		{
			return num;
		}
		PlayerDataManager instance = SingletonComponent<PlayerDataManager>.Instance;
		if (instance == null)
		{
			return num;
		}
		PlayerData playerData = instance.GetPlayerData();
		if (playerData == null)
		{
			return num;
		}
		if (m_battles == null || m_battleCounter >= m_battles.Count)
		{
			return num;
		}
		int level = playerData.Level;
		string monsterId = m_battles[m_battleCounter].monsterId;
		DbPlayerDataStruct monsterDbData = instance.GetMonsterDbData(monsterId);
		if (monsterDbData == null)
		{
			Dbg.Log("No current Monster found : " + monsterId);
			return num;
		}
		int rewardExperience = monsterDbData.rewardExperience;
		int level2 = monsterDbData.level;
		float num2 = level - level2;
		float num3 = 0.1f;
		if (num2 >= 0f)
		{
			if (num2 < 10f)
			{
				float num4 = num2 * num3;
				Dbg.Log(" ************** FLOATS ************* : levelDiff*ratio " + num2 + "x" + num3 + " = " + num4);
				int num5 = (int)(num4 * (float)rewardExperience);
				num = rewardExperience - num5;
			}
			else
			{
				Dbg.Log("your level difference is too high no xp will be added.");
			}
		}
		else
		{
			float num6 = Mathf.Abs(num2);
			if (num6 >= 5f)
			{
				num6 = 5f;
			}
			float num7 = num6 * num3;
			Dbg.Log(" ************** FLOATS ************* : absDiff*ratio " + num6 + "x" + num3 + " = " + num7);
			int num8 = (int)(num7 * (float)rewardExperience);
			num = rewardExperience + num8;
		}
		if (num < 0)
		{
			num = 0;
		}
		Dbg.Log("XP Reward : " + num);
		return num;
	}

	public ItemEnum.Chest GetTreasureReward()
	{
		if (m_battles == null || m_battleCounter >= m_battles.Count)
		{
			return ItemEnum.Chest.NONE;
		}
		ItemEnum.Chest itemChest = m_battles[m_battleCounter].itemChest;
		Dbg.Log("Chest Reward : " + itemChest);
		return itemChest;
	}

	public List<ItemEnum.Chest> GetTreasureList()
	{
		List<ItemEnum.Chest> list = new List<ItemEnum.Chest>();
		for (int i = 0; i < m_battles.Count; i++)
		{
			if (m_battles[i].itemChest != ItemEnum.Chest.NONE)
			{
				list.Add(m_battles[i].itemChest);
			}
		}
		return list;
	}

	public bool HasDungeonComplete()
	{
		return m_hasDungeonComplete;
	}

	public bool ShowDungeonResultScreen()
	{
		return m_showDungeonResult;
	}

	public void SetShowDungeonResultScreen(bool doShow)
	{
		m_showDungeonResult = doShow;
	}

	public bool HasLevelUp()
	{
		return m_hasLevelUp;
	}

	public bool HasCityUnlocked()
	{
		return m_cityUnlocked;
	}

	public bool HasDungeonUnlocked()
	{
		return m_dungeonUnlocked;
	}

	public bool HasEventCategoryUnlocked()
	{
		return m_eventCategoryUnlocked;
	}

	public bool HasDungeonEventUnlocked()
	{
		return m_dungeonEventUnlocked;
	}

	public bool HasPetEventUnlocked()
	{
		return m_petEventUnlocked;
	}

	public void ResetDungeonCompleteFlag()
	{
		m_hasDungeonComplete = false;
	}

	public void ResetLevelUpFlag()
	{
		m_hasLevelUp = false;
	}

	public int GetGainedXP()
	{
		return m_expGained;
	}

	public int GetInGameGained()
	{
		return m_ingameGained;
	}

	public void ResetCityUnlockedFlag()
	{
		m_cityUnlocked = false;
	}

	public void ResetDungeonUnlockedFlag()
	{
		m_dungeonUnlocked = false;
	}

	public void ResetDungeonEventUnlockedFlag()
	{
		m_dungeonEventUnlocked = false;
	}

	public void ResetEventCategoryUnlockedFlag()
	{
		m_eventCategoryUnlocked = false;
	}

	public void ResetPetEventUnlockedFlag()
	{
		m_petEventUnlocked = false;
	}

	public int FindDungeonInStack(DungeonDbStruct p_dungeon)
	{
		for (int i = 0; i < m_dungeonStack.Count; i++)
		{
			if (m_dungeonStack[i].dungeon == p_dungeon)
			{
				return i;
			}
		}
		return -1;
	}

	public int GetNumOfNewDungeon()
	{
		int num = 0;
		foreach (DungeonStruct item in m_dungeonStack)
		{
			if (!item.visited && !item.cleared)
			{
				num++;
			}
		}
		return num;
	}

	public bool IsDungeonUnlocked(DungeonDbStruct p_dungeon)
	{
		return FindDungeonInStack(p_dungeon) != -1;
	}

	public DungeonDbStruct GetDungeonData(string p_id)
	{
		foreach (DungeonDbStruct item in m_dungeonsDb)
		{
			DungeonDbStruct dungeonDbStruct = item;
			if (dungeonDbStruct.id == p_id)
			{
				return dungeonDbStruct;
			}
		}
		return null;
	}

	public List<DungeonDbStruct> GetAllDungeonData()
	{
		return m_dungeonsDb;
	}

	public List<DungeonDbStruct> GetAllDungeonDataOfCity(string p_id)
	{
		List<DungeonDbStruct> list = new List<DungeonDbStruct>();
		for (int i = 0; i < m_dungeonsDb.Count; i++)
		{
			if (m_dungeonsDb[i].cityRef == p_id)
			{
				list.Add(m_dungeonsDb[i]);
			}
		}
		return list;
	}

	public List<DungeonStruct> GetAllUnlockedDungeonsOfCity(CityDbStruct p_city)
	{
		List<DungeonStruct> list = new List<DungeonStruct>();
		for (int i = 0; i < m_dungeonStack.Count; i++)
		{
			if (m_dungeonStack[i].dungeon.cityRef == p_city.id)
			{
				DungeonStruct dungeonStruct = m_dungeonStack[i];
				if (m_currentDungeon == null && (!dungeonStruct.visited || (!dungeonStruct.cleared && dungeonStruct.visited)))
				{
					m_currentDungeon = dungeonStruct.dungeon;
				}
				list.Insert(0, dungeonStruct);
			}
		}
		return list;
	}

	public CityDbStruct GetCityData(string p_id)
	{
		foreach (CityDbStruct item in m_citiesDb)
		{
			CityDbStruct cityDbStruct = item;
			if (cityDbStruct.id == p_id)
			{
				return cityDbStruct;
			}
		}
		return null;
	}

	public CityDbStruct GetAdjacentCityData(string p_id)
	{
		bool flag = false;
		foreach (CityDbStruct item in m_citiesDb)
		{
			CityDbStruct cityDbStruct = item;
			if (flag)
			{
				return cityDbStruct;
			}
			if (cityDbStruct.id == p_id)
			{
				flag = true;
			}
		}
		return null;
	}

	public List<CityDbStruct> GetAllCityData()
	{
		return m_citiesDb;
	}

	public List<CityStruct> GetAllUnlockedCities()
	{
		List<CityStruct> list = new List<CityStruct>();
		for (int i = 0; i < m_citiesStack.Count; i++)
		{
			CityStruct cityStruct = m_citiesStack[i];
			if (m_currentCity == null && (!cityStruct.visited || (!cityStruct.cleared && cityStruct.visited)))
			{
				m_currentCity = cityStruct.city;
			}
			list.Insert(0, cityStruct);
			UpdateCityStatus(cityStruct);
		}
		return list;
	}

	public void UpdateCityStatus(CityStruct p_city)
	{
		List<DungeonStruct> allUnlockedDungeonsOfCity = GetAllUnlockedDungeonsOfCity(p_city.city);
		p_city.cleared = true;
		p_city.visited = true;
		foreach (DungeonStruct item in allUnlockedDungeonsOfCity)
		{
			p_city.cleared = p_city.cleared && item.cleared;
			if (!item.visited)
			{
				p_city.visited = false;
			}
		}
	}

	public int GetLatestActOfCity(CityDbStruct p_city)
	{
		List<DungeonStruct> allUnlockedDungeonsOfCity = GetAllUnlockedDungeonsOfCity(p_city);
		if (allUnlockedDungeonsOfCity.Count < 1)
		{
			return 0;
		}
		return allUnlockedDungeonsOfCity[0].dungeon.difficulty;
	}

	public int FindCityInStack(CityDbStruct p_city)
	{
		for (int i = 0; i < m_citiesStack.Count; i++)
		{
			if (m_citiesStack[i].city == p_city)
			{
				return i;
			}
		}
		return -1;
	}

	public bool IsCityUnlocked(CityDbStruct p_city)
	{
		return FindCityInStack(p_city) != -1;
	}

	public DungeonEventStruct FindDungeonEvent(string p_szDungeonID)
	{
		if (m_dungeonEvents == null)
		{
			Dbg.LogError("m_dungeonEvents is null, Dungeon not found : " + p_szDungeonID);
			return null;
		}
		foreach (DungeonEventStruct dungeonEvent in m_dungeonEvents)
		{
			if (dungeonEvent.id == p_szDungeonID)
			{
				return dungeonEvent;
			}
		}
		Dbg.LogWarn("Dungeon not found : " + p_szDungeonID);
		return null;
	}

	public void StartDungeonOnline(Services.ServiceHandle.ServiceCompleteCallback p_success, Services.ServiceHandle.ServiceErrorCallback p_fail)
	{
		m_type = Battle.Type.Dungeon;
		m_hasLevelUp = false;
		m_startDungeonSuccessCb = p_success;
		m_startDungeonFailCb = p_fail;
		ResetBattleCounter();
		ResetDodoboCurrency();
		m_battles.Clear();
		ClearDungeonPrefs();
		DungeonDbStruct currentDungeon = CurrentDungeon;
		if (Dbg.Assert(currentDungeon != null, "Current Dungeon is null"))
		{
			if (p_fail != null)
			{
				p_fail(null, "The CurrentDungeon is null");
			}
		}
		else
		{
			isSendingDungeonEnd = false;
			SingletonComponent<Services>.Instance.DungeonStart(SingletonComponent<Services>.Instance.NubeeId, SingletonComponent<Services>.Instance.DeviceId, currentDungeon.id, "story", m_sessionRequestID.ToString(), null, InternalStartDungeonSuccess, InternalStartDungeonFail);
		}
	}

	private string GetDodoboNameForBattle()
	{
		MonsterSpecies monsterSpecies = MonsterSpecies.Unknown;
		monsterSpecies = ((UnityEngine.Random.Range(0, 2) != 0) ? MonsterSpecies.Bobodo : MonsterSpecies.Dodobo);
		DungeonDbStruct currentDungeon = CurrentDungeon;
		string text = "none";
		PlayerDataManager instance = SingletonComponent<PlayerDataManager>.Instance;
		ArrayList arrayList = new ArrayList();
		instance.QueryMonsters(arrayList, currentDungeon.difficulty, Element.Type.Metal);
		if (arrayList.Count == 0)
		{
			return null;
		}
		foreach (DbPlayerDataStruct item in arrayList)
		{
			if (item.species == monsterSpecies)
			{
				text = item.id;
				break;
			}
		}
		if (text == "none")
		{
			return null;
		}
		return text;
	}

	public void ClearDungeonPrefs()
	{
		MBPrefs.SetFloat("Ms:PlayerRetryHp", 0f);
		MBPrefs.SetFloat("Ms:ItemProgress", 0f);
		MBPrefs.SetString("Ms:Rankings", string.Empty);
		MBPrefs.SetString("Ms:Medals", string.Empty);
		MBPrefs.SetString("Ms:BattleScores", string.Empty);
	}

	public void StartDungeonEventOnline(Services.ServiceHandle.ServiceCompleteCallback p_success, Services.ServiceHandle.ServiceErrorCallback p_fail, int p_difficulty = 0, string p_eventId = "", int p_battleCount = 1, bool p_isTutorial = false)
	{
		m_type = Battle.Type.DungeonEvent;
		m_hasLevelUp = false;
		m_startDungeonSuccessCb = p_success;
		m_startDungeonFailCb = p_fail;
		ClearDungeonPrefs();
		isSendingDungeonEnd = false;
		SingletonComponent<Services>.Instance.DungeonEventStart(SingletonComponent<Services>.Instance.NubeeId, SingletonComponent<Services>.Instance.DeviceId, CurrentDungeonEvent.id, CurrentDungeonEvent.battleCount, m_sessionRequestID.ToString(), CurrentEventDifficulty, CurrentDungeonEvent.type == EventEnum.Type.Tutorial, InternalStartDungeonEventSuccess, InternalStartDungeonEventFail);
	}

	public void EndDungeon(bool p_win, Services.ServiceHandle.ServiceCompleteCallback p_success, Services.ServiceHandle.ServiceErrorCallback p_fail)
	{
		isSendingDungeonEnd = true;
		m_endDungeonSuccessCb = p_success;
		m_endDungeonFailCb = p_fail;
		m_hasDungeonComplete = p_win && IsLastBattle();
		SetShowDungeonResultScreen(true);
		string text = MBPrefs.GetString("Ms:BattleScores", string.Empty);
		NumOfMonsterDefeated = text.Split('|').Length - 1;
		float result = 0f;
		string[] array = text.Split('|');
		if (!m_hasDungeonComplete)
		{
			MBPrefs.SetString("Ms:Medals", string.Empty);
		}
		else if (array.Length > 1)
		{
			if (!float.TryParse(array[0], out result))
			{
				result = 0f;
			}
			for (int i = 1; i < array.Length - 1; i++)
			{
				float result2 = 0f;
				if (!float.TryParse(array[i], out result2))
				{
					result2 = 0f;
				}
				if (result2 < result)
				{
					result = result2;
				}
			}
		}
		string text2 = string.Empty;
		if (SingletonComponent<PlayerDataManager>.Instance != null)
		{
			PlayerData allyData = SingletonComponent<PlayerDataManager>.Instance.GetAllyData(0);
			if (allyData != null)
			{
				text2 = allyData.m_userData.nubeeId;
			}
		}
		int num = (int)SingletonComponent<LeaderboardManager>.Instance.m_TotalScore;
		if (!m_hasDungeonComplete)
		{
			Dbg.Log("The player lose, set the battle score to 0");
			num = 0;
			SingletonComponent<LeaderboardManager>.Instance.m_TotalScore = 0f;
		}
		Dbg.Log("[Info] The friend assisted the player, id: " + text2);
		SingletonComponent<Services>.Instance.DungeonEnd(SingletonComponent<Services>.Instance.NubeeId, SingletonComponent<Services>.Instance.DeviceId, m_dungeonKey, (!m_hasDungeonComplete) ? "lose" : "completed", NumOfMonsterDefeated, m_isKingBossDefeated, num.ToString(), result.ToString(), text2, m_dodoboIgcDungeon, m_sessionRequestID.ToString(), InternalEndDungeonSuccess, InternalEndDungeonFail);
	}

	private void InternalStartDungeonSuccess(Services.ServiceHandle p_handle)
	{
		Dbg.LogWarn("Start Dungeon Success");
		ServerBattleDetails serverBattleDetails = (ServerBattleDetails)p_handle.returnValue;
		MBPrefs.SetInt("dungeon_flag_visited_" + m_currentDungeon.id, 1);
		SingletonComponent<LeaderboardManager>.Instance.m_TotalScore = 0f;
		SingletonComponent<LeaderboardManager>.Instance.m_TotalMaxScore = 0f;
		if (!InitBattle(serverBattleDetails, false) && m_startDungeonFailCb != null)
		{
			m_startDungeonFailCb(p_handle, "Could not initBattle with the Given Monsters.");
			m_startDungeonFailCb = null;
		}
		if (!string.IsNullOrEmpty(serverBattleDetails.kingBossId))
		{
			BattleStruct battleStruct = new BattleStruct(m_battles[serverBattleDetails.kingBossInsertIndex]);
			battleStruct.monsterId = serverBattleDetails.kingBossId;
			m_battles.Insert(serverBattleDetails.kingBossInsertIndex, battleStruct);
			Dbg.Log(string.Format("[Info] Insert king boss: {0} to index: {1}", serverBattleDetails.kingBossId, serverBattleDetails.kingBossInsertIndex));
		}
		foreach (BattleStruct battle in m_battles)
		{
			if (!battle.monsterId.Contains("BOSS") && !battle.monsterId.Contains("BODOBO") && !battle.monsterId.Contains("DODOBO"))
			{
				continue;
			}
			DbPlayerDataStruct monsterDbData = SingletonComponent<PlayerDataManager>.Instance.GetMonsterDbData(battle.monsterId);
			if (monsterDbData != null)
			{
				switch (monsterDbData.species)
				{
				case MonsterSpecies.LandDragon:
					SingletonComponent<LoadingTipsManager>.Instance.NextTipCategory = "VAHAGN_TIP";
					break;
				case MonsterSpecies.Hydra:
					SingletonComponent<LoadingTipsManager>.Instance.NextTipCategory = "SAHAPET_DJOUR_TIP";
					break;
				case MonsterSpecies.WingChicken:
					SingletonComponent<LoadingTipsManager>.Instance.NextTipCategory = "NHANG_AGNI_TIP";
					break;
				case MonsterSpecies.StoneDragon:
					SingletonComponent<LoadingTipsManager>.Instance.NextTipCategory = "ANAHIT_TIP";
					break;
				case MonsterSpecies.Bobodo:
					SingletonComponent<LoadingTipsManager>.Instance.NextTipCategory = "BODOBO_TIP";
					break;
				case MonsterSpecies.Dodobo:
					SingletonComponent<LoadingTipsManager>.Instance.NextTipCategory = "DODOBO_TIP";
					break;
				case MonsterSpecies.Wolf:
					SingletonComponent<LoadingTipsManager>.Instance.NextTipCategory = "ARALEZ_TIP";
					break;
				}
			}
		}
		int index = FindDungeonInStack(m_currentDungeon);
		m_dungeonStack[index].visited = true;
		index = FindCityInStack(m_currentCity);
		UpdateCityStatus(m_citiesStack[index]);
		m_dungeonKey = serverBattleDetails.dungeon_key;
		if (m_startDungeonSuccessCb != null)
		{
			m_startDungeonSuccessCb(p_handle);
		}
		m_startDungeonSuccessCb = null;
	}

	private bool InitBattle(ServerBattleDetails p_battleDetails, bool p_eventOrDungeon)
	{
		if (Dbg.Assert(p_battleDetails != null, "Server Battle Details are null!"))
		{
			return false;
		}
		List<string> monster_ids = p_battleDetails.monster_ids;
		if (Dbg.Assert(monster_ids != null, "No MonsterID from the server Battle Details!"))
		{
			return false;
		}
		if (SingletonComponent<PlayerDataManager>.Instance != null)
		{
			SingletonComponent<PlayerDataManager>.Instance.LoadMonsterForDungeonEvents(monster_ids);
		}
		m_dungeonKey = p_battleDetails.dungeon_key;
		DungeonDbStruct dungeonDbStruct = null;
		dungeonDbStruct = ((!p_eventOrDungeon) ? CurrentDungeon : CurrentDungeonEvent);
		if (Dbg.Assert(dungeonDbStruct != null, "Current Dungeon or Event is null!"))
		{
			return false;
		}
		ResetBattleCounter();
		ResetDodoboCurrency();
		m_battles.Clear();
		int num = 0;
		BattleStruct battleStruct = null;
		for (num = 0; num < dungeonDbStruct.battleCount; num++)
		{
			battleStruct = new BattleStruct();
			battleStruct.dungeonRef = dungeonDbStruct.id;
			battleStruct.battleNumber = num;
			battleStruct.playerLife = 0f;
			if (num < p_battleDetails.chest_list.Count)
			{
				try
				{
					ChestDetails chestDetails = p_battleDetails.chest_list[num];
					if (chestDetails == null)
					{
						battleStruct.itemChest = ItemEnum.Chest.NONE;
					}
					else
					{
						bool _created = false;
						ItemDetails orCreate = SingletonComponent<InventoryManager>.Instance.GetOrCreate(chestDetails.item_id, out _created, chestDetails.item_tier);
						if (orCreate != null)
						{
							battleStruct.itemChest = orCreate.Chest;
						}
						else
						{
							battleStruct.itemChest = ItemEnum.Chest.NONE;
						}
					}
				}
				catch (Exception ex)
				{
					Dbg.LogWarn("error: " + ex.Message);
				}
			}
			else
			{
				battleStruct.itemChest = ItemEnum.Chest.NONE;
			}
			battleStruct.lootExp = 0;
			battleStruct.isBoss = num == dungeonDbStruct.battleCount - 1;
			if (num < 0 || num >= monster_ids.Count)
			{
				Dbg.LogError("Monster Index not found : " + num);
				m_battles.Clear();
				return false;
			}
			DbPlayerDataStruct monsterDbData = SingletonComponent<PlayerDataManager>.Instance.GetMonsterDbData(monster_ids[num]);
			if (Dbg.Assert(monsterDbData != null, "MonsterData was not loaded from CSV : " + monster_ids[num]))
			{
				m_battles.Clear();
				return false;
			}
			battleStruct.monsterElement = monsterDbData.element;
			battleStruct.monsterId = monster_ids[num];
			Dbg.LogWarn("Monster " + num + ": " + battleStruct.monsterId);
			m_battles.Add(battleStruct);
		}
		return true;
	}

	private void InternalStartDungeonEventSuccess(Services.ServiceHandle p_handle)
	{
		Dbg.LogWarn("Start DungeonEvent Success");
		ServerBattleDetails serverBattleDetails = (ServerBattleDetails)p_handle.returnValue;
		if (serverBattleDetails != null)
		{
			foreach (ChestDetails item in serverBattleDetails.chest_list)
			{
				if (item != null)
				{
					Dbg.Log("The rewards for this event : " + item.item_id);
				}
			}
		}
		if (!InitBattle(serverBattleDetails, true))
		{
			if (m_startDungeonFailCb != null)
			{
				m_startDungeonFailCb(p_handle, "Could not initBattle with the Given Monsters.");
				m_startDungeonFailCb = null;
			}
			return;
		}
		if (m_currentEventDungeon != null)
		{
			m_currentEventDungeon.visited = true;
		}
		m_dungeonKey = serverBattleDetails.dungeon_key;
		if (m_startDungeonSuccessCb != null)
		{
			m_startDungeonSuccessCb(p_handle);
		}
		m_startDungeonSuccessCb = null;
	}

	private void InternalStartDungeonFail(Services.ServiceHandle p_handle, string p_error)
	{
		Dbg.LogError("Start Dungeon Fail:" + p_error);
		HandleStaminaShieldException(p_handle);
		if (m_startDungeonFailCb != null)
		{
			m_startDungeonFailCb(p_handle, p_error);
		}
		m_startDungeonFailCb = null;
	}

	private void InternalStartDungeonEventFail(Services.ServiceHandle p_handle, string p_error)
	{
		Dbg.LogError("Start Dungeon Event Fail:" + p_error);
		HandleStaminaShieldException(p_handle);
		if (m_startDungeonFailCb != null)
		{
			m_startDungeonFailCb(p_handle, p_error);
		}
		m_startDungeonFailCb = null;
	}

	private void HandleStaminaShieldException(Services.ServiceHandle p_handle)
	{
		switch ((ServiceResponseCodes)p_handle.nReturnCode)
		{
		case ServiceResponseCodes.EXCEPTION_DUNGEON_NOT_ENOUGH_STAMINA:
		{
			if (p_handle.returnJson == null || !p_handle.returnJson.HasField("data"))
			{
				break;
			}
			JSONObject field2 = p_handle.returnJson.GetField("data");
			if (field2 != null)
			{
				long num4 = GLOBAL.GAME_SETTINGS.shields_reset_time;
				long num5 = num4;
				int num6 = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData().m_userData.stamina;
				if (field2.HasField("interval_increment"))
				{
					num4 = (long)field2["interval_increment"].n;
				}
				if (field2.HasField("current_stamina"))
				{
					num6 = (int)field2["current_stamina"].n;
				}
				if (field2.HasField("next_increment"))
				{
					num5 = (long)field2["next_increment"].n;
				}
				Dbg.Log("new stamina values from server are - Interval_increment: " + num4 + " current_stamina: " + num6 + " next_increment: " + num5);
				PlayerDataManager instance2 = SingletonComponent<PlayerDataManager>.Instance;
				if (null != instance2)
				{
					PlayerData playerData2 = instance2.GetPlayerData();
					playerData2.m_userData.stamina = ((num6 <= playerData2.m_userData.maxStamina) ? num6 : playerData2.m_userData.maxStamina);
					playerData2.m_userData.timeNextStamina = num5;
				}
			}
			break;
		}
		case ServiceResponseCodes.EXCEPTION_DUNGEON_NOT_ENOUGH_SHIELD:
		{
			if (p_handle.returnJson == null || !p_handle.returnJson.HasField("data"))
			{
				break;
			}
			JSONObject field = p_handle.returnJson.GetField("data");
			if (field != null)
			{
				long num = GLOBAL.GAME_SETTINGS.shields_reset_time;
				long num2 = num;
				int num3 = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData().m_userData.shield;
				if (field.HasField("interval_increment"))
				{
					num = (long)field["interval_increment"].n;
				}
				if (field.HasField("current_shields"))
				{
					num3 = (int)field["current_shields"].n;
				}
				if (field.HasField("next_increment"))
				{
					num2 = (long)field["next_increment"].n;
				}
				Dbg.Log("new shield values from server are - Interval_increment: " + num + " current_stamina: " + num3 + " next_increment: " + num2);
				GLOBAL.GAME_SETTINGS.shields_reset_time = (int)num;
				PlayerDataManager instance = SingletonComponent<PlayerDataManager>.Instance;
				if (null != instance)
				{
					PlayerData playerData = instance.GetPlayerData();
					playerData.m_userData.shield = ((num3 <= playerData.m_userData.maxShield) ? num3 : playerData.m_userData.maxShield);
					playerData.m_userData.timeNextShield = num2;
				}
			}
			break;
		}
		}
	}

	private void InternalEndDungeonSuccess(Services.ServiceHandle p_handle)
	{
		Dbg.LogWarn("End Dungeon Success");
		ServerBattleEndDetails serverBattleEndDetails = (ServerBattleEndDetails)p_handle.returnValue;
		m_dungeonKey = string.Empty;
		PlayerData _playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		m_oldData = new PlayerData(_playerData);
		_playerData.SetAccountDetails(serverBattleEndDetails.character);
		m_originalExp = _playerData.m_userData.currentXp;
		m_chestRewards = new List<ServerInventoryItem>(serverBattleEndDetails.rewarded_items.Values);
		m_hasLevelUp = serverBattleEndDetails.character.level != _playerData.Level;
		Dbg.Log("debug, battleDetails.character.level : " + serverBattleEndDetails.character.level);
		Dbg.Log("debug, m_hasLevelUp: " + m_hasLevelUp);
		Dbg.Log("debug, battleDetails.character.exp : " + serverBattleEndDetails.character.exp);
		Dbg.Log("debug, data.m_userData.currentXp : " + _playerData.m_userData.currentXp);
		Dbg.Log("debug, data.m_targetXp(next level xp) : " + _playerData.m_targetXp);
		Dbg.Log("debug, data.m_previousTargetXp : " + _playerData.m_previousTargetXp);
		Dbg.Log("debug-levelup, level: " + _playerData.m_level + "->" + serverBattleEndDetails.character.level);
		Dbg.Log("debug-levelup, agility: " + _playerData.m_agility + "->" + serverBattleEndDetails.character.agility);
		Dbg.Log("debug-levelup, constitution: " + _playerData.m_constitution + "->" + serverBattleEndDetails.character.constitution);
		Dbg.Log("debug-levelup, strength: " + _playerData.m_strength + "->" + serverBattleEndDetails.character.strength);
		Dbg.Log("debug-levelup, stamina: " + _playerData.m_userData.maxStamina + "->" + serverBattleEndDetails.character.max_stamina);
		m_expGained = serverBattleEndDetails.GainedXp;
		Dbg.Log("debug, m_expGained: " + m_expGained);
		m_ingameGained = (int)serverBattleEndDetails.character.currencies.ingame - _playerData.m_userData.inGameCurrency;
		m_dungeonCompScore = SingletonComponent<LeaderboardManager>.Instance.CalculateDungeonScore(SingletonComponent<PlayerDataManager>.Instance.GetPlayerData().Level);
		LoadUnlockedCitiesFromJson(serverBattleEndDetails.unlockedCitiesJson);
		LoadEventDungeonsFromJson(serverBattleEndDetails.eligibleEvents);
		SingletonComponent<SocialManager>.Instance.SaveGiftSenders(serverBattleEndDetails.giftSenders);
		if (SingletonComponent<LeaderboardManager>.Instance != null && SingletonComponent<LeaderboardManager>.Instance.m_player != null)
		{
			SingletonComponent<LeaderboardManager>.Instance.m_player.Score = serverBattleEndDetails.character.battleScore;
			SingletonComponent<LeaderboardManager>.Instance.m_player.Rank = serverBattleEndDetails.character.leaderboardRank;
			SingletonComponent<LeaderboardManager>.Instance.m_player.CharaName = serverBattleEndDetails.character.name;
		}
		if (m_endDungeonSuccessCb != null)
		{
			m_endDungeonSuccessCb(p_handle);
		}
		m_endDungeonSuccessCb = null;
		serverBattleEndDetails.character.ToPlayerData(ref _playerData);
		_playerData.ComputeFinalValues();
		if (SingletonComponent<GameManager>.Instance != null)
		{
			SingletonComponent<GameManager>.Instance.StartStaminaTimer();
		}
	}

	private void InternalEndDungeonFail(Services.ServiceHandle p_handle, string p_error)
	{
		Dbg.LogError("End Dungeon Fail:" + p_error);
		m_chestRewards.Clear();
		if (m_endDungeonFailCb != null)
		{
			m_endDungeonFailCb(p_handle, p_error);
		}
		m_endDungeonFailCb = null;
	}

	public BattleStruct GetCurrentBattle()
	{
		return m_battles[m_battleCounter];
	}

	public BattleStruct GetBattleData(int p_battleNumber)
	{
		return m_battles[p_battleNumber];
	}

	public int GetBattleCount()
	{
		return m_battles.Count;
	}

	public int GetCurrentBattleCounter()
	{
		return m_battleCounter;
	}

	public void IncreaseBattleCounter()
	{
		DungeonDbStruct dungeonDbStruct = CurrentDungeon;
		if (m_type == Battle.Type.DungeonEvent)
		{
			dungeonDbStruct = CurrentDungeonEvent;
		}
		m_battleCounter++;
		if (m_battleCounter >= dungeonDbStruct.battleCount)
		{
			m_battleCounter = dungeonDbStruct.battleCount - 1;
		}
	}

	public void IncreaseDungeonBattleCount()
	{
		DungeonDbStruct dungeonDbStruct = CurrentDungeon;
		if (m_type == Battle.Type.DungeonEvent)
		{
			dungeonDbStruct = CurrentDungeonEvent;
		}
		dungeonDbStruct.battleCount++;
		Dbg.Log("[Info] Increse battle count because the king boss, count: " + dungeonDbStruct.battleCount);
	}

	public bool IsLastBattle()
	{
		DungeonDbStruct dungeonDbStruct = CurrentDungeon;
		if (m_type == Battle.Type.DungeonEvent)
		{
			dungeonDbStruct = CurrentDungeonEvent;
		}
		return m_battleCounter == dungeonDbStruct.battleCount - 1;
	}

	public void ResetBattleCounter()
	{
		m_battleCounter = 0;
	}

	public void ResetDodoboCurrency()
	{
		m_dodoboIgcDungeon = 0;
	}

	public void Reset()
	{
		m_battleCounter = 0;
		m_currentDungeon = null;
		m_currentCity = null;
		m_currentEventDungeon = null;
		m_latestUnlockedDungeon = null;
		m_dodoboIgcDungeon = 0;
	}

	protected void UnlockCity(DungeonDbStruct p_dungeon)
	{
		CityStruct cityAddIfNotExists = GetCityAddIfNotExists(p_dungeon.cityRef);
		cityAddIfNotExists.visited = false;
		cityAddIfNotExists.cleared = false;
		m_cityUnlocked = true;
	}

	public void RefreshDungeonProgression(AsyncCallback p_success, AsyncCallback p_fail)
	{
		m_successRefreshCb = p_success;
		m_failRefreshCb = p_fail;
		SingletonComponent<Services>.Instance.GetUnlockedCities(SingletonComponent<Services>.Instance.NubeeId, SingletonComponent<Services>.Instance.DeviceId, OnGetUnlockedCitiesSuccess, OnGetUnlockedCitiesFail);
	}

	private void OnGetUnlockedCitiesSuccess(Services.ServiceHandle p_handle)
	{
		List<string> list = (List<string>)p_handle.returnValue;
		foreach (string item in list)
		{
			GetCityAddIfNotExists(item);
		}
		m_numberOfCitiesToRefresh = list.Count;
		m_unlockedDungeonsSuccessCount = 0;
		foreach (string item2 in list)
		{
			SingletonComponent<Services>.Instance.GetUnlockedDungeons(SingletonComponent<Services>.Instance.NubeeId, SingletonComponent<Services>.Instance.DeviceId, item2, OnGetUnlockedDungeonsSuccess, OnGetUnlockedDungeonsFail);
		}
	}

	private void OnGetUnlockedCitiesFail(Services.ServiceHandle p_handle, string p_error)
	{
		if (m_failRefreshCb != null)
		{
			m_failRefreshCb(p_handle);
		}
	}

	private void OnGetUnlockedDungeonsSuccess(Services.ServiceHandle p_handle)
	{
		LoadUnlockedDungeonsFromJson((JSONObject)p_handle.returnValue);
		m_unlockedDungeonsSuccessCount++;
		if (m_unlockedDungeonsSuccessCount >= m_numberOfCitiesToRefresh)
		{
			m_unlockedDungeonsSuccessCount = 0;
			if (m_successRefreshCb != null)
			{
				m_successRefreshCb(p_handle);
			}
			m_successRefreshCb = null;
		}
	}

	private void OnGetUnlockedDungeonsFail(Services.ServiceHandle p_handle, string p_error)
	{
		Dbg.LogWarn("Unable to GetUnlockedDungeons: " + ServiceCallUtils.GetErrorMessage(p_handle));
		m_unlockedDungeonsSuccessCount = 0;
		if (m_failRefreshCb != null)
		{
			m_failRefreshCb(p_handle);
		}
		m_failRefreshCb = null;
	}

	private int GetMinimumLevel(string cityId)
	{
		int num = 777;
		foreach (DungeonDbStruct item in m_dungeonsDb)
		{
			if (item.cityRef == cityId && item.requiredLvl < num)
			{
				num = item.requiredLvl;
			}
		}
		return num;
	}

	private CityStruct GetCityAddIfNotExists(string p_cityid)
	{
		CityDbStruct cityData = GetCityData(p_cityid);
		if (cityData == null)
		{
			Dbg.LogError("[DungeonManager::GetCityAddIfNotExists] city id:" + p_cityid + " could not be found");
			return null;
		}
		int num = FindCityInStack(cityData);
		if (num == -1)
		{
			CityStruct cityStruct = new CityStruct();
			cityStruct.visited = false;
			cityStruct.city = cityData;
			cityStruct.minLevel = GetMinimumLevel(p_cityid);
			m_citiesStack.Insert(0, cityStruct);
			return cityStruct;
		}
		return m_citiesStack[num];
	}

	private IEnumerator SetupWorldFromData(JSONObject p_unlockedCities, JSONObject p_dungeonEvents)
	{
		NBProfiler.BeginTask("[DUNGEON_MANAGER][INIT_MANAGER]");
		NBBinaryFormat csvCity = SingletonComponent<ParameterManager>.Instance.GetParametersFor("Cities");
		NBBinaryFormat csvDungeons = SingletonComponent<ParameterManager>.Instance.GetParametersFor("Dungeons");
		Dictionary<string, List<string>> tempCityIdVsBossesId = new Dictionary<string, List<string>>();
		int nRows = 0;
		if (csvDungeons != null)
		{
			nRows = csvDungeons.RowsCount;
		}
		for (int it = 0; it < nRows; it++)
		{
			DungeonDbStruct dungeon = new DungeonDbStruct();
			string rowName = (dungeon.id = csvDungeons.RowsNames[it]);
			dungeon.name = csvDungeons.GetData(rowName, "NAME");
			dungeon.element = Element.FromString(csvDungeons.GetData(rowName, "ELEMENT"));
			dungeon.stage = csvDungeons.GetData(rowName, "STAGE");
			dungeon.boss_id = csvDungeons.GetData(rowName, "BOSS_MONSTER_ID");
			dungeon.chanceElements[0] = csvDungeons.GetDataGeneric<float>(rowName, "CHANCE_FIRE");
			dungeon.chanceElements[1] = csvDungeons.GetDataGeneric<float>(rowName, "CHANCE_NATURE");
			dungeon.chanceElements[2] = csvDungeons.GetDataGeneric<float>(rowName, "CHANCE_LIGHTNING");
			dungeon.chanceElements[3] = csvDungeons.GetDataGeneric<float>(rowName, "CHANCE_WATER");
			dungeon.chanceElements[4] = csvDungeons.GetDataGeneric<float>(rowName, "CHANCE_METAL");
			dungeon.battleCount = csvDungeons.GetDataGeneric<int>(rowName, "NUMBER_OF_BATTLES");
			dungeon.cityRef = csvDungeons.GetData(rowName, "CITY_ID");
			dungeon.difficulty = csvDungeons.GetDataGeneric<int>(rowName, "DIFFICULTY");
			dungeon.igcReward = csvDungeons.GetDataGeneric<int>(rowName, "IGC_REWARD");
			dungeon.stamina = csvDungeons.GetDataGeneric<int>(rowName, "STAMINA_COST");
			dungeon.min = csvDungeons.GetDataGeneric<int>(rowName, "MIN");
			dungeon.max = csvDungeons.GetDataGeneric<int>(rowName, "MAX");
			dungeon.requiredLvl = csvDungeons.GetDataGeneric<int>(rowName, "PLAYER_LEVEL_LIMIT");
			dungeon.hasHalfDungeonCutscene = csvDungeons.GetDataGeneric<int>(rowName, "HAS_HALF_CUTSCENE") != 0;
			dungeon.chanceDodobo = csvDungeons.GetDataGeneric<float>(rowName, "CHANCE_DODOBO");
			dungeon.iconSpriteName = csvDungeons.GetData(rowName, "STAGE_IMG");
			if (dungeon.iconSpriteName == "-1" || string.IsNullOrEmpty(dungeon.iconSpriteName))
			{
				dungeon.iconSpriteName = string.Empty;
				Dbg.LogWarnEx("There's no STAGE_IMG for dungeon id : {0}", Dbg.NBLogType.COMMON_LOG, dungeon.id);
			}
			if (!tempCityIdVsBossesId.ContainsKey(dungeon.cityRef))
			{
				tempCityIdVsBossesId.Add(value: new List<string> { dungeon.boss_id }, key: dungeon.cityRef);
			}
			else
			{
				tempCityIdVsBossesId[dungeon.cityRef].Add(dungeon.boss_id);
			}
			m_dungeonsDb.Add(dungeon);
		}
		yield return null;
		m_currentDungeon = m_dungeonsDb[0];
		nRows = 0;
		if (csvCity != null)
		{
			nRows = csvCity.RowsCount;
		}
		for (int i = 0; i < nRows; i++)
		{
			CityDbStruct city = new CityDbStruct();
			string rowName2 = (city.id = csvCity.RowsNames[i]);
			city.cityName = csvCity.GetData(rowName2, "NAME");
			city.description = csvCity.GetData(rowName2, "DESCRIPTION");
			city.iconSpriteName = csvCity.GetData(rowName2, "STAGE_IMG");
			if (city.iconSpriteName == "-1")
			{
				city.iconSpriteName = string.Empty;
			}
			city.element = Element.FromString(csvCity.GetData(rowName2, "ELEMENT"));
			if (tempCityIdVsBossesId.ContainsKey(city.id))
			{
				city.dungeonBosses = tempCityIdVsBossesId[city.id];
			}
			m_citiesDb.Add(city);
		}
		yield return null;
		m_currentCity = m_citiesDb[0];
		LoadUnlockedCitiesFromJson(p_unlockedCities);
		LoadEventDungeonsFromJson(p_dungeonEvents);
		if (m_dungeonEvents != null && m_dungeonEvents.Count > 0)
		{
			m_currentEventDungeon = m_dungeonEvents[0];
		}
		else
		{
			Dbg.LogError("Dummy Dungeon Event Created!!!");
			m_currentEventDungeon = new DungeonEventStruct();
		}
		SingletonComponent<ParameterManager>.Instance.UnloadParametersFor("Cities");
		csvCity = null;
		SingletonComponent<ParameterManager>.Instance.UnloadParametersFor("Dungeons");
		csvDungeons = null;
		NBProfiler.EndTask("[DUNGEON_MANAGER][INIT_MANAGER]");
	}

	public void UpdateEvents(AsyncCallback p_success, AsyncCallback p_fail)
	{
		Dbg.LogWarn("-------- This function is obsolete it should not be called anymore (re-enable to test 09/01/2013) ------- ");
		m_successGetEventsCb = p_success;
		m_failGetEventsCb = p_fail;
		SingletonComponent<Services>.Instance.GetAllDungeonEvents(SingletonComponent<Services>.Instance.NubeeId, SingletonComponent<Services>.Instance.DeviceId, InternalUpdateEventsSuccess, InternalUpdateEventsFail);
	}

	private void InternalUpdateEventsSuccess(Services.ServiceHandle p_handle)
	{
		if (p_handle == null)
		{
			if (m_failGetEventsCb != null)
			{
				m_failGetEventsCb(null);
				m_failGetEventsCb = null;
			}
			return;
		}
		LoadEventDungeonsFromJson((JSONObject)p_handle.returnValue);
		if (m_successGetEventsCb != null)
		{
			m_successGetEventsCb(p_handle);
			m_successGetEventsCb = null;
		}
	}

	private void InternalUpdateEventsFail(Services.ServiceHandle p_handle, string p_error)
	{
		if (m_failGetEventsCb != null)
		{
			AsyncCallback failGetEventsCb = m_failGetEventsCb;
			m_failGetEventsCb = null;
			failGetEventsCb(p_handle);
		}
		Dbg.LogError("InternalUpdateEventsFail code : " + ServiceCallUtils.GetErrorMessage(p_handle));
	}

	public void LoadUnlockedCitiesFromJson(JSONObject p_payload)
	{
		if (p_payload == null || p_payload.type == JSONObject.Type.NULL)
		{
			Dbg.LogError("[DungeonManager::LoadUnlockedCitiesFromJson] invalid payload");
			return;
		}
		if (p_payload.Count < 0 || p_payload.IsEmptyArray())
		{
			Dbg.LogWarn(" ***** [DungeonManager::LoadUnlockedCitiesFromJson] No City found ***** ");
			return;
		}
		foreach (JSONObject item in p_payload.list)
		{
			string stringField = ServiceCallUtils.GetStringField(item, "city_id", string.Empty);
			if (stringField != null && stringField.CompareTo(string.Empty) != 0)
			{
				GetCityAddIfNotExists(stringField);
			}
			if (item.HasField("dungeons"))
			{
				LoadUnlockedDungeonsFromJson(item["dungeons"]);
			}
		}
	}

	private void LoadUnlockedDungeonsFromJson(JSONObject p_dungeons)
	{
		if (p_dungeons == null || p_dungeons.type == JSONObject.Type.NULL || p_dungeons.Count < 0 || p_dungeons.IsEmptyArray())
		{
			Dbg.LogError("[DungeonManager::LoadUnlockedDungeonsFromJson] invalid dungeons payload");
			return;
		}
		foreach (JSONObject item in p_dungeons.list)
		{
			string stringField = ServiceCallUtils.GetStringField(item, "dungeon_id", string.Empty);
			float field = ServiceCallUtils.GetField<float>(item, "highest_score");
			int field2 = ServiceCallUtils.GetField<int>(item, "is_cleared");
			DungeonDbStruct dungeonData = GetDungeonData(stringField);
			if (dungeonData == null)
			{
				Dbg.LogError("[DungeonManager::LoadUnlockedCitiesFromJson] unable to find dungeon csv data id:" + stringField);
				continue;
			}
			int num = FindDungeonInStack(dungeonData);
			if (num == -1)
			{
				DungeonStruct dungeonStruct = new DungeonStruct();
				dungeonStruct.cleared = field2 > 0;
				dungeonStruct.medal = (DungeonStruct.Medal)SingletonComponent<LeaderboardManager>.Instance.CalculateMedalRank(field);
				dungeonStruct.visited = MBPrefs.GetInt("dungeon_flag_visited_" + dungeonData.id) > 0;
				dungeonStruct.dungeon = dungeonData;
				m_dungeonStack.Insert(0, dungeonStruct);
			}
			else
			{
				m_dungeonStack[num].cleared = field2 > 0;
				m_dungeonStack[num].medal = (DungeonStruct.Medal)SingletonComponent<LeaderboardManager>.Instance.CalculateMedalRank(field);
				m_dungeonStack[num].visited = MBPrefs.GetInt("dungeon_flag_visited_" + dungeonData.id) > 0;
			}
		}
	}

	private static int CompareDungeonEventDbStruct(DungeonEventStruct a, DungeonEventStruct b)
	{
		return a.id.CompareTo(b.id);
	}

	public void LoadEventDungeonsFromJson(JSONObject p_payload)
	{
		if (p_payload == null || p_payload.type == JSONObject.Type.NULL || p_payload.Count < 0 || p_payload.IsEmptyArray())
		{
			Dbg.LogError("[DungeonManager::LoadEventDungensFromJson] invalid payload");
			return;
		}
		m_dungeonEvents.Clear();
		DungeonEventStruct dungeonEventStruct = new DungeonEventStruct();
		if (dungeonEventStruct == null)
		{
			return;
		}
		foreach (JSONObject item in p_payload.list)
		{
			dungeonEventStruct.id = ServiceCallUtils.GetStringField(item, "id", string.Empty);
			if (dungeonEventStruct.id == null || dungeonEventStruct.id.CompareTo(string.Empty) == 0)
			{
				Dbg.LogError("[DungeonManager::LoadEventDungeonFromJson] invalid id");
				continue;
			}
			dungeonEventStruct.boss_id = ServiceCallUtils.GetStringField(item, "boss_monster_id", string.Empty);
			if (dungeonEventStruct.boss_id == null || dungeonEventStruct.boss_id.CompareTo(string.Empty) == 0)
			{
				Dbg.LogError("[DungeonManager::LoadEventDungeonFromJson] Invalid or missing boss_id");
				continue;
			}
			dungeonEventStruct.categoryRef = ServiceCallUtils.GetStringField(item, "event_city_id", string.Empty);
			if (dungeonEventStruct.categoryRef == null || dungeonEventStruct.categoryRef.CompareTo(string.Empty) == 0)
			{
				Dbg.LogError("[DungeonManager::LoadEventDungeonFromJson] Invalid or missing categoryRef");
				continue;
			}
			dungeonEventStruct.stage = ServiceCallUtils.GetStringField(item, "stage", string.Empty);
			if (dungeonEventStruct.stage == null || dungeonEventStruct.stage.CompareTo(string.Empty) == 0)
			{
				Dbg.LogError("[DungeonManager::LoadEventDungeonFromJson] Invalid or missing stage");
				continue;
			}
			dungeonEventStruct.type = EventEnum.FromString(ServiceCallUtils.GetStringField(item, "type", string.Empty));
			if (dungeonEventStruct.type == EventEnum.Type.Max)
			{
				Dbg.LogError("[DungeonManager::LoadEventDungeonFromJson] Invalid event type: " + ServiceCallUtils.GetStringField(item, "type", string.Empty));
				continue;
			}
			dungeonEventStruct.imageUrl = ServiceCallUtils.GetStringField(item, "image_url", string.Empty);
			if (dungeonEventStruct.imageUrl == null)
			{
				dungeonEventStruct.imageUrl = string.Empty;
			}
			dungeonEventStruct.imageMD5 = ServiceCallUtils.GetStringField(item, "image_md5", string.Empty);
			if (dungeonEventStruct.imageUrl == null)
			{
				dungeonEventStruct.imageUrl = string.Empty;
			}
			dungeonEventStruct.name = ServiceCallUtils.GetStringField(item, "name", string.Empty);
			if (dungeonEventStruct.name == null || dungeonEventStruct.name.CompareTo(string.Empty) == 0)
			{
				Dbg.LogError("[DungeonManager::LoadEventDungeonFromJson] Invalid or missing name");
				continue;
			}
			dungeonEventStruct.igcReward = ServiceCallUtils.GetField<int>(item, "igc_reward");
			dungeonEventStruct.visited = ServiceCallUtils.GetField<int>(item, "is_new") == 0;
			dungeonEventStruct.cleared = ServiceCallUtils.GetField<int>(item, "is_cleared") > 0;
			dungeonEventStruct.battleCount = ServiceCallUtils.GetField<int>(item, "num_battles");
			dungeonEventStruct.xpMultiplier = ServiceCallUtils.GetField<int>(item, "xp_multiplier");
			dungeonEventStruct.endTime = ServiceCallUtils.GetField<long>(item, "end_time");
			Element.Type element = Element.FromString(ServiceCallUtils.GetStringField(item, "element", string.Empty).ToLower());
			dungeonEventStruct.element = element;
			DungeonEventStruct dungeonEventStruct2 = FindDungeonEvent(dungeonEventStruct.id);
			if (dungeonEventStruct2 == null)
			{
				m_dungeonEvents.Add(dungeonEventStruct);
				dungeonEventStruct = new DungeonEventStruct();
				if (dungeonEventStruct != null)
				{
					continue;
				}
				return;
			}
			dungeonEventStruct2.id = dungeonEventStruct.id;
			dungeonEventStruct2.boss_id = dungeonEventStruct.boss_id;
			dungeonEventStruct2.categoryRef = dungeonEventStruct.categoryRef;
			dungeonEventStruct2.stage = dungeonEventStruct.stage;
			dungeonEventStruct2.name = dungeonEventStruct.name;
			dungeonEventStruct2.type = dungeonEventStruct.type;
			dungeonEventStruct2.difficulty = dungeonEventStruct.difficulty;
			dungeonEventStruct2.igcReward = dungeonEventStruct.igcReward;
			dungeonEventStruct2.visited = dungeonEventStruct.visited;
			dungeonEventStruct2.cleared = dungeonEventStruct.cleared;
			dungeonEventStruct2.min = dungeonEventStruct.min;
			dungeonEventStruct2.max = dungeonEventStruct.max;
			dungeonEventStruct2.battleCount = dungeonEventStruct.battleCount;
			dungeonEventStruct2.stamina = dungeonEventStruct.stamina;
			dungeonEventStruct2.xpMultiplier = dungeonEventStruct.xpMultiplier;
		}
		MBSort.Sort(m_dungeonEvents, CompareDungeonEventDbStruct);
	}

	private Element.Type ChooseElementBasedOnProbabilities(float[] p_chances)
	{
		Element.Type type = Element.Type.Max;
		int num = 0;
		int num2 = 0;
		for (num = 0; num < p_chances.Length; num++)
		{
			num2 += (int)(p_chances[num] * 100f);
		}
		int num3 = UnityEngine.Random.Range(1, num2);
		for (num = p_chances.Length - 1; num >= 0; num--)
		{
			num2 -= (int)(p_chances[num] * 100f);
			if (num2 <= num3)
			{
				type = (Element.Type)num;
				break;
			}
		}
		if (type == Element.Type.Max)
		{
			Dbg.LogError("Chosen Element for monster is not valid! Defaulting to fire.....");
			return Element.Type.Fire;
		}
		return type;
	}

	private string GetMonsterForBattle(int p_difficulty, Element.Type p_element, int p_playerLevel, int p_battle, int p_totalBattles, int min, int max)
	{
		PlayerDataManager instance = SingletonComponent<PlayerDataManager>.Instance;
		ArrayList arrayList = new ArrayList();
		int num = p_difficulty;
		int num2 = ((p_playerLevel < 2) ? 1 : (p_playerLevel - 1)) + p_battle;
		int num3 = p_totalBattles - 2;
		if (num3 < 1)
		{
			num3 = 1;
		}
		if (p_battle == num3)
		{
			num2 = p_playerLevel - 1;
		}
		if (num2 < 1)
		{
			num2 = 1;
		}
		if (num2 > max)
		{
			num2 = max;
		}
		if (num2 < min)
		{
			num2 = min;
		}
		int num4 = 0;
		do
		{
			arrayList.Clear();
			num4 = instance.QueryMonstersExceptSpecies(arrayList, p_element, m_previousSpecies);
			Dbg.Log("Query difficulty: " + num + " element: " + p_element.ToString() + " results: " + num4);
			if (num4 > 0)
			{
				RemoveDodoboFromList(arrayList);
				instance.FilterTrimBoss(arrayList);
				instance.FilterExceptLevel(arrayList, num2);
				num4 = arrayList.Count;
				Dbg.Log("Filter level: " + num2 + " results: " + num4);
				if (num4 < 1)
				{
					num2--;
					Dbg.LogWarn("GetMonsterForBattle: No matching monster for the specified level. Looping");
					if (num2 < 1)
					{
						Dbg.LogError("GetMonsterForBattle: Negative level reached!");
						return string.Empty;
					}
				}
			}
			else
			{
				num--;
				Dbg.LogWarn("GetMonsterForBattle: No matching monster given the element and difficulty. Looping");
				if (num < 1)
				{
					Dbg.LogError("GetMonsterForBattle: Minimum difficulty reached!");
					return string.Empty;
				}
			}
		}
		while (num4 < 1);
		int index = UnityEngine.Random.Range(0, arrayList.Count);
		DbPlayerDataStruct dbPlayerDataStruct = arrayList[index] as DbPlayerDataStruct;
		if (p_battle != p_totalBattles - 1 && dbPlayerDataStruct.isBoss)
		{
			Dbg.LogError("WALAU EH! BOSS IS NOT ALLOWED AS TRASH MOB!");
		}
		m_previousSpecies = dbPlayerDataStruct.species;
		return dbPlayerDataStruct.id;
	}

	private void RemoveDodoboFromList(ArrayList list)
	{
		int num = list.Count;
		for (int num2 = list.Count - 1; num2 >= 0; num2--)
		{
			DbPlayerDataStruct dbPlayerDataStruct = list[num2] as DbPlayerDataStruct;
			if (dbPlayerDataStruct.species == MonsterSpecies.Dodobo || dbPlayerDataStruct.species == MonsterSpecies.Bobodo)
			{
				list.RemoveAt(num2);
				num--;
			}
		}
	}

	private void PrepareBossIdVsBossIconMap()
	{
		m_BossIdVsBossIcon.Clear();
		for (int i = 0; i < m_bossIds.Length; i++)
		{
			m_BossIdVsBossIcon.Add(m_bossIds[i], m_bossIcons[i]);
		}
	}

	public string GetBossIconByBossId(string p_bossId)
	{
		string text = Regex.Replace(p_bossId, "\\d", string.Empty);
		if (text.EndsWith("_"))
		{
			text = text.Remove(text.Length - 1);
		}
		if (m_BossIdVsBossIcon.ContainsKey(text))
		{
			return m_BossIdVsBossIcon[text];
		}
		Dbg.LogWarnEx("Unable to find boss icon for boss id: {0}", Dbg.NBLogType.COMMON_WARNING, p_bossId);
		return "Earth_wingchicken_boss";
	}

	public SetInformation GetSetInfoByCity(CityDbStruct p_cityData)
	{
		Debug.Log("GetSetInfoByCity, current city : " + p_cityData.cityName);
		SetInformation setInformation = new SetInformation();
		setInformation.WeaponItemId = "WEAPON_DRAGON_KING_T4_EPIC_HILL";
		setInformation.HelmItemId = "ARMOUR_HELM_DRAGON_KING_T4_EPIC_HILL";
		setInformation.ShoulderItemId = "ARMOUR_SHOULDERS_DRAGON_KING_T4_EPIC_HILL";
		setInformation.ChestItemId = "ARMOUR_CHEST_DRAGON_KING_T4_EPIC_HILL";
		setInformation.LeggingItemId = "ARMOUR_LEGGINGS_DRAGON_KING_T4_EPIC_HILL";
		setInformation.SetName = "Red Husky Set";
		return setInformation;
	}

	public int DifficultyOfShieldCost(int p_difficulty)
	{
		switch (p_difficulty)
		{
		case 0:
			return GLOBAL.GAME_SETTINGS.event_easy_cost;
		case 1:
			return GLOBAL.GAME_SETTINGS.event_normal_cost;
		case 2:
			return GLOBAL.GAME_SETTINGS.event_hard_cost;
		default:
			return GLOBAL.GAME_SETTINGS.event_easy_cost;
		}
	}

	public void GenerateNewSessionID()
	{
		m_sessionRequestID = (uint)(UnityEngine.Random.value * 4.2949673E+09f);
	}
}
