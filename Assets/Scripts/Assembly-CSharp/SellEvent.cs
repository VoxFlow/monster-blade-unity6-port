using System.Collections.Generic;

public struct SellEvent
{
	public SellEventType type;

	public List<InventoryCell> items;
}
