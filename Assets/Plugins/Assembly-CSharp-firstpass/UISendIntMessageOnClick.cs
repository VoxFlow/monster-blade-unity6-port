using UnityEngine;

[AddComponentMenu("NGUI/Deprecated/Send Int Message (On Click)")]
[ExecuteInEditMode]
public class UISendIntMessageOnClick : UISend
{
	public string functionName = "OnSendMessage";

	public int valueToSend;

	private void Start()
	{
		Debug.LogWarning(string.Concat(NGUITools.GetHierarchy(base.gameObject), " uses a deprecated script: ", GetType(), "\nConsider using UIButtonMessage or UIButtonTween instead."), this);
	}

	private void OnClick()
	{
		Send(functionName, valueToSend);
	}
}
