using System;
using System.Collections.Generic;
using UnityEngine;

public class NBContainerUtil
{
	public static T RandomElement<T>(T[] p_list)
	{
		if (p_list != null && p_list.Length != 0)
		{
			return p_list[UnityEngine.Random.Range(0, p_list.Length)];
		}
		Dbg.LogWarn("Cannot random from the empty list: " + p_list);
		return default(T);
	}

	public static T RandomElement<T>(List<T> p_list)
	{
		if (p_list != null && p_list.Count != 0)
		{
			return p_list[UnityEngine.Random.Range(0, p_list.Count)];
		}
		Dbg.LogWarn("Cannot random from the empty list: " + p_list);
		return default(T);
	}

	public static bool IsContain<T>(T[] p_array, T p_value)
	{
		return Find(p_array, p_value) != null;
	}

	public static T Find<T>(T[] p_array, T p_value)
	{
		return Array.Find(p_array, (T content) => content.Equals(p_value));
	}

	public static bool IsNullOrEmpty<T>(T[] p_array)
	{
		return p_array == null || p_array.Length <= 0;
	}
}
