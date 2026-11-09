using System.Collections.Generic;

namespace Server.Models
{
	public sealed class friendList_model : IObjectModel
	{
		public List<uint> game;

		public List<uint> gameOnly;

		public List<GameAndSocialFriendInfo> gameAndFB;

		public List<SocialFriendInfo> FBOnly;
	}
}
