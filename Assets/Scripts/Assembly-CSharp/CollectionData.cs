using System.Collections.Generic;
using Models;

public class CollectionData
{
	public List<ItemDetails> Items = new List<ItemDetails>();

	public string SetNameKey = string.Empty;

	public void Add(ChestDetails p_chestDetails)
	{
		if (!string.IsNullOrEmpty(p_chestDetails.item_id) && SingletonComponent<ItemManager>.Instance != null)
		{
			ItemDetails item = SingletonComponent<ItemManager>.Instance.CreateItemDetails(p_chestDetails.item_id, p_chestDetails.item_tier, p_chestDetails.item_level);
			Items.Add(item);
		}
	}

	public void Add(List<ChestDetails> p_chestDetails)
	{
		if (!(SingletonComponent<ItemManager>.Instance != null))
		{
			return;
		}
		foreach (ChestDetails p_chestDetail in p_chestDetails)
		{
			Add(p_chestDetail);
		}
	}
}
