using UnityEngine;

public class NewEggRevealItem : MonoBehaviour
{
	public SkinnedMeshRenderer[] m_childRenderers;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void SetEggFracTextureBasedOnElement(int p_index)
	{
		float x = (float)(p_index % 4) * 0.25f;
		float y = 0.75f - (float)(int)Mathf.Floor((float)p_index / 4f) * 0.25f;
		SkinnedMeshRenderer[] childRenderers = m_childRenderers;
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in childRenderers)
		{
			skinnedMeshRenderer.material.SetTextureOffset("_MainTex", new Vector2(x, y));
		}
	}
}
