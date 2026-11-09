using System.Collections.Generic;

namespace Models
{
	public class ServerBattleDetails
	{
		public string dungeon_key = string.Empty;

		public List<string> monster_ids = new List<string>();

		public List<ChestDetails> chest_list = new List<ChestDetails>();

		public string kingBossId = string.Empty;

		public int kingBossInsertIndex = -1;

		public Dictionary<string, string> dungeonMonsterIdMap = new Dictionary<string, string>();
	}
}
