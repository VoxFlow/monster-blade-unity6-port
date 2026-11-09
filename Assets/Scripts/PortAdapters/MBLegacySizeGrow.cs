// Unity 4's legacy particle size growth. Register CD-017.
using Unity.Collections;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;

[RequireComponent(typeof(ParticleSystem))]
public class MBLegacySizeGrow : MonoBehaviour
{
	public float sizeGrow;

	private struct GrowJob : IJobParticleSystemParallelFor
	{
		public float factor;

		public void Execute(ParticleSystemJobData data, int i)
		{
			var x = data.sizes.x;
			var y = data.sizes.y;
			var z = data.sizes.z;
			x[i] *= factor;
			if (y.IsCreated) y[i] *= factor;
			if (z.IsCreated) z[i] *= factor;
		}
	}

	private ParticleSystem m_ps;

	private void Awake()
	{
		m_ps = GetComponent<ParticleSystem>();
	}

	private void OnParticleUpdateJobScheduled()
	{
		if (sizeGrow == 0f) return;
		var job = new GrowJob { factor = Mathf.Pow(1f + sizeGrow, Time.deltaTime) };
		job.Schedule(m_ps, 256);
	}
}
