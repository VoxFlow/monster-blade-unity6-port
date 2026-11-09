using UnityEngine;

[AddComponentMenu("NGUI/Deprecated/Send State")]
[ExecuteInEditMode]
public class UISendState : UISend
{
	public int normalState;

	public int hoverState = 1;

	public int pressedState = 2;

	private void Start()
	{
		Debug.LogWarning(string.Concat(NGUITools.GetHierarchy(base.gameObject), " uses a deprecated script: ", GetType(), "\nConsider switching to UIButtonScale, UIButtonColor, UIButtonOffset or UIButtonTween instead."), this);
	}

	private void OnHover(bool isOver)
	{
		Send((!isOver) ? normalState : hoverState);
	}

	private void OnPress(bool pressed)
	{
		Send((!pressed) ? normalState : pressedState);
	}
}
