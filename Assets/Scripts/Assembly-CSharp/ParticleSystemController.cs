using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemController : MonoBehaviour
{
	private ParticleSystem[] m_ps;

	// PORT (CD-019)
	private MBLegacyEmitter[] m_legacyEmitters;

	// PORT (CD-020)
	private Dictionary<ParticleSystem, ParticleSystem.Particle[]> m_kept;

	public bool playOnStart;

	private void Awake()
	{
		m_ps = MBLegacyEmitter.Shuriken(MBHier.GetComponentsInChildren<ParticleSystem>(base.gameObject, true));
		m_legacyEmitters = MBHier.GetComponentsInChildren<MBLegacyEmitter>(base.gameObject);
		if (!playOnStart)
		{
			base.enabled = false;
		}
		else
		{
			base.enabled = true;
		}
	}

	private void Start()
	{
		if (playOnStart)
		{
			Play();
		}
	}

	public void Play()
	{
		if (m_ps != null)
		{
			if (!base.gameObject.active)
			{
				base.gameObject.active = true;
			}
			base.enabled = true;
			StartCoroutine(OnPlay());
		}
	}

	public void Stop(bool p_smooth = true)
	{
		if (m_ps == null)
		{
			return;
		}
		ParticleSystem[] ps = m_ps;
		foreach (ParticleSystem particleSystem in ps)
		{
			if (!(particleSystem == null))
			{
				particleSystem.Stop();
				if (particleSystem.gameObject.GetComponent<Animation>() != null)
				{
					particleSystem.gameObject.GetComponent<Animation>().enabled = false;
				}
				if (!p_smooth)
				{
					Keep(particleSystem);
					particleSystem.gameObject.SetActiveRecursively(false);
				}
			}
		}
		MBLegacyEmitter[] legacyEmitters = m_legacyEmitters;
		foreach (MBLegacyEmitter particleEmitter in legacyEmitters)
		{
			if (!(particleEmitter == null))
			{
				particleEmitter.emit = false;
				particleEmitter.ClearParticles();
			}
		}
		base.enabled = false;
	}

	public void Pause(bool p_pause)
	{
		if (m_ps == null)
		{
			return;
		}
		ParticleSystem[] ps = m_ps;
		foreach (ParticleSystem particleSystem in ps)
		{
			if (!(particleSystem == null))
			{
				if (p_pause)
				{
					particleSystem.Pause();
				}
				else
				{
					particleSystem.Play();
				}
				if (particleSystem.gameObject.GetComponent<Animation>() != null)
				{
					particleSystem.gameObject.GetComponent<Animation>().enabled = !p_pause;
				}
			}
		}
	}

	public bool IsPlaying()
	{
		if (m_ps == null)
		{
			return false;
		}
		ParticleSystem[] ps = m_ps;
		foreach (ParticleSystem particleSystem in ps)
		{
			if (particleSystem != null && particleSystem.isPlaying)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsPaused()
	{
		if (m_ps == null)
		{
			return false;
		}
		ParticleSystem[] ps = m_ps;
		foreach (ParticleSystem particleSystem in ps)
		{
			if (particleSystem != null && particleSystem.isPlaying)
			{
				return false;
			}
		}
		return true;
	}

	public bool IsAlive()
	{
		if (m_ps == null)
		{
			return false;
		}
		ParticleSystem[] ps = m_ps;
		foreach (ParticleSystem particleSystem in ps)
		{
			if (particleSystem != null && particleSystem.IsAlive())
			{
				return true;
			}
		}
		return false;
	}

	private void OnEnable()
	{
		if (playOnStart)
		{
			Play();
		}
		else
		{
			Stop(false);
		}
	}

	private void OnDisable()
	{
		Stop();
	}

	public void Clear()
	{
		ParticleSystem[] ps = m_ps;
		foreach (ParticleSystem particleSystem in ps)
		{
			particleSystem.Clear();
		}
	}

	public void Simulate(float p_t)
	{
		ParticleSystem[] ps = m_ps;
		foreach (ParticleSystem particleSystem in ps)
		{
			particleSystem.Simulate(p_t);
		}
	}

	private IEnumerator OnPlay()
	{
		ParticleSystem[] ps = m_ps;
		foreach (ParticleSystem ps2 in ps)
		{
			if (!(ps2 == null))
			{
				ps2.Play();
				ps2.gameObject.SetActiveRecursively(true);
				Restore(ps2);
				if (ps2.gameObject.GetComponent<Animation>() != null)
				{
					ps2.gameObject.GetComponent<Animation>().enabled = true;
				}
			}
		}
		MBLegacyEmitter[] legacyEmitters = m_legacyEmitters;
		foreach (MBLegacyEmitter e in legacyEmitters)
		{
			if (!(e == null))
			{
				e.emit = true;
				e.Emit();
			}
		}
		yield break;
	}

	private void Keep(ParticleSystem p_ps)
	{
		if (!p_ps.gameObject.activeInHierarchy || p_ps.particleCount == 0)
		{
			return;
		}
		ParticleSystem.Particle[] particles = new ParticleSystem.Particle[p_ps.particleCount];
		p_ps.GetParticles(particles);
		if (m_kept == null)
		{
			m_kept = new Dictionary<ParticleSystem, ParticleSystem.Particle[]>();
		}
		m_kept[p_ps] = particles;
	}

	private void Restore(ParticleSystem p_ps)
	{
		ParticleSystem.Particle[] particles;
		if (m_kept != null && m_kept.TryGetValue(p_ps, out particles))
		{
			m_kept.Remove(p_ps);
			p_ps.SetParticles(particles, particles.Length);
		}
	}

	private void Update()
	{
		if (!IsAlive())
		{
			Stop(false);
		}
	}
}
