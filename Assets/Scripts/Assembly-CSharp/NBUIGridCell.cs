using UnityEngine;

public class NBUIGridCell<T> : MonoBehaviour where T : class
{
	[SerializeField]
	[HideInInspector]
	public int row;

	[SerializeField]
	[HideInInspector]
	public int col;

	[SerializeField]
	[HideInInspector]
	public float width;

	[HideInInspector]
	[SerializeField]
	public float height;

	[SerializeField]
	[HideInInspector]
	public int cellIndex;

	protected T m_data;

	public virtual T data
	{
		get
		{
			return m_data;
		}
		set
		{
			m_data = value;
		}
	}

	protected virtual void Awake()
	{
		Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(base.transform);
		width = bounds.size.x;
		height = bounds.size.y;
		BoxCollider boxCollider = base.GetComponent<Collider>() as BoxCollider;
		if ((bool)boxCollider)
		{
			boxCollider.bounds.Encapsulate(bounds);
		}
	}

	protected virtual void Start()
	{
		Object.Destroy(GetComponent<UIPanel>());
	}

	public virtual void SetLayerRecursively(int layer)
	{
		Transform[] componentsInChildren = MBHier.GetComponentsInChildren<Transform>(this, true);
		Transform[] array = componentsInChildren;
		foreach (Transform transform in array)
		{
			transform.gameObject.layer = layer;
		}
	}
}
