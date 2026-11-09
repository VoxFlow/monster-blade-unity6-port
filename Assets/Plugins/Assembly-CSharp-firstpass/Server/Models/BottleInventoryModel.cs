namespace Server.Models
{
	public sealed class BottleInventoryModel : IObjectModel
	{
		public uint templateId;

		public int count;

		public int countDiff;
	}
}
