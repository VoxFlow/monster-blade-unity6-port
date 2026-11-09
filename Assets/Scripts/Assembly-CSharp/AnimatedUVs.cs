using UnityEngine;

public class AnimatedUVs : MonoBehaviour
{
	public int materialIndex;

	public Vector2 uvAnimationRate = new Vector2(1f, 0f);

	public string textureName = "_MainTex";

	public Vector2 uvOffset = Vector2.zero;

	private Material material;

	private void LateUpdate()
	{
		uvOffset += uvAnimationRate * Time.deltaTime;
		if (base.GetComponent<Renderer>().enabled)
		{
			if (material == null && base.GetComponent<Renderer>() != null && materialIndex < base.GetComponent<Renderer>().materials.Length)
			{
				material = base.GetComponent<Renderer>().materials[materialIndex];
			}
			material.SetTextureOffset(textureName, uvOffset);
		}
	}
}
