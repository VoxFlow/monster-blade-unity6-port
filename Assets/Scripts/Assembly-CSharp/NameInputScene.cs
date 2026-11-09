using System.Collections;
using System.Collections.Generic;
using ServiceCalls;
using UnityEngine;

public class NameInputScene : MonoBehaviour
{
	public UIInput m_nameInput;

	public UIButton m_connectFbButton;

	public UIButton m_connectWbButton;

	public UIButton m_missoutButton;

	private bool m_exited;

	private bool m_alreadyLogin;

	private void OnEnable()
	{
		List<UICamera> allUICameras = UICamera.allUICameras;
		foreach (UICamera item in allUICameras)
		{
			item.useKeyboard = true;
		}
	}

	private void OnDisable()
	{
		List<UICamera> allUICameras = UICamera.allUICameras;
		foreach (UICamera item in allUICameras)
		{
			item.useKeyboard = false;
		}
		if (m_alreadyLogin)
		{
			StopCoroutine("ExitIfLogin");
		}
	}

	private void Awake()
	{
		ShowCorrectSNSButton();
		Dbg.Assert(m_connectFbButton);
		Dbg.Assert(m_connectWbButton);
		Dbg.Assert(m_missoutButton);
	}

	private void ShowCorrectSNSButton()
	{
		if (!(null == m_connectFbButton) && !(null == m_connectWbButton))
		{
			m_connectWbButton.gameObject.SetActiveRecursively(false);
			m_connectFbButton.gameObject.SetActiveRecursively(true);
		}
	}

	private void Start()
	{
		if (!(null == SingletonComponent<SaveData>.Instance))
		{
			m_alreadyLogin = SingletonComponent<SaveData>.Instance.GameData.IsNameGiven();
			if (m_alreadyLogin)
			{
				StartCoroutine("ExitIfLogin");
			}
		}
	}

	private IEnumerator ExitIfLogin()
	{
		while (SingletonComponent<GameManager>.Instance.IsLoadingScene())
		{
			yield return null;
		}
		AnalyticsManager.FirstInstallEvents("tut_already_connectedtofacebook");
		Exit();
	}

	private void OnButtonClick(GameObject p_source)
	{
		if (!m_alreadyLogin)
		{
			if (null != m_connectFbButton && p_source == m_connectFbButton.gameObject)
			{
				Dbg.Log("Login with Facebook");
				OnFBClick();
			}
			else if (null != m_missoutButton && p_source == m_missoutButton.gameObject)
			{
				Dbg.Log("Missout");
				OnMissoutClick();
			}
		}
	}

	private void OnMissoutClick()
	{
		if (!m_exited && !m_alreadyLogin)
		{
			AnalyticsManager.FirstInstallEvents("tut_missout_pressed");
			OnUpdateNubeeAccount();
		}
	}

	private void OnFBClick()
	{
		if (!m_exited && !m_alreadyLogin)
		{
			AnalyticsManager.FirstInstallEvents("tut_connecttofacebook_pressed");
			if (null != SingletonComponent<SocialManager>.Instance)
			{
				SingletonComponent<SocialManager>.Instance.InviteFriend(null, OnFBLoginCallback, OnFBInviteFriendCallback);
			}
		}
	}

	private void OnFBLoginCallback(ServiceResponseCodes p_code)
	{
		switch (p_code)
		{
		case ServiceResponseCodes.SUCCESS:
			if ((bool)UIPopupController.Instance)
			{
				UIPopupController.Instance.ShowLoadingPopupNetWait();
			}
			break;
		case ServiceResponseCodes.FAILED:
			if ((bool)UIPopupController.Instance)
			{
				UIPopupController.Instance.HideLoadingPopup();
			}
			break;
		}
	}

	private void OnFBInviteFriendCallback(ServiceResponseCodes p_code)
	{
		if ((bool)UIPopupController.Instance)
		{
			UIPopupController.Instance.HideLoadingPopup();
		}
		OnUpdateNubeeAccount();
	}

	private void ValidateCharacters()
	{
		string text = m_nameInput.text;
		foreach (char c in text)
		{
			int num = c;
			if (num < 32 || num > 126)
			{
				if ((bool)UIPopupController.Instance)
				{
					UIPopupController.Instance.ShowOKPopupMSG("common_info", "name_input_english_only");
				}
				m_nameInput.text = string.Empty;
				break;
			}
		}
	}

	private void OnUpdateNubeeAccount()
	{
		if (null != UIPopupController.Instance)
		{
			UIPopupController.Instance.ShowLoadingPopupNetWait();
		}
		Services instance = SingletonComponent<Services>.Instance;
		SaveData instance2 = SingletonComponent<SaveData>.Instance;
		if (null != instance && null != instance2)
		{
			instance.CreateOrUpdateNubeeAccount(instance.NubeeId, instance.DeviceId, instance2.GameData.CharacterName, NBUnity.ExpandedToIso2(instance2.GameData.Language), instance2.GameData.FBID, instance2.GameData.FBToken, string.Empty, true, OnUpdateNubeeAccountSuccess, OnUpdateNubeeAccountFail);
		}
	}

	private void OnUpdateNubeeAccountSuccess(Services.ServiceHandle p_handle)
	{
		SingletonComponent<SaveData>.Instance.GameData.NeedUpdateFBID = false;
		SingletonComponent<SaveData>.Instance.GameData.NeedUpdateFBToken = false;
		SingletonComponent<SaveData>.Instance.GameData.NeedUpdateFBName = false;
		SingletonComponent<SaveData>.Instance.SaveDataToPlayerPrefs();
		if (SingletonComponent<SaveData>.Instance.GameData.FBID != string.Empty)
		{
			AnalyticsManager.FirstInstallEvents("tut_connecttofacebook_success");
		}
		else
		{
			AnalyticsManager.FirstInstallEvents("tut_missout_success");
		}
		if (null != SingletonComponent<PlayerDataManager>.Instance)
		{
			SingletonComponent<PlayerDataManager>.Instance.GetPlayerData().Name = SingletonComponent<SaveData>.Instance.GameData.CharacterName;
		}
		if (null != MainMenu.Instance)
		{
			MainMenu.Instance.Refresh();
		}
		Exit();
	}

	private void OnUpdateNubeeAccountFail(Services.ServiceHandle p_handle, string p_errorMessage)
	{
		UIPopupController.Instance.HideLoadingPopup();
		if (p_handle == null)
		{
			UIPopupController.Instance.ShowServerGenericError(OnUpdateNubeeAccount);
			return;
		}
		Dbg.LogWarn("Register for Nubee Account fail: " + ServiceCallUtils.GetErrorMessage(p_handle), Dbg.NBLogType.SYSTEM_INIT);
		if (p_handle.IsNoInternet())
		{
			UIPopupController.Instance.ShowNoNetMessage(OnUpdateNubeeAccount);
		}
		else
		{
			UIPopupController.Instance.ShowServerGenericError(OnUpdateNubeeAccount, p_handle.nReturnCode);
		}
	}

	private void Exit()
	{
		if (!m_exited)
		{
			m_exited = true;
			if (UIPopupController.Instance != null)
			{
				UIPopupController.Instance.ShowFullScreenLoading(UIPopupController.LoadingScreenMode.FULLSCREEN_MONSTER_BLADE);
			}
			AnalyticsManager.FirstInstallEvents("tut_lastloading");
			SingletonComponent<GameManager>.Instance.LoadScene("MainMenuInit");
		}
	}
}
