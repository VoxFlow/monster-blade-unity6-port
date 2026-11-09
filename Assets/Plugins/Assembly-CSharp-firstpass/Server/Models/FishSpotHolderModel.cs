namespace Server.Models
{
	public sealed class FishSpotHolderModel : IObjectModel
	{
		public uint lastArea;

		public int eventElement;

		public int areaCount;

		public FishSpotModel[] list;
	}
}
