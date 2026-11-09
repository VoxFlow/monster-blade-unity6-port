using System.Collections;
using UnityEngine;

public class PromotionWindow : NBUIGridCell<PromotionData>
{
	public UITexture Background;

	public InventoryCell[] InvCells;

	public PromoItemInfo PromoItemInformation;

	public UISprite ItemSetIcon;

	public UILabel ItemSetTitle;

	public UILabel ItemSetDesc;

	public UIButtonMessage ActionBtn;

	public EventCountdownTimer m_countdownTimer;

	private PromotionActionBtn PromoActionBtn;

	private BoxCollider m_collider;

	protected override void Awake()
	{
		base.Awake();
		UIPanel component = GetComponent<UIPanel>();
		PromoActionBtn = ActionBtn.GetComponent<PromotionActionBtn>();
		if (component != null)
		{
			Object.DestroyImmediate(component);
		}
		m_collider = GetComponent<BoxCollider>();
	}

	protected override void Start()
	{
		base.Start();
		if (ActionBtn != null && PromoActionBtn != null)
		{
			switch (data.PromoType.ToLower())
			{
			case "shop":
				ActionBtn.functionName = "OnGotoShopClick";
				PromoActionBtn.SetButtonIconShop();
				PromoActionBtn.SetBtnTextL10nKey("promotion_shop_sale");
				PromoActionBtn.OffsetBtnText(false);
				break;
			case "event":
				ActionBtn.functionName = "OnJointEvent";
				PromoActionBtn.EnableButtonIcon(false);
				PromoActionBtn.SetBtnTextL10nKey("promotion_join_event");
				PromoActionBtn.OffsetBtnText(false);
				break;
			case "url":
				ActionBtn.functionName = "OnUrlClick";
				PromoActionBtn.EnableButtonIcon(false);
				PromoActionBtn.SetBtnTextL10nKey("promotion_url");
				PromoActionBtn.OffsetBtnText(false);
				break;
			default:
			{
				ActionBtn.functionName = "OnGetItClick";
				PromoActionBtn.SetButtonIconGacha();
				PromoActionBtn.SetBtnTextL10nKey("promotion_get_it");
				PromoActionBtn.OffsetBtnText(false);
				InventoryCell[] invCells = InvCells;
				foreach (InventoryCell inventoryCell in invCells)
				{
					if (null != inventoryCell)
					{
						inventoryCell.gameObject.SetActiveRecursively(false);
					}
				}
				if (null != PromoItemInformation)
				{
					PromoItemInformation.gameObject.SetActiveRecursively(false);
				}
				if (null != ItemSetIcon)
				{
					ItemSetIcon.gameObject.SetActiveRecursively(false);
				}
				if (null != ItemSetTitle)
				{
					ItemSetTitle.gameObject.SetActiveRecursively(false);
				}
				if (null != ItemSetDesc)
				{
					ItemSetDesc.gameObject.SetActiveRecursively(false);
				}
				break;
			}
			}
		}
		if (null != m_countdownTimer && m_data != null)
		{
			long endTime = m_data.endTime;
			m_countdownTimer.SetEndTime(endTime);
			m_countdownTimer.StartTimer();
		}
	}

	public void EnablePromoItemInfo(bool p_enable)
	{
		if (null != PromoItemInformation)
		{
			PromoItemInformation.gameObject.SetActiveRecursively(p_enable);
		}
	}

	public void ExtendCollider(float p_width, float p_height)
	{
		if (m_collider != null)
		{
			m_collider.size = new Vector3(p_width, p_height, 1f);
		}
	}

	public void HideInventoryCells()
	{
		InventoryCell[] invCells = InvCells;
		foreach (InventoryCell inventoryCell in invCells)
		{
			if (null != inventoryCell)
			{
				inventoryCell.HideChildren();
				inventoryCell.gameObject.SetActiveRecursively(false);
			}
		}
	}

	public void LoadBackgroundTexture()
	{
		if (!data.ImgUrl.Trim().EndsWith("\\"))
		{
			int num = data.ImgUrl.LastIndexOf('/');
			if (num > 0)
			{
				string p_szFilename = data.ImgUrl.Substring(num + 1);
				NBCacheManager.Instance.GetFile(data.ImgUrl, "promo", p_szFilename, data.ImgMd5, DownloadPromoTextureCallback, null, null);
			}
			else
			{
				Dbg.LogError("filename not found in " + data.ImgUrl);
			}
		}
		else
		{
			Dbg.LogError("url ends with no filename: " + data.ImgUrl);
		}
	}

	public void DownloadPromoTextureCallback(bool p_bSuccessful, string p_szStatusMsg, byte[] p_byData, object p_arg1, object p_arg2)
	{
		if (!p_bSuccessful || p_byData == null)
		{
			Dbg.LogError("Unable to get promotion banner image");
			UIPopupController.Instance.CloseCurrentPopup();
			return;
		}
		Texture2D texture2D = new Texture2D(GLOBAL.PROMOTION_UI_WIDTH, GLOBAL.PROMOTION_UI_HEIGHT, TextureFormat.RGBA32, false, true);
		if (null == texture2D)
		{
			Dbg.LogError("unable to allocate memory for texture");
			UIPopupController.Instance.CloseCurrentPopup();
			return;
		}
		if (!texture2D.LoadImage(p_byData))
		{
			Dbg.LogError("unable to load texture from byte[]");
			UIPopupController.Instance.CloseCurrentPopup();
			return;
		}
		if (Background != null)
		{
			Shader shader = Shader.Find("Unlit/Transparent Colored");
			if (shader == null)
			{
				return;
			}
			Material material = new Material(shader);
			if (material == null)
			{
				return;
			}
			material.mainTexture = texture2D;
			Background.material = material;
		}
		SetupVisualByType();
	}

	private void SetupVisualByType()
	{
		switch (data.PromoType.ToLower())
		{
		case "shop":
			EnablePromoActionBtn(true);
			break;
		case "event":
			EnablePromoActionBtn(true);
			PromoActionBtn.EnableButtonIcon(false);
			break;
		case "url":
			EnablePromoActionBtn(true);
			PromoActionBtn.EnableButtonIcon(false);
			break;
		case "gacha":
			if (null != ItemSetTitle)
			{
				ItemSetTitle.text = SingletonComponent<NBLocalization>.Instance.Get(data.ItemSetTitleL10NKey);
			}
			if (null != ItemSetDesc)
			{
				ItemSetDesc.text = SingletonComponent<NBLocalization>.Instance.Get(data.ItemSetDescL10NKey);
			}
			SetInventoyCells();
			EnablePromoActionBtn(true);
			break;
		}
	}

	public void HideSetItemIcon()
	{
		if (null != ItemSetIcon)
		{
			ItemSetIcon.enabled = false;
		}
	}

	public void SetInventoyCells()
	{
		int num = 0;
		foreach (PromoItemData promoItem in data.PromoItems)
		{
			if (num >= InvCells.Length)
			{
				Dbg.LogWarn("Try to show more items than the available slots.");
				break;
			}
			Dbg.Log("[Info] SetInventoyCells(), itemid: " + promoItem.ItemId);
			ItemDetails itemDetails = null;
			if (!string.IsNullOrEmpty(promoItem.ItemId))
			{
				itemDetails = SingletonComponent<ItemManager>.Instance.CreateItemDetails(promoItem.ItemId);
				InvCells[num].gameObject.SetActiveRecursively(false);
				InvCells[num].data = itemDetails;
				InvCells[num].gameObject.active = true;
				if (null != PromoItemInformation)
				{
					if (PromoItemInformation.PromoItemMap.ContainsKey(promoItem.ItemId))
					{
						PromoItemInformation.PromoItemMap.Remove(promoItem.ItemId);
					}
					PromoItemInformation.PromoItemMap.Add(promoItem.ItemId, itemDetails);
					Dbg.Log("[Info] Add item : " + promoItem.ItemId + " to PromoItemInformation.PromoItemMap");
				}
			}
			if (num == 0)
			{
				if (null != PromoItemInformation)
				{
					PromoItemInformation.ShowItemInfo(promoItem.ItemId);
				}
				if (itemDetails != null)
				{
					SetItemSetIcon(itemDetails);
					InvCells[num].Selected = true;
				}
			}
			num++;
		}
	}

	private IEnumerator AssignBackgroundTexture()
	{
		float timeout = 5f;
		while (timeout > 0f && !data.TextureDownloaded)
		{
			timeout -= Time.deltaTime;
			yield return null;
		}
		if (!(data.TexFromWeb != null))
		{
			yield break;
		}
		Shader s = Shader.Find("Unlit/Transparent Colored");
		if (!(s == null))
		{
			Material m = new Material(s);
			if (!(m == null))
			{
				m.mainTexture = data.TexFromWeb;
				Background.material = m;
			}
		}
	}

	public void SetItemSetIcon(ItemDetails p_itemDetails)
	{
		if (!(null == ItemSetIcon) && p_itemDetails != null)
		{
			ItemSetIcon.atlas = p_itemDetails.BGAtlas;
			ItemSetIcon.spriteName = p_itemDetails.BGSpriteName;
		}
	}

	public void OnItemCellClick(GameObject p_sender)
	{
		InventoryCell component = p_sender.GetComponent<InventoryCell>();
		if (component != null)
		{
			InventoryCell[] invCells = InvCells;
			foreach (InventoryCell inventoryCell in invCells)
			{
				inventoryCell.Selected = false;
			}
			component.Selected = true;
			if (null != PromoItemInformation)
			{
				PromoItemInformation.ShowItemInfo(component.data.ItemID);
			}
		}
	}

	public void OnGetItClick()
	{
		UIPopupController.Instance.CloseCurrentPopup();
		if (!MainMenu.Instance.CheckExcessItem2() && SingletonComponent<GachaManager>.Instance != null && MainMenu.Instance != null)
		{
			SingletonComponent<GachaManager>.Instance.IsFromPromotion = true;
			MainMenu.Instance.StartLoadNextScreenWithFadeout(MainMenu.Menu.Treasure);
			MainMenu.Instance.ShowTopAndBottomBar(true);
		}
	}

	private void GotoEpicDirectly()
	{
		if (MainMenu.Instance.currScreen is GachaScreen)
		{
			GachaScreen gachaScreen = MainMenu.Instance.currScreen as GachaScreen;
			if (gachaScreen != null)
			{
				UIPopupController.Instance.ShowLoadingPopupNetWait();
				SingletonComponent<GachaManager>.Instance.RefreshGachaEvents(gachaScreen.OnRefreshPremiumGachaSuccess, gachaScreen.OnRefreshPremiumGachaFail);
			}
		}
	}

	public void OnJointEvent()
	{
		UIPopupController.Instance.CloseCurrentPopup();
		if (!MainMenu.Instance.CheckExcessItem2())
		{
			Dbg.LogEx("Try to go to dungeon event directly from promotion window", Dbg.NBLogType.COMMON_LOG);
			SingletonComponent<DailyManager>.Instance.UserJoinEventFromPromo = data.EventId;
			SingletonComponent<DailyManager>.Instance.EnteredEventScreen = true;
			MainMenu.Instance.GotoDungeonEventSelectionPanel();
		}
	}

	public void OnGotoShopClick()
	{
		UIPopupController.Instance.CloseCurrentPopup();
		if (!MainMenu.Instance.CheckExcessItem2())
		{
			MainMenu.Instance.StartLoadNextScreenWithFadeout(MainMenu.Menu.Shop);
		}
	}

	public void OnUrlClick()
	{
		Dbg.LogEx("Open URL :{0}", Dbg.NBLogType.COMMON_LOG, data.Url);
		if (!string.IsNullOrEmpty(data.Url))
		{
			Application.OpenURL(data.Url);
		}
	}

	public void EnablePromoActionBtn(bool p_enable)
	{
		if (PromoActionBtn != null)
		{
			PromoActionBtn.EnableBtnVisual(p_enable);
		}
	}
}
