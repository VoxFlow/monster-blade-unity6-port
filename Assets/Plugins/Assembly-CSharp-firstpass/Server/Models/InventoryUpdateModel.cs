namespace Server.Models
{
	public sealed class InventoryUpdateModel : IObjectModel
	{
		public InventoryModel added;

		public InventoryModel removed;
	}
}
