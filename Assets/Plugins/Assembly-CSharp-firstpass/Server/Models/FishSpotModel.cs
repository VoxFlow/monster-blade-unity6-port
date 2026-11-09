namespace Server.Models
{
	public sealed class FishSpotModel : IObjectModel
	{
		public uint areaId;

		public int element;

		public int criteriaType;

		public int criteriaCount;

		public int criteriaTarget;

		public int timeLeft;

		public int expiry;

		public string prefab;

		public ObjectiveModel[] objectives;
	}
}
