using UnityEngine;

public class UIBadge : MonoBehaviour
{
	public UILabel m_label;

	public int m_number;

	public bool disableWhenZero;

	public void SetNumber(int p_number)
	{
		m_number = p_number;
		if (null != m_label)
		{
			m_label.text = m_number + string.Empty;
		}
		if (disableWhenZero && m_number == 0)
		{
			base.gameObject.SetActiveRecursively(false);
		}
	}
}
