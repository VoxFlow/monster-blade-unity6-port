public class ItemLoot
{
	public readonly string itemID = string.Empty;

	public readonly ItemEnum.LootType type = ItemEnum.LootType.NONE;

	public readonly int tier;

	public readonly bool consumable = true;

	public readonly float duration;

	public readonly float dungeonDropRate;

	public readonly float gachaSDropRate;

	public readonly float gachaPDropRate;

	public readonly string atlasName = string.Empty;

	public readonly string spriteName = string.Empty;

	public readonly string descriptionHead = string.Empty;

	public readonly string description = string.Empty;

	public readonly float fusionValue;

	public readonly float sellValue;

	public ItemLoot()
	{
	}

	public ItemLoot(string _itemID, ItemEnum.LootType _type, int _tier, bool _consumable, float _duration, float _dungeonDropRate, float _gachaSDropRate, float _gachaPDropRate, string _atlasName, string _spriteName, string _descriptionHead, string _description, float _fusionValue, float _sellValue)
	{
		itemID = _itemID;
		type = _type;
		tier = _tier;
		consumable = _consumable;
		duration = _duration;
		dungeonDropRate = _dungeonDropRate;
		gachaSDropRate = _gachaSDropRate;
		gachaPDropRate = _gachaPDropRate;
		atlasName = _atlasName;
		spriteName = _spriteName;
		descriptionHead = _descriptionHead;
		description = _description;
		fusionValue = _fusionValue;
		sellValue = _sellValue;
	}

	public override string ToString()
	{
		return string.Format("[Loot]\ntype:{0}  consumable:{1}  duration:{2}  DropRateDg:{3}    DropRateSG:{4}    DropRatePG:{5}\tFusionValue:{6}\tSellValue:{7}\n", type, consumable, duration, dungeonDropRate, gachaSDropRate, gachaPDropRate, fusionValue, sellValue);
	}
}
