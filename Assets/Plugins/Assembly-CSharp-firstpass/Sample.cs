using UnityEngine;

public class Sample : MonoBehaviour
{
	private string clientId = "";

	private string applicationId = "JRSULKKJSR0001";

	private string applicationKey = "pv6b5wyvzk5k7zo";

	private string endUserId = "testEndUserId";

	private string scenario = "testScenario";

	private string infoLabel = string.Empty;

	private void Start()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			AndroidJNI.AttachCurrentThread();
		}
		MetapsPluginAndroid.Initialize(clientId, applicationId, applicationKey, true, MetapsPluginAndroid.SdkModeTest());
		MetapsPluginAndroid.PrepareFeaturedAppDialog(endUserId, scenario);
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

	public void MetapsRetrivePoints(int points, MetapsPluginAndroid.Offer offer)
	{
		writeLog("Points retrieved " + points + " from " + offer.AppName);
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

	private void writeLog(string log)
	{
		infoLabel = log + "\n" + infoLabel;
	}

	private void OnGUI()
	{
		GUIStyle gUIStyle = new GUIStyle();
		gUIStyle.alignment = TextAnchor.MiddleCenter;
		gUIStyle.normal.textColor = Color.white;
		gUIStyle.wordWrap = true;
		float num = 60f;
		float num2 = 300f;
		float height = 40f;
		float num3 = 50f;
		float left = (float)(Screen.width / 2) - num2 / 2f;
		GUI.Label(new Rect(left, num3, num2, height), "Metaps Unity Sample App", gUIStyle);
		num3 += num;
		if (GUI.Button(new Rect(left, num3, num2, height), "Launch Offer Wall"))
		{
			writeLog("Launch Offer Wall");
			MetapsPluginAndroid.LaunchOfferWall(endUserId, scenario);
		}
		num3 += num;
		if (GUI.Button(new Rect(left, num3, num2, height), "Send Action"))
		{
			writeLog("Send Action");
			MetapsPluginAndroid.SendAction(clientId, applicationId, applicationKey, MetapsPluginAndroid.SdkModeTest());
		}
		num3 += num;
		if (GUI.Button(new Rect(left, num3, num2, height), "Check Invitation"))
		{
			writeLog("Check Invitation");
			MetapsPluginAndroid.CheckInvitation();
		}
		num3 += num;
		if (GUI.Button(new Rect(left, num3, num2, height), "Run Install Report"))
		{
			writeLog("Run Install Report");
			MetapsPluginAndroid.RunInstallReport();
		}
		num3 += num;
		if (GUI.Button(new Rect(left, num3, num2, height), "Prepare Featured App"))
		{
			writeLog("Prepare Featured App");
			MetapsPluginAndroid.PrepareFeaturedAppDialog(endUserId, scenario);
		}
		num3 += num;
		if (GUI.Button(new Rect(left, num3, num2, height), "Show Featured App"))
		{
			writeLog("Show Featured App");
			MetapsPluginAndroid.ShowFeaturedAppDialog(true);
		}
		num3 += num;
		GUIStyle gUIStyle2 = new GUIStyle();
		gUIStyle2.alignment = TextAnchor.UpperLeft;
		gUIStyle2.stretchHeight = true;
		gUIStyle2.normal.textColor = Color.yellow;
		gUIStyle2.wordWrap = true;
		GUI.Box(new Rect(left, num3, num2, height), infoLabel, gUIStyle2);
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}
	}
}
