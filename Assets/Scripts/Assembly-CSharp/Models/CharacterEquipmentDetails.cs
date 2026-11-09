namespace Models
{
	public class CharacterEquipmentDetails
	{
		public string db_id = string.Empty;

		public string item_id = string.Empty;

		public int item_level = 1;

		public int fusionXp;

		public int item_tier = 1;

		public CharacterEquipmentDetails()
		{
		}

		public CharacterEquipmentDetails(CharacterEquipmentDetails p_rhs)
		{
			db_id = p_rhs.db_id;
			item_id = p_rhs.item_id;
			item_level = p_rhs.item_level;
			fusionXp = p_rhs.fusionXp;
			item_tier = p_rhs.item_tier;
		}
	}
}
