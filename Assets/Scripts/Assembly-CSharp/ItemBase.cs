public class ItemBase
{
	public readonly string itemID = string.Empty;

	public string name = string.Empty;

	public readonly ItemEnum.Chest chest = ItemEnum.Chest.LOW;

	public readonly float dropRate;

	public readonly ItemEnum.Type type = ItemEnum.Type.NONE;

	public readonly Element.Type elementType = Element.Type.Metal;

	public readonly ItemEnum.Skill skill = ItemEnum.Skill.NONE;

	public ItemBase(string _id, string _name, ItemEnum.Chest _chest, float _dropRate, ItemEnum.Type _type, Element.Type _elementType, ItemEnum.Skill _skill)
	{
		itemID = _id;
		name = _name;
		chest = _chest;
		dropRate = _dropRate;
		type = _type;
		elementType = _elementType;
		skill = _skill;
	}

	public override string ToString()
	{
		return string.Format("[ItemBase]\nIID:{0}     {1}    chest:{2}    type:{3}    {4}    skill:{5}\n", itemID, name, chest, type, elementType, skill);
	}
}
