using System.Collections.Generic;
using UnityEngine;

public class GachaMenuPanel : StandardMenuPanel
{
	[SerializeField]
	private UIAnchor m_socialPointsAnchor;

	private List<UIButtonMessage> m_btnMsgs = new List<UIButtonMessage>();

	protected override void Awake()
	{
		base.Awake();
		m_btnMsgs.Clear();
		GameObject[] array = buttons;
		foreach (GameObject gameObject in array)
		{
			UIButtonMessage component = gameObject.GetComponent<UIButtonMessage>();
			m_btnMsgs.Add(component);
		}
		if (null != m_socialPointsAnchor)
		{
			UICamera uICamera = UICamera.FindCameraForLayer(MBLayers.NameToLayer("UI"));   // PORT (CD-050)
			if (null != uICamera)
			{
				m_socialPointsAnchor.uiCamera = uICamera.cachedCamera;
			}
		}
	}

	public override void OnEnable()
	{
		base.OnEnable();
		EnableGachaBtns(true);
	}

	public void EnableGachaBtns(bool p_enable)
	{
		foreach (UIButtonMessage btnMsg in m_btnMsgs)
		{
			btnMsg.enabled = p_enable;
		}
	}
}
