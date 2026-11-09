using System;
using System.Collections;
using UnityEngine;

public class TutorialSlashHint : IgnoreTimeScale
{
	public UISprite m_spriteSlash;

	public UISprite m_spriteHand;

	public UISprite m_spriteTextSlash;

	public PlayerAttack.Direction m_direction = PlayerAttack.Direction.TopLeftToBottomRight;

	private Vector3 m_startPos;

	private Vector3 m_endPos;

	private Vector3 m_defaultTextPosition;

	private Vector3 m_defaultTextScale;

	private float m_angle = 40f;

	private bool m_show;

	private float m_slashTime;

	private float m_slashDuration = 2f;

	private float m_slashSpeed;

	private float m_pauseDuration = 2f;

	private NBTimer m_pauseTimer;

	private void Awake()
	{
		Dbg.Assert(m_spriteSlash);
		Dbg.Assert(m_spriteHand);
		Dbg.Assert(m_spriteTextSlash);
		m_pauseTimer = base.gameObject.AddComponent<NBTimer>();
		Dbg.Assert(m_pauseTimer);
		m_pauseTimer.m_ignoreTimeScale = true;
		m_defaultTextPosition = m_spriteTextSlash.transform.localPosition;
		m_defaultTextScale = m_spriteTextSlash.transform.localScale;
	}

	private void OnDisable()
	{
		Reset();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
	}

	private void Reset()
	{
		m_slashTime = 0f;
		m_show = false;
	}

	public void Show(bool showHandDirection = true)
	{
		if (showHandDirection)
		{
			m_startPos = Vector3.zero;
			m_endPos = Vector3.zero;
			m_spriteSlash.transform.localPosition = Vector3.zero;
			float num = 0f;
			float num2 = 0f;
			Vector3 zero = Vector3.zero;
			Vector3 one = Vector3.one;
			switch (m_direction)
			{
			case PlayerAttack.Direction.TopRightToBottomLeft:
				m_spriteSlash.transform.localRotation = Quaternion.Euler(0f, 0f, m_angle);
				m_startPos = (m_endPos = m_spriteSlash.transform.localPosition);
				num = m_spriteSlash.transform.localScale.x * 0.5f * Mathf.Cos(m_angle * ((float)Math.PI / 180f));
				num2 = m_spriteSlash.transform.localScale.x * 0.5f * Mathf.Sin(m_angle * ((float)Math.PI / 180f));
				zero = Vector3.right * -90f + Vector3.up * 50f;
				m_startPos += Vector3.right * num + Vector3.up * num2 + zero;
				zero = Vector3.right * -70f + Vector3.up * 70f;
				m_endPos += Vector3.right * (0f - num) + Vector3.up * (0f - num2) + zero;
				one = m_spriteHand.transform.localScale;
				one.x = Mathf.Abs(one.x);
				m_spriteHand.transform.localScale = one;
				m_spriteHand.transform.localRotation = Quaternion.Euler(0f, 0f, -45f);
				break;
			case PlayerAttack.Direction.TopLeftToBottomRight:
				m_spriteSlash.transform.localRotation = Quaternion.Euler(0f, 0f, 0f - m_angle + 180f);
				m_startPos = (m_endPos = m_spriteSlash.transform.localPosition);
				num = m_spriteSlash.transform.localScale.x * 0.5f * Mathf.Cos(m_angle * ((float)Math.PI / 180f));
				num2 = m_spriteSlash.transform.localScale.x * 0.5f * Mathf.Sin(m_angle * ((float)Math.PI / 180f));
				zero = Vector3.right * 90f + Vector3.up * 50f;
				m_startPos += Vector3.right * (0f - num) + Vector3.up * num2 + zero;
				zero = Vector3.right * 70f + Vector3.up * 70f;
				m_endPos += Vector3.right * num + Vector3.up * (0f - num2) + zero;
				one = m_spriteHand.transform.localScale;
				one.x = 0f - Mathf.Abs(one.x);
				m_spriteHand.transform.localScale = one;
				m_spriteHand.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
				break;
			}
			m_spriteTextSlash.transform.localPosition = m_defaultTextPosition;
		}
		else
		{
			m_spriteTextSlash.transform.localPosition = m_defaultTextPosition + Vector3.up * 200f;
		}
		if (!m_show && !base.gameObject.active)
		{
			Hashtable hashtable = new Hashtable();
			hashtable.Add("scale", m_defaultTextScale * 1.2f);
			hashtable.Add("time", 0.5f);
			hashtable.Add("looptype", iTween.LoopType.pingPong);
			hashtable.Add("ignoretimescale", true);
			hashtable.Add("islocal", true);
			hashtable.Add("easetype", iTween.EaseType.linear);
			iTween.ScaleTo(m_spriteTextSlash.gameObject, hashtable);
		}
		m_slashTime = 0f;
		m_slashSpeed = 1f / m_slashDuration;
		m_show = true;
		base.enabled = true;
		if (!base.gameObject.active)
		{
			base.gameObject.SetActiveRecursively(true);
			m_spriteHand.enabled = showHandDirection;
			m_spriteSlash.enabled = showHandDirection;
		}
	}

	private void OnShow()
	{
		Show();
	}

	public void Hide()
	{
		Reset();
		if ((bool)m_spriteTextSlash)
		{
			iTween.Stop(m_spriteTextSlash.gameObject);
			m_spriteTextSlash.transform.localScale = m_defaultTextScale;
		}
		if ((bool)m_pauseTimer)
		{
			m_pauseTimer.Reset();
		}
		base.gameObject.SetActiveRecursively(false);
	}

	private void Update()
	{
		if (m_show && m_spriteHand.enabled)
		{
			m_spriteHand.transform.localPosition = Vector3.Lerp(m_startPos, m_endPos, m_slashTime);
			float num = UpdateRealTimeDelta();
			m_slashTime += num * m_slashSpeed;
			if (m_slashTime >= 1f)
			{
				m_slashTime = 0f;
				m_pauseTimer.CountDown(m_pauseDuration, "OnShow");
				base.enabled = false;
			}
		}
	}
}
