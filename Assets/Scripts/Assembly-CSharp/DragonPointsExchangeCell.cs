using Models;
using UnityEngine;

public class DragonPointsExchangeCell : NBUIGridCell<ShopData>
{
	[SerializeField]
	private InventoryCell m_inventoryCellPrefab;

	[SerializeField]
	private UIAtlas m_customAtlas;

	[SerializeField]
	private UILabel m_nameLabel;

	[SerializeField]
	private UILabel m_descLabel;

	[SerializeField]
	private UILabel m_priceLabel;

	[SerializeField]
	private UIButton m_exchangeButton;

	[SerializeField]
	private UISprite m_customSprite;

	[SerializeField]
	private UISprite m_frame;

	[SerializeField]
	private UISprite m_blackSprite;

	[SerializeField]
	private Transform m_itemSlot;

	private GameObject m_eventTarget;

	private InventoryCell m_cell;

	public override ShopData data
	{
		get
		{
			return m_data;
		}
		set
		{
			m_data = value;
			if (m_data == null)
			{
				return;
			}
			SetName(m_data.Title);
			SetDescription(m_data.Description);
			SetPrice(m_data.Price, m_data.Price);
			if (m_data.Reward == RewardsType.ITEM)
			{
				ShopDataItem shopDataItem = m_data as ShopDataItem;
				if (shopDataItem == null || shopDataItem.ServerItem == null)
				{
					return;
				}
				ItemManager instance = SingletonComponent<ItemManager>.Instance;
				if (null != instance)
				{
					ShowCustomSprite(false, false);
					string text = shopDataItem.ServerItem.item_id;
					if (shopDataItem.ServerItem.IsWeaponOrArmor() && !shopDataItem.ServerItem.HasSkill())
					{
						text += GLOBAL.DEFAULT_ITEM_SKILL_PREFIX;
					}
					ItemDetails item = instance.CreateItemDetails(text, shopDataItem.ServerItem.tier);
					SetItem(item);
				}
			}
			else if (m_data.Reward == RewardsType.GACHA)
			{
				ShopDataGacha shopDataGacha = m_data as ShopDataGacha;
				if (shopDataGacha != null)
				{
					ShowCustomSprite(true, false);
					SetCustomImage("Icon_treasure_pressed", string.Empty);
				}
			}
			else
			{
				if (m_data.Reward != RewardsType.PET)
				{
					return;
				}
				ShopDataPet shopDataPet = m_data as ShopDataPet;
				if (shopDataPet != null)
				{
					PetDetails petDetails = Singleton<PetSetup>.Instance.GetPetDetails(shopDataPet.PetID);
					if (petDetails != null)
					{
						EnableExchangeButton();
						ShowCustomSprite(true, true);
						shopDataPet.SetPetDetails(petDetails);
						SetCustomImage(petDetails.PetIconPath, petDetails.PetFrameName);
					}
				}
			}
		}
	}

	private void EnableExchangeButton()
	{
		if (m_data != null && m_data.Reward == RewardsType.PET)
		{
			ShopDataPet p_petShopData = m_data as ShopDataPet;
			if (null != m_exchangeButton)
			{
				bool flag = SingletonComponent<StoreManager>.Instance.IsValidPetShopData(p_petShopData);
				m_exchangeButton.SetButtonGrey(!flag);
			}
		}
	}

	private void OnEnable()
	{
		Messenger<int>.AddListener("exchange_update_points", OnDragonPointsUpdated);
		EnableExchangeButton();
	}

	private void OnDisable()
	{
		Messenger<int>.RemoveListener("exchange_update_points", OnDragonPointsUpdated);
	}

	private void OnDragonPointsUpdated(int p_points)
	{
		if (m_data != null)
		{
			SetPrice(m_data.Price, p_points);
			EnableExchangeButton();
		}
	}

	public void SetItem(ItemDetails p_item)
	{
		if (p_item == null)
		{
			return;
		}
		if (null == m_cell && null != m_inventoryCellPrefab)
		{
			m_cell = Object.Instantiate(m_inventoryCellPrefab) as InventoryCell;
			if (null != m_itemSlot)
			{
				m_cell.transform.parent = m_itemSlot.transform.parent;
				m_cell.transform.localScale = m_itemSlot.transform.localScale;
				m_cell.transform.localPosition = ((!(null == m_itemSlot)) ? m_itemSlot.transform.localPosition : Vector3.zero);
			}
		}
		if (null != m_cell)
		{
			m_cell.data = p_item;
		}
	}

	public UISprite GetCustomSprite()
	{
		return m_customSprite;
	}

	public void ShowCustomSprite(bool p_show, bool p_showFrame)
	{
		if (null != m_customSprite)
		{
			m_customSprite.enabled = p_show;
		}
		if (null != m_frame && null != m_blackSprite)
		{
			m_frame.enabled = p_showFrame;
			m_blackSprite.enabled = p_showFrame;
		}
	}

	public void SetCustomImage(string p_spriteName, string p_frameName = "")
	{
		if (null != m_customSprite && null != m_customAtlas)
		{
			m_customSprite.atlas = m_customAtlas;
			m_customSprite.spriteName = p_spriteName;
			m_customSprite.MakePixelPerfect();
		}
		if (null != m_frame && null != m_customAtlas && string.Empty != p_frameName)
		{
			m_frame.atlas = m_customAtlas;
			m_frame.spriteName = p_frameName;
		}
	}

	public void SetName(string p_name)
	{
		if (null != m_nameLabel)
		{
			m_nameLabel.text = p_name;
		}
	}

	public void SetDescription(string p_text)
	{
		if (null != m_descLabel)
		{
			m_descLabel.text = p_text;
		}
	}

	public void SetPrice(int p_price, int p_base = 0)
	{
		if (null != m_priceLabel)
		{
			Color color = Color.white;
			if (p_price <= p_base)
			{
				color = Color.green;
			}
			else if (p_price > p_base)
			{
				color = Color.red;
			}
			m_priceLabel.color = color;
			m_priceLabel.text = p_price + string.Empty;
		}
	}

	public void SetExchangeButtonMessageTarget(GameObject p_target)
	{
		m_eventTarget = p_target;
		if (null != m_exchangeButton)
		{
			UIButtonMessage component = m_exchangeButton.GetComponent<UIButtonMessage>();
			if (null != component)
			{
				component.target = base.gameObject;
			}
		}
		if (null != m_cell)
		{
			UIButtonMessage component2 = m_cell.GetComponent<UIButtonMessage>();
			if (null != component2)
			{
				component2.target = base.gameObject;
			}
		}
	}

	private void OnButtonClick(GameObject p_sender)
	{
		if (!(null == p_sender) && !(null == m_eventTarget))
		{
			if (p_sender == m_exchangeButton.gameObject)
			{
				p_sender = base.gameObject;
			}
			if (null != m_exchangeButton)
			{
				m_eventTarget.SendMessage("OnButtonClick", p_sender, SendMessageOptions.DontRequireReceiver);
			}
		}
	}
}
