using UnityEngine;

public class BackButtonMapping : MonoBehaviour
{
	public MainMenu m_mainMenuObject;

	public GameObject[] m_backButtons;

	public InventoryPanel m_inventoryPanel;

	private UIPopupController m_popupController;

	private UICamera m_popupCamera;

	private TutorialManager m_tutorialManager;

	private bool m_quitPrompt;

	public bool alwaysShowExitConfirmation;

	private void Awake()
	{
		m_tutorialManager = SingletonComponent<TutorialManager>.Instance;
		m_popupController = UIPopupController.Instance;
		if (null != m_popupController)
		{
			Camera popupCamera = m_popupController.GetPopupCamera();
			m_popupCamera = popupCamera.GetComponent<UICamera>();
		}
	}

	private void Update()
	{
		if ((!IsBackButtonUsable() && !alwaysShowExitConfirmation) || !Input.GetKeyDown(KeyCode.Escape))
		{
			return;
		}
		SubtitlePopup subtitlePopup = Object.FindObjectOfType(typeof(SubtitlePopup)) as SubtitlePopup;
		GameObject activeButton = GetActiveButton();
		bool flag = m_tutorialManager != null && m_tutorialManager.enabled && m_tutorialManager.IsAnyTutorialExisting();
		LevelUpWindow levelUpWindow = Object.FindObjectOfType(typeof(LevelUpWindow)) as LevelUpWindow;
		DungeonCompletePanel dungeonCompletePanel = Object.FindObjectOfType(typeof(DungeonCompletePanel)) as DungeonCompletePanel;
		AnnouncementPanel announcementPanel = Object.FindObjectOfType(typeof(AnnouncementPanel)) as AnnouncementPanel;
		GameObject gameObject = GameObject.FindWithTag("Inventory");
		if (m_popupController != null && m_popupController.currentPopup != null)
		{
			if (m_quitPrompt)
			{
				m_popupController.SendMessage("OnYes", SendMessageOptions.DontRequireReceiver);
			}
			else if (!flag)
			{
				m_popupController.SendMessage("OnDeviceBackButtonPressed", SendMessageOptions.DontRequireReceiver);
			}
		}
		else if (null != gameObject)
		{
			gameObject.SendMessage("OnClose", SendMessageOptions.DontRequireReceiver);
		}
		else if (null != subtitlePopup && subtitlePopup.gameObject.active)
		{
			subtitlePopup.gameObject.SendMessage("OnRelease", SendMessageOptions.DontRequireReceiver);
		}
		else if (null != announcementPanel && announcementPanel.gameObject.active)
		{
			announcementPanel.gameObject.SendMessage("OnCloseButtonClick", SendMessageOptions.DontRequireReceiver);
		}
		else if (levelUpWindow != null && levelUpWindow.gameObject.active)
		{
			levelUpWindow.gameObject.SendMessage("ResumeAnimation", SendMessageOptions.DontRequireReceiver);
		}
		else if (dungeonCompletePanel != null && dungeonCompletePanel.gameObject.active)
		{
			dungeonCompletePanel.gameObject.SendMessage("TapToContinue", SendMessageOptions.DontRequireReceiver);
		}
		else if (!flag && m_inventoryPanel != null && m_inventoryPanel.gameObject.active)
		{
			if (m_inventoryPanel.isReady)
			{
				m_inventoryPanel.SendMessage("OnButtonClick", m_inventoryPanel.buttonCancel.gameObject, SendMessageOptions.DontRequireReceiver);
			}
		}
		else if (activeButton != null && !flag)
		{
			activeButton.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
		}
		else if (NeedToPromptQuitting())
		{
			ShowExitConfirmationPopup();
		}
		else if (m_mainMenuObject != null && m_mainMenuObject.currScreenIndex != 0 && m_mainMenuObject.IsHomeReady)
		{
			m_mainMenuObject.SendMessage("OnButtonClick", m_mainMenuObject.bottomBar.m_buttons[0].gameObject, SendMessageOptions.DontRequireReceiver);
		}
	}

	private void ShowExitConfirmationPopup()
	{
		m_popupController.ShowExitConfirmationPopup(OnExitCanceled, OnExitConfirmed);
		m_quitPrompt = true;
		Time.timeScale = 0f;
	}

	private void OnExitConfirmed()
	{
		Dbg.Log("Exiting lah!");
		Application.Quit();
	}

	private void OnExitCanceled()
	{
		Time.timeScale = 1f;
		m_quitPrompt = false;
	}

	private GameObject GetActiveButton()
	{
		GameObject result = null;
		if (m_backButtons != null)
		{
			GameObject[] backButtons = m_backButtons;
			foreach (GameObject gameObject in backButtons)
			{
				if (gameObject.active)
				{
					result = gameObject;
					break;
				}
			}
		}
		return result;
	}

	public bool NeedToPromptQuitting()
	{
		bool result = alwaysShowExitConfirmation;
		if (m_popupController != null)
		{
			if (m_tutorialManager != null && m_tutorialManager.enabled && m_tutorialManager.IsAnyTutorialExisting())
			{
				result = true;
			}
			else if (m_mainMenuObject != null && m_mainMenuObject.currScreenIndex == 0)
			{
				result = true;
			}
		}
		return result;
	}

	public bool IsBackButtonUsable()
	{
		bool result = true;
		bool flag = (m_popupController != null && m_popupController.isLoading) || m_mainMenuObject.IsLoadingNextScreen();
		bool flag2 = m_popupCamera != null && m_popupCamera.enabled && m_popupCamera.useTouch;
		if (flag || !flag2)
		{
			result = false;
		}
		else if ((!(m_tutorialManager != null) || !m_tutorialManager.enabled) && UICamera.currentFilter != null)
		{
			result = false;
		}
		return result;
	}
}
