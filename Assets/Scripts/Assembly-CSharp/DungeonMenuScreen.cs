using UnityEngine;

public class DungeonMenuScreen : NBUIMenuScreen
{
	public enum ScreenState
	{
		HOME = 0,
		AREA_SELECT = 0,
		DUNGEON_SELECT = 1,
		FRIEND_SELECT = 2,
		EVENT_SELECT = 3,
		PET_EVENT_AREA_SELECT = 4,
		PET_EVENT_SELECT = 5,
		FRIEND_DETAILS = 6,
		SET_DEMO_PANEL = 7
	}

	private ScreenState _currState;

	private ScreenState _nextState;

	[SerializeField]
	private GameObject m_particleEffects;

	[SerializeField]
	private GameObject m_localBackground;

	private ScreenState currState
	{
		get
		{
			return _currState;
		}
		set
		{
			if (!(this == null))
			{
				_currState = value;
				TutorialManager instance = SingletonComponent<TutorialManager>.Instance;
				bool flag = null != instance && instance.enabled && _currState == ScreenState.EVENT_SELECT;
				if ((bool)backButton)
				{
					backButton.SetActiveRecursively(true);
				}
				string p_headerLabel = string.Empty;
				bool flag2 = true;
				float p_value = -101f;
				switch (_currState)
				{
				case ScreenState.HOME:
					p_headerLabel = "cityselect_header";
					break;
				case ScreenState.DUNGEON_SELECT:
					p_headerLabel = "dungeon_select_header";
					break;
				case ScreenState.FRIEND_SELECT:
					p_headerLabel = "friendselect_header";
					break;
				case ScreenState.EVENT_SELECT:
					p_headerLabel = ((!flag) ? "dungeon_events_header" : "dungeon_events_tutorial_header");
					break;
				case ScreenState.FRIEND_DETAILS:
					p_headerLabel = "slayer_profile_header";
					flag2 = false;
					p_value = -46f;
					break;
				}
				if (MainMenu.Instance != null)
				{
					MainMenu.Instance.EnableHeader(true, p_headerLabel);
					MainMenu.Instance.ShowTopAndBottomBar(flag2);
					MainMenu.Instance.MoveHeaderToTop(flag2);
				}
				if (null != backButton)
				{
					backButton.transform.SetLocalY(p_value);
				}
			}
		}
	}

	public override void OnEnable()
	{
		base.OnEnable();
		Init();
	}

	public override void OnDisable()
	{
		base.OnDisable();
		if (MainMenu.Instance != null)
		{
			MainMenu.Instance.DungeonMenuScreenJumpToState = ScreenState.HOME;
		}
		if (!SingletonComponent<GameManager>.Instance.IsLoadingScene())
		{
			Reset();
		}
	}

	public override void Start()
	{
		base.Start();
	}

	public override void Home()
	{
		if (_currState != ScreenState.HOME)
		{
			base.Home();
		}
	}

	protected override void Reset()
	{
		ClearStack();
		if ((bool)MenuBackground3D.Instance)
		{
			MenuBackground3D.Instance.HideBackground();
		}
	}

	protected override void Init()
	{
		if (MainMenu.Instance != null && backButton == null && MainMenu.Instance.m_sharedBackButton != null)
		{
			backButton = MainMenu.Instance.m_sharedBackButton.gameObject;
			m_backButton = MainMenu.Instance.m_sharedBackButton;
		}
		if ((bool)backButton)
		{
			UIButtonMessage component = backButton.GetComponent<UIButtonMessage>();
			component.target = base.gameObject;
		}
		if (SingletonComponent<TutorialManager>.Instance.enabled && !SingletonComponent<TutorialManager>.Instance.IsCompleted(Tutorial.State.AllCombats))
		{
			PushState(ScreenState.EVENT_SELECT);
		}
		else if (MainMenu.Instance.DungeonMenuScreenJumpToState == ScreenState.EVENT_SELECT)
		{
			PushState(ScreenState.EVENT_SELECT);
			MainMenu.Instance.DungeonMenuScreenJumpToState = ScreenState.HOME;
			if ((bool)backButton)
			{
				backButton.SetActiveRecursively(false);
			}
		}
		else if (MainMenu.Instance.DungeonMenuScreenJumpToState == ScreenState.DUNGEON_SELECT)
		{
			PushStateNoTransitions(ScreenState.HOME);
			PushStateNoTransitions(ScreenState.DUNGEON_SELECT);
		}
		else
		{
			PushState(ScreenState.HOME);
		}
	}

	public void PushState(ScreenState nextState)
	{
		if (nextState >= ScreenState.HOME && (int)nextState < menuPanels.Length && PushPanelWithTransitions(menuPanels[(int)nextState]))
		{
			_nextState = nextState;
		}
	}

	public void PushStateNoTransitions(ScreenState nextState)
	{
		if (nextState >= ScreenState.HOME && (int)nextState < menuPanels.Length)
		{
			PushPanel(menuPanels[(int)nextState]);
			currState = nextState;
		}
	}

	public override void PushPanel(NBUIMenuPanel newPanel)
	{
		base.PushPanel(newPanel);
		currState = _nextState;
	}

	private void PopState(bool showPrevious = true)
	{
		PopPanelWithTransitions(showPrevious, SetCurrState);
	}

	private void SetCurrState()
	{
		ScreenState currentPanelIndex = (ScreenState)GetCurrentPanelIndex();
		currState = currentPanelIndex;
	}

	public void OnBack()
	{
		Dbg.Log("--- OnBack ---");
		if (GetCurrentPanelIndex() == 3)
		{
			MainMenu.Instance.ShowHomePage();
		}
		else if (GetCurrentPanelIndex() == 7)
		{
			PopState();
			PushState(ScreenState.DUNGEON_SELECT);
		}
		else if (GetCurrentPanelIndex() == 0)
		{
			MainMenu.Instance.ShowHomePage();
		}
		else
		{
			PopState();
		}
	}

	private void OnButtonClick(GameObject source)
	{
		if (source == backButton)
		{
			OnBack();
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_cancel");
		}
	}

	private void OnDungeonAreaCellEvent(DungeonAreaCell cell)
	{
		if ((bool)SingletonComponent<DungeonManager>.Instance)
		{
			SingletonComponent<DungeonManager>.Instance.CurrentCity = cell.data.city;
		}
		PushState(ScreenState.DUNGEON_SELECT);
	}

	private void OnViewSetDemoEvent(SetInformation p_setInfo)
	{
		SingletonComponent<DungeonManager>.Instance.m_demoSetInfo = p_setInfo;
		PushState(ScreenState.SET_DEMO_PANEL);
	}

	private void OnDungeonCellEvent(DungeonCell cell)
	{
		if ((bool)SingletonComponent<DungeonManager>.Instance)
		{
			SingletonComponent<DungeonManager>.Instance.CurrentDungeon = cell.data.dungeon;
			SingletonComponent<DungeonManager>.Instance.BattleType = Battle.Type.Dungeon;
		}
		if (!(SingletonComponent<PlayerDataManager>.Instance == null) && !(UIPopupController.Instance == null) && !(SingletonComponent<StoreManager>.Instance == null))
		{
			PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
			if (playerData.m_userData.maxStamina < cell.data.dungeon.stamina)
			{
				UIPopupController.Instance.ShowOKPopupMSG("common_info", "dungeon_prompt_not_enough_max_stamina");
				Messenger.Invoke("dungeon_select_cancel");
			}
			else if (playerData.m_userData.stamina < cell.data.dungeon.stamina)
			{
				SingletonComponent<StoreManager>.Instance.ShowInsufficientStaminaShop();
				Messenger.Invoke("dungeon_select_cancel");
			}
			else if (IsWeaponGood())
			{
				PushState(ScreenState.FRIEND_SELECT);
			}
		}
	}

	private bool IsWeaponGood()
	{
		bool flag = true;
		PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		Equipment.Data equipmentData = playerData.GetEquipmentData(Equipment.Part.Weapon);
		DungeonDbStruct currentDungeon = SingletonComponent<DungeonManager>.Instance.CurrentDungeon;
		Dbg.Log("[weaponcheck]start");
		Dbg.Log("[weaponcheck]dungeon element:" + currentDungeon.element);
		Dbg.Log("[weaponcheck]monsterlevel:" + currentDungeon.min);
		Dbg.Log("[weaponcheck]weapon element:" + equipmentData.element);
		Dbg.Log("[weaponcheck]weapon level:" + equipmentData.itemDetails.Level);
		Dbg.Log("[weaponcheck]end");
		Element.Type element = equipmentData.element;
		Element.Type element2 = currentDungeon.element;
		if (Element.GetWeakElement(element2) == element)
		{
			flag = false;
			if (Element.GetWeakElement(element) == element2)
			{
				flag = true;
			}
		}
		if (currentDungeon.min > equipmentData.itemDetails.BaseLevel)
		{
			flag = false;
		}
		if (!flag)
		{
			UIPopupController.Instance.ShowWeaponCheck(equipmentData, currentDungeon, OnDungeonEnhanceItemFirst, OnDungeonGachaFirst, OnDungeonContinue);
			Messenger.Invoke("dungeon_select_cancel");
		}
		return flag;
	}

	private void OnDungeonEnhanceItemFirst()
	{
		MainMenu.Instance.CharacterMenuScreenJumpToState = CharacterMenuScreen.ScreenState.FUSION;
		MainMenu.Instance.StartLoadNextScreenWithFadeout(MainMenu.Menu.Character);
		Dbg.Log("[weaponcheck]goto enhance");
	}

	private void OnDungeonGachaFirst()
	{
		MainMenu.Instance.StartLoadNextScreenWithFadeout(MainMenu.Menu.Treasure);
		Dbg.Log("[weaponcheck]goto gacha");
	}

	private void OnDungeonContinue()
	{
		PushState(ScreenState.FRIEND_SELECT);
	}

	private void OnEventCellEvent(EventCell cell)
	{
		if (!(SingletonComponent<DungeonManager>.Instance != null))
		{
			return;
		}
		if (cell.data.id.Contains("TUTORIAL"))
		{
			Dbg.Log("It's tutorial, no need to show the difficulty selection UI");
			SingletonComponent<DungeonManager>.Instance.CurrentDungeonEvent = cell.data;
			SingletonComponent<DungeonManager>.Instance.BattleType = Battle.Type.DungeonEvent;
			PushState(ScreenState.FRIEND_SELECT);
			return;
		}
		SingletonComponent<DungeonManager>.Instance.CurrentDungeonEvent = cell.data;
		SingletonComponent<DungeonManager>.Instance.BattleType = Battle.Type.DungeonEvent;
		if (UIPopupController.Instance != null)
		{
			UIPopupController.Instance.ShowEventDifficultySelection();
		}
		if (MainMenu.Instance.currScreen.currentPanel is EventSelectPanel)
		{
			EventSelectPanel eventSelectPanel = MainMenu.Instance.currScreen.currentPanel as EventSelectPanel;
			if (eventSelectPanel != null)
			{
				eventSelectPanel.ResetClicked();
			}
		}
	}

	private void OnPetEventCellEvent(PetEventCellEvent evt)
	{
		Dbg.LogWarn("PetEvent Cell Clicked");
	}

	private void OnEventButtonClick()
	{
		if (GetCurrentPanelIndex() == 1)
		{
			PushState(ScreenState.EVENT_SELECT);
		}
		else
		{
			PushState(ScreenState.EVENT_SELECT);
		}
	}

	private void OnPetEventButtonClick()
	{
	}

	public void GotoDungeonEventSelectionPanel()
	{
		PushState(ScreenState.EVENT_SELECT);
	}

	private void OnFriendDetailsClick(PlayerData _friendData)
	{
		if (_friendData == null)
		{
			return;
		}
		Dbg.Log("Show Friend Details : " + _friendData.Name);
		PushState(ScreenState.FRIEND_DETAILS);
		CharacterInfoPanel characterInfoPanel = null;
		if (base.currentPanel == null)
		{
			Dbg.LogWarn("The current panel is null, try to get the number 8 panel");
			characterInfoPanel = menuPanels[6] as CharacterInfoPanel;
		}
		else
		{
			characterInfoPanel = base.currentPanel as CharacterInfoPanel;
			if (characterInfoPanel == null)
			{
				Dbg.LogWarn("The current panel is not CharacterInfoPanel, try to get the number 8 panel");
				characterInfoPanel = menuPanels[6] as CharacterInfoPanel;
			}
		}
		if (characterInfoPanel == null)
		{
			Dbg.LogError("fatal error unable to find CharacterInfoPanel");
		}
		else
		{
			characterInfoPanel.SetData(_friendData);
		}
	}

	public void EnableBackground(bool enable)
	{
		if ((bool)m_particleEffects)
		{
			m_particleEffects.SetActiveRecursively(enable);
		}
		if ((bool)m_localBackground)
		{
			m_localBackground.SetActiveRecursively(enable);
		}
	}

	public void EnableBackBtnCollider(bool p_enable)
	{
		backButton.GetComponent<Collider>().enabled = p_enable;
	}
}
