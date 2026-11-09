using System;
using System.Collections.Generic;
using UnityEngine;

public class PromotionPopup : MonoBehaviour
{
	public PromotionPopupGrid m_grid;

	public UIDraggablePanel m_scrollView;

	[SerializeField]
	private UIAnchor m_closeBtnUiAnchor;

	[SerializeField]
	private UISprite m_pageDotTemplate;

	[SerializeField]
	private float dotGap = 50f;

	[SerializeField]
	private float dotTop = -280f;

	private List<UISprite> pageDots = new List<UISprite>();

	private int curPage;

	private bool isSnapped = true;

	private bool m_release;

	private bool m_showOnlyGacha;

	private float m_screenWidth = 960f;

	private float m_screenHeight = 640f;

	public bool ShowOnlyGacha
	{
		get
		{
			return m_showOnlyGacha;
		}
		set
		{
			m_showOnlyGacha = value;
		}
	}

	private void Awake()
	{
		m_screenWidth = Screen.width;
		m_screenHeight = Screen.height;
		m_scrollView.scale = Vector3.right;
		m_grid.transform.localScale = Vector3.one;
		UIEventListener uIEventListener = UIEventListener.Get(m_scrollView.gameObject);
		uIEventListener.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onPress, new UIEventListener.BoolDelegate(OnPressed));
		UIPanel component = m_scrollView.GetComponent<UIPanel>();
		m_screenWidth = (float)Screen.width * GLOBAL.ORTHO_RATIO / GLOBAL.UI_SCALE_FACTOR;
		component.clipRange = new Vector4(0f, 0f, m_screenWidth, GLOBAL.PROMOTION_UI_HEIGHT);
		m_grid.hSpace = 0f;
		m_closeBtnUiAnchor.uiCamera = UIPopupController.Instance.GetPopupCamera();
	}

	protected void OnPressed(GameObject source, bool isPressed)
	{
		m_release = !isPressed;
		if (m_release)
		{
			float num = m_scrollView.transform.localPosition.x - m_screenWidth / 2f;
			curPage = Mathf.FloorToInt(num / (0f - m_screenWidth));
			if (curPage < 0)
			{
				curPage = 0;
			}
			if (curPage >= pageDots.Count)
			{
				curPage = pageDots.Count - 1;
			}
			Dbg.Log("[Info] Page: " + curPage + " , current scroll view pos: " + m_scrollView.transform.localPosition.x + " ,target pos: " + (float)curPage * (0f - m_screenWidth));
			isSnapped = false;
		}
		else
		{
			isSnapped = true;
		}
	}

	public void SetupDisplay()
	{
		if (m_grid != null)
		{
			int num = 0;
			foreach (PromotionData promoData in SingletonComponent<DailyManager>.Instance.PromoDatas)
			{
				if (ShowOnlyGacha && promoData.PromoType != "gacha")
				{
					Dbg.Log("Only need to show gacha type promotion banner, current one is : " + promoData.PromoType);
				}
				else if (promoData.PromoType == "gacha" || promoData.PromoType == "event" || promoData.PromoType == "shop" || promoData.PromoType == "url")
				{
					m_grid.AddData(promoData);
					float screenWidth = m_screenWidth;
					PromotionWindow promotionWindow = m_grid.AddCell(screenWidth, 0f);
					promotionWindow.ExtendCollider(screenWidth, m_screenHeight * 0.65f);
					if (promoData.PromoType == "gacha")
					{
						promotionWindow.EnablePromoActionBtn(false);
						promotionWindow.HideInventoryCells();
						promotionWindow.LoadBackgroundTexture();
					}
					else if (promoData.PromoType == "event")
					{
						promotionWindow.EnablePromoActionBtn(false);
						promotionWindow.HideInventoryCells();
						promotionWindow.HideSetItemIcon();
						promotionWindow.EnablePromoItemInfo(false);
						promotionWindow.LoadBackgroundTexture();
					}
					else if (promoData.PromoType == "shop")
					{
						promotionWindow.EnablePromoActionBtn(false);
						promotionWindow.HideInventoryCells();
						promotionWindow.HideSetItemIcon();
						promotionWindow.EnablePromoItemInfo(false);
						promotionWindow.LoadBackgroundTexture();
					}
					else if (promoData.PromoType == "url")
					{
						promotionWindow.EnablePromoActionBtn(false);
						promotionWindow.HideInventoryCells();
						promotionWindow.HideSetItemIcon();
						promotionWindow.LoadBackgroundTexture();
						promotionWindow.EnablePromoItemInfo(false);
					}
					GameObject gameObject = UnityEngine.Object.Instantiate(m_pageDotTemplate.gameObject, Vector3.zero, Quaternion.identity) as GameObject;
					gameObject.transform.parent = base.transform;
					gameObject.transform.localPosition = new Vector3((float)(-(SingletonComponent<DailyManager>.Instance.PromoDatas.Count - 1)) * dotGap / 2f + (float)num * dotGap, dotTop, -25f);
					UISprite component = gameObject.GetComponent<UISprite>();
					pageDots.Add(component);
					num++;
				}
				else
				{
					Dbg.LogEx("Unknown or not handled promotype : {0}", Dbg.NBLogType.COMMON_LOG, promoData.PromoType);
				}
			}
		}
		else
		{
			Dbg.LogWarn("[Warn] Unable to find grid under the PromotionPopup");
		}
		UpdatePageDotStatus(curPage);
	}

	private void Start()
	{
		SetupDisplay();
	}

	private void UpdatePageDotStatus(int p_curPage)
	{
		foreach (UISprite pageDot in pageDots)
		{
			pageDot.color = new Color(9f / 32f, 9f / 32f, 9f / 32f, 1f);
		}
		if (p_curPage < pageDots.Count)
		{
			pageDots[p_curPage].color = Color.white;
		}
	}

	private void Update()
	{
		if (!isSnapped)
		{
			float num = (float)curPage * (0f - m_screenWidth);
			if (m_scrollView.transform.localPosition.x < num - 20f)
			{
				m_scrollView.currentMomentum = new Vector3(15f, 0f, 0f);
				UpdatePageDotStatus(curPage);
			}
			else if (m_scrollView.transform.localPosition.x > num + 20f)
			{
				m_scrollView.currentMomentum = new Vector3(-15f, 0f, 0f);
				UpdatePageDotStatus(curPage);
			}
			else
			{
				m_scrollView.currentMomentum = Vector3.zero;
				isSnapped = true;
				UpdatePageDotStatus(curPage);
			}
		}
	}
}
