using System;
using System.Collections.Generic;
using Models;
using ServiceCalls;
using UnityEngine;

public class AchievementManager : SingletonComponent<AchievementManager>
{
	public class ClaimBtnActionAndText
	{
		public string m_Action = string.Empty;

		public string m_Text = string.Empty;

		public ClaimBtnActionAndText(string p_action, string p_text)
		{
			m_Action = p_action;
			m_Text = p_text;
		}
	}

	public class AchievementRewards
	{
		public string achievementId = string.Empty;

		public int rewardXP;

		public int rewardIgc;

		public int rewardPrem;

		public int currentPlayerXP;

		public int currentPlayerLevel;

		public List<string> rewardItemIds = new List<string>();

		public List<string> rewardPetIds = new List<string>();

		public CharacterAccountDetails characterData = new CharacterAccountDetails();

		public override string ToString()
		{
			return string.Format("AchievementRewards rewardXP:{0}, rewardIgc:{1}, rewardPrem:{2} ", rewardXP, rewardIgc, rewardPrem);
		}
	}

	public class AchievementItem
	{
		public string db_id = string.Empty;

		public string id = string.Empty;

		public string ios_id = string.Empty;

		public string title = string.Empty;

		public string description = string.Empty;

		public bool marked;

		private bool achieved;

		private bool claimed;

		public bool displayed;

		public int progress;

		public int targetProgress = 1;

		public AchievementRewards rewards = new AchievementRewards();

		public bool Achieved
		{
			get
			{
				return achieved;
			}
			set
			{
				achieved = value;
				if (MainMenu.Instance != null && SingletonComponent<AchievementManager>.Instance != null)
				{
					MainMenu.Instance.UpdateHintCounter(MainMenu.Instance.m_AchievedCounter, (SingletonComponent<AchievementManager>.Instance.NumOfCompleted() - SingletonComponent<AchievementManager>.Instance.NumOfClaimed()).ToString());
				}
			}
		}

		public bool Claimed
		{
			get
			{
				return claimed;
			}
			set
			{
				claimed = value;
				if (MainMenu.Instance != null && SingletonComponent<AchievementManager>.Instance != null)
				{
					MainMenu.Instance.UpdateHintCounter(MainMenu.Instance.m_AchievedCounter, (SingletonComponent<AchievementManager>.Instance.NumOfCompleted() - SingletonComponent<AchievementManager>.Instance.NumOfClaimed()).ToString());
				}
			}
		}

		public float ProgressAsPercentage
		{
			get
			{
				if ((float)targetProgress > 0f)
				{
					return (float)progress / (float)targetProgress * 100f;
				}
				return 0f;
			}
		}

		public override string ToString()
		{
			return string.Format("AchievementItem id: {0}, ios_id: {1}, achieved: {2}, claimed: {3}, displayed: {4}, title: {5}, description: {6}, progress: {7}, targetProgress: {8} ...rewards:{9}", id, ios_id, Achieved, claimed, displayed, title, description, progress, targetProgress, rewards.ToString());
		}
	}

	public class MarkingRequest
	{
		public string id = string.Empty;

		public string platformId = string.Empty;

		public float progress;

		public MarkingRequest(string p_id, string p_platformId, float p_progress)
		{
			id = p_id;
			platformId = p_platformId;
			progress = p_progress;
		}
	}

	public delegate void AchievementManagerCb(bool p_success);

	public delegate void AchievementClaimSuccessCb();

	[SerializeField]
	private NotificationController m_notificationController;

	private NotificationController m_notificationCtrlRef;

	public bool m_AchievementMapBuilt;

	public AchievementClaimSuccessCb m_AchievementClaimSuccessCb;

	public PlayerData m_OldPlayerData = new PlayerData();

	public Dictionary<string, ClaimBtnActionAndText> m_ActionMap = new Dictionary<string, ClaimBtnActionAndText>();

	public bool m_IsGettingAchievementInfo;

	private bool m_showPanel;

	private AchievementRewards tempRewardInfo;

	private MarkingRequest m_currentMarking;

	private AchievementManagerCb m_getAchievementsCb;

	private bool m_ready;

	private List<MarkingRequest> m_forMarking = new List<MarkingRequest>();

	private List<AchievementItem> m_displayQueue = new List<AchievementItem>();

	private Dictionary<string, AchievementItem> m_achievements = new Dictionary<string, AchievementItem>();

	public bool m_NeedToShineAchievementBtn
	{
		get
		{
			AchievementItem achievement = GetAchievement("achievement_system_1");
			AchievementItem achievement2 = GetAchievement("achievement_system_2");
			if (achievement != null && achievement2 != null)
			{
				return !achievement.Achieved || !achievement2.Achieved;
			}
			return false;
		}
	}

	public void InitManager(AchievementManagerCb p_callback)
	{
		NBProfiler.BeginTask("[ACHIEVEMENT_MANAGER][INIT_MANAGER]");
		m_getAchievementsCb = p_callback;
		GetAchievementsFromServer();
		ClaimBtnActionAndText value = new ClaimBtnActionAndText("OnFbLikeUsClick", "option_facebook");
		ClaimBtnActionAndText value2 = new ClaimBtnActionAndText("OnRateUsClick", "option_rate_us");
		m_ActionMap.Add("achievement_system_1", value);
		m_ActionMap.Add("achievement_system_2", value2);
	}

	public void GetAchievementsFromServer(bool p_showPanel = false)
	{
		m_showPanel = p_showPanel;
		if (SingletonComponent<Services>.Instance != null)
		{
			m_IsGettingAchievementInfo = true;
			if (UIPopupController.Instance != null)
			{
				UIPopupController.Instance.ShowLoadingPopup(string.Empty, string.Empty);
			}
			SingletonComponent<Services>.Instance.GetAchievements(SingletonComponent<Services>.Instance.NubeeId, SingletonComponent<Services>.Instance.DeviceId, OnGetAchievementsSuccess, OnGetAchievementsFail);
		}
		else
		{
			Dbg.LogError("Services.Instance is null, the game should shut down");
		}
	}

	private void UpdateAchievementProgressToServer(Dictionary<string, int> p_achieveIdProgressMap)
	{
		if (SingletonComponent<Services>.Instance != null)
		{
			SingletonComponent<Services>.Instance.UpdateAchievementProgress(SingletonComponent<Services>.Instance.NubeeId, SingletonComponent<Services>.Instance.DeviceId, p_achieveIdProgressMap, OnUpdateAchievementProgressSuccess, OnUpdateAchievementProgressFail);
		}
		else
		{
			Dbg.LogError("Services.Instance is null, the game should shut down");
		}
	}

	private void OnUpdateAchievementProgressSuccess(Services.ServiceHandle p_handle)
	{
		List<string> list = p_handle.returnValue as List<string>;
		if (list == null)
		{
			return;
		}
		foreach (string item in list)
		{
			AchievementItem achievement = GetAchievement(item);
			if (achievement != null)
			{
				achievement.Achieved = true;
				if (achievement.id == "achievement_system_1" || achievement.id == "achievement_system_2")
				{
					achievement.Claimed = true;
				}
				if (!achievement.displayed)
				{
					achievement.displayed = true;
					MBPrefs.SetInt(achievement.id + ".displayed", 1);
					Dbg.Log("[Info] Try to show notification popup");
					if (m_notificationCtrlRef == null)
					{
						GameObject gameObject = UnityEngine.Object.Instantiate(m_notificationController.gameObject, new Vector3(2000f, 0f, 0f), Quaternion.identity) as GameObject;
						NotificationController component = gameObject.GetComponent<NotificationController>();
						if (component != null)
						{
							m_notificationCtrlRef = component;
							component.ShowNotification(achievement);
						}
					}
					else
					{
						m_notificationCtrlRef.ShowNotification(achievement);
					}
				}
				else
				{
					Dbg.LogEx("The achievement {0} has been shown in this machine before, no need to show it again", Dbg.NBLogType.COMMON_LOG, achievement.id);
				}
			}
			else
			{
				Dbg.LogWarn("Unable to find achievement id: " + item);
			}
			Messenger<AchievementItem>.Invoke("achievement_update", achievement);
		}
	}

	private void OnUpdateAchievementProgressFail(Services.ServiceHandle p_handle, string p_error)
	{
		Dbg.LogWarn("UpdateAchievementProgress failed, err: " + ServiceCallUtils.GetErrorMessage(p_handle));
	}

	public Dictionary<string, AchievementItem> GetAchievements()
	{
		return m_achievements;
	}

	public AchievementItem GetAchievement(string p_achievementId)
	{
		if (m_achievements.ContainsKey(p_achievementId))
		{
			return m_achievements[p_achievementId];
		}
		Dbg.LogWarn("[Warn] Unable to find achievement id: " + p_achievementId + " , return null");
		return null;
	}

	public void ClaimAchievement(AchievementItem p_achievement, AchievementClaimSuccessCb p_claimCb = null)
	{
		if (SingletonComponent<Services>.Instance != null)
		{
			UIPopupController.Instance.ShowLoadingPopupNetWait();
			m_AchievementClaimSuccessCb = p_claimCb;
			SingletonComponent<Services>.Instance.ClaimAchievement(SingletonComponent<Services>.Instance.NubeeId, SingletonComponent<Services>.Instance.DeviceId, p_achievement.id, OnClaimAchievementSuccess, OnClaimAchievementFail);
		}
		else
		{
			Dbg.LogError("Services.Instance is null, the game should shut down");
		}
	}

	public bool IsReady()
	{
		return m_ready;
	}

	private void GameCenterAuthenticationCb(bool p_success)
	{
	}

	public void InitManager(Dictionary<int, AchievementItem> p_achievementItems)
	{
		List<KeyValuePair<int, AchievementItem>> list = new List<KeyValuePair<int, AchievementItem>>(p_achievementItems);
		MBSort.Sort(list, (KeyValuePair<int, AchievementItem> first, KeyValuePair<int, AchievementItem> second) => first.Key.CompareTo(second.Key));
		Dictionary<int, AchievementItem> dictionary = new Dictionary<int, AchievementItem>();
		foreach (KeyValuePair<int, AchievementItem> item in list)
		{
			dictionary.Add(item.Key, item.Value);
		}
		if (dictionary != null)
		{
			foreach (KeyValuePair<int, AchievementItem> item2 in dictionary)
			{
				AchievementItem value = item2.Value;
				if (!value.Achieved || !value.marked)
				{
				}
				if (m_achievements.ContainsKey(value.id))
				{
					m_achievements.Remove(value.id);
				}
				value.displayed = MBPrefs.GetInt(value.id + ".displayed", 0) > 0;
				m_achievements.Add(value.id, value);
			}
		}
		if (m_showPanel)
		{
			ShowPanel();
		}
		if (m_getAchievementsCb != null)
		{
			m_getAchievementsCb(true);
		}
		NBProfiler.EndTask("[ACHIEVEMENT_MANAGER][INIT_MANAGER]");
		m_ready = true;
		m_AchievementMapBuilt = true;
	}

	private void OnGetAchievementsSuccess(Services.ServiceHandle p_handle)
	{
		Dbg.Log("[Info] OnGetAchievementsSuccess");
		Dictionary<int, AchievementItem> collection = p_handle.returnValue as Dictionary<int, AchievementItem>;
		List<KeyValuePair<int, AchievementItem>> list = new List<KeyValuePair<int, AchievementItem>>(collection);
		MBSort.Sort(list, (KeyValuePair<int, AchievementItem> first, KeyValuePair<int, AchievementItem> second) => first.Key.CompareTo(second.Key));
		Dictionary<int, AchievementItem> dictionary = new Dictionary<int, AchievementItem>();
		foreach (KeyValuePair<int, AchievementItem> item in list)
		{
			dictionary.Add(item.Key, item.Value);
		}
		if (dictionary != null)
		{
			foreach (KeyValuePair<int, AchievementItem> item2 in dictionary)
			{
				AchievementItem value = item2.Value;
				if (value.Achieved)
				{
					if (!value.marked)
					{
					}
					if (value.id == "achievement_system_1")
					{
						value.Claimed = true;
						SingletonComponent<SaveData>.Instance.GameData.SetFbLikedUsFlag(true);
					}
					if (value.id == "achievement_system_2")
					{
						value.Claimed = true;
						SingletonComponent<SaveData>.Instance.GameData.SetAppStoreRateUsFlag(true);
					}
				}
				if (m_achievements.ContainsKey(value.id))
				{
					m_achievements.Remove(value.id);
				}
				value.displayed = MBPrefs.GetInt(value.id + ".displayed", 0) > 0;
				m_achievements.Add(value.id, value);
			}
		}
		if (m_showPanel)
		{
			ShowPanel();
		}
		if (m_getAchievementsCb != null)
		{
			m_getAchievementsCb(true);
		}
		m_IsGettingAchievementInfo = false;
		NBProfiler.EndTask("[ACHIEVEMENT_MANAGER][INIT_MANAGER]");
		m_ready = true;
		if (UIPopupController.Instance != null)
		{
			UIPopupController.Instance.HideLoadingPopup();
		}
		ClearWeekendEventProgress();
	}

	public void OnGetAchievementsFail(Services.ServiceHandle p_handle, string p_error)
	{
		if (m_getAchievementsCb != null)
		{
			m_getAchievementsCb(false);
		}
		m_IsGettingAchievementInfo = false;
		if (UIPopupController.Instance != null)
		{
			UIPopupController.Instance.HideFullScreenLoading();
			if (m_showPanel)
			{
				if (p_handle == null || p_handle.IsNoInternet())
				{
					UIPopupController.Instance.ShowNoNetMessage();
				}
				else
				{
					UIPopupController.Instance.ShowServerGenericError(null, p_handle.nReturnCode);
				}
			}
		}
		NBProfiler.EndTask("[ACHIEVEMENT_MANAGER][INIT_MANAGER]");
	}

	private void Update()
	{
		if (m_ready)
		{
		}
	}

	private void OnMarkingToGameCenter(bool p_success)
	{
		Dbg.Log("OnMarkingToGameCenter, p_success:" + p_success);
		if (!p_success)
		{
			m_currentMarking = null;
			if (m_forMarking != null && m_forMarking.Count > 0)
			{
				m_forMarking.RemoveAt(0);
			}
		}
		else if (m_currentMarking == null)
		{
			Dbg.LogError("[AchievementManager::OnMarkingToGameCenter] m_currentMarking is null");
		}
		else
		{
			List<string> list = new List<string>();
			list.Add(m_currentMarking.id);
			SingletonComponent<Services>.Instance.MarkAchievements(SingletonComponent<Services>.Instance.NubeeId, SingletonComponent<Services>.Instance.DeviceId, list, OnMarkAchievementSuccess, OnMarkAchievementFail);
		}
	}

	private void OnMarkAchievementSuccess(Services.ServiceHandle p_handle)
	{
		m_currentMarking = null;
		m_forMarking.RemoveAt(0);
	}

	private void OnMarkAchievementFail(Services.ServiceHandle p_handle, string p_error)
	{
		Dbg.LogError("Marking achievement failed: " + ServiceCallUtils.GetErrorMessage(p_handle));
		if (SingletonComponent<Services>.Instance != null)
		{
			List<string> list = new List<string>();
			list.Add(m_currentMarking.id);
			SingletonComponent<Services>.Instance.MarkAchievements(SingletonComponent<Services>.Instance.NubeeId, SingletonComponent<Services>.Instance.DeviceId, list, OnMarkAchievementSuccess, OnMarkAchievementFail);
		}
		else
		{
			Dbg.LogError("Services.Instance is null, the game should shut down");
		}
	}

	private void OnUnlockAchievementSuccess(Services.ServiceHandle p_handle)
	{
	}

	private void OnDisplayAchievementEnd()
	{
		m_displayQueue.RemoveAt(0);
	}

	private void OnClaimAchievementSuccess(Services.ServiceHandle p_handle)
	{
		UIPopupController.Instance.HideLoadingPopup();
		tempRewardInfo = p_handle.returnValue as AchievementRewards;
		if (tempRewardInfo != null)
		{
			PlayerData _playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
			tempRewardInfo.characterData.ToPlayerData(ref _playerData);
			if (!m_achievements.ContainsKey(tempRewardInfo.achievementId))
			{
				return;
			}
			m_achievements[tempRewardInfo.achievementId].Claimed = true;
			if (UIPopupController.Instance != null)
			{
				if (m_AchievementClaimSuccessCb != null)
				{
					m_AchievementClaimSuccessCb();
				}
				UIPopupController.Instance.ShowAchievementClaimPopup(tempRewardInfo, OnClaimAchievementPopupClose);
				if (null != SingletonComponent<SoundManager>.Instance)
				{
					SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_level_up_notification");
				}
			}
			else
			{
				Dbg.LogWarn("UIPopupController.Instance is null, cannot show popup");
			}
		}
		else
		{
			Dbg.LogError("[Error] Unable to find the return data of ClaimAchievement");
		}
	}

	private void OnClaimAchievementFail(Services.ServiceHandle p_handle, string p_error)
	{
		UIPopupController.Instance.HideLoadingPopup();
		Dbg.LogError("OnClaimAchievementFail: " + ServiceCallUtils.GetErrorMessage(p_handle));
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

	public void OnClaimAchievementPopupClose()
	{
		Dbg.Log("[Enter] OnClaimAchievementPopupClose");
		if (MainMenu.Instance != null)
		{
			PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
			MainMenu.Instance.SetIgcValue(m_OldPlayerData.m_userData.inGameCurrency + tempRewardInfo.rewardIgc);
			MainMenu.Instance.SetPremiumValue(m_OldPlayerData.m_userData.premiumCurrency + tempRewardInfo.rewardPrem);
			MainMenu.Instance.SetXpValue(m_OldPlayerData.m_userData.currentXp + tempRewardInfo.rewardXP);
			if (m_OldPlayerData != null && m_OldPlayerData.Level < tempRewardInfo.characterData.level)
			{
				MainMenu.Instance.m_LastLevel = m_OldPlayerData.Level;
				playerData.Level = tempRewardInfo.currentPlayerLevel;
				playerData.Xp = tempRewardInfo.currentPlayerXP;
				Dbg.Log("Level before claim reward : " + MainMenu.Instance.m_LastLevel);
				MainMenu.Instance.RefreshPlayerStats();
			}
			if (MainMenu.Instance.currScreen is HomeMenuScreen)
			{
				HomePanel homePanel = MainMenu.Instance.currScreen.currentPanel as HomePanel;
				if (homePanel != null)
				{
					homePanel.SetNotificationAchievements(NumOfCompleted() - NumOfClaimed());
				}
			}
		}
		else
		{
			Dbg.LogWarn("MainMenu.Instance is null, cannot update IGC, XP, PGC value");
		}
		tempRewardInfo = null;
		Dbg.Log("[Leave] OnClaimAchievementPopupClose");
	}

	public void UpdateAchievementProgress(List<AchievementItem> p_achieveItems)
	{
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		foreach (AchievementItem p_achieveItem in p_achieveItems)
		{
			if (p_achieveItem != null)
			{
				dictionary.Add(p_achieveItem.id, p_achieveItem.progress);
			}
		}
		if (dictionary.Count > 0)
		{
			UpdateAchievementProgressToServer(dictionary);
		}
		else
		{
			Dbg.LogWarn("there's no achievement progress to update");
		}
	}

	public int TotalNumAchievements()
	{
		return m_achievements.Count;
	}

	public int NumOfCompleted()
	{
		int num = 0;
		foreach (string key in m_achievements.Keys)
		{
			if (m_achievements[key].Achieved)
			{
				num++;
			}
		}
		return num;
	}

	public int NumOfClaimed()
	{
		int num = 0;
		foreach (string key in m_achievements.Keys)
		{
			if (m_achievements[key].Claimed)
			{
				num++;
			}
		}
		return num;
	}

	public void ClearWeekendEventProgress()
	{
		bool flag = false;
		AchievementItem achievementItem = null;
		if (m_achievements.ContainsKey("achievement_event_2"))
		{
			achievementItem = m_achievements["achievement_event_2"];
		}
		if (achievementItem != null)
		{
			if (!achievementItem.Achieved && (DateTime.UtcNow.DayOfWeek == DayOfWeek.Monday || DateTime.UtcNow.DayOfWeek == DayOfWeek.Tuesday || DateTime.UtcNow.DayOfWeek == DayOfWeek.Wednesday || DateTime.UtcNow.DayOfWeek == DayOfWeek.Thursday))
			{
				Dbg.Log("Today is Monday day, and the weekend event achievement is not achieved, try to reset the weekend achievement progress");
				flag = true;
			}
			else
			{
				long unixEpochTime = TimeUtil.GetUnixEpochTime();
				if (SingletonComponent<SaveData>.Instance.GameData.LatestWeekFriday > 0)
				{
					if (SingletonComponent<SaveData>.Instance.GameData.LatestWeekSaturday > 0)
					{
						if (DateTime.UtcNow.DayOfWeek == DayOfWeek.Sunday && !SingletonComponent<SaveData>.Instance.GameData.IsSameWeek(SingletonComponent<SaveData>.Instance.GameData.LatestWeekSaturday, unixEpochTime))
						{
							Dbg.Log("User has Friday and Saturday time stamp, and login on Sunday, and if the Saturday time stamp is not the previous week of current time.");
							flag = true;
						}
					}
					else if (DateTime.UtcNow.DayOfWeek == DayOfWeek.Saturday)
					{
						if (!SingletonComponent<SaveData>.Instance.GameData.IsSameWeek(SingletonComponent<SaveData>.Instance.GameData.LatestWeekFriday, unixEpochTime))
						{
							Dbg.Log("User has Friday time stamp but no Saturday time stamp, and login on Saturday, and if the Friday time stamp is not the same week with current time.");
							flag = true;
						}
					}
					else
					{
						Dbg.Log("User has Friday time stamp but no saturday time stamp, and today is not saturday. So reset all.");
						flag = true;
					}
				}
				else if (DateTime.UtcNow.DayOfWeek == DayOfWeek.Sunday || DateTime.UtcNow.DayOfWeek == DayOfWeek.Saturday)
				{
					Dbg.Log("User has no Friday time stamp(no matter user has Sat. or Sun. time stamp), but login on Sat. or Sun.");
					flag = true;
				}
			}
		}
		if (flag)
		{
			SingletonComponent<SaveData>.Instance.GameData.ResetWeekEndTimeStamp();
			UpdateAchievementProgress(SingletonComponent<SaveData>.Instance.GameData.PrepareAchiItemList(2, 2, 0, "achievement_event_"));
			SingletonComponent<SaveData>.Instance.GameData.LoadAchievementData();
		}
	}

	private AchievementItem FindAchievementOfId(string p_id)
	{
		if (m_achievements.ContainsKey(p_id))
		{
			return m_achievements[p_id];
		}
		return null;
	}

	private void ShowPanel()
	{
		if (MainMenu.Instance == null)
		{
			Dbg.LogWarn("MainMenu.Instance is null, do nothing");
		}
		else
		{
			MainMenu.Instance.ShowAchievementInfos();
		}
	}
}
