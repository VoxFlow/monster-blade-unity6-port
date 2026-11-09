using UnityEngine;

public class PlayMusicIndependentlyOnStart : MonoBehaviour
{
	public string m_titleMusic = string.Empty;

	private AudioSource m_soundHandle;

	public void Start()
	{
		m_soundHandle = SingletonComponent<SoundManager>.Instance.PlayMusicIndependently("TitleMusic", m_titleMusic, true);
	}

	public void OnDestroy()
	{
		if ((bool)m_soundHandle)
		{
			m_soundHandle.Stop();
		}
	}
}
