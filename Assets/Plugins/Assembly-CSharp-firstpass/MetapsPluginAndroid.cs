using System;
using UnityEngine;

public class MetapsPluginAndroid : MonoBehaviour
{
	public class Offer
	{
		public readonly string AppId;

		public readonly string CampaignId;

		public readonly string PackageName;

		public readonly string Scenario;

		public readonly string AppName;

		public readonly int RewardEndUser;

		public readonly int RewardDeveloper;

		public readonly int OfferStatus;

		public Offer(AndroidJavaObject androidOffer)
		{
			AppId = androidOffer.Call<string>("getAppId", new object[0]);
			CampaignId = androidOffer.Call<string>("getCampaignId", new object[0]);
			PackageName = androidOffer.Call<string>("getPackageName", new object[0]);
			Scenario = androidOffer.Call<string>("getScenario", new object[0]);
			AppName = androidOffer.Call<string>("getAppName", new object[0]);
			RewardEndUser = androidOffer.Call<int>("getRewardForUser", new object[0]);
			RewardDeveloper = androidOffer.Call<int>("getRewardForMedia", new object[0]);
			OfferStatus = androidOffer.Call<int>("getStatus", new object[0]);
		}
	}

	private static AndroidJavaClass factory;

	private static AndroidJavaClass Factory
	{
		get
		{
			if (factory == null)
			{
				Debug.Log("Loading metaps unity wrapper");
				factory = new AndroidJavaClass("net.metaps.sdk.UnityWrapper");
			}
			return factory;
		}
	}

	public static event Action<int, Offer> retrievePoints;

	public static event Action<Offer> finalizeOfferOnError;

	public static event Action onStartWaiting;

	public static event Action onShowDialog;

	public static event Action onNoFeaturedApplication;

	public static event Action onCancelDialog;

	public static event Action onDownloadApplication;

	public static event Action onShowDialogNotPossible;

	public static event Action<string> initializeFailed;

	public static event Action<string> launchOfferWallFailed;

	public static event Action<string> sendActionFailed;

	public static event Action<string> checkInvitationFailed;

	public static event Action<string> runInstallReportFailed;

	public static event Action<string> prepareFailed;

	public static event Action<string> showFailed;

	private void Awake()
	{
		base.gameObject.name = GetType().ToString();
		SetCallbackHandler(base.gameObject.name);
		Debug.Log("Wrapper sends message to " + base.gameObject.name);
	}

	private static void SetCallbackHandler(string handlerName)
	{
		Factory.CallStatic("setHandlerName", handlerName);
	}

	public static int SdkModeProduction()
	{
		return Factory.CallStatic<int>("getSdkModeProductionValue", new object[0]);
	}

	public static int SdkModeTest()
	{
		return Factory.CallStatic<int>("getSdkModeTestValue", new object[0]);
	}

	public static void Initialize(string clientId, string applicationId, string applicationKey, bool useReceiver, int mode)
	{
		Factory.CallStatic("initialize", clientId, applicationId, applicationKey, useReceiver, mode);
	}

	public static void LaunchOfferWall(string endUserId, string scenario)
	{
		Factory.CallStatic("launchOfferWall", endUserId, scenario);
	}

	public static void SendAction(string clientId, string applicationId, string applicationKey, int mode)
	{
		Factory.CallStatic("sendAction", clientId, applicationId, applicationKey, mode);
	}

	public static void CheckInvitation()
	{
		Factory.CallStatic("checkInvitation");
	}

	public static void RunInstallReport()
	{
		Factory.CallStatic("runInstallReport");
	}

	public static void PrepareFeaturedAppDialog(string endUserId, string scenario)
	{
		Factory.CallStatic("prepare", endUserId, endUserId);
	}

	public static void ShowFeaturedAppDialog(bool useListener)
	{
		Factory.CallStatic("show", useListener);
	}

	public void ReceiverRetrieve(string offerJsonString)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		if (MetapsPluginAndroid.retrievePoints != null)
		{
			Offer offer = new Offer(Factory.CallStatic<AndroidJavaObject>("buildOfferFromJson", new object[1] { offerJsonString }));
			MetapsPluginAndroid.retrievePoints(offer.RewardEndUser, offer);
		}
#else

#endif
	}

	public void ReceiverFinalizeOnError(string offerJsonString)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		if (MetapsPluginAndroid.finalizeOfferOnError != null)
		{
			Offer offer = new Offer(Factory.CallStatic<AndroidJavaObject>("finalizeOfferOnError", new object[1] { offerJsonString }));
			MetapsPluginAndroid.retrievePoints(offer.RewardEndUser, offer);
		}
#else

#endif
	}

	public void FeaturedAppDialogOnStartWaiting()
	{
		if (MetapsPluginAndroid.onStartWaiting != null)
		{
			MetapsPluginAndroid.onStartWaiting();
		}
	}

	public void FeaturedAppDialogOnShowDialog()
	{
		if (MetapsPluginAndroid.onShowDialog != null)
		{
			MetapsPluginAndroid.onShowDialog();
		}
	}

	public void FeaturedAppDialogOnNoFeaturedApplication()
	{
		if (MetapsPluginAndroid.onNoFeaturedApplication != null)
		{
			MetapsPluginAndroid.onNoFeaturedApplication();
		}
	}

	public void FeaturedAppDialogOnCancelDialog()
	{
		if (MetapsPluginAndroid.onCancelDialog != null)
		{
			MetapsPluginAndroid.onCancelDialog();
		}
	}

	public void FeaturedAppDialogOnDownloadApplication()
	{
		if (MetapsPluginAndroid.onDownloadApplication != null)
		{
			MetapsPluginAndroid.onDownloadApplication();
		}
	}

	public void FeaturedAppDialogOnShowDialogNotPossible()
	{
		if (MetapsPluginAndroid.onShowDialogNotPossible != null)
		{
			MetapsPluginAndroid.onShowDialogNotPossible();
		}
	}

	public void InitializeFailed(string stackTrace)
	{
		if (MetapsPluginAndroid.initializeFailed != null)
		{
			MetapsPluginAndroid.initializeFailed(stackTrace);
		}
	}

	public void LaunchOfferWallFailed(string stackTrace)
	{
		if (MetapsPluginAndroid.launchOfferWallFailed != null)
		{
			MetapsPluginAndroid.launchOfferWallFailed(stackTrace);
		}
	}

	public void SendActionFailed(string stackTrace)
	{
		if (MetapsPluginAndroid.sendActionFailed != null)
		{
			MetapsPluginAndroid.sendActionFailed(stackTrace);
		}
	}

	public void CheckInvitationFailed(string stackTrace)
	{
		if (MetapsPluginAndroid.checkInvitationFailed != null)
		{
			MetapsPluginAndroid.checkInvitationFailed(stackTrace);
		}
	}

	public void RunInstallReportFailed(string stackTrace)
	{
		if (MetapsPluginAndroid.runInstallReportFailed != null)
		{
			MetapsPluginAndroid.runInstallReportFailed(stackTrace);
		}
	}

	public void PrepareFailed(string stackTrace)
	{
		if (MetapsPluginAndroid.prepareFailed != null)
		{
			MetapsPluginAndroid.prepareFailed(stackTrace);
		}
	}

	public void ShowFailed(string stackTrace)
	{
		if (MetapsPluginAndroid.showFailed != null)
		{
			MetapsPluginAndroid.showFailed(stackTrace);
		}
	}
}
