using System.Collections.Generic;
using UnityEngine;

public class AreaCompletePrompt : MonoBehaviour
{
	public UIButton m_buttonHome;

	public UIButton m_buttonReplay;

	public UIButton m_buttonNextDungeon;

	private void Awake()
	{
		Dbg.Assert(m_buttonHome);
		Dbg.Assert(m_buttonReplay);
		Dbg.Assert(m_buttonNextDungeon);
	}

	private void Start()
	{
		PlayerDataManager instance = SingletonComponent<PlayerDataManager>.Instance;
		PlayerData playerData = null;
		if (null != instance && null != m_buttonReplay)
		{
			playerData = instance.GetPlayerData();
			if (playerData != null && !playerData.m_isInLevelRangeForCurrentDungeon)
			{
				m_buttonReplay.gameObject.SetActiveRecursively(false);
			}
		}
		DungeonManager instance2 = SingletonComponent<DungeonManager>.Instance;
		if (null != instance2 && instance2.IsPlayingEventDungeon && null != m_buttonNextDungeon)
		{
			m_buttonNextDungeon.gameObject.SetActiveRecursively(false);
		}
	}

	private void OnButtonClick(GameObject p_source)
	{
		if (null == p_source)
		{
			return;
		}
		MainMenu instance = MainMenu.Instance;
		if (null == instance)
		{
			Dbg.LogError("There is no instance of MainMenu.");
		}
		else if (null != m_buttonHome && p_source == m_buttonHome.gameObject)
		{
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_cancel");
			instance.StartHomePage();
		}
		else if (null != m_buttonReplay && p_source == m_buttonReplay.gameObject)
		{
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_cancel");
			DungeonManager instance2 = SingletonComponent<DungeonManager>.Instance;
			bool flag = false;
			if (null != instance2)
			{
				flag = instance2.IsPlayingEventDungeon;
				if (flag)
				{
					instance2.NextDungeonEvent = instance2.CurrentDungeonEvent;
				}
				else
				{
					instance2.NextDungeon = instance2.CurrentDungeon;
				}
			}
			instance.ShowBottomBar(true);
			if (flag)
			{
				instance.ShowEventPage();
			}
			else
			{
				instance.ShowDungeonPage();
			}
		}
		else if (null != m_buttonNextDungeon && p_source == m_buttonNextDungeon.gameObject)
		{
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_ok");
			DungeonManager instance3 = SingletonComponent<DungeonManager>.Instance;
			if (null != instance3)
			{
				List<DungeonManager.CityStruct> allUnlockedCities = instance3.GetAllUnlockedCities();
				DungeonManager.CityStruct cityStruct = allUnlockedCities[allUnlockedCities.Count - 1];
				instance3.CurrentCity = cityStruct.city;
				instance3.NextDungeon = null;
			}
			instance.ShowBottomBar(true);
			instance.ShowDungeonPage();
		}
	}

	private void Update()
	{
		MainMenu instance = MainMenu.Instance;
		if (!(null == instance) && Input.GetKeyDown(KeyCode.Escape))
		{
			instance.StartHomePage();
		}
	}
}
