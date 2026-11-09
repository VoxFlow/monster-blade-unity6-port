using UnityEngine;

[AddComponentMenu("NGUI/Deprecated/Grow On Press")]
[ExecuteInEditMode]
public class UIGrowOnPress : MonoBehaviour
{
	public float duration = 0.25f;

	public Vector3 amount = new Vector3(1.1f, 1.1f, 1.1f);

	private void Awake()
	{
		Debug.LogWarning(string.Concat(GetType(), " is obsolete. Replacing with UIButtonScale instead."));
		UIButtonScale uIButtonScale = base.gameObject.GetComponent<UIButtonScale>();
		if (uIButtonScale == null)
		{
			uIButtonScale = base.gameObject.AddComponent<UIButtonScale>();
		}
		uIButtonScale.pressed = amount;
		uIButtonScale.duration = duration;
		Object.DestroyImmediate(this);
	}
}
