using System;
using UnityEngine;

public static class CopySMR
{
	public static void SetSMRenderer(SkinnedMeshRenderer renderer, GameObject target)
	{
		SkinnedMeshRenderer skinnedMeshRenderer = (SkinnedMeshRenderer)target.AddComponent(typeof(SkinnedMeshRenderer));
		skinnedMeshRenderer.sharedMesh = renderer.sharedMesh;
		skinnedMeshRenderer.sharedMaterials = renderer.sharedMaterials;
		Transform[] array = new Transform[renderer.bones.Length];
		for (int i = 0; i < renderer.bones.Length; i++)
		{
			Transform transform = null;
			Component[] componentsInChildren = MBHier.GetComponentsInChildren(target, typeof(Transform), true);
			foreach (Component component in componentsInChildren)
			{
				if (component.name == renderer.bones[i].name)
				{
					transform = component.transform;
					break;
				}
			}
			if (transform == null)
			{
				throw new Exception("Bone not found: " + renderer.bones[i].name);
			}
			array[i] = transform;
		}
		skinnedMeshRenderer.bones = array;
	}
}
