using System.Collections.Generic;

public class ItemSelectEvent
{
	public ItemSelectEventType eventType;

	public List<InventoryCell> selectedItems;

	public bool multipleSelection;

	public ItemSelectEvent()
	{
	}

	public ItemSelectEvent(ItemSelectEventType eventType, List<InventoryCell> selectedItems, bool multipleSelection)
	{
		this.eventType = eventType;
		this.selectedItems = selectedItems;
		this.multipleSelection = multipleSelection;
	}
}
