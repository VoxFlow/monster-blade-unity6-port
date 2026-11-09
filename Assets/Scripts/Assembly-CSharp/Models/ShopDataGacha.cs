namespace Models
{
	public class ShopDataGacha : ShopData
	{
		public int Tier { get; private set; }

		public int GachaID { get; private set; }

		public int Count { get; private set; }

		public string Atlas { get; private set; }

		public string Sprite { get; private set; }

		public override bool IsValid
		{
			get
			{
				return base.IsValid && Count >= 1;
			}
		}

		public bool NeedToPlayGacha
		{
			get
			{
				return GachaManager.IsValidGachaID(GachaID);
			}
		}

		public ShopDataGacha(string p_id, string p_title, string p_description, int p_price, CurrencyEnum p_currency, int p_tier = -1, int p_gachaID = -1, int p_count = 1)
			: base(p_id, p_title, p_description, p_price, p_currency, RewardsType.GACHA)
		{
			Tier = p_tier;
			GachaID = p_gachaID;
			Count = p_count;
		}

		public void SetImage(string p_atlas, string p_sprite)
		{
			Atlas = p_atlas;
			Sprite = p_sprite;
		}
	}
}
