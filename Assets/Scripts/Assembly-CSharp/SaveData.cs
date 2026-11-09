using System.Collections.Generic;
using UnityEngine;

public class SaveData : SingletonComponent<SaveData>
{
	public class MsData
	{
		private const string SAVE_RECEIPT_HASH = "MsData::m_receiptHash";

		private bool m_saveOcha;

		private bool m_tutorialFinished;

		private int m_fusionSuccessCount;

		private bool[] m_isCityDone = new bool[20];

		private int m_lastAct;

		private bool m_hasEnteredAnEvent;

		private long m_latestWeekFriday;

		private long m_latestWeekSaturday;

		private long m_latestWeekSunday;

		private long m_latestWeekdayEvent;

		private long m_latestWeeklyEvent;

		private long m_latestMonthlyEvent;

		private int[] m_defeatMonthlyEventYear = new int[12];

		private int m_highestGoldInDungeon;

		private int m_openPremiumTreasureCount;

		private int m_openSocialTreasureCount;

		private bool m_hasEvolvedAnItem;

		private int m_highestEvolvedTier;

		private int m_monsterKillCount;

		private bool m_fbLikedUs;

		private bool m_appStoreRateUs;

		private string m_language = "English";

		private string m_previousLanguage = "English";

		private bool m_genderMale = true;

		private bool m_genderChosen;

		private string m_nubeeId = string.Empty;

		private string m_deviceId = string.Empty;

		private string m_nameCharacter = GLOBAL.CHARACTER_DEFAULT_NAME;

		private bool m_nameGiven;

		private bool m_characterCreated;

		private bool m_firstTimeToRun = true;

		private long m_lastAppRun;

		private bool m_needVibrate;

		private string m_szFBID = string.Empty;

		private string m_szServerFBID = string.Empty;

		private string m_szFBServerName = string.Empty;

		private string m_szFBToken = string.Empty;

		private string m_szServerFBToken = string.Empty;

		private string m_szReceiptHash = string.Empty;

		private long m_previousLeaderboardTimeStamp;

		private long m_bpRemainingTime;

		private int m_tutorialProgress;

		private long m_pauseTimeStamp;

		public long LastAppRun
		{
			get
			{
				return m_lastAppRun;
			}
			set
			{
				m_lastAppRun = value;
			}
		}

		public string Language
		{
			get
			{
				return m_language;
			}
			set
			{
				m_language = value;
			}
		}

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

		public long PauseTimeStamp
		{
			get
			{
				return m_pauseTimeStamp;
			}
			set
			{
				m_pauseTimeStamp = value;
			}
		}

		public long BpRemainingTime
		{
			get
			{
				return m_bpRemainingTime;
			}
			set
			{
				m_bpRemainingTime = value;
			}
		}

		public string CharacterName
		{
			get
			{
				return m_nameCharacter;
			}
			set
			{
				m_nameCharacter = value;
				m_nameGiven = true;
				MBPrefs.SetInt("MsData::m_nameGiven", m_nameGiven ? 1 : 0);
				MBPrefs.SetString("MsData::m_nameCharacter", m_nameCharacter);
				MBPrefs.Save();
			}
		}

		public bool CharacterCreated
		{
			get
			{
				int num = MBPrefs.GetInt("MsData::m_characterCreated", 0);
				Dbg.Log("MsData::m_characterCreated is " + num);
				m_characterCreated = num > 0;
				return m_characterCreated;
			}
			set
			{
				m_characterCreated = value;
				MBPrefs.SetInt("MsData::m_characterCreated", m_characterCreated ? 1 : 0);
				MBPrefs.Save();
			}
		}

		public bool IsFirstTimeToRun
		{
			get
			{
				return m_firstTimeToRun;
			}
			set
			{
				m_firstTimeToRun = value;
			}
		}

		public bool IsDeviceLanguageChanged
		{
			get
			{
				m_previousLanguage = MBPrefs.GetString("MsData::m_previousLanguage");
				return m_previousLanguage != m_language;
			}
		}

		public string FBID
		{
			get
			{
				return m_szFBID;
			}
			set
			{
				m_szFBID = value;
				MBPrefs.SetString("MsData::m_fbid", m_szFBID);
				MBPrefs.Save();
			}
		}

		public string ServerFBID
		{
			get
			{
				return m_szServerFBID;
			}
		}

		public bool NeedUpdateFBID
		{
			get
			{
				return m_szFBID.CompareTo(string.Empty) != 0 && m_szFBID != m_szServerFBID;
			}
			set
			{
				if (!value && m_szFBID.CompareTo(m_szServerFBID) != 0)
				{
					m_szServerFBID = m_szFBID;
					MBPrefs.SetString("MsData::m_serverfbid", m_szServerFBID);
					MBPrefs.Save();
				}
			}
		}

		public string ServerFBName
		{
			get
			{
				return m_szFBServerName;
			}
		}

		public bool NeedUpdateFBName
		{
			get
			{
				return m_nameCharacter.CompareTo(m_szFBServerName) != 0;
			}
			set
			{
				if (!value && m_nameCharacter.CompareTo(m_szFBServerName) != 0)
				{
					m_szFBServerName = m_nameCharacter;
					MBPrefs.SetString("MsData::m_serverfbname", m_szFBServerName);
					MBPrefs.Save();
				}
			}
		}

		public string FBToken
		{
			get
			{
				return m_szFBToken;
			}
			set
			{
				m_szFBToken = value;
				MBPrefs.SetString("MsData::m_fbtoken", m_szFBToken);
				MBPrefs.Save();
			}
		}

		public string ServerFBToken
		{
			get
			{
				return m_szServerFBToken;
			}
		}

		public bool NeedUpdateFBToken
		{
			get
			{
				return m_szFBToken.CompareTo(m_szServerFBToken) != 0;
			}
			set
			{
				if (!value && m_szFBToken.CompareTo(m_szServerFBID) != 0)
				{
					m_szServerFBToken = m_szFBToken;
					MBPrefs.SetString("MsData::m_serverfbtoken", m_szServerFBToken);
					MBPrefs.Save();
				}
			}
		}

		public string ReceiptHash
		{
			get
			{
				return m_szReceiptHash;
			}
			set
			{
				m_szReceiptHash = value;
				MBPrefs.SetString("MsData::m_receiptHash", m_szReceiptHash);
				MBPrefs.Save();
			}
		}

		public bool SaveOcha
		{
			get
			{
				if (SingletonComponent<AchievementManager>.Instance != null)
				{
					AchievementManager.AchievementItem achievement = SingletonComponent<AchievementManager>.Instance.GetAchievement("achievement_tutorial_1");
					if (achievement != null)
					{
						m_saveOcha = ((achievement.progress > 0) ? true : false);
					}
				}
				else
				{
					Dbg.LogWarn("[Warn] The AchievementManager.Instance is null");
				}
				return m_saveOcha;
			}
			set
			{
				if (SingletonComponent<AchievementManager>.Instance != null)
				{
					m_saveOcha = value;
					int p_progress = (m_saveOcha ? 1 : 0);
					SingletonComponent<AchievementManager>.Instance.UpdateAchievementProgress(PrepareAchiItemList(1, 1, p_progress, "achievement_tutorial_"));
				}
				else
				{
					Dbg.LogWarn("[Warn] The AchievementManager.Instance is null, the achievement progress is not updated.");
				}
			}
		}

		public bool TutorialFinished
		{
			get
			{
				if (SingletonComponent<AchievementManager>.Instance != null)
				{
					AchievementManager.AchievementItem achievement = SingletonComponent<AchievementManager>.Instance.GetAchievement("achievement_tutorial_2");
					if (achievement != null)
					{
						m_tutorialFinished = ((achievement.progress > 0) ? true : false);
					}
				}
				else
				{
					Dbg.LogWarn("[Warn] The AchievementManager.Instance is null");
				}
				return m_tutorialFinished;
			}
			set
			{
				if (SingletonComponent<AchievementManager>.Instance != null)
				{
					m_tutorialFinished = value;
					SingletonComponent<AchievementManager>.Instance.UpdateAchievementProgress(PrepareAchiItemList(2, 2, m_tutorialFinished ? 1 : 0, "achievement_tutorial_"));
				}
				else
				{
					Dbg.LogWarn("[Warn] The AchievementManager.Instance is null, the achievement progress is not updated.");
				}
			}
		}

		public int FusionSuccessCount
		{
			get
			{
				if (SingletonComponent<AchievementManager>.Instance != null)
				{
					AchievementManager.AchievementItem achievement = SingletonComponent<AchievementManager>.Instance.GetAchievement("achievement_fusion_5");
					if (achievement != null)
					{
						m_fusionSuccessCount = achievement.progress;
					}
				}
				else
				{
					Dbg.LogWarn("[Warn] The AchievementManager.Instance is null");
				}
				return m_fusionSuccessCount;
			}
			set
			{
				if (SingletonComponent<AchievementManager>.Instance != null)
				{
					m_fusionSuccessCount = value;
					SingletonComponent<AchievementManager>.Instance.UpdateAchievementProgress(PrepareAchiItemList(1, 5, m_fusionSuccessCount, "achievement_fusion_"));
				}
				else
				{
					Dbg.LogWarn("[Warn] The AchievementManager.Instance is null, the achievement progress is not updated.");
				}
			}
		}

		public int LastAct
		{
			get
			{
				if (SingletonComponent<AchievementManager>.Instance != null)
				{
					AchievementManager.AchievementItem achievement = SingletonComponent<AchievementManager>.Instance.GetAchievement("ACHIEVEMENT_ID_13");
					if (achievement != null)
					{
						m_lastAct = achievement.progress;
					}
				}
				else
				{
					Dbg.LogWarn("[Warn] The AchievementManager.Instance is null");
				}
				return m_lastAct;
			}
			set
			{
				if (SingletonComponent<AchievementManager>.Instance != null)
				{
					m_lastAct = value;
					SingletonComponent<AchievementManager>.Instance.UpdateAchievementProgress(PrepareAchiItemList(9, 13, m_lastAct));
				}
				else
				{
					Dbg.LogWarn("[Warn] The AchievementManager.Instance is null, the achievement progress is not updated.");
				}
			}
		}

		public bool HasEnteredAnEvent
		{
			get
			{
				if (SingletonComponent<AchievementManager>.Instance != null)
				{
					AchievementManager.AchievementItem achievement = SingletonComponent<AchievementManager>.Instance.GetAchievement("achievement_event_1");
					if (achievement != null)
					{
						m_hasEnteredAnEvent = ((achievement.progress > 0) ? true : false);
					}
				}
				else
				{
					Dbg.LogWarn("[Warn] The AchievementManager.Instance is null");
				}
				return m_hasEnteredAnEvent;
			}
			set
			{
				if (SingletonComponent<AchievementManager>.Instance != null)
				{
					m_hasEnteredAnEvent = value;
					int p_progress = (m_hasEnteredAnEvent ? 1 : 0);
					SingletonComponent<AchievementManager>.Instance.UpdateAchievementProgress(PrepareAchiItemList(1, 1, p_progress, "achievement_event_"));
				}
				else
				{
					Dbg.LogWarn("[Warn] The AchievementManager.Instance is null, the achievement progress is not updated.");
				}
			}
		}

		public long LatestWeekFriday
		{
			get
			{
				return m_latestWeekFriday;
			}
			set
			{
				m_latestWeekFriday = value;
				if (SingletonComponent<AchievementManager>.Instance != null)
				{
					SingletonComponent<AchievementManager>.Instance.UpdateAchievementProgress(PrepareAchiItemList(2, 2, 1, "achievement_event_"));
				}
				else
				{
					Dbg.LogWarn("[Warn] The AchievementManager.Instance is null, the achievement progress is not updated.");
				}
			}
		}

		public long LatestWeekSaturday
		{
			get
			{
				return m_latestWeekSaturday;
			}
			set
			{
				m_latestWeekSaturday = value;
				if (SingletonComponent<AchievementManager>.Instance != null)
				{
					AchievementManager.AchievementItem achievement = SingletonComponent<AchievementManager>.Instance.GetAchievement("achievement_event_2");
					if (achievement != null && achievement.progress == 1)
					{
						achievement.progress++;
						SingletonComponent<AchievementManager>.Instance.UpdateAchievementProgress(PrepareAchiItemList(2, 2, achievement.progress, "achievement_event_"));
					}
				}
				else
				{
					Dbg.LogWarn("[Warn] The AchievementManager.Instance is null, the achievement progress is not updated.");
				}
			}
		}

		public long LatestWeekSunday
		{
			get
			{
				return m_latestWeekSunday;
			}
			set
			{
				m_latestWeekSunday = value;
				if (SingletonComponent<AchievementManager>.Instance != null)
				{
					AchievementManager.AchievementItem achievement = SingletonComponent<AchievementManager>.Instance.GetAchievement("achievement_event_2");
					if (achievement != null && achievement.progress == 2)
					{
						achievement.progress++;
						SingletonComponent<AchievementManager>.Instance.UpdateAchievementProgress(PrepareAchiItemList(2, 2, achievement.progress, "achievement_event_"));
					}
				}
				else
				{
					Dbg.LogWarn("[Warn] The AchievementManager.Instance is null, the achievement progress is not updated.");
				}
			}
		}

		public long DefeatWeekdayEvent
		{
			get
			{
				return m_latestWeekdayEvent;
			}
			set
			{
				m_latestWeekdayEvent = value;
				if (AllEventTypesDefeatedInOneWeek())
				{
					if (SingletonComponent<AchievementManager>.Instance != null)
					{
						SingletonComponent<AchievementManager>.Instance.UpdateAchievementProgress(PrepareAchiItemList(3, 3, 1, "achievement_event_"));
					}
					else
					{
						Dbg.LogWarn("[Warn] The AchievementManager.Instance is null, the achievement progress is not updated.");
					}
				}
			}
		}

		public long DefeatWeeklyEvent
		{
			get
			{
				return m_latestWeeklyEvent;
			}
			set
			{
				m_latestWeeklyEvent = value;
				if (AllEventTypesDefeatedInOneWeek())
				{
					if (SingletonComponent<AchievementManager>.Instance != null)
					{
						SingletonComponent<AchievementManager>.Instance.UpdateAchievementProgress(PrepareAchiItemList(3, 3, 1, "achievement_event_"));
					}
					else
					{
						Dbg.LogWarn("[Warn] The AchievementManager.Instance is null, the achievement progress is not updated.");
					}
				}
			}
		}

		public long DefeatMonthlyEvent
		{
			get
			{
				return m_latestMonthlyEvent;
			}
			set
			{
				m_latestMonthlyEvent = value;
				if (AllEventTypesDefeatedInOneWeek())
				{
					if (SingletonComponent<AchievementManager>.Instance != null)
					{
						SingletonComponent<AchievementManager>.Instance.UpdateAchievementProgress(PrepareAchiItemList(3, 3, 1, "achievement_event_"));
					}
					else
					{
						Dbg.LogWarn("[Warn] The AchievementManager.Instance is null, the achievement progress is not updated.");
					}
				}
			}
		}

		public int HighestGoldInDungeon
		{
			get
			{
				if (SingletonComponent<AchievementManager>.Instance != null)
				{
					AchievementManager.AchievementItem achievement = SingletonComponent<AchievementManager>.Instance.GetAchievement("achievement_igc_5");
					if (achievement != null)
					{
						m_highestGoldInDungeon = achievement.progress;
					}
				}
				else
				{
					Dbg.LogWarn("[Warn] The AchievementManager.Instance is null");
				}
				return m_highestGoldInDungeon;
			}
			set
			{
				if (SingletonComponent<AchievementManager>.Instance != null)
				{
					m_highestGoldInDungeon = value;
					SingletonComponent<AchievementManager>.Instance.UpdateAchievementProgress(PrepareAchiItemList(1, 5, m_highestGoldInDungeon, "achievement_igc_"));
				}
				else
				{
					Dbg.LogWarn("[Warn] The AchievementManager.Instance is null, the achievement progress is not updated.");
				}
			}
		}

		public int OpenPremiumTreasureCount
		{
			get
			{
				if (SingletonComponent<AchievementManager>.Instance != null)
				{
					AchievementManager.AchievementItem achievement = SingletonComponent<AchievementManager>.Instance.GetAchievement("achievement_epic_4");
					if (achievement != null)
					{
						m_openPremiumTreasureCount = achievement.progress;
					}
				}
				else
				{
					Dbg.LogWarn("[Warn] The AchievementManager.Instance is null");
				}
				return m_openPremiumTreasureCount;
			}
			set
			{
				if (SingletonComponent<TutorialManager>.Instance != null && SingletonComponent<TutorialManager>.Instance.enabled && !SingletonComponent<TutorialManager>.Instance.IsCompleted(Tutorial.State.Gacha))
				{
					Dbg.Log("It's tutorial gacha, no need to inform to server to update the achievement progress");
				}
				else if (SingletonComponent<AchievementManager>.Instance != null)
				{
					m_openPremiumTreasureCount = value;
					SingletonComponent<AchievementManager>.Instance.UpdateAchievementProgress(PrepareAchiItemList(1, 4, m_openPremiumTreasureCount, "achievement_epic_"));
				}
				else
				{
					Dbg.LogWarn("[Warn] The AchievementManager.Instance is null, the achievement progress is not updated.");
				}
			}
		}

		public int OpenSocialTreasureCount
		{
			get
			{
				if (SingletonComponent<AchievementManager>.Instance != null)
				{
					AchievementManager.AchievementItem achievement = SingletonComponent<AchievementManager>.Instance.GetAchievement("achievement_common_5");
					if (achievement != null)
					{
						m_openSocialTreasureCount = achievement.progress;
					}
				}
				else
				{
					Dbg.LogWarn("[Warn] The AchievementManager.Instance is null");
				}
				return m_openSocialTreasureCount;
			}
			set
			{
				if (SingletonComponent<AchievementManager>.Instance != null)
				{
					m_openSocialTreasureCount = value;
					SingletonComponent<AchievementManager>.Instance.UpdateAchievementProgress(PrepareAchiItemList(1, 5, m_openSocialTreasureCount, "achievement_common_"));
				}
				else
				{
					Dbg.LogWarn("[Warn] The AchievementManager.Instance is null, the achievement progress is not updated.");
				}
			}
		}

		public bool HasEvolvedAnItem
		{
			get
			{
				if (SingletonComponent<AchievementManager>.Instance != null)
				{
					AchievementManager.AchievementItem achievement = SingletonComponent<AchievementManager>.Instance.GetAchievement("achievement_evolve_1");
					if (achievement != null)
					{
						m_hasEvolvedAnItem = ((achievement.progress > 0) ? true : false);
					}
				}
				else
				{
					Dbg.LogWarn("[Warn] The AchievementManager.Instance is null");
				}
				return m_hasEvolvedAnItem;
			}
			set
			{
				if (SingletonComponent<AchievementManager>.Instance != null)
				{
					m_hasEvolvedAnItem = value;
					int p_progress = (m_hasEvolvedAnItem ? 1 : 0);
					SingletonComponent<AchievementManager>.Instance.UpdateAchievementProgress(PrepareAchiItemList(1, 1, p_progress, "achievement_evolve_"));
				}
				else
				{
					Dbg.LogWarn("[Warn] The AchievementManager.Instance is null, the achievement progress is not updated.");
				}
			}
		}

		public int HighestEvolvedTier
		{
			get
			{
				if (SingletonComponent<AchievementManager>.Instance != null)
				{
					AchievementManager.AchievementItem achievement = SingletonComponent<AchievementManager>.Instance.GetAchievement("achievement_evolve_2");
					if (achievement != null)
					{
						m_highestEvolvedTier = achievement.progress;
					}
				}
				else
				{
					Dbg.LogWarn("[Warn] The AchievementManager.Instance is null");
				}
				return m_highestEvolvedTier;
			}
			set
			{
				if (SingletonComponent<AchievementManager>.Instance != null)
				{
					m_highestEvolvedTier = value;
					SingletonComponent<AchievementManager>.Instance.UpdateAchievementProgress(PrepareAchiItemList(2, 2, m_highestEvolvedTier, "achievement_evolve_"));
				}
				else
				{
					Dbg.LogWarn("[Warn] The AchievementManager.Instance is null, the achievement progress is not updated.");
				}
			}
		}

		public int MonsterKillCount
		{
			get
			{
				if (SingletonComponent<AchievementManager>.Instance != null)
				{
					AchievementManager.AchievementItem achievement = SingletonComponent<AchievementManager>.Instance.GetAchievement("achievement_kill_7");
					if (achievement != null)
					{
						m_monsterKillCount = achievement.progress;
					}
				}
				else
				{
					Dbg.LogWarn("[Warn] The AchievementManager.Instance is null");
				}
				return m_monsterKillCount;
			}
			set
			{
				if (SingletonComponent<AchievementManager>.Instance != null)
				{
					m_monsterKillCount = value;
					SingletonComponent<AchievementManager>.Instance.UpdateAchievementProgress(PrepareAchiItemList(1, 7, m_monsterKillCount, "achievement_kill_"));
				}
				else
				{
					Dbg.LogWarn("[Warn] The AchievementManager.Instance is null, the achievement progress is not updated.");
				}
			}
		}

		public bool FbLikedUs
		{
			get
			{
				return m_fbLikedUs;
			}
			set
			{
				if (SingletonComponent<AchievementManager>.Instance != null)
				{
					if (m_fbLikedUs != value)
					{
						m_fbLikedUs = value;
						if (m_fbLikedUs)
						{
							SingletonComponent<AchievementManager>.Instance.UpdateAchievementProgress(PrepareAchiItemList(1, 1, m_fbLikedUs ? 1 : 0, "achievement_system_"));
						}
					}
				}
				else
				{
					Dbg.LogWarn("[Warn] The AchievementManager.Instance is null, the achievement progress is not updated.");
				}
			}
		}

		public bool FbLogin
		{
			set
			{
				if (SingletonComponent<AchievementManager>.Instance != null)
				{
					if (value)
					{
						SingletonComponent<AchievementManager>.Instance.UpdateAchievementProgress(PrepareAchiItemList(3, 3, value ? 1 : 0, "achievement_system_"));
					}
				}
				else
				{
					Dbg.LogWarn("[Warn] The AchievementManager.Instance is null, the achievement progress is not updated.");
				}
			}
		}

		public bool AppStoreRateUs
		{
			get
			{
				return m_appStoreRateUs;
			}
			set
			{
				if (SingletonComponent<AchievementManager>.Instance != null)
				{
					if (m_appStoreRateUs != value)
					{
						m_appStoreRateUs = value;
						if (m_appStoreRateUs)
						{
							SingletonComponent<AchievementManager>.Instance.UpdateAchievementProgress(PrepareAchiItemList(2, 2, m_appStoreRateUs ? 1 : 0, "achievement_system_"));
						}
					}
				}
				else
				{
					Dbg.LogWarn("[Warn] The AchievementManager.Instance is null, the achievement progress is not updated.");
				}
			}
		}

		public long LatestWeekdayEvent
		{
			get
			{
				return m_latestWeekdayEvent;
			}
		}

		public long LatestWeeklyEvent
		{
			get
			{
				return m_latestWeeklyEvent;
			}
		}

		public long LatestMonthlyEvent
		{
			get
			{
				return m_latestMonthlyEvent;
			}
		}

		public int TutorialProgress
		{
			get
			{
				return m_tutorialProgress;
			}
			set
			{
				m_tutorialProgress = value;
				if (SingletonComponent<TutorialManager>.Instance.IsCompleted(Tutorial.State.AfterEnterName) && SingletonComponent<AchievementManager>.Instance != null && SingletonComponent<AchievementManager>.Instance.m_AchievementMapBuilt)
				{
					TutorialFinished = true;
				}
				if (m_tutorialProgress == 0)
				{
					if (SingletonComponent<SaveData>.Instance != null)
					{
						SingletonComponent<SaveData>.Instance.ResetName();
						Dbg.Log("Reset name, because the tutorial progress is 0");
					}
					else
					{
						Dbg.LogWarn("SaveData.Instance is null, unable to reset name even the tutorial progress is 0");
					}
				}
			}
		}

		public bool NeedVibrate
		{
			get
			{
				return m_needVibrate;
			}
			set
			{
				m_needVibrate = value;
				MBPrefs.SetInt("MsData::m_needVibrate", m_needVibrate ? 1 : 0);
			}
		}

		public void ResetData()
		{
			m_language = "English";
			m_previousLanguage = "English";
			m_genderMale = true;
			m_genderChosen = false;
			m_nubeeId = string.Empty;
			m_deviceId = string.Empty;
			m_nameCharacter = GLOBAL.CHARACTER_DEFAULT_NAME;
			m_nameGiven = false;
			m_characterCreated = false;
			m_firstTimeToRun = true;
			m_tutorialProgress = 0;
			m_szFBID = string.Empty;
			m_szServerFBID = string.Empty;
			m_szFBServerName = GLOBAL.CHARACTER_DEFAULT_NAME;
			m_szFBToken = string.Empty;
			m_szServerFBToken = string.Empty;
			m_szReceiptHash = string.Empty;
		}

		public void ResetName()
		{
			m_nameCharacter = GLOBAL.CHARACTER_DEFAULT_NAME;
			m_nameGiven = false;
			MBPrefs.SetString("MsData::m_nameCharacter", m_nameCharacter);
			MBPrefs.SetInt("MsData::m_nameGiven", m_nameGiven ? 1 : 0);
			MBPrefs.Save();
		}

		public void Load()
		{
			m_language = NBUnity.GetPreferredLanguage();
			m_previousLanguage = MBPrefs.GetString("MsData::m_previousLanguage");
			m_genderMale = MBPrefs.GetInt("MsData::m_genderMale", 1) > 0;
			m_genderChosen = MBPrefs.GetInt("MsData::m_genderChosen") > 0;
			m_nubeeId = MBPrefs.GetString("MsData::m_nubeeId");
			m_deviceId = MBPrefs.GetString("MsData::m_deviceId");
			m_nameCharacter = MBPrefs.GetString("MsData::m_nameCharacter", GLOBAL.CHARACTER_DEFAULT_NAME);
			m_nameGiven = MBPrefs.GetInt("MsData::m_nameGiven") > 0;
			m_firstTimeToRun = MBPrefs.GetInt("MsData::m_firstTimeToRun") > 0;
			m_characterCreated = MBPrefs.GetInt("MsData::m_characterCreated") > 0;
			m_needVibrate = MBPrefs.GetInt("MsData::m_needVibrate", 0) > 0;
			m_szFBID = MBPrefs.GetString("MsData::m_fbid");
			m_szServerFBID = MBPrefs.GetString("MsData::m_serverfbid");
			m_szFBServerName = MBPrefs.GetString("MsData::m_serverfbname");
			m_szFBToken = MBPrefs.GetString("MsData::m_fbtoken");
			m_szServerFBToken = MBPrefs.GetString("MsData::m_serverfbtoken");
			m_szReceiptHash = MBPrefs.GetString("MsData::m_receiptHash");
			long result = 0L;
			if (long.TryParse(MBPrefs.GetString("MsData:m_bpRemainingTime"), out result))
			{
				m_bpRemainingTime = result;
			}
			else
			{
				m_bpRemainingTime = -1L;
			}
			if (long.TryParse(MBPrefs.GetString("MsData:m_previousLeaderboardTimeStamp"), out result))
			{
				m_previousLeaderboardTimeStamp = result;
			}
			else
			{
				m_previousLeaderboardTimeStamp = -1L;
			}
			LoadAchievementData();
		}

		public void Save()
		{
			MBPrefs.SetString("MsData::m_previousLanguage", m_previousLanguage);
			MBPrefs.SetString("MsData::m_language", m_language);
			MBPrefs.SetInt("MsData::m_genderMale", m_genderMale ? 1 : 0);
			MBPrefs.SetInt("MsData::m_genderChosen", m_genderChosen ? 1 : 0);
			MBPrefs.SetString("MsData::m_nubeeId", m_nubeeId);
			MBPrefs.SetString("MsData::m_deviceId", m_deviceId);
			MBPrefs.SetString("MsData::m_nameCharacter", m_nameCharacter);
			MBPrefs.SetInt("MsData::m_nameGiven", m_nameGiven ? 1 : 0);
			MBPrefs.SetInt("MsData::m_firstTimeToRun", m_firstTimeToRun ? 1 : 0);
			MBPrefs.SetInt("MsData::m_completeTutorial", m_tutorialProgress);
			MBPrefs.SetInt("MsData::m_needVibrate", m_needVibrate ? 1 : 0);
			MBPrefs.SetString("MsData::m_fbid", m_szFBID);
			MBPrefs.SetString("MsData::m_serverfbid", m_szServerFBID);
			MBPrefs.SetString("MsData::m_serverfbname", m_szFBServerName);
			MBPrefs.SetString("MsData::m_fbtoken", m_szFBToken);
			MBPrefs.SetString("MsData::m_serverfbtoken", m_szServerFBToken);
			MBPrefs.SetString("MsData:m_bpRemainingTime", m_bpRemainingTime.ToString());
			MBPrefs.SetString("MsData:m_previousLeaderboardTimeStamp", m_previousLeaderboardTimeStamp.ToString());
			MBPrefs.SetString("MsData::m_receiptHash", m_szReceiptHash);
			SaveAchievementData();
			MBPrefs.Save();
		}

		public void SavePreviousLbdTimeStamp(long p_timeStamp)
		{
			m_previousLeaderboardTimeStamp = p_timeStamp;
			MBPrefs.SetString("MsData:m_previousLeaderboardTimeStamp", m_previousLeaderboardTimeStamp.ToString());
		}

		public long LoadPreviousLbdTimeStamp()
		{
			long result = 0L;
			if (long.TryParse(MBPrefs.GetString("MsData:m_previousLeaderboardTimeStamp"), out result))
			{
				m_previousLeaderboardTimeStamp = result;
			}
			else
			{
				m_previousLeaderboardTimeStamp = -1L;
			}
			return m_previousLeaderboardTimeStamp;
		}

		public bool IsMale()
		{
			return m_genderMale;
		}

		public void SetMale(bool p_value)
		{
			m_genderMale = p_value;
			m_genderChosen = true;
		}

		public bool IsGenderChosen()
		{
			return m_genderChosen;
		}

		public bool NeedToUpdateDeviceID(string p_nubeeId, string p_deviceId)
		{
			Dbg.Log("[NeedToUpdateDeviceID] fromSaveFile NID : " + m_nubeeId + " DID: " + m_deviceId + "\nFromNubeePlatform NID : " + p_nubeeId + " DID : " + p_deviceId);
			return m_nubeeId != string.Empty && m_deviceId != p_deviceId;
		}

		public void SetNubeeAccount(string p_nubeeId, string p_deviceId)
		{
			m_nubeeId = p_nubeeId;
			m_deviceId = p_deviceId;
		}

		public bool IsNameGiven()
		{
			m_nameGiven = ((MBPrefs.GetInt("MsData::m_nameGiven", 0) > 0) ? true : false);
			return m_nameGiven;
		}

		public static string ToShorthandLanguage(string p_language)
		{
			p_language = p_language.ToLower();
			switch (p_language)
			{
			case "english":
				return "en";
			case "japanese":
				return "jp";
			case "chinese":
				return "ch";
			case "korean":
				return "kr";
			default:
				return "en";
			}
		}

		public void SaveDeviceLanguage()
		{
			m_previousLanguage = m_language;
			MBPrefs.SetString("MsData::m_previousLanguage", m_previousLanguage);
			MBPrefs.SetString("MsData::m_language", m_language);
			MBPrefs.Save();
		}

		public List<AchievementManager.AchievementItem> PrepareAchiItemList(int p_startId, int p_endId, int p_progress, string p_idPrefix = "ACHIEVEMENT_ID_")
		{
			if (SingletonComponent<AchievementManager>.Instance != null)
			{
				List<AchievementManager.AchievementItem> list = new List<AchievementManager.AchievementItem>();
				list.Clear();
				for (int i = p_startId; i < p_endId + 1; i++)
				{
					AchievementManager.AchievementItem achievement = SingletonComponent<AchievementManager>.Instance.GetAchievement(p_idPrefix + i);
					if (achievement != null)
					{
						achievement.progress = p_progress;
					}
					else
					{
						Dbg.LogWarnEx("Unable to find achievement id : {0}", Dbg.NBLogType.COMMON_LOG, p_idPrefix + i);
					}
					list.Add(achievement);
				}
				return list;
			}
			Dbg.LogWarn("[Warn] The AchievementManager.Instance is null");
			return new List<AchievementManager.AchievementItem>();
		}

		public bool IsCityDone(int p_index)
		{
			if (p_index < m_isCityDone.Length && SingletonComponent<AchievementManager>.Instance != null)
			{
				AchievementManager.AchievementItem achievement = SingletonComponent<AchievementManager>.Instance.GetAchievement("achievement_city_" + (p_index + 1));
				if (achievement != null)
				{
					m_isCityDone[p_index] = achievement.progress > 0;
					if (achievement != null)
					{
						return achievement.progress > 0;
					}
				}
				else
				{
					Dbg.LogWarn("Unable to find the achievement id : achievement_city_" + (p_index + 1));
				}
			}
			return false;
		}

		public void SetIsCityDone(int p_index, bool p_done)
		{
			if (p_index < m_isCityDone.Length && SingletonComponent<AchievementManager>.Instance != null)
			{
				m_isCityDone[p_index] = p_done;
				int p_progress = (p_done ? 1 : 0);
				SingletonComponent<AchievementManager>.Instance.UpdateAchievementProgress(PrepareAchiItemList(p_index + 1, p_index + 1, p_progress, "achievement_city_"));
			}
			else
			{
				Dbg.LogWarn("[Warn] The AchievementManager.Instance is null, the achievement progress is not updated.");
			}
		}

		public void SetYearForMonth(string p_monthStr, int p_year)
		{
			switch (p_monthStr)
			{
			case "JANUARY":
				m_defeatMonthlyEventYear[0] = p_year;
				break;
			case "FEBRUARY":
				m_defeatMonthlyEventYear[1] = p_year;
				break;
			case "MARCH":
				m_defeatMonthlyEventYear[2] = p_year;
				break;
			case "APRIL":
				m_defeatMonthlyEventYear[3] = p_year;
				break;
			case "MAY":
				m_defeatMonthlyEventYear[4] = p_year;
				break;
			case "JUNE":
				m_defeatMonthlyEventYear[5] = p_year;
				break;
			case "JULY":
				m_defeatMonthlyEventYear[6] = p_year;
				break;
			case "AUGUST":
				m_defeatMonthlyEventYear[7] = p_year;
				break;
			case "SEPTEMBER":
				m_defeatMonthlyEventYear[8] = p_year;
				break;
			case "OCTOBER":
				m_defeatMonthlyEventYear[9] = p_year;
				break;
			case "NOVEMBER":
				m_defeatMonthlyEventYear[10] = p_year;
				break;
			case "DECEMBER":
				m_defeatMonthlyEventYear[11] = p_year;
				break;
			}
			if (IsMonthsAllSameYear())
			{
				if (SingletonComponent<AchievementManager>.Instance != null)
				{
					SingletonComponent<AchievementManager>.Instance.UpdateAchievementProgress(PrepareAchiItemList(4, 4, 1, "achievement_event_"));
				}
				else
				{
					Dbg.LogWarn("[Warn] The AchievementManager.Instance is null, the achievement progress is not updated.");
				}
			}
		}

		public int GetYearForMonth(int p_month)
		{
			return m_defeatMonthlyEventYear[p_month];
		}

		public bool IsMonthsAllSameYear()
		{
			for (int i = 1; i < 12; i++)
			{
				if (m_defeatMonthlyEventYear[i] == 0)
				{
					return false;
				}
				if (m_defeatMonthlyEventYear[0] != m_defeatMonthlyEventYear[i])
				{
					return false;
				}
			}
			return true;
		}

		public bool IsConsecutiveWeekend()
		{
			if (m_latestWeekFriday < 1 || m_latestWeekSaturday < 1 || m_latestWeekSunday < 1)
			{
				return false;
			}
			int weekNumber = TimeUtil.GetWeekNumber(m_latestWeekFriday);
			if (weekNumber == TimeUtil.GetWeekNumber(m_latestWeekSaturday) && weekNumber + 1 == TimeUtil.GetWeekNumber(m_latestWeekSunday))
			{
				return true;
			}
			return false;
		}

		public bool IsSameWeek(long p_timeStamp1, long p_timeStamp2)
		{
			int weekNumber = TimeUtil.GetWeekNumber(p_timeStamp1);
			return weekNumber == TimeUtil.GetWeekNumber(p_timeStamp2);
		}

		public bool IsNextWeek(long p_timeStamp1, long p_timeStamp2)
		{
			int weekNumber = TimeUtil.GetWeekNumber(p_timeStamp1);
			return weekNumber + 1 == TimeUtil.GetWeekNumber(p_timeStamp2);
		}

		public bool AllEventTypesDefeatedInOneWeek()
		{
			if (m_latestWeekdayEvent < 1 || m_latestMonthlyEvent < 1 || m_latestWeeklyEvent < 1)
			{
				return false;
			}
			int weekNumber = TimeUtil.GetWeekNumber(m_latestWeekdayEvent);
			if (weekNumber == TimeUtil.GetWeekNumber(m_latestWeeklyEvent) && weekNumber == TimeUtil.GetWeekNumber(m_latestMonthlyEvent))
			{
				return true;
			}
			return false;
		}

		public void SetFbLikedUsFlag(bool p_flag)
		{
			m_fbLikedUs = p_flag;
		}

		public void SetAppStoreRateUsFlag(bool p_flag)
		{
			m_appStoreRateUs = p_flag;
		}

		public void ResetWeekEndTimeStamp()
		{
			MBPrefs.SetInt("MsData::m_latestWeekFriday", 0);
			MBPrefs.SetInt("MsData::m_latestWeekSaturday", 0);
			MBPrefs.SetInt("MsData::m_latestWeekSunday", 0);
		}

		public void LoadAchievementData()
		{
			m_latestWeekFriday = MBPrefs.GetInt("MsData::m_latestWeekFriday", 0);
			m_latestWeekSaturday = MBPrefs.GetInt("MsData::m_latestWeekSaturday", 0);
			m_latestWeekSunday = MBPrefs.GetInt("MsData::m_latestWeekSunday", 0);
			m_latestWeekdayEvent = MBPrefs.GetInt("MsData::m_latestWeekdayEvent", 0);
			m_latestWeeklyEvent = MBPrefs.GetInt("MsData::m_latestWeeklyEvent", 0);
			m_latestMonthlyEvent = MBPrefs.GetInt("MsData::m_latestMonthlyEvent", 0);
			for (int i = 0; i < 12; i++)
			{
				m_defeatMonthlyEventYear[i] = MBPrefs.GetInt("MsData::m_defeatMonthlyEventYear_" + i, 0);
			}
			Dbg.Log("m_latestWeekFriday: " + m_latestWeekFriday);
			Dbg.Log("m_latestWeekSaturday: " + m_latestWeekSaturday);
			Dbg.Log("m_latestWeekSunday: " + m_latestWeekSunday);
			Messenger<LoadingProgressManager.LoadProgress>.Invoke("progress_bar", LoadingProgressManager.LoadProgress.AchievementDataLoad);
		}

		public void SaveAchievementData()
		{
			MBPrefs.SetInt("MsData::m_latestWeekFriday", (int)m_latestWeekFriday);
			MBPrefs.SetInt("MsData::m_latestWeekSaturday", (int)m_latestWeekSaturday);
			MBPrefs.SetInt("MsData::m_latestWeekSunday", (int)m_latestWeekSunday);
			MBPrefs.SetInt("MsData::m_latestWeekdayEvent", (int)m_latestWeekdayEvent);
			MBPrefs.SetInt("MsData::m_latestWeeklyEvent", (int)m_latestWeeklyEvent);
			MBPrefs.SetInt("MsData::m_latestMonthlyEvent", (int)m_latestMonthlyEvent);
			for (int i = 0; i < 12; i++)
			{
				MBPrefs.SetInt("MsData::m_defeatMonthlyEventYear_" + i, m_defeatMonthlyEventYear[i]);
			}
		}

		public void UpdateLocalAchievementProgressData()
		{
		}
	}

	private float m_curTimeScale = 1f;

	private MsData m_data = new MsData();

	public MsData GameData
	{
		get
		{
			return m_data;
		}
	}

	public void ResetSaveData()
	{
		m_data.ResetData();
	}

	public void ResetName()
	{
		m_data.ResetName();
	}

	public void SaveDataToPlayerPrefs()
	{
		m_data.Save();
	}

	public void SaveGamePauseTimeStamp()
	{
		m_data.PauseTimeStamp = TimeUtil.GetUnixEpochTime();
		MBPrefs.SetString("Ms:PauseTime", m_data.PauseTimeStamp.ToString());
	}

	public long LoadGamePauseTimeStamp()
	{
		string p_value = MBPrefs.GetString("Ms:PauseTime", TimeUtil.GetUnixEpochTime().ToString());
		long p_outResult = 0L;
		NBGeneric.TryParse<long>(p_value, out p_outResult);
		m_data.PauseTimeStamp = p_outResult;
		return m_data.PauseTimeStamp;
	}

	public void LoadDataFromPlayerPrefs()
	{
		m_data.Load();
	}

	public void OnApplicationPause(bool p_pause)
	{
		if (p_pause)
		{
			Dbg.Log("Application was paused.");
			SaveDataToPlayerPrefs();
			SaveGamePauseTimeStamp();
			m_curTimeScale = Time.timeScale;
			Time.timeScale = 0f;
		}
		else
		{
			Time.timeScale = m_curTimeScale;
			UpdateStamina();
			UpdateBpRemainingTime();
			Dbg.Log("Application was unpaused.");
			LoadDataFromPlayerPrefs();
		}
	}

	private void UpdateStamina()
	{
		if (SingletonComponent<PlayerDataManager>.Instance != null && MainMenu.Instance != null)
		{
			float staminaCounterSecs = MainMenu.Instance.GetStaminaCounterSecs();
			long num = LoadGamePauseTimeStamp();
			long unixEpochTime = TimeUtil.GetUnixEpochTime();
			long num2 = unixEpochTime - num;
			long num3 = num2 % (long)GLOBAL.GAME_SETTINGS.stamina_reset_time;
			num2 -= Mathf.FloorToInt(staminaCounterSecs);
			long num4 = ((num2 >= 0) ? 1 : 0);
			num4 += num2 / (long)GLOBAL.GAME_SETTINGS.stamina_reset_time;
			MainMenu.Instance.IncreaseStaminaValue((int)num4);
			if (staminaCounterSecs - (float)num3 < 0f)
			{
				MainMenu.Instance.SetStaminaCounterTo(GLOBAL.GAME_SETTINGS.stamina_reset_time - ((float)num3 - staminaCounterSecs));
			}
			else
			{
				MainMenu.Instance.SetStaminaCounterTo(staminaCounterSecs - (float)num3);
			}
		}
	}

	private void UpdateBpRemainingTime()
	{
		if (SingletonComponent<PlayerDataManager>.Instance != null && MainMenu.Instance != null)
		{
			PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
			long num = LoadGamePauseTimeStamp();
			long unixEpochTime = TimeUtil.GetUnixEpochTime();
			long num2 = unixEpochTime - num;
			Dbg.LogEx("Bp display, game paused, passed time: {0} ", Dbg.NBLogType.COMMON_LOG, num2.ToString());
			playerData.m_userData.timeNextShield -= num2;
		}
	}

	public void OnApplicationFocus(bool p_focus)
	{
		if (!p_focus)
		{
			Dbg.Log("Application lost focus.");
			SaveDataToPlayerPrefs();
		}
		else
		{
			Dbg.Log("Application gained focus.");
			LoadDataFromPlayerPrefs();
			UIPopupController.PrintCameraAndLayers();
		}
	}
}
