// Resolves the game's own "UI" layer where Unity 6's built-in layer of the same name would be found first. Register CD-050.
using UnityEngine;

public static class MBLayers
{
	public static int NameToLayer(string name)
	{
		for (int i = 8; i < 32; i++) if (LayerMask.LayerToName(i) == name) return i;
		return LayerMask.NameToLayer(name);
	}
}
