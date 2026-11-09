using System;
using UnityEngine;

[RequireComponent(typeof(UISprite))]
public class BattleTextEffect : MonoBehaviour
{
	public delegate void Callback(GameObject p_sender);

	private UISprite m_sprite;

	private float m_angle;

	private float m_angleStep;

	private float m_fadeTime;

	private float m_fadeStep;

	private float m_fadeTarget;

	private float m_holdTime;

	private float m_maxHoldTime;

	private float m_defaultAlpha;

	private Vector3 m_defaultSize = Vector3.one;

	private float m_scaleFactor = 1f;

	private float m_baseScale = 1f;

	public UISprite Sprite
	{
		get
		{
			if (m_sprite == null)
			{
				m_sprite = base.gameObject.GetComponent<UISprite>();
			}
			return m_sprite;
		}
	}

	public float Width
	{
		get
		{
			if (m_sprite == null)
			{
				return 0f;
			}
			float x = m_sprite.relativeSize.x;
			return x * m_defaultSize.x;
		}
	}

	public float Scale
	{
		get
		{
			return m_baseScale;
		}
		set
		{
			m_baseScale = value;
			ResetSize();
		}
	}

	private void Awake()
	{
		m_sprite = base.gameObject.GetComponent<UISprite>();
		m_defaultAlpha = m_sprite.color.a;
		base.gameObject.SetActiveRecursively(false);
		base.enabled = false;
	}

	private void OnEnable()
	{
		if (m_sprite == null)
		{
			base.enabled = false;
		}
	}

	public void Show(float p_displayTime, float p_popupTime, float p_fadeTime, float p_scaleFactor = 1f, float p_fadeTarget = 0f)
	{
		Stop();
		if (!(m_sprite == null))
		{
			m_angle = ((!(p_popupTime > 0f)) ? ((float)Math.PI) : 0f);
			m_angleStep = ((!(p_popupTime > 0f)) ? 0f : ((float)Math.PI / p_popupTime));
			m_fadeTime = ((p_fadeTime == 0f) ? 1f : p_fadeTime);
			m_fadeStep = ((!(p_fadeTime > 0f)) ? 0f : ((m_defaultAlpha - p_fadeTarget) / p_fadeTime));
			m_fadeTarget = p_fadeTarget;
			m_holdTime = 0f;
			m_maxHoldTime = p_displayTime - p_popupTime - p_fadeTime;
			m_maxHoldTime = Mathf.Max(0f, m_maxHoldTime);
			m_scaleFactor = p_scaleFactor;
			m_sprite.enabled = true;
			base.gameObject.SetActiveRecursively(true);
			base.enabled = true;
		}
	}

	public void Stop()
	{
		if (!(m_sprite == null))
		{
			ResetSize();
			SetSpriteAlpha(m_defaultAlpha);
			base.gameObject.SetActiveRecursively(false);
			base.enabled = false;
		}
	}

	public void ResetSize()
	{
		m_sprite.MakePixelPerfect();
		m_defaultSize = m_sprite.cachedTransform.localScale;
		m_defaultSize *= Scale;
		m_defaultSize.z = 1f;
		m_sprite.cachedTransform.localScale = m_defaultSize;
	}

	private void SetSpriteAlpha(float p_alpha)
	{
		if (m_sprite != null)
		{
			Color color = m_sprite.color;
			color.a = p_alpha;
			m_sprite.color = color;
		}
		else
		{
			Dbg.LogWarn("[Warn] The m_sprite is null");
		}
	}

	private void Update()
	{
		if (m_angle < (float)Math.PI)
		{
			m_angle += m_angleStep * Time.deltaTime;
			if (m_angle >= (float)Math.PI)
			{
				m_angle = (float)Math.PI;
			}
			m_sprite.cachedTransform.localScale = (m_defaultSize + m_defaultSize * Mathf.Sin(m_angle) * 0.5f) * m_scaleFactor;
		}
		else if (m_holdTime < m_maxHoldTime)
		{
			m_holdTime += Time.deltaTime;
		}
		else if (m_fadeTime < 1f)
		{
			if (m_fadeStep > 0f)
			{
				m_fadeTime += m_fadeStep * Time.deltaTime;
				SetSpriteAlpha(Mathf.Lerp(m_defaultAlpha, m_fadeTarget, m_fadeTime));
			}
		}
		else if (m_fadeTarget == 0f)
		{
			Stop();
		}
	}
}
