public class TierRange
{
	public static TierRange DEFAULT_TIER_RANGE = new TierRange();

	public static int TIER_MIN = 1;

	public static int TIER_MAX = 5;

	public readonly int BaseLevelStart = 1;

	public readonly int BaseLevelEnd = 1;

	public readonly int LevelRange = 1;

	private TierRange()
	{
	}

	public TierRange(int _bLvlStart, int _bLvlEnd, int _dLvlEnd)
	{
		BaseLevelStart = _bLvlStart;
		BaseLevelEnd = _bLvlEnd;
		LevelRange = _dLvlEnd;
	}

	public static bool IsValidTier(int p_tier)
	{
		return p_tier >= TIER_MIN && p_tier <= TIER_MAX;
	}

	public override string ToString()
	{
		return string.Format("[TierRange] BaseLevelStart:{0}, BaseLevelEnd:{1}, LevelRange:{3}", BaseLevelStart, BaseLevelEnd, LevelRange);
	}
}
