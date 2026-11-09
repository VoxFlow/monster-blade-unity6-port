using System.Collections.Generic;

namespace Server.Models
{
	public sealed class FriendInviteReplyMessageModel : IObjectModel
	{
		public uint userId;

		public int reply;

		public string userName;

		public int timeStamp;

		public List<ulong> socialNets;
	}
}
