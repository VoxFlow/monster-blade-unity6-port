using UnityEngine;

public class TitleScreenBackButtonMapping : MonoBehaviour
{
	private UIPopupController m_popupController;

	private bool m_quitPrompt;

	private void Start()
	{
		m_popupController = UIPopupController.Instance;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape) && m_popupController != null && m_popupController.currentPopup != null)
		{
			if (m_quitPrompt)
			{
				m_popupController.SendMessage("OnYes", SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				m_popupController.SendMessage("OnDeviceBackButtonPressed", SendMessageOptions.DontRequireReceiver);
			}
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
}
