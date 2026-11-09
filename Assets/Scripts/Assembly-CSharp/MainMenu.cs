using System;
using System.Collections;
using Models;
using UnityEngine;

public class MainMenu : UIController
{
	public enum Menu
	{
		Home = 0,
		Character = 1,
		Achievement = 2,
		LeaderBoard = 3,
		Shop = 4,
		Treasure = 5,
		Social = 6,
		Options = 7,
		Max = 8,
		Dungeon = 8,
		DungeonResult = 9,
		Announcement = 10,
		ArmorCollections = 11
	}

	public delegate void LoadSceneCompleteCb();

	public UIPanel m_panel;

	public TopBar topBar;

	public BottomBar bottomBar;

	public EventBar eventBar;

	public TweenFade m_fader;

	public string[] screenScenes = new string[12];

	private bool[] m_loadingScreenFlags = new bool[12];

	public InventoryPanel inventory;

	public DailyBonusPopup DailyBonusWindow;

	public UISprite m_overlay;

	public AchievementPanel m_AchievementPanel;

	public AnnouncementPanel m_AnnouncementPanel;

	public DragonPointsExchangePanel m_dragonPointsExchangePanelPrefab;

	public HolyDiamondsExchangePanel m_holyDiamondsExchangePanelPrefab;

	public RewardItemInfoPanel m_RewardItemInfoPanel;

	public CharacterInfoPanel m_sharedCharacterInfoPanel;

	public FriendSelectPanel m_sharedFriendSelectPanel;

	public UIButton m_sharedBackButton;

	public GameObject m_screenAnchor;

	public Menu m_homeMenu;

	public UIPanel m_panelSubtitle;

	public UILabel m_freeGachaCounter;

	public UILabel m_gachaGiftCounter;

	public UILabel m_NewItemCounter;

	public UILabel m_AchievedCounter;

	public UILabel m_OptionCounter;

	public UISprite m_spriteTitleGradient;

	public UISprite m_blackBG;

	public LevelUpWindow m_levelUpWindow;

	public int m_LastLevel = -1;

	public UIAtlas m_petAtlas;

	[SerializeField]
	private UISprite m_header;

	[SerializeField]
	private UILabel m_headerLabel;

	[SerializeField]
	private Camera m_mainMenuCamera;

	[SerializeField]
	private Camera m_barsCamera;

	[SerializeField]
	private Camera m_inventoryPanelCamera;

	[SerializeField]
	private Camera m_ochaCamera;

	private StoreManager m_storeManager;

	private PlayerData m_playerData;

	private NBUIMenuScreen m_currScreen;

	private int m_currScreenIndex = -1;

	private int m_nextScreenIndex = -1;

	private Callback m_onCurrentScreenReady;

	private float m_deltaCounter;

	public float m_rateOfAnimation = 10000f;

	private bool m_hasChangedValues;

	private float m_aniPlayerXpBar;

	private int m_aniPlayerXpValue;

	private int m_aniPlayerXpMax;

	private int m_aniPlayerStamina;

	private int m_aniPlayerStaminaMax;

	private int m_aniPlayerIgcValue;

	private int m_aniPlayerPcValue;

	private int m_aniPlayerLevel;

	private Vector3 m_topBarOriginalPos;

	private Vector3 m_bottomBarOriginalPos;

	private Vector3 m_eventBarOriginalPos;

	// PORT (CD-026)
	private Vector3 m_topBarTarget;

	private Vector3 m_bottomBarTarget;

	private NBUIMenuScreen m_nextScreen;

	private bool m_isHomeReady;

	private GameObject m_nextScreenRoot;

	private GameObject m_currScreenRoot;

	private CharacterMenuScreen.ScreenState m_CharacterScreenJumpToState;

	private DungeonMenuScreen.ScreenState m_dungeonScreenJumpToState;

	private OptionMenuScreen.ScreenState m_optionScreenJumpToState;

	public bool m_parentSceneIsHomepage;

	[SerializeField]
	private float m_fadeDuration = 0.2f;

	public UIPopupController m_popupController;

	private PopupCallback m_lockedInTown;

	private PopupCallback m_slotsExpandedCallback;

	private static MainMenu s_instance;

	private float bpTimer;

	public bool IsHomeReady
	{
		get
		{
			return m_isHomeReady;
		}
	}

	public Vector3 bottomBarOriginalPos
	{
		get
		{
			return bottomBar.transform.parent.TransformPoint(m_bottomBarOriginalPos);
		}
	}

	public NBUIMenuScreen currScreen
	{
		get
		{
			return m_currScreen;
		}
	}

	public int currScreenIndex
	{
		get
		{
			return m_currScreenIndex;
		}
	}

	public static MainMenu Instance
	{
		get
		{
			return s_instance;
		}
	}

	public CharacterMenuScreen.ScreenState CharacterMenuScreenJumpToState
	{
		get
		{
			return m_CharacterScreenJumpToState;
		}
		set
		{
			m_CharacterScreenJumpToState = value;
		}
	}

	public DungeonMenuScreen.ScreenState DungeonMenuScreenJumpToState
	{
		get
		{
			return m_dungeonScreenJumpToState;
		}
		set
		{
			m_dungeonScreenJumpToState = value;
		}
	}

	public OptionMenuScreen.ScreenState OptionMenuScreenJumpToState
	{
		get
		{
			return m_optionScreenJumpToState;
		}
		set
		{
			m_optionScreenJumpToState = value;
		}
	}

	public bool IsLoadingNextScreen()
	{
		bool[] loadingScreenFlags = m_loadingScreenFlags;
		for (int i = 0; i < loadingScreenFlags.Length; i++)
		{
			if (loadingScreenFlags[i])
			{
				return true;
			}
		}
		return false;
	}

	private IEnumerator LoadNextScreenAsync(Menu menu, LoadSceneCompleteCb p_completeCb = null)
	{
		m_nextScreenIndex = (int)menu;
		if (m_loadingScreenFlags[(int)menu])
		{
			yield break;
		}
		m_loadingScreenFlags[(int)menu] = true;
		if (m_nextScreen != null)
		{
			if (m_nextScreen != m_currScreen)
			{
				UnityEngine.Object.DestroyObject(m_nextScreen.gameObject);
				if (m_nextScreenRoot != null)
				{
					UnityEngine.Object.DestroyObject(m_nextScreenRoot);
				}
			}
			m_nextScreen = null;
		}
		UICamera[] uiCameras = MBHier.GetComponentsInChildren<UICamera>(this);
		UICamera[] array = uiCameras;
		foreach (UICamera uiCam in array)
		{
			uiCam.enabled = false;
		}
		yield return Application.LoadLevelAdditiveAsync(screenScenes[(int)menu]);
		UICamera[] array2 = uiCameras;
		foreach (UICamera uiCam2 in array2)
		{
			uiCam2.enabled = true;
		}
		m_loadingScreenFlags[(int)menu] = false;
		GameObject go = GameObject.Find(screenScenes[(int)menu] + "Root");
		Dbg.Assert(go != null, screenScenes[(int)menu] + "Root is missing");
		if (go == null)
		{
			yield break;
		}
		Transform screen = go.transform.Find(screenScenes[(int)menu]);
		Dbg.Assert(screen != null, "screen is missing:" + screenScenes[(int)menu] + ", go: " + go.name);
		if (m_nextScreenIndex == (int)menu && screen != null)
		{
			NBUIMenuScreen nextScreen = screen.GetComponent<NBUIMenuScreen>();
			if (m_nextScreen != null && m_nextScreen != m_currScreen && m_nextScreen != nextScreen)
			{
				UnityEngine.Object.DestroyObject(m_nextScreen.gameObject);
				if (m_nextScreenRoot != null)
				{
					UnityEngine.Object.DestroyObject(m_nextScreenRoot);
				}
			}
			Vector3 pos = nextScreen.transform.localPosition;
			Vector3 scale = nextScreen.transform.localScale;
			nextScreen.transform.parent = m_screenAnchor.transform;
			nextScreen.transform.localPosition = pos;
			nextScreen.transform.localScale = scale;
			m_nextScreen = nextScreen;
			m_nextScreenRoot = go;
			Dbg.Assert(m_nextScreen != null);
		}
		else
		{
			UnityEngine.Object.DestroyObject(go);
		}
		if (p_completeCb != null)
		{
			p_completeCb();
		}
		Dbg.Log("Finished loading next menu: " + menu);
	}

	public void StartLoadNextScreenWithFadeout(Menu menu, LoadSceneCompleteCb p_compelteCb = null)
	{
		StartLoadNextScreen(menu, p_compelteCb);
		FadeOutFullScreen();
	}

	public void GotoDungeonEventSelectionPanel()
	{
		Instance.ShowEventPage();
	}

	public void CheckGachaEventPromotion()
	{
		UIPopupController.Instance.HideLoadingPopup();
		if (!(SingletonComponent<DailyManager>.Instance != null) || SingletonComponent<DailyManager>.Instance.EnteredEventScreen)
		{
			return;
		}
		bool flag = false;
		foreach (PromotionData promoData in SingletonComponent<DailyManager>.Instance.PromoDatas)
		{
			if (promoData.PromoType == "gacha")
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			UIPopupController.Instance.ShowPromotion(true);
			SingletonComponent<DailyManager>.Instance.EnteredEventScreen = true;
		}
	}

	public void StartLoadNextScreen(Menu menu, LoadSceneCompleteCb p_compelteCb = null)
	{
		Dbg.Log("Start loading menu: " + menu);
		if (null != m_blackBG && menu != Menu.DungeonResult)
		{
			m_blackBG.gameObject.active = false;
		}
		if (m_nextScreenIndex != (int)menu)
		{
			if (menu != (Menu)m_currScreenIndex || m_currScreen == null)
			{
				StartCoroutine(LoadNextScreenAsync(menu, p_compelteCb));
				return;
			}
			m_nextScreen = m_currScreen;
			m_nextScreenRoot = m_currScreenRoot;
			m_nextScreenIndex = (int)menu;
		}
	}

	public IEnumerator SwitchToNextScreen(Menu menu)
	{
		if (m_nextScreenIndex != (int)menu)
		{
			StartLoadNextScreen(menu);
		}
		while (m_nextScreen == null)
		{
			yield return null;
		}
		if (m_currScreen == m_nextScreen)
		{
			m_currScreen.Home();
			m_nextScreen = null;
			Dbg.Assert(m_nextScreenRoot == m_currScreenRoot);
			m_nextScreenRoot = null;
			m_nextScreenIndex = -1;
			yield break;
		}
		if (m_currScreen != null)
		{
			m_currScreen.gameObject.SetActiveRecursively(false);
			UnityEngine.Object.DestroyObject(m_currScreen.gameObject);
			if (m_currScreenRoot != null)
			{
				UnityEngine.Object.DestroyObject(m_currScreenRoot);
			}
		}
		m_currScreen = m_nextScreen;
		m_currScreenRoot = m_nextScreenRoot;
		m_currScreenIndex = m_nextScreenIndex;
		m_nextScreen = null;
		m_nextScreenRoot = null;
		m_nextScreenIndex = -1;
		menu = (Menu)m_currScreenIndex;
		Messenger<Menu>.Invoke("mainmenu_screen_enter", menu);
		m_currScreen.gameObject.active = true;
		m_currScreen.Home();
		if (m_onCurrentScreenReady != null)
		{
			m_onCurrentScreenReady();
		}
		switch (menu)
		{
		case Menu.Treasure:
			EnableHeader(true, "treasure_header");
			MoveHeaderToTop();
			break;
		case Menu.Achievement:
			EnableHeader(true, "achievement_header");
			MoveHeaderToTop();
			break;
		case Menu.LeaderBoard:
			EnableHeader(true, "leaderboard_header");
			MoveHeaderToTop(false);
			break;
		case Menu.Max:
			EnableHeader(true, "cityselect_header");
			MoveHeaderToTop();
			break;
		case Menu.Character:
			EnableHeader(true, "character_header");
			MoveHeaderToTop();
			ShowTopAndBottomBar(true);
			break;
		case Menu.Shop:
			EnableHeader(true, "shop_header");
			MoveHeaderToTop();
			break;
		case Menu.Social:
			EnableHeader(true, "social_header");
			MoveHeaderToTop();
			break;
		case Menu.Options:
			EnableHeader(true, "options_header");
			MoveHeaderToTop();
			break;
		case Menu.Home:
			EnableHeader(false, string.Empty);
			m_isHomeReady = true;
			break;
		case Menu.ArmorCollections:
			EnableHeader(true, "armorcollections_header");
			MoveHeaderToTop(false);
			break;
		default:
			EnableHeader(true, string.Empty);
			MoveHeaderToTop(false);
			break;
		}
	}

	public Camera GetMainMenuCamera()
	{
		return m_mainMenuCamera;
	}

	public Camera GetBarsCamera()
	{
		return m_barsCamera;
	}

	public void EnableMainMenuCameras(bool bEnable)
	{
		EnableMainMenuCamera(bEnable);
		EnableBarsCamera(bEnable);
	}

	public void EnableMainMenuCamera(bool p_enable)
	{
		if (m_mainMenuCamera != null)
		{
			m_mainMenuCamera.gameObject.active = p_enable;
		}
	}

	public void EnableBarsCamera(bool p_enable)
	{
		if (m_barsCamera != null)
		{
			m_barsCamera.gameObject.active = p_enable;
		}
	}

	public void EnableInventoryCamera(bool p_enable)
	{
		if (m_inventoryPanelCamera != null)
		{
			m_inventoryPanelCamera.gameObject.active = p_enable;
		}
	}

	public void EnableOchaCamera(bool p_enable)
	{
		if (m_ochaCamera != null)
		{
			m_ochaCamera.gameObject.active = p_enable;
		}
	}

	private void OnEnable()
	{
		Messenger.AddListener("inventory_updated", OnInventoryUpdated);
		Messenger<NBUIMenuPanel>.AddListener("mainmenu_show_panel", OnShowPanel);
		Messenger<NBUIMenuPanel>.AddListener("mainmenu_hide_panel", OnHidePanel);
		Messenger.AddListener("town_update_badge", OnTownUpdate);
		Messenger.AddListener("promocode_update_badge", OnPromoCodeUpdate);
	}

	private void OnDisable()
	{
		Messenger.RemoveListener("inventory_updated", OnInventoryUpdated);
		Messenger<NBUIMenuPanel>.RemoveListener("mainmenu_show_panel", OnShowPanel);
		Messenger<NBUIMenuPanel>.RemoveListener("mainmenu_hide_panel", OnHidePanel);
		Messenger.RemoveListener("town_update_badge", OnTownUpdate);
		Messenger.RemoveListener("promocode_update_badge", OnPromoCodeUpdate);
	}

	private void OnTownUpdate()
	{
		UpdateNewItemCounter();
	}

	private void OnPromoCodeUpdate()
	{
		UpdateOptionCounter();
	}

	private void OnDestroy()
	{
		s_instance = null;
	}

	private void OnHidePanel(NBUIMenuPanel p_panel)
	{
		TutorialManager instance = SingletonComponent<TutorialManager>.Instance;
		if ((bool)instance && !instance.IsAnyTutorialExisting())
		{
			instance.EndSubtitle();
		}
		if ((bool)instance && !instance.enabled)
		{
			bool flag = true;
			DungeonManager instance2 = SingletonComponent<DungeonManager>.Instance;
			if (null != instance2 && instance2.BattleType == Battle.Type.DungeonEvent)
			{
				flag = false;
			}
			if (p_panel is EventSelectPanel && flag)
			{
				ShowEventBar(false);
			}
		}
	}

	private void OnShowPanel(NBUIMenuPanel p_panel)
	{
		if (p_panel == null || m_currScreen == null)
		{
			return;
		}
		TutorialManager instance = SingletonComponent<TutorialManager>.Instance;
		if (null == instance || (instance.enabled && instance.IsAnyTutorialExisting()))
		{
			return;
		}
		if (p_panel is ShopPanel)
		{
			ShowOchaSubtitle("ocha_guide_shopscreen", true, "Idle01", "Default", string.Empty);
		}
		else if (p_panel is DungeonAreaSelectPanel)
		{
			ShowOchaSubtitle("ocha_guide_dungeonareaselect", true, "Idle01", "Default", string.Empty);
		}
		else if (p_panel is EventSelectPanel)
		{
			if (instance.enabled)
			{
				ShowOchaSubtitle("ocha_guide_tutorial_eventselect", true, "Idle01", "Default", string.Empty);
				return;
			}
			ShowOchaSubtitle("ocha_guide_eventselect", true, "Idle01", "Default", string.Empty);
			ShowEventBar(true);
		}
		else if (p_panel is FriendSelectPanel)
		{
			FriendSelectPanel friendSelectPanel = p_panel as FriendSelectPanel;
			if (friendSelectPanel.m_friendCellType == FriendCell.CellType.FRIEND_SELECT)
			{
				ShowOchaSubtitle("ocha_guide_friendselect", true, "Idle01", "Default", string.Empty);
			}
			DungeonManager instance2 = SingletonComponent<DungeonManager>.Instance;
			if (null != instance2 && instance2.BattleType == Battle.Type.DungeonEvent && currScreen is DungeonMenuScreen && !instance.enabled)
			{
				ShowEventBar(true);
			}
		}
		else if (p_panel is AchievementMenuPanel)
		{
			ShowOchaSubtitle("ocha_guide_achievement", true, "Idle01", "Default", string.Empty);
		}
		else if (m_currScreen is LeaderboardMenuScreen && p_panel == m_currScreen.menuPanels[0])
		{
			ShowOchaSubtitle("ocha_guide_slayerranks", false, "Idle01", "Default", string.Empty);
		}
		else if (m_currScreen is CharacterMenuScreen && p_panel == m_currScreen.menuPanels[0])
		{
			ShowOchaSubtitle("ocha_guide_town", true, "Idle01", "Default", string.Empty);
		}
		else if (m_currScreen is GachaScreen && p_panel == m_currScreen.menuPanels[0])
		{
			ShowOchaSubtitle("ocha_guide_gacha", true, "Idle01", "Default", string.Empty);
		}
		else if (m_currScreen is SocialMenuScreen && p_panel == m_currScreen.menuPanels[0])
		{
			ShowOchaSubtitle("ocha_guide_social", true, "Idle01", "Default", string.Empty);
		}
		else if (m_currScreen is OptionMenuScreen && p_panel == m_currScreen.menuPanels[0])
		{
			ShowOchaSubtitle("ocha_guide_options", true, "Idle01", "Default", string.Empty);
		}
	}

	private void ShowOchaSubtitle(string p_key, bool p_behindUIBar = true, string p_animName = "Idle01", string p_faceName = "Default", string p_soundName = "")
	{
		string p_text = p_key;
		if ((bool)SingletonComponent<NBLocalization>.Instance)
		{
			p_text = SingletonComponent<NBLocalization>.Instance.Get(p_key);
		}
		SingletonComponent<TutorialManager>.Instance.ShowSubtitle(p_text, true, p_animName, p_faceName, p_soundName, OnHideSubtitle, false, p_behindUIBar, false);
	}

	private void OnHideSubtitle()
	{
		if ((bool)SingletonComponent<TutorialManager>.Instance)
		{
			SingletonComponent<TutorialManager>.Instance.EndSubtitle();
		}
	}

	public override void Awake()
	{
		NBProfiler.PrintUsedHeap("[MAINMENU]");
		base.Awake();
		Transform transform = base.transform.Find("3 - Camera (Main)/Panel");
		Dbg.Assert(transform);
		m_panel = transform.GetComponent<UIPanel>();
		Dbg.Assert(m_panel);
		if (Singleton<MonsterFactory>.Instance != null)
		{
			Singleton<MonsterFactory>.Instance.m_debugMonsterID = string.Empty;
		}
		if (s_instance == null)
		{
			s_instance = this;
		}
		m_storeManager = SingletonComponent<StoreManager>.Instance;
		m_topBarOriginalPos = topBar.transform.localPosition;
		m_bottomBarOriginalPos = bottomBar.transform.localPosition;
		topBar.transform.localPosition += Vector3.up * 140f;
		bottomBar.transform.localPosition += Vector3.up * -110f;
		if (null != eventBar)
		{
			eventBar.enabled = false;
			m_eventBarOriginalPos = eventBar.transform.localPosition;
			eventBar.transform.localPosition += Vector3.up * -110f;
		}
		bool flag = false;
		if (SingletonComponent<TutorialManager>.Instance != null && SingletonComponent<TutorialManager>.Instance.enabled)
		{
			Dbg.LogEx("Current tutorial progress: {0}", Dbg.NBLogType.TUTORIAL, SingletonComponent<SaveData>.Instance.GameData.TutorialProgress.ToString());
			flag = SingletonComponent<TutorialManager>.Instance.IsCompleted(Tutorial.State.All);
		}
		else
		{
			flag = true;
		}
		if (!flag)
		{
			Dbg.Log("The tutorial is not completed, do not show the promotion window or daily bonus window");
		}
		else
		{
			if (SingletonComponent<DungeonManager>.Instance == null || !SingletonComponent<DungeonManager>.Instance.ShowDungeonResultScreen())
			{
				if (SingletonComponent<DailyManager>.Instance != null)
				{
					if (SingletonComponent<DailyManager>.Instance.NeedToShowDailyBonus)
					{
						UIPopupController.Instance.ShowDailyBonus();
						SingletonComponent<DailyManager>.Instance.NeedToShowDailyBonus = false;
					}
					else if (SingletonComponent<DailyManager>.Instance.NeedToShowBattleHelp)
					{
						ShowBattleHelpPopup();
					}
					else
					{
						ShowPromotionPopup();
					}
				}
				else
				{
					Dbg.LogWarn("[Warn] DailyManager.Instance is null! Please add the script back");
				}
			}
			else
			{
				Dbg.Log("Because need to show dungeon complete screen, so will not show daily bonus or promotion window");
			}
			if (SingletonComponent<DailyManager>.Instance != null)
			{
				SingletonComponent<DailyManager>.Instance.UpdatePromoCodeData();
			}
		}
		m_popupController = UIPopupController.Instance;
	}

	public override void Start()
	{
		NBProfiler.EndTask("ENTRY_POINT_TO_MENU");
		base.Start();
		if (SingletonComponent<DungeonManager>.Instance == null || !SingletonComponent<DungeonManager>.Instance.ShowDungeonResultScreen())
		{
			if (null != m_blackBG)
			{
				m_blackBG.gameObject.active = false;
			}
			if (MenuBackground3D.Instance != null)
			{
				MenuBackground3D.Instance.ShowDefaultBackGround(true);
				MenuBackground3D.Instance.m_camera.gameObject.SetActiveRecursively(true);
			}
		}
		else if (null != m_blackBG)
		{
			m_blackBG.gameObject.active = true;
		}
		PlayMusicOnStart component = GetComponent<PlayMusicOnStart>();
		if (component != null)
		{
			component.m_music = "list_e_main_menu_music";
		}
		RetrievePlayerData();
		Refresh();
		if (m_playerData == null)
		{
			Dbg.LogError("--- No player data found. Disable This Script!!!! --- ");
			base.enabled = false;
			return;
		}
		if (m_playerData != null && m_playerData.m_userData.maxStamina < 1)
		{
			Dbg.LogError("Player has 0 max stamina. Probably invalid player. Ignoring topBar update.");
		}
		if (m_playerData != null)
		{
			m_aniPlayerXpValue = m_playerData.m_userData.currentXp;
			m_aniPlayerXpMax = m_playerData.TargetXp;
			m_aniPlayerXpBar = (float)m_aniPlayerXpValue / (float)m_aniPlayerXpMax;
			m_aniPlayerStamina = m_playerData.m_userData.stamina;
			m_aniPlayerStaminaMax = m_playerData.m_userData.maxStamina;
			m_aniPlayerIgcValue = m_playerData.m_userData.inGameCurrency;
			m_aniPlayerPcValue = m_playerData.m_userData.premiumCurrency;
			m_aniPlayerLevel = m_playerData.Level;
			if (m_aniPlayerXpBar > 1f)
			{
				Dbg.LogWarn("The Exp bar value should not be over 1.0");
				if (m_aniPlayerLevel >= m_playerData.MaxLevel)
				{
					m_aniPlayerXpBar = 1f;
				}
				else
				{
					m_aniPlayerXpBar = (float)(m_aniPlayerXpValue - m_aniPlayerXpMax) / (float)m_aniPlayerXpMax;
				}
			}
			if (SingletonComponent<SaveData>.Instance != null)
			{
				topBar.SetName(m_playerData.Name, m_playerData.Level);
			}
			if (SingletonComponent<DungeonManager>.Instance != null && SingletonComponent<DungeonManager>.Instance.ShowDungeonResultScreen())
			{
				if (SingletonComponent<DungeonManager>.Instance.HasLevelUp())
				{
					m_aniPlayerXpMax = m_playerData.m_previousTargetXp;
				}
				else
				{
					m_aniPlayerXpMax = m_playerData.m_targetXp;
				}
				topBar.labelStamina.text = m_aniPlayerStamina + "/" + m_aniPlayerStaminaMax;
				topBar.labelPC.text = m_aniPlayerPcValue + string.Empty;
				int inGameGained = SingletonComponent<DungeonManager>.Instance.GetInGameGained();
				int num = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData().m_userData.inGameCurrency - inGameGained;
				if (num < 0)
				{
					num = 0;
				}
				topBar.labelIGC.text = num.ToString();
				Dbg.Log("do pre-process because of HasDungeonComplete()");
				m_aniPlayerXpValue = SingletonComponent<DungeonManager>.Instance.m_originalExp;
				if (m_aniPlayerXpValue < 0)
				{
					m_aniPlayerXpValue = 0;
				}
				m_aniPlayerXpBar = (float)m_aniPlayerXpValue / (float)m_aniPlayerXpMax;
				Dbg.Log("[Info] set xp bar: " + (float)m_aniPlayerXpValue / (float)m_aniPlayerXpMax);
				Dbg.Log("[Info] set m_aniPlayerXpValue: " + m_aniPlayerXpValue);
				Dbg.Log("[Info] set m_aniPlayerXpMax: " + m_aniPlayerXpMax);
				if (m_aniPlayerXpBar > 1f)
				{
					Dbg.LogWarn("The Exp bar value should not be over 1.0");
					if (m_aniPlayerLevel >= m_playerData.MaxLevel)
					{
						m_aniPlayerXpBar = 1f;
					}
					else
					{
						m_aniPlayerXpBar = (float)(m_aniPlayerXpValue - m_aniPlayerXpMax) / (float)m_aniPlayerXpMax;
					}
				}
				m_aniPlayerIgcValue = m_playerData.m_userData.inGameCurrency - SingletonComponent<DungeonManager>.Instance.GetInGameGained();
				SaveData.MsData gameData = SingletonComponent<SaveData>.Instance.GameData;
				if (SingletonComponent<TutorialManager>.Instance != null && SingletonComponent<TutorialManager>.Instance.IsCompleted(Tutorial.State.All) && gameData.HighestGoldInDungeon < SingletonComponent<DungeonManager>.Instance.GetInGameGained())
				{
					gameData.HighestGoldInDungeon = SingletonComponent<DungeonManager>.Instance.GetInGameGained();
					gameData.SaveAchievementData();
				}
				DungeonManager instance = SingletonComponent<DungeonManager>.Instance;
				if (instance.BattleType == Battle.Type.DungeonEvent || instance.BattleType == Battle.Type.PetEvent)
				{
					long unixEpochTime = TimeUtil.GetUnixEpochTime();
					switch (TimeUtil.GetDayOfTheWeek(unixEpochTime))
					{
					case DayOfWeek.Friday:
						gameData.LatestWeekFriday = unixEpochTime;
						break;
					case DayOfWeek.Saturday:
						gameData.LatestWeekSaturday = unixEpochTime;
						break;
					case DayOfWeek.Sunday:
						gameData.LatestWeekSunday = unixEpochTime;
						break;
					}
					if (instance.CurrentDungeonEvent.categoryRef == "MONDAY" || instance.CurrentDungeonEvent.categoryRef == "TUESDAY" || instance.CurrentDungeonEvent.categoryRef == "WEDNESDAY" || instance.CurrentDungeonEvent.categoryRef == "THURSDAY" || instance.CurrentDungeonEvent.categoryRef == "FRIDAY" || instance.CurrentDungeonEvent.categoryRef == "SATURDAY" || instance.CurrentDungeonEvent.categoryRef == "SUNDAY")
					{
						gameData.DefeatWeekdayEvent = unixEpochTime;
					}
					if (instance.CurrentDungeonEvent.categoryRef == "WEEK_1" || instance.CurrentDungeonEvent.categoryRef == "WEEK_2" || instance.CurrentDungeonEvent.categoryRef == "WEEK_3" || instance.CurrentDungeonEvent.categoryRef == "WEEK_4")
					{
						gameData.DefeatWeeklyEvent = unixEpochTime;
					}
					if (instance.CurrentDungeonEvent.categoryRef == "JANUARY" || instance.CurrentDungeonEvent.categoryRef == "FEBRUARY" || instance.CurrentDungeonEvent.categoryRef == "MARCH" || instance.CurrentDungeonEvent.categoryRef == "APRIL" || instance.CurrentDungeonEvent.categoryRef == "MAY" || instance.CurrentDungeonEvent.categoryRef == "JUNE" || instance.CurrentDungeonEvent.categoryRef == "JULY" || instance.CurrentDungeonEvent.categoryRef == "AUGUST" || instance.CurrentDungeonEvent.categoryRef == "SEPTEMBER" || instance.CurrentDungeonEvent.categoryRef == "OCTOBER" || instance.CurrentDungeonEvent.categoryRef == "NOVEMBER" || instance.CurrentDungeonEvent.categoryRef == "DECEMBER")
					{
						gameData.DefeatMonthlyEvent = unixEpochTime;
						gameData.SetYearForMonth(instance.CurrentDungeonEvent.categoryRef, DateTime.Today.Year);
					}
					gameData.SaveAchievementData();
				}
				if (m_aniPlayerIgcValue < 0)
				{
					m_aniPlayerIgcValue = 0;
				}
			}
		}
		UIPopupController.SetAllInputsEnabled(false);
		if (SingletonComponent<TutorialManager>.Instance.IsCompleted(Tutorial.State.All))
		{
			SingletonComponent<TutorialManager>.Instance.enabled = false;
		}
		else
		{
			SingletonComponent<TutorialManager>.Instance.enabled = true;
		}
		if (SingletonComponent<TutorialManager>.Instance.IsBattleCompleted)
		{
			bottomBar.SetAllButtonsEnable(true);
			bottomBar.HideUnusedButton();
		}
		Messenger.Invoke("mainmenu_start");
		StartShowMenu();
	}

	public void StartLoadHomeScreen()
	{
		if (IsInventoryItemsExceedLimit() || bottomBar.IsMenuLocked(m_homeMenu))
		{
			StartLoadNextScreen(Menu.Character);
		}
		else
		{
			StartLoadNextScreen(m_homeMenu);
		}
	}

	private void StartShowMenu()
	{
		StartCoroutine(DoOpeningAnimation());
	}

	public void CheckPlayerName()
	{
		SaveData instance = SingletonComponent<SaveData>.Instance;
		if (null == instance)
		{
			Dbg.LogError("There is no SaveData instance.");
			return;
		}
		string characterName = instance.GameData.CharacterName;
		string language = instance.GameData.Language;
		if (language.Equals("English") && NBGeneric.ContainsNonAsciiChar(characterName))
		{
			UIPopupController instance2 = UIPopupController.Instance;
			if (null != instance2)
			{
				UIPopupController.Instance.ShowOKPopupMSG("common_info", "name_input_english_only");
			}
		}
	}

	public void CreateLevelUpWindow()
	{
		if (!(m_levelUpWindow == null))
		{
			return;
		}
		UnityEngine.Object obj = Resources.Load("UI/Prefabs/MainMenu/Screens/LevelUpAnimationNew");
		if (obj == null)
		{
			Dbg.LogError("Unable to instantiate level up window prefab");
			return;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(obj) as GameObject;
		if (gameObject == null)
		{
			Dbg.LogError("Unable to instantiate level up window to game object");
			return;
		}
		m_levelUpWindow = gameObject.GetComponent<LevelUpWindow>();
		UIPopupController instance = UIPopupController.Instance;
		if (null != instance && null != instance.m_overlay)
		{
			instance.m_overlay.gameObject.active = true;
			instance.m_overlay.SendMessage("FadeIn", null, SendMessageOptions.DontRequireReceiver);
		}
		if (SingletonComponent<NotifyManager>.Instance != null)
		{
			SingletonComponent<NotifyManager>.Instance.ClearLocalNotification();
			SingletonComponent<NotifyManager>.Instance.RegisterAllLocalNotifications();
		}
	}

	public void MoveLevelUpWindowToPopup()
	{
		if (m_levelUpWindow == null)
		{
			Dbg.LogError("The level up window is null, cannot move it");
			return;
		}
		if (m_popupController == null)
		{
			Dbg.LogError("The UIPopupController is null, cannot move level up window under it");
			return;
		}
		m_levelUpWindow.transform.parent = m_popupController.Panel.transform;
		m_levelUpWindow.transform.localPosition = new Vector3(0f, 0f, -12f);
		m_levelUpWindow.transform.localScale = Vector3.one;
	}

	public void DestroyLevelUpWindow()
	{
		if (m_levelUpWindow != null)
		{
			UnityEngine.Object.DestroyImmediate(m_levelUpWindow.gameObject);
			m_levelUpWindow = null;
			UIPopupController instance = UIPopupController.Instance;
			if (null != instance && null != instance.m_overlay)
			{
				instance.m_overlay.gameObject.active = true;
				instance.m_overlay.SendMessage("FadeOut", null, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	private IEnumerator ShowLevelUpWindow(int p_level)
	{
		PlayerData newData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		newData.ComputeFinalValues();
		CreateLevelUpWindow();
		if (SingletonComponent<SoundManager>.Instance != null)
		{
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_level_up_notification");
		}
		else
		{
			Dbg.LogWarn("The SoundManager is null, unable to play the sound");
		}
		MoveLevelUpWindowToPopup();
		int levelUpDone = 0;
		if (SingletonComponent<AchievementManager>.Instance.m_OldPlayerData != null)
		{
			m_levelUpWindow.SetLevelUpData(SingletonComponent<AchievementManager>.Instance.m_OldPlayerData, newData, levelUpDone);
			levelUpDone++;
		}
		if (m_levelUpWindow != null)
		{
			m_levelUpWindow.SetLevelDigits(p_level);
			m_levelUpWindow.UpdateGameObjects();
			while (m_levelUpWindow.Show())
			{
				yield return null;
			}
			m_levelUpWindow.Hide();
		}
		SingletonComponent<AchievementManager>.Instance.m_OldPlayerData = null;
		DestroyLevelUpWindow();
	}

	private IEnumerator DoOpeningAnimation()
	{
		yield return new MBWaitForSeconds(1.5f);   // PORT (CD-059)
		if (!(SingletonComponent<DungeonManager>.Instance != null) || !SingletonComponent<DungeonManager>.Instance.ShowDungeonResultScreen())
		{
			ShowTopAndBottomBar(true);
		}
		yield return new MBWaitForSeconds(1f);   // PORT (CD-059)
		yield return StartCoroutine(StartScreen());
		if (m_popupController != null)
		{
			m_popupController.EnableUI();
		}
	}

	private IEnumerator StartScreen()
	{
		if (SingletonComponent<DungeonManager>.Instance != null && SingletonComponent<DungeonManager>.Instance.ShowDungeonResultScreen())
		{
			yield return StartCoroutine(SwitchToNextScreen(Menu.DungeonResult));
			SingletonComponent<DungeonManager>.Instance.SetShowDungeonResultScreen(false);
		}
		else
		{
			yield return StartCoroutine(StartHome());
		}
	}

	private void Reset()
	{
		if (m_currScreen != null)
		{
			m_currScreen.gameObject.SetActiveRecursively(false);
			UnityEngine.Object.DestroyObject(m_currScreen);
			m_currScreen = null;
		}
		ShowTopAndBottomBar(false);
	}

	private IEnumerator StartHome()
	{
		Dbg.Log("StartHome");
		if (SingletonComponent<NotifyManager>.Instance != null)
		{
			SingletonComponent<NotifyManager>.Instance.EnableNotifications(true);
		}
		if (IsInventoryItemsExceedLimit() || bottomBar.IsMenuLocked(m_homeMenu))
		{
			yield return StartCoroutine(SwitchToNextScreen(Menu.Character));
		}
		else
		{
			yield return StartCoroutine(SwitchToNextScreen(m_homeMenu));
			NubeePlatformBinding.CheckPendingPurchaseV3();
		}
		CheckProfileButtonFlash();
	}

	public void Home()
	{
		StartCoroutine(StartHome());
	}

	public void StartHomePage()
	{
		Dbg.Log("StartHomePage");
		m_isHomeReady = false;
		if (m_currScreen != null)
		{
			m_currScreen.gameObject.SetActiveRecursively(false);
			UnityEngine.Object.DestroyObject(m_currScreen.gameObject);
			if (m_currScreenRoot != null)
			{
				UnityEngine.Object.DestroyObject(m_currScreenRoot);
			}
		}
		if (MenuBackground3D.Instance != null)
		{
			MenuBackground3D.Instance.ShowDefaultBackGround(true);
			MenuBackground3D.Instance.m_camera.gameObject.SetActiveRecursively(true);
		}
		StartCoroutine(WaitAndGoHome(1f));
	}

	private IEnumerator WaitAndGoHome(float p_delay)
	{
		ShowTopBar(false);
		ShowBottomBar(false);
		ShowEventBar(false);
		yield return new MBWaitForSeconds(p_delay);   // PORT (CD-059)
		ShowTopAndBottomBar(true);
		Home();
	}

	private void Update()
	{
		if (m_playerData == null || m_playerData.m_userData.maxStamina < 1)
		{
			return;
		}
		CountdownBpRegenerateTime();
		float num = (float)m_playerData.m_userData.stamina / (float)m_playerData.m_userData.maxStamina;
		if (m_playerData.m_userData.maxStamina > 0 && topBar.labelTimer != null)
		{
			topBar.labelTimer.text = ((!(num >= 1f)) ? GetStaminaCountDown() : string.Empty);
		}
		if (!m_hasChangedValues)
		{
			return;
		}
		bool flag = false;
		float deltaTime = Time.deltaTime;
		m_deltaCounter += m_rateOfAnimation * deltaTime;
		int num2 = Mathf.FloorToInt(m_deltaCounter);
		m_deltaCounter -= num2;
		if (m_aniPlayerLevel < m_playerData.Level)
		{
			m_aniPlayerLevel += num2;
			if (m_aniPlayerLevel > m_playerData.Level)
			{
				m_aniPlayerLevel = m_playerData.Level;
			}
			flag = true;
		}
		else if (m_aniPlayerLevel > m_playerData.Level)
		{
			m_aniPlayerLevel -= num2;
			if (m_aniPlayerLevel < m_playerData.Level)
			{
				m_aniPlayerLevel = m_playerData.Level;
			}
			flag = true;
		}
		if (m_aniPlayerStamina < m_playerData.m_userData.stamina)
		{
			m_aniPlayerStamina += num2;
			if (m_aniPlayerStamina > m_playerData.m_userData.stamina)
			{
				m_aniPlayerStamina = m_playerData.m_userData.stamina;
			}
			flag = true;
		}
		else if (m_aniPlayerStamina > m_playerData.m_userData.stamina)
		{
			m_aniPlayerStamina -= num2;
			if (m_aniPlayerStamina < m_playerData.m_userData.stamina)
			{
				m_aniPlayerStamina = m_playerData.m_userData.stamina;
			}
			flag = true;
		}
		if (m_aniPlayerStaminaMax < m_playerData.m_userData.maxStamina)
		{
			m_aniPlayerStaminaMax += num2;
			if (m_aniPlayerStaminaMax > m_playerData.m_userData.maxStamina)
			{
				m_aniPlayerStaminaMax = m_playerData.m_userData.maxStamina;
			}
			flag = true;
		}
		else if (m_aniPlayerStaminaMax > m_playerData.m_userData.maxStamina)
		{
			m_aniPlayerStaminaMax -= num2;
			if (m_aniPlayerStaminaMax < m_playerData.m_userData.maxStamina)
			{
				m_aniPlayerStaminaMax = m_playerData.m_userData.maxStamina;
			}
			flag = true;
		}
		if (SingletonComponent<DungeonManager>.Instance != null && !SingletonComponent<DungeonManager>.Instance.ShowDungeonResultScreen())
		{
			if (m_aniPlayerXpValue < m_playerData.m_userData.currentXp)
			{
				m_aniPlayerXpValue += num2;
				if (m_aniPlayerXpValue >= m_playerData.m_userData.currentXp)
				{
					m_aniPlayerXpValue = m_playerData.m_userData.currentXp;
				}
				flag = true;
			}
			else if (m_aniPlayerXpValue > m_playerData.m_userData.currentXp)
			{
				m_aniPlayerXpValue -= num2;
				if (m_aniPlayerXpValue <= m_playerData.m_userData.currentXp)
				{
					m_aniPlayerXpValue = m_playerData.m_userData.currentXp;
				}
				flag = true;
			}
			int targetXp = m_playerData.TargetXp;
			if (m_aniPlayerXpMax < targetXp)
			{
				m_aniPlayerXpMax += num2;
				if (m_aniPlayerXpMax > targetXp)
				{
					m_aniPlayerXpMax = targetXp;
				}
				flag = true;
			}
			else if (m_aniPlayerXpMax > targetXp)
			{
				m_aniPlayerXpMax -= num2;
				if (m_aniPlayerXpMax < targetXp)
				{
					m_aniPlayerXpMax = targetXp;
				}
				flag = true;
			}
			m_aniPlayerXpBar = (float)m_aniPlayerXpValue / (float)m_aniPlayerXpMax;
			if (m_aniPlayerXpBar > 1f)
			{
				Dbg.LogWarn("The Exp bar value should not be over 1.0");
				if (m_aniPlayerLevel >= m_playerData.MaxLevel)
				{
					m_aniPlayerXpBar = 1f;
				}
				else
				{
					m_aniPlayerXpBar = (float)(m_aniPlayerXpValue - m_aniPlayerXpMax) / (float)m_aniPlayerXpMax;
				}
			}
			m_aniPlayerXpValue = m_playerData.m_userData.currentXp;
		}
		if (m_aniPlayerIgcValue < m_playerData.m_userData.inGameCurrency && SingletonComponent<DungeonManager>.Instance != null && !SingletonComponent<DungeonManager>.Instance.ShowDungeonResultScreen())
		{
			m_aniPlayerIgcValue += num2;
			if (m_aniPlayerIgcValue > m_playerData.m_userData.inGameCurrency)
			{
				m_aniPlayerIgcValue = m_playerData.m_userData.inGameCurrency;
			}
			flag = true;
		}
		else if (m_aniPlayerIgcValue > m_playerData.m_userData.inGameCurrency)
		{
			m_aniPlayerIgcValue -= num2;
			if (m_aniPlayerIgcValue < m_playerData.m_userData.inGameCurrency)
			{
				m_aniPlayerIgcValue = m_playerData.m_userData.inGameCurrency;
			}
			flag = true;
		}
		if (m_aniPlayerPcValue < m_playerData.m_userData.premiumCurrency)
		{
			m_aniPlayerPcValue += num2;
			if (m_aniPlayerPcValue > m_playerData.m_userData.premiumCurrency)
			{
				m_aniPlayerPcValue = m_playerData.m_userData.premiumCurrency;
			}
			flag = true;
		}
		else if (m_aniPlayerPcValue > m_playerData.m_userData.premiumCurrency)
		{
			m_aniPlayerPcValue -= num2;
			if (m_aniPlayerPcValue < m_playerData.m_userData.premiumCurrency)
			{
				m_aniPlayerPcValue = m_playerData.m_userData.premiumCurrency;
			}
			flag = true;
		}
		if (SingletonComponent<DungeonManager>.Instance != null && SingletonComponent<DungeonManager>.Instance.ShowDungeonResultScreen())
		{
			int num3 = m_playerData.Level;
			if (SingletonComponent<DungeonManager>.Instance.HasLevelUp())
			{
				num3--;
			}
			topBar.SetName(m_playerData.Name, num3);
			topBar.SetXP(m_aniPlayerXpBar, num3);
		}
		else
		{
			topBar.SetName(m_playerData.Name, m_playerData.Level);
			topBar.SetXP(m_aniPlayerXpBar, m_playerData.Level);
			topBar.labelTimer.text = ((!(num >= 1f)) ? GetStaminaCountDown() : string.Empty);
			topBar.labelStamina.text = m_aniPlayerStamina + "/" + m_aniPlayerStaminaMax;
			topBar.labelIGC.text = m_aniPlayerIgcValue + string.Empty;
			topBar.labelPC.text = m_aniPlayerPcValue + string.Empty;
		}
		if (!flag)
		{
			m_hasChangedValues = false;
			UpdateSocialCounter();
			UpdateGachaCounter();
			UpdateNewItemCounter();
			UpdateOptionCounter();
		}
	}

	private void OnDashboardOpened()
	{
		Dbg.Log("Nubee Platform OnDashboardOpened");
	}

	private void OnDashboardClosed()
	{
		Dbg.Log("Nubee Platform OnDashboardClosed");
	}

	public void ShowPromotionPopup()
	{
		if (!SingletonComponent<DailyManager>.Instance.NeedToShowPromo || IsInventoryItemsExceedLimit())
		{
			return;
		}
		if (SingletonComponent<DailyManager>.Instance.PromoDatas.Count > 0)
		{
			if (UIPopupController.Instance != null)
			{
				UIPopupController.Instance.ShowPromotion(false, OnPromotionPopupClose);
			}
		}
		else
		{
			Dbg.LogWarn("There's no promotion data, so it will not show promotion window");
		}
		SingletonComponent<DailyManager>.Instance.NeedToShowPromo = false;
	}

	public void ShowBattleHelpPopup()
	{
		if (SingletonComponent<DailyManager>.Instance.NeedToShowBattleHelp)
		{
			if (UIPopupController.Instance != null)
			{
				UIPopupController.Instance.ShowBattleHelpPopup(OnBattleHelpPopupClose);
			}
		}
		else
		{
			ShowPromotionPopup();
		}
		if (SingletonComponent<DailyManager>.Instance != null)
		{
			SingletonComponent<DailyManager>.Instance.NeedToShowBattleHelp = false;
		}
	}

	private void OnBattleHelpPopupClose()
	{
		Dbg.Log("OnBattleHelpPopupClose()");
		ShowPromotionPopup();
	}

	private void OnPromotionPopupClose()
	{
		Dbg.Log("OnPromotionPopupClose()");
		SingletonComponent<DailyManager>.Instance.NeedToShowPromo = false;
		if (!SingletonComponent<LeaderboardManager>.Instance.m_NeedToShowResult || !string.IsNullOrEmpty(SingletonComponent<DailyManager>.Instance.UserJoinEventFromPromo))
		{
			return;
		}
		long num = SingletonComponent<SaveData>.Instance.GameData.LoadPreviousLbdTimeStamp();
		Dbg.LogEx("Previous leaderboard, Local time stamp : {0}, time stamp from server: {1}", Dbg.NBLogType.COMMON_LOG, num.ToString(), SingletonComponent<LeaderboardManager>.Instance.LastResultTimeStamp.ToString());
		if (num < SingletonComponent<LeaderboardManager>.Instance.LastResultTimeStamp)
		{
			if (m_popupController != null)
			{
				m_popupController.ShowLbResultPopup();
			}
			SingletonComponent<LeaderboardManager>.Instance.SetLastRankInLocal(0);
		}
		SingletonComponent<SaveData>.Instance.GameData.SavePreviousLbdTimeStamp(SingletonComponent<LeaderboardManager>.Instance.LastResultTimeStamp);
		SingletonComponent<LeaderboardManager>.Instance.m_NeedToShowResult = false;
	}

	private void OnDailyBonusPopupClose()
	{
		SingletonComponent<DailyManager>.Instance.NeedToShowDailyBonus = false;
	}

	public float GetFaderAlpha()
	{
		return m_fader.GetWindowAlpha();
	}

	public void FadeOutOnly(Callback cb = null)
	{
		if (m_fader == null)
		{
			Dbg.Assert(false);
			return;
		}
		m_fader.enterDuration = m_fadeDuration;
		m_fader.exitDuration = m_fadeDuration;
		m_fader.Init();
		m_fader.SetAlpha(0.9f);
		m_fader.OnExit(cb);
	}

	public void FadeOutFullScreen()
	{
		if (m_fader == null)
		{
			Dbg.Assert(false);
			return;
		}
		m_fader.enterDuration = m_fadeDuration;
		m_fader.exitDuration = m_fadeDuration;
		m_fader.Init();
		m_fader.OnEnter(FadeInFullScreen);
	}

	private IEnumerator WaitNextScreenAndFadeIn()
	{
		Dbg.Log("WaitNextScreenAndFadeIn");
		yield return StartCoroutine(SwitchToNextScreen((Menu)m_nextScreenIndex));
		m_fader.OnExit();
	}

	public void FadeInFullScreen()
	{
		Dbg.Log("FadeInFullScreen");
		if (m_fader == null)
		{
			Dbg.Assert(false);
		}
		else if (0 <= m_nextScreenIndex)
		{
			StartCoroutine(WaitNextScreenAndFadeIn());
		}
	}

	public void ShowHomePage()
	{
		ShowTopAndBottomBar(true);
		ShowEventBar(false);
		StartLoadNextScreen(Menu.Home);
		FadeOutFullScreen();
	}

	public void ShowHuntPage()
	{
		StartLoadNextScreen(Menu.Max);
		FadeOutFullScreen();
	}

	public void ShowEventPage()
	{
		if (!(null != currScreen) || !(null != currScreen.currentPanel) || !(currScreen.currentPanel is EventSelectPanel))
		{
			DungeonMenuScreenJumpToState = DungeonMenuScreen.ScreenState.EVENT_SELECT;
			StartLoadNextScreen(Menu.Max, CheckGachaEventPromotion);
			FadeOutFullScreen();
		}
	}

	public void ShowDungeonPage()
	{
		DungeonMenuScreenJumpToState = DungeonMenuScreen.ScreenState.DUNGEON_SELECT;
		StartLoadNextScreen(Menu.Max);
		FadeOutFullScreen();
	}

	public void ShowTownScreen()
	{
		CharacterMenuScreenJumpToState = CharacterMenuScreen.ScreenState.HOME;
		StartLoadNextScreen(Menu.Character);
		FadeOutFullScreen();
	}

	public void ShowUpgradePage()
	{
		CharacterMenuScreenJumpToState = CharacterMenuScreen.ScreenState.FUSION;
		StartLoadNextScreen(Menu.Character);
		FadeOutFullScreen();
		m_parentSceneIsHomepage = true;
	}

	public void ShowGachaPage(bool p_toEpicTreasures = false)
	{
		if (m_currScreenIndex != 5)
		{
			if (p_toEpicTreasures)
			{
				m_onCurrentScreenReady = OnJumpToEpicTreasures;
			}
			else
			{
				m_onCurrentScreenReady = null;
			}
			StartLoadNextScreen(Menu.Treasure);
			FadeOutFullScreen();
		}
		else if (p_toEpicTreasures)
		{
			OnJumpToEpicTreasures();
		}
	}

	private void OnJumpToEpicTreasures()
	{
		if (null != m_currScreen && m_currScreen is GachaScreen)
		{
			m_currScreen.SendMessage("JumpToEpicTreasures", SendMessageOptions.DontRequireReceiver);
		}
		m_onCurrentScreenReady = null;
	}

	public void ShowLeaderboard()
	{
		if (m_popupController != null)
		{
			m_popupController.ShowLoadingPopupNetWait();
		}
		SingletonComponent<LeaderboardManager>.Instance.GetGlobalLeaderboard(0);
	}

	public void ShowArmorCollections()
	{
		StartLoadNextScreen(Menu.ArmorCollections);
		FadeOutFullScreen();
	}

	public bool CheckExcessItem2()
	{
		if (IsInventoryItemsExceedLimit())
		{
			if (SingletonComponent<StoreManager>.Instance != null)
			{
				SingletonComponent<StoreManager>.Instance.ShowBagSlotsShop(CustomizedBuyBagSlotNo);
			}
			return true;
		}
		return false;
	}

	public void OnButtonClick(GameObject source)
	{
		UIButtonMessage component = source.GetComponent<UIButtonMessage>();
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_mm_button_tap");
		if (source.transform.parent == bottomBar.transform)
		{
			int id = component.id;
			if (id >= 8 || (id != 1 && CheckExcessItem2()))
			{
				return;
			}
			if (id != m_currScreenIndex)
			{
				if (m_nextScreenIndex < 0)
				{
					Menu menu = (Menu)id;
					StartLoadNextScreen((Menu)id);
					FadeOutFullScreen();
				}
			}
			else
			{
				currScreen.Home();
			}
		}
		else if (source == topBar.buttonNubee.gameObject)
		{
			ShowPlayerProfilePopup();
		}
		else if (source == topBar.buttonAddStamina)
		{
			if (m_storeManager != null)
			{
				m_storeManager.ShowStaminaShop();
			}
			Dbg.LogWarn("Add Stamina");
		}
		else if (source == topBar.buttonAddPC)
		{
			if (m_storeManager != null)
			{
				m_storeManager.ShowDiamondsShop();
			}
			Dbg.LogWarn("Add Premium Currency");
		}
	}

	public bool CheckExcessItem(PopupCallback p_lockInTown = null, PopupCallback p_slotsExpandedCallback = null)
	{
		m_lockedInTown = p_lockInTown;
		m_slotsExpandedCallback = p_slotsExpandedCallback;
		InventoryManager instance = SingletonComponent<InventoryManager>.Instance;
		StoreManager instance2 = SingletonComponent<StoreManager>.Instance;
		if (null == inventory || instance == null || instance2 == null)
		{
			return false;
		}
		int num = inventory.HowManyExcessItems();
		if (num > 0)
		{
			if (instance.isMaxSpaceLimitReached)
			{
				LockdownPlayerToCharacterMenuOnly();
			}
			else
			{
				instance2.InventoryExcess(num, OnPurchaseSlotsYes, OnPurchaseSlotsNo);
			}
		}
		return num > 0;
	}

	private void OnPurchaseSlotsYes()
	{
		if (!CheckExcessItem(m_lockedInTown, m_slotsExpandedCallback) && m_slotsExpandedCallback != null)
		{
			m_slotsExpandedCallback();
			m_slotsExpandedCallback = null;
		}
	}

	private void OnPurchaseSlotsNo()
	{
		LockdownPlayerToCharacterMenuOnly();
	}

	public void ShowPromoCodePopup()
	{
		if (m_currScreenIndex == 7 && null != UIPopupController.Instance)
		{
			UIPopupController.Instance.ShowPromoCodePopup(OnPromoCodePopupClosed, OnPromoCodePopupClosed);
		}
	}

	public void ShowPlayerProfilePopup()
	{
		if (null != UIPopupController.Instance)
		{
			UIPopupController.Instance.ShowPlayerProfilePopup(OnPlayerProfilePopupClosed, OnPlayerProfilePopupClosed);
		}
	}

	private void OnPromoCodePopupClosed()
	{
		Messenger.Invoke("promocode_update_badge");
	}

	private void OnPlayerProfilePopupClosed()
	{
		CheckProfileButtonFlash();
	}

	private void CheckProfileButtonFlash()
	{
		if (null != topBar && null != topBar.buttonNubee)
		{
			bool needUpdateFBName = SingletonComponent<SaveData>.Instance.GameData.NeedUpdateFBName;
			topBar.buttonNubee.isFlashing = needUpdateFBName;
		}
	}

	public void ShowTopAndBottomBar(bool show)
	{
		ShowTopBar(show);
		ShowBottomBar(show);
		if (!show)
		{
			ShowEventBar(false);
		}
	}

	public void ShowBottomBar(bool show)
	{
		Vector3 vector = ((!show) ? (m_bottomBarOriginalPos + Vector3.up * -100f) : m_bottomBarOriginalPos);
		if (BarSlideRunning(bottomBar.gameObject, vector, ref m_bottomBarTarget))
		{
			return;
		}
		iTween.Stop(bottomBar.gameObject);
		iTween.MoveTo(bottomBar.gameObject, iTween.Hash("position", vector, "time", 0.25f, "islocal", true, "easetype", iTween.EaseType.easeOutBack));
	}

	public void ShowTopBar(bool show)
	{
		Vector3 vector = ((!show) ? (m_topBarOriginalPos + Vector3.up * 100f) : m_topBarOriginalPos);
		if (BarSlideRunning(topBar.gameObject, vector, ref m_topBarTarget))
		{
			return;
		}
		iTween.Stop(topBar.gameObject);
		iTween.MoveTo(topBar.gameObject, iTween.Hash("position", vector, "time", 0.25f, "islocal", true, "easetype", iTween.EaseType.easeOutBack));
	}

	// PORT (CD-026)
	private static bool BarSlideRunning(GameObject bar, Vector3 target, ref Vector3 lastTarget)
	{
		bool running = lastTarget == target && bar.GetComponent<iTween>() != null;
		lastTarget = target;
		return running;
	}

	public void ShowEventBar(bool show)
	{
		if (!(null == eventBar))
		{
			bool flag = Mathf.Approximately(eventBar.transform.localPosition.y, m_eventBarOriginalPos.y);
			if (show && !flag)
			{
				eventBar.enabled = true;
				iTween.MoveTo(eventBar.gameObject, iTween.Hash("position", m_eventBarOriginalPos, "time", 0.25f, "easetype", iTween.EaseType.easeOutBack, "islocal", true));
			}
			else if (!show && flag)
			{
				eventBar.enabled = false;
				TweenPosition.Begin(eventBar.gameObject, 0.2f, m_eventBarOriginalPos + Vector3.up * -100f);
			}
		}
	}

	public void ShowInventory(bool show)
	{
		ShowInventoryOverlay(show);
		inventory.show = show;
	}

	public void ShowInventoryOverlay(bool p_show)
	{
		if (!(null == m_overlay))
		{
			string methodName = ((!p_show) ? "FadeOut" : "FadeIn");
			m_overlay.gameObject.active = true;
			m_overlay.SendMessage(methodName, null, SendMessageOptions.DontRequireReceiver);
		}
	}

	public bool IsInventoryItemsExceedLimit()
	{
		return inventory.HasExcessItems();
	}

	private IEnumerator LoadCharacterMenuAndLockdownPlayerToCharacterMenuOnly()
	{
		yield return StartCoroutine(SwitchToNextScreen(Menu.Character));
		GachaScreen.CheckForHolyDiamondsRewards();
		if (m_popupController != null)
		{
			m_popupController.ShowOKPopupMSG("bag_slots_are_full", "empty_some_slots_desc");
		}
	}

	public void LockdownPlayerToCharacterMenuOnly()
	{
		if (m_lockedInTown != null)
		{
			m_lockedInTown();
			m_lockedInTown = null;
		}
		if (!(currScreen is CharacterMenuScreen))
		{
			StartCoroutine(LoadCharacterMenuAndLockdownPlayerToCharacterMenuOnly());
		}
	}

	private void CustomizedBuyBagSlotNo()
	{
		if (m_popupController != null)
		{
			m_popupController.ShowOKPopupMSG("bag_slots_are_full", "empty_some_slots_desc");
		}
	}

	public void UnlockAllMenus()
	{
		bottomBar.SetAllButtonsEnable(true);
		bottomBar.HideUnusedButton();
	}

	private void OnInventoryUpdated()
	{
		if (!IsInventoryItemsExceedLimit())
		{
			UnlockAllMenus();
		}
	}

	private void RetrievePlayerData()
	{
		if (m_playerData == null && null != SingletonComponent<PlayerDataManager>.Instance)
		{
			m_playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		}
	}

	public void SyncStaminaCountDown()
	{
		SingletonComponent<GameManager>.Instance.StartStaminaTimer();
		Refresh();
	}

	public void RefreshStamina()
	{
		PlayerDataManager instance = SingletonComponent<PlayerDataManager>.Instance;
		if (null != instance)
		{
			SyncStaminaCountDown();
		}
	}

	private string GetStaminaCountDown()
	{
		NBTimer staminaTimer = SingletonComponent<GameManager>.Instance.GetStaminaTimer();
		if (staminaTimer == null)
		{
			Dbg.LogWarn("Unable to get stamina timer in GameManager");
			return string.Empty;
		}
		int num = (int)(staminaTimer.Counter / 60f);
		int num2 = (int)(staminaTimer.Counter % 60f);
		return num.ToString("D2") + ":" + num2.ToString("D2");
	}

	public void SetStaminaCounterTo(float p_newValue)
	{
		NBTimer staminaTimer = SingletonComponent<GameManager>.Instance.GetStaminaTimer();
		if (staminaTimer == null)
		{
			Dbg.LogWarn("Unable to get stamina timer in GameManager");
		}
		else
		{
			staminaTimer.ResetCounterTo(p_newValue);
		}
	}

	public float GetStaminaCounterSecs()
	{
		NBTimer staminaTimer = SingletonComponent<GameManager>.Instance.GetStaminaTimer();
		if (staminaTimer != null)
		{
			return staminaTimer.Counter;
		}
		Dbg.LogError("Unable to get stamina timer in GameManager");
		return 0f;
	}

	public void SetCurrencies(CurrencyDetails currency)
	{
		SingletonComponent<PlayerDataManager>.Instance.SetCurrency(currency);
		UpdateFreeTicketsCounter(m_freeGachaCounter, m_playerData.TotalGachaCount);
		int pendingFriendRequest = m_playerData.m_PendingFriendRequest;
		pendingFriendRequest += m_playerData.m_userData.giftGacha;
		UpdateFreeTicketsCounter(m_gachaGiftCounter, pendingFriendRequest);
		SetPremiumValue(Mathf.FloorToInt(currency.premium));
		SetIgcValue(Mathf.FloorToInt(currency.ingame));
	}

	public void Refresh(float pps = 10000f)
	{
		m_rateOfAnimation = pps;
		m_hasChangedValues = true;
		LeaderboardManager instance = SingletonComponent<LeaderboardManager>.Instance;
		if (null != instance && instance.m_player != null && m_playerData != null)
		{
			instance.m_player.CharaName = m_playerData.Name;
		}
	}

	public void SetPlayerLevel(int p_value)
	{
		m_hasChangedValues = true;
		m_playerData.Level = p_value;
	}

	public void SetStaminaValue(int p_value)
	{
		m_hasChangedValues = true;
		if (p_value > m_playerData.m_userData.maxStamina)
		{
			m_playerData.m_userData.stamina = m_playerData.m_userData.maxStamina;
		}
		else
		{
			m_playerData.m_userData.stamina = p_value;
		}
	}

	public void IncreaseStaminaValue(int p_increaseValue)
	{
		SetStaminaValue(m_playerData.m_userData.stamina + p_increaseValue);
	}

	public void DecreaseStaminaValue(int p_decreaseValue)
	{
		SetStaminaValue(m_playerData.m_userData.stamina - p_decreaseValue);
		if (topBar != null)
		{
			topBar.DecrementStamina(p_decreaseValue);
		}
	}

	public void SetStaminaMax(int p_value)
	{
		m_hasChangedValues = true;
		m_playerData.m_userData.maxStamina = p_value;
	}

	public void SetXpValue(int p_value)
	{
		m_hasChangedValues = true;
		m_playerData.m_userData.currentXp = p_value;
	}

	public void SetIgcValue(int p_value)
	{
		m_hasChangedValues = true;
		m_playerData.m_userData.inGameCurrency = p_value;
	}

	public void SetPremiumValue(int p_value)
	{
		m_hasChangedValues = true;
		m_playerData.m_userData.premiumCurrency = p_value;
	}

	public void RefreshPlayerStats(bool _success = true)
	{
		if ((bool)SingletonComponent<PlayerDataManager>.Instance)
		{
			if (m_playerData == null)
			{
				m_playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
			}
			if (m_LastLevel > 0 && m_playerData.Level > m_LastLevel)
			{
				StartCoroutine(ShowLevelUpWindow(m_playerData.Level));
			}
			m_LastLevel = m_playerData.Level;
			Refresh();
		}
	}

	public void UpdateFreeTicketsCounter(UILabel p_label, int p_num, bool p_needToEnableGO = true)
	{
		Dbg.Log("UpdateFreeTicketsCounter");
		if (p_num == 0)
		{
			p_label.transform.parent.gameObject.SetActiveRecursively(false);
			return;
		}
		if (p_needToEnableGO)
		{
			p_label.transform.parent.gameObject.SetActiveRecursively(true);
		}
		p_label.text = p_num + string.Empty;
	}

	public void UpdateNewItemCounter()
	{
		InventoryManager instance = SingletonComponent<InventoryManager>.Instance;
		if (instance != null)
		{
			if (m_NewItemCounter != null)
			{
				int num = instance.GetNewCollectionItemsTotal() + instance.GetNumOfNewItems();
				UpdateHintCounter(m_NewItemCounter, num + string.Empty);
			}
			else
			{
				Dbg.LogWarn("[Warn] Unable to find the m_NewItemCounter");
			}
			return;
		}
		Dbg.LogWarn("[Warn] InventoryManager.Instance is null, unable to show new item counter in main menu");
		if (m_NewItemCounter != null)
		{
			if (m_NewItemCounter.transform.parent != null)
			{
				m_NewItemCounter.transform.parent.gameObject.SetActiveRecursively(false);
			}
			else
			{
				Dbg.LogWarn("[Warn] Unable to find the parent transform of m_NewItemCounter");
			}
		}
		else
		{
			Dbg.LogWarn("[Warn] Unable to find the m_NewItemCounter");
		}
	}

	public void UpdateSocialCounter()
	{
		int pendingFriendRequest = m_playerData.m_PendingFriendRequest;
		pendingFriendRequest += m_playerData.m_userData.giftGacha;
		UpdateFreeTicketsCounter(m_gachaGiftCounter, pendingFriendRequest);
	}

	public void UpdateGachaCounter()
	{
		UpdateFreeTicketsCounter(m_freeGachaCounter, m_playerData.TotalGachaCount);
	}

	public void UpdateOptionCounter()
	{
		DailyManager instance = SingletonComponent<DailyManager>.Instance;
		int num = 0;
		if (null != instance && instance.m_promoCodeDatas != null)
		{
			num = instance.GetActivePromoCodeDataCount(true);
		}
		UpdateHintCounter(m_OptionCounter, num.ToString());
	}

	public void UpdateAchievementCounter()
	{
		if (SingletonComponent<AchievementManager>.Instance != null)
		{
			int num = SingletonComponent<AchievementManager>.Instance.NumOfCompleted() - SingletonComponent<AchievementManager>.Instance.NumOfClaimed();
			if (m_AchievedCounter != null)
			{
				UpdateHintCounter(m_AchievedCounter, num.ToString());
			}
			else
			{
				Dbg.LogWarn("[Warn] Unable to find the m_NewItemCounter");
			}
			return;
		}
		Dbg.LogWarn("[Warn] AchievementManager.Instance is null, unable to show achieved counter in main menu");
		if (m_AchievedCounter != null)
		{
			if (m_AchievedCounter.transform.parent != null)
			{
				m_AchievedCounter.transform.parent.gameObject.SetActiveRecursively(false);
			}
			else
			{
				Dbg.LogWarn("[Warn] Unable to find the parent transform of m_AchievedCounter");
			}
		}
		else
		{
			Dbg.LogWarn("[Warn] Unable to find the m_NewItemCounter");
		}
	}

	public void UpdateHintCounter(UILabel p_label, string p_numStr)
	{
		if (!(null == p_label))
		{
			if (p_numStr == "0" || string.IsNullOrEmpty(p_numStr))
			{
				p_label.transform.parent.gameObject.SetActiveRecursively(false);
				return;
			}
			p_label.transform.parent.gameObject.SetActiveRecursively(true);
			p_label.text = p_numStr;
		}
	}

	public void EnableHeader(bool p_enable, string p_headerLabel = "")
	{
		if (m_header != null)
		{
			m_header.gameObject.SetActiveRecursively(p_enable);
		}
		if (m_headerLabel != null)
		{
			m_headerLabel.gameObject.SetActiveRecursively(p_enable);
			string text = SingletonComponent<NBLocalization>.Instance.Get(p_headerLabel);
			m_headerLabel.text = text;
		}
	}

	public void MoveHeaderToTop(bool p_withTopBar = true)
	{
		if (!(m_header != null) || !(m_header.transform.parent != null))
		{
			return;
		}
		UIAnchor component = m_header.transform.parent.GetComponent<UIAnchor>();
		if (component != null)
		{
			if (p_withTopBar)
			{
				Dbg.Log("Move header to top with top bar");
				component.relativeOffset = new Vector2(0f, 0f);
			}
			else
			{
				Dbg.Log("Move header to top without top bar");
				component.relativeOffset = new Vector2(0f, 0.09f);
			}
		}
	}

	public Transform GetRootPanelTransform()
	{
		return base.transform.FindInHierarchy("Panel");
	}

	public void ShowAchievementInfos()
	{
		if (m_AchievementPanel != null)
		{
			m_AchievementPanel.SetSubtitleInfo(SingletonComponent<AchievementManager>.Instance.NumOfCompleted(), SingletonComponent<AchievementManager>.Instance.TotalNumAchievements());
			m_AchievementPanel.gameObject.SetActiveRecursively(true);
		}
	}

	public void ShowAnnouncementPanel()
	{
		if (m_AnnouncementPanel != null)
		{
			m_AnnouncementPanel.show = true;
		}
	}

	public void ShowDragonPointsExchangePanel()
	{
		if (null != m_dragonPointsExchangePanelPrefab)
		{
			NBUIMenuPanel nBUIMenuPanel = UnityEngine.Object.Instantiate(m_dragonPointsExchangePanelPrefab) as NBUIMenuPanel;
			if (null != nBUIMenuPanel && null != inventory)
			{
				nBUIMenuPanel.transform.parent = inventory.transform.parent;
				nBUIMenuPanel.show = true;
			}
		}
	}

	public void ShowHolyDiamondsExchangePanel()
	{
		if (null != m_holyDiamondsExchangePanelPrefab)
		{
			NBUIMenuPanel nBUIMenuPanel = UnityEngine.Object.Instantiate(m_holyDiamondsExchangePanelPrefab) as NBUIMenuPanel;
			if (null != m_panel && null != inventory)
			{
				nBUIMenuPanel.transform.parent = inventory.transform.parent;
				nBUIMenuPanel.show = true;
			}
		}
	}

	public void CountdownBpRegenerateTime()
	{
		if (m_playerData.m_userData.shield >= m_playerData.m_userData.maxShield)
		{
			m_playerData.m_userData.timeNextShield = 0L;
			bpTimer = 0f;
			return;
		}
		bpTimer += Time.deltaTime;
		if (!(bpTimer >= 1f))
		{
			return;
		}
		bpTimer -= 1f;
		m_playerData.m_userData.timeNextShield--;
		if (m_playerData.m_userData.timeNextShield <= 0)
		{
			m_playerData.m_userData.shield++;
			if (m_playerData.m_userData.shield < m_playerData.m_userData.maxShield)
			{
				m_playerData.m_userData.timeNextShield += GLOBAL.GAME_SETTINGS.shields_reset_time;
			}
		}
	}
}
