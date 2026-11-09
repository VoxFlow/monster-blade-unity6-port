using System.Collections.Generic;
using Models;
using UnityEngine;

public class HomePanel : NBUIMenuPanel
{
	public UIButton m_announcementButton;

	public UIButton m_achievementButton;

	public UIButton m_promoCodeButton;

	public UIButton m_switchPetButton;

	[SerializeField]
	private UILabel m_labelAchievements;

	[SerializeField]
	private UILabel m_labelAnnouncements;

	[SerializeField]
	private UILabel m_labelRank;

	[SerializeField]
	private UILabel m_labelName;

	[SerializeField]
	private UILabel m_labelScore;

	[SerializeField]
	private UILabel m_labelEventTimeLeft;

	[SerializeField]
	private UISprite m_spriteHunt;

	[SerializeField]
	private UISprite m_spriteEvent;

	[SerializeField]
	private UITexture m_textureEvent;

	[SerializeField]
	private UISprite m_arrowSprite;

	[SerializeField]
	private SimplifiedLeaderboard m_simplifiedLeaderboard;

	private GameObject m_character;

	private GameObject m_pet;

	public UIButton m_huntButton;

	public UIButton m_upgradeButton;

	public EventBtnEx m_eventButton;

	private UIButtonMessage m_eventBtnMsg;

	[SerializeField]
	private GameObject m_leftAnchor;

	[SerializeField]
	private GameObject m_rightAnchor;

	private void OnAchievementUpdated(AchievementManager.AchievementItem p_item)
	{
		RefreshAchievementButtonState();
	}

	private void OnButtonClick(GameObject p_sender)
	{
		if ((bool)base.GetComponent<Animation>() && base.GetComponent<Animation>().isPlaying)
		{
			return;
		}
		if (null != m_achievementButton && p_sender == m_achievementButton.gameObject)
		{
			if (!SingletonComponent<AchievementManager>.Instance.m_IsGettingAchievementInfo)
			{
				SingletonComponent<AchievementManager>.Instance.GetAchievementsFromServer(true);
			}
		}
		else if (null != m_announcementButton && p_sender == m_announcementButton.gameObject)
		{
			MainMenu.Instance.ShowAnnouncementPanel();
		}
		else if (null != m_promoCodeButton && p_sender == m_promoCodeButton.gameObject)
		{
			MainMenu.Instance.ShowPromoCodePopup();
		}
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_secondary_ui_tap");
	}

	private void UpadateAnnouncementCounter(NBUIMenuPanel p_panel = null)
	{
		if (null != m_announcementButton && null != SingletonComponent<PlayerDataManager>.Instance)
		{
			PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
			SetBadgeCount(m_announcementButton, playerData.m_AnnouncementCount);
		}
	}

	private void OnPromoCodePopupClosed()
	{
		SetBadgeCount(m_promoCodeButton, 0);
	}

	private void OnDisable()
	{
		Messenger<AchievementManager.AchievementItem>.RemoveListener("achievement_update", OnAchievementUpdated);
		Messenger.RemoveListener("promocode_update_badge", OnPromoCodePopupClosed);
		Messenger<NBUIMenuPanel>.RemoveListener("mainmenu_hide_panel", UpadateAnnouncementCounter);
		Messenger.RemoveListener("player_generated", SetupCharacter);
		ShowCharacter(false);
		ShowPet(false);
		SavePetOnServer();
	}

	private void OnEnable()
	{
		Messenger.AddListener("player_generated", SetupCharacter);
		Messenger<AchievementManager.AchievementItem>.AddListener("achievement_update", OnAchievementUpdated);
		Messenger.AddListener("promocode_update_badge", OnPromoCodePopupClosed);
		Messenger<NBUIMenuPanel>.AddListener("mainmenu_hide_panel", UpadateAnnouncementCounter);
		UpdateBadges();
		m_character = SingletonComponent<StageManager>.Instance.PlayerTemplate;
		if (MenuBackground3D.Instance != null)
		{
			MenuBackground3D.Instance.ShowDefaultBackGround();
		}
		Transform transform = base.transform.FindInHierarchy("TimeLeft");
		if ((bool)transform)
		{
			transform.gameObject.SetActiveRecursively(false);
		}
		if (m_leftAnchor != null)
		{
			UIAnchor component = m_leftAnchor.GetComponent<UIAnchor>();
			if (component != null)
			{
				component.uiCamera = MainMenu.Instance.GetMainMenuCamera();
			}
		}
		if (m_rightAnchor != null)
		{
			UIAnchor component2 = m_rightAnchor.GetComponent<UIAnchor>();
			if (component2 != null)
			{
				component2.uiCamera = MainMenu.Instance.GetMainMenuCamera();
			}
		}
		CharacterSetting3D characterSetting3D = Object.FindObjectOfType(typeof(CharacterSetting3D)) as CharacterSetting3D;
		if (null != characterSetting3D)
		{
			characterSetting3D.AttachCamera("MainMenuCameraRoot");
		}
		PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		if (playerData.HasPet)
		{
			m_pet = SingletonComponent<PetManager>.Instance.CreateMainMenuPet(playerData);
			ShowPet(false);
		}
		SingletonComponent<StageManager>.Instance.CreatePlayerMainMenuTemplate(SetupCharacter);
		if (SingletonComponent<AchievementManager>.Instance != null)
		{
			SetNotificationAchievements(SingletonComponent<AchievementManager>.Instance.NumOfCompleted() - SingletonComponent<AchievementManager>.Instance.NumOfClaimed());
		}
		else
		{
			Dbg.LogWarn("AchievementManager.Instance is null, unable to update the display of the number of Achievements");
		}
		if (m_simplifiedLeaderboard != null)
		{
			m_simplifiedLeaderboard.SetInfo();
		}
		else
		{
			Dbg.LogWarn("The m_simplifiedLeaderboard is null, please assign it");
		}
		DungeonEventStruct dungeonEventStruct = SingletonComponent<DungeonManager>.Instance.FindDungeonEvent(SingletonComponent<DungeonManager>.Instance.MainEventID);
		if (dungeonEventStruct != null)
		{
			if (!dungeonEventStruct.imageUrl.Trim().EndsWith("\\"))
			{
				int num = dungeonEventStruct.imageUrl.LastIndexOf('/');
				if (num > 0)
				{
					string p_szFilename = dungeonEventStruct.imageUrl.Substring(num + 1);
					NBCacheManager.Instance.GetFile(dungeonEventStruct.imageUrl, "event", p_szFilename, dungeonEventStruct.imageMD5, DownloadEventTextureCallback, null, null);
				}
				else
				{
					Dbg.LogError("filename not found in " + dungeonEventStruct.imageUrl);
				}
			}
			else
			{
				Dbg.LogError("url ends with no filename: " + dungeonEventStruct.imageUrl);
			}
		}
		if (SingletonComponent<TutorialManager>.Instance != null && !SingletonComponent<TutorialManager>.Instance.IsCompleted((Tutorial.State)123))
		{
			m_eventButton.Lock = true;
		}
		else
		{
			m_eventButton.Lock = false;
		}
		if (m_eventBtnMsg != null)
		{
			m_eventBtnMsg.enabled = true;
		}
		if (null != m_switchPetButton && playerData.details.unlockedPets.Count <= 1)
		{
			m_switchPetButton.gameObject.SetActiveRecursively(false);
		}
		if (SingletonComponent<DailyManager>.Instance.NeedToShowDialyBonusAfterTutorial)
		{
			SingletonComponent<DailyManager>.Instance.NeedToShowDialyBonusAfterTutorial = false;
			UIPopupController.Instance.ShowDailyBonus();
		}
		MainMenu.Instance.ShowPromotionPopup();
	}

	private void UpdateBadges()
	{
		UpadateAnnouncementCounter();
		UpdatePromoCodesButtonBadge();
	}

	private void UpdatePromoCodesButtonBadge()
	{
		if (null != m_promoCodeButton)
		{
			m_promoCodeButton.gameObject.SetActiveRecursively(false);
		}
	}

	private void SetBadgeCount(UIButton p_button, int p_count, bool p_hideButtonWhenZeroCount = false)
	{
		if (!(null != p_button))
		{
			return;
		}
		UIBadge componentInChildren = MBHier.GetComponentInChildren<UIBadge>(p_button);
		if (null != componentInChildren)
		{
			Component component = componentInChildren;
			if (p_hideButtonWhenZeroCount)
			{
				component = p_button;
			}
			if (p_count > 0)
			{
				componentInChildren.SetNumber(p_count);
				component.gameObject.SetActiveRecursively(true);
			}
			else
			{
				component.gameObject.SetActiveRecursively(false);
			}
		}
	}

	private new void Awake()
	{
		m_eventBtnMsg = m_eventButton.GetComponent<UIButtonMessage>();
	}

	public void SetNotificationAchievements(int p_number = 0)
	{
		if (m_labelAchievements == null)
		{
			return;
		}
		if (p_number > 0)
		{
			m_labelAchievements.text = p_number.ToString();
			if ((bool)m_labelAchievements.gameObject.transform.parent)
			{
				m_labelAchievements.gameObject.transform.parent.gameObject.SetActiveRecursively(true);
			}
		}
		else if ((bool)m_labelAchievements.gameObject.transform.parent)
		{
			m_labelAchievements.gameObject.transform.parent.gameObject.SetActiveRecursively(false);
		}
	}

	private void SetCharacterPosition()
	{
		CharacterSetting3D characterSetting3D = Object.FindObjectOfType(typeof(CharacterSetting3D)) as CharacterSetting3D;
		if (null != characterSetting3D)
		{
			characterSetting3D.AttachObject(m_character, "MainMenuPlayerRoot");
			characterSetting3D.AttachObject(m_pet, "MainMenuPetRoot");
			characterSetting3D.AttachCamera("MainMenuCameraRoot");
		}
	}

	private void ShowPet(bool p_show)
	{
		if (null != m_pet)
		{
			m_pet.SetActiveRecursively(p_show);
		}
	}

	private void ShowCharacter(bool p_show)
	{
		if (!(m_character == null))
		{
			m_character.SetActiveRecursively(p_show);
			m_character.GetComponent<Animation>().CrossFade(CharacterInfoPanel.s_idleAnimation, 0f);
			m_character.GetComponent<Animation>()[CharacterInfoPanel.s_idleAnimation].wrapMode = WrapMode.Loop;
		}
	}

	private void SetupCharacter()
	{
		if (SingletonComponent<StageManager>.Instance.PlayerTemplate == null)
		{
			return;
		}
		m_character = SingletonComponent<StageManager>.Instance.PlayerTemplate;
		ShowCharacter(true);
		ShowPet(true);
		SetCharacterPosition();
		UIPopupController.Instance.HideLoadingPopup();
		if (SingletonComponent<TutorialManager>.Instance != null && !SingletonComponent<TutorialManager>.Instance.IsCompleted(Tutorial.State.All))
		{
			SingletonComponent<LeaderboardManager>.Instance.m_NeedToShowResult = false;
		}
		if (SingletonComponent<LeaderboardManager>.Instance.m_NeedToShowResult)
		{
			long num = SingletonComponent<SaveData>.Instance.GameData.LoadPreviousLbdTimeStamp();
			Dbg.LogEx("Previous leaderboard, Local time stamp : {0}, time stamp from server: {1}", Dbg.NBLogType.COMMON_LOG, num.ToString(), SingletonComponent<LeaderboardManager>.Instance.LastResultTimeStamp.ToString());
			if (num < SingletonComponent<LeaderboardManager>.Instance.LastResultTimeStamp)
			{
				UIPopupController.Instance.ShowLbResultPopup();
				SingletonComponent<LeaderboardManager>.Instance.SetLastRankInLocal(0);
			}
			SingletonComponent<SaveData>.Instance.GameData.SavePreviousLbdTimeStamp(SingletonComponent<LeaderboardManager>.Instance.LastResultTimeStamp);
			SingletonComponent<LeaderboardManager>.Instance.m_NeedToShowResult = false;
		}
	}

	private void OnEventButtonClick()
	{
		if ((bool)base.GetComponent<Animation>() && base.GetComponent<Animation>().isPlaying)
		{
			return;
		}
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_ok");
		if (UIPopupController.Instance != null)
		{
			if (m_eventBtnMsg != null)
			{
				Dbg.Log("Disable the event button to prevent user click twice.");
				m_eventBtnMsg.enabled = false;
			}
			UIPopupController.Instance.ShowLoadingPopup(string.Empty, string.Empty);
			SingletonComponent<DungeonManager>.Instance.UpdateEvents(OnUpdateEventsSuccess, OnUpdateEventsFailed);
		}
		else
		{
			Dbg.LogWarn("UIPopupController is null!, Should shut down the game");
		}
	}

	private void OnUpdateEventsSuccess(Services.ServiceHandle p_handle)
	{
		MainMenu.Instance.ShowEventPage();
	}

	private void OnUpdateEventsFailed(Services.ServiceHandle p_handle)
	{
		if (m_eventBtnMsg != null)
		{
			m_eventBtnMsg.enabled = true;
		}
		UIPopupController.Instance.HideLoadingPopup();
		UIPopupController.Instance.ShowServerGenericError();
	}

	private void OnHuntButtonClick()
	{
		if (!base.GetComponent<Animation>() || !base.GetComponent<Animation>().isPlaying)
		{
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_ok");
			MainMenu.Instance.ShowHuntPage();
		}
	}

	private void OnUpgradeButtonClick()
	{
		if (!base.GetComponent<Animation>() || !base.GetComponent<Animation>().isPlaying)
		{
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_secondary_ui_tap");
			Messenger.Invoke("homepanel_upgrade");
			MainMenu.Instance.ShowUpgradePage();
		}
	}

	private void OnPetButtonClick()
	{
		if (((bool)base.GetComponent<Animation>() && base.GetComponent<Animation>().isPlaying) || null == SingletonComponent<PlayerDataManager>.Instance)
		{
			return;
		}
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_secondary_ui_tap");
		PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		if (playerData.details == null || playerData.details.unlockedPets == null)
		{
			return;
		}
		Dictionary<string, PetDetails> unlockedPets = playerData.details.unlockedPets;
		if (unlockedPets.Count <= 1)
		{
			return;
		}
		Dictionary<string, PetDetails>.Enumerator enumerator = unlockedPets.GetEnumerator();
		string text = string.Empty;
		string text2 = string.Empty;
		string activePet = playerData.details.activePet;
		while (enumerator.MoveNext())
		{
			if (enumerator.Current.Key != null && string.IsNullOrEmpty(text))
			{
				text = enumerator.Current.Key;
			}
			if (enumerator.Current.Key == activePet)
			{
				text2 = text;
				if (enumerator.MoveNext())
				{
					text2 = enumerator.Current.Key;
				}
				break;
			}
		}
		if (!string.IsNullOrEmpty(text2))
		{
			playerData.details.activePet = text2;
			if (null != SingletonComponent<PetManager>.Instance)
			{
				m_pet = SingletonComponent<PetManager>.Instance.CreateMainMenuPet(playerData);
				SetCharacterPosition();
			}
		}
	}

	private void SavePetOnServer()
	{
		PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		if (playerData.details != null && !(string.Empty == playerData.details.activePet))
		{
			Services instance = SingletonComponent<Services>.Instance;
			if (!(null == instance))
			{
				instance.SwitchActivePet(instance.NubeeId, SingletonComponent<Services>.Instance.DeviceId, playerData.details.activePet);
			}
		}
	}

	private void OnLeaderboardButtonClick()
	{
		if (!base.GetComponent<Animation>() || !base.GetComponent<Animation>().isPlaying)
		{
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_secondary_ui_tap");
			MainMenu.Instance.ShowLeaderboard();
		}
	}

	public void DownloadEventTextureCallback(bool p_bSuccessful, string p_szStatusMsg, byte[] p_byData, object p_arg1, object p_arg2)
	{
		if (!p_bSuccessful || p_byData == null)
		{
			Dbg.LogError("Unable to get event image");
			return;
		}
		Texture2D texture2D = new Texture2D(4, 4, TextureFormat.RGBA32, false, true);
		if (null == texture2D)
		{
			Dbg.LogError("unable to allocate memory for texture");
		}
		else if (!texture2D.LoadImage(p_byData))
		{
			Dbg.LogError("unable to load texture from byte[]");
		}
		else
		{
			SetEventButtonTexture(texture2D);
		}
	}

	private void SetEventButtonTexture(Texture p_tex)
	{
		if (m_textureEvent == null)
		{
			return;
		}
		Shader shader = Shader.Find("Unlit/Transparent Colored");
		if (!(shader == null))
		{
			Material material = new Material(shader);
			if (!(material == null))
			{
				material.mainTexture = p_tex;
				m_textureEvent.material = material;
			}
		}
	}

	public void RefreshAchievementButtonState()
	{
		if (m_achievementButton != null)
		{
			AchievementBtn component = m_achievementButton.GetComponent<AchievementBtn>();
			if (null != component)
			{
				component.CheckShining();
			}
		}
		SetNotificationAchievements(SingletonComponent<AchievementManager>.Instance.NumOfCompleted() - SingletonComponent<AchievementManager>.Instance.NumOfClaimed());
	}
}
