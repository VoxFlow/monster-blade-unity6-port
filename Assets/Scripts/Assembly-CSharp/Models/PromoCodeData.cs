using System;

namespace Models
{
	public class PromoCodeData
	{
		public string ID = string.Empty;

		public string name = string.Empty;

		public DateTime startTime = default(DateTime);

		public DateTime endTime = default(DateTime);

		public RewardsType type;

		public string rewardsValue = string.Empty;

		public int tier;

		public bool isActive;

		public string description = string.Empty;

		public DateTime lastModifiedDate = default(DateTime);

		public bool isNew = true;

		public bool IsPromoEnable()
		{
			return DateTime.Compare(DateTime.UtcNow, startTime) > 0 && DateTime.Compare(DateTime.UtcNow, endTime) < 0 && isActive && description != string.Empty && rewardsValue != string.Empty && type != RewardsType.NONE && name != string.Empty && ID != string.Empty && tier != 0;
		}
	}
}
