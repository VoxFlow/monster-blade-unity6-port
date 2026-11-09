using UnityEngine;

public class SceneTransition : MonoBehaviour
{
	public float m_secondsTillNextScene = 5f;

	public string m_sceneToLoad = string.Empty;

	private float m_expire;

	private bool m_loadCalled;

	public void Awake()
	{
		m_expire = Time.time + m_secondsTillNextScene;
		m_loadCalled = false;
	}

	public void Update()
	{
		if (!(Time.time < m_expire) && !m_loadCalled)
		{
			SingletonComponent<GameManager>.Instance.LoadScene(m_sceneToLoad, true, false);
			m_loadCalled = true;
		}
	}
}
