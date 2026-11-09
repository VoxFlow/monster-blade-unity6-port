using UnityEngine;

public class CellColor : MonoBehaviour
{
	public UIWidget[] m_colorables;

	private Color m_color = Color.white;

	public float m_alpha = 1f;

	[SerializeField]
	private bool m_isUpdatedViaAnimation;

	public Color color
	{
		get
		{
			return m_color;
		}
		set
		{
			m_color = value;
			UIWidget[] colorables = m_colorables;
			foreach (UIWidget uIWidget in colorables)
			{
				if (uIWidget.gameObject.active)
				{
					uIWidget.color = m_color;
				}
			}
		}
	}

	public float alpha
	{
		get
		{
			return m_color.a;
		}
		set
		{
			m_color.a = value;
			UIWidget[] colorables = m_colorables;
			foreach (UIWidget uIWidget in colorables)
			{
				if (uIWidget.gameObject.active)
				{
					uIWidget.color = new Color(uIWidget.color.r, uIWidget.color.g, uIWidget.color.b, m_color.a);
				}
			}
		}
	}

	private void Awake()
	{
		m_colorables = MBHier.GetComponentsInChildren<UIWidget>(this, true);
	}

	private void Update()
	{
		if (m_isUpdatedViaAnimation)
		{
			alpha = m_alpha;
		}
	}
}
