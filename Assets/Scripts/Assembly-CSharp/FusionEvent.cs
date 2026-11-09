using System.Collections.Generic;

public class FusionEvent
{
	public FusionEventType eventType;

	public InventoryCell baseCell;

	public List<InventoryCell> materialCells;

	public ItemDetails resultItem;

	public ItemDetails baseItem;

	public List<ItemDetails> otherItems;

	public bool regenerateCharacter;
}
