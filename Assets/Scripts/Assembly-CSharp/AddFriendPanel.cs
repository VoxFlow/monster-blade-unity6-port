using UnityEngine;

public class AddFriendPanel : MonoBehaviour
{
	public UILabel m_headerLabel;

	public UILabel m_inputID;

	public UILabel m_infoLabel;

	private int m_retry;

	private int m_retryMax = 5;

	private SocialManager m_socialManager;

	private UIPopupController m_uiPopupController;

	private string m_yourNubeeID = "social_your_nubee_id";

	private string m_addSuccessfullKey = "social_friend_request_sent";

	private string m_friendIdNotFound = "social_friend_id_not_found";

	private string m_commonError = "social_add_friend_common_error";

	private string m_friendLimitKey = "social_friend_limit_reached";

	private string m_requestBlockedKey = "social_friend_request_blocked";

	private string m_alreadyRequestKey = "social_friend_already_requested";

	private string m_requestingHeader = "social_requesting_friend_header";

	private string m_requestingContent = "social_requesting_friend_content";

	private string m_cannotAddYourself = "social_cannot_add_yourself";

	private string m_alreadyFriends = "social_already_friends";

	private string m_characterNotFound = "social_character_not_found";

	public Color m_addedColor = Color.green;

	public Color m_failColor = Color.red;

	private void Start()
	{
		m_uiPopupController = UIPopupController.Instance;
		m_socialManager = SingletonComponent<SocialManager>.Instance;
		m_infoLabel.gameObject.SetActiveRecursively(false);
		string format = SingletonComponent<NBLocalization>.Instance.Get(m_yourNubeeID);
		string text = string.Format(format, SingletonComponent<Services>.Instance.NubeeId);
		m_headerLabel.text = text;
		Transform transform = base.transform.Find("3-AppleStoreGroup");
		if (transform != null)
		{
			transform.gameObject.SetActiveRecursively(false);
		}
		else
		{
			Dbg.LogWarn("Unable to find \"3-AppleStoreGroup\" and disable it");
		}
	}

	private void OnBack()
	{
		if (m_uiPopupController != null)
		{
			m_uiPopupController.CloseCurrentPopup();
		}
	}

	private void OnAppStoreClick(GameObject _obj)
	{
		Dbg.Log(" Go To AppStore. ");
	}

	private void OnAddFriend()
	{
		if (m_inputID == null || m_socialManager == null || m_uiPopupController == null)
		{
			Dbg.LogWarn(" Missing Managers or prefab connections. ");
			return;
		}
		m_retry = 0;
		m_uiPopupController.HideLoadingPopup();
		m_infoLabel.gameObject.SetActiveRecursively(false);
		AddFriend();
	}

	private void AddFriend()
	{
		m_uiPopupController.ShowLoadingPopup(m_requestingHeader, m_requestingContent);
		m_socialManager.AddFriend(m_inputID.text, "0", OnFriendAddSuccess, OnFriendAddFail);
	}

	private void OnFriendAddSuccess(ServiceResponseCodes _code)
	{
		Dbg.Log(" --> Friend Added <-- ");
		m_infoLabel.gameObject.SetActiveRecursively(true);
		m_infoLabel.text = SingletonComponent<NBLocalization>.Instance.Get(m_addSuccessfullKey);
		m_infoLabel.color = m_addedColor;
		m_uiPopupController.HideLoadingPopup();
	}

	private void ShowFailMessage(string _key)
	{
		m_uiPopupController.HideLoadingPopup();
		m_infoLabel.gameObject.SetActiveRecursively(true);
		m_infoLabel.text = SingletonComponent<NBLocalization>.Instance.Get(_key);
		m_infoLabel.color = m_failColor;
	}

	private void OnFriendAddFail(ServiceResponseCodes _code)
	{
		switch (_code)
		{
		case ServiceResponseCodes.FAILED:
			RetryOnFail();
			break;
		case ServiceResponseCodes.EXCEPTION_FRIEND_LIMIT_EXCEEDED:
			ShowFailMessage(m_friendLimitKey);
			break;
		case ServiceResponseCodes.EXCEPTION_FRIEND_REQUEST_BLOCKED:
			ShowFailMessage(m_requestBlockedKey);
			break;
		case ServiceResponseCodes.EXCEPTION_FRIEND_ALREADY_REQUESTED:
			ShowFailMessage(m_alreadyRequestKey);
			break;
		case ServiceResponseCodes.EXCEPTION_FRIEND_ALREADY_APPROVED:
			ShowFailMessage(m_alreadyFriends);
			break;
		case ServiceResponseCodes.EXCEPTION_FRIEND_ADD_YOURSELF:
			ShowFailMessage(m_cannotAddYourself);
			break;
		case ServiceResponseCodes.EXCEPTION_USER_CHARACTER_NOT_FOUND:
			ShowFailMessage(m_characterNotFound);
			break;
		default:
			ShowFailMessage(m_commonError);
			break;
		}
	}

	private void RetryOnFail()
	{
		if (m_retry < m_retryMax)
		{
			m_retry++;
			AddFriend();
		}
		else
		{
			m_retry = 0;
			ShowFailMessage(m_friendIdNotFound);
		}
	}
}
