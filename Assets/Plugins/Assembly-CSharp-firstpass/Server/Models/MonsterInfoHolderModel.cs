namespace Server.Models
{
	public sealed class MonsterInfoHolderModel : IObjectModel
	{
		public MonsterInfoModel info;

		public uint id;

		public uint templateId;

		public uint areaId;

		public uint userId;

		public string userName;

		public int hp;

		public int timeLeft;

		public int found;
	}
}
