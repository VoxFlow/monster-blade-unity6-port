namespace Server.Models
{
	public sealed class BattleLogModel : IObjectModel
	{
		public uint tid;

		public string name;

		public long time;

		public int damage;
	}
}
