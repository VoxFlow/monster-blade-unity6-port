using System.Collections.Generic;
using UnityEngine;

public class NBUIGrid2D<T, Data> : MonoBehaviour where T : NBUIGridCell<Data> where Data : class
{
	public T cellPrefab;

	public int maxRow;

	public int maxCol;

	public float vSpace;

	public float hSpace;

	public bool NeedToExtendCell;

	private UIDraggablePanel m_panel;

	protected List<T> m_allCells = new List<T>();

	protected List<T> m_visibleCells = new List<T>();

	private List<Data> _data = new List<Data>();

	public List<T> cells
	{
		get
		{
			return m_allCells;
		}
	}

	public List<T> visibleCells
	{
		get
		{
			return m_visibleCells;
		}
	}

	protected virtual void Awake()
	{
		m_panel = NGUITools.FindInParents<UIDraggablePanel>(base.transform.parent.gameObject);
		Dbg.Assert(m_panel);
	}

	public void Clear()
	{
		List<T>.Enumerator enumerator = m_allCells.GetEnumerator();
		while (enumerator.MoveNext())
		{
			T current = enumerator.Current;
			if (current != null)
			{
				Object.DestroyImmediate(current.gameObject);
			}
		}
		m_allCells.Clear();
		m_visibleCells.Clear();
		_data.Clear();
		Reposition();
	}

	public void AddData(Data data)
	{
		_data.Add(data);
	}

	public void RemoveData(Data data)
	{
		if (_data.Contains(data))
		{
			_data.Remove(data);
		}
	}

	public void Hide(T p_cell)
	{
		if (!(null == p_cell))
		{
			m_visibleCells.Remove(p_cell);
			p_cell.gameObject.SetActiveRecursively(false);
		}
	}

	public void Remove(T p_cell)
	{
		if (!(p_cell == null))
		{
			m_allCells.Remove(p_cell);
			m_visibleCells.Remove(p_cell);
			Object.DestroyImmediate(p_cell.gameObject);
			Reposition();
		}
	}

	public void RemoveCells(List<T> p_cells, bool p_reposition = true)
	{
		if (p_cells != null)
		{
			int count = p_cells.Count;
			for (int num = count - 1; num >= 0; num--)
			{
				T item = p_cells[num];
				m_allCells.Remove(item);
				m_visibleCells.Remove(item);
				Object.DestroyImmediate(item.gameObject);
			}
			if (p_reposition)
			{
				Reposition();
			}
		}
	}

	public void Reposition(bool p_includeInactive = false)
	{
		int num = 0;
		foreach (T visibleCell in m_visibleCells)
		{
			T current = visibleCell;
			if (!(current == null) && (p_includeInactive || current.gameObject.active))
			{
				float z = current.transform.localPosition.z;
				if (maxRow == 1 && maxCol == 0)
				{
					current.col = num;
					current.transform.localPosition = new Vector3((float)current.col * (current.width + hSpace), 0f, z);
				}
				else if (maxRow == 0 && maxCol == 1)
				{
					current.row = num;
					current.transform.localPosition = new Vector3(0f, (float)(-current.row) * (current.height + vSpace), z);
				}
				else
				{
					current.col = num % maxCol;
					current.row = num / maxCol;
					current.cellIndex = num;
					Vector3 zero = Vector3.zero;
					zero.x = (float)current.col * (current.width + hSpace);
					zero.y = (float)(-current.row) * (current.height + vSpace);
					zero.z = z;
					current.transform.localPosition = zero;
				}
				num++;
			}
		}
	}

	public T AddCell(float p_cusWidth = 0f, float p_cusHeight = 0f)
	{
		T val = Object.Instantiate(cellPrefab) as T;
		int index = (val.cellIndex = m_allCells.Count);
		val.data = _data[index];
		float z = val.transform.localPosition.z;
		val.transform.parent = base.transform;
		val.transform.localScale = Vector3.one;
		val.SetLayerRecursively(base.gameObject.layer);
		int count = m_visibleCells.Count;
		m_allCells.Add(val);
		m_visibleCells.Add(val);
		if (maxRow == 1 && maxCol == 0)
		{
			val.col = count;
			if (p_cusWidth > 0f)
			{
				val.transform.localPosition = new Vector3((float)val.col * (p_cusWidth + hSpace), 0f, z);
			}
			else
			{
				val.transform.localPosition = new Vector3((float)val.col * (val.width + hSpace), 0f, z);
			}
		}
		else if (maxRow == 0 && maxCol == 1)
		{
			val.row = count;
			if (p_cusHeight > 0f)
			{
				val.transform.localPosition = new Vector3(0f, (float)(-val.row) * (p_cusHeight + vSpace), z);
			}
			else
			{
				val.transform.localPosition = new Vector3(0f, (float)(-val.row) * (val.height + vSpace), z);
			}
		}
		else
		{
			val.col = count % maxCol;
			val.row = count / maxCol;
			Vector3 zero = Vector3.zero;
			zero.x = (float)val.col * (val.width + hSpace);
			zero.y = (float)(-val.row) * (val.height + vSpace);
			zero.z = z;
			val.transform.localPosition = zero;
		}
		BoxCollider boxCollider = val.GetComponent<Collider>() as BoxCollider;
		if (NeedToExtendCell && null != boxCollider)
		{
			Vector3 zero2 = Vector3.zero;
			if (hSpace > 0f)
			{
				zero2.x = hSpace;
			}
			if (vSpace > 0f)
			{
				zero2.y = vSpace;
			}
			Bounds bounds = boxCollider.bounds;
			bounds.size = new Vector3(val.width, val.height, 1f);
			bounds.Expand(zero2);
			boxCollider.size = bounds.size;
		}
		return val;
	}
}
