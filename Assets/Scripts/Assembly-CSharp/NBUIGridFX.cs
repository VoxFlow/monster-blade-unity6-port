using System;
using UnityEngine;

public class NBUIGridFX : IgnoreTimeScale
{
	public float maxScale = 1.3f;

	public Color grayColor = new Color(0.9f, 0.9f, 0.9f);

	public float m_gridSpacing;

	public bool m_snap = true;

	private UIDraggablePanel m_scrollView;

	private UIPanel m_panel;

	private UIDragPanelContents[] m_cells;

	private bool m_release;

	private Vector3 m_originalPos;

	private SpringPanel m_spring;

	private void OnDisable()
	{
		Reset();
	}

	public void Reset()
	{
		m_cells = null;
	}

	private void Awake()
	{
		m_scrollView = NGUITools.FindInParents<UIDraggablePanel>(base.gameObject);
		m_panel = m_scrollView.GetComponent<UIPanel>();
		m_originalPos = m_scrollView.transform.localPosition;
		if ((bool)m_scrollView.horizontalScrollBar)
		{
			UIScrollBar horizontalScrollBar = m_scrollView.horizontalScrollBar;
			horizontalScrollBar.onChange = (UIScrollBar.OnScrollBarChange)Delegate.Combine(horizontalScrollBar.onChange, new UIScrollBar.OnScrollBarChange(OnScrollBarChanged));
			UIEventListener uIEventListener = UIEventListener.Get(m_scrollView.horizontalScrollBar.foreground.gameObject);
			uIEventListener.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onPress, new UIEventListener.BoolDelegate(OnPress));
		}
		UIEventListener uIEventListener2 = UIEventListener.Get(m_scrollView.gameObject);
		uIEventListener2.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener2.onPress, new UIEventListener.BoolDelegate(OnPress));
	}

	private void OnPress(GameObject source, bool isPressed)
	{
		m_release = false;
		if (!isPressed)
		{
			m_release = true;
		}
	}

	private void OnClick()
	{
		m_release = false;
	}

	protected override void Start()
	{
	}

	private void Update()
	{
		if (!m_snap || m_cells == null || m_cells.Length <= 1 || !m_release)
		{
			return;
		}
		m_scrollView.currentMomentum = Vector3.zero;
		m_release = false;
		Vector3 localPosition = m_scrollView.transform.localPosition;
		localPosition.y = m_originalPos.y;
		float x = m_scrollView.transform.localPosition.x;
		float num = (float)m_cells.Length * m_gridSpacing;
		float num2 = Mathf.Abs(x / num);
		int num3 = m_cells.Length;
		int num4 = Mathf.RoundToInt(num2 * (float)(num3 - 1) + 0.1f);
		if (num4 > num3 - 1)
		{
			num4 = num3 - 1;
		}
		num4 = 0;
		for (int i = 1; i < num3; i++)
		{
			float num5 = Mathf.Abs(x + m_cells[i - 1].transform.localPosition.x);
			float num6 = Mathf.Abs(x + m_cells[i].transform.localPosition.x);
			if (num6 < num5)
			{
				num4 = i;
			}
		}
		localPosition.x = 0f - m_cells[num4].transform.localPosition.x;
		SpringPanel.Begin(m_scrollView.gameObject, localPosition, 5f);
	}

	private void Init()
	{
		m_cells = MBHier.GetComponentsInChildren<UIDragPanelContents>(this, true);
	}

	public void OnScrollBarChanged(UIScrollBar sb)
	{
		if (m_cells == null || m_gridSpacing == 0f)
		{
			Init();
		}
		if (m_cells != null && m_cells.Length == 0)
		{
			Init();
		}
		int num = m_cells.Length;
		if (num == 1)
		{
			return;
		}
		Vector4 clipRange = m_panel.clipRange;
		clipRange.y = 0f;
		m_panel.clipRange = clipRange;
		if (!(m_scrollView != null))
		{
			return;
		}
		float x = m_scrollView.transform.localPosition.x;
		for (int i = 0; i < num; i++)
		{
			UIDragPanelContents uIDragPanelContents = m_cells[i];
			if (m_cells[i].gameObject.active)
			{
				float f = uIDragPanelContents.transform.localPosition.x + x;
				float num2 = uIDragPanelContents.GetComponent<Collider>().bounds.size.x / GLOBAL.UI_SCALE_FACTOR;
				float num3 = Mathf.Abs(f);
				Transform transform = m_cells[i].transform;
				if (num3 < num2)
				{
					float num4 = (num2 - num3) / num2;
					float num5 = (maxScale - 1f) * num4 + 1f;
					transform.localScale = Vector3.one * num5;
				}
				else
				{
					transform.localScale = Vector3.one;
				}
			}
		}
	}
}
