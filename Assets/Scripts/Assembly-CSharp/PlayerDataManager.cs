using System;
using System.Collections;
using System.Collections.Generic;
using Models;
using UnityEngine;

public class PlayerDataManager : SingletonComponent<PlayerDataManager>
{
	public string m_currentPlayerId = "Unknown";

	private NBBinaryFormat m_monsterCsv;

	private Dictionary<string, DbPlayerDataStruct> m_monstersMap = new Dictionary<string, DbPlayerDataStruct>();

	private DbPlayerDataStruct m_player;

	private DbPlayerDataStruct[] m_friends;

	private bool m_isMonsterLoaded;

	private PlayerData m_hero = new PlayerData();

	private PlayerData m_temporaryHero = new PlayerData();

	private PlayerData m_opponent;

	private Dictionary<int, PlayerData> m_allies = new Dictionary<int, PlayerData>();

	public bool IsMonsterLoaded
	{
		get
		{
			return m_isMonsterLoaded;
		}
	}

	public PlayerData GetPlayerData()
	{
		return m_hero;
	}

	public PlayerData GetCopyOfPlayerData()
	{
		return new PlayerData(m_hero);
	}

	public PlayerData GetTemporaryPlayerData()
	{
		return m_temporaryHero;
	}

	public void SetPlayerData(CharacterAccountDetails _accountDetails)
	{
		if (_accountDetails != null)
		{
			Dbg.Log("_accountDetails.activePet: " + _accountDetails.activePet);
			if (_accountDetails.unlockedPets != null)
			{
				Dbg.Log("_accountDetails.unlockedPets.Count: " + _accountDetails.unlockedPets.Count);
			}
			_accountDetails.ToPlayerData(ref m_hero);
			_accountDetails.ToPlayerData(ref m_temporaryHero);
			m_hero.SetAccountDetails(_accountDetails);
			m_temporaryHero.SetAccountDetails(_accountDetails);
			m_hero.CharacterType = CharacterType.MainCharacter;
			m_temporaryHero.CharacterType = CharacterType.MainCharacter;
		}
	}

	public PlayerData GetOpponentData()
	{
		if (m_opponent == null)
		{
			Dbg.LogError("No data set for opponent.");
		}
		return m_opponent;
	}

	public void SetOpponentData(DbPlayerDataStruct p_opponent)
	{
		if (p_opponent == null)
		{
			Dbg.LogError("You must specify a player data struct for the opponent!");
		}
		else
		{
			PlayerStructToData(p_opponent, m_opponent);
		}
	}

	public PlayerData GetAllyData(int p_key)
	{
		PlayerData value = null;
		if (!m_allies.TryGetValue(p_key, out value))
		{
			Dbg.LogError("No data set for ally: " + p_key + ".");
			return null;
		}
		Dbg.Log("PLAYER_DATA_MANAGER : Get Ally : " + p_key + "\n" + value, Dbg.NBLogType.COMMON_WARNING);
		return value;
	}

	private void SetAllyData(PlayerData _playerData, int _index)
	{
		if (_playerData == null)
		{
			Dbg.LogWarn("PLAYER_DATA : player data for the ally is null! Create a dummy for index : " + _index);
			_playerData = new PlayerData();
		}
		Dbg.Log("PLAYER_DATA_MANAGER : Set Ally : " + _index + "\n" + _playerData);
		m_allies.Remove(_index);
		m_allies.Add(_index, _playerData);
	}

	public DbPlayerDataStruct GetPlayerDbData()
	{
		if (m_player == null)
		{
			Dbg.LogError("No data set to current player.");
			return null;
		}
		return m_player;
	}

	public DbPlayerDataStruct GetMonsterDbData(string p_monsterId)
	{
		DbPlayerDataStruct value = null;
		if (!m_monstersMap.TryGetValue(p_monsterId, out value))
		{
			Dbg.LogWarn(string.Concat("There is no data retrieved from ", m_monsterCsv, " with id: ", p_monsterId));
			return null;
		}
		return value;
	}

	public DbPlayerDataStruct GetFriendDbData(int p_index)
	{
		if (m_friends[p_index] == null)
		{
			Dbg.LogError("No data set to friend " + p_index + ".");
			return null;
		}
		return m_friends[p_index];
	}

	public int QueryMonsters(ArrayList p_list, int p_difficulty, Element.Type p_element)
	{
		if (p_list == null)
		{
			p_list = new ArrayList();
		}
		else if (p_list.Count > 0)
		{
			Dbg.LogWarn("The results will be appended on the supplied array.");
		}
		int num = 0;
		foreach (DbPlayerDataStruct value in m_monstersMap.Values)
		{
			if (value.difficulty == p_difficulty && value.element == p_element)
			{
				p_list.Add(value);
				num++;
			}
		}
		return num;
	}

	public int QueryMonstersExceptSpecies(ArrayList p_list, Element.Type p_element, MonsterSpecies p_species)
	{
		if (p_list == null)
		{
			p_list = new ArrayList();
		}
		else if (p_list.Count > 0)
		{
			Dbg.LogWarn("The results will be appended on the supplied array.");
		}
		int num = 0;
		foreach (DbPlayerDataStruct value in m_monstersMap.Values)
		{
			if (value.element == p_element && value.species != p_species)
			{
				p_list.Add(value);
				num++;
			}
		}
		return num;
	}

	public int FilterEqualLevel(ArrayList p_list, int p_level)
	{
		int num = p_list.Count;
		for (int num2 = p_list.Count - 1; num2 >= 0; num2--)
		{
			DbPlayerDataStruct dbPlayerDataStruct = p_list[num2] as DbPlayerDataStruct;
			if (dbPlayerDataStruct.level == p_level)
			{
				p_list.RemoveAt(num2);
				num--;
			}
		}
		return num;
	}

	public int FilterTrimBoss(ArrayList p_list)
	{
		int num = p_list.Count;
		for (int num2 = p_list.Count - 1; num2 >= 0; num2--)
		{
			DbPlayerDataStruct dbPlayerDataStruct = p_list[num2] as DbPlayerDataStruct;
			if (dbPlayerDataStruct.isBoss)
			{
				p_list.RemoveAt(num2);
				num--;
			}
		}
		return num;
	}

	public int FilterExceptLevel(ArrayList p_list, int p_level)
	{
		int num = p_list.Count;
		for (int num2 = p_list.Count - 1; num2 >= 0; num2--)
		{
			DbPlayerDataStruct dbPlayerDataStruct = p_list[num2] as DbPlayerDataStruct;
			if (dbPlayerDataStruct.level != p_level)
			{
				p_list.RemoveAt(num2);
				num--;
			}
		}
		return num;
	}

	public void LoadMonsterDBForTutorial()
	{
		NBProfiler.BeginTask("[LOAD_TUTORIAL_MONSTER_DB]");
		if (m_monsterCsv == null)
		{
			Dbg.LogError("Problem in csv reader.");
			return;
		}
		m_monstersMap.Clear();
		List<string> list = m_monsterCsv.RowNameWithPattern("TUTORIAL");
		foreach (string item in list)
		{
			DbPlayerDataStruct p_monsterData = new DbPlayerDataStruct();
			FillMonsterData(ref p_monsterData, item);
			m_monstersMap.Add(p_monsterData.id, p_monsterData);
		}
		NBProfiler.EndTask("[LOAD_TUTORIAL_MONSTER_DB]");
	}

	public void LoadMonsterForDungeonEvents(List<string> p_monsterIDs)
	{
		NBProfiler.BeginTask("[LOAD_MONSTERS_DUNGEON_DB]");
		if (m_monsterCsv == null)
		{
			Dbg.LogError("Problem in csv reader.");
			return;
		}
		int num = 0;
		List<string> rowsNames = m_monsterCsv.RowsNames;
		foreach (string p_monsterID in p_monsterIDs)
		{
			if (m_monstersMap.ContainsKey(p_monsterID))
			{
				num++;
				continue;
			}
			if (rowsNames.Contains(p_monsterID))
			{
				DbPlayerDataStruct p_monsterData = new DbPlayerDataStruct();
				FillMonsterData(ref p_monsterData, p_monsterID);
				m_monstersMap.Add(p_monsterData.id, p_monsterData);
				num++;
			}
			else
			{
				bool flag = false;
				int num2 = p_monsterID.LastIndexOf('_');
				if (0 <= num2 && num2 < p_monsterID.Length)
				{
					string s = p_monsterID.Substring(num2 + 1);
					string text = p_monsterID.Substring(0, num2 + 1);
					try
					{
						int num3 = int.Parse(s);
						string text2 = text;
						int num4 = -1;
						foreach (string item in rowsNames)
						{
							if (item.StartsWith(text))
							{
								text2 = text;
								break;
							}
							int num5 = 0;
							int i = 0;
							for (int num6 = Mathf.Min(text.Length, item.Length); i < num6; i++)
							{
								if (item[i] == text[i])
								{
									num5++;
								}
							}
							if (num4 < num5)
							{
								text2 = item;
								num4 = num5;
							}
						}
						if (text2 != text)
						{
							int num7 = text2.LastIndexOf('_');
							text = text2.Substring(0, num7 + 1);
							num3++;
						}
						while (0 < num3--)
						{
							string text3 = text + num3;
							if (rowsNames.Contains(text3))
							{
								DbPlayerDataStruct p_monsterData2 = new DbPlayerDataStruct();
								FillMonsterData(ref p_monsterData2, text3);
								m_monstersMap.Add(p_monsterID, p_monsterData2);
								num++;
								flag = true;
								break;
							}
						}
					}
					catch (Exception)
					{
					}
				}
				if (!flag)
				{
					string p_rowName = rowsNames[0];
					DbPlayerDataStruct p_monsterData3 = new DbPlayerDataStruct();
					FillMonsterData(ref p_monsterData3, p_rowName);
					m_monstersMap.Add(p_monsterID, p_monsterData3);
					num++;
				}
			}
			if (num < p_monsterIDs.Count)
			{
				continue;
			}
			break;
		}
		NBProfiler.EndTask("[LOAD_MONSTERS_DUNGEON_DB]");
	}

	private void FillMonsterData(ref DbPlayerDataStruct p_monsterData, string p_rowName)
	{
		if (m_monsterCsv == null || p_monsterData == null)
		{
			return;
		}
		p_monsterData.isMonster = true;
		p_monsterData.id = p_rowName;
		p_monsterData.name = m_monsterCsv.GetData(p_rowName, "MONSTER_NAME");
		p_monsterData.difficulty = m_monsterCsv.GetDataGeneric<int>(p_rowName, "DIFFICULTY");
		p_monsterData.isBoss = m_monsterCsv.GetDataGeneric<int>(p_rowName, "BOSS") == 1;
		p_monsterData.level = m_monsterCsv.GetDataGeneric<int>(p_rowName, "LEVEL");
		p_monsterData.strength = m_monsterCsv.GetDataGeneric<int>(p_rowName, "STRENGTH");
		p_monsterData.constitution = m_monsterCsv.GetDataGeneric<int>(p_rowName, "CONSTITUTION");
		p_monsterData.agility = m_monsterCsv.GetDataGeneric<int>(p_rowName, "AGILITY");
		p_monsterData.species = SpeciesParser.Parse(m_monsterCsv.GetData(p_rowName, "SPECIES"));
		p_monsterData.rewardExperience = m_monsterCsv.GetDataGeneric<int>(p_rowName, "REWARD_EXPERIENCE");
		p_monsterData.element = Element.FromString(m_monsterCsv.GetData(p_rowName, "ELEMENT"));
		p_monsterData.IdleBeforeAttack = m_monsterCsv.GetDataGeneric<int>(p_rowName, "IDLE_BEFORE_ATTACK");
		p_monsterData.AttackAnimSpeed = m_monsterCsv.GetDataGeneric<float>(p_rowName, "ATTACK_SPEED_MODIFIER");
		for (int i = 1; i <= p_monsterData.advanceAtkAbility.Length; i++)
		{
			if (m_monsterCsv.GetDataGeneric<int>(p_rowName, "A_Attack_" + i) == 0)
			{
				p_monsterData.advanceAtkAbility[i - 1] = false;
			}
			else
			{
				p_monsterData.advanceAtkAbility[i - 1] = true;
			}
		}
		for (int j = 1; j <= p_monsterData.specialAtkAbility.Length; j++)
		{
			if (m_monsterCsv.GetDataGeneric<int>(p_rowName, "SP_Attack_" + j) == 0)
			{
				p_monsterData.specialAtkAbility[j - 1] = false;
			}
			else
			{
				p_monsterData.specialAtkAbility[j - 1] = true;
			}
		}
		string[] dataAsStringArray = m_monsterCsv.GetDataAsStringArray(p_rowName, "RESISTANCES");
		for (int k = 0; k < p_monsterData.resistances.Length; k++)
		{
			if (!float.TryParse(dataAsStringArray[k], out p_monsterData.resistances[k]))
			{
				p_monsterData.resistances[k] = 1f;
			}
			if (p_monsterData.resistances[k] <= 0f)
			{
				p_monsterData.resistances[k] = 1f;
			}
		}
	}

	public IEnumerator RetrieveMonsterDbDataFromDatabase()
	{
		if (m_isMonsterLoaded)
		{
			Dbg.LogWarn("Monster Data already loaded.");
			yield break;
		}
		m_isMonsterLoaded = false;
		NBProfiler.BeginTask("[LOAD_MONSTER_DB]");
		if (m_monsterCsv == null)
		{
			Dbg.LogError("Problem in csv reader.");
			yield break;
		}
		List<string> rows = m_monsterCsv.RowsNames;
		foreach (string row in rows)
		{
			if (!m_monstersMap.ContainsKey(row))
			{
				DbPlayerDataStruct monsterData = new DbPlayerDataStruct();
				FillMonsterData(ref monsterData, row);
				m_monstersMap.Add(monsterData.id, monsterData);
			}
		}
		SingletonComponent<ParameterManager>.Instance.UnloadParametersFor("Monsters");
		m_monsterCsv = null;
		m_isMonsterLoaded = true;
		NBProfiler.EndTask("[LOAD_MONSTER_DB]");
	}

	public void SetFriendDataFromServer(PlayerData _friendData1, PlayerData _friendData2)
	{
		if (_friendData1 == null)
		{
			Dbg.LogWarn("PLAYER_DATA_MANAGER : This friend 1 is null.");
		}
		if (_friendData2 == null)
		{
			Dbg.LogWarn("PLAYER_DATA_MANAGER : This friend 2 is null.");
		}
		SetAllyData(_friendData1, 0);
		SetAllyData(_friendData2, 1);
	}

	public void InitManager()
	{
		NBProfiler.BeginTask("[PLAYER_DATA_MANAGER][INIT_MANAGER]");
		m_opponent = new PlayerData();
		m_allies.Add(0, new PlayerData());
		m_allies.Add(1, new PlayerData());
		m_monsterCsv = SingletonComponent<ParameterManager>.Instance.GetParametersFor("Monsters");
		NBProfiler.EndTask("[PLAYER_DATA_MANAGER][INIT_MANAGER]");
	}

	public void Load()
	{
		m_hero.Name = MBPrefs.GetString("PlayerDataManager.Player.m_name");
		m_hero.Level = MBPrefs.GetInt("PlayerDataManager.Player.m_level");
		m_hero.m_agility = MBPrefs.GetFloat("PlayerDataManager.Player.m_agility");
		m_hero.m_constitution = MBPrefs.GetFloat("PlayerDataManager.Player.m_constitution");
		m_hero.m_strength = MBPrefs.GetFloat("PlayerDataManager.Player.m_strength");
		int num = 0;
		m_hero.m_isMonster = MBPrefs.GetInt("PlayerDataManager.Player.m_isMonster") > 0;
		m_hero.m_userData.currentXp = MBPrefs.GetInt("PlayerDataManager.Player.m_userData.currentXp");
		m_hero.m_userData.inGameCurrency = MBPrefs.GetInt("PlayerDataManager.Player.m_userData.inGameCurrency");
		m_hero.m_userData.maxStamina = MBPrefs.GetInt("PlayerDataManager.Player.m_userData.maxStamina");
		m_hero.m_userData.stamina = MBPrefs.GetInt("PlayerDataManager.Player.m_userData.stamina");
		if (m_hero.m_userData.stamina > m_hero.m_userData.maxStamina)
		{
			Dbg.LogError("PlayerDataManager.Load : Current Stamina Higher than Max Stamina. Cap the current stamina.");
			m_hero.m_userData.stamina = m_hero.m_userData.maxStamina;
		}
		m_hero.m_targetXp = MBPrefs.GetInt("PlayerDataManager.Player.m_targetXp");
		int num2 = MBPrefs.GetInt("PlayerDataManager.Player.m_equips.Length");
		m_hero.m_equips = new Equipment.Data[num2];
		for (num = 0; num < num2; num++)
		{
			m_hero.m_equips[num] = new Equipment.Data();
			m_hero.m_equips[num].id = MBPrefs.GetString("PlayerDataManager.Player.m_equips[" + num + "]");
			m_hero.m_equips[num].lifeMaxMod = 0f;
			m_hero.m_equips[num].maxDamageMod = 0f;
			m_hero.m_equips[num].minDamageMod = 0f;
			m_hero.m_equips[num].armorMod = 0f;
			m_hero.m_equips[num].element = Element.Type.Metal;
		}
		m_hero.ComputeFinalValues();
		m_hero.ResetHp();
	}

	public void Save()
	{
		MBPrefs.SetString("PlayerDataManager.Player.m_name", m_hero.Name);
		MBPrefs.SetInt("PlayerDataManager.Player.m_level", m_hero.Level);
		MBPrefs.SetFloat("PlayerDataManager.Player.m_agility", m_hero.Agi);
		MBPrefs.SetFloat("PlayerDataManager.Player.m_constitution", m_hero.Con);
		MBPrefs.SetFloat("PlayerDataManager.Player.m_strength", m_hero.Str);
		MBPrefs.SetInt("PlayerDataManager.Player.m_isMonster", m_hero.m_isMonster ? 1 : 0);
		MBPrefs.SetInt("PlayerDataManager.Player.m_userData.currentXp", m_hero.m_userData.currentXp);
		MBPrefs.SetInt("PlayerDataManager.Player.m_userData.inGameCurrency", m_hero.m_userData.inGameCurrency);
		MBPrefs.SetInt("PlayerDataManager.Player.m_userData.maxStamina", m_hero.m_userData.maxStamina);
		MBPrefs.SetInt("PlayerDataManager.Player.m_userData.stamina", m_hero.m_userData.stamina);
		MBPrefs.SetInt("PlayerDataManager.Player.m_targetXp", m_hero.m_targetXp);
		MBPrefs.SetInt("PlayerDataManager.Player.m_equips.Length", m_hero.m_equips.Length);
		int num = m_hero.m_equips.Length;
		int num2 = 0;
		for (num2 = 0; num2 < num; num2++)
		{
			MBPrefs.SetString("PlayerDataManager.Player.m_equips[" + num2 + "]", m_hero.m_equips[num2].id);
		}
	}

	public void PlayerDataToStruct(PlayerData p_playerData, DbPlayerDataStruct p_playerStruct)
	{
		p_playerStruct.isMonster = p_playerData.m_isMonster;
		p_playerStruct.characterType = p_playerData.CharacterType;
		p_playerStruct.level = p_playerData.m_level;
		p_playerStruct.agility = p_playerData.m_agility;
		p_playerStruct.constitution = p_playerData.m_constitution;
		p_playerStruct.strength = p_playerData.m_strength;
		if (p_playerStruct.isMonster)
		{
			p_playerStruct.isBoss = p_playerData.m_monsterData.isBoss;
			p_playerStruct.element = p_playerData.m_monsterData.element;
			p_playerStruct.IdleBeforeAttack = p_playerData.m_monsterData.idleBeforeAttack;
			p_playerStruct.AttackAnimSpeed = p_playerData.m_monsterData.attackAnimSpeed;
			for (int i = 0; i < p_playerStruct.advanceAtkAbility.Length; i++)
			{
				p_playerStruct.advanceAtkAbility[i] = p_playerData.m_monsterData.advanceAtkAbility[i];
			}
			for (int j = 0; j < p_playerStruct.specialAtkAbility.Length; j++)
			{
				p_playerStruct.specialAtkAbility[j] = p_playerData.m_monsterData.specialAtkAbility[j];
			}
			for (int k = 0; k < p_playerStruct.resistances.Length; k++)
			{
				p_playerStruct.resistances[k] = p_playerData.m_monsterData.resistances[k];
			}
		}
	}

	public void PlayerStructToData(DbPlayerDataStruct p_playerStruct, PlayerData p_playerData)
	{
		p_playerData.m_isMonster = p_playerStruct.isMonster;
		p_playerData.CharacterType = p_playerStruct.characterType;
		p_playerData.m_level = p_playerStruct.level;
		p_playerData.m_agility = p_playerStruct.agility;
		p_playerData.m_constitution = p_playerStruct.constitution;
		p_playerData.m_strength = p_playerStruct.strength;
		if (p_playerStruct.isMonster)
		{
			p_playerData.m_monsterData.monsterId = p_playerStruct.id;
			p_playerData.m_monsterData.level = p_playerStruct.level;
			p_playerData.m_monsterData.isBoss = p_playerStruct.isBoss;
			p_playerData.m_monsterData.element = p_playerStruct.element;
			p_playerData.m_monsterData.idleBeforeAttack = p_playerStruct.IdleBeforeAttack;
			p_playerData.m_monsterData.attackAnimSpeed = p_playerStruct.AttackAnimSpeed;
			p_playerData.m_monsterData.difficulty = p_playerStruct.difficulty;
			for (int i = 0; i < p_playerStruct.advanceAtkAbility.Length; i++)
			{
				p_playerData.m_monsterData.advanceAtkAbility[i] = p_playerStruct.advanceAtkAbility[i];
			}
			for (int j = 0; j < p_playerStruct.specialAtkAbility.Length; j++)
			{
				p_playerData.m_monsterData.specialAtkAbility[j] = p_playerStruct.specialAtkAbility[j];
			}
			for (int k = 0; k < p_playerStruct.resistances.Length; k++)
			{
				p_playerData.m_monsterData.resistances[k] = p_playerStruct.resistances[k];
			}
		}
		p_playerData.m_userData.maxStamina = p_playerStruct.maxStamina;
		p_playerData.m_userData.stamina = p_playerStruct.stamina;
		p_playerData.m_userData.shield = p_playerStruct.shield;
		p_playerData.m_userData.premiumCurrency = (int)p_playerStruct.premiumCurrency;
		p_playerData.m_userData.socialCurrency = (int)p_playerStruct.socialCurrency;
		p_playerData.m_userData.inGameCurrency = (int)p_playerStruct.ingameCurrency;
		p_playerData.m_monsterData.species = p_playerStruct.species;
	}

	public void SetCurrency(CurrencyDetails _details)
	{
		m_hero.m_userData.inGameCurrency = (int)_details.ingame;
		m_hero.m_userData.premiumCurrency = (int)_details.premium;
		m_hero.m_userData.socialCurrency = (int)_details.social;
		m_hero.m_userData.freePremiumGacha = (int)_details.freeGacha;
		m_hero.m_userData.freeSocialGacha = (int)_details.freeSocialGacha;
		m_hero.m_userData.giftGacha = (int)_details.giftGacha;
		m_hero.details.currencies.dragonPoints = _details.dragonPoints;
		m_hero.details.currencies.holyDiamonds = _details.holyDiamonds;
		m_hero.details.currencies.ingame = _details.ingame;
		m_hero.details.currencies.premium = _details.premium;
		m_hero.details.currencies.social = _details.social;
		m_hero.details.currencies.freeGacha = _details.freeGacha;
		m_hero.details.currencies.freeSocialGacha = _details.freeSocialGacha;
		m_hero.details.currencies.giftGacha = _details.giftGacha;
	}

	public CurrencyDetails GetCurrency()
	{
		if (m_hero == null || m_hero.details == null)
		{
			return null;
		}
		return m_hero.details.currencies;
	}

	private DbPlayerDataStruct PrepareDefaultPlayer()
	{
		DbPlayerDataStruct dbPlayerDataStruct = new DbPlayerDataStruct();
		dbPlayerDataStruct.level = 1;
		dbPlayerDataStruct.agility = 5f;
		dbPlayerDataStruct.constitution = 240f;
		dbPlayerDataStruct.strength = 40f;
		dbPlayerDataStruct.configFile = "PlayerCharacter/Male_anim";
		dbPlayerDataStruct.isMonster = false;
		dbPlayerDataStruct.characterType = CharacterType.Stranger;
		return dbPlayerDataStruct;
	}

	private PlayerAttack.Direction StringToAttackDirection(string p_string)
	{
		switch (p_string)
		{
		case "TopLeft":
			return PlayerAttack.Direction.TopLeftToBottomRight;
		case "Top":
			return PlayerAttack.Direction.TopToBottom;
		case "TopRight":
			return PlayerAttack.Direction.TopRightToBottomLeft;
		case "Left":
			return PlayerAttack.Direction.LeftToRight;
		case "Right":
			return PlayerAttack.Direction.RightToLeft;
		case "BottomLeft":
			return PlayerAttack.Direction.BottomLeftToTopRight;
		case "Bottom":
			return PlayerAttack.Direction.BottomToTop;
		case "BottomRight":
			return PlayerAttack.Direction.BottomRightToTopLeft;
		default:
			return PlayerAttack.Direction.Max;
		}
	}
}
