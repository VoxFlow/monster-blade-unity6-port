namespace Server.Models
{
	public sealed class SpecialEventModel : IObjectModel
	{
		public int eventType;

		public uint eventId;

		public float[] args1;

		public uint[] args2;

		public uint[] args3;

		public SpecialEventOptionModel options;

		public int stopTime;

		public int startTime;

		public int order;
	}
}
