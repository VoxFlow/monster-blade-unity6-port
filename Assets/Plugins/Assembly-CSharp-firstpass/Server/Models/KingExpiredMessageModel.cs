namespace Server.Models
{
	public sealed class KingExpiredMessageModel : IObjectModel
	{
		public uint kingId;

		public uint templateId;

		public int level;

		public uint areaId;

		public int hp;

		public int timeLeft;

		public int timeStamp;
	}
}
