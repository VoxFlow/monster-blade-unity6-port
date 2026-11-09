using UnityEngine;

public class PauseComboTabBtn : MonoBehaviour
{
	[SerializeField]
	private UISprite m_tabTexture;

	private bool m_tabbed = true;

	public bool m_Tabbed
	{
		get
		{
			return m_tabbed;
		}
		set
		{
			if (m_tabbed != value)
			{
				m_tabbed = value;
				m_tabTexture.spriteName = ((!m_tabbed) ? "pause/btn_tab" : "pause/btn_tab_down");
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
