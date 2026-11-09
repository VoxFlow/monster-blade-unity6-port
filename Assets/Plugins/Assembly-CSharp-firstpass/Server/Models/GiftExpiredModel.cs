namespace Server.Models
{
	public sealed class GiftExpiredModel : IObjectModel
	{
		public uint senderId;

		public string senderName;

		public uint classId;

		public uint templateId;

		public int count;

		public int igcGain;
	}
}
