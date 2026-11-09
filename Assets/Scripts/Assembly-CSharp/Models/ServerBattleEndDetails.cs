using System.Collections.Generic;

namespace Models
{
	public class ServerBattleEndDetails
	{
		public bool win;

		public CharacterAccountDetails character = new CharacterAccountDetails();

		public Dictionary<string, ServerInventoryItem> rewarded_items = new Dictionary<string, ServerInventoryItem>();

		public JSONObject unlockedCitiesJson;

		public JSONObject eligibleEvents;

		public JSONObject giftSenders;

		public int GainedXp;
	}
}
