namespace Models
{
	public class ShopDataPet : ShopData
	{
		public string PetID { get; private set; }

		public PetDetails PetDetails { get; private set; }

		public override bool IsValid
		{
			get
			{
				return base.IsValid && PetID != string.Empty;
			}
		}

		public ShopDataPet(string p_id, string p_title, string p_description, int p_price, CurrencyEnum p_currency, string p_petID)
			: base(p_id, p_title, p_description, p_price, p_currency, RewardsType.PET)
		{
			PetID = p_petID.Split('^')[0];
		}

		public void SetPetDetails(PetDetails p_pet)
		{
			PetDetails = p_pet;
		}
	}
}
