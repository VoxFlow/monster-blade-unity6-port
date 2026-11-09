using UnityEngine;

[RequireComponent(typeof(UISprite))]
public class UISpriteFader : MonoBehaviour
{
	public float m_duration = 0.5f;

	public GameObject eventTarget;

	public Color m_color = Color.white;

	private UISprite m_sprite;

	private TweenColor m_tween;

	private void Awake()
	{
		m_sprite = GetComponent<UISprite>();
	}

	private void FadeIn()
	{
		if ((bool)m_tween)
		{
			m_tween.enabled = false;
		}
		if (!m_sprite.enabled)
		{
			m_sprite.enabled = true;
		}
		m_sprite.color = Color.clear;
		m_tween = TweenColor.Begin(m_sprite.gameObject, m_duration, m_color);
		m_tween.eventReceiver = null;
		if (base.GetComponent<Collider>() != null)
		{
			base.GetComponent<Collider>().enabled = true;
		}
	}

	private void FadeOut()
	{
		if ((bool)m_tween)
		{
			m_tween.enabled = false;
		}
		if (base.GetComponent<Collider>() != null)
		{
			base.GetComponent<Collider>().enabled = false;
		}
		m_sprite.color = m_color;
		m_tween = TweenColor.Begin(m_sprite.gameObject, m_duration, Color.clear);
		m_tween.callWhenFinished = "OnFadingOutDone";
		m_tween.eventReceiver = base.gameObject;
	}

	private void OnFadingOutDone()
	{
		base.gameObject.active = false;
	}
}
