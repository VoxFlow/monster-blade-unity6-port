using System;
using UnityEngine;

public class NubeePlatformBinding
{
	private static AndroidJavaObject _pluginObject;

	private static string m_szGooglePaymentManager;

	static NubeePlatformBinding()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		m_szGooglePaymentManager = "com/nubee/platform/payment/android/v3/GooglePaymentManager";
		Debug.Log("NubeePlatformBinding");
		using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.nubee.monsterslayer.MonsterSlayerSocialPlugin"))
		{
			_pluginObject = androidJavaClass.CallStatic<AndroidJavaObject>("Instance", new object[0]);
			_pluginObject.Call("CreateNBConnectInstance");
			_pluginObject.Call("CreateBurstlyManager");
			_pluginObject.Call("CreateLocalNotificationManager");
			_pluginObject.Call("CreateGooglePaymentManager");
		}
#else
		m_szGooglePaymentManager = "com/nubee/platform/payment/android/v3/GooglePaymentManager";
		Debug.Log("NubeePlatformBinding");
#endif
	}

	public static void TryConnect(string appId, string hashSalt)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		Debug.Log("NubeePlatformBinding.TryConnect. appId: " + appId + ". hashSalt: " + hashSalt);
		_pluginObject.Call("TryConnect", appId, hashSalt);
#else
		if (OfflineServerMode.Enabled)
		{
			OfflineServerMode.ConnectNubeePlatform();
		}
#endif
	}

	public static string GetNubeeID()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		return _pluginObject.Call<string>("GetNubeeID", new object[0]);
#else
		return OfflineServerMode.Enabled ? OfflineServerMode.NubeeId : null;
#endif
	}

	public static string GetDeviceInfo()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		return _pluginObject.Call<string>("GetDeviceInfo", new object[0]);
#else
		return OfflineServerMode.Enabled ? OfflineServerMode.DeviceId : null;
#endif
	}

	public static void OpenDashboard()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		_pluginObject.Call("OpenDashboard");
#else

#endif
	}

	public static void CloseDashboard()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		_pluginObject.Call("CloseDashboard");
#else

#endif
	}

	public static string GetPersonString()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		return _pluginObject.Call<string>("GetPersonString", new object[0]);
#else
		return null;
#endif
	}

	public static string GetPersonStringSignature()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		return _pluginObject.Call<string>("GetPersonStringSignature", new object[0]);
#else
		return null;
#endif
	}

	public static void InitFacebook(string p_facebookAppID, string p_facebookAppSecret)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		_pluginObject.Call("CreateFacebookInstance", p_facebookAppID, p_facebookAppSecret, string.Empty);
#else

#endif
	}

	public static void LoginFacebook(bool p_bAllowUI)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		Debug.Log("LoginFacebook");
		_pluginObject.Call("LoginFacebook");
#else

#endif
	}

	public static bool IsLoggedInToFacebook()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		bool flag = _pluginObject.Call<bool>("IsLoggedInToFacebook", new object[0]);
		Debug.Log("IsLoggedInToFacebook: " + flag);
		return flag;
#else
		return false;
#endif
	}

	public static void LogoutFacebook()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		Debug.Log("LogoutFacebook");
		_pluginObject.Call("LogoutFacebook");
#else

#endif
	}

	public static void GetUserIdOnFacebook()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		Debug.Log("GetUserIdOnFacebook");
		_pluginObject.Call("GetUserIdOnFacebook");
#else

#endif
	}

	public static void TryGetFacebookFriendsWithAccessToken()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		Debug.Log("LoginFacebook");
		_pluginObject.Call("TryGetFacebookFriendsWithAccessToken");
#else

#endif
	}

	public static void TryGetFacebookFriendsWithoutAccessToken()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		Debug.Log("TryGetFacebookFriendsWithoutAccessToken");
		_pluginObject.Call("TryGetFacebookFriendsWithoutAccessToken");
#else

#endif
	}

	public static string GetFacebookFriendInfo(int index)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		Debug.Log("GetFacebookFriendInfo. index: " + index);
		return _pluginObject.Call<string>("GetFacebookFriendInfo", new object[1] { index });
#else
		return null;
#endif
	}

	public static string GetFacebookToken()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		Debug.Log("GetFacebookToken");
		return _pluginObject.Call<string>("GetFacebookToken", new object[0]);
#else
		return null;
#endif
	}

	public static void LoginRenren()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		_pluginObject.Call("LoginRenren");
#else

#endif
	}

	public static bool IsLoggedInToRenren()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		return _pluginObject.Call<bool>("IsLoggedInToRenren", new object[0]);
#else
		return false;
#endif
	}

	public static long GetRenrenLoggedInUserId()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		return _pluginObject.Call<long>("GetRenrenLoggedInUserId", new object[0]);
#else
		return 0;
#endif
	}

	public static string GetRenrenLoggedInUserPictureUrl()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		return _pluginObject.Call<string>("GetRenrenLoggedInUserPicture", new object[0]);
#else
		return null;
#endif
	}

	public static void GetOtherUserPictureUrl(string renrenId)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		_pluginObject.Call("GetOtherUserPicture", renrenId);
#else

#endif
	}

	public static void LoginTwitter()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		_pluginObject.Call("LoginTwitter");
#else

#endif
	}

	public static bool IsLoggedInToTwitter()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		return _pluginObject.Call<bool>("IsLoggedInToTwitter", new object[0]);
#else
		return false;
#endif
	}

	public static void LogoutTwitter()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		_pluginObject.Call("LogoutTwitter");
#else

#endif
	}

	public static void GetUserIdOnTwitter()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		_pluginObject.Call("GetUserIdOnTwitter");
#else

#endif
	}

	public static void TryPostTwitter(string message, byte[] imageBytes = null)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		string text = null;
		if (imageBytes != null)
		{
			text = Convert.ToBase64String(imageBytes);
		}
		_pluginObject.Call("TryPostTwitterMessage", message, text);
#else

#endif
	}

	public static void LoginWeibo()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		_pluginObject.Call("LoginWeibo");
#else

#endif
	}

	public static bool IsLoggedInToWeibo()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		return _pluginObject.Call<bool>("IsLoggedInToWeibo", new object[0]);
#else
		return false;
#endif
	}

	public static void LogoutWeibo()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		_pluginObject.Call("LogoutWeibo");
#else

#endif
	}

	public static void TryPostWeibo(string message, byte[] imageBytes = null)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		string text = null;
		if (imageBytes != null)
		{
			text = Convert.ToBase64String(imageBytes);
		}
		_pluginObject.Call("TryPostWeiboMessage", message, text);
#else

#endif
	}

	public static void PushNotification(int timeLeft, string message)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		_pluginObject.Call("PushNotification", timeLeft, message);
#else

#endif
	}

	public static void ClearAllNotification()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		_pluginObject.Call("ClearAllNotification");
#else

#endif
	}

	public static void AnalyticsSetNubeeID(uint nubeeID)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		_pluginObject.Call("AnalyticsSetNubeeID", nubeeID.ToString());
#else

#endif
	}

	public static void AnalyticsRegisterEventWithAttributes(string eventName, string json)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		_pluginObject.Call("AnalyticsRegisterEventWithAttributes", eventName, json);
#else

#endif
	}

	public static bool IsPaymentSupported()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		return _pluginObject.Call<bool>("IsPaymentSupported", new object[0]);
#else
		return false;
#endif
	}

	public static bool RequestPurchase(string productId)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		return _pluginObject.Call<bool>("RequestPurchase", new object[1] { productId });
#else
		return false;
#endif
	}

	public static bool RequestPurchaseV3(string productId)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		string text = "inapp";
		string text2 = "developer_payload";
		jvalue[] args = AndroidJNIHelper.CreateJNIArgArray(new object[0]);
		IntPtr clazz = AndroidJNI.FindClass(m_szGooglePaymentManager);
		IntPtr staticMethodID = AndroidJNI.GetStaticMethodID(clazz, "getInstance", "()Lcom/nubee/platform/payment/android/v3/GooglePaymentManager;");
		IntPtr obj = AndroidJNI.CallStaticObjectMethod(clazz, staticMethodID, args);
		IntPtr methodID = AndroidJNI.GetMethodID(clazz, "requestPurchase", "(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V");
		jvalue[] args2 = AndroidJNIHelper.CreateJNIArgArray(new string[3] { productId, text, text2 });
		AndroidJNI.CallVoidMethod(obj, methodID, args2);
		return true;
#else
		return false;
#endif
	}

	public static void CheckPendingPurchaseV3()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		jvalue[] args = AndroidJNIHelper.CreateJNIArgArray(new object[0]);
		IntPtr clazz = AndroidJNI.FindClass(m_szGooglePaymentManager);
		IntPtr staticMethodID = AndroidJNI.GetStaticMethodID(clazz, "getInstance", "()Lcom/nubee/platform/payment/android/v3/GooglePaymentManager;");
		IntPtr obj = AndroidJNI.CallStaticObjectMethod(clazz, staticMethodID, args);
		IntPtr methodID = AndroidJNI.GetMethodID(clazz, "checkPendingPurchases", "()V");
		AndroidJNI.CallVoidMethod(obj, methodID, args);
#else

#endif
	}

	public static void ConfirmPurchaseV3(string purchaseItem)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		jvalue[] args = AndroidJNIHelper.CreateJNIArgArray(new object[0]);
		IntPtr clazz = AndroidJNI.FindClass(m_szGooglePaymentManager);
		IntPtr staticMethodID = AndroidJNI.GetStaticMethodID(clazz, "getInstance", "()Lcom/nubee/platform/payment/android/v3/GooglePaymentManager;");
		IntPtr obj = AndroidJNI.CallStaticObjectMethod(clazz, staticMethodID, args);
		IntPtr methodID = AndroidJNI.GetMethodID(clazz, "confirmPurchase", "(Ljava/lang/String;)V");
		if (purchaseItem != string.Empty)
		{
			jvalue[] args2 = AndroidJNIHelper.CreateJNIArgArray(new string[1] { purchaseItem });
			AndroidJNI.CallVoidMethod(obj, methodID, args2);
		}
		else
		{
			Debug.Log("Nothing to confirm");
		}
#else

#endif
	}

	public static void ConfirmPurchase(string[] notifications)
	{
	}

	public static void ConfirmPurchaseByJson(string json)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		_pluginObject.Call("ConfirmPurchaseByJson", json);
#else

#endif
	}

	public static void ShowBurstlyBanner(bool show)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		_pluginObject.Call("ShowBurstlyBanner", show);
#else

#endif
	}

	public static void ShowBurstlyInterstitial()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		_pluginObject.Call("ShowBurstlyInterstitial");
#else

#endif
	}

	public static void ChatInitRoom(string roomId)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		_pluginObject.Call("ChatInitRoom", roomId);
#else

#endif
	}

	public static void ChatSendMessage(string message)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		_pluginObject.Call("ChatSendMessage", message);
#else

#endif
	}

	public static void ShowWriteReviewAlert(string title, string message, string okButtonName, string cancelButtonName)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		_pluginObject.Call("ShowWriteReviewAlert", title, message, okButtonName, cancelButtonName);
#else

#endif
	}
}
