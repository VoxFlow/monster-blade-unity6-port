using System;
using UnityEngine;

[Serializable]
public class SuperAttackData : SkillData
{
	[SerializeField]
	private string m_frameSprite = string.Empty;

	[SerializeField]
	private string m_glowSprite = string.Empty;

	[SerializeField]
	private string m_sliderSprite = string.Empty;

	[SerializeField]
	private float m_stunTime = 4f;

	private float m_momentum;

	public string frameSprite
	{
		get
		{
			return m_frameSprite;
		}
	}

	public string glowSprite
	{
		get
		{
			return m_glowSprite;
		}
	}

	public string sliderSprite
	{
		get
		{
			return m_sliderSprite;
		}
	}

	public float stunTime
	{
		get
		{
			return m_stunTime;
		}
	}

	public float momentum
	{
		get
		{
			return m_momentum;
		}
		set
		{
			float num = m_momentum;
			m_momentum = value;
			m_momentum = Mathf.Clamp01(m_momentum);
			if (num < 1f && m_momentum >= 1f && onMomentumFullCallback != null)
			{
				onMomentumFullCallback();
			}
			if (onMomentumUpdateCallback != null)
			{
				onMomentumUpdateCallback(m_momentum);
			}
		}
	}

	public Action onMomentumFullCallback { get; set; }

	public Action<float> onMomentumUpdateCallback { get; set; }
}
