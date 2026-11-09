namespace Server.Models
{
	public sealed class UpdateVersionModel : IObjectModel
	{
		public int major;

		public int minor;

		public string domain;

		public string filename;
	}
}
