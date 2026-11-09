namespace Server.Models
{
	public sealed class ShopModel : IObjectModel
	{
		public uint id;

		public uint shopSequenceId;

		public uint[] classId;

		public uint[] templateId;

		public int[] cost;

		public int[] count;
	}
}
