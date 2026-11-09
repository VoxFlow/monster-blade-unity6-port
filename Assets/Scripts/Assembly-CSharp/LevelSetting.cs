using UnityEngine;

public class LevelSetting : SingletonComponent<LevelSetting>
{
	public Color ambientLight = Color.gray;

	private void Awake()
	{
		base.enabled = false;
	}
}
