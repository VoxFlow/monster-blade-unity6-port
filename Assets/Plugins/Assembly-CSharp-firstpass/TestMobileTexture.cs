using UnityEngine;

[RequireComponent(typeof(MobileMovieTexture))]
public class TestMobileTexture : MonoBehaviour
{
	private MobileMovieTexture m_movieTexture;

	private void Awake()
	{
		m_movieTexture = GetComponent<MobileMovieTexture>();
		m_movieTexture.onFinished += OnFinished;
	}

	private void OnFinished(MobileMovieTexture sender)
	{
		Debug.Log(sender.Path + " has finished ");
	}

	private void OnGUI()
	{
		if (GUI.Button(new Rect(0f, 0f, 100f, 100f), (!m_movieTexture.isPlaying) ? "Play" : "Pause"))
		{
			if (m_movieTexture.isPlaying)
			{
				m_movieTexture.pause = true;
			}
			else if (m_movieTexture.pause)
			{
				m_movieTexture.pause = false;
			}
			else
			{
				m_movieTexture.Play();
			}
		}
	}
}
