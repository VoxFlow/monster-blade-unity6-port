using UnityEngine;

public class EventBar : MonoBehaviour
{
	[SerializeField]
	private BattlePointsDisplay m_battlePoints;

	[SerializeField]
	private UILabel m_dragonPointsLabel;

	[SerializeField]
	private UIButton m_dragonPointsExchangeButton;

	private void OnEnable()
	{
		Messenger<int>.AddListener("exchange_update_points", OnUpdateDragonPoints);
		Init();
	}

	private void OnDisable()
	{
		Messenger<int>.RemoveListener("exchange_update_points", OnUpdateDragonPoints);
		Reset();
	}

	private void OnUpdateDragonPoints(int p_points)
	{
		RefreshDisplay();
	}

	private void Reset()
	{
		if (null != m_battlePoints)
		{
			m_battlePoints.enabled = false;
		}
	}

	private void Init()
	{
		if (null != m_battlePoints)
		{
			m_battlePoints.enabled = true;
		}
		PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		TutorialManager instance = SingletonComponent<TutorialManager>.Instance;
		if (playerData != null && null != instance && null != m_dragonPointsExchangeButton)
		{
			m_dragonPointsExchangeButton.SetButtonGrey(instance.enabled);
		}
		RefreshDisplay();
	}

	private void RefreshDisplay()
	{
		if (!(null == SingletonComponent<PlayerDataManager>.Instance))
		{
			PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
			if (playerData.details != null && null != m_dragonPointsLabel)
			{
				m_dragonPointsLabel.text = playerData.details.currencies.dragonPoints + string.Empty;
			}
		}
	}

	private void OnButtonClick(GameObject p_sender)
	{
		if (null != m_dragonPointsExchangeButton && m_dragonPointsExchangeButton.gameObject == p_sender)
		{
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_secondary_ui_tap");
			MainMenu instance = MainMenu.Instance;
			if (null != instance)
			{
				instance.ShowDragonPointsExchangePanel();
			}
		}
	}
}
