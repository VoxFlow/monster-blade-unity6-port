public class AnnouncementMenuScreen : NBUIMenuScreen
{
	private enum ScreenState
	{
		ANNOUNCEMENT = 0
	}

	private ScreenState m_currState;

	private ScreenState currState
	{
		get
		{
			return m_currState;
		}
		set
		{
			m_currState = value;
			if (m_currState == ScreenState.ANNOUNCEMENT && (bool)MainMenu.Instance.m_sharedBackButton)
			{
				MainMenu.Instance.m_sharedBackButton.gameObject.SetActiveRecursively(true);
				UIButtonMessage component = MainMenu.Instance.m_sharedBackButton.GetComponent<UIButtonMessage>();
				if (component != null)
				{
					component.target = base.gameObject;
					component.functionName = "OnButtonClick";
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
	}

	protected override void Init()
	{
		PushState(ScreenState.ANNOUNCEMENT);
	}

	public override void Start()
	{
	}

	private void Update()
	{
	}

	private void PushState(ScreenState nextState)
	{
		if (nextState >= ScreenState.ANNOUNCEMENT && (int)nextState < menuPanels.Length && PushPanelWithTransitions(menuPanels[(int)nextState]))
		{
			currState = nextState;
		}
	}

	public void OnButtonClick()
	{
		if ((bool)MainMenu.Instance.m_sharedBackButton)
		{
			UIButtonMessage component = MainMenu.Instance.m_sharedBackButton.GetComponent<UIButtonMessage>();
			component.target = null;
			MainMenu.Instance.m_sharedBackButton.gameObject.SetActiveRecursively(false);
		}
		MainMenu.Instance.ShowHomePage();
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_cancel");
	}
}
