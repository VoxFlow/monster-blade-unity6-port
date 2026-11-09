using System.Collections.Generic;

namespace Models
{
	public class DragonShop
	{
		public List<ShopData> ShopData { get; private set; }

		public long ExpiryDate { get; private set; }

		public DragonShop(List<ShopData> p_data, long p_expiry)
		{
			ShopData = p_data;
			ExpiryDate = p_expiry;
		}
	}
}
