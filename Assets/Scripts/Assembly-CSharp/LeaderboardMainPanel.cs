using UnityEngine;

public class LeaderboardMainPanel : NBUIMenuPanel
{
	[SerializeField]
	private UIButton m_spShop;

	private UIButton m_social;

	private new void Awake()
	{
		Transform transform = base.transform.Find("2 - Social");
		if (transform != null)
		{
			m_social = transform.gameObject.GetComponent<UIButton>();
		}
	}

	private void OnEnable()
	{
		if (m_spShop != null)
		{
			m_spShop.gameObject.SetActiveRecursively(false);
		}
		if (m_social != null)
		{
			m_social.gameObject.SetActiveRecursively(false);
		}
	}

	private void Start()
	{
	}

	private new void Update()
	{
	}

	public void OnGlobalBtnClick()
	{
		Dbg.Log("[Info] OnGlobalBtnClick()");
		UIPopupController.Instance.ShowLoadingPopupNetWait();
		SingletonComponent<LeaderboardManager>.Instance.GetGlobalLeaderboard(0);
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_secondary_ui_tap");
	}

	public void OnSocialBtnClick()
	{
		Dbg.Log("[Info] OnSocialBtnClick()");
		UIPopupController.Instance.ShowLoadingPopupNetWait();
		SingletonComponent<LeaderboardManager>.Instance.GetSocailLeaderboard(0);
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_secondary_ui_tap");
	}

	private void OnSpShopBtnClick()
	{
		Debug.LogWarning("[Info] OnSpShopBtnClick()");
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_secondary_ui_tap");
	}
}
