using UnityEngine;

public class EventDifficultySelectionPopup : MonoBehaviour
{
	[SerializeField]
	private UIButtonMessage[] m_buttonMessages;

	[SerializeField]
	private UILabel m_easy_btn_desc;

	[SerializeField]
	private UILabel m_normal_btn_desc;

	[SerializeField]
	private UILabel m_hard_btn_desc;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void SetButtonMessageTarget()
	{
		UIButtonMessage[] buttonMessages = m_buttonMessages;
		foreach (UIButtonMessage uIButtonMessage in buttonMessages)
		{
			uIButtonMessage.target = base.gameObject;
		}
	}

	public void SetButtonBpCost()
	{
		if (SingletonComponent<NBLocalization>.Instance != null)
		{
			m_easy_btn_desc.text = string.Format(SingletonComponent<NBLocalization>.Instance.Get("event_easy_desc"), GLOBAL.GAME_SETTINGS.event_easy_cost.ToString());
			m_normal_btn_desc.text = string.Format(SingletonComponent<NBLocalization>.Instance.Get("event_normal_desc"), GLOBAL.GAME_SETTINGS.event_normal_cost.ToString());
			m_hard_btn_desc.text = string.Format(SingletonComponent<NBLocalization>.Instance.Get("event_hard_desc"), GLOBAL.GAME_SETTINGS.event_hard_cost.ToString());
		}
	}

	public void OnEasyClick()
	{
		MockOnDifficultyButtonClick(0);
	}

	public void OnNormalClick()
	{
		MockOnDifficultyButtonClick(1);
	}

	public void OnHardClick()
	{
		MockOnDifficultyButtonClick(2);
	}

	private void MockOnDifficultyButtonClick(int p_difficulty)
	{
		if (CheckBattlePointEnough(p_difficulty))
		{
			SetDifficulty(p_difficulty);
			return;
		}
		UIPopupController.Instance.CloseCurrentPopup();
		SingletonComponent<StoreManager>.Instance.ShowInsufficientBattlePointShop();
	}

	private void SetDifficulty(int p_difficulty = 0)
	{
		if (SingletonComponent<DungeonManager>.Instance != null)
		{
			SingletonComponent<DungeonManager>.Instance.CurrentEventDifficulty = p_difficulty;
			Dbg.LogEx("Event difficulty : {0}", Dbg.NBLogType.COMMON_LOG, SingletonComponent<DungeonManager>.Instance.CurrentEventDifficulty.ToString());
		}
		UIPopupController.Instance.CloseCurrentPopup();
		DungeonMenuScreen dungeonMenuScreen = MainMenu.Instance.currScreen as DungeonMenuScreen;
		if (dungeonMenuScreen != null)
		{
			dungeonMenuScreen.PushState(DungeonMenuScreen.ScreenState.FRIEND_SELECT);
		}
	}

	private bool CheckBattlePointEnough(int p_difficulty)
	{
		PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		if (playerData != null)
		{
			switch (p_difficulty)
			{
			case 0:
				return playerData.m_userData.shield >= GLOBAL.GAME_SETTINGS.event_easy_cost;
			case 1:
				return playerData.m_userData.shield >= GLOBAL.GAME_SETTINGS.event_normal_cost;
			case 2:
				return playerData.m_userData.shield >= GLOBAL.GAME_SETTINGS.event_hard_cost;
			default:
				return false;
			}
		}
		return false;
	}
}
