using System;
using UnityEngine;

public class NBUIGridMagnifier : IgnoreTimeScale
{
	public float maxScale = 1.15f;

	public Color grayColor = new Color(0.35f, 0.35f, 0.35f);

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

	private void Reset()
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
		if (m_cells != null && m_cells.Length - 2 > 1 && m_release && m_scrollView.currentMomentum.sqrMagnitude < 0.01f)
		{
			m_scrollView.currentMomentum = Vector3.zero;
			m_release = false;
			Vector3 localPosition = m_scrollView.transform.localPosition;
			localPosition.y = m_originalPos.y;
			float scrollValue = m_scrollView.horizontalScrollBar.scrollValue;
			int num = m_cells.Length - 2;
			int num2 = Mathf.RoundToInt(scrollValue * (float)(num - 1));
			float num3 = m_scrollView.bounds.size.x / (float)m_cells.Length;
			localPosition.x = (float)(-num2) * num3;
			SpringPanel.Begin(m_scrollView.gameObject, localPosition, 13f);
		}
	}

	private void Init()
	{
		m_cells = MBHier.GetComponentsInChildren<UIDragPanelContents>(this, true);
		Debug.Log("***********************************************");
	}

	private void OnScrollBarChanged(UIScrollBar sb)
	{
		if (m_cells == null)
		{
			Init();
		}
		int num = m_cells.Length - 2;
		if (num == 1)
		{
			return;
		}
		float scrollValue = sb.scrollValue;
		Vector4 clipRange = m_panel.clipRange;
		clipRange.y = 0f;
		m_panel.clipRange = clipRange;
		for (int i = 1; i <= num; i++)
		{
			Transform transform = m_cells[i].transform;
			float num2 = (float)(i - 1) * 1f / (float)(num - 1);
			float num3 = Mathf.Abs(scrollValue - num2) * (float)(num - 1);
			if (num3 <= 2f)
			{
				num3 = Mathf.Clamp01(num3);
				float num4 = Mathfx.Sinerp(maxScale, 1f, num3);
				transform.localScale = Vector3.one * num4;
				CellColor component = transform.GetComponent<CellColor>();
				if ((bool)component)
				{
					component.color = Color.Lerp(Color.white, grayColor, num3);
				}
			}
		}
	}
}
