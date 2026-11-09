using UnityEngine;

public class InfoPopup : MonoBehaviour
{
	[SerializeField]
	private UIButton m_closeButton;

	[SerializeField]
	private UIButton m_yesButton;

	[SerializeField]
	private UIButton m_noButton;

	public bool m_showCloseButton;

	private void OnEnable()
	{
		if (null != m_closeButton)
		{
			m_closeButton.gameObject.SetActiveRecursively(m_showCloseButton);
		}
	}
}
