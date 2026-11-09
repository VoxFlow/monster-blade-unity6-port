using UnityEngine;

[RequireComponent(typeof(UILabel))]
public class NameLabel : MonoBehaviour
{
	private UILabel m_label;

	private Camera m_uiCam;

	private Transform m_referencePoint;

	public Camera UiCam
	{
		set
		{
			m_uiCam = value;
		}
	}

	public Camera BattleCamera { private get; set; }

	public Transform ReferencePoint
	{
		set
		{
			m_referencePoint = value;
			if (value != null)
			{
				base.gameObject.active = true;
			}
		}
	}

	public string CharacterName
	{
		set
		{
			if (m_label == null)
			{
				m_label = base.gameObject.GetComponent<UILabel>();
			}
			if (null != m_label)
			{
				m_label.text = SingletonComponent<NBLocalization>.Instance.Get(value);
			}
		}
	}

	private void Awake()
	{
		if (m_referencePoint == null)
		{
			base.gameObject.active = false;
		}
		m_label = base.gameObject.GetComponent<UILabel>();
	}

	private void FixedUpdate()
	{
		if (m_referencePoint != null)
		{
			m_label.enabled = m_referencePoint.gameObject.active;
		}
	}

	private void Update()
	{
		if (!(m_label == null) && m_label.enabled)
		{
			Vector3 screenPos = UiUtils.GetScreenPos(m_referencePoint.position, BattleCamera, m_uiCam);
			screenPos.z = 0f;
			m_label.transform.position = screenPos;
		}
	}
}
