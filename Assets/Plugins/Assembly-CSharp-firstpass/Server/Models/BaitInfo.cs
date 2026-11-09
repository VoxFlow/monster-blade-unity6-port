namespace Server.Models
{
	public sealed class BaitInfo : IObjectModel
	{
		public uint id;

		public string name;

		public float fishWaitDurationMin;

		public float fishWaitDurationMax;
	}
}
