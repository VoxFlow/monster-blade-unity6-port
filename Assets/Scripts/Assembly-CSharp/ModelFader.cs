using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class ModelFader : MonoBehaviour
{
	public delegate void FadeCallback(Renderer p_renderer);

	public Shader fadeShader;

	private float m_fadeTo;

	private float m_fadeSpeed = 0.5f;

	private bool m_incremental = true;

	private FadeCallback m_endCallback;

	public float currentAlpha
	{
		get
		{
			float result = 0f;
			if (base.GetComponent<Renderer>() != null)
			{
				result = ColorOf(base.GetComponent<Renderer>().material, "_Color").a;
			}
			return result;
		}
	}

	public string colorPropertyName { get; set; }

	// PORT (CD-014)
	private static Color ColorOf(Material m, string property)
	{
		return m.HasProperty(property) ? m.GetColor(property) : new Color(0f, 0f, 0f, 0f);
	}

	private void Awake()
	{
		base.enabled = false;
		colorPropertyName = "_Color";
	}

	public void FadeFromCurrentAlpha(float p_to, float p_speed, FadeCallback p_endCallback)
	{
		float p_from = 1f;
		if (base.GetComponent<Renderer>() != null)
		{
			p_from = ColorOf(base.GetComponent<Renderer>().material, "_Color").a;
		}
		Fade(p_from, p_to, p_speed, p_endCallback);
	}

	public void Fade(float p_from, float p_to, float p_speed, FadeCallback p_endCallback)
	{
		Material[] materials = base.GetComponent<Renderer>().materials;
		foreach (Material material in materials)
		{
			material.shader = fadeShader;
			if (SingletonComponent<LevelSetting>.Instance != null && material.HasProperty("_AmbientColor"))
			{
				material.SetColor("_AmbientColor", SingletonComponent<LevelSetting>.Instance.ambientLight);
			}
			if (material.shader != null)
			{
				Color color = material.GetColor(colorPropertyName);
				color.a = p_from;
				material.SetColor(colorPropertyName, color);
				m_fadeTo = p_to;
				m_fadeSpeed = p_speed;
				m_incremental = p_to >= p_from;
			}
		}
		base.enabled = true;
		m_endCallback = p_endCallback;
	}

	public bool IsFading()
	{
		if (base.GetComponent<Renderer>() != null)
		{
			return ColorOf(base.GetComponent<Renderer>().material, colorPropertyName).a != m_fadeTo;
		}
		return false;
	}

	private void Update()
	{
		if (base.GetComponent<Renderer>() == null)
		{
			base.enabled = false;
			return;
		}
		for (int i = 0; i < base.GetComponent<Renderer>().materials.Length; i++)
		{
			if (base.GetComponent<Renderer>().materials[i].shader != fadeShader)
			{
				base.GetComponent<Renderer>().materials[i].shader = fadeShader;
			}
		}
		float num = m_fadeSpeed * Time.deltaTime;
		Color color = new Color(0f, 0f, 0f);
		color = base.GetComponent<Renderer>().material.GetColor(colorPropertyName);
		if (m_incremental)
		{
			color.a += num;
			if (color.a >= m_fadeTo)
			{
				color.a = m_fadeTo;
				base.enabled = false;
			}
		}
		else
		{
			color.a -= num;
			if (color.a <= m_fadeTo)
			{
				color.a = m_fadeTo;
				base.enabled = false;
			}
		}
		Material[] materials = base.GetComponent<Renderer>().materials;
		foreach (Material material in materials)
		{
			material.SetColor(colorPropertyName, color);
		}
		if (!base.enabled && m_endCallback != null)
		{
			m_endCallback(base.GetComponent<Renderer>());
		}
	}
}
