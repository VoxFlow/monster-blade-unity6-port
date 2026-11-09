namespace Server.Models
{
	public sealed class KingDefeatedMessageModel : IObjectModel
	{
		public uint templateId;

		public int level;

		public int score;

		public uint mostDamageTeamId;

		public int mostDamageTeamScore;

		public uint mostDamageUserId;

		public string mostDamageUserName;

		public int mostDamageUserScore;

		public int isParticipant;

		public int timeStamp;
	}
}
