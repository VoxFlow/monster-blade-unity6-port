namespace Models
{
	public class DailyBonusData
	{
		public enum Type
		{
			None = 0,
			Item = 1,
			IGC = 2,
			SC = 3,
			PC = 4,
			SocialGacha = 5,
			PremiumGacha = 6,
			DoubleXP = 7,
			DoubleLoot = 8
		}

		public class ItemData
		{
			public string dbId = string.Empty;

			public string ItemId = string.Empty;

			public int ItemLevel;

			public string ItemType = string.Empty;

			public int ItemTier;
		}

		public int Id;

		public int TypeId;

		public int Sequence;

		public string Item = string.Empty;

		public bool IsActive;

		public int RequireLogins;

		public bool RequireLoginWeek;

		public bool Marked;

		public int Progress;

		public ItemData MyItemData = new ItemData();

		public DailyBonusData()
		{
		}

		public DailyBonusData(int p_id, int p_typeId, int p_sequence, string p_item, bool p_isActive, int p_requireLogins, bool p_requireLoginWeek, bool p_marked)
		{
			Id = p_id;
			TypeId = p_typeId;
			Sequence = p_sequence;
			Item = p_item;
			IsActive = p_isActive;
			RequireLogins = p_requireLogins;
			RequireLoginWeek = p_requireLoginWeek;
			Marked = p_marked;
		}

		public void ClearData()
		{
			Id = 0;
			TypeId = 0;
			Sequence = 0;
			Item = string.Empty;
			IsActive = false;
			RequireLogins = 0;
			RequireLoginWeek = false;
			Marked = false;
		}

		public override string ToString()
		{
			return string.Format("DailyBonusData Id:{0}, TypeId:{1}, Sequence:{2}, Item:{3}, IsActive:{4}, RequireLogins:{5}, RequireLoginWeek:{6}, Marked:{7}, Progress:{8}", Id, TypeId, Sequence, Item, IsActive, RequireLogins, RequireLoginWeek, Marked, Progress);
		}
	}
}
