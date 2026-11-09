using System.Collections.Generic;
using UnityEngine;

public class PetrifyEffect : MonoBehaviour
{
	protected static readonly float blendFactor = 0.333333f;

	protected static readonly float delayTime = 0.5f;

	public Renderer[] m_renderers;

	protected ModelFader[] m_faders;

	protected Shader[] m_defaultShaders;

	protected bool[] m_fading;

	private bool m_cutscenePlaying;

	private NBTimer m_delayTimer;

	private List<GameObject> m_instantiatedEffects = new List<GameObject>();

	[SerializeField]
	protected GameObject m_fxShatteredPrefab;

	[SerializeField]
	protected GameObject m_fxShakingPrefab;

	[SerializeField]
	protected Shader m_grayShader;

	[SerializeField]
	protected Texture2D m_texRockOverlay;

	public Renderer[] targetRenderers
	{
		private get
		{
			return m_renderers;
		}
		set
		{
			m_renderers = value;
			m_defaultShaders = new Shader[m_renderers.Length];
			m_faders = new ModelFader[m_renderers.Length];
			for (int i = 0; i < m_renderers.Length; i++)
			{
				m_defaultShaders[i] = m_renderers[i].material.shader;
				m_faders[i] = m_renderers[i].gameObject.GetComponent<ModelFader>();
			}
			m_fading = new bool[m_renderers.Length];
			for (int j = 0; j < m_renderers.Length; j++)
			{
				m_fading[j] = false;
			}
		}
	}

	public Transform shakeParticleRoot { get; set; }

	protected void InstatiateFx(GameObject p_prefab)
	{
		if (!m_cutscenePlaying && p_prefab != null)
		{
			GameObject gameObject = Object.Instantiate(p_prefab) as GameObject;
			Vector3 position = gameObject.transform.position;
			gameObject.transform.parent = shakeParticleRoot;
			gameObject.transform.localPosition = position;
			gameObject.transform.localRotation = Quaternion.identity;
			m_instantiatedEffects.Add(gameObject);
			base.enabled = true;
		}
	}

	protected void Awake()
	{
		m_delayTimer = base.gameObject.AddComponent<NBTimer>();
		m_delayTimer.Name = "Petrify Fx Delay Timer";
		Messenger<CutsceneController.CutsceneType>.AddListener("cutscene_start", OnCutsceneStart);
		Messenger<CutsceneController.CutsceneType>.AddListener("cutscene_end", OnCutsceneEnd);
		base.enabled = false;
	}

	protected void OnDestroy()
	{
		Messenger<CutsceneController.CutsceneType>.RemoveListener("cutscene_start", OnCutsceneStart);
		Messenger<CutsceneController.CutsceneType>.RemoveListener("cutscene_end", OnCutsceneEnd);
	}

	public virtual void OnPetrified()
	{
		if (targetRenderers != null && targetRenderers.Length > 0)
		{
			for (int i = 0; i < targetRenderers.Length; i++)
			{
				for (int j = 0; j < targetRenderers[i].materials.Length; j++)
				{
					Material material = targetRenderers[i].materials[j];
					material.shader = m_grayShader;
					material.SetTexture("_Texture2", m_texRockOverlay);
					material.SetFloat("_Blend", blendFactor);
				}
			}
			for (int k = 0; k < m_faders.Length; k++)
			{
				m_fading[k] = m_faders[k].enabled;
				m_faders[k].enabled = false;
			}
		}
		base.enabled = true;
	}

	public void OnShake()
	{
		if (m_delayTimer.Counter == 0f)
		{
			InstatiateFx(m_fxShakingPrefab);
			m_delayTimer.CountDown(delayTime);
		}
	}

	private void OnCutsceneStart(CutsceneController.CutsceneType p_cutscene)
	{
		m_cutscenePlaying = true;
		for (int num = m_instantiatedEffects.Count - 1; num >= 0; num--)
		{
			if (null != m_instantiatedEffects[num])
			{
				Object.Destroy(m_instantiatedEffects[num]);
			}
			m_instantiatedEffects.RemoveAt(num);
		}
	}

	private void OnCutsceneEnd(CutsceneController.CutsceneType p_cutscene)
	{
		m_cutscenePlaying = false;
	}

	public void OnBreak()
	{
		for (int i = 0; i < m_faders.Length; i++)
		{
			m_faders[i].enabled = m_fading[i];
		}
		ResetShader();
		InstatiateFx(m_fxShatteredPrefab);
		base.enabled = false;
	}

	public void OnShattered()
	{
		OnBreak();
		for (int i = 0; i < m_faders.Length; i++)
		{
			if (null != m_faders[i])
			{
				m_faders[i].Fade(0.5f, 0f, 1.5f, null);
			}
		}
	}

	private void ResetShader()
	{
		if (targetRenderers == null || targetRenderers.Length <= 0)
		{
			return;
		}
		for (int i = 0; i < targetRenderers.Length; i++)
		{
			for (int j = 0; j < targetRenderers[i].materials.Length; j++)
			{
				Material material = targetRenderers[i].materials[j];
				material.shader = m_defaultShaders[i];
			}
		}
	}

	private void Update()
	{
		for (int num = m_instantiatedEffects.Count - 1; num >= 0; num--)
		{
			if (null == m_instantiatedEffects[num])
			{
				m_instantiatedEffects.RemoveAt(num);
			}
		}
	}
}
