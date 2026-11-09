namespace Server.Models
{
	public sealed class TeamInfoModel : IObjectModel
	{
		public int total;

		public int totalRank;

		public TeamMemberInfoModel[] members;

		public int myScore;

		public int myRank;
	}
}
