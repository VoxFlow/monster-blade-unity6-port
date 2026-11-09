using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CellColor))]
public class TweenFade : MonoBehaviour
{
	public float enterDuration = 0.5f;

	public float exitDuration = 0.5f;

	private CellColor m_window;

	public Callback OnFadeOutCB;

	public Callback OnFadeInCB;

	private bool isFadingIn;

	private bool isFadingOut;

	private void Awake()
	{
		m_window = GetComponent<CellColor>();
	}

	public void Init()
	{
		if (!m_window)
		{
			m_window = GetComponent<CellColor>();
		}
		m_window.alpha = 0f;
	}

	public void SetAlpha(float p_alpha)
	{
		if ((bool)m_window)
		{
			m_window.alpha = p_alpha;
		}
	}

	private IEnumerator FadeIn(float duration)
	{
		for (float fadeTime = 0f; fadeTime <= duration; fadeTime += Time.fixedDeltaTime)
		{
			yield return null;
			m_window.alpha = Mathf.Lerp(0f, 1f, fadeTime / duration);
		}
		m_window.alpha = 1f;
		if (OnFadeInCB != null)
		{
			OnFadeInCB();
		}
		isFadingIn = false;
	}

	private IEnumerator FadeOut(float duration)
	{
		for (float fadeTime = 0f; fadeTime <= duration; fadeTime += Time.fixedDeltaTime)
		{
			yield return null;
			m_window.alpha = Mathf.Lerp(1f, 0f, fadeTime / duration);
		}
		m_window.alpha = 0f;
		if (OnFadeOutCB != null)
		{
			OnFadeOutCB();
		}
		isFadingOut = false;
	}

	public void OnEnter(Callback cb = null)
	{
		OnFadeInCB = cb;
		if (!isFadingIn)
		{
			isFadingIn = true;
			StartCoroutine(FadeIn(enterDuration));
		}
	}

	public void OnExit(Callback cb = null)
	{
		OnFadeOutCB = cb;
		if (!isFadingOut)
		{
			isFadingOut = true;
			StartCoroutine(FadeOut(exitDuration));
		}
	}

	public float GetWindowAlpha()
	{
		return m_window.alpha;
	}
}
