namespace Server.Models
{
	public sealed class FusionModel : IObjectModel
	{
		public uint classId;

		public uint templateId;

		public uint instanceId;

		public int level;

		public int levelGain;

		public int xp;

		public int xpGain;
	}
}
