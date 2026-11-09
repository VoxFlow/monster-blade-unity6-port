using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Deprecated/Send State (OnHover)")]
public class UISendStateOnHover : UISend
{
	public int normalState;

	public int hoverState = 1;

	private void Start()
	{
		Debug.LogWarning(string.Concat(NGUITools.GetHierarchy(base.gameObject), " uses a deprecated script: ", GetType(), "\nConsider switching to UIButtonScale, UIButtonColor, UIButtonOffset or UIButtonTween instead."), this);
	}

	private void OnHover(bool isOver)
	{
		Send((!isOver) ? normalState : hoverState);
	}
}
