using UnityEngine;

public class PreloadBattle : MonoBehaviour
{
	private int createdCharacter;

	private int characterToCreate = 2;

	private void Start()
	{
		SingletonComponent<CameraConfiguration>.Instance.InitManager();
		LoadSound();
		if (UIPopupController.Instance != null)
		{
			UIPopupController.Instance.ShowLoadingPopup(SingletonComponent<NBLocalization>.Instance.Get("character_creation_header"), SingletonComponent<NBLocalization>.Instance.Get("character_creation_content"));
			UIPopupController.Instance.LoadBattlePopups();
		}
		SingletonComponent<InventoryManager>.Instance.UnloadAllVisual();
		Singleton<CharacterGenerator>.Instance.ClearCache();
		SingletonComponent<TexturesManager>.Instance.UnloadAllMenuAtlas();
		SingletonComponent<TexturesManager>.Instance.LoadAllBattleAtlas();
		SingletonComponent<StageManager>.Instance.CreatePlayerTemplate(PlayerTemplateCreated);
		UIPopupController.Instance.LoadBattleClearScreen();
	}

	private void FirstFriendTemplateCreated()
	{
		createdCharacter++;
		if (createdCharacter >= characterToCreate)
		{
			LoadBattle();
		}
	}

	private void PlayerTemplateCreated()
	{
		createdCharacter++;
		SingletonComponent<StageManager>.Instance.CreateFriendTempalte(FirstFriendTemplateCreated);
	}

	private void LoadBattle()
	{
		if (UIPopupController.Instance != null)
		{
			UIPopupController.Instance.HideLoadingPopup();
		}
		SingletonComponent<GameManager>.Instance.LoadScene("BattleView", true, false);
		Singleton<CharacterGenerator>.Instance.UnloadPreviousConfiguration(true);
	}

	private void LoadSound()
	{
		SingletonComponent<SoundManager>.Instance.UnloadSoundsInList("PlayList_Menu");
		SingletonComponent<SoundManager>.Instance.LoadPlayList("PlayList_LevelGeneral");
		int battleCount = SingletonComponent<DungeonManager>.Instance.GetBattleCount();
		for (int i = 0; i < battleCount; i++)
		{
			BattleStruct battleData = SingletonComponent<DungeonManager>.Instance.GetBattleData(i);
			DbPlayerDataStruct monsterDbData = SingletonComponent<PlayerDataManager>.Instance.GetMonsterDbData(battleData.monsterId);
			string playList = Singleton<MonsterFactory>.Instance.GetPlayList(monsterDbData.name);
			SingletonComponent<SoundManager>.Instance.LoadPlayList(playList);
		}
	}
}
