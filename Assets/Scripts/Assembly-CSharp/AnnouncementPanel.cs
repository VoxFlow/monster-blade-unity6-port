using Models;
using UnityEngine;

public class AnnouncementPanel : NBUIMenuPanel
{
	public AnnouncementGrid grid;

	public UIDraggablePanel scrollView;

	[SerializeField]
	private UILabel m_numOfMsg;

	[SerializeField]
	private UILabel m_noMsg;

	[SerializeField]
	private UISprite m_overlay;

	private void OnEnable()
	{
		UIPopupController.SetEnableLayer("UI", false);
		if (null != m_overlay)
		{
			m_overlay.gameObject.active = true;
			m_overlay.SendMessage("FadeIn", null, SendMessageOptions.DontRequireReceiver);
		}
		else
		{
			Dbg.LogWarn("[Warn] Unable to find \"Overlay\", please add the link to this prefab.");
		}
	}

	private void OnDisable()
	{
		if (null != grid)
		{
			grid.Clear();
		}
		SingletonComponent<AnnouncementManager>.Instance.GetNewAnnouncements();
		if (null != m_overlay)
		{
			m_overlay.SendMessage("FadeOut", null, SendMessageOptions.DontRequireReceiver);
		}
		else
		{
			Dbg.LogWarn("[Warn] Unable to find \"Overlay\", please add the link to this prefab.");
		}
		UIPopupController.SetEnableLayer("UI", true);
	}

	private void ShowAnnouncenments()
	{
		AnnounceAndFriendNotifyData data = SingletonComponent<AnnouncementManager>.Instance.Data;
		if (null != grid && data != null)
		{
			foreach (AnnouncementData value in data.Annoucements.Values)
			{
				grid.AddData(value);
				AnnouncementCell announcementCell = grid.AddCell(0f, 0f);
				announcementCell.ShowAnnouncementInfo();
			}
		}
		UpdateMsgNumText();
	}

	public void UpdateCells()
	{
		if (!(null != grid))
		{
			return;
		}
		foreach (AnnouncementCell cell in grid.cells)
		{
			cell.ShowAnnouncementInfo();
		}
	}

	public void UpdateMsgNumText()
	{
		if (!(null != m_numOfMsg))
		{
			return;
		}
		if (SingletonComponent<AnnouncementManager>.Instance.Data == null)
		{
			m_noMsg.gameObject.SetActiveRecursively(true);
			return;
		}
		m_numOfMsg.text = SingletonComponent<AnnouncementManager>.Instance.Data.Annoucements.Count + " " + SingletonComponent<NBLocalization>.Instance.Get("announcement_msg_num");
		if (SingletonComponent<AnnouncementManager>.Instance.Data.Annoucements.Count > 0)
		{
			m_noMsg.gameObject.SetActiveRecursively(false);
		}
		else
		{
			m_noMsg.gameObject.SetActiveRecursively(true);
		}
	}

	public void UpdateNewFlags()
	{
		if (null == grid)
		{
			return;
		}
		foreach (AnnouncementCell cell in grid.cells)
		{
			cell.UpdateNewTag();
		}
	}

	public override void PostShowPanel()
	{
		ShowAnnouncenments();
		if (null != scrollView)
		{
			scrollView.ResetPosition();
		}
	}

	public void OnCloseButtonClick()
	{
		if (null != MainMenu.Instance && null != MainMenu.Instance.currScreen)
		{
			Dbg.Log("[Info] Try to update announcement num in the home panel");
			MainMenu.Instance.Refresh();
		}
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_cancel");
		base.show = false;
	}
}
