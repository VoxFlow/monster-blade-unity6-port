using UnityEngine;

[AddComponentMenu("NGUI/Deprecated/State Rotations")]
[ExecuteInEditMode]
public class UIStateRotations : MonoBehaviour
{
	public int currentState;

	public float duration = 0.5f;

	public Vector3[] rotations;

	private Transform mTrans;

	private void Start()
	{
		mTrans = base.transform;
		Debug.LogWarning(string.Concat(NGUITools.GetHierarchy(base.gameObject), " uses a deprecated script: ", GetType(), "\nConsider switching to TweenRotation instead."), this);
	}

	private void OnState(int state)
	{
		if (currentState != state)
		{
			currentState = state;
			if (rotations != null && rotations.Length != 0)
			{
				int num = Mathf.Clamp(currentState, 0, rotations.Length - 1);
				TweenRotation tweenRotation = UITweener.Begin<TweenRotation>(base.gameObject, duration);
				tweenRotation.method = UITweener.Method.EaseInOut;
				tweenRotation.from = mTrans.localRotation.eulerAngles;
				tweenRotation.to = rotations[num];
			}
		}
	}
}
