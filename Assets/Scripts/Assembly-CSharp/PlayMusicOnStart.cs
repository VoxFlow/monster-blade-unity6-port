using UnityEngine;

public class PlayMusicOnStart : MonoBehaviour
{
	public string m_music = string.Empty;

	public void Start()
	{
		if (SingletonComponent<SoundManager>.Instance != null && !string.IsNullOrEmpty(m_music))
		{
			SingletonComponent<SoundManager>.Instance.PlayPlaylist(m_music);
		}
	}
}
