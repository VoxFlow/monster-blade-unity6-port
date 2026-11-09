namespace Models
{
	public class CurrencyDetails
	{
		public float social;

		public float premium;

		public float ingame;

		public float freeGacha;

		public float giftGacha;

		public float freeSocialGacha;

		public float medal;

		public int dragonPoints;

		public int holyDiamonds;

		public CurrencyDetails()
		{
		}

		public CurrencyDetails(CurrencyDetails p_rhs)
		{
			social = p_rhs.social;
			premium = p_rhs.premium;
			ingame = p_rhs.ingame;
			freeGacha = p_rhs.freeGacha;
			giftGacha = p_rhs.giftGacha;
			freeSocialGacha = p_rhs.freeSocialGacha;
			medal = p_rhs.medal;
			dragonPoints = p_rhs.dragonPoints;
			holyDiamonds = p_rhs.holyDiamonds;
		}

		public static CurrencyDetails operator +(CurrencyDetails p_lhs, CurrencyDetails p_rhs)
		{
			CurrencyDetails currencyDetails = new CurrencyDetails(p_lhs);
			currencyDetails.social += p_rhs.social;
			currencyDetails.premium += p_rhs.premium;
			currencyDetails.ingame += p_rhs.ingame;
			currencyDetails.freeGacha += p_rhs.freeGacha;
			currencyDetails.giftGacha += p_rhs.giftGacha;
			currencyDetails.freeSocialGacha += p_rhs.freeSocialGacha;
			currencyDetails.medal += p_rhs.medal;
			currencyDetails.dragonPoints += p_rhs.dragonPoints;
			currencyDetails.holyDiamonds += p_rhs.holyDiamonds;
			return currencyDetails;
		}
	}
}
