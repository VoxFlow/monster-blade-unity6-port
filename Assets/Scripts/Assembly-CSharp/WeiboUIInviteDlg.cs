using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeiboUIInviteDlg : MonoBehaviour
{
	private const int m_displayerPerPage = 20;

	private WeiboUILogic m_logic = new WeiboUILogic();

	private WeiboUser[] m_users;

	private Dictionary<string, WeiboFriendCell> m_idToCell;

	private int m_pageIndex;

	[SerializeField]
	private UIButton m_previousButton;

	[SerializeField]
	private UIButton m_nextButton;

	[SerializeField]
	private UILabel m_sentLabel;

	[SerializeField]
	private WeiboFriendGrid m_grib;

	private bool m_hasSentInvites;

	public PopupCallback m_invitesSentCallback;

	public PopupCallback m_invitesCanceledCallback;

	private void Awake()
	{
		if (null != m_previousButton)
		{
			m_previousButton.SetButtonGrey(m_pageIndex == 0);
		}
		GetFriendsOnPage(m_pageIndex);
	}

	private void OnEnable()
	{
		if (null != m_sentLabel)
		{
			m_sentLabel.gameObject.SetActiveRecursively(false);
		}
	}

	private void OnInviteClicked(GameObject cell)
	{
		if (null == cell || null == cell.transform.parent)
		{
			return;
		}
		WeiboFriendCell component = cell.transform.parent.GetComponent<WeiboFriendCell>();
		if (!(null == component) && component.data != null && !component.data.InviteSent)
		{
			Dbg.Log("WeiboUIInviteDlg.OnInviteClicked " + component.data.name);
			m_logic.SendInvitation(component.data.name, OnInviteCallback);
			component.data.InviteSent = true;
			UIButton component2 = cell.GetComponent<UIButton>();
			if (null != component2)
			{
				component2.SetButtonGrey(true);
			}
			if (null != SingletonComponent<JobManager>.Instance)
			{
				SingletonComponent<JobManager>.Instance.StartCoroutine(ShowInviteSent());
			}
		}
	}

	private void OnInviteCallback(ServiceResponseCodes p_code)
	{
		m_hasSentInvites |= p_code == ServiceResponseCodes.SUCCESS;
	}

	private IEnumerator ShowInviteSent()
	{
		if (null != m_sentLabel && null != SingletonComponent<NBLocalization>.Instance)
		{
			m_sentLabel.gameObject.SetActiveRecursively(true);
			m_sentLabel.text = SingletonComponent<NBLocalization>.Instance.Get("weibo_invite_sent");
		}
		yield return new MBWaitForSeconds(2f);   // PORT (CD-059)
		if (null != m_sentLabel)
		{
			m_sentLabel.gameObject.SetActiveRecursively(false);
		}
	}

	private void OnNextPage()
	{
		m_pageIndex++;
		if (null != m_previousButton && m_pageIndex != 0)
		{
			m_previousButton.SetButtonGrey(false);
		}
		GetFriendsOnPage(m_pageIndex);
		bool flag = m_users != null && m_users.Length == 20;
		if (null != m_nextButton)
		{
			m_nextButton.SetButtonGrey(!flag);
		}
	}

	private void OnPreviousPage()
	{
		m_pageIndex--;
		if (m_pageIndex < 0)
		{
			m_pageIndex = 0;
		}
		if (null != m_previousButton)
		{
			m_previousButton.SetButtonGrey(m_pageIndex == 0);
		}
		GetFriendsOnPage(m_pageIndex);
	}

	private void OnCloseButtonClick()
	{
		if (null != UIPopupController.Instance)
		{
			UIPopupController.Instance.CloseCurrentPopup();
		}
		if (m_hasSentInvites)
		{
			if (m_invitesSentCallback != null)
			{
				m_invitesSentCallback();
			}
		}
		else if (m_invitesCanceledCallback != null)
		{
			m_invitesCanceledCallback();
		}
	}

	private void GetFriendsOnPage(int page_idx)
	{
		if (null != UIPopupController.Instance)
		{
			UIPopupController.Instance.ShowLoadingPopupNetWait();
		}
		WeiboMod.DoAction(delegate(WeiboMod weibo)
		{
			m_users = weibo.GetFriendList(page_idx, 20);
			SingletonComponent<JobManager>.Instance.StartCoroutine(SetupCells());
		});
	}

	private void SetupPicture(string weibo_id)
	{
		if (m_idToCell != null)
		{
			Texture2D weiboTexture = m_logic.GetWeiboTexture(weibo_id);
			WeiboFriendCell value = null;
			if (m_idToCell.TryGetValue(weibo_id, out value))
			{
				value.SetWBPicture(weiboTexture);
			}
		}
	}

	private IEnumerator SetupCells()
	{
		if (m_users == null || null == m_grib)
		{
			Dbg.LogWarn("We need to get the users from Weibo API first, and check Grid reference.");
			yield break;
		}
		m_grib.Clear();
		m_idToCell = new Dictionary<string, WeiboFriendCell>();
		for (int i = 0; i < m_users.Length; i++)
		{
			WeiboUser user = m_users[i];
			if (user != null)
			{
				m_grib.AddData(user);
				WeiboFriendCell wCell = m_grib.AddCell(0f, 0f);
				wCell.data = user;
				wCell.SetTarget(base.gameObject, "OnInviteClicked");
				m_idToCell.Add(user.uid, wCell);
			}
			yield return null;
		}
		SingletonComponent<JobManager>.Instance.StartCoroutine(m_logic.DownloadWeiboPictures(m_users, SetupPicture));
		if (null != UIPopupController.Instance)
		{
			UIPopupController.Instance.HideLoadingPopup();
		}
	}
}
