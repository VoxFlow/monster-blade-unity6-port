using Models;
using UnityEngine;

public class ServerCheck : MonoBehaviour
{
	private VoidCallback m_callback;

	public void StartupCheck(VoidCallback p_callback)
	{
		m_callback = p_callback;
		Services instance = SingletonComponent<Services>.Instance;
		if (instance == null)
		{
			Dbg.LogError("Cannot access Service class. We cannot start the game.");
		}
		else
		{
			instance.StartupCheck(OnStartupCheckSucess, OnStartupCheckFail);
		}
	}

	private void OnStartupCheckSucess(Services.ServiceHandle p_handle)
	{
		Models.ServerCheck serverCheck = (Models.ServerCheck)p_handle.returnValue;
		if (serverCheck == null)
		{
			Dbg.LogError("ServerCheck is null! Retry.");
			UIPopupController.Instance.ShowServerGenericError(RetryStartupCheck);
			return;
		}
		if (serverCheck.maintenance)
		{
			if (null != UIPopupController.Instance)
			{
				UIPopupController.Instance.ShowOKPopupMSG("common_info", "server_in_maintenance", CloseTheApplication, "common_quit_button");
				return;
			}
		}
		else if (string.Compare(serverCheck.minimum_version, GLOBAL.CLIENT_VERSION) > 0 && null != UIPopupController.Instance)
		{
			UIPopupController.Instance.ShowOKPopupMSG("common_error", "error_force_update", GoToAppStore);
			return;
		}
		if (m_callback != null)
		{
			m_callback();
		}
	}

	private void OnStartupCheckFail(Services.ServiceHandle p_handle, string p_error)
	{
		Dbg.LogError("StartupCheck Failed : " + p_error);
		if (p_handle == null)
		{
			UIPopupController.Instance.ShowServerGenericError(RetryStartupCheck);
		}
		else if (p_handle.IsNoInternet())
		{
			UIPopupController.Instance.ShowNoNetMessage(RetryStartupCheck);
		}
		else
		{
			UIPopupController.Instance.ShowServerGenericError(RetryStartupCheck, p_handle.nReturnCode);
		}
	}

	private void RetryStartupCheck()
	{
		StartupCheck(m_callback);
	}

	private void CloseTheApplication()
	{
		Dbg.Log("CloseTheApplication");
		Application.Quit();
	}

	private void GoToAppStore()
	{
		Dbg.Log("GoToAppStore");
		Application.OpenURL(GLOBAL.APP_STORE_LINK);
		CloseTheApplication();
	}
}
