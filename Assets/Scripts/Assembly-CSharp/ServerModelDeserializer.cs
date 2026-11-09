using System.Collections.Generic;
using Models;

public class ServerModelDeserializer
{
	public static List<AssetsBundleModel> ParseAssetsBundleData(string json)
	{
		JSONObject jSONObject = new JSONObject(json);
		JSONObject jSONObject2 = jSONObject["payload"];
		List<AssetsBundleModel> list = new List<AssetsBundleModel>();
		foreach (JSONObject item2 in jSONObject2.list)
		{
			AssetsBundleModel item = JSONObjectDeserializer.Deserialize<AssetsBundleModel>(item2);
			list.Add(item);
		}
		return list;
	}

	public static List<PlayerItemModel> ParsePlayerItemData(string json)
	{
		JSONObject jSONObject = new JSONObject(json);
		JSONObject jSONObject2 = jSONObject["payload"];
		List<PlayerItemModel> list = new List<PlayerItemModel>();
		foreach (JSONObject item2 in jSONObject2.list)
		{
			PlayerItemModel item = JSONObjectDeserializer.Deserialize<PlayerItemModel>(item2);
			list.Add(item);
		}
		return list;
	}
}
