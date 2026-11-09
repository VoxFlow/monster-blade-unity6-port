namespace Models
{
	public class ServerInventoryItem
	{
		public string db_id = string.Empty;

		public string item_type = string.Empty;

		public string item_id = string.Empty;

		public int item_level = 1;

		public bool isEquip;

		public int fusion_xp;

		public int tier;

		public bool isPet;

		public bool isFinalLoot;

		public string serverStringFormat { get; private set; }

		public string collectionId { get; private set; }

		public string itemIDNoSkill { get; private set; }

		public string itemSkill { get; private set; }

		public ServerInventoryItem(string p_serverStringFormat)
		{
			CreateItemFromString(p_serverStringFormat);
		}

		public ServerInventoryItem(string p_item, int p_tier, string p_db_id = "", string p_item_type = "", int p_itemLevel = 0, int p_fusion_xp = 0, string p_itemSkill = "", int p_isEquip = 0)
		{
			if (!p_item.Contains(GLOBAL.DEFAULT_BASE_ITEM_FLAG) && p_item_type != "essence")
			{
				int num = p_item.LastIndexOf('_');
				if (num >= 0 && num < p_item.Length)
				{
					p_itemSkill = p_item.Substring(num);
					p_item = p_item.Replace(p_itemSkill, string.Empty);
				}
			}
			if (p_item_type == "essence")
			{
				p_itemLevel = -1;
				p_itemSkill = string.Empty;
			}
			string p_serverStringFormat = p_item + "^" + p_tier + "^" + p_db_id + "^" + p_item_type + "^" + p_itemLevel + "^" + p_fusion_xp + "^" + p_itemSkill + "^" + p_isEquip;
			CreateItemFromString(p_serverStringFormat);
		}

		public bool IsWeaponOrArmor()
		{
			int length = item_id.IndexOf('_');
			string text = item_id.Substring(0, length);
			return text == "ARMOUR" || text == "WEAPON";
		}

		public bool HasSkill()
		{
			int startIndex = item_id.LastIndexOf('_') + 1;
			string text = item_id.Substring(startIndex);
			string[] array = new string[5] { "GHOUL", "HILL", "BRUTE", "ROGUE", "SOLDIER" };
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == text)
				{
					return true;
				}
			}
			return false;
		}

		private void CreateItemFromString(string p_serverStringFormat)
		{
			if (p_serverStringFormat == null)
			{
				return;
			}
			serverStringFormat = p_serverStringFormat;
			string[] array = p_serverStringFormat.Split('^');
			if (array.Length >= 2)
			{
				item_id = array[0];
				itemIDNoSkill = item_id;
				NBGeneric.TryParse<int>(array[1], out tier);
				collectionId = itemIDNoSkill + "^" + tier;
				if (array.Length == 8)
				{
					db_id = array[2];
					item_type = array[3];
					NBGeneric.TryParse<int>(array[4], out item_level);
					NBGeneric.TryParse<int>(array[5], out fusion_xp);
					itemSkill = array[6];
					int p_outResult = 0;
					NBGeneric.TryParse<int>(array[7], out p_outResult);
					isEquip = p_outResult > 0;
				}
				if (itemSkill != string.Empty)
				{
					item_id += itemSkill;
				}
			}
		}

		public override string ToString()
		{
			return string.Format("db_id:{0}, item_type:{1}, item_id:{2}, item_level:{3}, isEquip:{4}, fusion_xp:{5}, tier:{6}, isPet:{7}, isFinalLoot:{8}", db_id, item_type, item_id, item_level, isEquip, fusion_xp, tier, isPet, isFinalLoot);
		}
	}
}
