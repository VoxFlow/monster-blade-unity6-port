using UnityEngine;

[AddComponentMenu("NGUI/Deprecated/Send State (OnClick)")]
[ExecuteInEditMode]
public class UISendStateOnClick : UISend
{
	public int clickState = 1;

	private void Start()
	{
		Debug.LogWarning(string.Concat(NGUITools.GetHierarchy(base.gameObject), " uses a deprecated script: ", GetType(), "\nConsider switching to UIButtonScale, UIButtonColor, UIButtonOffset or UIButtonTween instead."), this);
	}

	private void OnClick()
	{
		Send(clickState);
	}
}
