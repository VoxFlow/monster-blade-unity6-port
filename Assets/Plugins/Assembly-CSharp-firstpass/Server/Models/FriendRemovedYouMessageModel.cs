namespace Server.Models
{
	public sealed class FriendRemovedYouMessageModel : IObjectModel
	{
		public uint senderId;

		public uint targetId;

		public ulong timestamp;
	}
}
