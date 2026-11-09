using UnityEngine;

public class StandardMenuPanel : NBUIMenuPanel
{
	public GameObject[] buttons;

	public GameObject target;

	public GameObject[] otherDisplayObjs;

	public virtual void OnEnable()
	{
		if (target != null)
		{
			NBUIMenuScreen component = target.GetComponent<NBUIMenuScreen>();
			if (component != null)
			{
				component.UpdateRedIconNum();
				component.StandardPanelOnEnablePostProcess();
			}
		}
	}

	public virtual void OnDisable()
	{
		GameObject[] array = buttons;
		foreach (GameObject gameObject in array)
		{
			if ((bool)gameObject)
			{
				UIButtonScale component = gameObject.GetComponent<UIButtonScale>();
				component.Reset();
				UIButtonColor component2 = gameObject.GetComponent<UIButtonColor>();
				component2.Reset();
			}
		}
	}

	public override void OnEnterTransition()
	{
		GameObject[] array = buttons;
		foreach (GameObject gameObject in array)
		{
			gameObject.SendMessage("OnEnter", null, SendMessageOptions.DontRequireReceiver);
		}
		GameObject[] array2 = otherDisplayObjs;
		foreach (GameObject gameObject2 in array2)
		{
			gameObject2.SendMessage("OnEnter", null, SendMessageOptions.DontRequireReceiver);
		}
		if (target != null)
		{
			NBUIMenuScreen component = target.GetComponent<NBUIMenuScreen>();
			if (component != null)
			{
				component.UpdateRedIconNum();
			}
		}
	}

	public override void OnExitTransition()
	{
		GameObject[] array = buttons;
		foreach (GameObject gameObject in array)
		{
			gameObject.SendMessage("OnExit", null, SendMessageOptions.DontRequireReceiver);
		}
		GameObject[] array2 = otherDisplayObjs;
		foreach (GameObject gameObject2 in array2)
		{
			gameObject2.SendMessage("OnExit", null, SendMessageOptions.DontRequireReceiver);
		}
	}

	protected virtual void Start()
	{
		int num = 0;
		for (int i = 0; i < buttons.Length; i++)
		{
			GameObject gameObject = buttons[i];
			UIButtonMessage component = gameObject.GetComponent<UIButtonMessage>();
			if (component != null)
			{
				component.id = num++;
				component.target = target;
				component.functionName = "OnButtonClick";
			}
		}
	}

	public override void EnableButton(int p_index, bool p_enable)
	{
		if (p_index < buttons.Length)
		{
			UIButton component = buttons[p_index].GetComponent<UIButton>();
			if (component != null)
			{
				component.isEnabled = p_enable;
			}
		}
	}

	public override void LockButton(int p_index, bool p_locked)
	{
		if (p_index < buttons.Length)
		{
			UIButton component = buttons[p_index].GetComponent<UIButton>();
			if (component != null)
			{
				component.isLocked = p_locked;
			}
		}
	}
}
