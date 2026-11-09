using UnityEngine;

public class PauseSettingsTabBtn : MonoBehaviour
{
	[SerializeField]
	private UISprite m_tabTexture;

	[SerializeField]
	private UILabel m_tabLabel;

	[SerializeField]
	private Color m_tabbedColor;

	[SerializeField]
	private Color m_untabbedColor;

	private bool m_tabbed;

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
