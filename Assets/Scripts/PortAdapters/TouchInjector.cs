// Produces one virtual finger's touches for the PC controls, before every game script each frame. Register CD-029.
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-32000)]
public class TouchInjector : MonoBehaviour
{
	private struct Frame
	{
		public Vector2 pos;

		public TouchPhase phase;
	}

	private readonly Queue<Frame> m_frames = new Queue<Frame>();

	private bool m_holding;

	private Vector2 m_holdPos;

	private Vector2 m_lastPos;

	public bool Busy
	{
		get { return m_frames.Count > 0 || m_holding; }
	}

	public bool Holding
	{
		get { return m_holding; }
	}

	public void Tap(Vector2 pos, int heldFrames = 2)
	{
		Add(pos, TouchPhase.Began);
		for (int i = 0; i < heldFrames; i++)
		{
			Add(pos, TouchPhase.Stationary);
		}
		Add(pos, TouchPhase.Ended);
	}

	public void Swipe(Vector2 from, Vector2 to, int frames = 6)
	{
		Add(from, TouchPhase.Began);
		for (int i = 1; i <= frames; i++)
		{
			Add(Vector2.Lerp(from, to, (float)i / frames), TouchPhase.Moved);
		}
		Add(to, TouchPhase.Ended);
	}

	public void Hold(Vector2 pos)
	{
		Add(pos, TouchPhase.Began);
		m_holding = true;
		m_holdPos = pos;
	}

	public void Release()
	{
		if (m_holding)
		{
			m_holding = false;
			Add(m_holdPos, TouchPhase.Ended);
		}
	}

	private void Add(Vector2 pos, TouchPhase phase)
	{
		m_frames.Enqueue(new Frame { pos = pos, phase = phase });
	}

	private void Update()
	{
		Frame f;
		if (m_frames.Count > 0)
		{
			f = m_frames.Dequeue();
		}
		else if (m_holding)
		{
			f = new Frame { pos = m_holdPos, phase = TouchPhase.Stationary };
		}
		else
		{
			MBInput.SetTouches(null);
			return;
		}
		Touch t = new Touch();
		t.fingerId = 0;
		t.position = f.pos;
		t.rawPosition = f.pos;
		t.deltaPosition = f.phase == TouchPhase.Began ? Vector2.zero : f.pos - m_lastPos;
		t.deltaTime = Time.unscaledDeltaTime;
		t.tapCount = 1;
		t.phase = f.phase;
		m_lastPos = f.pos;
		MBInput.SetTouches(new Touch[] { t });
	}
}
