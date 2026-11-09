using UnityEngine;

public class TutorialButtonFilter : UIFilter
{
	public GameObject m_pressable;

	// PORT (CD-027)
	private int m_popupLayer = int.MinValue;

	public override bool Accept(GameObject go)
	{
		if (m_popupLayer == int.MinValue)
		{
			m_popupLayer = LayerMask.NameToLayer("PopUpLayer");
		}
		if ((bool)m_pressable && (m_pressable == go || ((bool)go && go.layer == m_popupLayer)))
		{
			return true;
		}
		return false;
	}
}
