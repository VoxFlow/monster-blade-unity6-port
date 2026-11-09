using System;
using System.Collections.Generic;
using System.Text;
using Models;
using NetDimension.Weibo;
using NetDimension.Weibo.Entities.status;
using NetDimension.Weibo.Entities.user;
using UnityEngine;

public class WeiboMod : MonoBehaviour
{
	private delegate NetDimension.Weibo.Entities.user.Collection user_list_func(string uid, string screenName, int count, int cursor, bool trimStatus);

	private string AppKey = "898827076";

	private string AppSecrect = "405fa976bf3b51b0d5e3610511449eab";

	private string CallbackUrl = "https://api.weibo.com/oauth2/default.html";

	private OAuth _oauth;

	private Client _client;

	private WebViewObject _web_view;

	private static WeiboMod _this;

	private Action<WeiboMod> _action;

	private Services m_services;

	private CharacterAccountDetails m_playerDetails;

	private string m_userWeiboID = string.Empty;

	private int m_pageIndex;

	public static WeiboMod Ptr
	{
		get
		{
			return _this;
		}
	}

	public Client Client
	{
		get
		{
			return _client;
		}
	}

	public static void DoAction(Action<WeiboMod> action)
	{
		if (null == Ptr)
		{
			Dbg.LogWarn("Ptr is null.");
		}
		else
		{
			Ptr.LoginDoAction(action);
		}
	}

	public bool IsLogin()
	{
		if (_client != null)
		{
			return true;
		}
		return false;
	}

	public void LoginDoAction(Action<WeiboMod> action)
	{
		if (IsLogin())
		{
			if (action != null)
			{
				action(this);
			}
			return;
		}
		string text = LoadAccessToken();
		if (!string.IsNullOrEmpty(text))
		{
			Debug.Log("[WEIBO] Login expired, try relogin.");
			_oauth = new OAuth(AppKey, AppSecrect, text, null);
			if (CreateClient())
			{
				if (action != null)
				{
					action(this);
				}
				return;
			}
		}
		Debug.Log("[WEIBO] not Login yet. login...");
		_oauth = new OAuth(AppKey, AppSecrect, CallbackUrl);
		string authorizeURL = _oauth.GetAuthorizeURL(ResponseType.Code, null, DisplayType.Mobile);
		_action = action;
		OpenURL(authorizeURL);
	}

	private void Start()
	{
		_this = this;
		UnsafeSecurityPolicy.Instate();
		UnityEngine.Object.DontDestroyOnLoad(this);
	}

	private void SaveAccessToken(string Token)
	{
		MBPrefs.SetString("access_token", Token);
	}

	private string LoadAccessToken()
	{
		return MBPrefs.GetString("access_token");
	}

	private void OpenURL(string Url)
	{
		Debug.Log("[WEBVIEW] open URL:" + Url);
		_web_view = new GameObject("WebViewObject").AddComponent<WebViewObject>();
		_web_view.Init(WebCallback);
		_web_view.LoadURL(Url);
		_web_view.SetVisibility(true);
	}

	private void WebCallback(string url)
	{
		Debug.Log(string.Format("[WEBVIEW] URL : {0}", url));
		PlayerLogedIn(url);
		_web_view = null;
	}

	private bool CreateClient()
	{
		if (!VerifierToken())
		{
			Debug.Log("[WEIBO] token is empty. login failed");
			return false;
		}
		Debug.Log("[WEIBO] token : " + _oauth.AccessToken);
		SaveAccessToken(_oauth.AccessToken);
		_client = new Client(_oauth);
		return true;
	}

	private void PlayerLogedIn(string url)
	{
		int num = url.LastIndexOf("code=");
		string text = url.Substring(num + 5);
		Debug.Log("[WEIBO] Code : " + text);
		AccessToken accessTokenByAuthorizationCode = _oauth.GetAccessTokenByAuthorizationCode(text);
		if (CreateClient())
		{
			if (_action != null)
			{
				_action(this);
			}
			_action = null;
			UpdateMSServerWeiboID();
			SendAllFriendsPage();
		}
	}

	private bool VerifierToken()
	{
		TokenResult tokenResult = _oauth.VerifierAccessToken();
		return tokenResult == TokenResult.Success;
	}

	private bool GetManagers()
	{
		if (_client == null)
		{
			Dbg.LogWarn("Client is not connected to Weibo yet.");
			return false;
		}
		m_userWeiboID = _client.API.Account.GetUID();
		m_services = SingletonComponent<Services>.Instance;
		if (null == SingletonComponent<PlayerDataManager>.Instance || null == m_services || null == SingletonComponent<SaveData>.Instance)
		{
			Dbg.Log("Missing Managers.");
			return false;
		}
		PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		if (playerData.details == null)
		{
			Dbg.LogWarn("The user doesn't have playerData details.");
			return false;
		}
		m_playerDetails = playerData.details;
		return true;
	}

	private bool CanUpdateWeiboID()
	{
		return GetManagers() && m_playerDetails.wbid == string.Empty;
	}

	private void UpdateMSServerWeiboID()
	{
		bool p_isTutorial = SingletonComponent<TutorialManager>.Instance != null && SingletonComponent<TutorialManager>.Instance.enabled;
		if (CanUpdateWeiboID())
		{
			UpdateLocalPlayerName();
			m_services.CreateOrUpdateNubeeAccount(m_services.NubeeId, m_services.DeviceId, SingletonComponent<SaveData>.Instance.GameData.CharacterName, NBUnity.ExpandedToIso2(SingletonComponent<SaveData>.Instance.GameData.Language), SingletonComponent<SaveData>.Instance.GameData.FBID, SingletonComponent<SaveData>.Instance.GameData.FBToken, m_userWeiboID, p_isTutorial, OnWeiboIDUpdated, OnWeiboIDUpdateFail);
		}
	}

	private void UpdateLocalPlayerName()
	{
		if (_client != null && _client.API.Account.GetPrivacy() != null && !(null == SingletonComponent<PlayerDataManager>.Instance) && !(null == MainMenu.Instance))
		{
			NetDimension.Weibo.Entities.status.Entity entity = _client.API.Statuses.Show(m_userWeiboID);
			if (entity == null || _client.API.Statuses.Show(m_userWeiboID).User == null)
			{
				Dbg.LogError("Cannot find User ID in Weibo Statuses.");
				return;
			}
			SingletonComponent<SaveData>.Instance.GameData.CharacterName = _client.API.Statuses.Show(m_userWeiboID).User.Name;
			m_playerDetails.name = SingletonComponent<SaveData>.Instance.GameData.CharacterName;
			m_playerDetails.wbid = m_userWeiboID;
			SingletonComponent<SaveData>.Instance.SaveDataToPlayerPrefs();
			SingletonComponent<PlayerDataManager>.Instance.GetPlayerData().Name = SingletonComponent<SaveData>.Instance.GameData.CharacterName;
			MainMenu.Instance.Refresh();
		}
	}

	private void OnWeiboIDUpdated(Services.ServiceHandle p_handle)
	{
		if (m_playerDetails != null && m_userWeiboID != string.Empty)
		{
			m_playerDetails.wbid = m_userWeiboID;
			Dbg.Log("Weibo ID updated : " + m_userWeiboID);
		}
	}

	private void OnWeiboIDUpdateFail(Services.ServiceHandle p_handle, string p_error)
	{
		Dbg.LogWarn("Could not update Weibo ID : " + p_error);
	}

	private void SendFriendPage()
	{
		Dictionary<int, List<string>> dictionary = new Dictionary<int, List<string>>();
		int ent_per_page = 100;
		List<string> list = new List<string>();
		Dbg.Log("Sending Weibo Friend page : " + m_pageIndex);
		WeiboUser[] friendList = GetFriendList(m_pageIndex, ent_per_page);
		if (friendList == null || friendList.Length == 0)
		{
			Dbg.Log("No more friend to send to MS Server.");
			return;
		}
		for (int i = 0; i < friendList.Length; i++)
		{
			if (friendList[i] != null)
			{
				list.Add(friendList[i].uid);
			}
		}
		dictionary.Add(m_pageIndex, list);
		PrintPageMap(dictionary);
		m_services.AddWeiboFriend(m_services.NubeeId, m_services.DeviceId, list, GetNextPage, AddWeiboFriendFail);
	}

	private void SendAllFriendsPage()
	{
		if (GetManagers())
		{
			m_pageIndex = 0;
			SendFriendPage();
		}
	}

	private void GetNextPage(Services.ServiceHandle p_handle)
	{
		m_pageIndex++;
		SendFriendPage();
	}

	private void AddWeiboFriendFail(Services.ServiceHandle p_handle, string p_error)
	{
		Dbg.LogWarn("Could not add weibo friend to MS Server : " + p_error);
	}

	private void PrintPageMap(Dictionary<int, List<string>> p_pageMap)
	{
		if (p_pageMap == null)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		foreach (int key in p_pageMap.Keys)
		{
			stringBuilder.AppendLine("PageIndex:" + key);
			List<string> list = p_pageMap[key];
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i] != null)
				{
					stringBuilder.AppendLine(list[i]);
				}
			}
		}
		Dbg.Log("Weibo friends :\n" + stringBuilder.ToString());
		stringBuilder = null;
	}

	private bool GetUsers(user_list_func collection, ref Dictionary<string, WeiboUser> user_list, int page_idx, int entities_per_page)
	{
		int cursor = page_idx * entities_per_page;
		string uID = _client.API.Account.GetUID();
		NetDimension.Weibo.Entities.user.Collection collection2 = collection(uID, string.Empty, 20, cursor, true);
		int result = 0;
		int.TryParse(collection2.NextCursor, out result);
		if (result == 0)
		{
			return false;
		}
		IEnumerable<NetDimension.Weibo.Entities.user.Entity> users = collection2.Users;
		foreach (NetDimension.Weibo.Entities.user.Entity item in users)
		{
			string iD = item.ID;
			if (!string.IsNullOrEmpty(iD))
			{
				string value = item.Name;
				if (string.IsNullOrEmpty(value))
				{
					value = iD;
				}
				if (!user_list.ContainsKey(iD))
				{
					WeiboUser weiboUser = new WeiboUser();
					weiboUser.uid = iD;
					weiboUser.name = value;
					weiboUser.imgUrl = item.ProfileImageUrl;
					user_list[iD] = weiboUser;
				}
			}
		}
		return true;
	}

	public WeiboUser[] GetFriendList(int page_idx, int ent_per_page)
	{
		if (_client == null)
		{
			return new WeiboUser[0];
		}
		Dictionary<string, WeiboUser> user_list = new Dictionary<string, WeiboUser>();
		if (!GetUsers(_client.API.Friendships.Friends, ref user_list, page_idx, ent_per_page))
		{
			return new WeiboUser[0];
		}
		List<WeiboUser> list = new List<WeiboUser>(user_list.Values);
		return list.ToArray();
	}
}
