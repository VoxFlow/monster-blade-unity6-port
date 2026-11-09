namespace Server.Models
{
	public sealed class KingEventResultModel : IObjectModel
	{
		public int mailType;

		public GeneralRewardModel teamReward;

		public GeneralRewardModel personReward;

		public int timeStamp;

		public int isWinner;

		public string id;
	}
}
