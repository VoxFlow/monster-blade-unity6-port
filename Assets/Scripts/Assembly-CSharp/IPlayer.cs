using Models;

public interface IPlayer
{
	void Equip(ItemDetails p_item);

	void Equip(PetDetails p_pet);

	bool IsEquipped(ItemDetails p_item);

	bool IsEquipped(PetDetails p_pet);
}
