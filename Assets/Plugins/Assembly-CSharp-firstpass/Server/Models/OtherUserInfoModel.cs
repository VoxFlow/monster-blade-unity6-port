namespace Server.Models
{
	public sealed class OtherUserInfoModel : IObjectModel
	{
		public sealed class SocialInfo
		{
			public int socialType;

			public long socialId;

			public uint coolDown;

			public bool hasClaim;
		}

		public uint userId;

		public int level;

		public uint teamId;

		public int rank;

		public string name;

		public uint rodTemplateId;

		public int rodLevel;

		public int score;

		public long dateTime;

		public SocialInfo[] socialInfo;

		public int socialType;

		public long socialId;
	}
}
