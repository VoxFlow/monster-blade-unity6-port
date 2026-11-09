using UnityEngine;

[ExecuteInEditMode]
public class CutsceneCharacterFader : MonoBehaviour
{
	public float m_alpha = 1f;

	public Shader m_shader;

	private SkinnedMeshRenderer[] m_renderers;

	private void Start()
	{
		m_renderers = MBHier.GetComponentsInChildren<SkinnedMeshRenderer>(base.gameObject);
		if (m_renderers == null)
		{
			Dbg.LogWarnEx("CutsceneCharacterFader.Start(): There's no renderable game-object inside this hierarchy - {0}", Dbg.NBLogType.COMMON_WARNING, base.gameObject.name);
			base.enabled = false;
			return;
		}
		if (m_shader == null)
		{
			Dbg.LogWarn("CutsceneCharacterFader.Start(): No shader has been given.");
			base.enabled = false;
		}
		for (int i = 0; i < m_renderers.Length; i++)
		{
			if (m_renderers[i] == null)
			{
				Dbg.LogWarn("CutsceneCharacterFader.Start(): An instance of SkinnedMeshRenderer component has been destroyed.");
				continue;
			}
			for (int j = 0; j < m_renderers[i].sharedMaterials.Length; j++)
			{
				if (null == m_renderers[i].sharedMaterials[j])
				{
					Dbg.LogWarn("CutsceneCharacterFader.Start(): Missing material.");
				}
				else
				{
					m_renderers[i].sharedMaterials[j].shader = m_shader;
				}
			}
		}
	}

	private void Update()
	{
		if (m_renderers == null)
		{
			Dbg.LogWarn("CutsceneCharacterFader.Update(): Instances of SkinnedMeshRenderer component have been destroyed.");
			base.enabled = false;
			return;
		}
		for (int i = 0; i < m_renderers.Length; i++)
		{
			if (m_renderers[i] == null)
			{
				Dbg.LogWarn("CutsceneCharacterFader.Update(): An instance of SkinnedMeshRenderer component has been destroyed.");
				continue;
			}
			for (int j = 0; j < m_renderers[i].sharedMaterials.Length; j++)
			{
				if (null == m_renderers[i].sharedMaterials[j])
				{
					Dbg.LogWarn("CutsceneCharacterFader.Update(): Missing material.");
					continue;
				}
				Color color = m_renderers[i].sharedMaterials[j].color;
				color.a = m_alpha;
				m_renderers[i].sharedMaterials[j].color = color;
			}
		}
	}
}
