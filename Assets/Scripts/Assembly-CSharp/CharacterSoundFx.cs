using UnityEngine;

public class CharacterSoundFx : MonoBehaviour
{
	private void PlaySoundFx(string p_sfxPlaylist)
	{
		if (SingletonComponent<SoundManager>.Instance != null)
		{
			SingletonComponent<SoundManager>.Instance.PlayPlaylist(p_sfxPlaylist);
		}
	}
}
