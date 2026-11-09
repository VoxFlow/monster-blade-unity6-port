namespace Models
{
	public class CommonRewards
	{
		public CurrencyDetails currencies;

		public ServerInventoryItem item;

		public PetDetails pet;

		public CommonRewards()
		{
			currencies = new CurrencyDetails();
		}
	}
}
