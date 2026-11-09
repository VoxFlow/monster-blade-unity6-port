namespace Models
{
	public class AnnouncementMessage
	{
		public int DbId;

		public int AnnoucementId;

		public string Title = string.Empty;

		public string Body = string.Empty;

		public string Language = "en";

		public string ImgKey = string.Empty;

		public string ImgUrl = string.Empty;

		public string ImgMd5 = string.Empty;

		public override string ToString()
		{
			return string.Format("DbId:{0}, AnnoucementId:{1}, Title:{2}, Body:{3}, Language:{4}, ImgKey:{5}, ImgUrl:{6}", DbId, AnnoucementId, Title, Body, Language, ImgKey, ImgUrl);
		}
	}
}
