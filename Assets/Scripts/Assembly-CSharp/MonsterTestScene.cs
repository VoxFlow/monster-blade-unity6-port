using UnityEngine;

public class MonsterTestScene : MonoBehaviour
{
	private void Start()
	{
		SingletonComponent<ParameterManager>.Instance.InitManager();
		string[] array = new string[12]
		{
			"PlayList_Mob01", "PlayList_Mob03", "PlayList_Mob05", "PlayList_Mob06", "PlayList_Mob07", "PlayList_Mob08", "PlayList_Mob09", "PlayList_Mob10", "PlayList_Mob11", "PlayList_Mob12",
			"PlayList_Mob13", "PlayList_Mob15"
		};
		string[] array2 = array;
		foreach (string playListName in array2)
		{
			SingletonComponent<SoundManager>.Instance.LoadMonsterSoundPlayList(playListName);
		}
		SingletonComponent<SoundManager>.Instance.LoadPlayList("PlayList_LevelGeneral");
		SingletonComponent<SoundManager>.Instance.LoadPlayList("PlayList_Menu");
		SingletonComponent<SoundManager>.Instance.LoadPlayList("PlayList_Title");
		SingletonComponent<SoundManager>.Instance.LoadPlayList("Playlists");
	}
}
