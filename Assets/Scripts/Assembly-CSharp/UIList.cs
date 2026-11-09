using System.Collections.Generic;
using UnityEngine;

public class UIList : MonoBehaviour
{
	public GameObject template;

	protected GameObject[] m_itemArray;

	public bool m_useDefaultSize = true;

	public bool m_codePopulated;

	public int m_offset;

	public int m_defaultWidth;

	public int m_defaultHeight;

	public int maxItemCount;

	public int maxRows;

	public int maxColumns;

	public int spacingHorizontal;

	public int spacingVertical;

	public int padding;

	public UIWidget background;

	public UICamera uiCamera;

	public int m_maxItemBeforeScroll;

	private bool m_allowScroll;

	private Vector2 m_defaultScale = Vector2.up;

	private bool m_started;

	public static string ScrollingLayer = "Scrolling";

	public GameObject[] ListData
	{
		get
		{
			return m_itemArray;
		}
		set
		{
			m_itemArray = value;
		}
	}

	private void OnEnable()
	{
		if (m_started)
		{
		}
	}

	public virtual void Start()
	{
		m_started = true;
		if (template == null)
		{
			Dbg.LogError("No template given. Disable List.");
			base.gameObject.SetActiveRecursively(false);
			base.enabled = false;
			return;
		}
		UIDragCamera component = template.GetComponent<UIDragCamera>();
		if (component != null)
		{
			m_defaultScale = new Vector2(component.scale.x, component.scale.y);
		}
		if (!m_codePopulated)
		{
			m_itemArray = new GameObject[maxItemCount];
			UpdateTable();
			SetScrollSize(m_maxItemBeforeScroll);
		}
	}

	public void SetScrollSize(int _sizeToScroll)
	{
		m_maxItemBeforeScroll = _sizeToScroll;
		AllowScrolling();
	}

	public void Resize(int p_size, int p_rowNumber, int p_columnNumber, int _scrollSize)
	{
		ResetData();
		m_itemArray = new GameObject[p_size];
		maxItemCount = p_size;
		maxRows = p_rowNumber;
		maxColumns = p_columnNumber;
		Dbg.Log("Resize UIList with total size : " + p_size + " number of rows : " + p_rowNumber + " number of Columns : " + p_columnNumber);
		UpdateTable();
		SetScrollSize(_scrollSize);
	}

	public void RepositionCells(GameObject[] _sortedList)
	{
		if (_sortedList == null)
		{
			Dbg.LogWarn("The given sorted list is null.");
			return;
		}
		for (int i = 0; i < maxRows; i++)
		{
			for (int j = 0; j < maxColumns; j++)
			{
				int num = i * maxColumns + j;
				if (num < 0 || num >= _sortedList.Length)
				{
					break;
				}
				GameObject gameObject = _sortedList[num];
				Transform transform = gameObject.transform;
				transform.localPosition = new Vector3(j * (m_defaultWidth + m_offset), -i * (m_defaultHeight + m_offset), 0f);
			}
		}
	}

	public List<GameObject> AddCells(int _addCell)
	{
		if (template == null)
		{
			return null;
		}
		if (m_itemArray == null)
		{
			return null;
		}
		List<GameObject> list = new List<GameObject>();
		Transform parent = base.transform;
		List<GameObject> list2 = new List<GameObject>(m_itemArray);
		if (_addCell < 0)
		{
			if (Mathf.Abs(_addCell) > m_itemArray.Length)
			{
				return null;
			}
			for (int i = list2.Count + _addCell; i < list2.Count; i++)
			{
				GameObject gameObject = list2[i];
				if (gameObject != null)
				{
					Object.Destroy(gameObject);
					gameObject = null;
				}
			}
			list2.RemoveRange(list2.Count + _addCell, -_addCell);
			m_itemArray = list2.ToArray();
			maxRows = (int)Mathf.Ceil((float)list2.Count / (float)maxColumns);
			maxItemCount += _addCell;
			Dbg.Log("Removed : " + -_addCell + " Cell counts : " + m_itemArray.Length + " MaxRow : " + maxRows);
			return list;
		}
		Dbg.Log("Adding : " + _addCell + " cells.");
		string empty = string.Empty;
		int num = list2.Count / maxColumns;
		int num2 = list2.Count % maxColumns;
		string text = empty;
		empty = text + "Starting Row : " + num + " ";
		Dbg.Log("Cell counts : " + m_itemArray.Length + "\nStarting row : " + num + " starting Column : " + num2);
		for (int j = 0; j < _addCell; j++)
		{
			GameObject gameObject2 = Object.Instantiate(template) as GameObject;
			Transform transform = gameObject2.transform;
			transform.parent = parent;
			if (m_useDefaultSize)
			{
				transform.localPosition = new Vector3(num2 * (m_defaultWidth + m_offset), -num * (m_defaultHeight + m_offset), 0f);
			}
			else
			{
				transform.localPosition = new Vector3((float)padding + ((float)num2 + 0.5f) * (float)spacingHorizontal, (float)(-padding) - ((float)num + 0.5f) * (float)spacingVertical, 0f);
			}
			transform.localRotation = Quaternion.identity;
			transform.localScale = Vector3.one;
			UIDragCamera component = gameObject2.GetComponent<UIDragCamera>();
			if ((bool)component)
			{
				component.SetTransform();
				if ((bool)uiCamera)
				{
					component.target = uiCamera.cachedCamera;
				}
				component.rootForBounds = base.transform;
			}
			num2++;
			if (num2 % maxColumns == 0)
			{
				num2 = 0;
				num++;
			}
			maxItemCount++;
			list2.Add(gameObject2);
			list.Add(gameObject2);
		}
		maxRows = (int)Mathf.Ceil((float)list2.Count / (float)maxColumns);
		m_itemArray = list2.ToArray();
		RepositionList();
		AllowScrolling();
		text = empty;
		empty = text + "Cell counts : " + m_itemArray.Length + " MaxRows : " + maxRows + " MaxItems : " + maxItemCount;
		Dbg.Log(empty);
		return list;
	}

	private void ResetData()
	{
		if (m_itemArray != null)
		{
			for (int i = 0; i < maxItemCount; i++)
			{
				Object.Destroy(m_itemArray[i]);
			}
		}
	}

	public void UpdateTable()
	{
		if (template == null)
		{
			return;
		}
		Dbg.Log("Updating List");
		int num = 0;
		Transform parent = base.transform;
		bool flag = true;
		for (int i = 0; i < maxRows; i++)
		{
			for (int j = 0; j < maxColumns; j++)
			{
				GameObject gameObject = Object.Instantiate(template) as GameObject;
				gameObject.name = num + string.Empty;
				gameObject.SetActiveRecursively(true);
				if (m_useDefaultSize && flag)
				{
					flag = false;
					UISprite[] componentsInChildren = MBHier.GetComponentsInChildren<UISprite>(gameObject);
					UISprite uISprite = MBHier.GetComponentInChildren<UISprite>(gameObject);
					UISprite[] array = componentsInChildren;
					foreach (UISprite uISprite2 in array)
					{
						if (uISprite2.gameObject.name.ToLower().Contains("background"))
						{
							uISprite = uISprite2;
						}
					}
					if (uISprite == null)
					{
						Dbg.LogError("No UISprite found in the template.\nYou cannot use Default size, Please give the parameter for spacing horizontal, vertical and padding, and un-tick useDefaultSize.");
					}
					else
					{
						m_defaultWidth = (int)(uISprite.relativeSize.x * uISprite.cachedTransform.localScale.x);
						m_defaultHeight = (int)(uISprite.relativeSize.y * uISprite.cachedTransform.localScale.y);
						Dbg.Log("Size width : " + m_defaultWidth + " height : " + m_defaultHeight + " with offset : " + m_offset);
					}
				}
				Transform transform = gameObject.transform;
				transform.parent = parent;
				if (m_useDefaultSize)
				{
					transform.localPosition = new Vector3(j * (m_defaultWidth + m_offset), -i * (m_defaultHeight + m_offset), 0f);
				}
				else
				{
					transform.localPosition = new Vector3((float)padding + ((float)j + 0.5f) * (float)spacingHorizontal, (float)(-padding) - ((float)i + 0.5f) * (float)spacingVertical, 0f);
				}
				transform.localRotation = Quaternion.identity;
				transform.localScale = Vector3.one;
				m_itemArray[num] = gameObject;
				UIDragCamera component = gameObject.GetComponent<UIDragCamera>();
				if ((bool)component)
				{
					component.SetTransform();
					if ((bool)uiCamera)
					{
						component.target = uiCamera.cachedCamera;
					}
					component.rootForBounds = base.transform;
				}
				if (++num >= maxItemCount)
				{
					break;
				}
			}
			if (num >= maxItemCount)
			{
				break;
			}
		}
		RepositionList();
		AllowScrolling();
	}

	private void AllowScrolling()
	{
		if (m_itemArray == null)
		{
			return;
		}
		Vector2 up = Vector2.up;
		if (m_itemArray.Length > m_maxItemBeforeScroll)
		{
			m_allowScroll = true;
			up = m_defaultScale;
		}
		else
		{
			m_allowScroll = false;
			up = Vector2.zero;
		}
		GameObject[] itemArray = m_itemArray;
		foreach (GameObject gameObject in itemArray)
		{
			UIDragCamera component = gameObject.GetComponent<UIDragCamera>();
			if ((bool)component)
			{
				component.scale = up;
				component.enabled = m_allowScroll;
			}
		}
		if (background != null)
		{
			UIDragCamera component2 = background.gameObject.GetComponent<UIDragCamera>();
			if (component2 != null)
			{
				component2.scale = up;
				component2.enabled = m_allowScroll;
			}
		}
	}

	public void RepositionList()
	{
		if (m_allowScroll && m_itemArray != null && m_itemArray.Length > 1 && m_itemArray[0] != null)
		{
			Dbg.Log("Reposition the list into the viewport.");
			m_itemArray[0].SendMessage("OnPress", true, SendMessageOptions.DontRequireReceiver);
			m_itemArray[0].SendMessage("OnDrag", new Vector2(5000f, -5000f), SendMessageOptions.DontRequireReceiver);
			m_itemArray[0].SendMessage("OnPress", false, SendMessageOptions.DontRequireReceiver);
		}
	}

	public virtual void OnButtonClicked(GameObject p_gameObject)
	{
		Dbg.LogError("Please override this in the drived class");
	}
}
