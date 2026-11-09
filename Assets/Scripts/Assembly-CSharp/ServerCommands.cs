using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerCommands
{
	public static IEnumerator PingServer()
	{
		Debug.Log("Pinging server");
		WWW www = new WWW("http://lasvegaslife.nubee.com/json/get/get_user?user_id=1");
		while (!www.isDone)
		{
			yield return null;
		}
		if (www.error != null)
		{
			Debug.Log("WWW error: " + www.error);
			yield break;
		}
		string json_string = www.text;
		Debug.Log("Received Json Reply: " + json_string);
		Dictionary<string, object> results = JSONObject.Deserialize(json_string);
		Dictionary<string, object> test1 = (Dictionary<string, object>)results["1"];
		Debug.Log(test1["facebook_id"]);
	}
}
