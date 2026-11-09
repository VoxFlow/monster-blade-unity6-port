namespace Server.Models
{
	public sealed class WeeklyRewardModel : IObjectModel
	{
		public int from;

		public int to;

		public uint classId;

		public uint templateId;

		public int count;
	}
}
