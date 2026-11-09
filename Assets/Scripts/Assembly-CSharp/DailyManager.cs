using System;
using System.Collections;
using System.Collections.Generic;
using Models;
using ServiceCalls;
using UnityEngine;

public class DailyManager : SingletonComponent<DailyManager>
{
	public delegate void DailyInitCB();

	public List<DailyBonusData> DailyBonuses = new List<DailyBonusData>();

	private int m_todayIndex;

	public List<PromotionData> PromoDatas = new List<PromotionData>();

	public bool NeedToShowDailyBonus;

	public bool NeedToShowDialyBonusAfterTutorial;

	private bool needToShowPromo;

	public string UserJoinEventFromPromo = string.Empty;

	public List<PromoCodeData> m_promoCodeDatas;

	public BattleHelpData m_BattleHelpData = new BattleHelpData();

	public bool NeedToShowBattleHelp;

	public bool EnteredEventScreen;

	private Services m_services;

	public int TodayIndex
	{
		get
		{
			return m_todayIndex;
		}
		set
		{
			m_todayIndex = value;
			if (m_todayIndex > 4)
			{
				m_todayIndex = 4;
			}
			else if (m_todayIndex < 0)
			{
				m_todayIndex = 0;
			}
		}
	}

	public bool NeedToShowPromo
	{
		get
		{
			return needToShowPromo;
		}
		set
		{
			if (value != needToShowPromo)
			{
				needToShowPromo = value;
			}
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void InitManager()
	{
		m_services = SingletonComponent<Services>.Instance;
	}

	public int GetActivePromoCodeDataCount(bool p_onlyNewPromo = false)
	{
		if (m_promoCodeDatas == null || !GLOBAL.GAME_SETTINGS.enable_promo_code)
		{
			return 0;
		}
		int num = 0;
		int count = m_promoCodeDatas.Count;
		for (int i = 0; i < count; i++)
		{
			PromoCodeData promoCodeData = m_promoCodeDatas[i];
			if (promoCodeData != null && promoCodeData.IsPromoEnable() && (!p_onlyNewPromo || promoCodeData.isNew))
			{
				num++;
			}
		}
		return num;
	}

	public void UpdatePromoCodeData()
	{
		if (m_promoCodeDatas == null)
		{
			return;
		}
		int count = m_promoCodeDatas.Count;
		Dictionary<string, PromoCodeData> promoCodeData = GetPromoCodeData();
		if (promoCodeData != null)
		{
			for (int i = 0; i < count; i++)
			{
				PromoCodeData promoCodeData2 = m_promoCodeDatas[i];
				if (promoCodeData2 == null)
				{
					continue;
				}
				string iD = promoCodeData2.ID;
				if (promoCodeData.ContainsKey(iD))
				{
					PromoCodeData promoCodeData3 = promoCodeData[iD];
					int num = promoCodeData3.lastModifiedDate.CompareTo(promoCodeData2.lastModifiedDate);
					if (num >= 0)
					{
						promoCodeData2.isNew = false;
					}
				}
			}
		}
		SavePromoCodeData(m_promoCodeDatas);
	}

	public void SavePromoCodeData(List<PromoCodeData> p_dataList)
	{
		if (p_dataList == null)
		{
			return;
		}
		int num = MBPrefs.GetInt("DailyManager.m_promoCodeData.Count", 0);
		int count = p_dataList.Count;
		int num2 = count - num;
		if (num2 < 0)
		{
			for (int i = count; i < num; i++)
			{
				string key = "DailyManager.m_promoCodeData[" + i + "].id";
				string key2 = "DailyManager.m_promoCodeData[" + i + "].date";
				MBPrefs.DeleteKey(key);
				MBPrefs.DeleteKey(key2);
			}
		}
		MBPrefs.SetInt("DailyManager.m_promoCodeData.Count", count);
		for (int j = 0; j < count; j++)
		{
			PromoCodeData promoCodeData = p_dataList[j];
			if (promoCodeData != null && 0 == 0 && promoCodeData.isNew)
			{
				MBPrefs.SetString("DailyManager.m_promoCodeData[" + j + "].id", promoCodeData.ID);
				MBPrefs.SetString("DailyManager.m_promoCodeData[" + j + "].date", promoCodeData.lastModifiedDate.ToBinary().ToString());
			}
		}
	}

	public Dictionary<string, PromoCodeData> GetPromoCodeData()
	{
		Dictionary<string, PromoCodeData> dictionary = null;
		int num = MBPrefs.GetInt("DailyManager.m_promoCodeData.Count", 0);
		if (num <= 0)
		{
			return null;
		}
		dictionary = new Dictionary<string, PromoCodeData>();
		for (int i = 0; i < num; i++)
		{
			string text = MBPrefs.GetString("DailyManager.m_promoCodeData[" + i + "].id", string.Empty);
			long dateData = Convert.ToInt64(MBPrefs.GetString("DailyManager.m_promoCodeData[" + i + "].date", "0"));
			PromoCodeData promoCodeData = new PromoCodeData();
			promoCodeData.ID = text;
			promoCodeData.lastModifiedDate = DateTime.FromBinary(dateData);
			if (!dictionary.ContainsKey(text))
			{
				dictionary.Add(text, promoCodeData);
			}
		}
		return dictionary;
	}

	public void DailyPromotion()
	{
		m_services.DailyPromotion(m_services.NubeeId, m_services.DeviceId, OnDailyPromotionSuccess, OnDailyPromotionFailed);
	}

	private void OnDailyPromotionSuccess(Services.ServiceHandle p_handle)
	{
		int num = 0;
		string language = SingletonComponent<SaveData>.Instance.GameData.Language;
		foreach (PromotionData promoData in PromoDatas)
		{
			switch (language)
			{
			case "Japanese":
				promoData.ImgUrl = promoData.ImgUrlJp;
				promoData.ImgMd5 = promoData.ImgMd5Jp;
				break;
			case "Chinese_s":
				promoData.ImgUrl = promoData.ImgUrlChs;
				promoData.ImgMd5 = promoData.ImgMd5Chs;
				break;
			case "Chinese_t":
				promoData.ImgUrl = promoData.ImgUrlCht;
				promoData.ImgMd5 = promoData.ImgMd5Cht;
				break;
			case "Korean":
				promoData.ImgUrl = promoData.ImgUrlKr;
				promoData.ImgMd5 = promoData.ImgMd5Kr;
				break;
			default:
				promoData.ImgUrl = promoData.ImgUrlEn;
				promoData.ImgMd5 = promoData.ImgMd5En;
				break;
			}
			if (!string.IsNullOrEmpty(promoData.ImgUrl))
			{
				StartCoroutine(DownloadBackground(promoData.ImgUrl, num));
			}
			num++;
		}
		NeedToShowPromo = true;
	}

	private IEnumerator DownloadBackground(string p_url, int p_index)
	{
		WWW www = new WWW(p_url);
		Dbg.Log("[Info] Time: " + Time.realtimeSinceStartup + " Start to download texture: " + p_url);
		yield return www;
		if (www.error == string.Empty)
		{
			PromoDatas[p_index].TexFromWeb = www.texture;
			PromoDatas[p_index].TextureDownloaded = true;
		}
		Dbg.Log("[Info] Time: " + Time.realtimeSinceStartup + " Download end, texture: " + p_url + " errors : " + www.error);
	}

	private void OnDailyPromotionFailed(Services.ServiceHandle p_handle, string p_errMsg)
	{
		Dbg.LogWarn("[Warn] OnDailyPromotionFailed, err: " + ServiceCallUtils.GetErrorMessage(p_handle));
	}

	public void DailyBonus()
	{
		m_services.DailyLoginBonus(m_services.NubeeId, m_services.DeviceId, OnDailyBonusSuccess, OnDailyBonusFailed);
	}

	private void OnDailyBonusSuccess(Services.ServiceHandle p_handle)
	{
		List<DailyBonusData> list = (List<DailyBonusData>)p_handle.returnValue;
		if (list != null)
		{
			DailyBonuses = list;
			int num = 0;
			foreach (DailyBonusData dailyBonuse in DailyBonuses)
			{
				num++;
				if (dailyBonuse.Marked)
				{
					TodayIndex = num % 5;
				}
			}
		}
		NBProfiler.EndTask("server call daily bonus");
	}

	private void OnDailyBonusFailed(Services.ServiceHandle p_handle, string p_errMsg)
	{
		NeedToShowDailyBonus = false;
		Dbg.LogWarn("[Warn] OnDailyBonusFailed, err: " + ServiceCallUtils.GetErrorMessage(p_handle));
	}

	public void CheckLocalTimeFlagToShowDailyBonus()
	{
		string s = MBPrefs.GetString("Ms:LastShowDailyBonusTime", new DateTime(1981, 1, 11, 0, 0, 0).ToString());
		DateTime utcNow = DateTime.UtcNow;
		DateTime value = DateTime.Parse(s);
		TimeSpan timeSpan = utcNow.Subtract(value);
		Dbg.Log("[Info] Last show daily bonus window time: " + value.ToString() + " ,current time: " + utcNow.ToString() + " ,time span: " + timeSpan.ToString());
		if (value.Date == DateTime.Today)
		{
			Dbg.Log("[Info] No need to show daily bonus");
			NeedToShowDailyBonus = false;
		}
		else
		{
			NeedToShowDailyBonus = true;
			MBPrefs.SetString("Ms:LastShowDailyBonusTime", utcNow.ToString());
			Dbg.Log("[Info] Need to show daily bonus, drop time stamp : " + utcNow.ToString());
		}
	}

	public void DisplayReward(DailyBonusData p_data, ref UISprite p_sprite, ref NBUILocalize p_label)
	{
		switch (p_data.TypeId)
		{
		case 1:
			if (p_data.Item == "DODOBO_FEATHER")
			{
				p_sprite.spriteName = "Bonus_feather";
				p_label.Key = "header_dodobo_feather";
			}
			else if (p_data.Item == "DODOBO_EGG")
			{
				p_sprite.spriteName = "Bonus_egg";
				p_label.Key = "header_dodobo_egg";
			}
			else
			{
				p_sprite.enabled = false;
				p_label.enabled = false;
			}
			break;
		case 2:
		{
			int result2 = 0;
			if (int.TryParse(p_data.Item, out result2))
			{
				if (result2 > 500)
				{
					p_sprite.spriteName = "Bonus_gold02";
				}
				else
				{
					p_sprite.spriteName = "Bonus_gold01";
				}
				p_label.Key = "[G]" + result2;
			}
			else
			{
				p_sprite.enabled = false;
				p_label.enabled = false;
			}
			break;
		}
		case 4:
		{
			int result = 0;
			if (int.TryParse(p_data.Item, out result))
			{
				p_sprite.spriteName = "Bonus_diamond";
				p_label.Key = "[D]" + result;
			}
			else
			{
				p_sprite.enabled = false;
				p_label.enabled = false;
			}
			break;
		}
		default:
			Dbg.LogWarn("Unknown reward type : " + p_data.TypeId);
			p_sprite.enabled = false;
			p_label.enabled = false;
			break;
		}
		p_sprite.MakePixelPerfect();
	}
}
