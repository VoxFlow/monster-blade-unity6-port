using UnityEngine;

public class SubtitleManager : SingletonComponent<SubtitleManager>
{
	private class SubtitleData
	{
		public string m_title = string.Empty;

		public string m_desc = string.Empty;

		public string m_mascotModelPath = string.Empty;

		public string m_mascotAnim = string.Empty;

		public string m_sound = string.Empty;

		public string m_music = string.Empty;
	}

	[SerializeField]
	private MascotPopup m_popupTemplate;

	private MascotPopup m_popup;

	private CloseSubtitleCallback m_onCloseSubtitle;

	private object[] m_onCloseSubtitleParams;

	private SubtitleData m_subtitleData = new SubtitleData();

	private void GetParameters(string p_id)
	{
		m_subtitleData.m_title = SingletonComponent<ParameterManager>.Instance.GetParameter("Subtitle", p_id, "TITLE");
		m_subtitleData.m_desc = SingletonComponent<ParameterManager>.Instance.GetParameter("Subtitle", p_id, "DESC");
		m_subtitleData.m_mascotModelPath = SingletonComponent<ParameterManager>.Instance.GetParameter("Subtitle", p_id, "MASCOT_MODEL");
		m_subtitleData.m_mascotAnim = SingletonComponent<ParameterManager>.Instance.GetParameter("Subtitle", p_id, "MASCOT_ANIM");
		m_subtitleData.m_sound = SingletonComponent<ParameterManager>.Instance.GetParameter("Subtitle", p_id, "SOUND");
		m_subtitleData.m_music = SingletonComponent<ParameterManager>.Instance.GetParameter("Subtitle", p_id, "MUSIC");
	}

	public void Show(string p_id)
	{
		Dbg.Log("[Enter] SubtitleManager::Show()");
		Dbg.Log("[Leave] SubtitleManager::Show()");
	}

	public void SetOffset(Vector2 p_offset)
	{
		if (m_popup != null)
		{
			m_popup.transform.Translate(new Vector3(p_offset.x, p_offset.y, 0f));
		}
	}

	public void SetMascotOffset(Vector3 p_offset)
	{
		m_popup.SetMascotOffset(p_offset);
	}

	public void SetHideCallback(CloseSubtitleCallback p_onCloseSubtitle = null, object[] p_onCloseSubtitleParams = null)
	{
		if (p_onCloseSubtitle != null)
		{
			m_onCloseSubtitle = p_onCloseSubtitle;
		}
		if (p_onCloseSubtitleParams != null)
		{
			p_onCloseSubtitleParams = m_onCloseSubtitleParams;
		}
	}

	private void ShowNext()
	{
		if (!m_popup.ShowPage())
		{
			m_popup.SetBtnCallback(base.gameObject, "Hide");
		}
	}

	private void Hide()
	{
		Dbg.Log("[Enter] SubtitleManager::Hide()");
		m_popup.Hide();
		m_popup.HideMascot();
		if (m_onCloseSubtitle != null)
		{
			m_onCloseSubtitle(m_onCloseSubtitleParams);
		}
		Dbg.Log("[Leave] SubtitleManager::Hide()");
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
