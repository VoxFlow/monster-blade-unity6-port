using UnityEngine;

public class CinematicBar : MonoBehaviour
{
	private enum BarState
	{
		NONE = 0,
		FADEIN = 1,
		FADEOUT = 2,
		MAX = 3
	}

	public UISprite m_topBar;

	public UISprite m_bototmBar;

	public float m_offset = 50f;

	public float m_fadeInTime = 0.5f;

	public float m_fadeOutTime = 0.5f;

	private float m_currTime;

	private BarState m_state;

	private void Awake()
	{
		m_state = BarState.NONE;
		if (m_topBar != null)
		{
			m_topBar.transform.localPosition = Vector3.zero;
		}
		if (m_bototmBar != null)
		{
			m_bototmBar.transform.localPosition = Vector3.zero;
		}
	}

	public void FadeIn(float fadeTime)
	{
		base.gameObject.SetActiveRecursively(true);
		m_currTime = 0f;
		m_state = BarState.FADEIN;
		m_fadeInTime = fadeTime;
	}

	public void FadeOut(float fadeTime)
	{
		base.gameObject.SetActiveRecursively(true);
		m_currTime = 0f;
		m_state = BarState.FADEOUT;
		m_fadeOutTime = fadeTime;
	}

	public void Update()
	{
		if (m_state == BarState.NONE)
		{
			return;
		}
		m_currTime += Time.deltaTime;
		float num = 0f;
		if (m_state == BarState.FADEIN)
		{
			if (m_currTime > m_fadeInTime)
			{
				m_currTime = m_fadeInTime;
				m_state = BarState.NONE;
			}
			num = m_currTime / m_fadeInTime;
		}
		else
		{
			if (m_currTime > m_fadeOutTime)
			{
				m_currTime = m_fadeOutTime;
				m_state = BarState.NONE;
				base.gameObject.SetActiveRecursively(false);
			}
			num = 1f - m_currTime / m_fadeInTime;
		}
		m_topBar.transform.localPosition = new Vector3(0f, 0f - num * m_offset, 0f);
		m_bototmBar.transform.localPosition = new Vector3(0f, num * m_offset, 0f);
	}
}
