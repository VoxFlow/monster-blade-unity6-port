using Models;
using UnityEngine;

public class AnnouncementItemPopup : MonoBehaviour
{
	[SerializeField]
	private UILabel m_title;

	[SerializeField]
	private UITexture m_itemIcon;

	private UILabel m_descBody;

	[SerializeField]
	private NBUILocalize m_buttonText;

	[SerializeField]
	private UIButton m_huntOrClaimBtn;

	[SerializeField]
	private UIButtonMessage m_huntOrClaimBtnMsg;

	[SerializeField]
	private UIButtonMessage m_closeBtnMsg;

	[SerializeField]
	private UISprite m_huntOrClaimSprite;

	private AnnouncementData m_data;

	public Transform _DescPanelTex;

	public UILabel _descTex;

	public Transform _DescPanelPic;

	public UILabel _descPic;

	public Transform _backGndTitle;

	private void ShowCtrol(Transform t, bool show)
	{
		Vector3 localPosition = t.localPosition;
		localPosition.x = ((!show) ? 100000 : 0);
		t.localPosition = localPosition;
	}

	private void SetNoGreenBtn(bool NoGreenBtn)
	{
		ShowCtrol(_DescPanelTex, NoGreenBtn);
		ShowCtrol(m_huntOrClaimBtn.transform, !NoGreenBtn);
		ShowCtrol(_DescPanelPic, !NoGreenBtn);
		m_descBody = ((!NoGreenBtn) ? _descPic : _descTex);
	}

	public void SetInfo(AnnouncementData p_data)
	{
		m_data = p_data;
		if (!string.IsNullOrEmpty(p_data.AnnounceMessage.ImgUrl) && null != SingletonComponent<AnnouncementManager>.Instance)
		{
			SingletonComponent<AnnouncementManager>.Instance.DownloadTexture(p_data.AnnounceMessage.ImgUrl, p_data.AnnounceMessage.ImgMd5, DownloadPromoTextureCallback);
		}
		SetNoGreenBtn(p_data.AnnTypeId == AnnoucementTypeId.TEXT);
		m_closeBtnMsg.target = base.gameObject;
		m_closeBtnMsg.functionName = "OnClose";
		m_huntOrClaimBtnMsg.target = base.gameObject;
		m_title.text = p_data.AnnounceMessage.Title;
		m_descBody.text = p_data.AnnounceMessage.Body;
		switch (p_data.AnnTypeId)
		{
		case AnnoucementTypeId.GACHA:
			m_buttonText.Key = "announcement_gacha";
			m_huntOrClaimBtnMsg.functionName = "GoToGacha";
			break;
		case AnnoucementTypeId.EVENT:
			m_buttonText.Key = "announcement_hunt";
			m_huntOrClaimBtnMsg.functionName = "GoToEvent";
			break;
		case AnnoucementTypeId.CLAIM:
		case AnnoucementTypeId.TEXT:
			if (p_data.AnnouncementRewards != null && p_data.AnnouncementRewards.Count > 0 && !SingletonComponent<AnnouncementManager>.Instance.HasClaimed(p_data))
			{
				if (null != m_huntOrClaimSprite)
				{
					m_huntOrClaimSprite.spriteName = "buttons/green_btn";
				}
				if (null != m_huntOrClaimBtn)
				{
					m_huntOrClaimBtn.isEnabled = true;
				}
				m_buttonText.Key = "achievement_claim";
				m_huntOrClaimBtnMsg.functionName = "ClaimReward";
			}
			else
			{
				m_buttonText.Key = "achievement_claimed";
				if (null != m_huntOrClaimSprite)
				{
					m_huntOrClaimSprite.spriteName = "buttons/grey_btn";
				}
				if (null != m_huntOrClaimBtn)
				{
					m_huntOrClaimBtn.isEnabled = false;
				}
			}
			break;
		case AnnoucementTypeId.SOCIAL:
			Dbg.LogError("Should no this type yet, type: " + p_data.AnnTypeId);
			break;
		case AnnoucementTypeId.TREASURE:
			Dbg.LogError("Should no this type yet, type: " + p_data.AnnTypeId);
			break;
		}
	}

	private void GoToGacha()
	{
		UIPopupController.Instance.CloseCurrentPopup();
		SingletonComponent<DailyManager>.Instance.EnteredEventScreen = true;
		if (null != MainMenu.Instance.m_AnnouncementPanel)
		{
			MainMenu.Instance.m_AnnouncementPanel.OnCloseButtonClick();
		}
		MainMenu.Instance.StartLoadNextScreenWithFadeout(MainMenu.Menu.Treasure);
	}

	private void GoToEvent()
	{
		SingletonComponent<DailyManager>.Instance.UserJoinEventFromPromo = m_data.EventId;
		UIPopupController.Instance.CloseCurrentPopup();
		SingletonComponent<DailyManager>.Instance.EnteredEventScreen = true;
		if (null != MainMenu.Instance)
		{
			if (null != MainMenu.Instance.m_AnnouncementPanel)
			{
				MainMenu.Instance.m_AnnouncementPanel.OnCloseButtonClick();
			}
			MainMenu.Instance.GotoDungeonEventSelectionPanel();
		}
	}

	private void ClaimReward()
	{
		if (null != SingletonComponent<AnnouncementManager>.Instance)
		{
			Dbg.LogEx("Try to claim the item of announcement, id: {0}", Dbg.NBLogType.COMMON_LOG, m_data.DbId.ToString());
			SingletonComponent<AnnouncementManager>.Instance.ClaimAnnouncementReward(m_data);
		}
	}

	private void OnClose()
	{
		UIPopupController.Instance.CloseCurrentPopup();
	}

	private void DownloadPromoTextureCallback(bool p_bSuccessful, string p_szStatusMsg, byte[] p_byData, object p_arg1, object p_arg2)
	{
		if (!p_bSuccessful || p_byData == null)
		{
			Dbg.LogError("Unable to get image");
			return;
		}
		Texture2D texture2D = new Texture2D(GLOBAL.PROMOTION_UI_WIDTH, GLOBAL.PROMOTION_UI_HEIGHT, TextureFormat.RGBA32, false, true);
		if (null == texture2D)
		{
			Dbg.LogError("unable to allocate memory for texture");
		}
		else if (!texture2D.LoadImage(p_byData))
		{
			Dbg.LogError("unable to load texture from byte[]");
		}
		else
		{
			if (!(null != m_itemIcon))
			{
				return;
			}
			Shader shader = Shader.Find("Unlit/Transparent Colored");
			if (!(null == shader))
			{
				Material material = new Material(shader);
				if (!(null == material))
				{
					material.mainTexture = texture2D;
					m_itemIcon.material = material;
				}
			}
		}
	}
}
