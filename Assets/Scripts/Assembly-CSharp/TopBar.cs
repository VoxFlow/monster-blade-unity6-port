using UnityEngine;

public class TopBar : MonoBehaviour
{
	public UILabel labelStamina;

	public UILabel labelStaminaDecrement;

	public UILabel labelPlayerName;

	public UILabel labelIGC;

	public UILabel labelPC;

	public UISlider sliderXP;

	public UILabel labelXPPercentage;

	public UILabel labelTimer;

	public UIButton buttonNubee;

	public GameObject buttonAddStamina;

	public GameObject buttonAddPC;

	private void Start()
	{
	}

	public void SetName(string name, int lvl)
	{
		labelPlayerName.text = name + " (LVL " + lvl + ")";
	}

	public void SetIGC(int p_value)
	{
		if (null != labelIGC)
		{
			CountingNumber component = labelIGC.GetComponent<CountingNumber>();
			if (null != component)
			{
				component.SetValue(p_value);
			}
			else
			{
				labelIGC.text = p_value.ToString();
			}
		}
	}

	public void SetPC(int p_value)
	{
		if (null != labelPC)
		{
			CountingNumber component = labelPC.GetComponent<CountingNumber>();
			if (null != component)
			{
				component.SetValue(p_value);
			}
			else
			{
				labelPC.text = p_value.ToString();
			}
		}
	}

	public void SetXP(float rate, int level = 0)
	{
		if (level >= 50)
		{
			rate = 0f;
		}
		sliderXP.sliderValue = rate;
		labelXPPercentage.text = Mathf.FloorToInt(rate * 100f) + "%";
	}

	public void EnableAddStaminaBtn(bool p_enable)
	{
		UIButton component = buttonAddStamina.GetComponent<UIButton>();
		if (component != null)
		{
			component.isEnabled = p_enable;
		}
	}

	public void EnableAddPCBtn(bool p_enable)
	{
		UIButton component = buttonAddPC.GetComponent<UIButton>();
		if (component != null)
		{
			component.isEnabled = p_enable;
		}
	}

	public void DecrementStamina(int decrementValue)
	{
		if (labelStamina != null && labelStaminaDecrement != null)
		{
			Vector2 relativeSize = labelStamina.relativeSize;
			Vector3 localPosition = labelStamina.transform.localPosition;
			localPosition.x -= relativeSize.x * labelStamina.transform.localScale.x;
			labelStaminaDecrement.transform.localPosition = localPosition;
			labelStaminaDecrement.color = Color.red;
			labelStaminaDecrement.text = "-" + decrementValue;
			labelStaminaDecrement.gameObject.active = true;
			TweenColor.Begin(labelStaminaDecrement.gameObject, 1f, new Color(1f, 1f, 1f, 0f));
			TweenPosition.Begin(labelStaminaDecrement.gameObject, 1f, new Vector3(localPosition.x, localPosition.y - 100f, localPosition.z));
		}
	}
}
