using System.Collections.Generic;

namespace Models
{
	public class GachaEventsInfo
	{
		public string id = string.Empty;

		public string gacha_type_id = string.Empty;

		public string gacha_status_id = string.Empty;

		public string name = string.Empty;

		public string date_created = string.Empty;

		public string start_date = string.Empty;

		public string end_date = string.Empty;

		public float price;

		public string reward_item_type = string.Empty;

		public int reward_amount;

		public string reward_same_set_flag = string.Empty;

		public int pay_times;

		public int free_times;

		public bool is_ticket_usable;

		public string filter_by = string.Empty;

		public string element = string.Empty;

		public List<List<ChestDetails>> sets = new List<List<ChestDetails>>();

		public bool IsSingleGacha
		{
			get
			{
				return pay_times == 0 && free_times == 0;
			}
		}
	}
}
