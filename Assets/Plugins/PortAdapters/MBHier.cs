// GetComponent(s)InChildren with Unity 4.0.1's result for a caller that is inactive in the hierarchy. Register CD-056.
using System;
using System.Collections.Generic;
using UnityEngine;

public static class MBHier
{
	private static readonly Dictionary<string, bool> s_logged = new Dictionary<string, bool>();

	private static void Note(GameObject g, string call, Type t, int unity6)
	{
		if (unity6 == 0 || s_logged.Count >= 200) return;
		string key = g.GetInstanceID() + "|" + call + "|" + t.Name;
		if (s_logged.ContainsKey(key)) return;
		s_logged[key] = true;
		string path = g.name;
		for (Transform p = g.transform.parent; p != null; p = p.parent) path = p.name + "/" + path;
		Debug.Log("[MBHier] inactive caller " + path + " (activeSelf " + g.activeSelf + "): " + call + "<" + t.Name + "> -> none, Unity 6 would return " + unity6);
	}

	public static T GetComponentInChildren<T>(GameObject g)
	{
		if (g.activeInHierarchy) return g.GetComponentInChildren<T>();
		Note(g, "GetComponentInChildren", typeof(T), g.GetComponentInChildren<T>() != null ? 1 : 0);
		return default(T);
	}

	public static T GetComponentInChildren<T>(Component c)
	{
		return GetComponentInChildren<T>(c.gameObject);
	}

	public static T[] GetComponentsInChildren<T>(GameObject g)
	{
		return GetComponentsInChildren<T>(g, false);
	}

	public static T[] GetComponentsInChildren<T>(Component c)
	{
		return GetComponentsInChildren<T>(c.gameObject, false);
	}

	public static T[] GetComponentsInChildren<T>(GameObject g, bool includeInactive)
	{
		if (includeInactive || g.activeInHierarchy) return g.GetComponentsInChildren<T>(includeInactive);
		Note(g, "GetComponentsInChildren", typeof(T), g.GetComponentsInChildren<T>().Length);
		return new T[0];
	}

	public static T[] GetComponentsInChildren<T>(Component c, bool includeInactive)
	{
		return GetComponentsInChildren<T>(c.gameObject, includeInactive);
	}

	public static Component[] GetComponentsInChildren(GameObject g, Type type, bool includeInactive)
	{
		if (includeInactive || g.activeInHierarchy) return g.GetComponentsInChildren(type, includeInactive);
		Note(g, "GetComponentsInChildren", type, g.GetComponentsInChildren(type).Length);
		return new Component[0];
	}

	public static Component[] GetComponentsInChildren(Component c, Type type, bool includeInactive)
	{
		return GetComponentsInChildren(c.gameObject, type, includeInactive);
	}
}
