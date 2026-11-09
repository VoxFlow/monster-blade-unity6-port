using System;
using UnityEngine;

public class NBUnityManager : MonoBehaviour
{
	public static event Action<string> registerRemoteNotificationSuccessCb;

	public static event Action<string> registerRemoteNotificationFailCb;

	public static event Action<string> notificationReceivedCb;

	static NBUnityManager()
	{
		NBUnityManager.registerRemoteNotificationSuccessCb = delegate
		{
		};
		NBUnityManager.registerRemoteNotificationFailCb = delegate
		{
		};
		NBUnityManager.notificationReceivedCb = delegate
		{
		};
	}

	public void OnRegisterRemoteNotificationSuccess(string p_token)
	{
		NBUnityManager.registerRemoteNotificationSuccessCb(p_token);
	}

	public void OnRegisterRemoteNotificationFail(string p_error)
	{
		NBUnityManager.registerRemoteNotificationFailCb(p_error);
	}

	public void OnPushNotificationReceived(string p_json)
	{
		NBUnityManager.notificationReceivedCb(p_json);
	}
}
