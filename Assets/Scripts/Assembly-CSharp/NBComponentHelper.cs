using UnityEngine;

public static class NBComponentHelper
{
	public static T GetOrAddComponent<T>(GameObject _gao) where T : Component
	{
		T val = _gao.GetComponent<T>();
		if (val == null)
		{
			val = _gao.AddComponent<T>();
		}
		return val;
	}
}
