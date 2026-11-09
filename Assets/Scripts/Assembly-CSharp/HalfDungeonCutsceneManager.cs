using UnityEngine;

public class HalfDungeonCutsceneManager : SingletonComponent<HalfDungeonCutsceneManager>
{
	public string landDragonCutscene = "Cutscene_5thDungeon_LandDragon";

	public int currentDungeon;

	public GameObject playerTemplate;

	private void Awake()
	{
		base.enabled = false;
	}

	public string GetCurrentDungeonBossCutscene()
	{
		DungeonManager instance = SingletonComponent<DungeonManager>.Instance;
		if (instance == null)
		{
			return landDragonCutscene;
		}
		return landDragonCutscene;
	}

	public void PlayHaflDungeonCutscene()
	{
		string currentDungeonBossCutscene = GetCurrentDungeonBossCutscene();
		GameManager instance = SingletonComponent<GameManager>.Instance;
		StageManager instance2 = SingletonComponent<StageManager>.Instance;
		if (instance2 != null && instance2.PlayerTemplate != null)
		{
			playerTemplate = Object.Instantiate(instance2.PlayerTemplate) as GameObject;
			playerTemplate.SetActiveRecursively(false);
			Object.DontDestroyOnLoad(playerTemplate);
		}
		if (instance != null)
		{
			instance.LoadScene(currentDungeonBossCutscene, true, false);
		}
		else
		{
			instance.LoadScene("MainMenuInit", true, false);
		}
	}

	public void CleanUp()
	{
		if (playerTemplate != null)
		{
			Object.Destroy(playerTemplate);
			playerTemplate = null;
		}
	}
}
