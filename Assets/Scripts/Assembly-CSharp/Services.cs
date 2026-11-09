using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Models;
using ServiceCalls;
using UnityEngine;

public class Services : SingletonComponent<Services>
{
	public enum ConnectionResults
	{
		None = 0,
		Busy = 1,
		Success = 2,
		ConnectionFailure = 3,
		CommandFailure = 4,
		ExpiredKey = 5,
		ExpiredCommand = 6,
		NoResults = 7,
		WrongPayload = 8,
		InvalidJson = 9,
		InvalidParameter = 10,
		Timeout = 11,
		Max = 12
	}

	public abstract class ServiceHandle
	{
		public delegate void ServiceCompleteCallback(ServiceHandle p_handle);

		public delegate void ServiceErrorCallback(ServiceHandle p_handle, string p_errorMsg);

		public ServiceCompleteCallback successCb;

		public ServiceErrorCallback failCb;

		public int id;

		public ConnectionResults result;

		public object returnValue;

		public JSONObject returnJson;

		public int nReturnCode = -1;

		public float ServerConnectionTimeOut = 30f;

		protected NetConnection UrlConnection;

		protected int m_connectAttempts;

		public virtual bool IsNubeeAccountRequired()
		{
			return true;
		}

		public virtual bool IsKeyRequired()
		{
			return true;
		}

		public virtual bool IsAnalyticsActivity()
		{
			return true;
		}

		public virtual int MaxReconnectRetries()
		{
			return 5;
		}

		public abstract IEnumerator Execute();

		public bool IsBusy()
		{
			return result == ConnectionResults.Busy;
		}

		public bool IsSuccessful()
		{
			return result == ConnectionResults.Success;
		}

		public bool IsFailed()
		{
			return !IsSuccessful();
		}

		public bool IsNoInternet()
		{
			return result == ConnectionResults.NoResults || result == ConnectionResults.ConnectionFailure;
		}

		public bool ShouldRetry()
		{
			return m_connectAttempts <= MaxReconnectRetries();
		}

		public IEnumerator WaitForQuery(string cmd, WWWForm formData)
		{
			if (cmd == null)
			{
				Dbg.LogError("cmd is null");
				yield break;
			}
			if (formData != null)
			{
				Dbg.Log("[*** sending] [" + SingletonComponent<Services>.Instance.ServerTimeDiff + "]" + cmd + "?" + Encoding.UTF8.GetString(formData.data), Dbg.NBLogType.SERVICES);
				UrlConnection = OfflineServerMode.Enabled ? null : NetConnection.Online(new WWW(cmd, formData));
			}
			else
			{
				Dbg.Log("[*** sending] [" + SingletonComponent<Services>.Instance.ServerTimeDiff + "]" + cmd, Dbg.NBLogType.SERVICES);
				UrlConnection = OfflineServerMode.Enabled ? null : NetConnection.Online(new WWW(cmd));
			}
			if (OfflineServerMode.Enabled)
			{
				yield return null;
				UrlConnection = NetConnection.Offline(LocalServer.Handle(cmd, formData));
				yield break;
			}
			float startTime = Time.realtimeSinceStartup;
			float tempTime = 0f;
			while (true)
			{
				float num;
				tempTime = (num = Time.realtimeSinceStartup - startTime);
				if (!(num < ServerConnectionTimeOut) || UrlConnection.isDone || UrlConnection.error != null)
				{
					break;
				}
				yield return null;
			}
			if (tempTime >= ServerConnectionTimeOut)
			{
				UrlConnection = null;
				result = ConnectionResults.Timeout;
				nReturnCode = 0 - result;
			}
		}

		protected bool CheckError()
		{
			NetConnection urlConnection = UrlConnection;
			if (urlConnection == null)
			{
				result = ConnectionResults.Timeout;
				nReturnCode = 0 - result;
				return true;
			}
			if (urlConnection.error != null)
			{
				result = ConnectionResults.ConnectionFailure;
				nReturnCode = 0 - result;
				return true;
			}
			if (urlConnection.text == null || urlConnection.text == string.Empty)
			{
				result = ConnectionResults.NoResults;
				nReturnCode = 0 - result;
				return true;
			}
			return false;
		}

		protected bool CheckJSON(JSONObject p_json)
		{
			NetConnection urlConnection = UrlConnection;
			JSONObject field = p_json.GetField("code");
			if (field == null)
			{
				result = ConnectionResults.InvalidJson;
				nReturnCode = 0 - result;
				returnJson = new JSONObject();
				returnJson.AddField("code", nReturnCode);
				returnJson.AddField("message", "\"Invalid server response\"");
				return true;
			}
			result = ConnectionResults.Success;
			JSONObject field2 = p_json.GetField("ts");
			if (field2 != null && field2.str != null)
			{
				int num = 0;
				if (int.TryParse(field2.str, out num) && num > 0)
				{
					SingletonComponent<Services>.Instance.ServerTimeDiff = TimeUtil.GetUnixEpochTime() - num;
				}
			}
			returnJson = p_json;
			nReturnCode = (int)field.n;
			if (nReturnCode != 200)
			{
				if (nReturnCode == 100)
				{
					result = ConnectionResults.ExpiredKey;
				}
				else
				{
					result = ConnectionResults.CommandFailure;
				}
				Dbg.LogError(GLOBAL.SERVER_ERROR_TAG + ServiceCallUtils.GetStringField(p_json, "message", string.Empty));
				returnValue = nReturnCode;
				return true;
			}
			return false;
		}

		protected bool AnalyzeResultsAndBreak(out JSONObject p_json)
		{
			m_connectAttempts++;
			p_json = null;
			if (CheckError())
			{
				return true;
			}
			NetConnection urlConnection = UrlConnection;
			Dbg.Log("[*** Receive] " + urlConnection.text, Dbg.NBLogType.SERVICES);
			p_json = new JSONObject(urlConnection.text);
			return CheckJSON(p_json);
		}

		protected void PerformSuccess()
		{
			result = ConnectionResults.Success;
			nReturnCode = 200;
			m_connectAttempts = 0;
			if (successCb != null)
			{
				if (IsAnalyticsActivity() && SingletonComponent<AnalyticsManager>.Instance != null)
				{
					SingletonComponent<AnalyticsManager>.Instance.RecalculateNextPing();
				}
				successCb(this);
			}
		}

		protected void PerformFail(string szMessage)
		{
			if (failCb != null)
			{
				failCb(this, szMessage);
			}
		}

		protected void PerformFailNoInternet()
		{
			returnJson = new JSONObject();
			returnJson.AddField("code", nReturnCode);
			returnJson.AddField("message", "\"no connection\"");
			PerformFail("No internet");
		}

		protected void PerformFailClientError(int nErrorCode, string szErrorMessage)
		{
			nReturnCode = nErrorCode;
			if (returnJson == null)
			{
				returnJson = new JSONObject(JSONObject.Type.OBJECT);
			}
			returnJson["code"] = new JSONObject(nErrorCode);
			returnJson["message"] = new JSONObject("\"" + szErrorMessage + "\"");
			PerformFail(szErrorMessage);
		}
	}

	public int m_keyExpireRetries = 3;

	private int m_keyExpireRetryCounter;

	private int m_pagination = 100;

	protected ServiceHandle m_retrieveKeyHandle;

	protected bool m_newKeyRetrieveCalled;

	private string m_nubeeId = "1";

	private string m_deviceId = "249b4cb6c98343b3e26700c551ce8dc8";

	private long m_timeDiff;

	private bool m_deviceIdUpdated;

	protected string m_key = string.Empty;

	protected long m_lastKeyTimestamp;

	protected long m_expiration;

	protected List<int> m_freeIds = new List<int>();

	protected List<ServiceHandle> m_handles = new List<ServiceHandle>();

	public string NubeeId
	{
		get
		{
			return m_nubeeId;
		}
	}

	public string DeviceId
	{
		get
		{
			return m_deviceId;
		}
	}

	public static string Key
	{
		get
		{
			return SingletonComponent<Services>.Instance.m_key;
		}
	}

	public int MaxPagination
	{
		get
		{
			return m_pagination;
		}
	}

	public int HandleCount
	{
		get
		{
			if (m_handles != null)
			{
				return m_handles.Count;
			}
			return -1;
		}
	}

	public long ServerTimeDiff
	{
		get
		{
			return m_timeDiff;
		}
		set
		{
			m_timeDiff = value;
		}
	}

	public ServiceHandle StartupCheck(ServiceHandle.ServiceCompleteCallback p_successCb, ServiceHandle.ServiceErrorCallback p_failCb)
	{
		Dbg.Log("Service Call: StartupCheck", Dbg.NBLogType.SERVICES);
		StartupCheck startupCheck = new StartupCheck();
		startupCheck.id = GetFreeHandleId();
		m_handles.Add(startupCheck);
		startupCheck.successCb = p_successCb;
		startupCheck.failCb = p_failCb;
		return startupCheck;
	}

	public ServiceHandle SaveSessionTimestamps(string p_nubeeId, string p_deviceId, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		Dbg.Log("Service Call: SaveSessionTimestamps", Dbg.NBLogType.SERVICES);
		SaveSessionTimestamps saveSessionTimestamps = new SaveSessionTimestamps();
		saveSessionTimestamps.id = GetFreeHandleId();
		m_handles.Add(saveSessionTimestamps);
		saveSessionTimestamps.successCb = p_successCb;
		saveSessionTimestamps.failCb = p_failCb;
		saveSessionTimestamps.nubeeId = p_nubeeId;
		saveSessionTimestamps.deviceId = p_deviceId;
		return saveSessionTimestamps;
	}

	public ServiceHandle CreateOrUpdateNubeeAccount(string p_nubeeId, string p_deviceId, string p_name, string p_lang, string p_fbid, string p_fbtoken, string p_weiboId, bool p_isTutorial = false, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		CreateOrUpdateNubeeAccount createOrUpdateNubeeAccount = new CreateOrUpdateNubeeAccount();
		createOrUpdateNubeeAccount.id = GetFreeHandleId();
		m_handles.Add(createOrUpdateNubeeAccount);
		createOrUpdateNubeeAccount.successCb = p_successCb;
		createOrUpdateNubeeAccount.failCb = p_failCb;
		createOrUpdateNubeeAccount.nubeeId = p_nubeeId;
		createOrUpdateNubeeAccount.deviceId = p_deviceId;
		createOrUpdateNubeeAccount.name = p_name;
		createOrUpdateNubeeAccount.lang = p_lang;
		createOrUpdateNubeeAccount.fbid = p_fbid;
		createOrUpdateNubeeAccount.fbtoken = p_fbtoken;
		createOrUpdateNubeeAccount.weiboId = p_weiboId;
		createOrUpdateNubeeAccount.isTutorial = p_isTutorial;
		return createOrUpdateNubeeAccount;
	}

	public ServiceHandle UpdateDeviceId(string p_nubeeId, string p_newDeviceId, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		Dbg.Log("Service Call: UpdateDeviceId NID : " + p_nubeeId + " DID : " + p_newDeviceId, Dbg.NBLogType.SERVICES);
		UpdateDeviceId updateDeviceId = new UpdateDeviceId();
		updateDeviceId.id = GetFreeHandleId();
		m_handles.Add(updateDeviceId);
		updateDeviceId.nubeeId = p_nubeeId;
		updateDeviceId.newDeviceId = p_newDeviceId;
		updateDeviceId.successCb = p_successCb;
		updateDeviceId.failCb = p_failCb;
		return updateDeviceId;
	}

	public ServiceHandle RetrieveSecretKey(string p_nubeeId, string p_deviceId, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		Dbg.Log("Service Call: RetrieveSecretKey\t" + p_nubeeId + "\t" + p_deviceId, Dbg.NBLogType.SERVICES);
		RetrieveSecretKey retrieveSecretKey = (RetrieveSecretKey)(m_retrieveKeyHandle = new RetrieveSecretKey());
		retrieveSecretKey.nubeeId = p_nubeeId;
		retrieveSecretKey.deviceId = p_deviceId;
		retrieveSecretKey.successCb = p_successCb;
		retrieveSecretKey.failCb = p_failCb;
		return retrieveSecretKey;
	}

	public ServiceHandle GetCharacter(string p_nubeeId, string p_deviceId, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		Dbg.Log("Service Call: GetCharacter NID: " + p_nubeeId + " DID: " + p_deviceId, Dbg.NBLogType.SERVICES);
		GetCharacter getCharacter = new GetCharacter();
		getCharacter.id = GetFreeHandleId();
		m_handles.Add(getCharacter);
		getCharacter.nubeeId = p_nubeeId;
		getCharacter.deviceId = p_deviceId;
		getCharacter.successCb = p_successCb;
		getCharacter.failCb = p_failCb;
		return getCharacter;
	}

	public ServiceHandle GetInventory(string p_nubeeId, string p_deviceId, int p_offset, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		Dbg.Log("Service Call: GetInventory", Dbg.NBLogType.SERVICES);
		GetInventory getInventory = new GetInventory();
		getInventory.id = GetFreeHandleId();
		m_handles.Add(getInventory);
		getInventory.nubeeId = p_nubeeId;
		getInventory.deviceId = p_deviceId;
		getInventory.offset = p_offset;
		getInventory.successCb = p_successCb;
		getInventory.failCb = p_failCb;
		return getInventory;
	}

	public ServiceHandle UpdateInventory(string p_nubeeId, string p_deviceId, List<string> p_items, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null, bool p_isTutorial = false)
	{
		Dbg.Log("Service Call: UpdateInventory", Dbg.NBLogType.SERVICES);
		UpdateInventory updateInventory = new UpdateInventory();
		updateInventory.id = GetFreeHandleId();
		m_handles.Add(updateInventory);
		updateInventory.nubeeId = p_nubeeId;
		updateInventory.deviceId = p_deviceId;
		updateInventory.items = p_items;
		updateInventory.successCb = p_successCb;
		updateInventory.failCb = p_failCb;
		updateInventory.isTutorial = p_isTutorial;
		return updateInventory;
	}

	public ServiceHandle MultiSellItem(string p_nubeeId, string p_deviceId, List<string> p_itemsDbId, string p_requestId, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		Dbg.Log("Service Call: MultiSellItem", Dbg.NBLogType.SERVICES);
		MultiSell multiSell = new MultiSell();
		multiSell.id = GetFreeHandleId();
		m_handles.Add(multiSell);
		multiSell.nubeeId = p_nubeeId;
		multiSell.deviceId = p_deviceId;
		multiSell.itemsId = p_itemsDbId;
		multiSell.requestId = p_requestId;
		multiSell.successCb = p_successCb;
		multiSell.failCb = p_failCb;
		return multiSell;
	}

	public ServiceHandle GetAchievements(string p_nubeeId, string p_deviceId, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		Dbg.Log("Service Call: GetAchievements ", Dbg.NBLogType.SERVICES);
		GetAchievements getAchievements = new GetAchievements();
		getAchievements.id = GetFreeHandleId();
		m_handles.Add(getAchievements);
		getAchievements.nubeeId = p_nubeeId;
		getAchievements.deviceId = p_deviceId;
		getAchievements.successCb = p_successCb;
		getAchievements.failCb = p_failCb;
		return getAchievements;
	}

	public ServiceHandle UpdateAchievementProgress(string p_nubeeId, string p_deviceId, Dictionary<string, int> p_achieveIdProgressMap, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		Dbg.Log("Service Call: UpdateAchievementProgress()", Dbg.NBLogType.SERVICES);
		UpdateAchievementProgress updateAchievementProgress = new UpdateAchievementProgress();
		updateAchievementProgress.id = GetFreeHandleId();
		m_handles.Add(updateAchievementProgress);
		updateAchievementProgress.nubeeId = p_nubeeId;
		updateAchievementProgress.deviceId = p_deviceId;
		updateAchievementProgress.achieveIdProgressMap = p_achieveIdProgressMap;
		updateAchievementProgress.successCb = p_successCb;
		updateAchievementProgress.failCb = p_failCb;
		return updateAchievementProgress;
	}

	public ServiceHandle MarkAchievements(string p_nubeeId, string p_deviceId, List<string> p_achievementIds, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		Dbg.Log("Service Call: MarkAchievements", Dbg.NBLogType.SERVICES);
		MarkAchievements markAchievements = new MarkAchievements();
		markAchievements.id = GetFreeHandleId();
		m_handles.Add(markAchievements);
		markAchievements.nubeeId = p_nubeeId;
		markAchievements.deviceId = p_deviceId;
		markAchievements.ids = p_achievementIds;
		markAchievements.successCb = p_successCb;
		markAchievements.failCb = p_failCb;
		return markAchievements;
	}

	public ServiceHandle ClaimAchievement(string p_nubeeId, string p_deviceId, string p_achievementId, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		Dbg.Log("Service Call: ClaimAchievement", Dbg.NBLogType.SERVICES);
		ClaimAchievement claimAchievement = new ClaimAchievement();
		claimAchievement.id = GetFreeHandleId();
		m_handles.Add(claimAchievement);
		claimAchievement.nubeeId = p_nubeeId;
		claimAchievement.deviceId = p_deviceId;
		claimAchievement.achievement_id = p_achievementId;
		claimAchievement.successCb = p_successCb;
		claimAchievement.failCb = p_failCb;
		return claimAchievement;
	}

	public ServiceHandle GetAnnouncements(string p_nubeeId, string p_deviceId, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		Dbg.Log("Service Call: GetAnnouncements", Dbg.NBLogType.SERVICES);
		GetAnnouncements getAnnouncements = new GetAnnouncements();
		getAnnouncements.id = GetFreeHandleId();
		m_handles.Add(getAnnouncements);
		getAnnouncements.nubeeId = p_nubeeId;
		getAnnouncements.deviceId = p_deviceId;
		getAnnouncements.successCb = p_successCb;
		getAnnouncements.failCb = p_failCb;
		return getAnnouncements;
	}

	public ServiceHandle ClaimAnnouncementReward(string p_nubeeId, string p_deviceId, int p_announcementDbId, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		Dbg.Log("Service Call : ClaimAnnouncement", Dbg.NBLogType.SERVICES);
		ClaimAnnouncement claimAnnouncement = new ClaimAnnouncement();
		claimAnnouncement.id = GetFreeHandleId();
		m_handles.Add(claimAnnouncement);
		claimAnnouncement.nubeeId = p_nubeeId;
		claimAnnouncement.deviceId = p_deviceId;
		claimAnnouncement.announcementDbId = p_announcementDbId;
		claimAnnouncement.successCb = p_successCb;
		claimAnnouncement.failCb = p_failCb;
		return claimAnnouncement;
	}

	public ServiceHandle DeleteAnnouncementMessage(string p_nubeeId, string p_deviceId, int p_announcementDbId, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		Dbg.Log("Service call : DeleteAnnouncementMessage", Dbg.NBLogType.SERVICES);
		DeleteAnnouncementMessage deleteAnnouncementMessage = new DeleteAnnouncementMessage();
		deleteAnnouncementMessage.id = GetFreeHandleId();
		m_handles.Add(deleteAnnouncementMessage);
		deleteAnnouncementMessage.nubeeId = p_nubeeId;
		deleteAnnouncementMessage.deviceId = p_deviceId;
		deleteAnnouncementMessage.announcementDbId = p_announcementDbId;
		deleteAnnouncementMessage.successCb = p_successCb;
		deleteAnnouncementMessage.failCb = p_failCb;
		return deleteAnnouncementMessage;
	}

	public ServiceHandle UnlockPet(string p_nubeeId, string p_deviceId, string p_petId, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		Dbg.Log("[Info] UnlockPet::Execute()", Dbg.NBLogType.SERVICES);
		UnlockPet unlockPet = new UnlockPet();
		unlockPet.id = GetFreeHandleId();
		m_handles.Add(unlockPet);
		unlockPet.m_nubeeId = p_nubeeId;
		unlockPet.m_deviceId = p_deviceId;
		unlockPet.m_petId = p_petId;
		unlockPet.successCb = p_successCb;
		unlockPet.failCb = p_failCb;
		return unlockPet;
	}

	public ServiceHandle SwitchActivePet(string p_nubeeId, string p_deviceId, string p_petId, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		Dbg.Log("[Info] SwitchActivePet::Execute()", Dbg.NBLogType.SERVICES);
		SwitchActivePet switchActivePet = new SwitchActivePet();
		switchActivePet.id = GetFreeHandleId();
		m_handles.Add(switchActivePet);
		switchActivePet.m_nubeeId = p_nubeeId;
		switchActivePet.m_deviceId = p_deviceId;
		switchActivePet.m_petId = p_petId;
		switchActivePet.successCb = p_successCb;
		switchActivePet.failCb = p_failCb;
		return switchActivePet;
	}

	public ServiceHandle GetGlobal(string p_nubeeId, string p_deviceId, int p_offset, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failedCb = null)
	{
		GetGlobalLeaderboard getGlobalLeaderboard = new GetGlobalLeaderboard();
		getGlobalLeaderboard.id = GetFreeHandleId();
		m_handles.Add(getGlobalLeaderboard);
		getGlobalLeaderboard.m_nubeeId = p_nubeeId;
		getGlobalLeaderboard.m_deviceId = p_deviceId;
		getGlobalLeaderboard.m_offset = p_offset;
		getGlobalLeaderboard.successCb = p_successCb;
		getGlobalLeaderboard.failCb = p_failedCb;
		return getGlobalLeaderboard;
	}

	public ServiceHandle GetSocial(string p_nubeeId, string p_deviceId, int p_offset, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failedCb = null)
	{
		GetSocialLeaderboard getSocialLeaderboard = new GetSocialLeaderboard();
		getSocialLeaderboard.id = GetFreeHandleId();
		m_handles.Add(getSocialLeaderboard);
		getSocialLeaderboard.m_nubeeId = p_nubeeId;
		getSocialLeaderboard.m_deviceId = p_deviceId;
		getSocialLeaderboard.m_offset = p_offset;
		getSocialLeaderboard.successCb = p_successCb;
		getSocialLeaderboard.failCb = p_failedCb;
		return getSocialLeaderboard;
	}

	public ServiceHandle SaveScore(string p_nubeeId, string p_deviceId, int p_score, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failedCb = null)
	{
		Dbg.Log("SaveScore : NID: " + p_nubeeId + " DID: " + p_deviceId + " Score: " + p_score, Dbg.NBLogType.SERVICES);
		SaveScore saveScore = new SaveScore();
		saveScore.id = GetFreeHandleId();
		m_handles.Add(saveScore);
		saveScore.m_nubeeId = p_nubeeId;
		saveScore.m_deviceId = p_deviceId;
		saveScore.m_score = p_score;
		saveScore.successCb = p_successCb;
		saveScore.failCb = p_failedCb;
		return saveScore;
	}

	public ServiceHandle DailyPromotion(string p_nubeeId, string p_deviceId, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		DailyPromotion dailyPromotion = new DailyPromotion();
		dailyPromotion.id = GetFreeHandleId();
		m_handles.Add(dailyPromotion);
		dailyPromotion.m_nubeeId = p_nubeeId;
		dailyPromotion.m_deviceId = p_deviceId;
		dailyPromotion.successCb = p_successCb;
		dailyPromotion.failCb = p_failCb;
		return dailyPromotion;
	}

	public ServiceHandle DailyLoginBonus(string p_nubeeId, string p_deviceId, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		DailyLoginBonus dailyLoginBonus = new DailyLoginBonus();
		dailyLoginBonus.id = GetFreeHandleId();
		m_handles.Add(dailyLoginBonus);
		dailyLoginBonus.m_nubeeId = p_nubeeId;
		dailyLoginBonus.m_deviceId = p_deviceId;
		dailyLoginBonus.successCb = p_successCb;
		dailyLoginBonus.failCb = p_failCb;
		return dailyLoginBonus;
	}

	public ServiceHandle DailyBonusClaim(string p_nubeeId, string p_deviceId, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		DailyBonusClaim dailyBonusClaim = new DailyBonusClaim();
		dailyBonusClaim.id = GetFreeHandleId();
		m_handles.Add(dailyBonusClaim);
		dailyBonusClaim.nubeeId = p_nubeeId;
		dailyBonusClaim.deviceId = p_deviceId;
		dailyBonusClaim.successCb = p_successCb;
		dailyBonusClaim.failCb = p_failCb;
		return dailyBonusClaim;
	}

	public ServiceHandle GetStoreItems(ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		Dbg.Log("Service Call: GetStoreItems", Dbg.NBLogType.SERVICES);
		GetStoreItems getStoreItems = new GetStoreItems();
		getStoreItems.id = GetFreeHandleId();
		m_handles.Add(getStoreItems);
		getStoreItems.successCb = p_successCb;
		getStoreItems.failCb = p_failCb;
		return getStoreItems;
	}

	public ServiceHandle Purchase(string p_nubeeId, string p_deviceId, string p_storeItemId, string p_receiptHash, int p_bagSlotsMultiple = 1, string p_json = "", ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		Purchase purchase = new Purchase();
		purchase.id = GetFreeHandleId();
		m_handles.Add(purchase);
		purchase.nubeeId = p_nubeeId;
		purchase.deviceId = p_deviceId;
		purchase.storeItemId = p_storeItemId;
		purchase.receiptHash = p_receiptHash;
		purchase.bagSlotsMultiple = p_bagSlotsMultiple.ToString();
		purchase.json = p_json;
		purchase.successCb = p_successCb;
		purchase.failCb = p_failCb;
		return purchase;
	}

	public ServiceHandle RetryBattle(string p_nubeeId, string p_deviceId, string p_dungeonKey, int p_monsterIndex, int p_snsReward, string p_requestId, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		Dbg.Log("Service Call: RetryBattle " + p_dungeonKey + " " + p_monsterIndex, Dbg.NBLogType.SERVICES);
		RetryBattle retryBattle = new RetryBattle();
		retryBattle.id = GetFreeHandleId();
		m_handles.Add(retryBattle);
		retryBattle.nubeeId = p_nubeeId;
		retryBattle.deviceId = p_deviceId;
		retryBattle.dungeonKey = p_dungeonKey;
		retryBattle.monsterIndex = p_monsterIndex;
		retryBattle.requestId = p_requestId;
		retryBattle.sns_reward = p_snsReward;
		retryBattle.successCb = p_successCb;
		retryBattle.failCb = p_failCb;
		return retryBattle;
	}

	public ServiceHandle ResetData(string p_nubeeId, string p_deviceId, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		Dbg.Log("Service Call: ResetData", Dbg.NBLogType.SERVICES);
		ResetData resetData = new ResetData();
		resetData.id = GetFreeHandleId();
		m_handles.Add(resetData);
		resetData.nubeeId = p_nubeeId;
		resetData.deviceId = p_deviceId;
		resetData.successCb = p_successCb;
		resetData.failCb = p_failCb;
		return resetData;
	}

	public ServiceHandle AddWeiboFriend(string p_nubeeId, string p_deviceId, List<string> p_wbids, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		AddWeiboFriend addWeiboFriend = new AddWeiboFriend();
		addWeiboFriend.id = GetFreeHandleId();
		m_handles.Add(addWeiboFriend);
		addWeiboFriend.nubeeId = p_nubeeId;
		addWeiboFriend.deviceId = p_deviceId;
		addWeiboFriend.wbids = p_wbids;
		addWeiboFriend.successCb = p_successCb;
		addWeiboFriend.failCb = p_failCb;
		return addWeiboFriend;
	}

	public ServiceHandle AddFacebookFriend(string p_nubeeId, string p_deviceId, List<string> p_fbids, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		AddFacebookFriend addFacebookFriend = new AddFacebookFriend();
		addFacebookFriend.id = GetFreeHandleId();
		m_handles.Add(addFacebookFriend);
		addFacebookFriend.nubeeId = p_nubeeId;
		addFacebookFriend.deviceId = p_deviceId;
		addFacebookFriend.fbids = p_fbids;
		addFacebookFriend.successCb = p_successCb;
		addFacebookFriend.failCb = p_failCb;
		return addFacebookFriend;
	}

	public ServiceHandle AddFriend(string p_nubeeId, string p_deviceId, string p_friendId, string p_isStranger = "0", ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		Dbg.Log("Service Call: AddFriend", Dbg.NBLogType.SERVICES);
		AddFriend addFriend = new AddFriend();
		addFriend.id = GetFreeHandleId();
		m_handles.Add(addFriend);
		addFriend.nubeeId = p_nubeeId;
		addFriend.deviceId = p_deviceId;
		addFriend.friendId = p_friendId;
		addFriend.isStranger = p_isStranger;
		addFriend.successCb = p_successCb;
		addFriend.failCb = p_failCb;
		return addFriend;
	}

	public ServiceHandle RemoveFriend(string p_nubeeId, string p_deviceId, string p_friendId, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		Dbg.Log("Service Call: RemoveFriend", Dbg.NBLogType.SERVICES);
		RemoveFriend removeFriend = new RemoveFriend();
		removeFriend.id = GetFreeHandleId();
		m_handles.Add(removeFriend);
		removeFriend.nubeeId = p_nubeeId;
		removeFriend.deviceId = p_deviceId;
		removeFriend.friendId = p_friendId;
		removeFriend.successCb = p_successCb;
		removeFriend.failCb = p_failCb;
		return removeFriend;
	}

	public ServiceHandle ApproveFriend(string p_nubeeId, string p_deviceId, string p_friendId, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		Dbg.Log("Service Call: ApproveFriend", Dbg.NBLogType.SERVICES);
		ApproveFriend approveFriend = new ApproveFriend();
		approveFriend.id = GetFreeHandleId();
		m_handles.Add(approveFriend);
		approveFriend.nubeeId = p_nubeeId;
		approveFriend.deviceId = p_deviceId;
		approveFriend.friendId = p_friendId;
		approveFriend.successCb = p_successCb;
		approveFriend.failCb = p_failCb;
		return approveFriend;
	}

	public ServiceHandle DenyFriend(string p_nubeeId, string p_deviceId, string p_friendId, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		Dbg.Log("Service Call: DenyFriend", Dbg.NBLogType.SERVICES);
		DenyFriend denyFriend = new DenyFriend();
		denyFriend.id = GetFreeHandleId();
		m_handles.Add(denyFriend);
		denyFriend.nubeeId = p_nubeeId;
		denyFriend.deviceId = p_deviceId;
		denyFriend.friendId = p_friendId;
		denyFriend.successCb = p_successCb;
		denyFriend.failCb = p_failCb;
		return denyFriend;
	}

	public ServiceHandle BlockFriend(string p_nubeeId, string p_deviceId, string p_friendId, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		Dbg.Log("Service Call: BlockFriend", Dbg.NBLogType.SERVICES);
		BlockFriend blockFriend = new BlockFriend();
		blockFriend.id = GetFreeHandleId();
		m_handles.Add(blockFriend);
		blockFriend.nubeeId = p_nubeeId;
		blockFriend.deviceId = p_deviceId;
		blockFriend.friendId = p_friendId;
		blockFriend.successCb = p_successCb;
		blockFriend.failCb = p_failCb;
		return blockFriend;
	}

	public ServiceHandle GetAlliesList(string p_nubeeId, string p_deviceId, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		Dbg.Log("Service Call: GetAlliesList", Dbg.NBLogType.SERVICES);
		GetAlliesList getAlliesList = new GetAlliesList();
		getAlliesList.id = GetFreeHandleId();
		m_handles.Add(getAlliesList);
		getAlliesList.nubeeId = p_nubeeId;
		getAlliesList.deviceId = p_deviceId;
		getAlliesList.successCb = p_successCb;
		getAlliesList.failCb = p_failCb;
		return getAlliesList;
	}

	public ServiceHandle GetFriendsList(string p_nubeeId, string p_deviceId, int p_offset, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		Dbg.Log("Service Call: GetFriendsList", Dbg.NBLogType.SERVICES);
		GetFriendsList getFriendsList = new GetFriendsList();
		getFriendsList.id = GetFreeHandleId();
		m_handles.Add(getFriendsList);
		getFriendsList.nubeeId = p_nubeeId;
		getFriendsList.deviceId = p_deviceId;
		getFriendsList.offset = p_offset;
		getFriendsList.successCb = p_successCb;
		getFriendsList.failCb = p_failCb;
		return getFriendsList;
	}

	public ServiceHandle GetPendingFriendsList(string p_nubeeId, string p_deviceId, int p_offset, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		Dbg.Log("Service Call: GetPendingFriendsList", Dbg.NBLogType.SERVICES);
		GetPendingFriendsList getPendingFriendsList = new GetPendingFriendsList();
		getPendingFriendsList.id = GetFreeHandleId();
		m_handles.Add(getPendingFriendsList);
		getPendingFriendsList.nubeeId = p_nubeeId;
		getPendingFriendsList.deviceId = p_deviceId;
		getPendingFriendsList.offset = p_offset;
		getPendingFriendsList.successCb = p_successCb;
		getPendingFriendsList.failCb = p_failCb;
		return getPendingFriendsList;
	}

	public ServiceHandle GetStrangersList(string p_nubeeId, string p_deviceId, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		Dbg.Log("Service Call: GetStrangersList", Dbg.NBLogType.SERVICES);
		GetStrangersList getStrangersList = new GetStrangersList();
		getStrangersList.id = GetFreeHandleId();
		m_handles.Add(getStrangersList);
		getStrangersList.nubeeId = p_nubeeId;
		getStrangersList.deviceId = p_deviceId;
		getStrangersList.successCb = p_successCb;
		getStrangersList.failCb = p_failCb;
		return getStrangersList;
	}

	public ServiceHandle SendGift(string p_nubeeId, string p_deviceId, string p_friendId, string p_amout = "1", ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		Dbg.Log("Service Call: SendGift " + p_nubeeId + "  " + p_deviceId + "  " + p_friendId + "  " + p_amout, Dbg.NBLogType.SERVICES);
		SendGift sendGift = new SendGift();
		sendGift.id = GetFreeHandleId();
		m_handles.Add(sendGift);
		sendGift.nubeeId = p_nubeeId;
		sendGift.deviceId = p_deviceId;
		sendGift.friendId = p_friendId;
		sendGift.amount = p_amout;
		sendGift.successCb = p_successCb;
		sendGift.failCb = p_failCb;
		return sendGift;
	}

	public ServiceHandle GetGiftSenders(string p_nubeeId, string p_deviceId, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		Dbg.Log("Service Call: GetGiftSenders " + p_nubeeId + "  " + p_deviceId, Dbg.NBLogType.SERVICES);
		GetGiftSenders getGiftSenders = new GetGiftSenders();
		getGiftSenders.id = GetFreeHandleId();
		m_handles.Add(getGiftSenders);
		getGiftSenders.nubeeId = p_nubeeId;
		getGiftSenders.deviceId = p_deviceId;
		getGiftSenders.successCb = p_successCb;
		getGiftSenders.failCb = p_failCb;
		return getGiftSenders;
	}

	public ServiceHandle FusionStart(string p_nubeeId, string p_deviceId, string p_baseItemDbId, List<string> p_fuseItemDbIds, string p_requestId, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null, bool p_isTutorial = false)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (string p_fuseItemDbId in p_fuseItemDbIds)
		{
			stringBuilder.AppendLine(p_fuseItemDbId);
		}
		Dbg.Log("Service Call: FusionStart NID: " + p_nubeeId + " DID: " + p_deviceId + " baseItem: " + p_baseItemDbId + " itemList\n: " + stringBuilder.ToString(), Dbg.NBLogType.SERVICES);
		stringBuilder = null;
		FusionStart fusionStart = new FusionStart();
		fusionStart.id = GetFreeHandleId();
		m_handles.Add(fusionStart);
		fusionStart.nubeeId = p_nubeeId;
		fusionStart.deviceId = p_deviceId;
		fusionStart.baseItem = p_baseItemDbId;
		fusionStart.fuseItems = p_fuseItemDbIds;
		fusionStart.requestId = p_requestId;
		fusionStart.successCb = p_successCb;
		fusionStart.failCb = p_failCb;
		fusionStart.IsTutorial = p_isTutorial;
		return fusionStart;
	}

	public ServiceHandle Evolve(string p_nubeeId, string p_deviceId, string p_baseItemDbId, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		Dbg.Log("Service Call: Evolve", Dbg.NBLogType.SERVICES);
		Evolve evolve = new Evolve();
		evolve.id = GetFreeHandleId();
		m_handles.Add(evolve);
		evolve.nubeeId = p_nubeeId;
		evolve.deviceId = p_deviceId;
		evolve.baseItem = p_baseItemDbId;
		evolve.successCb = p_successCb;
		evolve.failCb = p_failCb;
		return evolve;
	}

	public ServiceHandle GetAllGachaEvents(ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		Dbg.Log("Service Call: GetAllGachaEvents", Dbg.NBLogType.SERVICES);
		GetAllGachaEvents getAllGachaEvents = new GetAllGachaEvents();
		getAllGachaEvents.id = GetFreeHandleId();
		m_handles.Add(getAllGachaEvents);
		getAllGachaEvents.successCb = p_successCb;
		getAllGachaEvents.failCb = p_failCb;
		return getAllGachaEvents;
	}

	public ServiceHandle PlayGacha(string p_nubeeId, string p_deviceId, string p_gachaId, string p_requestId, bool p_useTicket = false, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null, bool p_isTutorial = false)
	{
		PlayGacha playGacha = new PlayGacha();
		playGacha.id = GetFreeHandleId();
		m_handles.Add(playGacha);
		playGacha.nubeeId = p_nubeeId;
		playGacha.deviceId = p_deviceId;
		playGacha.gachaId = p_gachaId;
		playGacha.SetTicket(p_useTicket);
		playGacha.successCb = p_successCb;
		playGacha.failCb = p_failCb;
		playGacha.m_isTutorial = p_isTutorial;
		playGacha.requestId = p_requestId;
		return playGacha;
	}

	public ServiceHandle PlayMultiGacha(string p_nubeeId, string p_deviceId, string p_gachaId, string p_requestId, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		Dbg.Log("Service Call: PlayMultiGacha", Dbg.NBLogType.SERVICES);
		PlayMultiGacha playMultiGacha = new PlayMultiGacha();
		playMultiGacha.id = GetFreeHandleId();
		m_handles.Add(playMultiGacha);
		playMultiGacha.nubeeId = p_nubeeId;
		playMultiGacha.deviceId = p_deviceId;
		playMultiGacha.gachaId = p_gachaId;
		playMultiGacha.requestId = p_requestId;
		playMultiGacha.successCb = p_successCb;
		playMultiGacha.failCb = p_failCb;
		return playMultiGacha;
	}

	public ServiceHandle DungeonStart(string p_nubeeId, string p_deviceId, string p_dungeonId, string p_dungeonType, string p_sessionRequestID, List<string> p_monsterIds = null, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		DungeonStart dungeonStart = new DungeonStart();
		dungeonStart.id = GetFreeHandleId();
		m_handles.Add(dungeonStart);
		dungeonStart.nubeeId = p_nubeeId;
		dungeonStart.deviceId = p_deviceId;
		dungeonStart.dungeonId = p_dungeonId;
		dungeonStart.sessionRequestID = p_sessionRequestID;
		dungeonStart.monsterIds = p_monsterIds;
		dungeonStart.dungeonType = p_dungeonType;
		dungeonStart.successCb = p_successCb;
		dungeonStart.failCb = p_failCb;
		return dungeonStart;
	}

	public ServiceHandle DungeonEventStart(string p_nubeeId, string p_deviceId, string p_dungeonEventId, int p_battleCount, string p_sessionRequestID, int p_difficulty = 0, bool p_isTutorial = false, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		DungeonEventStart dungeonEventStart = new DungeonEventStart();
		dungeonEventStart.id = GetFreeHandleId();
		m_handles.Add(dungeonEventStart);
		dungeonEventStart.nubeeId = p_nubeeId;
		dungeonEventStart.deviceId = p_deviceId;
		dungeonEventStart.dungeonEventId = p_dungeonEventId;
		dungeonEventStart.sessionRequestID = p_sessionRequestID;
		dungeonEventStart.difficulty = p_difficulty;
		dungeonEventStart.isTutorial = p_isTutorial;
		dungeonEventStart.successCb = p_successCb;
		dungeonEventStart.failCb = p_failCb;
		return dungeonEventStart;
	}

	public ServiceHandle DungeonEnd(string p_nubeeId, string p_deviceId, string p_dungeonKey, string p_endStatus, int p_monstersDefeated, bool p_isKingBossDefeated, string p_overallScore, string p_lowestScore, string p_assistFriendId, int dodoboCurrency, string p_sessionRequestId, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		Dbg.LogEx("Service Call: DungeonEnd NID: {0}, DID: {1}, DKey: {2}, EndStatus: {3}, MonstersD: {4}, KingBoss: {5}, overAllScore: {6}, lowestScore: {7}", Dbg.NBLogType.SERVICES, p_nubeeId, p_deviceId, p_dungeonKey, p_endStatus, p_monstersDefeated.ToString(), p_isKingBossDefeated.ToString(), p_overallScore, p_lowestScore);
		DungeonEnd dungeonEnd = new DungeonEnd();
		dungeonEnd.id = GetFreeHandleId();
		m_handles.Add(dungeonEnd);
		dungeonEnd.nubeeId = p_nubeeId;
		dungeonEnd.deviceId = p_deviceId;
		dungeonEnd.dungeonKey = p_dungeonKey;
		dungeonEnd.endStatus = p_endStatus;
		dungeonEnd.monstersDefeated = p_monstersDefeated;
		dungeonEnd.isKingBossDefeated = p_isKingBossDefeated;
		dungeonEnd.overallScore = p_overallScore;
		dungeonEnd.lowestScore = p_lowestScore;
		dungeonEnd.assistFriendId = p_assistFriendId;
		dungeonEnd.dodoboCurrency = dodoboCurrency;
		dungeonEnd.sessionRequestID = p_sessionRequestId;
		dungeonEnd.successCb = p_successCb;
		dungeonEnd.failCb = p_failCb;
		return dungeonEnd;
	}

	public ServiceHandle GetUnlockedCities(string p_nubeeId, string p_deviceId, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		Dbg.Log("Service Call: GetUnlockedCities", Dbg.NBLogType.SERVICES);
		GetUnlockedCities getUnlockedCities = new GetUnlockedCities();
		getUnlockedCities.id = GetFreeHandleId();
		m_handles.Add(getUnlockedCities);
		getUnlockedCities.nubeeId = p_nubeeId;
		getUnlockedCities.deviceId = p_deviceId;
		getUnlockedCities.successCb = p_successCb;
		getUnlockedCities.failCb = p_failCb;
		return getUnlockedCities;
	}

	public ServiceHandle GetUnlockedDungeons(string p_nubeeId, string p_deviceId, string p_cityId, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		Dbg.Log("Service Call: GetUnlockedDungeons", Dbg.NBLogType.SERVICES);
		GetUnlockedDungeons getUnlockedDungeons = new GetUnlockedDungeons();
		getUnlockedDungeons.id = GetFreeHandleId();
		m_handles.Add(getUnlockedDungeons);
		getUnlockedDungeons.nubeeId = p_nubeeId;
		getUnlockedDungeons.deviceId = p_deviceId;
		getUnlockedDungeons.cityId = p_cityId;
		getUnlockedDungeons.successCb = p_successCb;
		getUnlockedDungeons.failCb = p_failCb;
		return getUnlockedDungeons;
	}

	public ServiceHandle GetAllDungeonEvents(string p_nubeeId, string p_deviceId, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		Dbg.Log("Service Call: GetAllDungeonEvents", Dbg.NBLogType.SERVICES);
		GetAllDungeonEvents getAllDungeonEvents = new GetAllDungeonEvents();
		getAllDungeonEvents.id = GetFreeHandleId();
		m_handles.Add(getAllDungeonEvents);
		getAllDungeonEvents.nubeeId = p_nubeeId;
		getAllDungeonEvents.deviceId = p_deviceId;
		getAllDungeonEvents.successCb = p_successCb;
		getAllDungeonEvents.failCb = p_failCb;
		return getAllDungeonEvents;
	}

	public ServiceHandle SendDeviceTokenToServer(string p_nubeeId, string p_token, string p_lang = "en", ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		Dbg.Log("Service Call: SendDeviceTokenToServer NID: " + p_nubeeId + " Token: " + p_token + " Lang: " + p_lang, Dbg.NBLogType.SERVICES);
		SendDeviceTokenToServer sendDeviceTokenToServer = new SendDeviceTokenToServer();
		sendDeviceTokenToServer.id = GetFreeHandleId();
		m_handles.Add(sendDeviceTokenToServer);
		sendDeviceTokenToServer.nubeeId = p_nubeeId;
		sendDeviceTokenToServer.token = p_token;
		sendDeviceTokenToServer.language = p_lang;
		sendDeviceTokenToServer.successCb = p_successCb;
		sendDeviceTokenToServer.failCb = p_failCb;
		return sendDeviceTokenToServer;
	}

	public ServiceHandle GameInitialization(string p_nubeeId, string p_deviceId, string p_platform, string p_version = "", string p_filter = "", string p_language = "", string p_gender = "M", string p_market = "", string p_osVersion = "", string p_deviceName = "", ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		string p_string = string.Format("GameInitialization : NID:{0},\tDID:{1},\tPlatform:{2},\nVersion:{3},\tFilter:{4},\tLang:{5},\nGender:{6}.\tmarket:{7},\tos:{8},\tdeviceName:{9}", p_nubeeId, p_deviceId, p_platform, p_version, p_filter, p_language, p_gender, p_market, p_osVersion, p_deviceName);
		Dbg.Log(p_string, Dbg.NBLogType.SERVICES);
		GameInitialization gameInitialization = new GameInitialization();
		gameInitialization.id = GetFreeHandleId();
		m_handles.Add(gameInitialization);
		gameInitialization.nubeeId = p_nubeeId;
		gameInitialization.deviceId = p_deviceId;
		gameInitialization.platform = p_platform;
		gameInitialization.version = p_version;
		gameInitialization.filter = p_filter;
		gameInitialization.language = p_language;
		gameInitialization.gender = p_gender;
		gameInitialization.market = p_market;
		gameInitialization.osVersion = p_osVersion;
		gameInitialization.deviceName = p_deviceName;
		gameInitialization.successCb = p_successCb;
		gameInitialization.failCb = p_failCb;
		return gameInitialization;
	}

	public ServiceHandle SendPromoCode(string p_nubeeId, string p_deviceId, string p_promoId, string p_promoCode, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		Dbg.Log("Service Call: SendPromoCode NID: " + p_nubeeId + " DID: " + p_deviceId + " PID: " + p_promoId + " PC: " + p_promoCode, Dbg.NBLogType.SERVICES);
		SendPromoCode sendPromoCode = new SendPromoCode();
		sendPromoCode.id = GetFreeHandleId();
		m_handles.Add(sendPromoCode);
		sendPromoCode.nubeeId = p_nubeeId;
		sendPromoCode.deviceId = p_deviceId;
		sendPromoCode.promoId = p_promoId;
		sendPromoCode.promoCode = p_promoCode;
		sendPromoCode.successCb = p_successCb;
		sendPromoCode.failCb = p_failCb;
		return sendPromoCode;
	}

	public ServiceHandle GetDragonShop(string p_nubeeId, string p_deviceId, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		Dbg.Log("Service Call: GetDragonShop NID: " + p_nubeeId + " DID: " + p_deviceId, Dbg.NBLogType.SERVICES);
		GetDragonShop getDragonShop = new GetDragonShop();
		getDragonShop.id = GetFreeHandleId();
		m_handles.Add(getDragonShop);
		getDragonShop.nubeeId = p_nubeeId;
		getDragonShop.deviceId = p_deviceId;
		getDragonShop.successCb = p_successCb;
		getDragonShop.failCb = p_failCb;
		return getDragonShop;
	}

	public ServiceHandle ExchangeDragonPoints(string p_nubeeId, string p_deviceId, string p_dragonShopId, string p_requestId, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		Dbg.Log("Service Call: ExchangeDragonPoints NID: " + p_nubeeId + " DID: " + p_deviceId + " DSID: " + p_dragonShopId, Dbg.NBLogType.SERVICES);
		ExchangeDragonPoints exchangeDragonPoints = new ExchangeDragonPoints();
		exchangeDragonPoints.id = GetFreeHandleId();
		m_handles.Add(exchangeDragonPoints);
		exchangeDragonPoints.nubeeId = p_nubeeId;
		exchangeDragonPoints.deviceId = p_deviceId;
		exchangeDragonPoints.dragonShopId = p_dragonShopId;
		exchangeDragonPoints.requestId = p_requestId;
		exchangeDragonPoints.successCb = p_successCb;
		exchangeDragonPoints.failCb = p_failCb;
		return exchangeDragonPoints;
	}

	public ServiceHandle GetHolyShop(string p_nubeeId, string p_deviceId, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		Dbg.Log("Service Call: GetHolyShop NID: " + p_nubeeId + " DID: " + p_deviceId, Dbg.NBLogType.SERVICES);
		GetHolyShop getHolyShop = new GetHolyShop();
		getHolyShop.id = GetFreeHandleId();
		m_handles.Add(getHolyShop);
		getHolyShop.nubeeId = p_nubeeId;
		getHolyShop.deviceId = p_deviceId;
		getHolyShop.successCb = p_successCb;
		getHolyShop.failCb = p_failCb;
		return getHolyShop;
	}

	public ServiceHandle ExchangeHolyDiamonds(string p_nubeeId, string p_deviceId, string p_holyShopId, string p_requestId, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		Dbg.Log("Service Call: ExchangeHolyDiamonds NID: " + p_nubeeId + " DID: " + p_deviceId + " DSID: " + p_holyShopId, Dbg.NBLogType.SERVICES);
		ExchangeHolyDiamonds exchangeHolyDiamonds = new ExchangeHolyDiamonds();
		exchangeHolyDiamonds.id = GetFreeHandleId();
		m_handles.Add(exchangeHolyDiamonds);
		exchangeHolyDiamonds.nubeeId = p_nubeeId;
		exchangeHolyDiamonds.deviceId = p_deviceId;
		exchangeHolyDiamonds.holyShopId = p_holyShopId;
		exchangeHolyDiamonds.requestId = p_requestId;
		exchangeHolyDiamonds.successCb = p_successCb;
		exchangeHolyDiamonds.failCb = p_failCb;
		return exchangeHolyDiamonds;
	}

	public ServiceHandle MetapsClaim(string p_nubeeId, string p_deviceId, string p_requestId, string p_diamondToadd, string p_packageName, string p_campaignId, ServiceHandle.ServiceCompleteCallback p_successCb = null, ServiceHandle.ServiceErrorCallback p_failCb = null)
	{
		MetapsClaim metapsClaim = new MetapsClaim();
		metapsClaim.id = GetFreeHandleId();
		m_handles.Add(metapsClaim);
		metapsClaim.nubeeId = p_nubeeId;
		metapsClaim.deviceId = p_deviceId;
		metapsClaim.requestId = p_requestId;
		metapsClaim.diamondsToAdd = p_diamondToadd;
		metapsClaim.packageName = p_packageName;
		metapsClaim.campaignId = p_campaignId;
		metapsClaim.successCb = p_successCb;
		metapsClaim.failCb = p_failCb;
		return metapsClaim;
	}

	public void ReleaseServiceHandle(ServiceHandle p_handle)
	{
		m_freeIds.Add(p_handle.id);
		m_handles.Remove(p_handle);
		p_handle = null;
	}

	public static string GenerateChecksum(string p_objectToEncrypt, string p_secretKey)
	{
		if (p_objectToEncrypt == null)
		{
			Dbg.LogError("object to encrypt cannot be null!");
			return string.Empty;
		}
		if (p_secretKey == null)
		{
			Dbg.LogError("secretkey cannot be null!");
			return string.Empty;
		}
		UTF8Encoding uTF8Encoding = new UTF8Encoding();
		byte[] bytes = uTF8Encoding.GetBytes(p_secretKey);
		HMACSHA512 hMACSHA = new HMACSHA512(bytes);
		byte[] bytes2 = uTF8Encoding.GetBytes(p_objectToEncrypt);
		byte[] p_buff = hMACSHA.ComputeHash(bytes2);
		return ByteToString(p_buff);
	}

	public static string ByteToString(byte[] p_buff)
	{
		return BitConverter.ToString(p_buff).Replace("-", string.Empty).ToLower();
	}

	protected int GetFreeHandleId()
	{
		int num = 0;
		if (m_freeIds.Count > 0)
		{
			num = m_freeIds[0];
			m_freeIds.RemoveAt(0);
			return num;
		}
		return m_handles.Count;
	}

	public bool IsSecretKeyExpired()
	{
		return TimeUtil.GetUnixEpochTime() >= m_lastKeyTimestamp + m_expiration;
	}

	public bool HasSecretKey()
	{
		return m_key != string.Empty;
	}

	public bool HasNubeeAccount()
	{
		return m_nubeeId != string.Empty && m_deviceId != string.Empty;
	}

	public void Update()
	{
		if (m_handles.Count < 1)
		{
			return;
		}
		int num = 0;
		while (num < m_handles.Count)
		{
			ServiceHandle serviceHandle = m_handles[num];
			if (serviceHandle.result == ConnectionResults.None)
			{
				if (serviceHandle.IsNubeeAccountRequired() && !HasNubeeAccount())
				{
					ReleaseServiceHandle(serviceHandle);
					continue;
				}
				if (serviceHandle.IsKeyRequired() && IsSecretKeyExpired())
				{
					if (!m_newKeyRetrieveCalled && m_keyExpireRetryCounter < m_keyExpireRetries)
					{
						m_keyExpireRetryCounter++;
						ServiceHandle serviceHandle2 = RetrieveSecretKey(m_nubeeId, m_deviceId, OnSecretKeyRetrieveSuccess, OnSecretKeyRetrieveFail);
						StartCoroutine(serviceHandle2.Execute());
						m_newKeyRetrieveCalled = true;
					}
					num++;
					continue;
				}
				StartCoroutine(serviceHandle.Execute());
			}
			if (serviceHandle.result == ConnectionResults.Success)
			{
				ReleaseServiceHandle(serviceHandle);
			}
			else
			{
				num++;
			}
		}
	}

	protected void OnSecretKeyRetrieveSuccess(ServiceHandle p_handle)
	{
		m_keyExpireRetryCounter = 0;
		SecretKeyInfo secretKeyInfo = (SecretKeyInfo)p_handle.returnValue;
		m_key = secretKeyInfo.secretKey;
		m_expiration = secretKeyInfo.remaining;
		m_lastKeyTimestamp = TimeUtil.GetUnixEpochTime();
		m_newKeyRetrieveCalled = false;
	}

	protected void ResetRetryCount()
	{
		m_newKeyRetrieveCalled = false;
		m_keyExpireRetryCounter = 0;
	}

	protected IEnumerator ResetKeyRetrieveCalledVariable()
	{
		yield return new MBWaitForSeconds(1f);   // PORT (CD-059)
		m_newKeyRetrieveCalled = false;
	}

	protected void OnSecretKeyRetrieveFail(ServiceHandle p_handle, string p_error)
	{
		m_key = string.Empty;
		if (m_keyExpireRetryCounter < m_keyExpireRetries)
		{
			SingletonComponent<JobManager>.Instance.StartCoroutine(ResetKeyRetrieveCalledVariable());
			return;
		}
		if (p_handle == null)
		{
			UIPopupController.Instance.HideLoadingPopup();
			UIPopupController.Instance.ShowServerGenericError(ResetRetryCount);
			return;
		}
		if (p_handle.IsNoInternet())
		{
			UIPopupController.Instance.HideLoadingPopup();
			UIPopupController.Instance.ShowNoNetMessage(ResetRetryCount);
			return;
		}
		int nReturnCode = p_handle.nReturnCode;
		if (nReturnCode == 302)
		{
			UIPopupController.Instance.HideLoadingPopup();
			UIPopupController.Instance.ShowServerGenericError(ResetRetryCount);
		}
		else
		{
			UIPopupController.Instance.HideLoadingPopup();
			UIPopupController.Instance.ShowServerGenericError(ResetRetryCount);
		}
	}

	public static void SetNubeeAccount(string p_nubeeId, string p_deviceId)
	{
		SingletonComponent<Services>.Instance.m_nubeeId = p_nubeeId;
		SingletonComponent<Services>.Instance.m_deviceId = p_deviceId;
	}
}
