using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Deprecated/Send State (OnPress)")]
public class UISendStateOnPress : UISend
{
	public int normalState;

	public int pressedState = 1;

	private void Start()
	{
		Debug.LogWarning(string.Concat(NGUITools.GetHierarchy(base.gameObject), " uses a deprecated script: ", GetType(), "\nConsider switching to UIButtonScale, UIButtonColor, UIButtonOffset or UIButtonTween instead."), this);
	}

	private void OnPress(bool pressed)
	{
		Send((!pressed) ? normalState : pressedState);
	}
}
