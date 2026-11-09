using System.Collections;
using Models;
using UnityEngine;

public class DailyBonusCell : MonoBehaviour
{
	public UISprite Background;

	public UISprite DailyItem;

	public UILabel DateIndex;

	public UILabel ItemAmount;

	public UISprite SelectedFrame;

	public UISprite Stamp;

	public UISprite SmallStamp;

	public UILabel LoginedNum;

	public UISprite DoubleLootTextPic;

	public DailyBonusData m_data;

	public DailyBonusPopup m_DailyBonusWinRef;

	public bool IsAnimatingStamp;

	public UIAtlas m_DailyBonusAtlas;

	private void Awake()
	{
		UISprite componentInChildren = MBHier.GetComponentInChildren<UISprite>(this);
		m_DailyBonusAtlas = componentInChildren.atlas;
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void OnCellBtnClick()
	{
		if (m_data != null)
		{
			Dbg.Log("[Info] Click on cell, data: " + m_data.ToString());
			int result = 0;
			if (string.IsNullOrEmpty(m_data.Item))
			{
				switch (m_data.TypeId)
				{
				case 5:
					if (m_data.Marked && !string.IsNullOrEmpty(m_data.MyItemData.ItemId))
					{
						ItemDetails item = SingletonComponent<ItemManager>.Instance.CreateItemDetails(m_data.MyItemData.ItemId, m_data.MyItemData.ItemTier, m_data.MyItemData.ItemLevel);
						UIPopupController.Instance.ShowItemStats(item);
					}
					else
					{
						UIPopupController.Instance.ShowSocialOrPremGacha(true, m_DailyBonusWinRef.m_TreasureAtlas);
					}
					break;
				case 6:
					UIPopupController.Instance.ShowSocialOrPremGacha(false, m_DailyBonusWinRef.m_TreasureAtlas);
					break;
				case 7:
					UIPopupController.Instance.ShowDoubleXpOrLoot(true, m_DailyBonusAtlas);
					break;
				case 8:
					UIPopupController.Instance.ShowDoubleXpOrLoot(false, m_DailyBonusAtlas);
					break;
				default:
					Dbg.LogWarnEx("[Warn] Unexpected type id: {0}", Dbg.NBLogType.COMMON_WARNING, m_data.TypeId.ToString());
					break;
				}
			}
			else if (int.TryParse(m_data.Item, out result))
			{
				UIPopupController.Instance.ShowHuntingIgc(result);
			}
			else
			{
				ItemDetails item2 = SingletonComponent<InventoryManager>.Instance.CreateItem(m_data.Item);
				UIPopupController.Instance.ShowItemStats(item2);
			}
		}
		else
		{
			Dbg.LogWarn("[Warn] No data on this cell");
		}
	}

	public IEnumerator AnimStamp(PopupCallback p_callback)
	{
		Stamp.gameObject.SetActiveRecursively(true);
		ItemAmount.gameObject.SetActiveRecursively(false);
		IsAnimatingStamp = true;
		Vector3 oriLocalScale = Stamp.transform.localScale;
		float tempScale = 6f;
		Stamp.transform.localScale = oriLocalScale * tempScale;
		while (tempScale > 0.8f)
		{
			tempScale -= Time.deltaTime * 9f;
			Stamp.transform.localScale = oriLocalScale * tempScale;
			yield return null;
		}
		while (tempScale < 1.1f)
		{
			tempScale += Time.deltaTime * 4f;
			Stamp.transform.localScale = oriLocalScale * tempScale;
			yield return null;
		}
		while (tempScale > 1f)
		{
			tempScale -= Time.deltaTime * 5f;
			Stamp.transform.localScale = oriLocalScale * tempScale;
			yield return null;
		}
		Stamp.transform.localScale = oriLocalScale;
		yield return new MBWaitForSeconds(0.5f);   // PORT (CD-059)
		IsAnimatingStamp = false;
		if (p_callback != null)
		{
			p_callback();
		}
	}

	public void SetDailyBonusData(DailyBonusData p_data)
	{
		m_data = p_data;
		Stamp.gameObject.SetActiveRecursively(p_data.Marked);
		ItemAmount.gameObject.SetActiveRecursively(!p_data.Marked);
		if (p_data.Sequence == SingletonComponent<DailyManager>.Instance.TodayIndex)
		{
			Stamp.gameObject.SetActiveRecursively(false);
			SelectedFrame.gameObject.SetActiveRecursively(true);
			Background.spriteName = "selected_day_bg";
			ItemAmount.gameObject.SetActiveRecursively(true);
		}
		else
		{
			SelectedFrame.gameObject.SetActiveRecursively(false);
		}
		if (p_data.Sequence % 7 == 0)
		{
			Background.spriteName = "weekend_bg";
			DoubleLootTextPic.gameObject.SetActiveRecursively(true);
			LoginedNum.gameObject.SetActiveRecursively(true);
			LoginedNum.text = p_data.Progress + "/" + p_data.RequireLogins;
			if (p_data.Progress > 0)
			{
				LoginedNum.color = new Color(0f, 0.6745098f, 0f);
			}
			SmallStamp.gameObject.SetActiveRecursively(true);
			SmallStamp.transform.localPosition = new Vector3(38f, 33f, -4f);
			SmallStamp.transform.localScale = new Vector3(30f, 28f, 1f);
		}
		else
		{
			SmallStamp.gameObject.SetActiveRecursively(false);
			LoginedNum.gameObject.SetActiveRecursively(false);
			DoubleLootTextPic.gameObject.SetActiveRecursively(false);
		}
		DateIndex.text = p_data.Sequence.ToString();
		switch (p_data.TypeId)
		{
		case 1:
			if (!string.IsNullOrEmpty(p_data.Item))
			{
				DailyItem.spriteName = p_data.Item;
				ItemAmount.text = "1";
			}
			break;
		case 2:
		{
			int result = 0;
			if (int.TryParse(p_data.Item, out result))
			{
				if (result <= 500)
				{
					DailyItem.spriteName = "500_coin";
				}
				else if (result > 500 && result <= 750)
				{
					DailyItem.spriteName = "750_coin";
				}
				else if (result > 750 && result <= 1000)
				{
					DailyItem.spriteName = "1000_coin";
				}
				else
				{
					DailyItem.spriteName = "1250";
				}
			}
			ItemAmount.text = result.ToString();
			break;
		}
		case 3:
			break;
		case 4:
			break;
		case 5:
			DailyItem.spriteName = "mystery_item";
			ItemAmount.text = string.Empty;
			break;
		case 6:
			DailyItem.spriteName = "double_loot";
			DoubleLootTextPic.gameObject.SetActiveRecursively(false);
			ItemAmount.text = string.Empty;
			break;
		case 7:
			DailyItem.spriteName = "double_xp";
			ItemAmount.text = string.Empty;
			break;
		case 8:
			DailyItem.spriteName = "double_loot";
			ItemAmount.text = string.Empty;
			break;
		default:
			DailyItem.gameObject.SetActiveRecursively(false);
			break;
		}
	}

	public void SetBrightness(float p_bright)
	{
		Background.color = new Color(Background.color.r * p_bright, Background.color.g * p_bright, Background.color.b * p_bright, 1f);
		DailyItem.color = new Color(DailyItem.color.r * p_bright, DailyItem.color.g * p_bright, DailyItem.color.b * p_bright, 1f);
		SelectedFrame.color = new Color(SelectedFrame.color.r * p_bright, SelectedFrame.color.g * p_bright, SelectedFrame.color.b * p_bright, 1f);
		Stamp.color = new Color(Stamp.color.r * p_bright, Stamp.color.g * p_bright, Stamp.color.b * p_bright, 1f);
		SmallStamp.color = new Color(SmallStamp.color.r * p_bright, SmallStamp.color.g * p_bright, SmallStamp.color.b * p_bright, 1f);
		DoubleLootTextPic.color = new Color(DoubleLootTextPic.color.r * p_bright, DoubleLootTextPic.color.g * p_bright, DoubleLootTextPic.color.b * p_bright, 1f);
		ItemAmount.color = new Color(ItemAmount.color.r * p_bright, ItemAmount.color.g * p_bright, ItemAmount.color.b * p_bright, 1f);
	}
}
