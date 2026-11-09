using Models;
using UnityEngine;

public class DailyBonusClaimPopup : MonoBehaviour
{
	[SerializeField]
	private InventoryCell m_invCell;

	[SerializeField]
	private UILabel m_title;

	[SerializeField]
	private UILabel m_desc;

	[SerializeField]
	private UISprite m_double_chest;

	[SerializeField]
	private UISprite m_double_text;

	private DailyBonusData m_data;

	public void ShowDailyBonusData(DailyBonusData p_data, UIAtlas p_cellAtlas, UIAtlas p_treasureAtlas)
	{
		m_title.text = SingletonComponent<NBLocalization>.Instance.Get("daily_bonus");
		m_desc.text = SingletonComponent<NBLocalization>.Instance.Get("daily_login_desc");
		m_data = p_data;
		switch (m_data.TypeId)
		{
		case 1:
			m_double_chest.enabled = false;
			m_double_text.enabled = false;
			if (p_data.MyItemData != null && !string.IsNullOrEmpty(p_data.MyItemData.ItemId))
			{
				ItemDetails itemDetails2 = SingletonComponent<ItemManager>.Instance.CreateItemDetails(p_data.MyItemData.ItemId, p_data.MyItemData.ItemTier, p_data.MyItemData.ItemLevel);
				m_invCell.data = itemDetails2;
				m_desc.text = string.Format(SingletonComponent<NBLocalization>.Instance.Get("you_got_daily_bonus_item"), itemDetails2.Name);
			}
			else if (!string.IsNullOrEmpty(p_data.Item))
			{
				ItemDetails data = SingletonComponent<ItemManager>.Instance.CreateItemDetails(p_data.Item);
				m_invCell.data = data;
			}
			break;
		case 2:
		{
			m_double_chest.enabled = false;
			m_double_text.enabled = false;
			int num = 0;
			num = int.Parse(p_data.Item);
			m_invCell.ShowIgc(num, 20f, -52f);
			m_desc.text = string.Format(SingletonComponent<NBLocalization>.Instance.Get("you_got_daily_bonus_igc"), num.ToString());
			break;
		}
		case 3:
			m_invCell.gameObject.SetActiveRecursively(false);
			break;
		case 4:
			m_invCell.gameObject.SetActiveRecursively(false);
			break;
		case 5:
			m_double_chest.enabled = false;
			m_double_text.enabled = false;
			if (p_data.Marked && p_data.MyItemData != null && !string.IsNullOrEmpty(p_data.MyItemData.ItemId))
			{
				ItemDetails itemDetails3 = SingletonComponent<ItemManager>.Instance.CreateItemDetails(p_data.MyItemData.ItemId, p_data.MyItemData.ItemTier, p_data.MyItemData.ItemLevel);
				m_invCell.data = itemDetails3;
				m_desc.text = string.Format(SingletonComponent<NBLocalization>.Instance.Get("you_got_daily_bonus_item"), itemDetails3.Name);
			}
			else
			{
				m_invCell.ShowSocialGacha(p_treasureAtlas);
			}
			break;
		case 6:
			m_double_chest.enabled = false;
			m_double_text.enabled = false;
			if (p_data.Marked && p_data.MyItemData != null && !string.IsNullOrEmpty(p_data.MyItemData.ItemId))
			{
				ItemDetails itemDetails = SingletonComponent<ItemManager>.Instance.CreateItemDetails(p_data.MyItemData.ItemId, p_data.MyItemData.ItemTier, p_data.MyItemData.ItemLevel);
				m_invCell.data = itemDetails;
				m_desc.text = string.Format(SingletonComponent<NBLocalization>.Instance.Get("you_got_daily_bonus_item"), itemDetails.Name);
			}
			else
			{
				m_invCell.ShowSocialGacha(p_treasureAtlas);
			}
			break;
		case 7:
			m_invCell.HideChildren();
			m_invCell.EnableChildren(false);
			m_double_chest.enabled = false;
			m_double_text.enabled = true;
			m_double_text.spriteName = "double_xp";
			m_double_text.transform.localPosition = new Vector3(m_double_text.transform.localPosition.x, 0f, m_double_text.transform.localPosition.z);
			m_desc.text = SingletonComponent<NBLocalization>.Instance.Get("you_got_daily_bonus_double_xp");
			break;
		case 8:
			m_invCell.HideChildren();
			m_invCell.EnableChildren(false);
			m_double_chest.enabled = true;
			m_double_text.enabled = true;
			m_double_text.spriteName = "double_loot_2";
			m_double_text.transform.localPosition = new Vector3(m_double_text.transform.localPosition.x, -20f, m_double_text.transform.localPosition.z);
			m_desc.text = SingletonComponent<NBLocalization>.Instance.Get("you_got_daily_bonus_double_loot");
			break;
		default:
			m_invCell.gameObject.SetActiveRecursively(false);
			break;
		}
	}

	public void ShowReward(string p_rewardType, int p_amount, ItemDetails p_itemData)
	{
		m_title.text = SingletonComponent<NBLocalization>.Instance.Get("announcement_reward_title");
		m_desc.text = string.Empty;
		m_double_chest.enabled = false;
		m_double_text.enabled = false;
		switch (p_rewardType)
		{
		case "IGC":
			m_invCell.ShowIgc(p_amount, 20f, -52f);
			m_desc.text = string.Format(SingletonComponent<NBLocalization>.Instance.Get("you_got_igc"), p_amount.ToString());
			break;
		case "PGC":
			m_invCell.ShowPgc(p_amount, 20f, -52f);
			m_desc.text = string.Format(SingletonComponent<NBLocalization>.Instance.Get("you_got_pgc"), p_amount.ToString());
			break;
		case "Item":
			m_invCell.data = p_itemData;
			m_desc.text = string.Format(SingletonComponent<NBLocalization>.Instance.Get("you_got_item"), p_itemData.Name);
			break;
		default:
			Dbg.LogWarnEx("Unknown reward type : {0}", Dbg.NBLogType.COMMON_WARNING, p_rewardType);
			break;
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
