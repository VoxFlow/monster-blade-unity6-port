// Generated: the main asset of each original asset bundle, by bundle path and lookup name. Register CD-009.
using System;
using System.Collections.Generic;
using UnityEngine;

public static class BundleMainAssets
{
	public static readonly Dictionary<string, KeyValuePair<string, Type>> ByBundle = new Dictionary<string, KeyValuePair<string, Type>>
	{
		{ "assetbundles/characters/character_element_database.assetbundle", new KeyValuePair<string, Type>("characterelementdatabase", typeof(UnityEngine.Object)) },
		{ "assetbundles/characters/common/atlas/customization_weapon_atlas_4.assetbundle", new KeyValuePair<string, Type>("customization_weapon_atlas_4", typeof(GameObject)) },
		{ "assetbundles/characters/common/atlas/customization_weapon_atlas_5.assetbundle", new KeyValuePair<string, Type>("customization_weapon_atlas_5", typeof(GameObject)) },
		{ "assetbundles/characters/common/atlas/customization_weapon_atlas_6.assetbundle", new KeyValuePair<string, Type>("customization_weapon_atlas_6", typeof(GameObject)) },
		{ "assetbundles/characters/common/atlas/loot_item_atlas.assetbundle", new KeyValuePair<string, Type>("loot_item_atlas", typeof(GameObject)) },
		{ "assetbundles/characters/male/atlas/customization_atlas_1.assetbundle", new KeyValuePair<string, Type>("customization_atlas_1", typeof(GameObject)) },
		{ "assetbundles/characters/male/atlas/customization_atlas_2.assetbundle", new KeyValuePair<string, Type>("customization_atlas_2", typeof(GameObject)) },
		{ "assetbundles/characters/male/atlas/customization_atlas_3.assetbundle", new KeyValuePair<string, Type>("customization_atlas_3", typeof(GameObject)) },
		{ "assetbundles/characters/male/atlas/customization_atlas_4.assetbundle", new KeyValuePair<string, Type>("customization_atlas_4", typeof(GameObject)) },
		{ "assetbundles/characters/male/atlas/customization_default_atlas.assetbundle", new KeyValuePair<string, Type>("customization_default_atlas", typeof(GameObject)) },
		{ "assetbundles/characters/male/male_characterbase.assetbundle", new KeyValuePair<string, Type>("male_characterbase", typeof(GameObject)) },
		{ "assetbundles/characters/male/male_mainmenu_characterbase.assetbundle", new KeyValuePair<string, Type>("male_mainmenu_characterbase", typeof(GameObject)) },
	};

	public static bool For(string url, out string name, out Type type)
	{
		foreach (KeyValuePair<string, KeyValuePair<string, Type>> kv in ByBundle)
		{
			if (url.EndsWith(kv.Key))
			{
				name = kv.Value.Key;
				type = kv.Value.Value;
				return true;
			}
		}
		name = null;
		type = null;
		return false;
	}
}
