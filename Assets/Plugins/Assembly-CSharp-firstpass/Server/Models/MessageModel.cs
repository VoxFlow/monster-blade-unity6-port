namespace Server.Models
{
	public sealed class MessageModel : IObjectModel
	{
		public uint senderId;

		public string senderName;

		public string message;
	}
}
