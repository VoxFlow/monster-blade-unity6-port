using System;
using UnityEngine;

public class ButtonScale : MonoBehaviour
{
	public enum Trigger
	{
		OnPress = 0,
		OnClick = 1
	}

	public float m_tweenTime = 0.2f;

	public Trigger m_trigger;

	public Vector3 m_scale = new Vector3(1.2f, 1.2f, 1f);

	private Vector3 m_start = Vector3.zero;

	private Vector3 m_target = Vector3.zero;

	private float m_time;

	private float m_timeStep;

	private Vector3 m_defaultScale = Vector3.one;

	private bool m_growing;

	private Action<GameObject> m_endCallback;

	private void Awake()
	{
		m_defaultScale = base.transform.localScale;
		base.enabled = false;
	}

	public void OnPressed(Action<GameObject> p_endCallback = null)
	{
		if (m_trigger == Trigger.OnPress)
		{
			BeginUpdate(m_defaultScale, m_scale);
			m_growing = false;
			m_endCallback = p_endCallback;
		}
	}

	public void OnReleased(Action<GameObject> p_endCallback = null)
	{
		if (m_trigger == Trigger.OnPress)
		{
			BeginUpdate(m_scale, m_defaultScale);
			m_growing = false;
			m_endCallback = p_endCallback;
		}
	}

	public void OnClicked(Action<GameObject> p_endCallback = null)
	{
		if (m_trigger == Trigger.OnClick)
		{
			BeginUpdate(m_defaultScale, m_scale);
			m_growing = true;
			m_endCallback = p_endCallback;
		}
	}

	public void Stop()
	{
		base.enabled = false;
		ResetButtonScale();
	}

	private void ResetButtonScale()
	{
		base.transform.localScale = m_defaultScale;
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
	}

	private void BeginUpdate(Vector3 p_from, Vector3 p_to)
	{
		base.transform.localScale = p_from;
		m_start = p_from;
		m_target = p_to;
		m_time = ((!(m_tweenTime > 0f)) ? 1f : 0f);
		m_timeStep = ((!(m_tweenTime > 0f)) ? 0f : (1f / m_tweenTime));
		base.enabled = true;
	}

	private void Update()
	{
		m_time += m_timeStep * Time.deltaTime * Time.timeScale;
		base.transform.localScale = Vector3.Lerp(m_start, m_target, m_time);
		if (!(m_time >= 1f))
		{
			return;
		}
		if (m_trigger == Trigger.OnPress || !m_growing)
		{
			base.enabled = false;
			if (m_endCallback != null)
			{
				m_endCallback(base.gameObject);
			}
		}
		else
		{
			BeginUpdate(m_scale, m_defaultScale);
			m_growing = false;
		}
	}
}
