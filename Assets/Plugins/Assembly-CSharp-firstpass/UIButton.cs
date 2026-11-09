using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Button")]
public class UIButton : UIButtonColor
{
	public float greyScale = 0.2f;

	public GameObject[] otherTargets;

	public bool enableHover = true;

	private bool _locked;

	private UILabel[] m_btnTexts;

	private UISprite m_btnBackground;

	public bool isLocked
	{
		get
		{
			return _locked;
		}
		set
		{
			_locked = value;
			isEnabled = !_locked;
		}
	}

	public bool isEnabled
	{
		get
		{
			Collider collider = base.GetComponent<Collider>();
			return (bool)collider && collider.enabled;
		}
		set
		{
			Collider collider = base.GetComponent<Collider>();
			if ((bool)collider && collider.enabled != value)
			{
				bool shouldBeEnabled = (collider.enabled = !_locked && value);
				UpdateColor(shouldBeEnabled, false);
			}
		}
	}

	public bool isFlashing
	{
		set
		{
			if (value)
			{
				Color color = base.defaultColor;
				color.r *= greyScale;
				color.g *= greyScale;
				color.b *= greyScale;
				TweenColor tweenColor = TweenColor.Begin(tweenTarget, 0.5f, color);
				tweenColor.from = Color.white;
				tweenColor.to = Color.grey;
				tweenColor.method = UITweener.Method.EaseIn;
				tweenColor.style = UITweener.Style.PingPong;
			}
			else
			{
				TweenColor component = tweenTarget.GetComponent<TweenColor>();
				if (null != component)
				{
					component.color = base.defaultColor;
					component.enabled = false;
				}
			}
		}
	}

	protected override void OnEnable()
	{
		if (m_btnTexts == null)
		{
			m_btnTexts = MBHier.GetComponentsInChildren<UILabel>(base.transform);
		}
		if (isEnabled)
		{
			base.OnEnable();
		}
		else
		{
			UpdateColor(false, false);
		}
	}

	public void SetButtonGrey(bool p_grey)
	{
		if (m_btnBackground == null)
		{
			m_btnBackground = MBHier.GetComponentInChildren<UISprite>(base.transform);
		}
		if (m_btnBackground != null)
		{
			if (p_grey)
			{
				m_btnBackground.spriteName = "buttons/grey_btn";
			}
			else
			{
				m_btnBackground.spriteName = "buttons/green_btn";
			}
		}
		isEnabled = !p_grey;
	}

	private void UpdateColor(bool shouldBeEnabled, bool immediate)
	{
		if (!tweenTarget)
		{
			return;
		}
		if (!mInitDone)
		{
			Init();
		}
		Color color = base.defaultColor;
		if (!shouldBeEnabled)
		{
			color.r *= greyScale;
			color.g *= greyScale;
			color.b *= greyScale;
		}
		TweenColor tweenColor = TweenColor.Begin(tweenTarget, 0.15f, color);
		if (immediate)
		{
			tweenColor.color = color;
			tweenColor.enabled = false;
		}
		GameObject[] array = otherTargets;
		foreach (GameObject gameObject in array)
		{
			if ((bool)gameObject)
			{
				tweenColor = TweenColor.Begin(gameObject, 0.15f, color);
				if (immediate)
				{
					tweenColor.color = color;
					tweenColor.enabled = false;
				}
			}
		}
		if (m_btnTexts == null)
		{
			return;
		}
		UILabel[] btnTexts = m_btnTexts;
		foreach (UILabel uILabel in btnTexts)
		{
			bool hasColorTag = uILabel.hasColorTag;
			if (!shouldBeEnabled && hasColorTag)
			{
				Color color2 = color;
				color2.a *= greyScale;
				uILabel.color = color2;
			}
			else
			{
				uILabel.color = color;
			}
		}
	}

	public override void OnPress(bool isPressed)
	{
		base.OnPress(isPressed);
		if (!base.enabled)
		{
			return;
		}
		GameObject[] array = otherTargets;
		foreach (GameObject gameObject in array)
		{
			if (null != gameObject)
			{
				TweenColor.Begin(gameObject, duration, isPressed ? pressed : ((!UICamera.IsHighlighted(gameObject)) ? mColor : hover));
			}
		}
	}

	public override void OnHover(bool isOver)
	{
		if (!enableHover)
		{
			return;
		}
		base.OnHover(isOver);
		if (!base.enabled)
		{
			return;
		}
		GameObject[] array = otherTargets;
		foreach (GameObject gameObject in array)
		{
			if (null != gameObject)
			{
				TweenColor.Begin(gameObject, duration, (!isOver) ? mColor : hover);
			}
		}
	}
}
