using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CellColor))]
public class TweenCell : MonoBehaviour
{
	public float enterDuration = 0.4f;

	public float exitDuration = 0.25f;

	public Vector3 offset;

	public Vector3 exitScale = Vector3.zero;

	private CellColor m_cell;

	private Vector3 m_originalPos;

	private Vector3 m_originalScale;

	private Vector3 m_fromPos;

	private void Awake()
	{
		m_cell = GetComponent<CellColor>();
		m_originalScale = base.transform.localScale;
	}

	private void OnDisable()
	{
		Reset();
	}

	public void Reset()
	{
		base.transform.localScale = m_originalScale;
		m_cell.alpha = 1f;
	}

	public void Init()
	{
		if (!m_cell)
		{
			m_cell = GetComponent<CellColor>();
		}
		m_originalScale = base.transform.localScale;
		base.transform.localScale = Vector3.zero;
		m_cell.alpha = 0f;
	}

	private IEnumerator FadeIn(float duration)
	{
		for (float fadeTime = 0f; fadeTime <= duration; fadeTime += Time.fixedDeltaTime)
		{
			yield return null;
			m_cell.alpha = Mathf.Lerp(0f, 1f, fadeTime / duration);
		}
		m_cell.alpha = 1f;
		EnableSpecialComponents();
	}

	private IEnumerator FadeOut(float duration)
	{
		for (float fadeTime = 0f; fadeTime <= duration; fadeTime += Time.fixedDeltaTime)
		{
			yield return null;
			m_cell.alpha = Mathf.Lerp(1f, 0f, fadeTime / duration);
		}
		m_cell.alpha = 0f;
	}

	public void OnEnter()
	{
		DisableSpecialComponents();
		StartCoroutine(FadeIn(enterDuration));
		base.transform.localScale = Vector3.zero;
		iTween.ScaleTo(base.gameObject, iTween.Hash("scale", m_originalScale, "time", enterDuration, "easetype", "easeOutBack"));
	}

	public void OnExit()
	{
		DisableSpecialComponents();
		StartCoroutine(FadeOut(exitDuration));
		m_originalScale = base.transform.localScale;
		TweenScale.Begin(base.gameObject, exitDuration, exitScale);
	}

	private void DisableSpecialComponents()
	{
		TweenColor[] componentsInChildren = MBHier.GetComponentsInChildren<TweenColor>(this);
		TweenColor[] array = componentsInChildren;
		foreach (TweenColor tweenColor in array)
		{
			tweenColor.enabled = false;
		}
	}

	private void EnableSpecialComponents()
	{
		TweenColor[] componentsInChildren = MBHier.GetComponentsInChildren<TweenColor>(this);
		TweenColor[] array = componentsInChildren;
		foreach (TweenColor tweenColor in array)
		{
			tweenColor.enabled = true;
		}
	}
}
