using UnityEngine;

public class LoadSceneCallbacks : MonoBehaviour
{
	public string[] m_listOfIngameScenes;

	public string[] m_scenesExemptedFromOverlays;

	public string[] m_scenesExemptedFromTransitionEffects;

	private string m_sceneToProcess = string.Empty;

	public void OnLoadSceneBegin(string p_scene)
	{
		string sceneToProcess = m_sceneToProcess;
		m_sceneToProcess = p_scene;
		if (IsExemptedFromTransitionEffects(sceneToProcess))
		{
			OnFadeOutEnd();
		}
		else
		{
			SingletonComponent<FaderGui>.Instance.FadeOut();
		}
	}

	public void OnLoadSceneEnd(string p_scene)
	{
		m_sceneToProcess = p_scene;
		if (IsExemptedFromTransitionEffects(p_scene))
		{
			OnFadeInEnd();
		}
		else
		{
			SingletonComponent<FaderGui>.Instance.FadeIn();
		}
	}

	public void OnFadeInEnd()
	{
		if ((bool)SingletonComponent<FaderGui>.Instance)
		{
			SingletonComponent<FaderGui>.Instance.HideLoadOverlay();
		}
	}

	public void OnFadeOutEnd()
	{
		if (!IsExemptedFromOverlay(m_sceneToProcess) && (bool)SingletonComponent<FaderGui>.Instance)
		{
			SingletonComponent<FaderGui>.Instance.ShowLoadOverlay();
		}
	}

	private bool IsSpecialFade(string p_scene)
	{
		string[] listOfIngameScenes = m_listOfIngameScenes;
		foreach (string text in listOfIngameScenes)
		{
			if (text == p_scene)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsExemptedFromOverlay(string p_scene)
	{
		string[] scenesExemptedFromOverlays = m_scenesExemptedFromOverlays;
		foreach (string text in scenesExemptedFromOverlays)
		{
			if (text == p_scene)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsExemptedFromTransitionEffects(string p_scene)
	{
		string[] scenesExemptedFromTransitionEffects = m_scenesExemptedFromTransitionEffects;
		foreach (string text in scenesExemptedFromTransitionEffects)
		{
			if (text == p_scene)
			{
				return true;
			}
		}
		return false;
	}
}
