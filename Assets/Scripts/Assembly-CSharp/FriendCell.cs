using UnityEngine;

public class FriendCell : NBUIGridCell<PlayerData>
{
	public enum CellType
	{
		FRIEND_LIST = 0,
		FRIEND_REQUEST = 1,
		FRIEND_GIFT = 2,
		FRIEND_SELECT = 3,
		FRIEND_FB_INVITE = 4,
		FRIEND_SINGLEPLAY = 5
	}

	public UISprite spritePlayerAvatar;

	public UITexture texturePlayerAvatar;

	public UILabel labelPlayerName;

	public UILabel labelPlayerLvl;

	public UILabel labelWeaponLvl;

	public UILabel labelStranger;

	public UILabel labelSkillName;

	public GameObject m_buttonShowDetails;

	public GameObject m_buttonRemove;

	public GameObject m_acceptRequest;

	public GameObject m_ignoreRequest;

	public GameObject m_friendInfoObj;

	public GameObject m_fbInviteObj;

	public GameObject m_singlePlayObj;

	public UISprite skillSprite;

	public UISprite[] m_stars;

	public Transform starsTemplatePosition;

	private FriendCellEventType m_eventType = FriendCellEventType.MAX;

	public Component eventTarget;

	public UISprite[] skillSprites = new UISprite[5];

	private CellType _cellType;

	public CellType cellType
	{
		get
		{
			return _cellType;
		}
		set
		{
			_cellType = value;
			UpdateCellType();
		}
	}

	public override PlayerData data
	{
		set
		{
			m_data = value;
			if (m_data == null)
			{
				return;
			}
			if (m_data.Name.Length > 10)
			{
				labelPlayerName.text = m_data.Name.Substring(0, 10) + "...";
			}
			else
			{
				labelPlayerName.text = m_data.Name;
			}
			labelPlayerLvl.text = "LVL " + m_data.Level;
			if (m_data.Equipments.Length > 0)
			{
				ItemDetails itemDetails = m_data.Equipments[4].itemDetails;
				if (m_data.IsEquipingFullSet(GLOBAL.DARK_WOLF_SETNAME))
				{
					skillSprite.spriteName = GLOBAL.DARKWOLF_SKILL_ICON;
				}
				else
				{
					int assistAction = (int)itemDetails.skill.assistAction;
					if (0 <= assistAction && assistAction < skillSprites.Length)
					{
						skillSprite.spriteName = skillSprites[assistAction].spriteName;
					}
				}
				labelWeaponLvl.text = "LVL " + itemDetails.Level;
				if (itemDetails.Level == 0)
				{
					labelWeaponLvl.text = string.Empty;
				}
				if (starsTemplatePosition != null && m_stars != null)
				{
					SetTier(itemDetails);
				}
			}
			Equipment.Data equipmentData = m_data.GetEquipmentData(Equipment.Part.Weapon);
			string text = SingletonComponent<NBLocalization>.Instance.Get("gacha_item_no_skill_label");
			if (equipmentData != null && equipmentData.itemDetails != null && equipmentData.itemDetails.skill != null && equipmentData.itemDetails.skill.assistAction != SkillEnum.AssistSkill.None && equipmentData.itemDetails.skill.visual.skill != ItemEnum.Skill.WEAPON_NONE)
			{
				SkillVisual visual = equipmentData.itemDetails.skill.visual;
				text = SingletonComponent<NBLocalization>.Instance.Get(visual.label_assist);
			}
			if (labelSkillName != null)
			{
				labelSkillName.text = text;
			}
			NBUILocalize component = labelStranger.gameObject.GetComponent<NBUILocalize>();
			if ((bool)component)
			{
				if (m_data.IsFriend)
				{
					component.key = "friendlist_friend";
				}
				else
				{
					component.key = "friendlist_stranger";
				}
			}
			texturePlayerAvatar.enabled = false;
			if (m_data.m_userData.fbId != null && m_data.m_userData.fbId != string.Empty)
			{
				SetFBPicture(m_data.m_userData.fbId);
			}
		}
	}

	private void SetFBPicture(string fbId)
	{
		Texture facebookPictureByIDAsTexture = SingletonComponent<SocialNetworkManager>.Instance.GetFacebookPictureByIDAsTexture(fbId);
		if (facebookPictureByIDAsTexture == null)
		{
			return;
		}
		Shader shader = Shader.Find("Unlit/Transparent Colored");
		if (!(shader == null))
		{
			Material material = new Material(shader);
			if (!(material == null) && !(texturePlayerAvatar == null))
			{
				spritePlayerAvatar.enabled = false;
				material.mainTexture = facebookPictureByIDAsTexture;
				texturePlayerAvatar.material = material;
				texturePlayerAvatar.enabled = true;
			}
		}
	}

	private void SetTier(ItemDetails p_item)
	{
		if (p_item == null || p_item == ItemManager.s_NULL_DATA)
		{
			SetStarsEnable(false);
			Dbg.LogWarn("TiersStars : Cannot display this item.");
			return;
		}
		if (p_item.Level == 0)
		{
			SetStarsEnable(false);
			return;
		}
		int tier = p_item.Tier;
		int p_maxTier = p_item.equipment.maxTier;
		if (p_item.IsEssence)
		{
			p_maxTier = tier;
		}
		SetTier(tier, p_maxTier);
	}

	private void SetTier(int p_tier, int p_maxTier)
	{
		if (p_tier < 0 || p_maxTier < 0 || p_tier > p_maxTier || p_tier > TierRange.TIER_MAX || p_maxTier > TierRange.TIER_MAX)
		{
			Dbg.LogError("TIERS_STARS : Hide tier. Wrong tier number given : " + p_tier + " " + p_maxTier);
			SetStarsEnable(false);
			return;
		}
		SetStarsEnable(true);
		int i;
		for (i = 0; i < p_tier; i++)
		{
			m_stars[i].spriteName = "Icons/star";
		}
		for (; i < p_maxTier; i++)
		{
			m_stars[i].spriteName = "Icons/no_star";
		}
		for (; i < m_stars.Length; i++)
		{
			m_stars[i].gameObject.SetActiveRecursively(false);
		}
	}

	private void SetStarsEnable(bool p_enable)
	{
		UISprite[] stars = m_stars;
		foreach (UISprite uISprite in stars)
		{
			uISprite.gameObject.SetActiveRecursively(p_enable);
		}
	}

	private void OnButtonClick(GameObject source)
	{
		m_eventType = FriendCellEventType.MAX;
		if (source == m_buttonShowDetails)
		{
			m_eventType = FriendCellEventType.FRIEND_INFO;
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_ok");
		}
		else if (source == m_buttonRemove)
		{
			m_eventType = FriendCellEventType.FRIEND_REMOVE;
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_cancel");
		}
		else if (source == m_acceptRequest)
		{
			m_eventType = FriendCellEventType.FRIEND_ACCEPT;
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_ok");
		}
		else if (source == m_ignoreRequest)
		{
			m_eventType = FriendCellEventType.FRIEND_REJECT;
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_cancel");
		}
		else
		{
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_ok");
			m_eventType = FriendCellEventType.FRIEND_SELECT;
			if (cellType == CellType.FRIEND_FB_INVITE)
			{
				m_eventType = FriendCellEventType.FRIEND_INVITE;
			}
			else
			{
				if (cellType == CellType.FRIEND_GIFT)
				{
					m_eventType = FriendCellEventType.FRIEND_GIFT;
				}
				UIPopupController.Instance.DisableUiExceptLayerMask("PopUpLayer");
			}
		}
		if (m_eventType != FriendCellEventType.MAX)
		{
			SendEvent();
		}
	}

	private void OnClosePopup()
	{
		UIPopupController.Instance.EnableUI();
	}

	private void SendEvent()
	{
		if (m_eventType != FriendCellEventType.MAX)
		{
			if (data == null && m_eventType != FriendCellEventType.FRIEND_INVITE)
			{
				UIPopupController.Instance.ShowOKPopupMSG("common_error", "social_prompt_nofriend_details", OnClosePopup);
			}
			else if ((bool)eventTarget)
			{
				FriendCellEvent friendCellEvent = new FriendCellEvent();
				friendCellEvent.type = m_eventType;
				friendCellEvent.cell = this;
				eventTarget.SendMessage("OnFriendCellEvent", friendCellEvent, SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	private void UpdateCellType()
	{
		switch (cellType)
		{
		case CellType.FRIEND_SELECT:
			m_buttonShowDetails.SetActiveRecursively(true);
			m_buttonRemove.SetActiveRecursively(false);
			m_ignoreRequest.SetActiveRecursively(false);
			m_acceptRequest.SetActiveRecursively(false);
			m_fbInviteObj.SetActiveRecursively(false);
			m_singlePlayObj.SetActiveRecursively(false);
			break;
		case CellType.FRIEND_LIST:
			m_buttonShowDetails.SetActiveRecursively(false);
			m_buttonRemove.SetActiveRecursively(false);
			m_ignoreRequest.SetActiveRecursively(false);
			m_acceptRequest.SetActiveRecursively(false);
			m_fbInviteObj.SetActiveRecursively(false);
			m_singlePlayObj.SetActiveRecursively(false);
			break;
		case CellType.FRIEND_REQUEST:
			m_buttonShowDetails.SetActiveRecursively(false);
			m_buttonRemove.SetActiveRecursively(false);
			m_ignoreRequest.SetActiveRecursively(false);
			m_acceptRequest.SetActiveRecursively(true);
			m_fbInviteObj.SetActiveRecursively(false);
			m_singlePlayObj.SetActiveRecursively(false);
			break;
		case CellType.FRIEND_GIFT:
			m_buttonShowDetails.SetActiveRecursively(false);
			m_buttonRemove.SetActiveRecursively(false);
			m_ignoreRequest.SetActiveRecursively(false);
			m_acceptRequest.SetActiveRecursively(false);
			m_fbInviteObj.SetActiveRecursively(false);
			m_singlePlayObj.SetActiveRecursively(false);
			break;
		case CellType.FRIEND_FB_INVITE:
			m_buttonShowDetails.SetActiveRecursively(false);
			m_buttonRemove.SetActiveRecursively(false);
			m_ignoreRequest.SetActiveRecursively(false);
			m_acceptRequest.SetActiveRecursively(false);
			m_fbInviteObj.SetActiveRecursively(true);
			m_singlePlayObj.SetActiveRecursively(false);
			m_friendInfoObj.SetActiveRecursively(false);
			break;
		case CellType.FRIEND_SINGLEPLAY:
			m_buttonShowDetails.SetActiveRecursively(false);
			m_buttonRemove.SetActiveRecursively(false);
			m_ignoreRequest.SetActiveRecursively(false);
			m_acceptRequest.SetActiveRecursively(false);
			m_fbInviteObj.SetActiveRecursively(false);
			m_singlePlayObj.SetActiveRecursively(true);
			m_friendInfoObj.SetActiveRecursively(false);
			break;
		}
	}
}
