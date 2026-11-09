namespace Server.Models
{
	public sealed class MailBoxModel : IObjectModel
	{
		public uint senderId;

		public string senderName;

		public string message;

		public int mailType;

		public uint classId;

		public int count;

		public uint templateId;

		public int xp;

		public int level;

		public ulong timeStamp;

		public string id;

		public int socialType;
	}
}
