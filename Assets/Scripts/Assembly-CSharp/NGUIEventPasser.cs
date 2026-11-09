using UnityEngine;

public class NGUIEventPasser : MonoBehaviour
{
	public GameObject m_eventReceiver;

	public void OnClick()
	{
		m_eventReceiver.SendMessage("OnClick", base.gameObject);
	}
}
