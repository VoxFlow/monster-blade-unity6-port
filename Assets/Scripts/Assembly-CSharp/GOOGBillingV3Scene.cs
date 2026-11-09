using System;
using UnityEngine;

public class GOOGBillingV3Scene : MonoBehaviour
{
	private static string m_szGooglePaymentManager = "com/nubee/platform/payment/android/v3/GooglePaymentManager";

	private IntPtr m_midGetInstance;

	private IntPtr m_midCheckPendingPurchases;

	private IntPtr m_midRequestPurchase;

	private IntPtr m_midConfirmPurchase;

	private string m_szItemPurchased;

	private string m_szItemName = "com.nubee.monsterslayer.diamondslot1";

	private string m_szItemType = "inapp";

	private string m_szItemLoad = "developer_payload";

	public void onCheckPurchase(string szItem)
	{
		Debug.LogError("GOOGBillingV3Scene: " + szItem);
		m_szItemPurchased = szItem;
	}

	private void Start()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		Dbg.Log("GOOGBillingV3Scene.Start");
		AndroidJNI.AttachCurrentThread();
		IntPtr intPtr = AndroidJNI.FindClass(m_szGooglePaymentManager);
		m_midGetInstance = AndroidJNI.GetStaticMethodID(intPtr, "getInstance", "()Lcom/nubee/platform/payment/android/v3/GooglePaymentManager;");
		m_midCheckPendingPurchases = AndroidJNI.GetMethodID(intPtr, "checkPendingPurchases", "()V");
		m_midRequestPurchase = AndroidJNI.GetMethodID(intPtr, "requestPurchase", "(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;)V");
		m_midConfirmPurchase = AndroidJNI.GetMethodID(intPtr, "confirmPurchase", "(Ljava/lang/String;)V");
		jvalue[] args = AndroidJNIHelper.CreateJNIArgArray(new object[0]);
		IntPtr intPtr2 = AndroidJNI.CallStaticObjectMethod(intPtr, m_midGetInstance, args);
		Debug.Log("GooglePaymentScript: cls: " + intPtr);
		Debug.Log("GooglePaymentScript: mid: " + m_midGetInstance);
		Debug.Log("GooglePaymentScript: cpp: " + m_midCheckPendingPurchases);
		Debug.Log("GooglePaymentScript: rpp: " + m_midRequestPurchase);
		Debug.Log("GooglePaymentScript: con: " + m_midConfirmPurchase);
		Debug.Log("GooglePaymentScript: obj: " + intPtr2);
#else

#endif
	}

	private void OnGUI()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		jvalue[] args = AndroidJNIHelper.CreateJNIArgArray(new object[0]);
		IntPtr clazz = AndroidJNI.FindClass(m_szGooglePaymentManager);
		IntPtr obj = AndroidJNI.CallStaticObjectMethod(clazz, m_midGetInstance, args);
		if (GUI.Button(new Rect(20f, 20f, 240f, 60f), m_szItemName))
		{
			jvalue[] args2 = AndroidJNIHelper.CreateJNIArgArray(new string[3] { m_szItemName, m_szItemType, m_szItemLoad });
			AndroidJNI.CallVoidMethod(obj, m_midRequestPurchase, args2);
		}
		string text = "Check Pending Purchase";
		if (GUI.Button(new Rect(20f, 120f, 240f, 60f), text))
		{
			AndroidJNI.CallVoidMethod(obj, m_midCheckPendingPurchases, args);
		}
		GUI.Label(new Rect(20f, 220f, 480f, 240f), m_szItemPurchased);
		if (GUI.Button(new Rect(20f, 520f, 240f, 60f), "CONFIRM"))
		{
			if (m_szItemPurchased != string.Empty)
			{
				jvalue[] args3 = AndroidJNIHelper.CreateJNIArgArray(new string[1] { m_szItemPurchased });
				AndroidJNI.CallVoidMethod(obj, m_midConfirmPurchase, args3);
			}
			else
			{
				Debug.Log("Nothing to confirm");
			}
		}
#else

#endif
	}
}
