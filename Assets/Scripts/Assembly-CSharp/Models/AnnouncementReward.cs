namespace Models
{
	public class AnnouncementReward
	{
		public int DbId;

		public int AnnouncementId;

		public bool Claimed;

		public int TypeId;

		public string TypeName = "PGC";

		public int Amount;

		public string ItemId = string.Empty;

		public int Tier = -1;
	}
}
