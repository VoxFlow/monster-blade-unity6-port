using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CellColor))]
public class TweenPopup : MonoBehaviour
{
	public float enterDuration = 0.5f;

	public float exitDuration = 0.5f;

	private CellColor m_window;

	private PopupCallback m_onExit;

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
		base.transform.localScale = Vector3.zero;
		m_window.alpha = 0f;
	}

	private IEnumerator FadeIn(float duration)
	{
		for (float fadeTime = 0f; fadeTime <= duration; fadeTime += Time.fixedDeltaTime)
		{
			yield return null;
			m_window.alpha = Mathf.Lerp(0f, 1f, fadeTime / duration);
		}
		m_window.alpha = 1f;
	}

	private IEnumerator FadeOut(float duration)
	{
		for (float fadeTime = 0f; fadeTime <= duration; fadeTime += Time.fixedDeltaTime)
		{
			yield return null;
			m_window.alpha = Mathf.Lerp(1f, 0f, fadeTime / duration);
		}
		m_window.alpha = 0f;
		if (m_onExit != null)
		{
			m_onExit();
		}
	}

	public void OnEnter()
	{
		StartCoroutine(FadeIn(enterDuration));
		TweenScale.Begin(base.gameObject, enterDuration, Vector3.one);
	}

	public void OnExit(PopupCallback p_onExit)
	{
		m_onExit = p_onExit;
		StartCoroutine(FadeOut(exitDuration));
		TweenScale.Begin(base.gameObject, exitDuration, Vector3.one * 0.2f);
	}
}
