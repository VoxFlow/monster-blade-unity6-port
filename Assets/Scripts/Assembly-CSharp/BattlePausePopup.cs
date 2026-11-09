using UnityEngine;

public class BattlePausePopup : MonoBehaviour
{
	public enum PauseTab
	{
		COMBOS = 0,
		SETTINGS = 1
	}

	public enum Mode
	{
		OPTION_MENU = 0,
		BATTLE_PAUSE = 1
	}

	[SerializeField]
	private GameObject m_comboHintPageTemplate;

	private BattlePauseComboHintPage m_comboHintPage;

	[SerializeField]
	private GameObject m_settingPageTemplate;

	private BattlePauseSettingsPage m_settingPage;

	[SerializeField]
	private PauseTabBtn m_settingsTabBtn;

	[SerializeField]
	private PauseTabBtn m_comboTabBtn;

	[SerializeField]
	private UILabel m_exitBtnText;

	[SerializeField]
	private UILabel m_resumeBtnText;

	[SerializeField]
	private UILabel m_title;

	public UIButton m_exitBtn;

	public UIButton m_resumeBtn;

	public Mode m_Mode = Mode.BATTLE_PAUSE;

	public PauseTab m_defaultShowingTab;

	private void Awake()
	{
		Dbg.Assert(m_exitBtn);
		Dbg.Assert(m_resumeBtn);
	}

	private void Start()
	{
		if (m_defaultShowingTab == PauseTab.COMBOS)
		{
			OnComboHintTabBtnClick();
		}
		else if (m_defaultShowingTab == PauseTab.SETTINGS)
		{
			OnSettingsTabBtnClick();
		}
		else
		{
			Dbg.LogWarnEx("Unknown default tab type : {0}", Dbg.NBLogType.COMMON_WARNING, m_defaultShowingTab.ToString());
		}
		if (m_Mode == Mode.BATTLE_PAUSE)
		{
			NBUILocalize nBUILocalize = null;
			if (null != m_exitBtnText)
			{
				nBUILocalize = m_exitBtnText.GetComponent<NBUILocalize>();
				if (null != nBUILocalize)
				{
					nBUILocalize.Key = "gamehud_prompt_exit_button";
				}
				nBUILocalize = m_resumeBtnText.GetComponent<NBUILocalize>();
				if (null != nBUILocalize)
				{
					nBUILocalize.Key = "gamehud_prompt_resume_button";
				}
			}
		}
		else if (m_Mode == Mode.OPTION_MENU)
		{
			Object.DestroyImmediate(m_exitBtn.gameObject);
			NBUILocalize component = m_resumeBtnText.GetComponent<NBUILocalize>();
			if (null != component)
			{
				component.Key = "common_ok";
			}
			m_resumeBtn.transform.localPosition = new Vector3(0f, m_resumeBtn.transform.localPosition.y, m_resumeBtn.transform.localPosition.z);
		}
	}

	private void OnEnable()
	{
		if (m_defaultShowingTab == PauseTab.COMBOS)
		{
			m_comboTabBtn.m_Tabbed = true;
			m_settingsTabBtn.m_Tabbed = false;
		}
		else if (m_defaultShowingTab == PauseTab.SETTINGS)
		{
			m_comboTabBtn.m_Tabbed = false;
			m_settingsTabBtn.m_Tabbed = true;
		}
	}

	public void SetTitle(string p_text)
	{
		m_title.text = p_text;
	}

	private void Update()
	{
	}

	public void OnSettingsTabBtnClick()
	{
		if (m_settingsTabBtn != null && m_comboTabBtn != null)
		{
			if (m_settingPage == null)
			{
				GameObject gameObject = Object.Instantiate(m_settingPageTemplate, Vector3.zero, Quaternion.identity) as GameObject;
				if (gameObject != null)
				{
					m_settingPage = gameObject.GetComponent<BattlePauseSettingsPage>();
					if (m_settingPage != null)
					{
						m_settingPage.transform.parent = base.transform;
						m_settingPage.transform.localPosition = Vector3.zero;
						m_settingPage.transform.localScale = Vector3.one;
						m_settingPage.Show();
						if (m_comboHintPage != null)
						{
							m_comboHintPage.Hide();
						}
					}
					else
					{
						Dbg.LogWarnEx("Unable to get component from game obj: {0}", Dbg.NBLogType.COMMON_WARNING, gameObject.name);
					}
				}
				else
				{
					Dbg.LogWarn("Unable to create game object");
				}
			}
			else if (m_comboHintPage != null)
			{
				m_comboHintPage.Hide();
				m_settingPage.Show();
			}
			m_settingsTabBtn.m_Tabbed = true;
			m_comboTabBtn.m_Tabbed = false;
		}
		else
		{
			Dbg.LogWarn("m_settingsTabBtn or m_comboTabBtn is null");
		}
	}

	public void OnComboHintTabBtnClick()
	{
		if (m_settingsTabBtn != null && m_comboTabBtn != null)
		{
			if (m_comboHintPage == null)
			{
				GameObject gameObject = Object.Instantiate(m_comboHintPageTemplate, Vector3.zero, Quaternion.identity) as GameObject;
				if (gameObject != null)
				{
					m_comboHintPage = gameObject.GetComponent<BattlePauseComboHintPage>();
					if (m_comboHintPage != null)
					{
						m_comboHintPage.transform.parent = base.transform;
						m_comboHintPage.transform.localPosition = Vector3.zero;
						m_comboHintPage.transform.localScale = Vector3.one;
						m_comboHintPage.Show();
						if (m_settingPage != null)
						{
							m_settingPage.Hide();
						}
					}
					else
					{
						Dbg.LogWarnEx("Unable to get component from game obj: {0}", Dbg.NBLogType.COMMON_WARNING, gameObject.name);
					}
				}
				else
				{
					Dbg.LogWarn("Unable to create game object");
				}
			}
			else if (m_settingPage != null)
			{
				m_settingPage.Hide();
				m_comboHintPage.Show();
			}
			m_settingsTabBtn.m_Tabbed = false;
			m_comboTabBtn.m_Tabbed = true;
		}
		else
		{
			Dbg.LogWarn("m_settingsTabBtn or m_comboTabBtn is null");
		}
	}

	public void SetButtonCallbackTarget()
	{
		UIButtonMessage component = m_settingsTabBtn.GetComponent<UIButtonMessage>();
		component.target = base.gameObject;
		component.functionName = "OnSettingsTabBtnClick";
		UIButtonMessage component2 = m_comboTabBtn.GetComponent<UIButtonMessage>();
		component2.target = base.gameObject;
		component2.functionName = "OnComboHintTabBtnClick";
	}
}
