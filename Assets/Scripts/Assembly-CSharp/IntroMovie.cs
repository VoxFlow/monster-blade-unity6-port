using System.Collections;
using UnityEngine;

public class IntroMovie : MonoBehaviour
{
	private enum SkipState
	{
		Playing = 0,
		ShowButton = 1,
		SkipNow = 2
	}

	private MovieManager m_movieManager;

	private SkipState m_skipState;

	public GameObject m_SkipButton;

	private bool m_isTouched;

	private IEnumerator Start()
	{
		if (m_SkipButton != null)
		{
			m_SkipButton.active = false;
		}
		m_movieManager = MBHier.GetComponentInChildren<MovieManager>(this);
		if (!(m_movieManager != null))
		{
			yield return new MBWaitForSeconds(1f);   // PORT (CD-059)
			StartCoroutine(MoveToNextScene());
		}
	}

	private void Update()
	{
		if (MBInput.GetMouseButtonDown(0) || (MBInput.touchCount > 0 && MBInput.touches[0].phase == TouchPhase.Began))
		{
			m_isTouched = true;
		}
		if (!(m_movieManager == null) && !m_movieManager.IsMoviePlaying() && m_movieManager.IsLoaded)
		{
			StartCoroutine(MoveToNextScene());
		}
	}

	private void FixedUpdate()
	{
		if (m_movieManager == null || !m_isTouched)
		{
			return;
		}
		m_isTouched = false;
		if (m_skipState == SkipState.Playing)
		{
			m_skipState = SkipState.ShowButton;
			if (m_SkipButton != null)
			{
				m_SkipButton.active = true;
			}
		}
		else
		{
			m_skipState = SkipState.Playing;
			if (m_SkipButton != null)
			{
				m_SkipButton.active = false;
			}
		}
	}

	public void StopMovie()
	{
		StartCoroutine(MoveToNextScene());
	}

	public IEnumerator MoveToNextScene()
	{
		Dbg.Log("MoveToNextScene");
		if (m_movieManager != null)
		{
			m_movieManager.Stop();
		}
		while (m_movieManager.IsLoaded && m_movieManager.IsMoviePlaying())
		{
			yield return null;
		}
		if (SingletonComponent<SaveData>.Instance.GameData.IsNameGiven())
		{
			Dbg.Log("MoveToNextScene:MainMenuInit");
			SingletonComponent<GameManager>.Instance.LoadScene("MainMenuInit");
			yield break;
		}
		Dbg.Log("MoveToNextScene:NameInput");
		SingletonComponent<GameManager>.Instance.LoadScene("NameInput");
		if (UIPopupController.Instance != null)
		{
			UIPopupController.Instance.HideFullScreenLoading();
		}
	}
}
