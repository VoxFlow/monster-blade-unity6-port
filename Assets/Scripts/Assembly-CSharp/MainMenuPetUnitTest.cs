using UnityEngine;

public class MainMenuPetUnitTest : MonoBehaviour
{
	private string m_petID = string.Empty;

	private AsyncOperation m_levelLoadOp;

	private bool m_levelLoaded;

	private void Awake()
	{
		SingletonComponent<ParameterManager>.Instance.InitManager();
		m_levelLoadOp = Application.LoadLevelAdditiveAsync("MainMenu");
	}

	private void Update()
	{
		if (!m_levelLoaded && m_levelLoadOp != null && m_levelLoadOp.isDone)
		{
			m_levelLoaded = true;
		}
	}

	private void OnGUI()
	{
		if (m_levelLoaded)
		{
			GUILayout.BeginHorizontal();
			GUI.color = Color.black;
			GUILayout.Label("Pet ID:", GUILayout.Width(70f));
			GUI.color = Color.white;
			m_petID = GUILayout.TextField(m_petID, GUILayout.Width(300f));
			GUILayout.EndHorizontal();
		}
	}
}
