using UnityEngine;

public class BattleScreenBackButton : MonoBehaviour
{
	private BattleScreenUI m_battleUI;

	private UIPopupController m_popupController;

	private bool m_quitPopupPrompted;

	private float m_prevTimeScale = 1f;

	private void Awake()
	{
		m_battleUI = base.gameObject.GetComponent<BattleScreenUI>();
		m_popupController = UIPopupController.Instance;
	}

	private void Update()
	{
		if (!Input.GetKeyDown(KeyCode.Escape) || m_battleUI.OnBackButtonPressed())
		{
			return;
		}
		SubtitlePopup subtitlePopup = Object.FindObjectOfType(typeof(SubtitlePopup)) as SubtitlePopup;
		if (null != subtitlePopup && subtitlePopup.gameObject.active)
		{
			subtitlePopup.gameObject.SendMessage("OnRelease", SendMessageOptions.DontRequireReceiver);
		}
		else
		{
			if (!(m_popupController != null))
			{
				return;
			}
			BattleCompleteScreen[] componentsInChildren = MBHier.GetComponentsInChildren<BattleCompleteScreen>(m_popupController, true);
			BattleCompleteScreen battleCompleteScreen = componentsInChildren[0];
			if (m_quitPopupPrompted)
			{
				UITweener[] componentsInChildren2 = MBHier.GetComponentsInChildren<UITweener>(m_popupController.gameObject);
				bool flag = false;
				for (int i = 0; i < componentsInChildren2.Length; i++)
				{
					flag |= componentsInChildren2[i].enabled;
				}
				if (!flag)
				{
					m_popupController.SendMessage("OnYes", SendMessageOptions.DontRequireReceiver);
				}
				return;
			}
			bool flag2 = SingletonComponent<TutorialManager>.Instance != null && SingletonComponent<TutorialManager>.Instance.IsAnyTutorialExisting();
			if (battleCompleteScreen != null && battleCompleteScreen.gameObject.active)
			{
				if (battleCompleteScreen.m_confirmExitPopupGO != null && battleCompleteScreen.m_confirmExitPopupGO.active)
				{
					battleCompleteScreen.SendMessage("OnYesBtnPressed", SendMessageOptions.DontRequireReceiver);
				}
				else
				{
					if (!(battleCompleteScreen.m_leaveBtn != null) || !battleCompleteScreen.m_leaveBtn.gameObject.active)
					{
						return;
					}
					UIButton component = battleCompleteScreen.m_leaveBtn.GetComponent<UIButton>();
					UIButton component2 = battleCompleteScreen.m_continueBtn.GetComponent<UIButton>();
					UISprite componentInChildren = MBHier.GetComponentInChildren<UISprite>(component);
					bool flag3 = null != component && null != component.GetComponent<Collider>() && component.GetComponent<Collider>().enabled && !component.isLocked && componentInChildren.enabled;
					bool flag4 = null != component2 && null != component2.GetComponent<Collider>() && component2.GetComponent<Collider>().enabled;
					if (flag3)
					{
						battleCompleteScreen.SendMessage("OnLeaveDungeonBtnClick", SendMessageOptions.DontRequireReceiver);
					}
					else if (flag4)
					{
						UITweener[] componentsInChildren3 = MBHier.GetComponentsInChildren<UITweener>(m_popupController.gameObject);
						bool flag5 = false;
						for (int j = 0; j < componentsInChildren3.Length; j++)
						{
							flag5 |= componentsInChildren3[j].enabled;
						}
						if (!flag5)
						{
							m_popupController.ShowExitConfirmationPopup(OnExitCanceled, OnExitConfirmed);
							m_quitPopupPrompted = true;
							Time.timeScale = 1f;
						}
					}
				}
			}
			else if (m_popupController.currentPopup != null && !flag2)
			{
				m_popupController.SendMessage("OnDeviceBackButtonPressed", SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	private void OnExitConfirmed()
	{
		Dbg.Log("Exiting lah!");
		Application.Quit();
	}

	private void OnExitCanceled()
	{
		m_quitPopupPrompted = false;
		Time.timeScale = 1f;
	}
}
