using UnityEngine;

public class FxAura : PlayerBuffFx
{
	public Shader m_tintShader;

	public Color m_auraTintColor = Color.white;

	private Shader m_defaultShader;

	protected override void OnEnable()
	{
		base.OnEnable();
		if (null == m_tintShader || null == base.player)
		{
			return;
		}
		m_defaultShader = base.player.GetComponent<Renderer>().material.shader;
		for (int i = 0; i < base.player.GetComponent<Renderer>().materials.Length; i++)
		{
			Material material = base.player.GetComponent<Renderer>().materials[i];
			material.shader = m_tintShader;
			if (material.HasProperty("_TintColor"))
			{
				material.SetColor("_TintColor", m_auraTintColor);
			}
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (!(null == m_tintShader) && !(null == base.player))
		{
			for (int i = 0; i < base.player.GetComponent<Renderer>().materials.Length; i++)
			{
				Material material = base.player.GetComponent<Renderer>().materials[i];
				material.shader = m_defaultShader;
			}
		}
	}
}
