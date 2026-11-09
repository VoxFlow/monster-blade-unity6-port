using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Button Message")]
public class UIButtonMessage : MonoBehaviour
{
	public enum Trigger
	{
		OnClick = 0,
		OnMouseOver = 1,
		OnMouseOut = 2,
		OnPress = 3,
		OnRelease = 4,
		OnDoubleClick = 5
	}

	public int id;

	public GameObject target;

	public string functionName;

	public Trigger trigger;

	public bool includeChildren;

	[SerializeField]
	private float m_clickCooldownInSeconds = 0.25f;

	private float m_lastTime;

	private bool mStarted;

	private bool mHighlighted;

	private void Start()
	{
		mStarted = true;
	}

	private void OnEnable()
	{
		if (mStarted && mHighlighted)
		{
			OnHover(UICamera.IsHighlighted(base.gameObject));
		}
	}

	private void OnHover(bool isOver)
	{
		if (base.enabled)
		{
			if ((isOver && trigger == Trigger.OnMouseOver) || (!isOver && trigger == Trigger.OnMouseOut))
			{
				Send();
			}
			mHighlighted = isOver;
		}
	}

	private void OnPress(bool isPressed)
	{
		if (base.enabled && ((isPressed && trigger == Trigger.OnPress) || (!isPressed && trigger == Trigger.OnRelease)))
		{
			Send();
		}
	}

	private void OnClick()
	{
		if (base.enabled && trigger == Trigger.OnClick)
		{
			Send();
		}
	}

	private void OnDoubleClick()
	{
		if (base.enabled && trigger == Trigger.OnDoubleClick)
		{
			Send();
		}
	}

	private void Send()
	{
		if (string.IsNullOrEmpty(functionName))
		{
			return;
		}
		if (target == null)
		{
			target = base.gameObject;
		}
		if (!CanSend())
		{
			return;
		}
		if (includeChildren)
		{
			Transform[] componentsInChildren = MBHier.GetComponentsInChildren<Transform>(target);
			int i = 0;
			for (int num = componentsInChildren.Length; i < num; i++)
			{
				Transform transform = componentsInChildren[i];
				transform.gameObject.SendMessage(functionName, base.gameObject, SendMessageOptions.DontRequireReceiver);
			}
		}
		else
		{
			target.SendMessage(functionName, base.gameObject, SendMessageOptions.DontRequireReceiver);
		}
	}

	private bool CanSend()
	{
		float realtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
		float num = realtimeSinceStartup - m_lastTime;
		if (num < m_clickCooldownInSeconds)
		{
			return false;
		}
		m_lastTime = realtimeSinceStartup;
		return true;
	}
}
