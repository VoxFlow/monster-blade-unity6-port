namespace Models
{
	public class PurchaseDetails
	{
		public CharacterAccountDetails characterAccountDetails;

		public string jsonPurchaseString = string.Empty;

		public PurchaseDetails()
		{
			characterAccountDetails = new CharacterAccountDetails();
			jsonPurchaseString = string.Empty;
		}
	}
}
