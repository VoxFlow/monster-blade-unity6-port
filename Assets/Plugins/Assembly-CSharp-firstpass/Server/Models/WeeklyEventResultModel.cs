namespace Server.Models
{
	public sealed class WeeklyEventResultModel : IObjectModel
	{
		public string id;

		public int ranking;

		public int teamRanking;

		public WeeklyRewardModel[] rewards;

		public WeeklyRewardModel[] teamRewards;

		public ulong timeStamp;
	}
}
