using UnityEngine;

public class HomeMenuScreen : NBUIMenuScreen
{
	[SerializeField]
	private AchievementBtn m_achievementBtn;

	public override void OnEnable()
	{
		base.OnEnable();
		Init();
		PushPanel(menuPanels[0]);
		if (backButton != null)
		{
			backButton.gameObject.SetActiveRecursively(false);
		}
	}

	public override void OnDisable()
	{
		base.OnDisable();
	}

	private void CallInit(bool _loaded = true)
	{
		Init();
	}

	public override void Start()
	{
		if (MainMenu.Instance != null)
		{
			if (backButton == null && MainMenu.Instance.m_sharedBackButton != null)
			{
				backButton = MainMenu.Instance.m_sharedBackButton.gameObject;
				m_backButton = MainMenu.Instance.m_sharedBackButton;
			}
			if (backButton != null)
			{
				backButton.gameObject.SetActiveRecursively(false);
			}
			MainMenu.Instance.ShowTopAndBottomBar(true);
		}
	}

	protected override void Reset()
	{
	}
}
