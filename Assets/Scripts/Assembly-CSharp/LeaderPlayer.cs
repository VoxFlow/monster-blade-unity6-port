using Models;

public class LeaderPlayer
{
	public CharacterAccountDetails CharaAccountData;

	public int Rank;

	public string CharaName;

	public string Title = string.Empty;

	public int Score;

	public int AmountOfPgcReward;

	public int AmountOfSocialPoint;

	public LeaderPlayer(int p_rank, string p_name, int p_score)
	{
		CharaAccountData = new CharacterAccountDetails();
		Rank = p_rank;
		CharaName = p_name;
		Score = p_score;
	}

	public override string ToString()
	{
		return base.ToString() + string.Format("Rank: {0}, Chara name:{1} , score: {2}, exp: {3}", Rank, CharaName, Score, CharaAccountData.exp);
	}
}
