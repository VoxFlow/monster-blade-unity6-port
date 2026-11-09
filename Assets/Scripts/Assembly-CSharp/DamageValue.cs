using UnityEngine;

[RequireComponent(typeof(Animation))]
[RequireComponent(typeof(UILabel))]
public class DamageValue : MonoBehaviour
{
	protected UILabel m_lable;

	protected NBTimer m_timer;

	protected int m_finalValue;

	protected float m_displayTime;

	protected float m_countStep;

	protected float m_value;

	protected bool m_startCount;

	protected float m_initialValueHoldTime = 0.3f;

	protected float m_fadeStep;

	protected string m_prefix = string.Empty;

	protected AnimationState m_playingAnim;

	public void Show(int p_value, int p_finalValue, Vector3 p_position, float p_period, Color p_color, string p_animation, string p_prefix)
	{
		base.gameObject.SetActiveRecursively(true);
		if (p_animation != string.Empty)
		{
			base.GetComponent<Animation>().Stop();
			m_playingAnim = base.GetComponent<Animation>().CrossFadeQueued(p_animation, 0f);
		}
		p_position.z = 0f;
		m_prefix = p_prefix;
		m_value = p_value;
		m_lable.text = m_prefix + p_value;
		m_lable.transform.parent.position = p_position;
		m_lable.color = p_color;
		m_finalValue = p_finalValue;
		m_displayTime = m_playingAnim.length;
		m_initialValueHoldTime = p_period * 0.25f;
		m_startCount = false;
		m_timer.CountDown(m_initialValueHoldTime, "OnFinishDisplayInitalValue");
	}

	protected void OnFinishDisplayInitalValue()
	{
		float num = m_displayTime - m_initialValueHoldTime;
		m_countStep = ((float)m_finalValue - m_value) / (num * 0.5f);
		m_startCount = true;
		m_fadeStep = m_lable.color.a * 1.25f / num;
		m_timer.CountDown(num, "OnTimeUp");
	}

	protected void OnTimeUp()
	{
		base.gameObject.SetActiveRecursively(false);
	}

	protected void OnEnable()
	{
		if (base.gameObject.active)
		{
			m_timer.Pause(false);
		}
		if (m_playingAnim != null)
		{
			m_playingAnim.enabled = true;
			m_playingAnim.speed = 1f;
		}
	}

	protected void OnDisable()
	{
		if (base.gameObject.active)
		{
			m_timer.Pause(true);
		}
		if (m_playingAnim != null)
		{
			m_playingAnim.enabled = false;
			m_playingAnim.speed = 0f;
		}
	}

	protected void Awake()
	{
		m_lable = base.gameObject.GetComponent<UILabel>();
		m_timer = base.gameObject.AddComponent<NBTimer>();
		m_timer.IsRealtime = false;
	}

	protected void Update()
	{
		if (m_startCount)
		{
			m_value += m_countStep * Time.deltaTime;
			if (m_countStep >= 0f && m_value >= (float)m_finalValue)
			{
				m_value = m_finalValue;
			}
			else if (m_countStep < 0f && m_value < (float)m_finalValue)
			{
				m_value = m_finalValue;
			}
			if (m_timer.Ratio <= 0.25f)
			{
				Color color = m_lable.color;
				color.a -= m_fadeStep * Time.deltaTime;
				m_lable.color = color;
			}
			m_lable.text = m_prefix + (int)m_value;
		}
	}
}
