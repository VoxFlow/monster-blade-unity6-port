namespace Server.Models
{
	public sealed class GiftBoxUpdateModel : IObjectModel
	{
		public GiftBoxModel[] added;

		public GiftBoxModel[] removed;
	}
}
