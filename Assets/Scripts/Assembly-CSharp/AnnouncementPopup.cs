using Models;
using UnityEngine;

public class AnnouncementPopup : MonoBehaviour
{
	[SerializeField]
	private UILabel m_title;

	[SerializeField]
	private UILabel m_descBody;

	[SerializeField]
	private UILabel m_date;

	[SerializeField]
	private UIButtonMessage m_closeBtnMsg;

	public void SetInfo(AnnouncementData p_data)
	{
		m_title.text = "[*] " + p_data.AnnounceMessage.Title + " [*]";
		m_descBody.text = p_data.AnnounceMessage.Body;
		m_date.text = p_data.StartDate.Substring(0, p_data.StartDate.IndexOf(' '));
		m_closeBtnMsg.target = base.gameObject;
		m_closeBtnMsg.functionName = "OnClose";
	}

	private void OnClose()
	{
		UIPopupController.Instance.CloseCurrentPopup();
	}
}
