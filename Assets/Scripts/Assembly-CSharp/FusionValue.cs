public class FusionValue
{
	public readonly int tier = 1;

	public readonly int xpToLevelUp = 1;

	public readonly int startXpValue = 1;

	public readonly int xpIncreasePerLevel = 1;

	public FusionValue(int _tier, int _xpToLevelUp, int _startXp, int _xpIncPerLevel)
	{
		tier = _tier;
		xpToLevelUp = _xpToLevelUp;
		startXpValue = _startXp;
		xpIncreasePerLevel = _xpIncPerLevel;
		if (startXpValue <= 0)
		{
			startXpValue = 1;
		}
	}

	public override string ToString()
	{
		return string.Format("[FusionValue]\nTier:{0}     xpToLevelUp:{1}    StartXP:{2}\tXpIncPerLevel:{3}", tier, xpToLevelUp, startXpValue, xpIncreasePerLevel);
	}
}
