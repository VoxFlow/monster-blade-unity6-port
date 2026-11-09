using UnityEngine;

public class ReassignUvAndColor : MonoBehaviour
{
	private void OnApplicationPause(bool p_pause)
	{
		Debug.Log("OnApplicationPause: " + p_pause);
		if (p_pause)
		{
			return;
		}
		Object[] array = Object.FindObjectsOfType(typeof(SkinnedMeshRenderer));
		Object[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			SkinnedMeshRenderer skinnedMeshRenderer = (SkinnedMeshRenderer)array2[i];
			Debug.Log("Restoring color and uv for " + skinnedMeshRenderer);
			Debug.Log("Vertex 0: " + skinnedMeshRenderer.sharedMesh.vertices[0]);
			if (skinnedMeshRenderer.sharedMesh.colors.Length > 0)
			{
				Debug.Log("Color 0: " + skinnedMeshRenderer.sharedMesh.colors[0]);
			}
			if (skinnedMeshRenderer.sharedMesh.colors32.Length > 0)
			{
				Debug.Log("Color32 0: " + skinnedMeshRenderer.sharedMesh.colors32[0]);
			}
			if (skinnedMeshRenderer.sharedMesh.uv.Length > 0)
			{
				Debug.Log("UV 0: " + skinnedMeshRenderer.sharedMesh.uv[0]);
			}
			skinnedMeshRenderer.sharedMesh.vertices = skinnedMeshRenderer.sharedMesh.vertices;
			skinnedMeshRenderer.sharedMesh.colors = skinnedMeshRenderer.sharedMesh.colors;
			skinnedMeshRenderer.sharedMesh.colors32 = skinnedMeshRenderer.sharedMesh.colors32;
			skinnedMeshRenderer.sharedMesh.uv = skinnedMeshRenderer.sharedMesh.uv;
			skinnedMeshRenderer.sharedMaterial = skinnedMeshRenderer.sharedMaterial;
			skinnedMeshRenderer.sharedMaterial.mainTexture = skinnedMeshRenderer.sharedMaterial.mainTexture;
		}
		array = Object.FindObjectsOfType(typeof(MeshRenderer));
		Object[] array3 = array;
		for (int j = 0; j < array3.Length; j++)
		{
			MeshRenderer meshRenderer = (MeshRenderer)array3[j];
			Debug.Log("Restoring color and uv for " + meshRenderer);
			meshRenderer.sharedMaterial = meshRenderer.sharedMaterial;
			meshRenderer.sharedMaterial.mainTexture = meshRenderer.sharedMaterial.mainTexture;
		}
	}
}
