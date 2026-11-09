namespace Server.Models
{
	public sealed class GiftBoxModel : IObjectModel
	{
		public string messageId;

		public uint senderId;

		public string senderName;

		public uint classId;

		public uint templateId;

		public int count;

		public int xp;

		public int level;

		public int expiryTime;
	}
}
