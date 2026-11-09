using System;
using System.Collections.Generic;
using Models;
using ServiceCalls;
using UnityEngine;

public class SocialManager : SingletonComponent<SocialManager>
{
	protected class CallbackPairs
	{
		public SocialCallback success;

		public SocialCallback fail;
	}

	public delegate void SocialCallback(ServiceResponseCodes p_code);

	protected Dictionary<Services.ServiceHandle, CallbackPairs> m_handles = new Dictionary<Services.ServiceHandle, CallbackPairs>();

	protected List<FriendAccountDetails> m_list = new List<FriendAccountDetails>();

	protected List<FriendAccountDetails> m_listPending = new List<FriendAccountDetails>();

	protected List<FriendAccountDetails> m_listStrangers = new List<FriendAccountDetails>();

	private string m_friendOp;

	private SocialCallback m_FriendRequest;

	private SocialCallback m_loginCallback;

	private bool m_warcryAbility = true;

	public bool WarCryAbility
	{
		get
		{
			return m_warcryAbility;
		}
	}

	public void RefreshFriendsList(int p_offset, SocialCallback p_success, SocialCallback p_fail)
	{
		Services.ServiceHandle friendsList = SingletonComponent<Services>.Instance.GetFriendsList(SingletonComponent<Services>.Instance.NubeeId, SingletonComponent<Services>.Instance.DeviceId, p_offset, InternalRefreshFriendsListSuccess, InternalServiceCallFailCb);
		CallbackPairs callbackPairs = new CallbackPairs();
		callbackPairs.success = p_success;
		callbackPairs.fail = p_fail;
		m_handles.Add(friendsList, callbackPairs);
	}

	public void SaveGiftSenders(JSONObject p_payload)
	{
		if (p_payload == null || p_payload.type == JSONObject.Type.NULL || p_payload.Count < 0 || p_payload.IsEmptyArray())
		{
			Dbg.LogWarn("[SocialManager::SaveGiftSenders] invalid payload or empty gift senders.");
			return;
		}
		List<string> list = new List<string>();
		foreach (JSONObject item in p_payload.list)
		{
			list.Add(item.str);
		}
		PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		playerData.m_userData.gifts.Clear();
		playerData.m_userData.gifts.AddRange(list);
		int num = MBPrefs.GetInt("PlayerDataManager.gifts.Count");
		foreach (string gift in playerData.m_userData.gifts)
		{
			MBPrefs.SetString("PlayerDataManager.gifts[" + num + "]", gift);
			num++;
		}
		MBPrefs.SetInt("PlayerDataManager.gifts.Count", num);
		MBPrefs.Save();
		Dbg.LogWarn("Gift senders =======> " + playerData.m_userData.gifts.Count);
	}

	private void LoginCallback(bool bSuccessful)
	{
		Dbg.Log("SNS LoginCallback: " + bSuccessful);
		SocialNetworkManager.OnLoginCallback -= LoginCallback;
		bSuccessful &= NubeePlatformBinding.IsLoggedInToFacebook();
		if (bSuccessful)
		{
			NubeePlatformBinding.TryGetFacebookFriendsWithoutAccessToken();
			NubeePlatformManager.OnFacebookGetFriendsWithoutAccessTokenDidComplete += GetFriendsWithoutAccessDidComplete;
			NubeePlatformManager.OnFacebookGetFriendsWithoutAccessTokenDidNotComplete += GetFriendsWithoutAccessDidNotComplete;
			NubeePlatformManager.OnFacebookInviteFriendsDidComplete += InviteFacebookFriendsDidComplete;
			NubeePlatformManager.OnFacebookInviteFriendsDidNotComplete += InviteFacebookFriendsDidNotComplete;
			if (m_loginCallback != null)
			{
				SocialCallback loginCallback = m_loginCallback;
				m_loginCallback = null;
				loginCallback(ServiceResponseCodes.SUCCESS);
			}
		}
		else if (m_loginCallback != null)
		{
			SocialCallback loginCallback2 = m_loginCallback;
			m_loginCallback = null;
			loginCallback2(ServiceResponseCodes.FAILED);
		}
	}

	private void GetFriendsWithoutAccessDidComplete(int nCount)
	{
		Dbg.Log("[SocialManager::GetFriendsWithoutAccessDidComplete]");
		NubeePlatformManager.OnFacebookGetFriendsWithoutAccessTokenDidComplete -= GetFriendsWithoutAccessDidComplete;
		NubeePlatformManager.OnFacebookGetFriendsWithoutAccessTokenDidNotComplete -= GetFriendsWithoutAccessDidNotComplete;
	}

	private void GetFriendsWithoutAccessDidNotComplete(string szReason)
	{
		Dbg.Log("[SocialManager::GetFriendsWithoutAccessDidNotComplete]");
		NubeePlatformManager.OnFacebookGetFriendsWithoutAccessTokenDidComplete -= GetFriendsWithoutAccessDidComplete;
		NubeePlatformManager.OnFacebookGetFriendsWithoutAccessTokenDidNotComplete -= GetFriendsWithoutAccessDidNotComplete;
		NubeePlatformManager.OnFacebookInviteFriendsDidComplete -= InviteFacebookFriendsDidComplete;
		NubeePlatformManager.OnFacebookInviteFriendsDidNotComplete -= InviteFacebookFriendsDidNotComplete;
		if (m_FriendRequest != null)
		{
			SocialCallback friendRequest = m_FriendRequest;
			m_FriendRequest = null;
			friendRequest(ServiceResponseCodes.FAILED);
		}
	}

	private void InviteFacebookFriendsDidComplete()
	{
		Dbg.Log("[SocialManager::InviteFacebookFriendsDidComplete]");
		NubeePlatformManager.OnFacebookInviteFriendsDidComplete -= InviteFacebookFriendsDidComplete;
		NubeePlatformManager.OnFacebookInviteFriendsDidNotComplete -= InviteFacebookFriendsDidNotComplete;
		if (m_FriendRequest != null)
		{
			SocialCallback friendRequest = m_FriendRequest;
			m_FriendRequest = null;
			friendRequest(ServiceResponseCodes.SUCCESS);
		}
	}

	private void InviteFacebookFriendsDidNotComplete()
	{
		Dbg.Log("[SocialManager::InviteFacebookFriendsDidNotComplete]");
		NubeePlatformManager.OnFacebookInviteFriendsDidComplete -= InviteFacebookFriendsDidComplete;
		NubeePlatformManager.OnFacebookInviteFriendsDidNotComplete -= InviteFacebookFriendsDidNotComplete;
		if (m_FriendRequest != null)
		{
			SocialCallback friendRequest = m_FriendRequest;
			m_FriendRequest = null;
			friendRequest(ServiceResponseCodes.FAILED);
		}
	}

	public void InviteFriend(string p_nubeeId, SocialCallback p_LoginCallback, SocialCallback p_InviteFriendCallback)
	{
		Dbg.Log("InviteFriend. Nubee ID: " + p_nubeeId);
		SocialNetworkManager.OnLoginCallback -= LoginCallback;
		NubeePlatformManager.OnFacebookGetFriendsWithoutAccessTokenDidComplete -= GetFriendsWithoutAccessDidComplete;
		NubeePlatformManager.OnFacebookGetFriendsWithoutAccessTokenDidNotComplete -= GetFriendsWithoutAccessDidNotComplete;
		NubeePlatformManager.OnFacebookInviteFriendsDidComplete -= InviteFacebookFriendsDidComplete;
		NubeePlatformManager.OnFacebookInviteFriendsDidNotComplete -= InviteFacebookFriendsDidNotComplete;
		m_FriendRequest = p_InviteFriendCallback;
		m_loginCallback = p_LoginCallback;
		if (!NubeePlatformBinding.IsLoggedInToFacebook())
		{
			SingletonComponent<SocialNetworkManager>.Instance.LoginSocialNetwork(SocialNetworkType.Facebook);
			SocialNetworkManager.OnLoginCallback += LoginCallback;
			if (m_loginCallback != null)
			{
				m_loginCallback(ServiceResponseCodes.FACEBOOK_FIRST_LOGIN);
			}
			return;
		}
		Dbg.Log("NubeePlatformBinding.IsLoggedInToFacebook() is true, call m_loginCallback(ServiceResponseCodes.SUCCESS)");
		if (m_loginCallback != null)
		{
			SocialCallback loginCallback = m_loginCallback;
			m_loginCallback = null;
			loginCallback(ServiceResponseCodes.SUCCESS);
		}
		NubeePlatformBinding.TryGetFacebookFriendsWithoutAccessToken();
		NubeePlatformManager.OnFacebookGetFriendsWithoutAccessTokenDidComplete += GetFriendsWithoutAccessDidComplete;
		NubeePlatformManager.OnFacebookGetFriendsWithoutAccessTokenDidNotComplete += GetFriendsWithoutAccessDidNotComplete;
		NubeePlatformManager.OnFacebookInviteFriendsDidComplete += InviteFacebookFriendsDidComplete;
		NubeePlatformManager.OnFacebookInviteFriendsDidNotComplete += InviteFacebookFriendsDidNotComplete;
	}

	public void RefreshAlliesList(SocialCallback p_success, SocialCallback p_fail)
	{
		Services.ServiceHandle alliesList = SingletonComponent<Services>.Instance.GetAlliesList(SingletonComponent<Services>.Instance.NubeeId, SingletonComponent<Services>.Instance.DeviceId, InternalRefreshAlliesListSuccess, InternalServiceCallFailCb);
		CallbackPairs callbackPairs = new CallbackPairs();
		callbackPairs.success = p_success;
		callbackPairs.fail = p_fail;
		m_handles.Add(alliesList, callbackPairs);
	}

	public void AddFriend(string p_nubeeId, string p_isStranger, SocialCallback p_success, SocialCallback p_fail)
	{
		if (IsFriend(p_nubeeId))
		{
			p_fail(ServiceResponseCodes.NONE);
			return;
		}
		m_friendOp = p_nubeeId;
		Services.ServiceHandle key = SingletonComponent<Services>.Instance.AddFriend(SingletonComponent<Services>.Instance.NubeeId, SingletonComponent<Services>.Instance.DeviceId, p_nubeeId, p_isStranger, InternalAddFriendSuccess, InternalServiceCallFailCb);
		CallbackPairs callbackPairs = new CallbackPairs();
		callbackPairs.success = p_success;
		callbackPairs.fail = p_fail;
		m_handles.Add(key, callbackPairs);
	}

	public void RemoveFriend(string p_nubeeId, SocialCallback p_success, SocialCallback p_fail)
	{
		if (!IsFriend(p_nubeeId))
		{
			p_fail(ServiceResponseCodes.NONE);
			return;
		}
		m_friendOp = p_nubeeId;
		Services.ServiceHandle key = SingletonComponent<Services>.Instance.RemoveFriend(SingletonComponent<Services>.Instance.NubeeId, SingletonComponent<Services>.Instance.DeviceId, p_nubeeId, InternalRemoveFriendSuccess, InternalServiceCallFailCb);
		CallbackPairs callbackPairs = new CallbackPairs();
		callbackPairs.success = p_success;
		callbackPairs.fail = p_fail;
		m_handles.Add(key, callbackPairs);
	}

	public void BlockFriend(string p_nubeeId, SocialCallback p_success, SocialCallback p_fail)
	{
		m_friendOp = p_nubeeId;
		Services.ServiceHandle key = SingletonComponent<Services>.Instance.BlockFriend(SingletonComponent<Services>.Instance.NubeeId, SingletonComponent<Services>.Instance.DeviceId, p_nubeeId, InternalBlockFriendSuccess, InternalServiceCallFailCb);
		CallbackPairs callbackPairs = new CallbackPairs();
		callbackPairs.success = p_success;
		callbackPairs.fail = p_fail;
		m_handles.Add(key, callbackPairs);
	}

	public void ApproveFriend(string p_nubeeId, SocialCallback p_success, SocialCallback p_fail)
	{
		if (!IsPendingFriend(p_nubeeId))
		{
			p_fail(ServiceResponseCodes.NONE);
			return;
		}
		m_friendOp = p_nubeeId;
		Services.ServiceHandle key = SingletonComponent<Services>.Instance.ApproveFriend(SingletonComponent<Services>.Instance.NubeeId, SingletonComponent<Services>.Instance.DeviceId, p_nubeeId, InternalApproveFriendSuccess, InternalServiceCallFailCb);
		CallbackPairs callbackPairs = new CallbackPairs();
		callbackPairs.success = p_success;
		callbackPairs.fail = p_fail;
		m_handles.Add(key, callbackPairs);
	}

	public void DenyFriend(string p_nubeeId, SocialCallback p_success, SocialCallback p_fail)
	{
		if (!IsPendingFriend(p_nubeeId))
		{
			p_fail(ServiceResponseCodes.NONE);
			return;
		}
		m_friendOp = p_nubeeId;
		Services.ServiceHandle key = SingletonComponent<Services>.Instance.DenyFriend(SingletonComponent<Services>.Instance.NubeeId, SingletonComponent<Services>.Instance.DeviceId, p_nubeeId, InternalDenyFriendSuccess, InternalServiceCallFailCb);
		CallbackPairs callbackPairs = new CallbackPairs();
		callbackPairs.success = p_success;
		callbackPairs.fail = p_fail;
		m_handles.Add(key, callbackPairs);
	}

	public void SendGift(string p_friendId, SocialCallback p_success, SocialCallback p_fail)
	{
		if (!IsFriend(p_friendId))
		{
			p_fail(ServiceResponseCodes.NONE);
			return;
		}
		m_friendOp = p_friendId;
		Services.ServiceHandle key = SingletonComponent<Services>.Instance.SendGift(SingletonComponent<Services>.Instance.NubeeId, SingletonComponent<Services>.Instance.DeviceId, p_friendId, "1", InternalGiftSuccessCb, InternalGiftFailCb);
		CallbackPairs callbackPairs = new CallbackPairs();
		callbackPairs.success = p_success;
		callbackPairs.fail = p_fail;
		m_handles.Add(key, callbackPairs);
	}

	public int FindFriend(string p_nubeeId)
	{
		for (int i = 0; i < m_list.Count; i++)
		{
			if (m_list[i].nubeeId == p_nubeeId)
			{
				return i;
			}
		}
		return -1;
	}

	public bool IsFriend(string p_nubeeId)
	{
		if (FindFriend(p_nubeeId) == -1)
		{
			return false;
		}
		return true;
	}

	public int FindPendingFriend(string p_nubeeId)
	{
		for (int i = 0; i < m_listPending.Count; i++)
		{
			if (m_listPending[i].nubeeId == p_nubeeId)
			{
				return i;
			}
		}
		return -1;
	}

	public bool IsPendingFriend(string p_nubeeId)
	{
		if (FindPendingFriend(p_nubeeId) == -1)
		{
			return false;
		}
		return true;
	}

	public List<FriendAccountDetails> GetFriendsList()
	{
		return m_list;
	}

	public List<FriendAccountDetails> GetPendingFriendsList()
	{
		return m_listPending;
	}

	public List<FriendAccountDetails> GetStrangersList()
	{
		return m_listStrangers;
	}

	public List<FriendAccountDetails> GetAlliesList(int p_maxStrangers)
	{
		List<FriendAccountDetails> list = new List<FriendAccountDetails>();
		foreach (FriendAccountDetails item in m_list)
		{
			item.isFriend = true;
			if (item.coolDown == -1)
			{
				list.Add(item);
			}
		}
		for (int i = 0; i < p_maxStrangers && i < m_listStrangers.Count; i++)
		{
			FriendAccountDetails friendAccountDetails = m_listStrangers[i];
			friendAccountDetails.isFriend = false;
			list.Add(friendAccountDetails);
		}
		return list;
	}

	private void InternalRefreshFriendsListSuccess(Services.ServiceHandle p_handle)
	{
		m_list = (List<FriendAccountDetails>)p_handle.returnValue;
		Dbg.Log("Refresh Friends List Success");
		InternalSuccessCb(p_handle);
	}

	private void InternalRefreshAlliesListSuccess(Services.ServiceHandle p_handle)
	{
		AlliesDetails alliesDetails = (AlliesDetails)p_handle.returnValue;
		m_list = alliesDetails.m_listFriends;
		m_listStrangers = alliesDetails.m_listStranger;
		InternalSuccessCb(p_handle);
	}

	private void InternalRefreshStrangersListSuccess(Services.ServiceHandle p_handle)
	{
		m_listStrangers = (List<FriendAccountDetails>)p_handle.returnValue;
		Dbg.Log("Refresh Strangers List Success");
		InternalSuccessCb(p_handle);
	}

	private void InternalAddFriendSuccess(Services.ServiceHandle p_handle)
	{
		InternalSuccessCb(p_handle);
	}

	private void InternalRemoveFriendSuccess(Services.ServiceHandle p_handle)
	{
		int num = FindFriend(m_friendOp);
		if (num > -1)
		{
			m_list.RemoveAt(num);
		}
		InternalSuccessCb(p_handle);
	}

	private void InternalBlockFriendSuccess(Services.ServiceHandle p_handle)
	{
		int num = FindFriend(m_friendOp);
		if (num > -1)
		{
			m_list.RemoveAt(num);
		}
		num = FindPendingFriend(m_friendOp);
		if (num > -1)
		{
			m_listPending.RemoveAt(num);
		}
		InternalSuccessCb(p_handle);
	}

	private void InternalApproveFriendSuccess(Services.ServiceHandle p_handle)
	{
		int num = FindPendingFriend(m_friendOp);
		if (num > -1)
		{
			FriendAccountDetails item = m_listPending[num];
			m_listPending.RemoveAt(num);
			m_list.Add(item);
		}
		UpdateSocialRedIcon(true);
		InternalSuccessCb(p_handle);
	}

	private void InternalDenyFriendSuccess(Services.ServiceHandle p_handle)
	{
		int num = FindPendingFriend(m_friendOp);
		if (num > -1)
		{
			m_listPending.RemoveAt(num);
		}
		UpdateSocialRedIcon(true);
		InternalSuccessCb(p_handle);
	}

	private void InternalServiceCallFailCb(Services.ServiceHandle p_handle, string p_error)
	{
		InternalFailCb(p_handle);
		Dbg.LogError("SOCIAL_MANAGER : error : " + p_error);
	}

	private void InternalGiftSuccessCb(Services.ServiceHandle p_handle)
	{
		CurrencyDetails currency = (CurrencyDetails)p_handle.returnValue;
		SingletonComponent<PlayerDataManager>.Instance.SetCurrency(currency);
		InternalSuccessCb(p_handle);
	}

	private void InternalGiftFailCb(Services.ServiceHandle p_handle, string p_error)
	{
		InternalFailCb(p_handle);
		Dbg.LogError("SOCIAL_MANAGER : " + ServiceCallUtils.GetErrorMessage(p_handle));
	}

	private void InternalSuccessCb(Services.ServiceHandle p_handle)
	{
		if (m_handles.ContainsKey(p_handle) && m_handles[p_handle].success != null)
		{
			m_handles[p_handle].success(ServiceResponseCodes.SUCCESS);
			m_handles.Remove(p_handle);
		}
	}

	private void InternalFailCb(Services.ServiceHandle p_handle)
	{
		if (m_handles.ContainsKey(p_handle) && m_handles[p_handle].fail != null)
		{
			ServiceResponseCodes p_code = ServiceResponseCodes.FAILED;
			if (p_handle != null)
			{
				p_code = (ServiceResponseCodes)p_handle.nReturnCode;
			}
			m_handles[p_handle].fail(p_code);
			m_handles.Remove(p_handle);
		}
	}

	public void UpdateSocialRedIcon(bool p_isFromFriendList = false)
	{
		if (MainMenu.Instance != null && SingletonComponent<PlayerDataManager>.Instance != null && MainMenu.Instance.m_gachaGiftCounter != null)
		{
			PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
			int num = 0;
			if (playerData != null)
			{
				num += playerData.m_userData.giftGacha;
				if (p_isFromFriendList)
				{
					if (m_listPending != null)
					{
						num += m_listPending.Count;
						playerData.m_PendingFriendRequest = m_listPending.Count;
					}
				}
				else
				{
					num += playerData.m_PendingFriendRequest;
				}
			}
			MainMenu.Instance.UpdateFreeTicketsCounter(MainMenu.Instance.m_gachaGiftCounter, num);
			Dbg.Log("UpdateSocialRedIcon, " + num);
		}
		else
		{
			Dbg.LogWarn("MainMenu.Instance , PlayerDataManager.Instance, or MainMenu.Instance.m_gachaGiftCounter is null, the social red icon number was not updated.");
		}
	}

	public PlayerData ServerFriendToPlayerData(FriendAccountDetails _serverFriend)
	{
		PlayerData _playerData = new PlayerData();
		CharacterAccountDetails character = _serverFriend.character;
		character.ToPlayerData(ref _playerData);
		_playerData.SetAccountDetails(character);
		if (_serverFriend.isFriend)
		{
			_playerData.CharacterType = CharacterType.Friend;
			_playerData.FriendAssistCoolDown = _serverFriend.coolDown;
		}
		else
		{
			_playerData.CharacterType = CharacterType.Stranger;
		}
		InventoryManager instance = SingletonComponent<InventoryManager>.Instance;
		CharacterEquipmentDetails[] equipment = character.equipment;
		foreach (CharacterEquipmentDetails characterEquipmentDetails in equipment)
		{
			ItemDetails itemDetails = instance.CreateItem(characterEquipmentDetails.item_id, characterEquipmentDetails.item_tier, characterEquipmentDetails.item_level);
			if (itemDetails == ItemManager.s_NULL_DATA)
			{
				_playerData = null;
				return null;
			}
			_playerData.Equip(itemDetails);
		}
		_playerData.ComputeFinalValues();
		return _playerData;
	}

	public PlayerData ServerCharaToPlayerData(CharacterAccountDetails _charaData)
	{
		PlayerData _playerData = new PlayerData();
		_charaData.ToPlayerData(ref _playerData);
		_playerData.SetAccountDetails(_charaData);
		_playerData.CharacterType = CharacterType.Stranger;
		InventoryManager instance = SingletonComponent<InventoryManager>.Instance;
		CharacterEquipmentDetails[] equipment = _charaData.equipment;
		foreach (CharacterEquipmentDetails characterEquipmentDetails in equipment)
		{
			ItemDetails p_item = instance.CreateItem(characterEquipmentDetails.item_id, characterEquipmentDetails.item_tier, characterEquipmentDetails.item_level);
			_playerData.Equip(p_item);
		}
		_playerData.ComputeFinalValues();
		return _playerData;
	}

	public bool UpdateWarCryAbility()
	{
		GetTimeToWarCry();
		return m_warcryAbility;
	}

	public TimeSpan GetTimeToWarCry()
	{
		PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		if (playerData == null)
		{
			m_warcryAbility = false;
			return TimeSpan.FromHours(24.0);
		}
		long timeNextWarcry = playerData.m_userData.timeNextWarcry;
		if (timeNextWarcry < 0)
		{
			m_warcryAbility = true;
			return TimeSpan.FromHours(0.0);
		}
		long serverTimeDiff = SingletonComponent<Services>.Instance.ServerTimeDiff;
		long num = TimeUtil.GetUnixEpochTime() - serverTimeDiff;
		long num2 = timeNextWarcry - num;
		if (num2 > 0)
		{
			m_warcryAbility = false;
		}
		else
		{
			m_warcryAbility = true;
		}
		return TimeSpan.FromSeconds(num2);
	}

	public bool IsSNSLogin()
	{
		bool flag = true;
		flag = NubeePlatformBinding.IsLoggedInToFacebook();
		Dbg.Log("isLoginIntoSNS: " + flag);
		return flag;
	}
}
