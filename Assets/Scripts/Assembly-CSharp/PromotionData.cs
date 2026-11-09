using System;
using System.Collections.Generic;
using UnityEngine;

public class PromotionData
{
	public int Id;

	public string PromoType = string.Empty;

	public string Name = string.Empty;

	public string EventId = string.Empty;

	public DateTime StartDateTime = default(DateTime);

	public DateTime EndDateTime = default(DateTime);

	public long endTime;

	public string ImgUrl = string.Empty;

	public string ImgMd5 = string.Empty;

	public string ImgUrlEn = string.Empty;

	public string ImgMd5En = string.Empty;

	public string ImgUrlJp = string.Empty;

	public string ImgMd5Jp = string.Empty;

	public string ImgUrlCht = string.Empty;

	public string ImgMd5Cht = string.Empty;

	public string ImgUrlChs = string.Empty;

	public string ImgMd5Chs = string.Empty;

	public string ImgUrlKr = string.Empty;

	public string ImgMd5Kr = string.Empty;

	public string Url = string.Empty;

	public List<PromoItemData> PromoItems = new List<PromoItemData>();

	public string ItemSetTitleL10NKey = string.Empty;

	public string ItemSetDescL10NKey = string.Empty;

	public UISprite BackgroundSprite;

	public Texture2D TexFromWeb = new Texture2D(GLOBAL.PROMOTION_UI_WIDTH, GLOBAL.PROMOTION_UI_HEIGHT, TextureFormat.ARGB4444, false);

	public bool TextureDownloaded;

	public override string ToString()
	{
		return string.Format("PromotionData Id: {0}, PromoType: {1}, Name: {2}, EventId: {3}, StartDateTime: {4}, EndDateTime: {5}, ImgUrl: {6}, Items: {7}", Id, PromoType, Name, EventId, StartDateTime.ToString(), EndDateTime.ToString(), ImgUrl, PromoItemsToString());
	}

	public string PromoItemsToString()
	{
		string text = string.Empty;
		foreach (PromoItemData promoItem in PromoItems)
		{
			text = text + promoItem.ToString() + " ";
		}
		return text;
	}
}
