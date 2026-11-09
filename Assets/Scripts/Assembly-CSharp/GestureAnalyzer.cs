using System;
using System.Collections.Generic;
using UnityEngine;

public class GestureAnalyzer : MonoBehaviour
{
	public enum GestureTypes
	{
		Unknown = 0,
		Tap = 1,
		Pinch = 2,
		Pan = 3,
		Swipe = 4,
		Rotate = 5,
		LongPress = 6
	}

	public enum GestureTouchPhase
	{
		Began = 0,
		Moved = 1,
		Stationary = 2,
		Ended = 3,
		Canceled = 4,
		Unknown = 5
	}

	public class GestureTouchInfo
	{
		public int finger = -1;

		public Vector2 firstPosition = Vector2.zero;

		public Vector2 deltaPosition = Vector2.zero;

		public Vector2 currentPosition = Vector2.zero;

		public float timeDelta;

		public float timeTotal;

		public GestureTouchPhase phase = GestureTouchPhase.Unknown;

		public GestureTypes type;
	}

	[Serializable]
	public class GestureExceptionRect
	{
		public Vector2 bottomLeft = Vector2.zero;

		public Vector2 topRight = new Vector2(1f, 1f);
	}

	public bool m_enableMultitouch;

	public float m_recognitionSensitivity = 0.05f;

	public float m_howLongIsTap = 0.1f;

	public float m_SlowestSwipeTime = 0.1f;

	public List<GestureExceptionRect> m_exceptions = new List<GestureExceptionRect>();

	private bool m_touchDown;

	private float m_timeStartTouch;

	private Dictionary<int, GestureTouchInfo> m_touches = new Dictionary<int, GestureTouchInfo>();

	private void Start()
	{
		Input.multiTouchEnabled = m_enableMultitouch;
	}

	private void Update()
	{
		if (Input.touches.Length >= 1)
		{
			if (Input.multiTouchEnabled)
			{
				UpdateMultiTouch();
			}
			else
			{
				AnalyzeSingleTouch(Input.touches[0]);
			}
		}
	}

	private void UpdateMultiTouch()
	{
		if (Input.touches.Length > 1)
		{
			AnalyzeTwoTouches(Input.touches[0], Input.touches[1]);
			if (Input.touches.Length > 2)
			{
				for (int i = 3; i < Input.touches.Length; i++)
				{
					AnalyzeSingleTouch(Input.touches[i]);
				}
			}
		}
		else
		{
			AnalyzeSingleTouch(Input.touches[0]);
		}
	}

	private void AnalyzeTwoTouches(Touch p_first, Touch p_second)
	{
	}

	private void AnalyzeSingleTouch(Touch p_touch)
	{
		Touch p_touch2 = p_touch;
		GestureTouchInfo p_info = new GestureTouchInfo();
		Dbg.Log("Touch Phase: " + p_touch2.phase);
		if (IsWithinExceptionRegions(p_touch2.position))
		{
			Dbg.LogWarn("Exception Region");
			if (p_touch2.phase == TouchPhase.Began)
			{
				return;
			}
		}
		if (p_touch2.phase == TouchPhase.Began)
		{
			m_timeStartTouch = Time.time;
			m_touchDown = true;
			UpdateTouchInfo(ref p_touch2, ref p_info);
		}
		if (p_touch2.phase == TouchPhase.Moved || p_touch2.phase == TouchPhase.Stationary)
		{
			if (!GetTouch(p_touch.fingerId, ref p_info))
			{
				return;
			}
			UpdateTouchInfo(ref p_touch2, ref p_info);
			if (m_touchDown && m_timeStartTouch + m_recognitionSensitivity <= Time.time)
			{
				EvaluateGesture(ref p_info);
				m_touchDown = false;
			}
		}
		if ((p_touch2.phase == TouchPhase.Ended || p_touch2.phase == TouchPhase.Canceled) && GetTouch(p_touch.fingerId, ref p_info))
		{
			if (m_touchDown)
			{
				EvaluateGesture(ref p_info);
			}
			m_touchDown = false;
			DestroyTouch(p_touch.fingerId);
		}
	}

	private void UpdateTouchInfo(ref Touch p_touch, ref GestureTouchInfo p_info)
	{
		if (p_touch.phase == TouchPhase.Began)
		{
			CreateTouch(p_touch.fingerId, ref p_info);
			p_info.firstPosition = p_touch.position;
			p_info.phase = GestureTouchPhase.Began;
		}
		else if (p_touch.phase == TouchPhase.Moved)
		{
			if (IsValidTouch(ref p_info))
			{
				p_info.timeDelta = p_touch.deltaTime;
				p_info.timeTotal += p_touch.deltaTime;
				p_info.currentPosition = p_touch.position;
				p_info.deltaPosition = p_touch.deltaPosition;
				p_info.phase = GestureTouchPhase.Moved;
			}
		}
		else if (p_touch.phase == TouchPhase.Stationary && IsValidTouch(ref p_info))
		{
			p_info.timeDelta = p_touch.deltaTime;
			p_info.timeTotal += p_touch.deltaTime;
			p_info.currentPosition = p_touch.position;
			p_info.deltaPosition = p_touch.deltaPosition;
			p_info.phase = GestureTouchPhase.Moved;
		}
	}

	private void EvaluateGesture(ref GestureTouchInfo p_info)
	{
		Dbg.LogWarn("EvaluateGesture");
		GestureTouchInfo gestureTouchInfo = p_info;
		if ((gestureTouchInfo.phase == GestureTouchPhase.Began || gestureTouchInfo.phase == GestureTouchPhase.Stationary) && gestureTouchInfo.timeTotal <= m_howLongIsTap)
		{
			gestureTouchInfo.type = GestureTypes.Tap;
			base.gameObject.SendMessage("OnGesture", gestureTouchInfo);
		}
		else if ((gestureTouchInfo.phase == GestureTouchPhase.Began || gestureTouchInfo.phase == GestureTouchPhase.Stationary) && gestureTouchInfo.timeTotal > m_howLongIsTap)
		{
			gestureTouchInfo.type = GestureTypes.LongPress;
			base.gameObject.SendMessage("OnGesture", gestureTouchInfo);
		}
		else if (gestureTouchInfo.phase == GestureTouchPhase.Moved)
		{
			if (gestureTouchInfo.timeTotal <= m_SlowestSwipeTime)
			{
				gestureTouchInfo.type = GestureTypes.Swipe;
				base.gameObject.SendMessage("OnGesture", gestureTouchInfo);
			}
			else
			{
				gestureTouchInfo.type = GestureTypes.Pan;
				base.gameObject.SendMessage("OnGesture", gestureTouchInfo);
			}
		}
		else
		{
			Dbg.LogError("Unknown phase to evaluate.");
		}
	}

	private bool GetTouch(int p_finger, ref GestureTouchInfo p_info)
	{
		return m_touches.TryGetValue(p_finger, out p_info);
	}

	private void CreateTouch(int p_finger, ref GestureTouchInfo p_info)
	{
		p_info.finger = p_finger;
		m_touches.Add(p_finger, p_info);
	}

	private void DestroyTouch(int p_finger)
	{
		m_touches.Remove(p_finger);
	}

	private bool IsValidTouch(ref GestureTouchInfo p_touch)
	{
		return p_touch.finger != -1 || p_touch.timeDelta != 0f || p_touch.timeTotal != 0f || p_touch.phase != GestureTouchPhase.Unknown || !(p_touch.deltaPosition == Vector2.zero) || !(p_touch.currentPosition == Vector2.zero);
	}

	public bool IsWithinExceptionRegions(Vector2 p_point)
	{
		bool result = false;
		foreach (GestureExceptionRect exception in m_exceptions)
		{
			if (p_point.x >= exception.bottomLeft.x && p_point.x <= exception.topRight.x && p_point.y >= exception.bottomLeft.y && p_point.y <= exception.topRight.y)
			{
				result = true;
				break;
			}
		}
		return result;
	}
}
