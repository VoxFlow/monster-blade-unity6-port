using UnityEngine;

public class AdMobSystem : SingletonComponent<AdMobSystem>
{
	private bool m_initilized;

	private void Start()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			m_initilized = true;
		}
	}

	public void ShowAdMob()
	{
		if (!m_initilized)
		{
		}
	}

	public void HideAdMob()
	{
	}

	private void Update()
	{
	}
}
