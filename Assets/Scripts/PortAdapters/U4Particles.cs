// Unity 4 particle rules applied to the converted systems: start randoms, bursts, damping, start delay and prewarm. Registers
// CD-051, CD-052, CD-061.
using System.Collections.Generic;
using UnityEngine;

public static class U4Particles
{
	public static bool Enabled = true;

	public static bool PrewarmPhaseEnabled = true;

	sealed class State
	{
		public bool shared;
		public bool bursts;
		public bool rateZero;
		public HashSet<uint> seen = new HashSet<uint>();
		public HashSet<uint> next = new HashSet<uint>();
		public float prewarmShift;
		public bool wasPlaying;
		public int lastCount = -1;
	}

	static readonly Dictionary<int, State> s_states = new Dictionary<int, State>();

	static ParticleSystem.Particle[] s_buf = new ParticleSystem.Particle[256];

	static readonly Dictionary<int, int> s_ageCount = new Dictionary<int, int>();

	static int s_frame = -1;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	static void Install()
	{
		Camera.onPreCull += OnPreCull;
	}

	static void OnPreCull(Camera cam)
	{
		if (!Enabled || s_frame == Time.frameCount)
		{
			return;
		}
		s_frame = Time.frameCount;
		foreach (ParticleSystem ps in Object.FindObjectsByType<ParticleSystem>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
		{
			State s = Prepare(ps);
			PrewarmPhase(ps, s);
			if (ps.particleCount > 0 && (s.shared || s.bursts))
			{
				Apply(ps, Time.deltaTime);
			}
		}
	}

	static State Prepare(ParticleSystem ps)
	{
		State s;
		if (s_states.TryGetValue(ps.GetInstanceID(), out s))
		{
			return s;
		}
		s = new State();
		ParticleSystem.MainModule m = ps.main;
		m.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
		if (ps.GetComponent<MBLegacyEmitter>() != null)
		{
			s_states[ps.GetInstanceID()] = s;
			return s;
		}
		int random = (m.startLifetime.mode == ParticleSystemCurveMode.TwoConstants ? 1 : 0)
			+ (!m.startSize3D && m.startSize.mode == ParticleSystemCurveMode.TwoConstants ? 1 : 0)
			+ (!m.startRotation3D && m.startRotation.mode == ParticleSystemCurveMode.TwoConstants ? 1 : 0);
		s.shared = random >= 2;
		ParticleSystem.EmissionModule em = ps.emission;
		s.bursts = em.enabled && em.burstCount > 0;
		s.rateZero = em.rateOverTime.constantMax == 0f && em.rateOverDistance.constantMax == 0f;
		s.prewarmShift = PrewarmShift(ps);
		s_states[ps.GetInstanceID()] = s;
		return s;
	}

	static float PrewarmShift(ParticleSystem ps)
	{
		ParticleSystem.MainModule m = ps.main;
		ParticleSystem.EmissionModule em = ps.emission;
		if (!m.prewarm || !m.loop || !em.enabled || ps.subEmitters.enabled || m.simulationSpeed != 1f)
		{
			return 0f;
		}
		if (em.rateOverTime.mode != ParticleSystemCurveMode.Constant || em.rateOverTime.constant <= 0f
			|| em.rateOverDistance.mode != ParticleSystemCurveMode.Constant || em.rateOverDistance.constant != 0f)
		{
			return 0f;
		}
		ParticleSystemCurveMode lm = m.startLifetime.mode;
		if (lm != ParticleSystemCurveMode.Constant && lm != ParticleSystemCurveMode.TwoConstants)
		{
			return 0f;
		}
		if (IsCurve(m.startSpeed) || IsCurve(m.startSize) || IsCurve(m.startRotation)
			|| (m.startColor.mode != ParticleSystemGradientMode.Color && m.startColor.mode != ParticleSystemGradientMode.TwoColors))
		{
			return 0f;
		}
		float rate = em.rateOverTime.constant;
		float life = lm == ParticleSystemCurveMode.Constant ? m.startLifetime.constant : Mathf.Max(m.startLifetime.constantMin, m.startLifetime.constantMax);
		float seed = U4PrewarmSeed(rate, life, m.duration, Time.fixedDeltaTime);
		return seed >= 1f ? 0f : seed / rate;
	}

	static bool IsCurve(ParticleSystem.MinMaxCurve c)
	{
		return c.mode == ParticleSystemCurveMode.Curve || c.mode == ParticleSystemCurveMode.TwoCurves;
	}

	public static float U4PrewarmSeed(float rate, float life, float length, float fixedDt)
	{
		float startT = -life;
		while (startT < 0f)
		{
			startT += length;
		}
		float endT = startT + life;
		float acc = 0f;
		while (startT + 0.0001f < endT)
		{
			float t0 = Mathf.Max(0f, startT % length);
			float t1 = Mathf.Max(0f, (startT + fixedDt) % length);
			float n = 0f;
			if (t1 < t0)
			{
				n += rate * t1;
				t1 = length;
			}
			n += rate * (t1 - t0);
			acc += n;
			acc -= (int)acc;
			startT += fixedDt;
		}
		return 1f - acc + 0.0001f;
	}

	static void PrewarmPhase(ParticleSystem ps, State s)
	{
		bool playing = ps.isPlaying;
		if (PrewarmPhaseEnabled && s.prewarmShift > 0f && playing && !s.wasPlaying && s.lastCount <= 0)
		{
			ParticleSystem.EmissionModule em = ps.emission;
			int bursts = em.burstCount;
			ParticleSystem.Burst[] saved = null;
			if (bursts > 0)
			{
				saved = new ParticleSystem.Burst[bursts];
				em.GetBursts(saved);
				em.burstCount = 0;
			}
			float time = ps.time;
			ps.Simulate(s.prewarmShift, false, false, false);
			if (saved != null)
			{
				em.SetBursts(saved);
			}
			ps.time = time;
			ps.Play(false);
		}
		s.wasPlaying = playing;
		s.lastCount = ps.particleCount;
	}

	static float U4BurstAge(float a6, float dt)
	{
		int n = dt <= 0.03f ? 1 : Mathf.CeilToInt(dt / 0.03f);
		float h = dt / n;
		int k = Mathf.Clamp(Mathf.CeilToInt((dt - a6) / h - 1e-4f), 1, n);
		return (n - k) * h;
	}

	public static void Apply(ParticleSystem ps, float dt)
	{
		dt *= ps.main.simulationSpeed;
		State s = Prepare(ps);
		if (!s.shared && !s.bursts)
		{
			return;
		}
		if (s_buf.Length < ps.particleCount)
		{
			s_buf = new ParticleSystem.Particle[Mathf.NextPowerOfTwo(ps.particleCount)];
		}
		int n = ps.GetParticles(s_buf);
		ParticleSystem.MainModule main = ps.main;
		ParticleSystem.MinMaxCurve lifeC = main.startLifetime;
		ParticleSystem.MinMaxCurve sizeC = main.startSize;
		ParticleSystem.MinMaxCurve rotC = main.startRotation;
		bool lifeR = lifeC.mode == ParticleSystemCurveMode.TwoConstants;
		bool sizeR = !main.startSize3D && sizeC.mode == ParticleSystemCurveMode.TwoConstants;
		bool rotR = !main.startRotation3D && rotC.mode == ParticleSystemCurveMode.TwoConstants;
		if (s.bursts && !s.rateZero)
		{
			s_ageCount.Clear();
			for (int i = 0; i < n; i++)
			{
				if (!s.seen.Contains(s_buf[i].randomSeed))
				{
					int key = Mathf.RoundToInt((s_buf[i].startLifetime - s_buf[i].remainingLifetime) * 100000f);
					int c;
					s_ageCount.TryGetValue(key, out c);
					s_ageCount[key] = c + 1;
				}
			}
		}
		bool changed = false;
		s.next.Clear();
		for (int i = 0; i < n; i++)
		{
			uint seed = s_buf[i].randomSeed;
			s.next.Add(seed);
			if (s.seen.Contains(seed))
			{
				continue;
			}
			float age = s_buf[i].startLifetime - s_buf[i].remainingLifetime;
			if (s.shared)
			{
				float t = lifeR ? Fraction(lifeC, s_buf[i].startLifetime) : Fraction(sizeC, s_buf[i].startSize);
				if (sizeR)
				{
					s_buf[i].startSize = Mathf.LerpUnclamped(sizeC.constantMin, sizeC.constantMax, t);
				}
				if (rotR)
				{
					s_buf[i].rotation = Mathf.LerpUnclamped(rotC.constantMin, rotC.constantMax, t) * Mathf.Rad2Deg + s_buf[i].angularVelocity * age;
				}
				changed = true;
			}
			if (s.bursts && age >= 0f && age <= dt + 1e-4f)
			{
				bool burst = s.rateZero;
				if (!burst)
				{
					int c;
					s_ageCount.TryGetValue(Mathf.RoundToInt(age * 100000f), out c);
					burst = c >= 2;
				}
				if (burst)
				{
					float back = age - U4BurstAge(age, dt);
					s_buf[i].remainingLifetime += back;
					s_buf[i].position -= s_buf[i].totalVelocity * back;
					s_buf[i].rotation -= s_buf[i].angularVelocity * back;
					changed = true;
				}
			}
		}
		HashSet<uint> swap = s.seen;
		s.seen = s.next;
		s.next = swap;
		if (changed)
		{
			ps.SetParticles(s_buf, n);
		}
	}

	public static float GetStartLifetime(ParticleSystem ps)
	{
		ParticleSystem.MinMaxCurve c = ps.main.startLifetime;
		switch (c.mode)
		{
		case ParticleSystemCurveMode.TwoConstants:
			return Mathf.Max(Mathf.Abs(c.constantMin), Mathf.Abs(c.constantMax));
		case ParticleSystemCurveMode.Constant:
			return c.constant;
		default:
			return c.curveMultiplier;
		}
	}

	public static void SetStartLifetime(ParticleSystem ps, float value)
	{
		ParticleSystem.MainModule main = ps.main;
		ParticleSystem.MinMaxCurve c = main.startLifetime;
		switch (c.mode)
		{
		case ParticleSystemCurveMode.TwoConstants:
		{
			float old = GetStartLifetime(ps);
			float k = old == 0f ? 0f : value / old;
			c.constantMin *= k;
			c.constantMax *= k;
			break;
		}
		case ParticleSystemCurveMode.Constant:
			c.constant = value;
			break;
		default:
			c.curveMultiplier = value;
			break;
		}
		main.startLifetime = c;
	}

	static float Fraction(ParticleSystem.MinMaxCurve c, float v)
	{
		float range = c.constantMax - c.constantMin;
		return range == 0f ? 0f : (v - c.constantMin) / range;
	}
}
