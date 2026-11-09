using System;
using System.Collections;
using System.Collections.Generic;
using ServiceCalls;
using UnityEngine;

public class SocialNetworkManager : SingletonComponent<SocialNetworkManager>
{
	private enum ActionType
	{
		register = 0,
		post = 1
	}

	private Material m_playerCurrentPictureMaterial;

	private Dictionary<uint, Material> m_userPictureMaterialMap = new Dictionary<uint, Material>();

	private bool m_bDownloading;

	private bool m_isToGetFBFriends;

	private string m_pictureUrl;

	private static string s_facebookAppID = "";

	private static string s_facebookSecret = "";

	public Dictionary<uint, Material> UserPictureMaterialMap
	{
		get
		{
			return m_userPictureMaterialMap;
		}
	}

	public static string facebookAppID
	{
		get
		{
			return s_facebookAppID;
		}
	}

	public static string facebookSecret
	{
		get
		{
			return s_facebookSecret;
		}
	}

	public static event Action OnUserPictureLoaded;

	public static event Action<bool> OnLoginCallback;

	public static event Action<bool> OnUserNameUpdated;

	public static event Action<List<string>> OnSNSFriendsInfoGot;

	public static event Action<bool> OnPostFacebook;

	static SocialNetworkManager()
	{
		SocialNetworkManager.OnUserPictureLoaded = delegate
		{
		};
		SocialNetworkManager.OnLoginCallback = delegate
		{
		};
		SocialNetworkManager.OnUserNameUpdated = delegate
		{
		};
		SocialNetworkManager.OnSNSFriendsInfoGot = delegate
		{
		};
		SocialNetworkManager.OnPostFacebook = delegate
		{
		};
	}

	private void OnApplicationPause(bool paused)
	{
		if (!paused && SingletonComponent<AchievementManager>.Instance != null && SingletonComponent<SocialManager>.Instance != null)
		{
			bool flag = false;
			bool flag2 = SingletonComponent<SocialManager>.Instance.IsSNSLogin();
			AchievementManager.AchievementItem achievement = SingletonComponent<AchievementManager>.Instance.GetAchievement("achievement_system_3");
			if (achievement != null && !achievement.Achieved && flag2 && SingletonComponent<SaveData>.Instance != null)
			{
				SingletonComponent<SaveData>.Instance.GameData.FbLogin = true;
			}
		}
	}

	private void OnEnable()
	{
		NubeePlatformManager.OnFacebookDidLogin += OnFacebookDidLogin;
		NubeePlatformManager.OnFacebookDidNotLogin += OnFacebookDidNotLogin;
		NubeePlatformManager.OnFacebookGetUserIdDidComplete += OnFacebookGetUserIdDidComplete;
		NubeePlatformManager.OnFacebookGetUserIdDidNotComplete += OnFacebookGetUserIdDidNotComplete;
		NubeePlatformManager.OnFacebookGetFriendsWithAccessTokenDidComplete += OnFacebookGetFriendsWithAccessTokenDidComplete;
		NubeePlatformManager.OnFacebookGetFriendsWithAccessTokenDidNotComplete += OnFacebookGetFriendsWithAccessTokenDidNotComplete;
		NubeePlatformManager.OnFacebookPostDidComplete += OnFaceBookPostDidComplete;
		NubeePlatformManager.OnFacebookPostDidNotComplete += OnFaceBookPostDidNotComplete;
		NubeePlatformManager.OnTwitterGetUserId += OnTwitterGetUserId;
		NubeePlatformManager.OnTwitterGetUserIdFailed += OnTwitterGetUserIdFailed;
		NubeePlatformManager.OnTwitterDidLogin += OnTwitterDidLogin;
		NubeePlatformManager.OnTwitterDidNotLogin += OnTwitterDidNotLogin;
		NubeePlatformManager.OnTwitterPostDone += OnTwitterPostDone;
		NubeePlatformManager.OnTwitterPostCancelled += OnTwitterPostCancelled;
	}

	private void OnDisable()
	{
		NubeePlatformManager.OnFacebookDidLogin -= OnFacebookDidLogin;
		NubeePlatformManager.OnFacebookDidNotLogin -= OnFacebookDidNotLogin;
		NubeePlatformManager.OnFacebookGetUserIdDidComplete -= OnFacebookGetUserIdDidComplete;
		NubeePlatformManager.OnFacebookGetUserIdDidNotComplete -= OnFacebookGetUserIdDidNotComplete;
		NubeePlatformManager.OnFacebookGetFriendsWithAccessTokenDidComplete -= OnFacebookGetFriendsWithAccessTokenDidComplete;
		NubeePlatformManager.OnFacebookGetFriendsWithAccessTokenDidNotComplete -= OnFacebookGetFriendsWithAccessTokenDidNotComplete;
		NubeePlatformManager.OnFacebookPostDidComplete -= OnFaceBookPostDidComplete;
		NubeePlatformManager.OnFacebookPostDidNotComplete -= OnFaceBookPostDidNotComplete;
		NubeePlatformManager.OnTwitterGetUserId -= OnTwitterGetUserId;
		NubeePlatformManager.OnTwitterGetUserIdFailed -= OnTwitterGetUserIdFailed;
		NubeePlatformManager.OnTwitterDidLogin -= OnTwitterDidLogin;
		NubeePlatformManager.OnTwitterDidNotLogin -= OnTwitterDidNotLogin;
		NubeePlatformManager.OnTwitterPostDone -= OnTwitterPostDone;
		NubeePlatformManager.OnTwitterPostCancelled -= OnTwitterPostCancelled;
	}

	public void InitSocialNetwork()
	{
		Dbg.Log("InitSocialNetwork");
		InitSocialNetworkInternal();
	}

	public void LoginSocialNetwork(SocialNetworkType socialType)
	{
		Dbg.Log("Social Network Log:  Try login SNS here!" + socialType);
		switch (socialType)
		{
		case SocialNetworkType.Facebook:
			Dbg.Log("logging into facebook");
			NubeePlatformBinding.LoginFacebook(true);
			break;
		case SocialNetworkType.Twitter:
			Dbg.Log("logging into twitter");
			NubeePlatformBinding.LoginTwitter();
			break;
		}
	}

	public void LogoutSocialNetwork(SocialNetworkType socialType)
	{
		Dbg.LogEx("Social Network Logout: " + socialType, Dbg.NBLogType.COMMON_LOG);
		switch (socialType)
		{
		case SocialNetworkType.Facebook:
		{
			SingletonComponent<SaveData>.Instance.GameData.FBToken = string.Empty;
			SingletonComponent<SaveData>.Instance.SaveDataToPlayerPrefs();
			SaveData instance = SingletonComponent<SaveData>.Instance;
			SingletonComponent<Services>.Instance.CreateOrUpdateNubeeAccount(SingletonComponent<Services>.Instance.NubeeId, SingletonComponent<Services>.Instance.DeviceId, instance.GameData.CharacterName, NBUnity.ExpandedToIso2(instance.GameData.Language), instance.GameData.FBID, instance.GameData.FBToken, string.Empty, false, OnRegisterNubeeAccountSuccess, OnRegisterNubeeAccountFail);
			break;
		}
		case SocialNetworkType.Twitter:
			Dbg.LogWarn("Not Supported yet!");
			break;
		}
	}

	public void PostToSocialNetwork(SocialNetworkType socialType, string message = "", PostType postType = PostType.General, byte[] image = null)
	{
		Dbg.LogEx("Social Network Log:  Try post on {0} with message : {1} ", Dbg.NBLogType.SNS, socialType.ToString(), message);
	}

	public void GetSocialNetworkFriends(SocialNetworkType socialType)
	{
		GetSocialNetworkFriendsInternal(socialType);
	}

	private void InitSocialNetworkInternal()
	{
		Dbg.Log("InitSocialNetworkInternal", Dbg.NBLogType.SNS);
		Dbg.Log("Init social network for Android", Dbg.NBLogType.SNS);
		NubeePlatformBinding.InitFacebook(facebookAppID, facebookSecret);
		if (NubeePlatformBinding.IsLoggedInToFacebook())
		{
			NubeePlatformBinding.TryGetFacebookFriendsWithAccessToken();
		}
	}

	public void GetFacebookPicture()
	{
		Dbg.Log("Social Network Log:  try get user picture here");
		NubeePlatformBinding.GetUserIdOnFacebook();
	}

	public Texture2D GetFacebookPictureAsTexture()
	{
		return GetFacebookPictureByIDAsTexture(SingletonComponent<SaveData>.Instance.GameData.FBID);
	}

	public bool GetFacebookPictureByID(string fbid)
	{
		if (fbid == null || fbid.CompareTo(string.Empty) == 0)
		{
			Dbg.LogWarn("[SocialNetworkManager::GetFacebookPictureByID] null fbid or empty string.");
			return false;
		}
		string text = string.Format("{0}{1}/picture?type=large", GLOBAL.FacebookURL, fbid);
		Dbg.Log("Downloading fb picture : " + text);
		NBCacheManager.Instance.GetFile(text, "facebook", fbid + ".dat", null, FriendPictureDownload, null, null, 10);
		return true;
	}

	public Texture2D GetFacebookPictureByIDAsTexture(string fbid)
	{
		if (fbid == null)
		{
			Dbg.LogError("[SocialNetworkManager::GetFacebookPictureByIDAsTexture] fbid is null");
			return null;
		}
		byte[] fileFromCacheOnly = NBCacheManager.Instance.GetFileFromCacheOnly("facebook", fbid + ".dat");
		if (fileFromCacheOnly == null)
		{
			Dbg.LogWarn("[SocialNetworkManager::GetFacebookPictureByIDAsTexture] picture does not exists in cache for " + fbid);
			return null;
		}
		Texture2D texture2D = new Texture2D(4, 4, TextureFormat.RGBA32, false, true);
		if (null == texture2D)
		{
			Dbg.LogError("[SocialNetworkManager::GetFacebookPictureByIDAsTexture] failed to create texture object");
			return null;
		}
		if (!texture2D.LoadImage(fileFromCacheOnly))
		{
			Dbg.LogError("[SocialNetworkManager::GetFacebookPictureByIDAsTexture] failed to load image into Texture object for " + fbid);
			return null;
		}
		if (texture2D.width == 8 && texture2D.height == 8)
		{
			return null;
		}
		return texture2D;
	}

	private void GetSocialNetworkFriendsInternal(SocialNetworkType socialType)
	{
		Dbg.Log("Social Network Log:  Try get friends here...");
		switch (socialType)
		{
		case SocialNetworkType.Facebook:
			m_isToGetFBFriends = true;
			LoginSocialNetwork(SocialNetworkType.Facebook);
			break;
		}
	}

	private void OnFacebookDidLogin()
	{
		string facebookToken = NubeePlatformBinding.GetFacebookToken();
		if (facebookToken != null)
		{
			SingletonComponent<SaveData>.Instance.GameData.FBToken = facebookToken;
			SingletonComponent<SaveData>.Instance.SaveDataToPlayerPrefs();
		}
		bool flag = false;
		if (SingletonComponent<AchievementManager>.Instance != null)
		{
			AchievementManager.AchievementItem achievement = SingletonComponent<AchievementManager>.Instance.GetAchievement("achievement_system_3");
			if (achievement != null && !achievement.Achieved && SingletonComponent<SaveData>.Instance != null)
			{
				SingletonComponent<SaveData>.Instance.GameData.FbLogin = true;
			}
		}
		Dbg.Log("Social Network Log:  We logged in facebook using nubee platform, oh yeah! token:" + SingletonComponent<SaveData>.Instance.GameData.FBToken);
		GetFacebookPicture();
		SocialNetworkManager.OnLoginCallback(true);
	}

	private void OnFacebookDidNotLogin()
	{
		if (m_isToGetFBFriends)
		{
			SocialNetworkManager.OnSNSFriendsInfoGot(null);
			m_isToGetFBFriends = false;
		}
		Dbg.Log("Social Network Log:  We cannot log in facebook using nubee platform!");
		SocialNetworkManager.OnLoginCallback(false);
	}

	private void OnRegisterNubeeAccountSuccess(Services.ServiceHandle p_handle)
	{
		Dbg.Log("update facebook id and token successful");
		SingletonComponent<SaveData>.Instance.GameData.NeedUpdateFBToken = false;
		SingletonComponent<SaveData>.Instance.GameData.NeedUpdateFBID = false;
		SingletonComponent<SaveData>.Instance.GameData.NeedUpdateFBName = false;
	}

	private void OnRegisterNubeeAccountFail(Services.ServiceHandle p_handle, string p_error)
	{
		Dbg.Log("Failed to register facebook id and token:" + ServiceCallUtils.GetErrorMessage(p_handle));
	}

	private void OnFacebookGetUserIdDidComplete(string fbUser)
	{
		Dbg.Log("Social Network Log:  Get user id successfully!");
		if (fbUser != null)
		{
			JSONObject p_payload = new JSONObject(fbUser);
			string stringField = ServiceCallUtils.GetStringField(p_payload, "id", string.Empty);
			if (stringField.CompareTo("0") == 0)
			{
				m_isToGetFBFriends = false;
				return;
			}
			SingletonComponent<SaveData>.Instance.GameData.FBID = stringField;
			string stringField2 = ServiceCallUtils.GetStringField(p_payload, "last_name", string.Empty);
			string text = ServiceCallUtils.GetStringField(p_payload, "first_name", string.Empty);
			if (stringField2 != null && stringField2.Length > 0)
			{
				string text2 = text;
				text = text2 + " " + stringField2[0] + ".";
			}
			if (text == null || text.Length <= 0)
			{
				Dbg.LogWarn("Name from facebook is empty, but this should not happen.", Dbg.NBLogType.SNS);
				text = GLOBAL.CHARACTER_DEFAULT_NAME;
			}
			if (text.Length > 11)
			{
				text = text.Substring(0, 11);
			}
			Dbg.Log("name: " + text);
			SingletonComponent<SaveData>.Instance.GameData.CharacterName = text;
			SingletonComponent<SaveData>.Instance.SaveDataToPlayerPrefs();
			if (null != SingletonComponent<PlayerDataManager>.Instance)
			{
				SingletonComponent<PlayerDataManager>.Instance.GetPlayerData().Name = SingletonComponent<SaveData>.Instance.GameData.CharacterName;
				SingletonComponent<PlayerDataManager>.Instance.GetPlayerData().m_userData.fbId = SingletonComponent<SaveData>.Instance.GameData.FBID;
			}
			if (null != MainMenu.Instance)
			{
				MainMenu.Instance.Refresh();
			}
			bool p_isTutorial = false;
			if (null != SingletonComponent<TutorialManager>.Instance)
			{
				p_isTutorial = SingletonComponent<TutorialManager>.Instance.enabled;
			}
			if (SingletonComponent<SaveData>.Instance.GameData.NeedUpdateFBID || SingletonComponent<SaveData>.Instance.GameData.NeedUpdateFBToken || SingletonComponent<SaveData>.Instance.GameData.NeedUpdateFBName)
			{
				SaveData instance = SingletonComponent<SaveData>.Instance;
				SingletonComponent<Services>.Instance.CreateOrUpdateNubeeAccount(SingletonComponent<Services>.Instance.NubeeId, SingletonComponent<Services>.Instance.DeviceId, instance.GameData.CharacterName, NBUnity.ExpandedToIso2(instance.GameData.Language), instance.GameData.FBID, instance.GameData.FBToken, string.Empty, p_isTutorial, OnRegisterNubeeAccountSuccess, OnRegisterNubeeAccountFail);
			}
			m_isToGetFBFriends = false;
			NubeePlatformBinding.TryGetFacebookFriendsWithAccessToken();
			Dbg.Log("Social Network Log:  Try to get facebook friends now!");
			Dbg.Log("OnFacebookGetUserIdDidComplete with UserID : " + SingletonComponent<SaveData>.Instance.GameData.FBID, Dbg.NBLogType.SNS);
			long num = long.Parse(SingletonComponent<SaveData>.Instance.GameData.FBID);
			string text3 = string.Format("{0}{1}/picture?type=large", GLOBAL.FacebookURL, num);
			Dbg.Log("URL to get your picture : " + text3);
			NBCacheManager.Instance.GetFile(text3, "facebook", num + ".dat", null, SelfPictureDownload, null, null, 30);
		}
		else
		{
			m_isToGetFBFriends = false;
		}
	}

	public void SelfPictureDownload(bool p_bSuccess, string p_szError, byte[] p_fileContent, object p_arg1, object p_arg2)
	{
		if (!p_bSuccess || p_fileContent == null)
		{
			Dbg.LogError("[SocialNetworkManager::SelfPictureDownload] Unable to download facebook picture: " + ((p_szError == null) ? string.Empty : p_szError));
		}
		else
		{
			SocialNetworkManager.OnUserPictureLoaded();
		}
	}

	public void FriendPictureDownload(bool p_bSuccess, string p_szError, byte[] p_fileContent, object p_arg1, object p_arg2)
	{
		m_bDownloading = false;
		if (!p_bSuccess || p_fileContent == null)
		{
			Dbg.LogError("[SocialNetworkManager::FriendPictureDownload] Unable to download friend facebook picture: " + ((p_szError == null) ? string.Empty : p_szError));
		}
	}

	private void OnFacebookGetUserIdDidNotComplete()
	{
		Dbg.Log("Social Network Log:  Get user id failed!");
	}

	private void OnFacebookGetFriendsWithAccessTokenDidComplete(int count)
	{
		Dbg.Log("OnFacebookGetFriendsWithAccessTokenDidComplete. Count: " + count);
		List<string> list = new List<string>();
		for (int i = 0; i < count; i++)
		{
			string facebookFriendInfo = NubeePlatformBinding.GetFacebookFriendInfo(i);
			if (facebookFriendInfo != null && facebookFriendInfo != string.Empty)
			{
				list.Add(facebookFriendInfo);
			}
		}
		if (list.Count > 0)
		{
			List<string> list2 = new List<string>();
			foreach (string item in list)
			{
				JSONObject jSONObject = new JSONObject(item);
				if (!jSONObject.HasField("uid"))
				{
					Dbg.LogError("[SocialNetworkManager::OnFacebookGetFriendsWithAccessTokenDidComplete] does not have uid. " + item);
					continue;
				}
				long num = (long)jSONObject["uid"].n;
				if (num == 0L)
				{
					Dbg.LogError("[SocialNetworkManager::OnFacebookGetFriendsWithAccessTokenDidComplete] Unable to get uid -" + num + "- ; -" + item + "-");
				}
				else
				{
					list2.Add(num.ToString());
				}
			}
			Dbg.Log("sending to server");
			SingletonComponent<Services>.Instance.AddFacebookFriend(SingletonComponent<Services>.Instance.NubeeId, SingletonComponent<Services>.Instance.DeviceId, list2);
			SingletonComponent<JobManager>.Instance.StartCoroutine(DownloadFriendFacebookPicture(list2));
		}
		SocialNetworkManager.OnSNSFriendsInfoGot(list);
		Dbg.LogWarn("Social Network Log: GET FACEBOOK FRIEND SUCCESS!");
	}

	public IEnumerator DownloadFriendFacebookPicture(List<string> fbids)
	{
		foreach (string fbid in fbids)
		{
			Dbg.Log("friend fbid: " + fbid);
			m_bDownloading = GetFacebookPictureByID(fbid);
			while (m_bDownloading)
			{
				yield return null;
			}
		}
	}

	private void OnFacebookGetFriendsWithAccessTokenDidNotComplete(string reason)
	{
		SocialNetworkManager.OnSNSFriendsInfoGot(null);
	}

	private void OnFaceBookPostDidComplete()
	{
		SocialNetworkManager.OnPostFacebook(true);
		Dbg.Log("OnFaceBookPostDidComplete", Dbg.NBLogType.SNS);
	}

	private void OnFaceBookPostDidNotComplete()
	{
		SocialNetworkManager.OnPostFacebook(false);
		Dbg.LogWarn("OnFaceBookPostDidNotComplete", Dbg.NBLogType.SNS);
	}

	private void OnTwitterGetUserId(string twitterId)
	{
		Dbg.Log("Social Network Log:  we get the twitter user Id using nubee platform already, yeah!!!");
		if (!(twitterId != string.Empty))
		{
		}
	}

	private void OnTwitterGetUserIdFailed()
	{
		Dbg.Log("Social Network Log:  we get the twitter user Id through nubee platform failed, fuck!!!");
	}

	private void OnTwitterDidLogin()
	{
		Dbg.Log("Social Network Log:  for android NB platform we need to make this call to get the Id!");
		NubeePlatformBinding.GetUserIdOnTwitter();
	}

	private void OnTwitterDidNotLogin()
	{
	}

	private void OnTwitterPostDone()
	{
		Dbg.Log("OnTwitterPostDone", Dbg.NBLogType.SNS);
	}

	private void OnTwitterPostCancelled()
	{
		Dbg.LogWarn("OnTwitterPostCancelled", Dbg.NBLogType.SNS);
	}

	private void OnCheckForUpdatesComplete()
	{
	}
}
