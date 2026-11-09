// Emission and self-destruction rules of Unity 4's legacy one-shot particle emitters. Registers CD-019, CD-038.
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class MBLegacyEmitter : MonoBehaviour
{
	public bool emit = true;

	public float minEmission;

	public float maxEmission;

	public bool autodestruct;

	private static readonly System.Random s_native = new System.Random();

	private ParticleSystem m_cachedPs;

	private bool m_emitted;

	private ParticleSystem m_ps
	{
		get
		{
			if (m_cachedPs == null) m_cachedPs = GetComponent<ParticleSystem>();
			return m_cachedPs;
		}
	}

	public int particleCount
	{
		get { return m_ps.particleCount; }
	}

	public void ClearParticles()
	{
		m_ps.Clear();
	}

	public void Emit()
	{
		Emit((int)Random.Range(minEmission, maxEmission));
	}

	public void Emit(int count)
	{
		if (!isActiveAndEnabled || count <= 0)
		{
			return;
		}
		if (!m_ps.isPlaying)
		{
			m_ps.Play();
		}
		m_ps.Emit(count);
		m_emitted = true;
	}

	private void LateUpdate()
	{
		if (emit && m_ps.particleCount == 0 && !(autodestruct && m_emitted))
		{
			float rate = minEmission + (float)s_native.NextDouble() * (maxEmission - minEmission);
			Emit(Mathf.RoundToInt(rate));
		}
		if (autodestruct && m_emitted && m_ps.particleCount == 0)
		{
			Destroy(gameObject);
		}
	}

	public static ParticleSystem[] Shuriken(ParticleSystem[] all)
	{
		List<ParticleSystem> l = new List<ParticleSystem>(all.Length);
		foreach (ParticleSystem ps in all)
		{
			if (ps.GetComponent<MBLegacyEmitter>() == null)
			{
				l.Add(ps);
			}
		}
		return l.ToArray();
	}
}
