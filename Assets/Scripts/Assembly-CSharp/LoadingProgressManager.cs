using System.Collections.Generic;

public class LoadingProgressManager : SingletonComponent<LoadingProgressManager>
{
	public enum LoadProgress
	{
		Start = 0,
		CameraConfig = 1,
		ItemManagerInit = 2,
		LocalizationInit = 3,
		InventoryInit = 4,
		ParameterManager = 5,
		PlayerDataManager = 6,
		AchievementDataLoad = 7,
		StartInit = 8,
		EndFirstInit = 9,
		NBPSuccess = 10,
		NewInstallationSuccess = 11,
		NubeeAccountRegisterSuccess = 12,
		LoadDataFromPref = 13,
		DailyManagerInit = 14,
		NotifyManagerInit = 15,
		SoundManagerInit = 16,
		NBCacheManagerInit = 17,
		GameNetInit = 18,
		DLCInit = 19,
		InventoryCreate = 20,
		PetManager = 21,
		LeaderBoardManagerInit = 22,
		TutorialManager = 23,
		DungeonManager = 24,
		AchievementManager = 25,
		LoadTitleSounds = 26,
		LoadPlaylist = 27,
		ItemVisualManager = 28,
		OnPushManager = 29,
		SoundManager = 30,
		InitPlayerData = 31,
		StoreManager = 32,
		GachaManager = 33,
		AchievementDataInit = 34,
		AnnouncementDataInit = 35,
		End = 36
	}

	private float m_currentProgress;

	private List<LoadProgress> m_progressData = new List<LoadProgress>();

	public float UpdateProgress(LoadProgress p)
	{
		if (p == LoadProgress.End)
		{
			m_currentProgress = 1f;
		}
		if (m_currentProgress < 1f)
		{
			if (!m_progressData.Contains(p))
			{
				m_progressData.Add(p);
			}
			m_currentProgress = (float)m_progressData.Count / 36f;
		}
		return m_currentProgress;
	}
}
