namespace Models
{
	public class ShopData
	{
		public string ID { get; protected set; }

		public string Title { get; protected set; }

		public string Description { get; protected set; }

		public int Price { get; protected set; }

		public CurrencyEnum Currency { get; protected set; }

		public RewardsType Reward { get; protected set; }

		public virtual bool IsValid
		{
			get
			{
				return Currency != CurrencyEnum.NONE && Reward != RewardsType.NONE && Title != string.Empty && Description != string.Empty && Price >= 0;
			}
		}

		public ShopData(string p_id, string p_title, string p_description, int p_price, CurrencyEnum p_currency, RewardsType p_reward)
		{
			ID = p_id;
			Title = p_title;
			Description = p_description;
			Price = p_price;
			Currency = p_currency;
			Reward = p_reward;
		}
	}
}
