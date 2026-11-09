using UnityEngine;

public class EventBtnEx : MonoBehaviour
{
	[SerializeField]
	private UITexture m_background;

	[SerializeField]
	private UISprite m_eventTextImg;

	[SerializeField]
	private UISprite m_lockImg;

	private UIButton m_uiBtn;

	private bool m_lock;

	public bool Lock
	{
		get
		{
			return m_lock;
		}
		set
		{
			m_lock = value;
			if (m_lock)
			{
				m_background.color = Color.gray;
				m_eventTextImg.color = Color.gray;
				m_lockImg.gameObject.SetActiveRecursively(true);
				base.GetComponent<Collider>().enabled = false;
			}
			else
			{
				m_background.color = Color.white;
				m_eventTextImg.color = Color.white;
				m_lockImg.gameObject.SetActiveRecursively(false);
				base.GetComponent<Collider>().enabled = true;
			}
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
