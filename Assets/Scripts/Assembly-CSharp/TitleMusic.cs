using UnityEngine;

public class TitleMusic : MonoBehaviour
{
	private float m_fadeOutTime = 0.5f;

	private float m_currTime;

	private bool m_isFadeOut;

	private float m_originVolume = 0.5f;

	private void Start()
	{
		m_originVolume = SingletonComponent<SoundManager>.Instance.MusicVolume;
	}

	private void Update()
	{
		if (m_isFadeOut)
		{
			m_currTime += Time.deltaTime;
			if (m_currTime >= m_fadeOutTime)
			{
				m_currTime = m_fadeOutTime;
			}
			float volume = m_originVolume * (1f - m_currTime / m_fadeOutTime);
			AudioSource component = GetComponent<AudioSource>();
			component.volume = volume;
		}
	}

	public void StartFadeOut()
	{
		m_isFadeOut = true;
	}
}
