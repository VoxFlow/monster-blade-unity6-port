using UnityEngine;

public class StopMusicOnStart : MonoBehaviour
{
	public void Start()
	{
		SingletonComponent<SoundManager>.Instance.StopMusic();
	}
}
