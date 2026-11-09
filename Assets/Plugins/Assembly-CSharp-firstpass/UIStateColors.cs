using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Deprecated/State Colors")]
public class UIStateColors : MonoBehaviour
{
	public int currentState;

	public float duration = 0.2f;

	public Color[] colors;

	private void Start()
	{
		Debug.LogWarning(string.Concat(NGUITools.GetHierarchy(base.gameObject), " uses a deprecated script: ", GetType(), "\nYou can remove this script by using UIButtonColors on your button instead."), this);
	}

	private void OnState(int state)
	{
		if (currentState != state)
		{
			currentState = state;
			if (colors != null && colors.Length != 0)
			{
				int num = Mathf.Clamp(currentState, 0, colors.Length - 1);
				TweenColor.Begin(base.gameObject, duration, colors[num]);
			}
		}
	}
}
