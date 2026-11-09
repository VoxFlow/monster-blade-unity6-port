namespace Server.Models
{
	public sealed class TeamInfoHistory : IObjectModel
	{
		public TeamMemberInfoModel[] myTeam;

		public TeamMemberInfoModel[] rivalTeam;
	}
}
