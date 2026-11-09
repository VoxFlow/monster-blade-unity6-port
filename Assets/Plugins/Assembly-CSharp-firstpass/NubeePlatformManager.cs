using System;
using UnityEngine;

public class NubeePlatformManager : MonoBehaviour
{
	public static event Action OnReceivedMemoryWarning;

	public static event Action OnUserAgreedToReview;

	public static event Action<int> OnGameBadgeCountUpdated;

	public static event Action OnConnectionSuccess;

	public static event Action OnConnectionFailed;

	public static event Action OnDashboardOpening;

	public static event Action OnDashboardClosing;

	public static event Action OnRegisterRemoteNotificationSuccess;

	public static event Action OnRegisterRemoteNotificationFailed;

	public static event Action OnFacebookDidLogin;

	public static event Action OnFacebookDidNotLogin;

	public static event Action<string> OnFacebookGetUserIdDidComplete;

	public static event Action OnFacebookGetUserIdDidNotComplete;

	public static event Action OnFacebookPostDidComplete;

	public static event Action OnFacebookPostDidNotComplete;

	public static event Action<int> OnFacebookGetFriendsWithAccessTokenDidComplete;

	public static event Action<string> OnFacebookGetFriendsWithAccessTokenDidNotComplete;

	public static event Action OnFacebookInviteFriendsDidComplete;

	public static event Action OnFacebookInviteFriendsDidNotComplete;

	public static event Action<int> OnFacebookGetFriendsWithoutAccessTokenDidComplete;

	public static event Action<string> OnFacebookGetFriendsWithoutAccessTokenDidNotComplete;

	public static event Action OnRenrenDidLogin;

	public static event Action OnRenrenDidNotLogin;

	public static event Action<string> OnRenrenDidGetOtherUserPictureUrl;

	public static event Action OnRenrenDidNotGetOtherUserPictureUrl;

	public static event Action OnTwitterDidLogin;

	public static event Action OnTwitterDidNotLogin;

	public static event Action<string> OnTwitterGetUserId;

	public static event Action OnTwitterGetUserIdFailed;

	public static event Action OnTwitterPostDone;

	public static event Action OnTwitterPostCancelled;

	public static event Action OnCallDisplayTwitterLoginAlert;

	public static event Action<string> OnWeiboDidLogin;

	public static event Action<string> OnWeiboDidNotLogin;

	public static event Action OnWeiboPostDone;

	public static event Action<string> OnWeiboPostFailed;

	public static event Action OnWeiboPostCancelled;

	public static event Action OnWeiboViewClose;

	public static event Action<string> OnChatRoomDidLoad;

	public static event Action<string> OnChatRoomDidNotLoad;

	public static event Action<string> OnChatMessageReceived;

	public static event Action OnChatRoomDisconnected;

	public static event Action<bool> OnBillSupported;

	public static event Action<GooglePaymentTransaction> OnPurchaseStateChanged;

	public static event Action<string> OnPurchaseUserCancelled;

	public static event Action<string> OnPurchaseItemUnavailable;

	public static event Action<string> OnPurchaseOtherError;

	static NubeePlatformManager()
	{
		NubeePlatformManager.OnReceivedMemoryWarning = delegate
		{
		};
		NubeePlatformManager.OnUserAgreedToReview = delegate
		{
		};
		NubeePlatformManager.OnGameBadgeCountUpdated = delegate
		{
		};
		NubeePlatformManager.OnConnectionSuccess = delegate
		{
		};
		NubeePlatformManager.OnConnectionFailed = delegate
		{
		};
		NubeePlatformManager.OnDashboardOpening = delegate
		{
		};
		NubeePlatformManager.OnDashboardClosing = delegate
		{
		};
		NubeePlatformManager.OnRegisterRemoteNotificationSuccess = delegate
		{
		};
		NubeePlatformManager.OnRegisterRemoteNotificationFailed = delegate
		{
		};
		NubeePlatformManager.OnFacebookDidLogin = delegate
		{
		};
		NubeePlatformManager.OnFacebookDidNotLogin = delegate
		{
		};
		NubeePlatformManager.OnFacebookGetUserIdDidComplete = delegate
		{
		};
		NubeePlatformManager.OnFacebookGetUserIdDidNotComplete = delegate
		{
		};
		NubeePlatformManager.OnFacebookPostDidComplete = delegate
		{
		};
		NubeePlatformManager.OnFacebookPostDidNotComplete = delegate
		{
		};
		NubeePlatformManager.OnFacebookGetFriendsWithAccessTokenDidComplete = delegate
		{
		};
		NubeePlatformManager.OnFacebookGetFriendsWithAccessTokenDidNotComplete = delegate
		{
		};
		NubeePlatformManager.OnFacebookInviteFriendsDidComplete = delegate
		{
		};
		NubeePlatformManager.OnFacebookInviteFriendsDidNotComplete = delegate
		{
		};
		NubeePlatformManager.OnFacebookGetFriendsWithoutAccessTokenDidComplete = delegate
		{
		};
		NubeePlatformManager.OnFacebookGetFriendsWithoutAccessTokenDidNotComplete = delegate
		{
		};
		NubeePlatformManager.OnRenrenDidLogin = delegate
		{
		};
		NubeePlatformManager.OnRenrenDidNotLogin = delegate
		{
		};
		NubeePlatformManager.OnRenrenDidGetOtherUserPictureUrl = delegate
		{
		};
		NubeePlatformManager.OnRenrenDidNotGetOtherUserPictureUrl = delegate
		{
		};
		NubeePlatformManager.OnTwitterDidLogin = delegate
		{
		};
		NubeePlatformManager.OnTwitterDidNotLogin = delegate
		{
		};
		NubeePlatformManager.OnTwitterGetUserId = delegate
		{
		};
		NubeePlatformManager.OnTwitterGetUserIdFailed = delegate
		{
		};
		NubeePlatformManager.OnTwitterPostDone = delegate
		{
		};
		NubeePlatformManager.OnTwitterPostCancelled = delegate
		{
		};
		NubeePlatformManager.OnCallDisplayTwitterLoginAlert = delegate
		{
		};
		NubeePlatformManager.OnWeiboDidLogin = delegate
		{
		};
		NubeePlatformManager.OnWeiboDidNotLogin = delegate
		{
		};
		NubeePlatformManager.OnWeiboPostDone = delegate
		{
		};
		NubeePlatformManager.OnWeiboPostFailed = delegate
		{
		};
		NubeePlatformManager.OnWeiboPostCancelled = delegate
		{
		};
		NubeePlatformManager.OnWeiboViewClose = delegate
		{
		};
		NubeePlatformManager.OnChatRoomDidLoad = delegate
		{
		};
		NubeePlatformManager.OnChatRoomDidNotLoad = delegate
		{
		};
		NubeePlatformManager.OnChatMessageReceived = delegate
		{
		};
		NubeePlatformManager.OnChatRoomDisconnected = delegate
		{
		};
		NubeePlatformManager.OnBillSupported = delegate
		{
		};
		NubeePlatformManager.OnPurchaseStateChanged = delegate
		{
		};
		NubeePlatformManager.OnPurchaseUserCancelled = delegate
		{
		};
		NubeePlatformManager.OnPurchaseItemUnavailable = delegate
		{
		};
		NubeePlatformManager.OnPurchaseOtherError = delegate
		{
		};
	}

	private void OnEnable()
	{
		Debug.Log("NubeePlatformManager is enabled");
	}

	private void OnDisable()
	{
		Debug.Log("NubeePlatformManager is disabled");
	}

	public void GameBadgeCountUpdated(string strCount)
	{
		int result = 0;
		int.TryParse(strCount, out result);
		NubeePlatformManager.OnGameBadgeCountUpdated(result);
	}

	public void ReceivedMemoryWarning()
	{
		NubeePlatformManager.OnReceivedMemoryWarning();
	}

	public void UserAgreedToReview()
	{
		NubeePlatformManager.OnUserAgreedToReview();
	}

	public void ConnectionSuccess()
	{
		NubeePlatformManager.OnConnectionSuccess();
	}

	public void ConnectionFailed()
	{
		NubeePlatformManager.OnConnectionFailed();
	}

	public void DashboardOpening()
	{
		NubeePlatformManager.OnDashboardOpening();
	}

	public void DashboardClosing()
	{
		NubeePlatformManager.OnDashboardClosing();
	}

	public void RegisterRemoteNotificationSuccess()
	{
		NubeePlatformManager.OnRegisterRemoteNotificationSuccess();
	}

	public void RegisterRemoteNotificationFailed()
	{
		NubeePlatformManager.OnRegisterRemoteNotificationFailed();
	}

	public void FacebookDidLogin()
	{
		NubeePlatformManager.OnFacebookDidLogin();
	}

	public void FacebookDidNotLogin()
	{
		NubeePlatformManager.OnFacebookDidNotLogin();
	}

	public void FacebookGetUserIdDidComplete(string fbId)
	{
		NubeePlatformManager.OnFacebookGetUserIdDidComplete(fbId);
	}

	public void FacebookGetUserIdDidNotComplete()
	{
		NubeePlatformManager.OnFacebookGetUserIdDidNotComplete();
	}

	public void FacebookPostDidComplete()
	{
		NubeePlatformManager.OnFacebookPostDidComplete();
	}

	public void FacebookPostDidNotComplete()
	{
		NubeePlatformManager.OnFacebookPostDidNotComplete();
	}

	public void FacebookGetFriendsWithAccessTokenDidComplete(string count)
	{
		int result = 0;
		if (int.TryParse(count, out result))
		{
			NubeePlatformManager.OnFacebookGetFriendsWithAccessTokenDidComplete(result);
		}
		else
		{
			NubeePlatformManager.OnFacebookGetFriendsWithAccessTokenDidNotComplete("Error parsing number of friends with access token from facebook!");
		}
	}

	public void FacebookGetFriendsWithAccessTokenDidNotComplete(string reason)
	{
		NubeePlatformManager.OnFacebookGetFriendsWithAccessTokenDidNotComplete(reason);
	}

	public void FacebookGetFriendsWithoutAccessTokenDidComplete(string count)
	{
		int result = 0;
		if (int.TryParse(count, out result))
		{
			NubeePlatformManager.OnFacebookGetFriendsWithoutAccessTokenDidComplete(result);
		}
		else
		{
			NubeePlatformManager.OnFacebookGetFriendsWithoutAccessTokenDidNotComplete("Error parsing number of friends without access token from facebook!");
		}
	}

	public void FacebookGetFriendsWithoutAccessTokenDidNotComplete(string reason)
	{
		NubeePlatformManager.OnFacebookGetFriendsWithoutAccessTokenDidNotComplete(reason);
	}

	public void FacebookInviteFriendsDidComplete()
	{
		NubeePlatformManager.OnFacebookInviteFriendsDidComplete();
	}

	public void FacebookInviteFriendsDidNotComplete()
	{
		NubeePlatformManager.OnFacebookInviteFriendsDidNotComplete();
	}

	public void RenrenDidLogin()
	{
		NubeePlatformManager.OnRenrenDidLogin();
	}

	public void RenrenDidNotLogin()
	{
		NubeePlatformManager.OnRenrenDidNotLogin();
	}

	public void RenrenDidGetOtherUserPictureUrl(string pictureUrl)
	{
		NubeePlatformManager.OnRenrenDidGetOtherUserPictureUrl(pictureUrl);
	}

	public void RenrenDidNotGetOtherUserPictureUrl()
	{
		NubeePlatformManager.OnRenrenDidNotGetOtherUserPictureUrl();
	}

	public void TwitterDidLogin()
	{
		NubeePlatformManager.OnTwitterDidLogin();
	}

	public void TwitterDidNotLogin()
	{
		NubeePlatformManager.OnTwitterDidNotLogin();
	}

	public void TwitterGetUserId(string userId)
	{
		NubeePlatformManager.OnTwitterGetUserId(userId);
	}

	public void TwitterGetUserIdFailed()
	{
		NubeePlatformManager.OnTwitterGetUserIdFailed();
	}

	public void TwitterPostDone()
	{
		NubeePlatformManager.OnTwitterPostDone();
	}

	public void TwitterPostCancelled()
	{
		NubeePlatformManager.OnTwitterPostCancelled();
	}

	public void CallDisplayTwitterLoginAlert()
	{
		NubeePlatformManager.OnCallDisplayTwitterLoginAlert();
	}

	public void WeiboDidLogin(string appId)
	{
		NubeePlatformManager.OnWeiboDidLogin(appId);
	}

	public void WeiboDidNotLogin(string reason)
	{
		NubeePlatformManager.OnWeiboDidNotLogin(reason);
	}

	public void WeiboPostDone()
	{
		NubeePlatformManager.OnWeiboPostDone();
	}

	public void WeiboPostFailed(string reason)
	{
		NubeePlatformManager.OnWeiboPostFailed(reason);
	}

	public void WeiboPostCancelled()
	{
		NubeePlatformManager.OnWeiboPostCancelled();
	}

	public void WeiboViewClose()
	{
		NubeePlatformManager.OnWeiboViewClose();
	}

	public void ChatRoomDidLoad(string messages)
	{
		NubeePlatformManager.OnChatRoomDidLoad(messages);
	}

	public void ChatRoomDidNotLoad(string reason)
	{
		NubeePlatformManager.OnChatRoomDidNotLoad(reason);
	}

	public void ChatMessageReceived(string message)
	{
		NubeePlatformManager.OnChatMessageReceived(message);
	}

	public void ChatRoomDisconnected()
	{
		NubeePlatformManager.OnChatRoomDisconnected();
	}

	public void BillSupported(string supported)
	{
		NubeePlatformManager.OnBillSupported(bool.Parse(supported));
	}

	public void PurchaseStateChanged(string json)
	{
		NubeePlatformManager.OnPurchaseStateChanged(GooglePaymentTransaction.transactionFromJson(json));
	}

	public void PurchaseUserCancelled(string productId)
	{
		NubeePlatformManager.OnPurchaseUserCancelled(productId);
	}

	public void PurchaseItemUnavailable(string productId)
	{
		NubeePlatformManager.OnPurchaseItemUnavailable(productId);
	}

	public void PurchaseOtherError(string productId)
	{
		NubeePlatformManager.OnPurchaseOtherError(productId);
	}
}
