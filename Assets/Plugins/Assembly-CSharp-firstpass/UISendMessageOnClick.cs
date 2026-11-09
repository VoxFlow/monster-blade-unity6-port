using UnityEngine;

[AddComponentMenu("NGUI/Deprecated/Send Message (OnClick)")]
[ExecuteInEditMode]
public class UISendMessageOnClick : MonoBehaviour
{
	public GameObject target;

	public bool includeChildren;

	public string functionName = "OnSendMessage";

	private void Awake()
	{
		Debug.LogWarning(string.Concat(NGUITools.GetHierarchy(base.gameObject), " uses a deprecated script: ", GetType(), "\nUpgrading to UIButtonMessage."), this);
		UIButtonMessage uIButtonMessage = base.gameObject.AddComponent<UIButtonMessage>();
		uIButtonMessage.functionName = functionName;
		uIButtonMessage.target = target;
		uIButtonMessage.trigger = UIButtonMessage.Trigger.OnClick;
		uIButtonMessage.includeChildren = includeChildren;
		Object.DestroyImmediate(this);
	}
}
