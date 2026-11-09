using Models;
using UnityEngine;

public class AnnouncementCell : NBUIGridCell<AnnouncementData>
{
	[SerializeField]
	private UILabel m_date;

	[SerializeField]
	private UILabel m_title;

	[SerializeField]
	private UILabel m_description;

	[SerializeField]
	private UISprite m_typeIcon;

	[SerializeField]
	private UISprite m_new;

	[SerializeField]
	private NBUILocalize m_viewOrClaimBtnText;

	[SerializeField]
	private UIButtonMessage m_viewOrClaimBtnMsg;

	[SerializeField]
	private UIButtonMessage m_deleteBtnMsg;

	protected override void Awake()
	{
		UIPanel component = GetComponent<UIPanel>();
		if (null != component)
		{
			Object.DestroyImmediate(component);
		}
	}

	public void UpdateNewTag()
	{
		if (null != m_new)
		{
			m_new.enabled = !data.IsViewed;
		}
	}

	public void ShowAnnouncementInfo()
	{
		m_date.text = data.StartDate;
		m_title.text = data.AnnounceMessage.Title;
		m_typeIcon.spriteName = SingletonComponent<AnnouncementManager>.Instance.m_CellIcons[(int)(data.AnnTypeId - 1)];
		m_typeIcon.MakePixelPerfect();
		m_description.text = PreProcessMsgBody(data.AnnounceMessage.Body, 20);
		UpdateNewTag();
		switch (data.AnnTypeId)
		{
		case AnnoucementTypeId.EVENT:
			m_viewOrClaimBtnText.Key = "announcement_view";
			m_viewOrClaimBtnMsg.functionName = "OnViewForEventClick";
			break;
		case AnnoucementTypeId.CLAIM:
			m_viewOrClaimBtnText.Key = "announcement_view";
			m_viewOrClaimBtnMsg.functionName = "OnViewForTextAndItemClick";
			break;
		case AnnoucementTypeId.TEXT:
			m_viewOrClaimBtnText.Key = "announcement_view";
			m_viewOrClaimBtnMsg.functionName = "OnViewForTextAndItemClick";
			break;
		case AnnoucementTypeId.SOCIAL:
			break;
		case AnnoucementTypeId.TREASURE:
			break;
		default:
			m_date.enabled = false;
			m_title.enabled = false;
			m_description.enabled = false;
			m_typeIcon.enabled = false;
			break;
		}
	}

	public void OnViewForEventClick()
	{
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_ok");
		UIPopupController.Instance.ShowAnnouncementItemPopup(m_data);
		View();
	}

	public void OnViewForTextAndItemClick()
	{
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_ok");
		UIPopupController.Instance.ShowAnnouncementItemPopup(m_data);
		View();
	}

	public void OnViewForTextClick()
	{
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_ok");
		UIPopupController.Instance.ShowAnnouncementPopup(m_data);
		View();
	}

	private void View()
	{
		m_data.IsViewed = true;
		if (null != MainMenu.Instance.m_AnnouncementPanel)
		{
			MainMenu.Instance.m_AnnouncementPanel.UpdateNewFlags();
		}
		if (null != SingletonComponent<AnnouncementManager>.Instance)
		{
			SingletonComponent<AnnouncementManager>.Instance.GetNewAnnouncements();
		}
	}

	private string PreProcessMsgBody(string p_str, int p_cutNum)
	{
		if (p_str.Length > p_cutNum)
		{
			return p_str.Substring(0, p_cutNum) + "...";
		}
		return p_str;
	}

	public void OnDeleteButtonClick(GameObject p_go)
	{
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_secondary_ui_tap");
		if (null != SingletonComponent<AnnouncementManager>.Instance)
		{
			if (null != UIPopupController.Instance)
			{
				UIPopupController.Instance.ShowGenericPopupMSG("delete_msg_confirm", "common_ok", "common_cancel", ConfirmDelete);
			}
		}
		else
		{
			Dbg.LogWarn("AnnouncementManager.Instance is null when try to send DeleteAnnouncementMessage()");
		}
	}

	public void ConfirmDelete()
	{
		Dbg.Log("[Info] Try to delete announcement message id: " + data.DbId);
		SingletonComponent<AnnouncementManager>.Instance.DeleteAnnouncementMessage(data.DbId);
		if (SingletonComponent<AnnouncementManager>.Instance.Data.Annoucements.ContainsKey(data.DbId))
		{
			SingletonComponent<AnnouncementManager>.Instance.Data.Annoucements.Remove(data.DbId);
		}
		if (null != MainMenu.Instance.m_AnnouncementPanel)
		{
			MainMenu.Instance.m_AnnouncementPanel.UpdateMsgNumText();
		}
		if (!(null != base.transform.parent))
		{
			return;
		}
		AnnouncementGrid component = base.transform.parent.GetComponent<AnnouncementGrid>();
		if (!(null != component))
		{
			return;
		}
		PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		if (playerData != null)
		{
			if (!data.IsViewed)
			{
				playerData.m_AnnouncementCount--;
			}
			Dbg.LogEx("Decrease the announcement number in the red circle to : {0}", Dbg.NBLogType.COMMON_LOG, playerData.m_AnnouncementCount.ToString());
		}
		component.RemoveData(data);
		component.Remove(this);
	}
}
