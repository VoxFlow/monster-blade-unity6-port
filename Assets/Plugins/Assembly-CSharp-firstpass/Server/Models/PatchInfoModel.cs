namespace Server.Models
{
	public sealed class PatchInfoModel : IObjectModel
	{
		public string filename;

		public long size;

		public string checksum;
	}
}
