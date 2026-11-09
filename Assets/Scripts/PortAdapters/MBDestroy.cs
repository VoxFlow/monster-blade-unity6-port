// Object.Destroy(obj, t) with Unity 4.0.1's frame rule. Register CD-053.
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(32000)]
public class MBDestroy : MonoBehaviour
{
	private struct Pending
	{
		public Object obj;
		public float due;
		public int pass;
	}

	private static readonly List<Pending> s_pending = new List<Pending>();
	private static int s_pass;
	private static int s_clockFrame = -1;
	private static double s_frame4;
	private static double s_fixed4;
	private static bool s_capture;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Boot()
	{
		GameObject go = new GameObject("MBDestroy");
		go.hideFlags = HideFlags.HideAndDontSave;
		DontDestroyOnLoad(go);
		go.AddComponent<MBDestroy>();
		go.AddComponent<MBDestroyEarly>();
	}

	private static double FrameClock()
	{
		int fc = Time.frameCount;
		if (fc == s_clockFrame)
		{
			return s_frame4;
		}
		float inc = Time.captureDeltaTime;
		bool capture = inc > 0f;
		if (capture && s_capture && fc == s_clockFrame + 1)
		{
			s_frame4 += (double)(inc * Time.timeScale);
		}
		else
		{
			s_frame4 = Time.timeAsDouble;
			if (!capture || !s_capture)
			{
				s_fixed4 = Time.fixedTimeAsDouble;
			}
		}
		s_capture = capture;
		s_clockFrame = fc;
		return s_frame4;
	}

	public static void Destroy(Object obj, float t)
	{
		if (t <= 0f || obj == null)
		{
			Object.Destroy(obj, t);
			return;
		}
		double now = Time.inFixedTimeStep ? (s_capture ? s_fixed4 + Time.fixedDeltaTime : Time.fixedTimeAsDouble) : FrameClock();
		s_pending.Add(new Pending { obj = obj, due = (float)(t + now), pass = s_pass });
	}

	internal static void FixedPasses()
	{
		double frame = FrameClock();
		if (!s_capture)
		{
			if (Time.fixedTimeAsDouble > s_fixed4)
			{
				s_fixed4 = Time.fixedTimeAsDouble;
				Sweep((float)s_fixed4);
			}
			return;
		}
		float step = Time.fixedDeltaTime;
		int guard = 0;
		while (s_fixed4 + step <= frame && guard++ < 64)
		{
			s_fixed4 += step;
			Sweep((float)s_fixed4);
		}
	}

	private void Update()
	{
		Sweep((float)FrameClock());
	}

	private void LateUpdate()
	{
		Sweep((float)FrameClock());
	}

	private static void Sweep(float now)
	{
		s_pass++;
		int count = s_pending.Count;
		for (int i = count - 1; i >= 0; i--)
		{
			if (i >= s_pending.Count) continue;
			Pending p = s_pending[i];
			if (p.obj == null) { s_pending.RemoveAt(i); continue; }
			if (p.pass == s_pass || now < p.due) continue;
			s_pending.RemoveAt(i);
			DestroyImmediate(p.obj);
		}
	}
}

[DefaultExecutionOrder(-32000)]
public class MBDestroyEarly : MonoBehaviour
{
	private void Update()
	{
		MBDestroy.FixedPasses();
	}
}
