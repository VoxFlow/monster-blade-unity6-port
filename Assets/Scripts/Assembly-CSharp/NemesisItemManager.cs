using System;
using UnityEngine;

public class NemesisItemManager : SingletonComponent<NemesisItemManager>
{
	private struct PlayerEquipmentData
	{
		public string id;

		public int tier;
	}

	private static readonly string s_databaseName = "Nemesis_Items";

	private PlayerData m_playerData;

	private PlayerEquipmentData[] m_equipments;

	private static readonly float defaultDmgBoost;

	private static readonly float defaultDmgReduction = 1f;

	private float m_damageBoost = defaultDmgBoost;

	private float m_damageReduction = defaultDmgReduction;

	private void Awake()
	{
		PlayerDataManager instance = SingletonComponent<PlayerDataManager>.Instance;
		if (instance == null)
		{
			Dbg.LogWarn("NemeisItemManager.Awake(): Singletons or player data weren't initialized.");
			return;
		}
		m_playerData = instance.GetPlayerData();
		Equipment.Data[] equipments = m_playerData.Equipments;
		if (equipments == null)
		{
			Dbg.LogWarn("NemeisItemManager.Awake(): Player equipments' data is null.");
			return;
		}
		m_equipments = new PlayerEquipmentData[equipments.Length];
		for (int i = 0; i < m_equipments.Length; i++)
		{
			if (equipments[i].itemDetails == null)
			{
				Dbg.LogWarn("No itemDetails attach to this equipment.");
				continue;
			}
			string itemID = equipments[i].itemDetails.ItemID;
			int num = itemID.LastIndexOf('_');
			m_equipments[i].id = ((num <= 0) ? itemID : itemID.Substring(0, num));
			m_equipments[i].tier = equipments[i].itemDetails.Tier;
		}
		Messenger.AddListener("battle_start", OnBattleStart);
		Messenger.AddListener("battle_end", OnBattleEnd);
		Messenger<PlayerAttack>.AddListener("player_begin_atk", OnPlayerAttack);
		Messenger<MonsterAttack>.AddListener("monster_begin_atk", OnMonsterAttack);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Messenger.RemoveListener("battle_start", OnBattleStart);
		Messenger.RemoveListener("battle_end", OnBattleEnd);
		Messenger<PlayerAttack>.RemoveListener("player_begin_atk", OnPlayerAttack);
		Messenger<MonsterAttack>.RemoveListener("monster_begin_atk", OnMonsterAttack);
	}

	public static NemesisItemData GetItemData(string p_itemID)
	{
		NemesisItemData nemesisItemData = null;
		ParameterManager instance = SingletonComponent<ParameterManager>.Instance;
		NBBinaryFormat parametersFor = instance.GetParametersFor(s_databaseName);
		if (parametersFor != null && parametersFor.ContainsRow(p_itemID))
		{
			nemesisItemData = new NemesisItemData();
			nemesisItemData.name = p_itemID;
			nemesisItemData.tier = parametersFor.GetDataGeneric<int>(p_itemID, "TIER");
			nemesisItemData.enemies = parametersFor.GetDataAsStringArray(p_itemID, "NEMESIS_ENEMY");
			NBGeneric.TryParse<float>(parametersFor.GetData(p_itemID, "DAMAGE_BOOST"), out nemesisItemData.damageBoost);
			NBGeneric.TryParse<float>(parametersFor.GetData(p_itemID, "DAMAGE_REDUCTION"), out nemesisItemData.damageReduction);
			nemesisItemData.damageBoost = Mathf.Max(0f, nemesisItemData.damageBoost);
			nemesisItemData.damageReduction = Mathf.Max(0f, nemesisItemData.damageReduction);
			int p_outResult = 0;
			NBGeneric.TryParse<int>(parametersFor.GetData(p_itemID, "EVENT_ONLY"), out p_outResult);
			nemesisItemData.eventOnly = p_outResult == 1;
		}
		return nemesisItemData;
	}

	private void OnBattleStart()
	{
		DungeonManager instance = SingletonComponent<DungeonManager>.Instance;
		PlayerDataManager instance2 = SingletonComponent<PlayerDataManager>.Instance;
		if (m_equipments == null || instance == null || instance2 == null)
		{
			return;
		}
		BattleStruct currentBattle = instance.GetCurrentBattle();
		if (currentBattle == null)
		{
			return;
		}
		DbPlayerDataStruct monsterDbData = instance2.GetMonsterDbData(currentBattle.monsterId);
		bool flag = !instance.IsPlayingEventDungeon;
		ResetAttributes();
		if (monsterDbData == null)
		{
			return;
		}
		for (int i = 0; i < m_equipments.Length; i++)
		{
			NemesisItemData itemData = GetItemData(m_equipments[i].id);
			if (itemData != null && itemData.tier == m_equipments[i].tier && Array.IndexOf(itemData.enemies, monsterDbData.name) >= 0 && (!flag || !itemData.eventOnly))
			{
				m_damageBoost += itemData.damageBoost;
				m_damageReduction += itemData.damageReduction;
			}
		}
		if (!m_damageReduction.Equals(defaultDmgReduction))
		{
			m_damageReduction = 1f / m_damageReduction;
		}
	}

	private void OnBattleEnd()
	{
		ResetAttributes();
	}

	private void OnPlayerAttack(PlayerAttack p_atk)
	{
		if (!Mathf.Approximately(m_damageBoost, defaultDmgBoost))
		{
			p_atk.multiplier *= m_damageBoost;
		}
	}

	private void OnMonsterAttack(MonsterAttack p_atk)
	{
		if (!Mathf.Approximately(m_damageReduction, defaultDmgReduction))
		{
			p_atk.multiplier *= m_damageReduction;
		}
	}

	private void ResetAttributes()
	{
		m_damageBoost = defaultDmgBoost;
		m_damageReduction = defaultDmgReduction;
	}
}
