using UnityEngine;

[ExecuteInEditMode]
public class ChangeNGUIAlpha : MonoBehaviour
{
	public float m_alpha = 1f;

	private UISprite m_sprite;

	private UILabel m_label;

	// PORT (CD-064)
	private float m_pushed = float.NaN;

	private void Start()
	{
		m_sprite = base.gameObject.GetComponent<UISprite>();
		if (m_sprite == null)
		{
			m_sprite = MBHier.GetComponentInChildren<UISprite>(this);
		}
		m_label = GetComponent<UILabel>();
		Update();
	}

	private void Update()
	{
		if ((bool)m_sprite)
		{
			m_sprite.color = new Color(m_sprite.color.r, m_sprite.color.g, m_sprite.color.b, m_alpha);
			if (m_alpha != m_pushed) m_sprite.MarkAsChanged();   // PORT (CD-064)
		}
		else if ((bool)m_label)
		{
			m_label.color = new Color(m_label.color.r, m_label.color.g, m_label.color.b, m_alpha);
			if (m_alpha != m_pushed) m_label.MarkAsChanged();   // PORT (CD-064)
		}
		m_pushed = m_alpha;
	}
}
