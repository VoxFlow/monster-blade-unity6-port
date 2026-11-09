using System;
using System.Reflection;
using UnityEngine;

public class JSONObjectDeserializer
{
	public static T Deserialize<T>(JSONObject jObject) where T : class, new()
	{
		T val = new T();
		Type typeFromHandle = typeof(T);
		FieldInfo[] fields = typeFromHandle.GetFields();
		Debug.Log("Deserializing JSONObject for " + typeFromHandle);
		for (int i = 0; i < fields.Length; i++)
		{
			if (jObject[fields[i].Name].type == JSONObject.Type.STRING)
			{
				fields[i].SetValue(val, jObject[fields[i].Name].str);
			}
			else if (jObject[fields[i].Name].type == JSONObject.Type.NUMBER)
			{
				if (fields[i].GetType() == typeof(int))
				{
					fields[i].SetValue(val, (int)jObject[fields[i].Name].n);
				}
				else
				{
					fields[i].SetValue(val, jObject[fields[i].Name].n);
				}
			}
			else if (jObject[fields[i].Name].type == JSONObject.Type.BOOL)
			{
				fields[i].SetValue(val, jObject[fields[i].Name].b);
			}
			else
			{
				Debug.LogError("There is no valid type deserialization for " + fields[i].Name);
			}
		}
		return val;
	}
}
