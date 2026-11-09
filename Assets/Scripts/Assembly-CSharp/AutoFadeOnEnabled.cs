using UnityEngine;

[RequireComponent(typeof(ModelFader))]
public class AutoFadeOnEnabled : MonoBehaviour
{
	private ModelFader m_fader;

	private Shader[] m_defaultShaders;

	public float m_fadeTime = 2f;

	private void Awake()
	{
		m_fader = base.gameObject.GetComponent<ModelFader>();
		m_defaultShaders = new Shader[base.GetComponent<Renderer>().materials.Length];
		for (int i = 0; i < m_defaultShaders.Length; i++)
		{
			m_defaultShaders[i] = base.GetComponent<Renderer>().materials[i].shader;
		}
	}

	private void OnEnable()
	{
		m_fader.Fade(0f, 1f, m_fadeTime, OnFadeInEnd);
	}

	private void OnFadeInEnd(Renderer p_renderer)
	{
		for (int i = 0; i < m_defaultShaders.Length; i++)
		{
			base.GetComponent<Renderer>().materials[i].shader = m_defaultShaders[i];
		}
	}
}
