using System.Collections.Generic;

public class EvolveEvent
{
	public EvolveEventType type;

	public InventoryCell baseCell;

	public List<InventoryCell> otherCells;

	public ItemDetails itemDetails;

	public ItemDetails resultItem;

	public List<ItemDetails> otherDetails;

	public bool regenerateCharacter;
}
