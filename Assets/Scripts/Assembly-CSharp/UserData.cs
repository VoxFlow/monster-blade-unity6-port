using System.Collections.Generic;

public class UserData
{
	public string name = string.Empty;

	public string nubeeId = string.Empty;

	public string fbId = string.Empty;

	public int currentXp;

	public int totalFriends;

	public int inGameCurrency;

	public int premiumCurrency;

	public int socialCurrency;

	public int freePremiumGacha;

	public int giftGacha;

	public int freeSocialGacha;

	public int inventoryUsedSize;

	public int inventoryMaxSize;

	public int maxStamina;

	public int maxShield;

	public int stamina;

	public int shield;

	public long timeNextStamina;

	public long timeNextShield;

	public long timeNextWarcry;

	public bool isBanFromIAP;

	public List<string> gifts = new List<string>();

	public UserData()
	{
	}

	public UserData(UserData p_userData)
	{
		if (p_userData != null)
		{
			name = p_userData.name;
			nubeeId = p_userData.nubeeId;
			currentXp = p_userData.currentXp;
			totalFriends = p_userData.totalFriends;
			inGameCurrency = p_userData.inGameCurrency;
			premiumCurrency = p_userData.premiumCurrency;
			socialCurrency = p_userData.socialCurrency;
			freePremiumGacha = p_userData.freePremiumGacha;
			inventoryUsedSize = p_userData.inventoryUsedSize;
			inventoryMaxSize = p_userData.inventoryMaxSize;
			maxStamina = p_userData.maxStamina;
			stamina = p_userData.stamina;
			shield = p_userData.shield;
			timeNextStamina = p_userData.timeNextStamina;
			timeNextShield = p_userData.timeNextShield;
			timeNextWarcry = p_userData.timeNextWarcry;
		}
	}

	public override string ToString()
	{
		return string.Format("[UserData] : name:{0} nubeeId:{1} IGC:{2}  PC:{3} SC:{4} ", name, nubeeId, inGameCurrency, premiumCurrency, socialCurrency);
	}
}
