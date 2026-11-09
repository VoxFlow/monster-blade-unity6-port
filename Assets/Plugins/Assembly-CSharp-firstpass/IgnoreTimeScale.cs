using UnityEngine;

[AddComponentMenu("NGUI/Internal/Ignore TimeScale Behaviour")]
public class IgnoreTimeScale : MonoBehaviour
{
	private float mTime;

	private float mActual;

	private float mDelta;

	public float realTimeDelta
	{
		get
		{
			return mDelta;
		}
	}

	protected virtual void OnEnable()
	{
		mTime = UnityEngine.Time.realtimeSinceStartup;
	}

	protected virtual void Start()
	{
		mTime = UnityEngine.Time.realtimeSinceStartup;
	}

	protected float UpdateRealTimeDelta()
	{
		float realtimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;
		float b = realtimeSinceStartup - mTime;
		mActual += Mathf.Max(0f, b);
		mDelta = 0.001f * Mathf.Round(mActual * 1000f);
		mActual -= mDelta;
		if (mDelta > 1f)
		{
			mDelta = 1f;
		}
		mTime = realtimeSinceStartup;
		return mDelta;
	}
}
