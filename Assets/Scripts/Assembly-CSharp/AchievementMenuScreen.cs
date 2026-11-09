using UnityEngine;

public class AchievementMenuScreen : NBUIMenuScreen
{
	private enum ScreenState
	{
		HOME = 0,
		ACHIEVEMENT = 1
	}

	[SerializeField]
	private UILabel m_achievedCounter;

	private ScreenState m_currState;

	private ScreenState m_nextState;

	private ScreenState currState
	{
		get
		{
			return m_currState;
		}
		set
		{
			if (m_currState != value)
			{
				m_currState = value;
				if (m_currState == ScreenState.HOME)
				{
					backButton.SetActiveRecursively(false);
				}
				else
				{
					backButton.SetActiveRecursively(true);
				}
			}
		}
	}

	public override void Start()
	{
	}

	public override void Home()
	{
		if (m_currState != ScreenState.HOME)
		{
			base.Home();
		}
	}

	public override void OnEnable()
	{
		base.OnEnable();
		Init();
		if (backButton != null)
		{
			backButton.gameObject.SetActiveRecursively(false);
		}
	}

	protected override void Init()
	{
		if (MainMenu.Instance != null && backButton == null && MainMenu.Instance.m_sharedBackButton != null)
		{
			backButton = MainMenu.Instance.m_sharedBackButton.gameObject;
			m_backButton = MainMenu.Instance.m_sharedBackButton;
		}
		PushState(ScreenState.HOME);
	}

	private void PushState(ScreenState nextState)
	{
		if (nextState >= ScreenState.HOME && (int)nextState < menuPanels.Length && PushPanelWithTransitions(menuPanels[(int)nextState]))
		{
			m_nextState = nextState;
		}
	}

	public override void PushPanel(NBUIMenuPanel newPanel)
	{
		base.PushPanel(newPanel);
		currState = m_nextState;
	}

	private void PopState(bool showPrevious = true)
	{
		PopPanelWithTransitions(showPrevious);
		ScreenState currentPanelIndex = (ScreenState)GetCurrentPanelIndex();
		currState = currentPanelIndex;
	}

	public override void UpdateRedIconNum()
	{
		MainMenu.Instance.UpdateHintCounter(m_achievedCounter, (SingletonComponent<AchievementManager>.Instance.NumOfCompleted() - SingletonComponent<AchievementManager>.Instance.NumOfClaimed()).ToString());
	}

	private void OnAchievementButtonClick()
	{
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_secondary_ui_tap");
		Dbg.Log("[Info] OnAchievementButtonClick");
	}
}
