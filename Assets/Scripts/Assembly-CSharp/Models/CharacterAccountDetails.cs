using System.Collections.Generic;
using UnityEngine;

namespace Models
{
	public class CharacterAccountDetails
	{
		public string id = string.Empty;

		public string nubee_id = string.Empty;

		public string fbid = string.Empty;

		public string wbid = string.Empty;

		public string name = string.Empty;

		public string title = string.Empty;

		public int level;

		public int exp;

		public float strength;

		public float agility;

		public float constitution;

		public string date_created = string.Empty;

		public string date_updated = string.Empty;

		public int max_inventory_slots;

		public int used_inventory_slots;

		public int max_stamina;

		public int max_shields;

		public int stamina;

		public int shields;

		public int timeTillNextStamina;

		public int timeTillNextShield;

		public int timeTillNextWarcry;

		public int totalNumberOfFriends;

		public int friendLimit = 1;

		public int pendingFriendRequest;

		public string activePet = string.Empty;

		public Dictionary<string, PetDetails> unlockedPets;

		public CurrencyDetails currencies = new CurrencyDetails();

		public CharacterEquipmentDetails[] equipment = new CharacterEquipmentDetails[5];

		public Dictionary<string, ServerInventoryItem> m_armorCollection = new Dictionary<string, ServerInventoryItem>();

		public int leaderboardRank = -1;

		public int battleScore;

		public bool isBanFromIAP;

		public CharacterAccountDetails()
		{
			for (int i = 0; i < 5; i++)
			{
				equipment[i] = new CharacterEquipmentDetails();
			}
		}

		public CharacterAccountDetails(CharacterAccountDetails p_rhs)
		{
			id = p_rhs.id;
			nubee_id = p_rhs.nubee_id;
			fbid = p_rhs.fbid;
			name = p_rhs.name;
			title = p_rhs.title;
			level = p_rhs.level;
			exp = p_rhs.exp;
			strength = p_rhs.strength;
			agility = p_rhs.agility;
			constitution = p_rhs.constitution;
			date_created = p_rhs.date_created;
			date_updated = p_rhs.date_updated;
			max_inventory_slots = p_rhs.max_inventory_slots;
			used_inventory_slots = p_rhs.used_inventory_slots;
			max_stamina = p_rhs.max_stamina;
			max_shields = p_rhs.max_shields;
			stamina = p_rhs.stamina;
			shields = p_rhs.shields;
			timeTillNextStamina = p_rhs.timeTillNextShield;
			timeTillNextShield = p_rhs.timeTillNextStamina;
			timeTillNextWarcry = p_rhs.timeTillNextWarcry;
			totalNumberOfFriends = p_rhs.totalNumberOfFriends;
			friendLimit = p_rhs.friendLimit;
			pendingFriendRequest = p_rhs.pendingFriendRequest;
			activePet = p_rhs.activePet;
			if (p_rhs.unlockedPets != null)
			{
				unlockedPets = new Dictionary<string, PetDetails>(p_rhs.unlockedPets);
			}
			currencies = new CurrencyDetails(p_rhs.currencies);
			m_armorCollection = new Dictionary<string, ServerInventoryItem>(p_rhs.m_armorCollection);
			leaderboardRank = p_rhs.leaderboardRank;
			isBanFromIAP = p_rhs.isBanFromIAP;
			if (p_rhs.equipment != null)
			{
				int num = 5;
				equipment = new CharacterEquipmentDetails[num];
				for (int i = 0; i < num; i++)
				{
					equipment[i] = new CharacterEquipmentDetails(p_rhs.equipment[i]);
				}
			}
		}

		public void ToPlayerData(ref PlayerData _playerData)
		{
			_playerData.m_name = name;
			_playerData.m_level = level;
			_playerData.m_title = title;
			_playerData.m_FriendLimit = friendLimit;
			_playerData.m_PendingFriendRequest = pendingFriendRequest;
			_playerData.m_strength = strength;
			_playerData.m_agility = agility;
			_playerData.m_constitution = constitution;
			UserData userData = _playerData.m_userData;
			userData.currentXp = exp;
			userData.nubeeId = nubee_id;
			userData.inventoryMaxSize = max_inventory_slots;
			userData.inventoryUsedSize = used_inventory_slots;
			userData.maxStamina = max_stamina;
			userData.maxShield = max_shields;
			userData.stamina = stamina;
			userData.shield = shields;
			userData.timeNextStamina = timeTillNextStamina;
			userData.timeNextShield = timeTillNextShield;
			userData.timeNextWarcry = timeTillNextWarcry;
			userData.totalFriends = totalNumberOfFriends;
			if (userData.totalFriends < 0)
			{
				userData.totalFriends = 0;
			}
			userData.socialCurrency = Mathf.FloorToInt(currencies.social);
			userData.premiumCurrency = Mathf.FloorToInt(currencies.premium);
			userData.inGameCurrency = Mathf.FloorToInt(currencies.ingame);
			userData.freePremiumGacha = Mathf.FloorToInt(currencies.freeGacha);
			userData.giftGacha = Mathf.FloorToInt(currencies.giftGacha);
			userData.freeSocialGacha = Mathf.FloorToInt(currencies.freeSocialGacha);
			userData.fbId = fbid;
			userData.isBanFromIAP = isBanFromIAP;
			if (_playerData.details == null)
			{
				return;
			}
			_playerData.details.currencies.dragonPoints = currencies.dragonPoints;
			_playerData.details.currencies.holyDiamonds = currencies.holyDiamonds;
			if (unlockedPets == null || unlockedPets.Count <= 0)
			{
				return;
			}
			_playerData.details.activePet = activePet;
			_playerData.details.unlockedPets = new Dictionary<string, PetDetails>();
			foreach (string key in unlockedPets.Keys)
			{
				if (!_playerData.details.unlockedPets.ContainsKey(key))
				{
					_playerData.details.unlockedPets.Add(key, unlockedPets[key]);
				}
			}
		}
	}
}
