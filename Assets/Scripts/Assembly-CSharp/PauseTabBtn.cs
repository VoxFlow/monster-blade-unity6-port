using UnityEngine;

public class PauseTabBtn : MonoBehaviour
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
			m_tabbed = value;
			if (m_tabTexture != null)
			{
				m_tabTexture.spriteName = ((!m_tabbed) ? "pause/btn_tab" : "pause/btn_tab_down");
			}
			else
			{
				Dbg.LogWarn("m_tabTexture is null, please assign the link");
			}
			if (m_tabLabel != null)
			{
				m_tabLabel.color = ((!m_tabbed) ? m_untabbedColor : m_tabbedColor);
			}
			else
			{
				Dbg.LogWarn("m_tabLabel is null, please assign the link");
			}
		}
	}
}
