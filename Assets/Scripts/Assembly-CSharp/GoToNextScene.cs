using UnityEngine;

public class GoToNextScene : MonoBehaviour
{
	public string m_nextScene = string.Empty;

	private void Start()
	{
		if (m_nextScene != string.Empty)
		{
			Application.LoadLevel(m_nextScene);
		}
	}
}
