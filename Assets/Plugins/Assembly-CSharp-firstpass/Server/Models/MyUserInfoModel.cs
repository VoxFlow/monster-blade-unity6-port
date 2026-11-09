namespace Server.Models
{
	public sealed class MyUserInfoModel : IObjectModel
	{
		public uint userId;

		public uint mTeamId;

		public uint mTeamLeaderId;

		public uint teamId;

		public uint rivalTeamId;

		public string name;

		public int socialType;

		public long[] socialNets;

		public int createTime;

		public string inviteCode;

		public int stepId;

		public string chatRoomId;

		public uint socialGiftCoolDown;
	}
}
