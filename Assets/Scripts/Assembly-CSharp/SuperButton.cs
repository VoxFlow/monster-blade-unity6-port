using System;
using UnityEngine;

[RequireComponent(typeof(UIImageButton))]
public class SuperButton : MonoBehaviour
{
	[SerializeField]
	private UISprite m_backgroundSprite;

	[SerializeField]
	private UISprite m_sliderSprite;

	[SerializeField]
	private UISlider m_slider;

	[SerializeField]
	private UISprite m_glowSprite;

	[SerializeField]
	private float m_sliderAlpha = 0.5f;

	private UIImageButton m_button;

	private SuperAttack m_superAttackLogic;

	public float momentum
	{
		private get
		{
			return m_slider.sliderValue;
		}
		set
		{
			if (!(m_slider != null))
			{
				return;
			}
			m_slider.sliderValue = value;
			if (value >= 1f)
			{
				if (m_button != null)
				{
					m_button.enabled = true;
				}
				if (m_glowSprite != null)
				{
					m_glowSprite.enabled = true;
				}
				if (m_backgroundSprite != null)
				{
					m_backgroundSprite.color = Color.white;
				}
				if (m_sliderSprite != null)
				{
					m_sliderSprite.color = Color.white;
					m_sliderSprite.alpha = 1f;
				}
			}
			else
			{
				if (m_button != null)
				{
					m_button.enabled = false;
				}
				if (m_glowSprite != null)
				{
					m_glowSprite.enabled = false;
				}
				if (m_backgroundSprite != null)
				{
					m_backgroundSprite.color = Color.grey;
				}
				if (m_sliderSprite != null)
				{
					m_sliderSprite.color = Color.grey;
					m_sliderSprite.alpha = m_sliderAlpha;
				}
			}
		}
	}

	private void Awake()
	{
		m_button = base.gameObject.GetComponent<UIImageButton>();
		Dbg.Assert(m_button);
		if (m_slider == null)
		{
			Dbg.LogWarn("[SuperButton] - Slider UI is missing");
		}
	}

	private void InitData()
	{
		if (null != SingletonComponent<SuperAttackSystem>.Instance && null != SingletonComponent<SuperAttackSystem>.Instance.superAttackLogic)
		{
			if (null != m_superAttackLogic)
			{
				SuperAttackData data = m_superAttackLogic.data;
				data.onUsableCallback = (Action<bool>)Delegate.Remove(data.onUsableCallback, new Action<bool>(OnSkillEnableOrDisable));
			}
			m_superAttackLogic = SingletonComponent<SuperAttackSystem>.Instance.superAttackLogic;
			SuperAttackData data2 = m_superAttackLogic.data;
			ChangeSprite(data2.frameSprite, data2.glowSprite, data2.sliderSprite);
			data2.onUsableCallback = (Action<bool>)Delegate.Combine(data2.onUsableCallback, new Action<bool>(OnSkillEnableOrDisable));
			momentum = data2.momentum;
		}
	}

	private void ChangeSprite(string p_frameSprite, string p_glowSprite, string p_sliderSprite)
	{
		m_button.hoverSprite = p_frameSprite;
		m_button.normalSprite = p_frameSprite;
		m_button.pressedSprite = p_frameSprite;
		m_backgroundSprite.spriteName = p_frameSprite;
		m_glowSprite.spriteName = p_glowSprite;
		m_sliderSprite.spriteName = p_sliderSprite;
	}

	private void OnBtnPressed(GameObject p_sender)
	{
		if (!(momentum < 1f))
		{
			Messenger.Invoke("player_super_atk");
		}
	}

	private void OnSkillEnableOrDisable(bool p_enable)
	{
		base.gameObject.SetActiveRecursively(p_enable);
	}

	private void OnEnable()
	{
		if (null == m_superAttackLogic)
		{
			InitData();
		}
		if (null == m_superAttackLogic || !m_superAttackLogic.data.isSkillUsable)
		{
			base.gameObject.SetActiveRecursively(false);
		}
	}

	private void Update()
	{
		if (m_superAttackLogic.data.momentum == 1f && !m_superAttackLogic.CanTriggerSuperAttack())
		{
			momentum = 0.95f;
		}
		else
		{
			momentum = m_superAttackLogic.data.momentum;
		}
	}
}
