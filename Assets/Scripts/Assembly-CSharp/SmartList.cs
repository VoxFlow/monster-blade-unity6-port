using UnityEngine;

public class SmartList : MonoBehaviour
{
	public UIList list;

	public int initialCapacity = 10;

	public bool grow = true;

	public int growCount = 10;

	public bool snap = true;

	public Vector2 snapOffset = Vector2.zero;

	private UIDragCamera _viewport;

	private bool _bouncing;

	private bool _snapped;

	private int _lastCapacity;

	private int _lastVisibleCellsCount;

	private int _visibleCellsCount;

	private bool _init;

	public int Capacity
	{
		get
		{
			if (list != null)
			{
				return list.ListData.Length;
			}
			return 0;
		}
	}

	public int VisibleCellsCount
	{
		get
		{
			return _visibleCellsCount;
		}
		set
		{
			_visibleCellsCount = value;
			UpdateCollider();
			_lastVisibleCellsCount = _visibleCellsCount;
		}
	}

	private void OnEnable()
	{
		Init();
	}

	private void OnDisable()
	{
		Reset();
	}

	private void Start()
	{
		Init();
	}

	private void Reset()
	{
		_init = false;
		_lastCapacity = 0;
		_lastVisibleCellsCount = (_visibleCellsCount = 0);
	}

	private void Init()
	{
		if (_init)
		{
			return;
		}
		list.Resize(initialCapacity, initialCapacity, 1, 3);
		_viewport = list.GetComponent<UIDragCamera>();
		if (list.ListData.Length > 0)
		{
			UpdateCollider();
			GameObject[] listData = list.ListData;
			foreach (GameObject gameObject in listData)
			{
				gameObject.SetActiveRecursively(false);
			}
			_lastCapacity = list.ListData.Length;
		}
		_init = true;
	}

	public GameObject GetCellObjectAt(int index)
	{
		if (index < list.ListData.Length)
		{
			return list.ListData[index];
		}
		return null;
	}

	public GameObject AddVisibleCell()
	{
		int visibleCellsCount = _visibleCellsCount;
		_visibleCellsCount++;
		if (_visibleCellsCount >= list.ListData.Length)
		{
			Grow();
		}
		GameObject cellObjectAt = GetCellObjectAt(visibleCellsCount);
		cellObjectAt.SetActiveRecursively(true);
		return cellObjectAt;
	}

	public void Refresh()
	{
		UpdateCollider();
	}

	private void UpdateCollider()
	{
		if (_lastVisibleCellsCount == _visibleCellsCount)
		{
			return;
		}
		BoxCollider boxCollider = list.GetComponent<Collider>() as BoxCollider;
		Vector3 size = boxCollider.size;
		if (list.ListData.Length > 0)
		{
			GameObject gameObject = list.ListData[0];
			UISprite componentInChildren = MBHier.GetComponentInChildren<UISprite>(gameObject);
			if ((bool)componentInChildren)
			{
				size.x = componentInChildren.cachedTransform.localScale.x;
			}
		}
		size.y = 0f;
		Debug.LogWarning("Visible cells count = " + _visibleCellsCount);
		for (int i = 0; i < _visibleCellsCount; i++)
		{
			size.y += list.spacingVertical;
		}
		boxCollider.size = size;
		Vector3 center = boxCollider.center;
		center.y = (0f - boxCollider.size.y) * 0.5f;
		boxCollider.center = center;
	}

	private void Snap()
	{
		int spacingVertical = list.spacingVertical;
		Vector3 localPosition = _viewport.target.transform.localPosition;
		localPosition.y -= snapOffset.y;
		localPosition.y = ((float)Mathf.RoundToInt(localPosition.y / (float)spacingVertical) + 0.5f) * (float)spacingVertical - snapOffset.y;
		_viewport.target.transform.localPosition = localPosition;
	}

	public void Grow()
	{
		list.AddCells(growCount);
		for (int i = _lastCapacity; i < list.ListData.Length; i++)
		{
			GameObject gameObject = list.ListData[i];
			gameObject.SetActiveRecursively(false);
		}
		_lastCapacity = list.ListData.Length;
	}

	private void NotifyGrow()
	{
		Debug.LogWarning("Grow list.");
	}

	private void Update()
	{
		if (!_viewport)
		{
			return;
		}
		if (grow)
		{
			if (!_bouncing)
			{
				if (_viewport.Offset.y < 0f)
				{
					NotifyGrow();
					_bouncing = true;
				}
			}
			else if (_viewport.Offset.y >= 0f)
			{
				_bouncing = false;
			}
		}
		if (snap && !_snapped && _viewport.IsStopped())
		{
			_snapped = true;
			_viewport.ForceStop();
			Snap();
		}
		else if (_snapped && _viewport.IsPressed && _viewport.Momentum.sqrMagnitude > 0.01f)
		{
			_snapped = false;
		}
	}
}
