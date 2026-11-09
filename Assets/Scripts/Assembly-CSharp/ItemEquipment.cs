using System.Collections.Generic;

public class ItemEquipment
{
	public readonly string itemID = string.Empty;

	public readonly int tier;

	public readonly string setID = string.Empty;

	public readonly string assetBundle = string.Empty;

	public readonly List<string> catalysts = new List<string>();

	public readonly string name = string.Empty;

	public readonly Equipment.Part part = Equipment.Part.Max;

	public readonly bool canEvolve;

	public readonly TierRange tierRange = TierRange.DEFAULT_TIER_RANGE;

	public readonly int maxTier = 1;

	public readonly bool isSpecial;

	public readonly CharacterElement characterElement;

	public ItemEquipment()
	{
	}

	public ItemEquipment(string _itemID, int _tier)
		: this(_itemID, _tier, string.Empty, string.Empty, null, string.Empty, Equipment.Part.Max, null, false, null, 0)
	{
	}

	public ItemEquipment(string _itemID, int _tier, string _setID, string _assetBundle, List<string> _catalysts, string p_name, Equipment.Part _part, CharacterElement _characterElement, bool _canEvolve, TierRange _tierRange, int _maxTier, bool p_isSpecial = false)
	{
		itemID = _itemID;
		tier = _tier;
		setID = _setID;
		assetBundle = _assetBundle;
		catalysts = _catalysts;
		name = p_name;
		part = _part;
		characterElement = _characterElement;
		canEvolve = _canEvolve;
		if (_tierRange == null)
		{
			tierRange = TierRange.DEFAULT_TIER_RANGE;
		}
		else
		{
			tierRange = _tierRange;
		}
		maxTier = _maxTier;
		isSpecial = p_isSpecial;
	}

	public override string ToString()
	{
		return string.Format("[Equipment]\nIID:{0}    T:{1}    setName:{2}    AssetBundle:{3}    CatalystCount:{4}    part:{5}\tcanEvolve:{6}\tmaxTier:{7}\n", itemID, tier, setID, assetBundle, catalysts.Count, part, canEvolve, maxTier);
	}
}
