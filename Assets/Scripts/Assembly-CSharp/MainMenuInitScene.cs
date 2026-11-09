using Models;
using ServiceCalls;
using UnityEngine;

public class MainMenuInitScene : MonoBehaviour
{
	public string nextScreen = string.Empty;

	private void Start()
	{
		if (!SingletonComponent<TutorialManager>.Instance.IsCompleted(Tutorial.State.Combat1))
		{
			UIPopupController.Instance.ShowOKPopupMSG("common_info", "tutorial_firstcombat_incomplete", GoToTutorial);
			return;
		}
		NBProfiler.EndTask("[LOAD_MAINMENU_INIT]");
		NBProfiler.PrintUsedHeap("[MAINMENU_INIT]");
		NBProfiler.BeginTask("[MAINMENU_INIT][START]");
		UnloadBattle();
		LoadMenu();
		if (null != SingletonComponent<TutorialManager>.Instance && !SingletonComponent<TutorialManager>.Instance.enabled)
		{
			CallDailyLoginBonus();
		}
		SingletonComponent<StageManager>.Instance.CreatePlayerMainMenuTemplate(InitItemVisual);
		if ((bool)SingletonComponent<TutorialManager>.Instance)
		{
			SingletonComponent<TutorialManager>.Instance.EndSubtitle();
		}
	}

	private void OnPrepareBattlesSuccess(Services.ServiceHandle p_handle)
	{
		GameObject gameObject = new GameObject();
		gameObject.AddComponent<PreloadBattle>();
	}

	private void OnPrepareBattlesFail(Services.ServiceHandle p_handle, string p_error)
	{
		UIPopupController.Instance.HideLoadingPopup();
		Dbg.LogError(" Failed to start Battle : " + ServiceCallUtils.GetErrorMessage(p_handle));
		if (p_handle == null)
		{
			UIPopupController.Instance.ShowServerGenericError(GoToTutorial);
			return;
		}
		if (p_handle.IsNoInternet())
		{
			UIPopupController.Instance.ShowNoNetMessage(GoToTutorial);
			return;
		}
		switch ((ServiceResponseCodes)p_handle.nReturnCode)
		{
		case ServiceResponseCodes.EXCEPTION_DUNGEON_NOT_ENOUGH_STAMINA:
			UIPopupController.Instance.ShowOKPopupMSG("common_info", "dungeon_prompt_not_enough_stamina");
			if (null != MainMenu.Instance)
			{
				MainMenu.Instance.RefreshStamina();
			}
			break;
		case ServiceResponseCodes.EXCEPTION_DUNGEON_NOT_ENOUGH_SHIELD:
			UIPopupController.Instance.ShowInsufficientShield(GoToTutorial);
			break;
		default:
			UIPopupController.Instance.ShowServerGenericError(GoToTutorial, p_handle.nReturnCode);
			break;
		}
	}

	private void GoToTutorial()
	{
		SingletonComponent<DungeonManager>.Instance.GenerateNewSessionID();
		SingletonComponent<DungeonManager>.Instance.StartDungeonEventOnline(OnPrepareBattlesSuccess, OnPrepareBattlesFail, 0, string.Empty);
	}

	private void UnloadBattle()
	{
		NBProfiler.BeginTask("[UNLOADING_BATTLE_MAINMENU_INIT]");
		SingletonComponent<SoundManager>.Instance.UnloadMonsterSoundsInList();
		SingletonComponent<SoundManager>.Instance.UnloadSoundsInList("PlayList_LevelGeneral");
		SingletonComponent<SoundManager>.Instance.UnloadSoundsInList("PlayList_Tutorial01");
		SingletonComponent<SoundManager>.Instance.UnloadTitleSounds();
		UIPopupController.Instance.UnloadBattleScreen();
		Singleton<CharacterGenerator>.Instance.ClearCache();
		SingletonComponent<TexturesManager>.Instance.UnloadAllBattleAtlas();
		NBProfiler.EndTask("[UNLOADING_BATTLE_MAINMENU_INIT]");
	}

	private void LoadMenu()
	{
		NBProfiler.BeginTask("[LOADING_MAINMENU_INIT]");
		SingletonComponent<SoundManager>.Instance.LoadPlayList("PlayList_Menu");
		SingletonComponent<TexturesManager>.Instance.LoadAllMenuAtlas();
		NBProfiler.EndTask("[LOADING_MAINMENU_INIT]");
	}

	private void InitItemVisual()
	{
		SingletonComponent<InventoryManager>.Instance.LoadVisual(ItemVisualMgrCB);
	}

	private void ItemVisualMgrCB(bool p_succeed)
	{
		AddNewItemsFromDungeon();
		SingletonComponent<AnnouncementManager>.Instance.GetAnnoucementsFromServer(GoToMainMenu);
	}

	private void GoToMainMenu()
	{
		UIPopupController.Instance.DisableOtherUILayers();
		SingletonComponent<GameManager>.Instance.LoadScene(nextScreen, true, false);
	}

	private void AddNewItemsFromDungeon()
	{
		DungeonManager instance = SingletonComponent<DungeonManager>.Instance;
		InventoryManager instance2 = SingletonComponent<InventoryManager>.Instance;
		if (!(instance != null) || !instance.HasDungeonComplete() || !(instance2 != null))
		{
			return;
		}
		foreach (ServerInventoryItem chestReward in instance.ChestRewards)
		{
			instance2.AddItemFromServer(chestReward);
		}
		instance2.SaveNewItemsState();
	}

	private void CallDailyLoginBonus()
	{
		if (UIPopupController.LoginCounter == 0)
		{
			SingletonComponent<DailyManager>.Instance.DailyPromotion();
		}
		UIPopupController.LoginCounter++;
	}
}
