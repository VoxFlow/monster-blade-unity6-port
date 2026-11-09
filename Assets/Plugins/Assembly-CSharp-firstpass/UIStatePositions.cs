using UnityEngine;

[AddComponentMenu("NGUI/Deprecated/State Positions")]
[ExecuteInEditMode]
public class UIStatePositions : MonoBehaviour
{
	public int currentState;

	public float duration = 0.5f;

	public Vector3[] positions;

	private Transform mTrans;

	private void Start()
	{
		mTrans = base.transform;
		Debug.LogWarning(string.Concat(NGUITools.GetHierarchy(base.gameObject), " uses a deprecated script: ", GetType(), "\nConsider switching to TweenPosition instead."), this);
	}

	private void OnState(int state)
	{
		if (currentState != state)
		{
			currentState = state;
			if (positions != null && positions.Length != 0)
			{
				int num = Mathf.Clamp(currentState, 0, positions.Length - 1);
				TweenPosition tweenPosition = UITweener.Begin<TweenPosition>(base.gameObject, duration);
				tweenPosition.method = UITweener.Method.EaseInOut;
				tweenPosition.from = mTrans.localPosition;
				tweenPosition.to = positions[num];
			}
		}
	}
}
