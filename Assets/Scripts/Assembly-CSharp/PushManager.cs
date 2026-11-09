using System.Collections;
using ServiceCalls;

public class PushManager : SingletonComponent<PushManager>
{
	public enum RegistrationState
	{
		None = 0,
		Registering = 1,
		Registered = 2,
		Max = 3
	}

	public delegate void AsyncCallback(bool p_succeed);

	public delegate void PushReceivedCallback(Hashtable p_jsonHash);

	private bool m_enabled = true;

	private RegistrationState m_registerState;

	private string m_deviceToken = string.Empty;

	private AsyncCallback m_registerCb;

	public string DeviceToken
	{
		get
		{
			return m_deviceToken;
		}
	}

	public void InitManager(AsyncCallback p_callback)
	{
		NBProfiler.BeginTask("[PUSH_MANAGER][INIT_MANAGER]");
		m_registerCb = p_callback;
		m_registerState = RegistrationState.None;
		NBUnityManager.registerRemoteNotificationSuccessCb += OnRegisterRemoteNotificationSuccess;
		NBUnityManager.registerRemoteNotificationFailCb += OnRegisterRemoteNotificationFail;
		NBUnityManager.notificationReceivedCb += OnPushNotificationReceived;
		RegisterPushNotification();
	}

	private void SendDeviceTokenToServer()
	{
		if (!m_enabled)
		{
			return;
		}
		if (m_deviceToken == string.Empty)
		{
			if (m_registerCb != null)
			{
				m_registerCb(false);
				m_registerCb = null;
			}
			NBProfiler.EndTask("[PUSH_MANAGER][INIT_MANAGER]");
		}
		else
		{
			string p_lang = NBUnity.ExpandedToIso2(SingletonComponent<SaveData>.Instance.GameData.Language);
			SingletonComponent<Services>.Instance.SendDeviceTokenToServer(SingletonComponent<Services>.Instance.NubeeId, DeviceToken, p_lang, OnSendDeviceTokenToServerSuccess, OnSendDeviceTokenToServerFail);
		}
	}

	private void RegisterPushNotification()
	{
		if (m_registerState == RegistrationState.None)
		{
			m_registerState = RegistrationState.Registering;
			if (m_registerCb != null)
			{
				m_registerCb(true);
				m_registerCb = null;
			}
			NBProfiler.EndTask("[PUSH_MANAGER][INIT_MANAGER]");
		}
	}

	private void OnSendDeviceTokenToServerSuccess(Services.ServiceHandle p_handle)
	{
		Dbg.Log("Successfully sent the device token to the server.");
		m_registerState = RegistrationState.Registered;
		if (m_registerCb != null)
		{
			m_registerCb(true);
			m_registerCb = null;
		}
		NBProfiler.EndTask("[PUSH_MANAGER][INIT_MANAGER]");
	}

	private void OnSendDeviceTokenToServerFail(Services.ServiceHandle p_handle, string p_error)
	{
		m_registerState = RegistrationState.None;
		Dbg.LogWarn("Cannot register for push notifications.: " + ServiceCallUtils.GetErrorMessage(p_handle));
		m_enabled = false;
		base.enabled = false;
		if (m_registerCb != null)
		{
			m_registerCb(false);
			m_registerCb = null;
		}
	}

	private void OnRegisterRemoteNotificationSuccess(string p_deviceToken)
	{
		Dbg.Log("Remote notification registration successful! Device token is " + p_deviceToken);
		m_deviceToken = p_deviceToken;
		m_registerState = RegistrationState.Registered;
		if (p_deviceToken != "NoDeviceToken")
		{
			SendDeviceTokenToServer();
		}
		else if (m_registerCb != null)
		{
			m_registerCb(true);
			m_registerCb = null;
		}
	}

	private void OnRegisterRemoteNotificationFail(string p_error)
	{
		m_registerState = RegistrationState.None;
		Dbg.LogError("Cannot register for push notifications.");
		m_enabled = false;
		base.enabled = false;
		if (m_registerCb != null)
		{
			m_registerCb(false);
			m_registerCb = null;
		}
	}

	private void OnPushNotificationReceived(string p_message)
	{
		Dbg.LogWarn("Push Notification received: " + p_message);
	}
}
