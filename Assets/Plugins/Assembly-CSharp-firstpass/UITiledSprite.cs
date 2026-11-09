using UnityEngine;

[AddComponentMenu("NGUI/UI/Sprite (Tiled)")]
[ExecuteInEditMode]
public class UITiledSprite : UISlicedSprite
{
	[SerializeField]
	[HideInInspector]
	public int tileWidth;

	[HideInInspector]
	[SerializeField]
	public int tileHeight;

	public override Vector4 border
	{
		get
		{
			return Vector4.zero;
		}
	}

	public override void MakePixelPerfect()
	{
		Vector3 localPosition = base.cachedTransform.localPosition;
		localPosition.x = Mathf.RoundToInt(localPosition.x);
		localPosition.y = Mathf.RoundToInt(localPosition.y);
		localPosition.z = Mathf.RoundToInt(localPosition.z);
		base.cachedTransform.localPosition = localPosition;
		Vector3 localScale = base.cachedTransform.localScale;
		localScale.x = Mathf.RoundToInt(localScale.x);
		localScale.y = Mathf.RoundToInt(localScale.y);
		localScale.z = 1f;
		base.cachedTransform.localScale = localScale;
	}

	public override void OnFill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		Texture texture = material.mainTexture;
		if (texture == null)
		{
			return;
		}
		Rect rect = mInner;
		if (base.atlas.coordinates == UIAtlas.Coordinates.TexCoords)
		{
			rect = NGUIMath.ConvertToPixels(rect, texture.width, texture.height, true);
		}
		Vector2 vector = base.cachedTransform.localScale;
		float pixelSize = base.atlas.pixelSize;
		float num = rect.width;
		float num2 = rect.height;
		if (tileWidth != 0 && tileHeight != 0)
		{
			num = tileWidth;
			num2 = tileHeight;
		}
		float num3 = Mathf.Abs(num / vector.x) * pixelSize;
		float num4 = Mathf.Abs(num2 / vector.y) * pixelSize;
		if (num3 < 0.01f || num4 < 0.01f)
		{
			Debug.LogWarning("The tiled sprite (" + NGUITools.GetHierarchy(base.gameObject) + ") is too small.\nConsider using a bigger one.");
			num3 = 0.01f;
			num4 = 0.01f;
		}
		Vector2 vector2 = new Vector2(rect.xMin / (float)texture.width, rect.yMin / (float)texture.height);
		Vector2 vector3 = new Vector2(rect.xMax / (float)texture.width, rect.yMax / (float)texture.height);
		Vector2 vector4 = vector3;
		for (float num5 = 0f; num5 < 1f; num5 += num4)
		{
			float num6 = 0f;
			vector4.x = vector3.x;
			float num7 = num5 + num4;
			if (num7 > 1f)
			{
				vector4.y = vector2.y + (vector3.y - vector2.y) * (1f - num5) / (num7 - num5);
				num7 = 1f;
			}
			for (; num6 < 1f; num6 += num3)
			{
				float num8 = num6 + num3;
				if (num8 > 1f)
				{
					vector4.x = vector2.x + (vector3.x - vector2.x) * (1f - num6) / (num8 - num6);
					num8 = 1f;
				}
				verts.Add(new Vector3(num8, 0f - num5, 0f));
				verts.Add(new Vector3(num8, 0f - num7, 0f));
				verts.Add(new Vector3(num6, 0f - num7, 0f));
				verts.Add(new Vector3(num6, 0f - num5, 0f));
				uvs.Add(new Vector2(vector4.x, 1f - vector2.y));
				uvs.Add(new Vector2(vector4.x, 1f - vector4.y));
				uvs.Add(new Vector2(vector2.x, 1f - vector4.y));
				uvs.Add(new Vector2(vector2.x, 1f - vector2.y));
				cols.Add(base.color);
				cols.Add(base.color);
				cols.Add(base.color);
				cols.Add(base.color);
			}
		}
	}

	public override int CountVertices()
	{
		int num = 0;
		Texture texture = material.mainTexture;
		if (texture == null)
		{
			return num;
		}
		Rect rect = mInner;
		if (base.atlas.coordinates == UIAtlas.Coordinates.TexCoords)
		{
			rect = NGUIMath.ConvertToPixels(rect, texture.width, texture.height, true);
		}
		Vector2 vector = base.cachedTransform.localScale;
		float pixelSize = base.atlas.pixelSize;
		float num2 = rect.width;
		float num3 = rect.height;
		if (tileWidth != 0 && tileHeight != 0)
		{
			num2 = tileWidth;
			num3 = tileHeight;
		}
		float num4 = Mathf.Abs(num2 / vector.x) * pixelSize;
		float num5 = Mathf.Abs(num3 / vector.y) * pixelSize;
		if (num4 < 0.01f || num5 < 0.01f)
		{
			Debug.LogWarning("The tiled sprite (" + NGUITools.GetHierarchy(base.gameObject) + ") is too small.\nConsider using a bigger one.");
			num4 = 0.01f;
			num5 = 0.01f;
		}
		for (float num6 = 0f; num6 < 1f; num6 += num5)
		{
			for (float num7 = 0f; num7 < 1f; num7 += num4)
			{
				num += 4;
			}
		}
		return num;
	}
}
