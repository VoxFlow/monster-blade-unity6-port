using Models;
using UnityEngine;

public class Metaps : MonoBehaviour
{
	private string clientId = "";

	private string applicationId = "JRSULKKJSR0001";

	private string applicationKey = "pv6b5wyvzk5k7zo";

	private static string endUserId = "testEndUserId";

	private static string scenario = "testScenario";

	private string infoLabel = string.Empty;

	private uint m_requestId;

	private string m_packageName;

	private string m_campaignId;

	private int m_points;

	private Services m_services;

	private void Start()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		AndroidJNI.AttachCurrentThread();
		MetapsPluginAndroid.Initialize(clientId, applicationId, applicationKey, true, MetapsPluginAndroid.SdkModeProduction());
		MetapsPluginAndroid.PrepareFeaturedAppDialog(endUserId, scenario);
		m_services = SingletonComponent<Services>.Instance;
#else
		m_services = SingletonComponent<Services>.Instance;
#endif
	}

	private void Awake()
	{
		writeLog("Awake");
		MetapsPluginAndroid.retrievePoints += MetapsRetrivePoints;
		MetapsPluginAndroid.finalizeOfferOnError += MetapsFinalizeOfferOnError;
		MetapsPluginAndroid.onStartWaiting += MetapsOnStartWaiting;
		MetapsPluginAndroid.onShowDialog += MetapsOnShowDialog;
		MetapsPluginAndroid.onNoFeaturedApplication += MetapsOnNoFeaturedApplication;
		MetapsPluginAndroid.onCancelDialog += MetapsOnCancelDialog;
		MetapsPluginAndroid.onDownloadApplication += MetapsOnDownloadApplication;
		MetapsPluginAndroid.onShowDialogNotPossible += MetapsOnShowDialogNotPossible;
		MetapsPluginAndroid.initializeFailed += MetapsFailureProcessing;
		MetapsPluginAndroid.launchOfferWallFailed += MetapsFailureProcessing;
		MetapsPluginAndroid.sendActionFailed += MetapsFailureProcessing;
		MetapsPluginAndroid.checkInvitationFailed += MetapsFailureProcessing;
		MetapsPluginAndroid.runInstallReportFailed += MetapsFailureProcessing;
		MetapsPluginAndroid.prepareFailed += MetapsFailureProcessing;
		MetapsPluginAndroid.showFailed += MetapsFailureProcessing;
	}

	private void OnDisable()
	{
		writeLog("OnDisable");
		MetapsPluginAndroid.retrievePoints -= MetapsRetrivePoints;
		MetapsPluginAndroid.finalizeOfferOnError -= MetapsFinalizeOfferOnError;
		MetapsPluginAndroid.onStartWaiting -= MetapsOnStartWaiting;
		MetapsPluginAndroid.onShowDialog -= MetapsOnShowDialog;
		MetapsPluginAndroid.onNoFeaturedApplication -= MetapsOnNoFeaturedApplication;
		MetapsPluginAndroid.onCancelDialog -= MetapsOnCancelDialog;
		MetapsPluginAndroid.onDownloadApplication -= MetapsOnDownloadApplication;
		MetapsPluginAndroid.onShowDialogNotPossible -= MetapsOnShowDialogNotPossible;
		MetapsPluginAndroid.initializeFailed -= MetapsFailureProcessing;
		MetapsPluginAndroid.launchOfferWallFailed -= MetapsFailureProcessing;
		MetapsPluginAndroid.sendActionFailed -= MetapsFailureProcessing;
		MetapsPluginAndroid.checkInvitationFailed -= MetapsFailureProcessing;
		MetapsPluginAndroid.runInstallReportFailed -= MetapsFailureProcessing;
		MetapsPluginAndroid.prepareFailed -= MetapsFailureProcessing;
		MetapsPluginAndroid.showFailed -= MetapsFailureProcessing;
	}

	public static void LaunchOfferWall()
	{
		writeLog("Launch Offer Wall");
		MetapsPluginAndroid.LaunchOfferWall(endUserId, scenario);
	}

	private void AddMetapsRewards(int p_points, string p_packageName, string p_campaignId)
	{
		if (null != m_services)
		{
			m_requestId = (uint)(Random.value * 4.2949673E+09f);
			m_points = p_points;
			m_packageName = p_packageName;
			m_campaignId = p_campaignId;
			m_services.MetapsClaim(m_services.NubeeId, m_services.DeviceId, m_requestId.ToString(), p_points.ToString(), p_packageName, p_campaignId, OnClaimSuccess, OnClaimFail);
		}
	}

	private void OnClaimSuccess(Services.ServiceHandle p_handle)
	{
		CurrencyDetails p_currencies = (CurrencyDetails)p_handle.returnValue;
		Dbg.Log("Metaps Claim currencies set successfully.");
		StoreManager instance = SingletonComponent<StoreManager>.Instance;
		if (null != instance)
		{
			instance.ShowDiamondReceived(p_currencies);
		}
	}

	private void OnClaimFail(Services.ServiceHandle p_handle, string p_error)
	{
		Dbg.LogError("Metaps Claim failed : " + p_error);
		if (p_handle == null)
		{
			UIPopupController.Instance.ShowServerGenericError();
			return;
		}
		if (p_handle.result == Services.ConnectionResults.Timeout)
		{
			UIPopupController.Instance.ShowTimeoutForceRetry(RetryClaim);
			return;
		}
		if (p_handle.IsNoInternet())
		{
			UIPopupController.Instance.ShowNoNetForceRetry(RetryClaim);
			return;
		}
		int p_error2 = ((p_handle != null) ? p_handle.nReturnCode : 0);
		UIPopupController.Instance.ShowServerError("metaps_claim_failed", null, p_error2);
	}

	private void RetryClaim()
	{
		Dbg.Log("RetryClaim");
		if (null != m_services)
		{
			m_services.MetapsClaim(m_services.NubeeId, m_services.DeviceId, m_requestId.ToString(), m_points.ToString(), m_packageName, m_campaignId, OnClaimSuccess, OnClaimFail);
		}
	}

	public void MetapsRetrivePoints(int points, MetapsPluginAndroid.Offer offer)
	{
		writeLog("Points retrieved " + points + " from " + offer.AppName);
		AddMetapsRewards(points, offer.PackageName, offer.CampaignId);
	}

	public void MetapsFinalizeOfferOnError(MetapsPluginAndroid.Offer offer)
	{
		writeLog("Error with " + offer.AppName);
	}

	public void MetapsOnStartWaiting()
	{
		writeLog("Start waiting featured app");
	}

	public void MetapsOnShowDialog()
	{
		writeLog("Show featured app");
	}

	public void MetapsOnNoFeaturedApplication()
	{
		writeLog("No featured app");
	}

	public void MetapsOnCancelDialog()
	{
		writeLog("Featured app dialog canceled");
	}

	public void MetapsOnDownloadApplication()
	{
		writeLog("Download on Featured app dialog");
	}

	public void MetapsOnShowDialogNotPossible()
	{
		writeLog("Featured app dialog can't be shown");
	}

	public void MetapsFailureProcessing(string stackTrace)
	{
		writeLog("Error in Metaps Wrapper " + stackTrace);
	}

	private static void writeLog(string log)
	{
		Debug.Log("[metaps] " + log);
	}
}
