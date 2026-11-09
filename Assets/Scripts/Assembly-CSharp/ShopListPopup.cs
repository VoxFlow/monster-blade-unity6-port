using UnityEngine;

public class ShopListPopup : MonoBehaviour
{
	public UIDraggablePanel m_scrollPanel;

	private void Start()
	{
		if (null != m_scrollPanel)
		{
			m_scrollPanel.RestrictWithinBounds(false);
		}
	}
}
