namespace Models
{
	public class ShopDataItem : ShopData
	{
		public ServerInventoryItem ServerItem { get; private set; }

		public ItemDetails ItemDetails { get; private set; }

		public override bool IsValid
		{
			get
			{
				return base.IsValid && ServerItem != null && ServerItem.item_id != string.Empty && TierRange.IsValidTier(ServerItem.tier);
			}
		}

		public ShopDataItem(string p_id, string p_title, string p_description, int p_price, CurrencyEnum p_currency, ServerInventoryItem p_item)
			: base(p_id, p_title, p_description, p_price, p_currency, RewardsType.ITEM)
		{
			ServerItem = p_item;
		}

		public void SetItemDetails(ItemDetails p_itemDetails)
		{
			ItemDetails = p_itemDetails;
		}
	}
}
