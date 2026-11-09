using UnityEngine;

public class FlameFx : MonoBehaviour
{
	public Color m_color = Color.white;

	protected ParticleSystem[] m_flameParticles;

	public Renderer[] targetRenderers { private get; set; }

	private void Awake()
	{
		targetRenderers = null;
		m_flameParticles = MBHier.GetComponentsInChildren<ParticleSystem>(base.gameObject, true);
		for (int i = 0; i < m_flameParticles.Length; i++)
		{
			m_flameParticles[i].Stop();
			m_flameParticles[i].playOnAwake = false;
		}
		base.enabled = false;
		base.gameObject.SetActiveRecursively(false);
	}

	public virtual void Burn()
	{
		base.gameObject.SetActiveRecursively(true);
		for (int i = 0; i < m_flameParticles.Length; i++)
		{
			m_flameParticles[i].Play();
		}
		if (targetRenderers != null)
		{
			for (int j = 0; j < targetRenderers.Length; j++)
			{
				Material material = targetRenderers[j].material;
				if (material.HasProperty("_Color"))
				{
					material.SetColor("_Color", m_color);
				}
			}
		}
		base.enabled = true;
	}

	public virtual void OnBurnOut()
	{
		for (int i = 0; i < m_flameParticles.Length; i++)
		{
			m_flameParticles[i].Stop();
		}
		if (targetRenderers != null)
		{
			for (int j = 0; j < targetRenderers.Length; j++)
			{
				Material material = targetRenderers[j].material;
				if (material.HasProperty("_Color"))
				{
					material.SetColor("_Color", Color.white);
				}
			}
		}
		base.enabled = false;
	}

	public static T FindOrCreateFlameFx<T>(GameObject p_root, GameObject p_prefab) where T : FlameFx
	{
		T val = MBHier.GetComponentInChildren<T>(p_root);
		if (null == val)
		{
			GameObject gameObject = Object.Instantiate(p_prefab) as GameObject;
			val = gameObject.GetComponent<T>();
			if (null == val)
			{
				Object.Destroy(gameObject);
			}
		}
		return val;
	}
}
