using UnityEngine;

public class ElementFilterButton : MonoBehaviour
{
	[SerializeField]
	private NBUILocalize m_label;

	[SerializeField]
	private UISprite m_elementSprite;

	private void Awake()
	{
		Init();
	}

	private void Init()
	{
		ShowLabel(false);
		ShowIcon(false);
	}

	public void Show(Element.Type p_element)
	{
		ShowLabel(false);
		if (null != m_elementSprite)
		{
			string value = string.Empty;
			GLOBAL.s_elementIconMap.TryGetValue(p_element, out value);
			if (!string.IsNullOrEmpty(value))
			{
				m_elementSprite.spriteName = value;
			}
			ShowIcon(true);
		}
	}

	public void Show(string p_textKey)
	{
		ShowIcon(false);
		if (null != m_label)
		{
			m_label.Key = p_textKey;
			ShowLabel(true);
		}
	}

	private void ShowLabel(bool p_show)
	{
		if (null != m_label)
		{
			m_label.gameObject.SetActiveRecursively(p_show);
		}
	}

	private void ShowIcon(bool p_show)
	{
		if (null != m_elementSprite)
		{
			m_elementSprite.gameObject.SetActiveRecursively(p_show);
		}
	}
}
