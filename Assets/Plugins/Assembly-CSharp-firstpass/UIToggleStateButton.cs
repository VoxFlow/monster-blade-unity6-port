using UnityEngine;

public class UIToggleStateButton : MonoBehaviour
{
	public UISprite normalSprite;

	public UISprite pressedSprite;

	public GameObject eventReceiver;

	public string functionName = "OnStateChange";

	private bool mChecked = true;

	private Transform mTrans;

	public bool isSelected
	{
		get
		{
			return mChecked;
		}
		set
		{
			Set(value);
		}
	}

	private void Start()
	{
		mTrans = base.transform;
		mChecked = false;
		Set(mChecked);
	}

	private void OnClick()
	{
		if (base.enabled)
		{
			isSelected = !isSelected;
		}
		if (eventReceiver != null && !string.IsNullOrEmpty(functionName))
		{
			eventReceiver.SendMessage(functionName, base.gameObject, SendMessageOptions.DontRequireReceiver);
		}
	}

	private void Set(bool state)
	{
		mChecked = state;
		if (state)
		{
			normalSprite.enabled = false;
			pressedSprite.enabled = true;
			pressedSprite.MakePixelPerfect();
		}
		else
		{
			normalSprite.enabled = true;
			pressedSprite.enabled = false;
			normalSprite.MakePixelPerfect();
		}
	}
}
