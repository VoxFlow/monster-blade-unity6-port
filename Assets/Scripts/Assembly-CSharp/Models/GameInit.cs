using System.Collections.Generic;

namespace Models
{
	public class GameInit
	{
		public GameSettings gameSettings = new GameSettings();

		public PatchInfo patchInfo = new PatchInfo();

		public List<ServerStoreItem> serverStoreItems = new List<ServerStoreItem>();

		public JSONObject unlockedCitiesJson;

		public JSONObject dungeonEvents;

		public JSONObject giftSenders;

		public string mainEventID = string.Empty;

		public CharacterAccountDetails characterAccountDetails = new CharacterAccountDetails();

		public List<ServerInventoryItem> inventoryItems = new List<ServerInventoryItem>();

		public List<GachaEventsInfo> gachaEvents = new List<GachaEventsInfo>();

		public Dictionary<int, AchievementManager.AchievementItem> achievements = new Dictionary<int, AchievementManager.AchievementItem>();

		public BattleHelpData m_BattleHelpData = new BattleHelpData();

		public List<PromoCodeData> m_promoCodeDatas = new List<PromoCodeData>();
	}
}
