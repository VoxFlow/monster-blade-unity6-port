namespace Server.Models
{
	public sealed class FishSpotRewardModel : IObjectModel
	{
		public uint classId;

		public int count;

		public uint areaId;

		public uint templateId;

		public ObjectiveModel objective;
	}
}
