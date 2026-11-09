using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : SingletonComponent<TutorialManager>
{
	public class Pointer
	{
		public Vector3 location;

		public PointingHand.Direction direction;

		public Transform parent;

		public float delay;
	}

	public delegate void TutorialMgrInitCB();

	private Tutorial.State m_tutorialProgress;

	private Dictionary<Tutorial.State, TutorialData> m_tutorialDataMap = new Dictionary<Tutorial.State, TutorialData>();

	private Dictionary<Tutorial.State, string> m_tutorialPrefabNameMap = new Dictionary<Tutorial.State, string>();

	public GameObject m_mascot;

	private Tutorial.State m_currentState;

	private GameObject m_hint;

	private GameObject m_handPrefab;

	private PointingHand m_pointer;

	private GameObject m_subtitlePrefab;

	private GameObject m_instructionPrefab;

	private SubtitlePopup m_subtitle;

	private InstructionBox m_instruction;

	public bool showDodgeInstructions;

	public bool DelayShowOchaSaviorAchievement;

	public Tutorial.State progress
	{
		get
		{
			return m_tutorialProgress;
		}
	}

	public string progressString
	{
		get
		{
			List<string> list = new List<string>();
			Tutorial.State tutorialProgress = m_tutorialProgress;
			Tutorial.State[] array = (Tutorial.State[])Enum.GetValues(typeof(Tutorial.State));
			string[] names = Enum.GetNames(typeof(Tutorial.State));
			for (int i = 0; i < array.Length; i++)
			{
				Tutorial.State state = array[i];
				if ((state & (state - 1)) == 0 && (tutorialProgress & state) != Tutorial.State.None)
				{
					list.Add(names[i]);
				}
			}
			return string.Join("|", list.ToArray());
		}
	}

	public Tutorial.State currentState
	{
		get
		{
			return m_currentState;
		}
		set
		{
			m_currentState = value;
		}
	}

	public bool IsBattleCompleted
	{
		get
		{
			return IsCompleted(Tutorial.State.Combat3);
		}
	}

	public bool NeedSpecificFriend(string p_dungeonId)
	{
		return p_dungeonId == "TUTORIAL_2";
	}

	public bool IsCompleted(Tutorial.State state)
	{
		return (m_tutorialProgress & state) == state;
	}

	public bool IsCompletedOnly(Tutorial.State state, Tutorial.State filter = Tutorial.State.All)
	{
		return (m_tutorialProgress & filter) == state;
	}

	private void Awake()
	{
		m_handPrefab = Resources.Load("Tutorial/Prefabs/Tutorial Pointing Hand") as GameObject;
		m_instructionPrefab = Resources.Load("Tutorial/Prefabs/Instruction Box") as GameObject;
		m_subtitlePrefab = Resources.Load("UI/Prefabs/Generic/Ocha Subtitle") as GameObject;
		Dbg.Assert(m_handPrefab);
		Dbg.Assert(m_instructionPrefab);
		Dbg.Assert(m_subtitlePrefab);
	}

	private void OnEnable()
	{
		Messenger<MainMenu.Menu>.AddListener("mainmenu_screen_enter", OnEnterScreen);
		Messenger<NBUIMenuPanel>.AddListener("mainmenu_show_panel", OnShowPanel);
		Messenger.AddListener("mainmenu_start", OnMainMenuStart);
		Messenger.AddListener("select_allies", OnSelectAllies);
		Messenger<CutsceneController.CutsceneType>.AddListener(CutsceneMessage.CUTSCENE_END, OnCutsceneEnd);
		Messenger<Tutorial.State>.AddListener("tutorial_completed", OnTutorialCompleted);
		Messenger.AddListener("battle_init", OnBattleInit);
		Messenger.AddListener("battle_start", OnBattleStart);
		Messenger.AddListener("dungeon_cleared_start", OnDungeonClearedStart);
		Messenger.AddListener("dungeon_cleared_end", OnDungeonClearedEnd);
		Messenger<FriendCell>.AddListener("dungeon_select_friend", OnFriendSelected);
		Messenger.AddListener("dungeon_enter", OnEnterDungeon);
	}

	private void OnDisable()
	{
		Messenger<MainMenu.Menu>.RemoveListener("mainmenu_screen_enter", OnEnterScreen);
		Messenger<NBUIMenuPanel>.RemoveListener("mainmenu_show_panel", OnShowPanel);
		Messenger.RemoveListener("mainmenu_start", OnMainMenuStart);
		Messenger.RemoveListener("select_allies", OnSelectAllies);
		Messenger<CutsceneController.CutsceneType>.RemoveListener(CutsceneMessage.CUTSCENE_END, OnCutsceneEnd);
		Messenger<Tutorial.State>.RemoveListener("tutorial_completed", OnTutorialCompleted);
		Messenger.RemoveListener("battle_init", OnBattleInit);
		Messenger.RemoveListener("battle_start", OnBattleStart);
		Messenger.RemoveListener("dungeon_cleared_start", OnDungeonClearedStart);
		Messenger.RemoveListener("dungeon_cleared_end", OnDungeonClearedEnd);
		Messenger<FriendCell>.RemoveListener("dungeon_select_friend", OnFriendSelected);
		Messenger.RemoveListener("dungeon_enter", OnEnterDungeon);
	}

	private bool IsCombatTutorial(Tutorial.State state)
	{
		return (state & Tutorial.State.AllCombats) != 0;
	}

	private void OnFriendSelected(FriendCell p_cell)
	{
		if (IsCompleted(Tutorial.State.Fusion) && !IsCompleted(Tutorial.State.Combat3))
		{
			DungeonEventStruct currentDungeonEvent = SingletonComponent<DungeonManager>.Instance.CurrentDungeonEvent;
			if (currentDungeonEvent.id == "TUTORIAL_3")
			{
				AnalyticsManager.FirstInstallEvents("tut_selectfriend2");
			}
		}
	}

	private void OnEnterDungeon()
	{
		if (IsCompleted(Tutorial.State.Fusion) && !IsCompleted(Tutorial.State.Combat3))
		{
			DungeonEventStruct currentDungeonEvent = SingletonComponent<DungeonManager>.Instance.CurrentDungeonEvent;
			if (currentDungeonEvent.id == "TUTORIAL_3")
			{
				AnalyticsManager.FirstInstallEvents("tut_battleloading2");
			}
		}
	}

	private void OnDungeonClearedStart()
	{
		if (!IsCompleted(Tutorial.State.Combat2))
		{
			Apsalar.NBSendEvent("tut_areaclear", true);
		}
		else if (!IsCompleted(Tutorial.State.Combat3))
		{
			AnalyticsManager.FirstInstallEvents("tut_areaclear2");
		}
	}

	private void OnDungeonClearedEnd()
	{
		if (!IsCompleted(Tutorial.State.Combat2))
		{
			AnalyticsManager.FirstInstallEvents("tut_areaclear_continue");
		}
		else if (!IsCompleted(Tutorial.State.Combat3))
		{
			AnalyticsManager.FirstInstallEvents("tut_areaclear2_continue");
		}
	}

	private void OnBattleInit()
	{
		if (IsCombatTutorial(m_currentState))
		{
			Tutorial tutorial = null;
			TutorialData data = null;
			switch (m_currentState)
			{
			case Tutorial.State.Combat1:
				SingletonComponent<StageManager>.Instance.PlayerObject.GetComponent<Player>().SetVisible(false);
				SingletonComponent<StageManager>.Instance.EnemyObject.GetComponent<Monster>().SetVisible(false);
				break;
			case Tutorial.State.Combat2:
				CreateTutorial(Tutorial.State.Combat2);
				break;
			}
			if ((bool)tutorial)
			{
				tutorial.Init(data);
			}
		}
	}

	private void OnBattleStart()
	{
		if (IsCombatTutorial(m_currentState))
		{
			Tutorial.State state = m_currentState;
			if (state == Tutorial.State.Combat3)
			{
				CreateTutorial(Tutorial.State.Combat3);
			}
		}
	}

	public void LoadProgress()
	{
		m_tutorialProgress = (Tutorial.State)SingletonComponent<SaveData>.Instance.GameData.TutorialProgress;
		Dbg.Log("Load tutorial progress: " + SingletonComponent<SaveData>.Instance.GameData.TutorialProgress);
		bool flag = false;
		if (IsCompleted(Tutorial.State.All) || flag)
		{
			base.enabled = false;
		}
		else if (!IsCompleted(Tutorial.State.Combat1))
		{
			m_currentState = Tutorial.State.Combat1;
		}
	}

	public void SaveProgress()
	{
		SingletonComponent<SaveData>.Instance.GameData.TutorialProgress = (int)m_tutorialProgress;
		Dbg.Log("Save tutorial progress: " + SingletonComponent<SaveData>.Instance.GameData.TutorialProgress);
	}

	public void ClearProgress()
	{
		m_tutorialProgress = Tutorial.State.None;
	}

	public void AddCompletedState(Tutorial.State state)
	{
		m_tutorialProgress |= state;
	}

	private void OnTutorialCompleted(Tutorial.State state)
	{
		AddCompletedState(state);
		SaveProgress();
		switch (state)
		{
		case Tutorial.State.Combat1:
		{
			DelayShowOchaSaviorAchievement = true;
			GameManager instance4 = SingletonComponent<GameManager>.Instance;
			if (null != instance4)
			{
				instance4.LoadScene("MainMenuInit", true, false);
			}
			break;
		}
		case Tutorial.State.Combat2:
		{
			GameManager instance3 = SingletonComponent<GameManager>.Instance;
			if (null != instance3)
			{
				instance3.LoadScene("MainMenuInit", true, false);
			}
			break;
		}
		case Tutorial.State.Inventory:
			RemoveTutorial(Tutorial.State.Inventory);
			CreateTutorial(Tutorial.State.PreCombat2);
			break;
		case Tutorial.State.Fusion:
		{
			RemoveTutorial(Tutorial.State.Fusion);
			MainMenu.Instance.bottomBar.UnlockAllMenus();
			DungeonEventStruct dungeonEventStruct = SingletonComponent<DungeonManager>.Instance.FindDungeonEvent("TUTORIAL_3");
			if (dungeonEventStruct != null)
			{
				dungeonEventStruct.locked = false;
			}
			UIPopupController.Instance.ShowOKPopupMSG("tutorial_completed_header", "tutorial_completed", OnTutorialCompletedPopupOk, "tutorial_completed_button");
			break;
		}
		case Tutorial.State.Gacha:
			RemoveTutorial(Tutorial.State.Gacha);
			CreateTutorial(Tutorial.State.Gacha2);
			break;
		case Tutorial.State.Gacha2:
			RemoveTutorial(Tutorial.State.Gacha2);
			CreateTutorial(Tutorial.State.Inventory);
			break;
		case Tutorial.State.Combat3:
		{
			if (SingletonComponent<SaveData>.Instance.GameData.TutorialProgress != 379)
			{
				SingletonComponent<SaveData>.Instance.GameData.TutorialProgress = 379;
				SingletonComponent<SaveData>.Instance.GameData.SaveAchievementData();
			}
			GameManager instance2 = SingletonComponent<GameManager>.Instance;
			if (null != instance2)
			{
				AnalyticsManager.FirstInstallEvents("tut_openingmovie");
				instance2.LoadScene("IntroMovie", true, false);
			}
			break;
		}
		case Tutorial.State.AfterEnterName:
		{
			RemoveTutorial(Tutorial.State.AfterEnterName);
			base.enabled = false;
			MainMenu instance = MainMenu.Instance;
			if (null != instance)
			{
				MainMenu.Instance.CheckPlayerName();
			}
			break;
		}
		}
	}

	private void OnTutorialCompletedPopupOk()
	{
		AnalyticsManager.FirstInstallEvents("tut_tutendpopup_ok");
		MainMenu.Instance.ShowEventPage();
	}

	private void OnCutsceneEnd(CutsceneController.CutsceneType p_type)
	{
		if (p_type == CutsceneController.CutsceneType.TUTORIAL1)
		{
			AnalyticsManager.FirstInstallEvents("tut_ochaend");
			CreateTutorial(Tutorial.State.Combat1);
			Time.timeScale = 1f;
		}
	}

	public void RemoveTutorial(Tutorial.State p_state)
	{
		if (m_tutorialPrefabNameMap.ContainsKey(p_state))
		{
			string value = m_tutorialPrefabNameMap[p_state];
			GameObject gameObject = GameObject.FindGameObjectWithTag("Tutorial");
			if ((bool)gameObject && gameObject.name.Contains(value))
			{
				UnityEngine.Object.Destroy(gameObject);
			}
		}
	}

	public bool IsTutorialExisting(Tutorial.State p_state)
	{
		if (!m_tutorialPrefabNameMap.ContainsKey(p_state))
		{
			return false;
		}
		string value = m_tutorialPrefabNameMap[p_state];
		GameObject gameObject = GameObject.FindGameObjectWithTag("Tutorial");
		if ((bool)gameObject && gameObject.name.Contains(value))
		{
			return true;
		}
		return false;
	}

	public bool IsAnyTutorialExisting()
	{
		GameObject gameObject = GameObject.FindGameObjectWithTag("Tutorial");
		if ((bool)gameObject)
		{
			return true;
		}
		return false;
	}

	private IEnumerator DoCreateTutorialAfterDelay(Tutorial.State p_state, float p_delay)
	{
		UIPopupController.SetAllInputsEnabled(false);
		yield return new MBWaitForSeconds(p_delay);   // PORT (CD-059)
		CreateTutorial(p_state);
		yield return null;
		UIPopupController.Instance.EnableUI();
	}

	public TutorialDataRow GetRandomRowFromParameterData(Tutorial.State p_state)
	{
		TutorialDataRow result = null;
		TutorialData parameterData = GetParameterData(p_state);
		if (parameterData != null && parameterData.Count > 0)
		{
			int index = UnityEngine.Random.Range(0, parameterData.Count);
			List<string> list = new List<string>();
			foreach (string key in parameterData.Keys)
			{
				list.Add(key);
			}
			result = parameterData[list[index]];
		}
		return result;
	}

	public TutorialData GetParameterData(Tutorial.State p_state)
	{
		if (m_tutorialDataMap != null && m_tutorialDataMap.ContainsKey(p_state))
		{
			return m_tutorialDataMap[p_state];
		}
		Dbg.LogWarn("There is no tutorial data with state: " + p_state, Dbg.NBLogType.TUTORIAL);
		return null;
	}

	public Tutorial CreateTutorial(Tutorial.State p_state)
	{
		if (m_mascot == null)
		{
			LoadMascot("Mascot/TutorialMascot");
		}
		TutorialData tutorialData = null;
		if (m_tutorialDataMap.ContainsKey(p_state))
		{
			tutorialData = m_tutorialDataMap[p_state];
		}
		if (tutorialData == null || tutorialData.Count == 0)
		{
			return null;
		}
		if (!m_tutorialPrefabNameMap.ContainsKey(p_state))
		{
			return null;
		}
		string text = m_tutorialPrefabNameMap[p_state];
		m_currentState = p_state;
		GameObject gameObject = GameObject.FindGameObjectWithTag("Tutorial");
		if ((bool)gameObject)
		{
			if (gameObject.name.Contains(text))
			{
				return gameObject.GetComponent<Tutorial>();
			}
			UnityEngine.Object.Destroy(gameObject);
		}
		GameObject gameObject2 = Resources.Load("Tutorial/Prefabs/" + text) as GameObject;
		Dbg.Assert(gameObject2);
		GameObject gameObject3 = UnityEngine.Object.Instantiate(gameObject2) as GameObject;
		Tutorial component = gameObject3.GetComponent<Tutorial>();
		if ((bool)component)
		{
			component.Init(tutorialData);
		}
		return component;
	}

	private void OnSelectAllies()
	{
	}

	private void OnMainMenuStart()
	{
		if (!IsCompleted(Tutorial.State.Fusion))
		{
			MainMenu.Instance.bottomBar.LockAllMenus();
		}
		if (IsCompletedOnly(Tutorial.State.Combat1, Tutorial.State.AllCombats))
		{
			MainMenu.Instance.bottomBar.UnlockMenu(MainMenu.Menu.Home);
			MainMenu.Instance.bottomBar.UnlockMenu(MainMenu.Menu.Character);
			MainMenu.Instance.bottomBar.UnlockMenu(MainMenu.Menu.Treasure);
			if (!IsCompleted(Tutorial.State.Gacha))
			{
				MainMenu.Instance.bottomBar.LockMenu(MainMenu.Menu.Character);
				MainMenu.Instance.bottomBar.LockMenu(MainMenu.Menu.Treasure);
			}
			if (!IsCompleted(Tutorial.State.Inventory))
			{
				MainMenu.Instance.bottomBar.LockMenu(MainMenu.Menu.Character);
				DungeonEventStruct dungeonEventStruct = SingletonComponent<DungeonManager>.Instance.FindDungeonEvent("TUTORIAL_2");
				if (dungeonEventStruct != null)
				{
					dungeonEventStruct.locked = true;
				}
			}
		}
		else if (IsCompletedOnly((Tutorial.State)17, Tutorial.State.AllCombats))
		{
			MainMenu.Instance.bottomBar.UnlockMenu(MainMenu.Menu.Home);
			MainMenu.Instance.bottomBar.UnlockMenu(MainMenu.Menu.Character);
			MainMenu.Instance.bottomBar.UnlockMenu(MainMenu.Menu.Treasure);
			if (!IsCompleted(Tutorial.State.Fusion))
			{
				DungeonEventStruct dungeonEventStruct2 = SingletonComponent<DungeonManager>.Instance.FindDungeonEvent("TUTORIAL_3");
				if (dungeonEventStruct2 != null)
				{
					dungeonEventStruct2.locked = true;
				}
			}
		}
		else if (IsCompleted(Tutorial.State.AllCombats))
		{
			MainMenu.Instance.bottomBar.UnlockAllMenus();
		}
	}

	private void Start()
	{
	}

	public void InitManager(TutorialMgrInitCB p_completeCB = null)
	{
		NBProfiler.BeginTask("[TUTORIAL_MANAGER][INIT_MANAGER]");
		LoadTutorialData();
		NBProfiler.EndTask("[TUTORIAL_MANAGER][INIT_MANAGER]");
	}

	private void LoadTutorialData()
	{
		NBBinaryFormat parametersFor = SingletonComponent<ParameterManager>.Instance.GetParametersFor("Subtitle");
		string[] names = Enum.GetNames(typeof(Tutorial.State));
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		string[] array = names;
		foreach (string text in array)
		{
			dictionary.Add(text, text);
		}
		int rowsCount = parametersFor.RowsCount;
		List<string> rowsNames = parametersFor.RowsNames;
		for (int j = 0; j < rowsCount; j++)
		{
			TutorialDataRow tutorialDataRow = new TutorialDataRow();
			string id = rowsNames[j];
			tutorialDataRow.Id = id;
			tutorialDataRow.Type = parametersFor.GetData(j, 0);
			tutorialDataRow.SubtitleKey = parametersFor.GetData(j, 1);
			tutorialDataRow.AssetPath = parametersFor.GetData(j, 2);
			tutorialDataRow.CallbackStr = parametersFor.GetData(j, 3);
			tutorialDataRow.NextId = parametersFor.GetData(j, 4);
			tutorialDataRow.MascotFacial = parametersFor.GetData(j, 5);
			tutorialDataRow.MascotAnim = parametersFor.GetData(j, 6);
			tutorialDataRow.Sound = parametersFor.GetData(j, 7);
			tutorialDataRow.Music = parametersFor.GetData(j, 8);
			tutorialDataRow.TriggerKey = parametersFor.GetData(j, 9);
			tutorialDataRow.Arg = parametersFor.GetData(j, 10);
			if (dictionary.ContainsKey(tutorialDataRow.Type))
			{
				Tutorial.State key = (Tutorial.State)(int)Enum.Parse(typeof(Tutorial.State), tutorialDataRow.Type);
				if (!m_tutorialPrefabNameMap.ContainsKey(key) && tutorialDataRow.AssetPath != string.Empty)
				{
					m_tutorialPrefabNameMap.Add(key, tutorialDataRow.AssetPath);
				}
				if (m_tutorialDataMap.ContainsKey(key))
				{
					TutorialData tutorialData = m_tutorialDataMap[key];
					tutorialData.Add(tutorialDataRow.Id, tutorialDataRow);
				}
				else
				{
					TutorialData tutorialData2 = new TutorialData();
					tutorialData2.Add(tutorialDataRow.Id, tutorialDataRow);
					m_tutorialDataMap.Add(key, tutorialData2);
				}
			}
		}
		SingletonComponent<ParameterManager>.Instance.UnloadParametersFor("Subtitle");
		parametersFor = null;
	}

	public void ShowInstruction(string p_text, InstructionBox.Anchor p_anchor = InstructionBox.Anchor.Top)
	{
		if (!m_instruction)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(m_instructionPrefab) as GameObject;
			Dbg.Assert(gameObject);
			if (!gameObject)
			{
				return;
			}
			gameObject.transform.SetLayerRecursively(LayerMask.NameToLayer("PopUpLayer"));
			gameObject.transform.parent = UIPopupController.Instance.Panel;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localScale = Vector3.one;
			UIAnchor componentInChildren = MBHier.GetComponentInChildren<UIAnchor>(gameObject.transform);
			if ((bool)componentInChildren)
			{
				componentInChildren.uiCamera = NGUITools.FindCameraForLayer(gameObject.layer);
			}
			m_instruction = gameObject.GetComponent<InstructionBox>();
			Dbg.Assert(m_instruction);
		}
		if ((bool)m_instruction)
		{
			m_instruction.anchor = p_anchor;
			m_instruction.text = p_text;
			m_instruction.Show();
		}
	}

	public void HideInstruction()
	{
		if ((bool)m_instruction)
		{
			m_instruction.Hide();
		}
	}

	public void ShowSubtitle(string p_text, bool p_showMascot = true, string p_mascotAnimName = "", string p_mascotFace = "", string p_soundName = "", PopupCallback p_onClose = null, bool p_isTutorial = false, bool p_behindUIBar = false, bool p_blockUIBar = true)
	{
		if (!m_subtitle)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(m_subtitlePrefab) as GameObject;
			Dbg.Assert(gameObject);
			if (!gameObject)
			{
				return;
			}
			m_subtitle = gameObject.GetComponent<SubtitlePopup>();
			Dbg.Assert(m_subtitle);
		}
		if (!m_subtitle)
		{
			return;
		}
		if (p_behindUIBar)
		{
			if ((bool)MainMenu.Instance.m_panelSubtitle)
			{
				m_subtitle.transform.SetLayerRecursively(LayerMask.NameToLayer("MascotLayer"));
				m_subtitle.transform.parent = MainMenu.Instance.m_panelSubtitle.transform;
				m_subtitle.transform.localScale = Vector3.one;
				if ((bool)m_subtitle.m_offset)
				{
					m_subtitle.m_offset.localPosition = Vector3.up * 165f;
				}
				if (p_blockUIBar)
				{
					UIPopupController.LockUICamera("UI_Bar", true);
					BoxCollider boxCollider = m_subtitle.GetComponent<Collider>() as BoxCollider;
					if ((bool)boxCollider)
					{
						boxCollider.center = new Vector3(boxCollider.center.x, 0f, boxCollider.center.z);
						boxCollider.size = new Vector3(boxCollider.size.x, 1000f, boxCollider.size.z);
					}
				}
				else
				{
					BoxCollider boxCollider2 = m_subtitle.GetComponent<Collider>() as BoxCollider;
					if ((bool)boxCollider2)
					{
						boxCollider2.center = new Vector3(boxCollider2.center.x, -170f, boxCollider2.center.z);
						boxCollider2.size = new Vector3(boxCollider2.size.x, 200f, boxCollider2.size.z);
					}
				}
			}
		}
		else
		{
			m_subtitle.transform.SetLayerRecursively(LayerMask.NameToLayer("PopUpLayer"));
			m_subtitle.transform.parent = UIPopupController.Instance.Panel;
			m_subtitle.transform.localScale = Vector3.one;
			if ((bool)m_subtitle.m_offset)
			{
				m_subtitle.m_offset.localPosition = Vector3.up * 82f;
			}
		}
		if ((bool)m_subtitle.m_spriteOverlay)
		{
			m_subtitle.m_spriteOverlay.enabled = p_blockUIBar;
		}
		m_subtitle.transform.localPosition = Vector3.zero;
		UIAnchor anchor = m_subtitle.m_anchor;
		if ((bool)anchor)
		{
			anchor.uiCamera = NGUITools.FindCameraForLayer(m_subtitle.gameObject.layer);
		}
		m_subtitle.Show(p_text, p_showMascot, p_mascotAnimName, p_mascotFace, p_soundName, p_onClose, p_isTutorial);
	}

	public void EndSubtitle()
	{
		if ((bool)m_subtitle)
		{
			m_subtitle.Hide();
		}
		UIPopupController.LockUICamera("UI_Bar", false);
	}

	public void ShowMascot(bool p_enable)
	{
		if (m_mascot == null)
		{
			return;
		}
		if (!p_enable)
		{
			Animation componentInChildren = MBHier.GetComponentInChildren<Animation>(m_mascot);
			if ((bool)componentInChildren)
			{
				componentInChildren.Stop();
			}
		}
		m_mascot.SetActiveRecursively(p_enable);
	}

	public void LoadMascot(string p_mascotPath)
	{
		UnityEngine.Object obj = Resources.Load(p_mascotPath);
		if (obj != null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(obj) as GameObject;
			if (gameObject != null)
			{
				m_mascot = gameObject;
				if (m_mascot == null)
				{
					Dbg.LogError("[Error] Unable to find mascot in path: " + p_mascotPath);
				}
				else
				{
					m_mascot.SetActiveRecursively(false);
				}
			}
			else
			{
				Dbg.LogError("[Error] Unable to find mascot in path: " + p_mascotPath);
			}
		}
		else
		{
			Dbg.LogError("[Error] Unable to find mascot model in path: " + p_mascotPath);
		}
	}

	private void OnShowPanel(NBUIMenuPanel panel)
	{
		if (panel is HomePanel)
		{
			if (IsCompletedOnly(Tutorial.State.Combat1, Tutorial.State.AllCombats))
			{
				if (!IsCompleted(Tutorial.State.Gacha))
				{
					if (!IsTutorialExisting(Tutorial.State.Gacha))
					{
						StartCoroutine(DoCreateTutorialAfterDelay(Tutorial.State.Gacha, 0.5f));
					}
				}
				else if (!IsCompleted(Tutorial.State.Gacha2))
				{
					if (!IsTutorialExisting(Tutorial.State.Gacha2))
					{
						StartCoroutine(MainMenu.Instance.SwitchToNextScreen(MainMenu.Menu.Treasure));
						StartCoroutine(DoCreateTutorialAfterDelay(Tutorial.State.Gacha2, 0.5f));
					}
				}
				else if (!IsCompleted(Tutorial.State.Inventory))
				{
					if (!IsTutorialExisting(Tutorial.State.Inventory))
					{
						StartCoroutine(DoCreateTutorialAfterDelay(Tutorial.State.Inventory, 0.5f));
					}
				}
				else if (!IsTutorialExisting(Tutorial.State.PreCombat2))
				{
					StartCoroutine(DoCreateTutorialAfterDelay(Tutorial.State.PreCombat2, 0.5f));
				}
			}
			else if (IsCompletedOnly((Tutorial.State)17, Tutorial.State.AllCombats))
			{
				if (!IsCompleted(Tutorial.State.Fusion) && !IsTutorialExisting(Tutorial.State.Fusion))
				{
					StartCoroutine(DoCreateTutorialAfterDelay(Tutorial.State.Fusion, 0.5f));
				}
			}
			else if (IsCompleted(Tutorial.State.AllCombats) && !IsCompleted(Tutorial.State.AfterEnterName))
			{
				StartCoroutine(DoCreateTutorialAfterDelay(Tutorial.State.AfterEnterName, 0.5f));
			}
		}
		else if (null != panel && panel.parentScreen is GachaScreen && !IsAnyTutorialExisting())
		{
		}
		if (IsCompleted(Tutorial.State.Fusion) && !IsCompleted(Tutorial.State.Combat3))
		{
			if (panel is EventSelectPanel)
			{
				AnalyticsManager.FirstInstallEvents("tut_hunt2");
			}
			else if (panel is FriendSelectPanel)
			{
				AnalyticsManager.FirstInstallEvents("tut_dungeon3");
			}
		}
		if (!IsCompleted(Tutorial.State.Combat2) && IsCompleted(Tutorial.State.Inventory) && panel is CharacterInfoPanel)
		{
			Dbg.Assert((bool)MainMenu.Instance && (bool)MainMenu.Instance.currScreen);
			if (MainMenu.Instance.currScreen is DungeonMenuScreen)
			{
				HidePointer();
			}
		}
	}

	private void OnEnterScreen(MainMenu.Menu menu)
	{
		Dbg.Log("Entering screen: " + menu);
		HidePointer();
		switch (menu)
		{
		case MainMenu.Menu.Home:
			if (!IsCompletedOnly(Tutorial.State.Combat1, Tutorial.State.AllCombats))
			{
				break;
			}
			if (!IsCompleted(Tutorial.State.Gacha))
			{
				if (IsTutorialExisting(Tutorial.State.Gacha))
				{
				}
			}
			else if (!IsCompleted(Tutorial.State.Gacha2))
			{
				if (IsTutorialExisting(Tutorial.State.Gacha2))
				{
				}
			}
			else if (!IsCompleted(Tutorial.State.Inventory) && IsTutorialExisting(Tutorial.State.Inventory))
			{
			}
			break;
		}
	}

	public IEnumerator DoShowPointer(Pointer p_pointer)
	{
		if (p_pointer != null)
		{
			float nextTime = UnityEngine.Time.realtimeSinceStartup + p_pointer.delay;
			while (UnityEngine.Time.realtimeSinceStartup < nextTime)
			{
				yield return null;
			}
			ShowPointer(p_pointer.location, p_pointer.direction, p_pointer.parent);
		}
	}

	public void ShowPointerWithDelay(Vector3 p_location, PointingHand.Direction p_direction, Transform p_parent, float p_delay)
	{
		Pointer pointer = new Pointer();
		pointer.location = p_location;
		pointer.direction = p_direction;
		pointer.parent = p_parent;
		pointer.delay = p_delay;
		StartCoroutine("DoShowPointer", pointer);
	}

	public void ShowPointerAtBottomMenu(MainMenu.Menu menu, float p_delay = 0f)
	{
		if (menu >= MainMenu.Menu.Home && menu < MainMenu.Menu.Max)
		{
			Transform transform = MainMenu.Instance.bottomBar.m_buttons[(int)menu].transform;
			Vector3 localPosition = transform.localPosition;
			localPosition.y = MainMenu.Instance.bottomBarOriginalPos.y + transform.localPosition.y;
			localPosition.z = -5f;
			Dbg.Assert(MainMenu.Instance.bottomBar);
			UIPanel uIPanel = NGUITools.FindInParents<UIPanel>(MainMenu.Instance.bottomBar.gameObject);
			Dbg.Assert(uIPanel);
			if (p_delay == 0f)
			{
				ShowPointer(localPosition, PointingHand.Direction.Down, uIPanel.transform);
			}
			else if (p_delay > 0f)
			{
				Pointer pointer = new Pointer();
				pointer.location = localPosition;
				pointer.direction = PointingHand.Direction.Down;
				pointer.parent = uIPanel.transform;
				pointer.delay = p_delay;
				StartCoroutine("DoShowPointer", pointer);
			}
		}
	}

	public void ShowPointer(Vector3 p_location, PointingHand.Direction p_direction, Transform p_parent)
	{
		if (!m_pointer)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(m_handPrefab) as GameObject;
			Dbg.Assert(gameObject);
			m_pointer = gameObject.GetComponent<PointingHand>();
		}
		Dbg.Assert(m_pointer);
		Transform parent = m_pointer.transform.parent;
		m_pointer.transform.parent = p_parent;
		if ((bool)p_parent && p_parent != parent)
		{
			m_pointer.transform.SetLayerRecursively(p_parent.gameObject.layer);
			if ((bool)m_pointer.sprite)
			{
				m_pointer.sprite.enabled = false;
				m_pointer.sprite.enabled = true;
			}
		}
		m_pointer.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
		m_pointer.gameObject.SetActiveRecursively(true);
		m_pointer.PointTo(p_location, p_direction);
	}

	public void HidePointer()
	{
		StopCoroutine("DoShowPointer");
		if ((bool)m_pointer)
		{
			m_pointer.gameObject.SetActiveRecursively(false);
			m_pointer.transform.parent = UIPopupController.Instance.Panel;
		}
	}
}
