namespace Server.Models
{
	public sealed class MTeamInfoModel : IObjectModel
	{
		public uint leaderId;

		public string name;

		public int total;

		public int totalRank;

		public TeamMemberInfoModel[] members;

		public int myScore;
	}
}
