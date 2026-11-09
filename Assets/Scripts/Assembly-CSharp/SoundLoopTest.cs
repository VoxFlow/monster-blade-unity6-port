using UnityEngine;

public class SoundLoopTest : MonoBehaviour
{
	private AudioSource[] m_musics;

	public AudioClip[] m_musicClips;

	private int m_currIndex;

	private void Start()
	{
		m_musics = new AudioSource[m_musicClips.Length];
		for (int i = 0; i < m_musicClips.Length; i++)
		{
			m_musics[i] = base.gameObject.AddComponent<AudioSource>();
			m_musics[i].clip = m_musicClips[i];
			m_musics[i].loop = true;
			m_musics[i].name = m_musicClips[i].name;
		}
	}

	private void Update()
	{
		if (Input.touchCount > 0 || Input.GetMouseButtonDown(0))
		{
			m_musics[m_currIndex].Stop();
			m_currIndex++;
			if (m_currIndex >= m_musics.Length)
			{
				m_currIndex = 0;
			}
			m_musics[m_currIndex].Play();
		}
	}

	private void OnGUI()
	{
		GUI.Label(new Rect(0f, (float)Screen.height / 2f, Screen.width, 50f), m_musicClips[m_currIndex].name + " is Playing.");
	}
}
