using System;
using UnityEngine;

public class PlayerProfilePopup : MonoBehaviour
{
	public UIButton m_closeButton;

	public UITexture m_profilePictureTexture;

	public UILabel m_statusLabel;

	public UISprite m_loading;

	private UIButton m_connectButton;

	private Transform m_connectedGroup;

	private Transform m_disconnectedGroup;

	private UILabel m_nameLabel;

	private UIPopupController m_popupController;

	private void SwitchSNS()
	{
		Transform transform = base.transform.Find("Player_Weibo");
		Transform transform2 = base.transform.Find("Player_Facebook");
		transform.gameObject.SetActiveRecursively(false);
		transform2.gameObject.SetActiveRecursively(true);
		m_connectedGroup = transform2.Find("Connected");
		m_disconnectedGroup = transform2.Find("Not Connected");
		if (null != m_disconnectedGroup)
		{
			m_connectButton = MBHier.GetComponentInChildren<UIButton>(m_disconnectedGroup);
		}
	}

	private void Awake()
	{
		m_popupController = UIPopupController.Instance;
	}

	private void OnEnable()
	{
		Init();
	}

	private void OnDisable()
	{
		SocialNetworkManager.OnLoginCallback -= OnFacebookLogin;
		SocialNetworkManager.OnUserPictureLoaded -= new Action(OnUserPictureLoadedCallback);
	}

	private void Init()
	{
		SwitchSNS();
		bool nameAndStatusDisplay = NubeePlatformBinding.IsLoggedInToFacebook();
		SetNameAndStatusDisplay(nameAndStatusDisplay);
	}

	private void SetGroupActive(Transform p_group, bool p_active)
	{
		if (null != p_group)
		{
			p_group.gameObject.SetActiveRecursively(p_active);
		}
	}

	private void SetNameAndStatusDisplay(bool p_connected)
	{
		if (null != m_loading)
		{
			m_loading.gameObject.SetActiveRecursively(false);
		}
		SetGroupActive(m_connectedGroup, p_connected);
		SetGroupActive(m_disconnectedGroup, !p_connected);
		if (null != m_connectedGroup && null == m_nameLabel)
		{
			Transform transform = m_connectedGroup.transform.Find("Label (player_name)");
			if (null != transform)
			{
				m_nameLabel = transform.gameObject.GetComponent<UILabel>();
			}
		}
		if (null != m_nameLabel && null != SingletonComponent<PlayerDataManager>.Instance)
		{
			m_nameLabel.text = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData().Name;
		}
		if (p_connected)
		{
			SetProfilePictureFromFacebook();
		}
	}

	private void SetProfilePictureFromFacebook()
	{
		PlayerDataManager instance = SingletonComponent<PlayerDataManager>.Instance;
		if (null == m_profilePictureTexture || null == instance)
		{
			return;
		}
		PlayerData playerData = instance.GetPlayerData();
		if (playerData == null)
		{
			return;
		}
		string fbId = playerData.m_userData.fbId;
		Texture2D facebookPictureByIDAsTexture = SingletonComponent<SocialNetworkManager>.Instance.GetFacebookPictureByIDAsTexture(fbId);
		if (null == facebookPictureByIDAsTexture)
		{
			return;
		}
		Shader shader = Shader.Find("Unlit/Transparent Colored");
		if (!(shader == null))
		{
			Material material = new Material(shader);
			if (!(material == null))
			{
				material.mainTexture = facebookPictureByIDAsTexture;
				m_profilePictureTexture.material = material;
				m_profilePictureTexture.enabled = true;
			}
		}
	}

	private void OnButtonClick(GameObject p_sender)
	{
		if (null != m_closeButton && p_sender == m_closeButton.gameObject)
		{
			CloseWindow();
		}
		else if (null != m_connectButton && p_sender == m_connectButton.gameObject)
		{
			ConnectToFacebook();
		}
	}

	private void ConnectToFacebook()
	{
		SingletonComponent<SocialNetworkManager>.Instance.LoginSocialNetwork(SocialNetworkType.Facebook);
		SocialNetworkManager.OnLoginCallback += OnFacebookLogin;
		SocialNetworkManager.OnUserPictureLoaded += new Action(OnUserPictureLoadedCallback);
	}

	private void CloseWindow()
	{
		if (null != m_popupController)
		{
			m_popupController.SendMessage("OnClose", SendMessageOptions.DontRequireReceiver);
		}
	}

	private void OnFacebookLogin(bool p_success)
	{
		SocialNetworkManager.OnLoginCallback -= OnFacebookLogin;
		if (!p_success)
		{
			OnUserPictureLoadedCallback();
			return;
		}
		if (null != m_loading)
		{
			m_loading.gameObject.SetActiveRecursively(true);
		}
		SetGroupActive(m_connectedGroup, false);
		SetGroupActive(m_disconnectedGroup, false);
	}

	private void OnUserPictureLoadedCallback()
	{
		OnUserPictureLoadedCallback(false);
	}

	private void OnUserPictureLoadedCallback(bool p_success = false)
	{
		SocialNetworkManager.OnUserPictureLoaded -= new Action(OnUserPictureLoadedCallback);
		SetNameAndStatusDisplay(p_success);
		if (!p_success && null != m_popupController)
		{
			m_popupController.SendMessage("OnClose", SendMessageOptions.DontRequireReceiver);
			m_popupController.ShowOKPopupMSG("common_error", "facebook_login_failed", OnFailPopupClosed);
		}
	}

	private void OnFailPopupClosed()
	{
		MainMenu instance = MainMenu.Instance;
		if (null != instance)
		{
			instance.ShowPlayerProfilePopup();
		}
	}
}
