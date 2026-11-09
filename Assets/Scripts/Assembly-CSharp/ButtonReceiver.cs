using UnityEngine;

public class ButtonReceiver : MonoBehaviour
{
	public UIList m_list;

	private void OnClick()
	{
		if (m_list != null)
		{
			m_list.OnButtonClicked(base.gameObject);
		}
	}
}
