using System.Collections;
using UnityEngine;

public class SkinnedMeshHelper : MonoBehaviour
{
	public SkinnedMeshRenderer source;

	private CWeightList[] nodeWeights;

	private Vector3[] newVert;

	private Mesh mesh;

	public Mesh deformedMesh
	{
		get
		{
			return mesh;
		}
	}

	public IEnumerator ExtractMesh()
	{
		SkinnedMeshRenderer rend = source;
		if (rend != null)
		{
			Mesh baseMesh = rend.sharedMesh;
			mesh = new Mesh();
			mesh.vertices = baseMesh.vertices;
			mesh.uv = baseMesh.uv;
			mesh.triangles = baseMesh.triangles;
			newVert = new Vector3[baseMesh.vertices.Length];
			nodeWeights = new CWeightList[rend.bones.Length];
			for (short i = 0; i < rend.bones.Length; i++)
			{
				nodeWeights[i] = new CWeightList();
				nodeWeights[i].transform = rend.bones[i];
			}
			for (short i = 0; i < baseMesh.vertices.Length; i++)
			{
				BoneWeight bw = baseMesh.boneWeights[i];
				if (bw.weight0 != 0f)
				{
					Vector3 localPt = baseMesh.bindposes[bw.boneIndex0].MultiplyPoint3x4(baseMesh.vertices[i]);
					nodeWeights[bw.boneIndex0].weights.Add(new CVertexWeight(i, localPt, bw.weight0));
				}
				if (bw.weight1 != 0f)
				{
					Vector3 localPt = baseMesh.bindposes[bw.boneIndex1].MultiplyPoint3x4(baseMesh.vertices[i]);
					nodeWeights[bw.boneIndex1].weights.Add(new CVertexWeight(i, localPt, bw.weight1));
				}
				if (bw.weight2 != 0f)
				{
					Vector3 localPt = baseMesh.bindposes[bw.boneIndex2].MultiplyPoint3x4(baseMesh.vertices[i]);
					nodeWeights[bw.boneIndex2].weights.Add(new CVertexWeight(i, localPt, bw.weight2));
				}
				if (bw.weight3 != 0f)
				{
					Vector3 localPt = baseMesh.bindposes[bw.boneIndex3].MultiplyPoint3x4(baseMesh.vertices[i]);
					nodeWeights[bw.boneIndex3].weights.Add(new CVertexWeight(i, localPt, bw.weight3));
				}
				if (i % 1000 == 0)
				{
					yield return null;
				}
			}
			yield return null;
			yield return StartCoroutine(UpdateCollisionMesh());
		}
		else
		{
			Debug.LogError(base.gameObject.name + ": SkinnedCollisionHelper: this object either has no SkinnedMeshRenderer!");
		}
	}

	private IEnumerator UpdateCollisionMesh()
	{
		if (!(mesh != null))
		{
			yield break;
		}
		for (int i = 0; i < newVert.Length; i++)
		{
			newVert[i] = Vector3.zero;
		}
		CWeightList[] array = nodeWeights;
		foreach (CWeightList wList in array)
		{
			foreach (CVertexWeight vw in wList.weights)
			{
				newVert[vw.index] += wList.transform.localToWorldMatrix.MultiplyPoint3x4(vw.localPosition) * vw.weight;
			}
		}
		for (int k = 0; k < newVert.Length; k++)
		{
			newVert[k] = base.transform.InverseTransformPoint(newVert[k]);
			if (k % 1000 == 0)
			{
				yield return null;
			}
		}
		mesh.vertices = newVert;
		mesh.RecalculateNormals();
	}
}
