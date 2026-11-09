namespace Server.Models
{
	public sealed class DailyEventResultModel : IObjectModel
	{
		public string id;

		public uint winTeam;

		public uint loseTeam;

		public RankModel rank1;

		public RankModel rank2;

		public RankModel rank3;

		public GeneralRewardModel rodWinTeam;

		public GeneralRewardModel rodLoseTeam;

		public GeneralRewardModel rodRank1;

		public GeneralRewardModel rodRank2;

		public GeneralRewardModel rodRank3;

		public ulong timeStamp;
	}
}
