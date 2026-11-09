using UnityEngine;

public class SlashEffectEmitter : MonoBehaviour
{
	public enum FxType
	{
		CleanHit = 0,
		GrazeHit = 1,
		GuardHit = 2,
		CriticalHit = 3,
		Max = 4
	}

	private int m_effectListSize = 4;

	public ParticleSystemController[] m_effectTemplates;

	public ParticleSystemController[,] m_effects;

	private void Awake()
	{
		m_effects = new ParticleSystemController[4, m_effectListSize];
	}

	private void Start()
	{
		for (int i = 0; i < 4; i++)
		{
			if (m_effectTemplates[i] == null)
			{
				Debug.LogError("Some of slash fx templates have not been provied");
				break;
			}
			m_effects[i, 0] = m_effectTemplates[i];
			for (int j = 1; j < m_effectListSize; j++)
			{
				m_effects[i, j] = Object.Instantiate(m_effectTemplates[i]) as ParticleSystemController;
				m_effects[i, j].transform.parent = base.transform;
				m_effects[i, j].Stop();
				m_effects[i, j].gameObject.SetActiveRecursively(false);
			}
			m_effectTemplates[i].gameObject.SetActiveRecursively(false);
		}
	}

	public void StopAllFX()
	{
		ParticleSystemController[,] effects = m_effects;
		int length = effects.GetLength(0);
		int length2 = effects.GetLength(1);
		for (int i = 0; i < length; i++)
		{
			for (int j = 0; j < length2; j++)
			{
				ParticleSystemController particleSystemController = effects[i, j];
				particleSystemController.Stop();
				particleSystemController.gameObject.SetActiveRecursively(false);
			}
		}
	}

	public void EmitSlash(FxType p_type, Vector3 pos, Vector3 dir)
	{
		if (!base.enabled)
		{
			return;
		}
		for (int i = 0; i < m_effectListSize; i++)
		{
			if (!m_effects[(int)p_type, i].gameObject.active)
			{
				dir.Normalize();
				m_effects[(int)p_type, i].transform.position = pos;
				m_effects[(int)p_type, i].transform.up = -dir;
				m_effects[(int)p_type, i].gameObject.SetActiveRecursively(true);
				m_effects[(int)p_type, i].Play();
				return;
			}
		}
		Dbg.LogWarn("There's no avialable slash fx. FX Type: " + p_type);
	}

	private void Update()
	{
		ParticleSystemController[,] effects = m_effects;
		int length = effects.GetLength(0);
		int length2 = effects.GetLength(1);
		for (int i = 0; i < length; i++)
		{
			for (int j = 0; j < length2; j++)
			{
				ParticleSystemController particleSystemController = effects[i, j];
				if (particleSystemController.gameObject.active && !particleSystemController.IsAlive())
				{
					particleSystemController.gameObject.SetActiveRecursively(false);
				}
			}
		}
	}
}
