using System;
using System.Collections.Generic;
using UnityEngine;

public static class TransformExtension
{
	public static Transform FindInHierarchy(this Transform p_current, string p_name)
	{
		Func<Transform, object, bool> p_function = (Transform current, object param) => current.name == p_name;
		Transform transform = p_current.Iterate(p_function);
		if (transform == null)
		{
			Debug.LogWarning(p_name + " cannot be found.");
		}
		return transform;
	}

	public static Transform FindInHierarchy(this Transform p_current, Func<Transform, object, bool> p_function, object p_param)
	{
		Func<Transform, object, bool> p_function2 = (Transform current, object param) => p_function(current, param);
		return p_current.Iterate(p_function2, p_param);
	}

	public static Transform[] FindChildrenInHierarchy(this Transform p_current, string p_name)
	{
		List<Transform> list = new List<Transform>();
		Func<Transform, object, bool> p_function = delegate(Transform current, object childList)
		{
			if (current.name == p_name)
			{
				((List<Transform>)childList).Add(current);
			}
			return false;
		};
		p_current.Iterate(p_function, list);
		if (list.Count == 0)
		{
			Debug.LogWarning(p_name + " cannot be found.");
		}
		return list.ToArray();
	}

	public static List<T> FindComponentsInHierarchy<T>(this Transform p_current) where T : Component
	{
		List<T> children = new List<T>();
		Func<Transform, object, bool> p_function = delegate(Transform current, object arg)
		{
			T component = current.GetComponent<T>();
			if (component != null)
			{
				children.Add(component);
			}
			return false;
		};
		p_current.Iterate(p_function);
		if (children.Count == 0)
		{
			Debug.LogWarning("Unable to found type");
		}
		return children;
	}

	public static Transform[] FindChildrenInHierarchy(this Transform p_current, Func<Transform, object, bool> p_function, object p_param)
	{
		List<Transform> list = new List<Transform>();
		Func<Transform, object, bool> p_function2 = delegate(Transform current, object childList)
		{
			if (p_function(current, p_param))
			{
				((List<Transform>)childList).Add(current);
			}
			return false;
		};
		p_current.Iterate(p_function2, list);
		return list.ToArray();
	}

	public static string GetPathTo(this Transform p_current, Transform p_target)
	{
		Transform transform = p_current;
		string text = transform.name;
		bool flag = false;
		while (transform != null && transform.parent != null)
		{
			text = transform.parent.name + "/" + text;
			if (transform.parent == p_target)
			{
				flag = true;
				break;
			}
			transform = transform.parent;
		}
		if (flag)
		{
			return text;
		}
		return string.Empty;
	}

	public static string GetPathBetween(this Transform p_current, Transform p_target)
	{
		string text = string.Empty;
		Transform transform = p_current;
		bool flag = false;
		while (transform != null && transform.parent != null)
		{
			if (transform.parent == p_target)
			{
				flag = true;
				break;
			}
			text = transform.parent.name + "/" + text;
			transform = transform.parent;
		}
		if (flag)
		{
			int num = text.LastIndexOf('/');
			if (num >= 0)
			{
				return text.Remove(num);
			}
			return string.Empty;
		}
		return string.Empty;
	}

	public static float GetLookAngle(this Transform p_this, Transform p_lookTarget)
	{
		return p_this.GetLookAngle(p_lookTarget.position);
	}

	public static float GetLookAngle(this Transform p_this, Vector3 p_lookTarget)
	{
		Vector3 vector = p_this.InverseTransformPoint(p_lookTarget);
		return Mathf.Atan2(vector.x, vector.z) * 57.29578f;
	}

	public static Transform Iterate(this Transform p_current, Func<Transform, object, bool> p_function, object p_param = null)
	{
		if (p_function(p_current, p_param))
		{
			return p_current;
		}
		for (int i = 0; i < p_current.childCount; i++)
		{
			Transform transform = p_current.GetChild(i).Iterate(p_function, p_param);
			if (transform != null)
			{
				return transform;
			}
		}
		return null;
	}

	public static void Copy(this Transform p_this, Transform p_source, bool p_sharedParent = false)
	{
		p_this.position = p_source.position;
		p_this.rotation = p_source.rotation;
		if (p_sharedParent)
		{
			p_this.parent = p_source.parent;
		}
	}

	public static void SetLayerRecursively(this Transform p_this, int layer)
	{
		Transform[] componentsInChildren = MBHier.GetComponentsInChildren<Transform>(p_this, true);
		Transform[] array = componentsInChildren;
		foreach (Transform transform in array)
		{
			transform.gameObject.layer = layer;
		}
	}

	public static void AttachTo(this Transform p_this, Transform p_parent, TransformAttachMode p_mode = TransformAttachMode.RETAIN_GLOBAL_TRANSFORM)
	{
		if (null != p_parent)
		{
			Vector3 localPosition = p_this.localPosition;
			Vector3 localScale = p_this.localScale;
			Quaternion localRotation = p_this.localRotation;
			p_this.parent = p_parent;
			switch (p_mode)
			{
			case TransformAttachMode.PARENT_ORIGIN:
				p_this.localPosition = Vector3.zero;
				p_this.localRotation = Quaternion.identity;
				p_this.localScale = Vector3.one;
				break;
			case TransformAttachMode.RETAIN_LOCAL_TRANSFORM:
				p_this.localPosition = localPosition;
				p_this.localRotation = localRotation;
				p_this.localScale = localScale;
				break;
			}
		}
	}

	public static void SetLocalX(this Transform p_this, float p_value)
	{
		Vector3 localPosition = p_this.localPosition;
		localPosition.x = p_value;
		p_this.localPosition = localPosition;
	}

	public static void SetLocalY(this Transform p_this, float p_value)
	{
		Vector3 localPosition = p_this.localPosition;
		localPosition.y = p_value;
		p_this.localPosition = localPosition;
	}

	public static void SetLocalZ(this Transform p_this, float p_value)
	{
		Vector3 localPosition = p_this.localPosition;
		localPosition.z = p_value;
		p_this.localPosition = localPosition;
	}
}
