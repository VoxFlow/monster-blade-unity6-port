using System;
using System.Collections;
using UnityEngine;

public class HydraSpecialFx : MonsterSpecialFx
{
	private enum Part
	{
		HEAD_LEFT = 0,
		HEAD_RIGHT = 1,
		HEAD_MIDDLE = 2,
		BODY = 3
	}

	public Material m_wispMaterial;

	public GameObject m_fxSpiralPrefab;

	public Shader m_headWispFadeShader;

	public Shader m_headDefaultShader;

	public Shader m_headFadeShader;

	private Material[] m_defaultMaterials = new Material[3];

	private Transform[] m_headMeshTransforms = new Transform[3];

	private Transform[] m_headBoneTransforms = new Transform[3];

	private TrailRenderer[] m_headTrails = new TrailRenderer[3];

	private SkinnedMeshRenderer[] m_partRenderers = new SkinnedMeshRenderer[4];

	private Color[] m_headColors = new Color[3]
	{
		Color.white,
		Color.white,
		Color.white
	};

	private HydraHead[] m_heads = new HydraHead[3];

	private ModelFader[] m_headFaders = new ModelFader[3];

	private bool m_coroutineRunning;

	protected override void Awake()
	{
		base.Awake();
		Transform transform = base.transform.FindInHierarchy("Mob05_LHead");
		Transform transform2 = base.transform.FindInHierarchy("Mob05_RHead");
		Transform transform3 = base.transform.FindInHierarchy("Mob05_MHead");
		Transform transform4 = base.transform.FindInHierarchy("Mob05_Body");
		m_headMeshTransforms[0] = transform;
		m_headMeshTransforms[1] = transform2;
		m_headMeshTransforms[2] = transform3;
		m_headBoneTransforms[0] = base.transform.FindInHierarchy("Mob05 Neck Left");
		m_headBoneTransforms[1] = base.transform.FindInHierarchy("Mob05 Neck Right");
		m_headBoneTransforms[2] = base.transform.FindInHierarchy("Mob05 Neck1");
		m_partRenderers[0] = transform.GetComponent<SkinnedMeshRenderer>();
		m_partRenderers[1] = transform2.GetComponent<SkinnedMeshRenderer>();
		m_partRenderers[2] = transform3.GetComponent<SkinnedMeshRenderer>();
		m_partRenderers[3] = transform4.GetComponent<SkinnedMeshRenderer>();
		m_heads[0] = transform.GetComponent<HydraHead>();
		m_heads[1] = transform2.GetComponent<HydraHead>();
		m_heads[2] = transform3.GetComponent<HydraHead>();
		for (int i = 0; i < 3; i++)
		{
			m_headTrails[i] = MBHier.GetComponentInChildren<TrailRenderer>(m_headBoneTransforms[i]);
		}
		Messenger<HydraHead.HeadEnum>.AddListener("hydra_head_hit", OnHeadHit);
		Messenger<HydraHead.HeadEnum>.AddListener("hydra_head_cut_off", OnHeadCuttOff);
		Messenger<HydraHead.HeadEnum>.AddListener("hydra_head_recover", OnHeadRecovered);
	}

	protected override void Start()
	{
		base.Start();
		for (int i = 0; i < 3; i++)
		{
			if (m_headMeshTransforms[i] != null)
			{
				m_headFaders[i] = m_headMeshTransforms[i].GetComponent<ModelFader>();
				if (m_headFaders[i] != null)
				{
					m_headFaders[i].fadeShader = m_headWispFadeShader;
					m_headFaders[i].colorPropertyName = "_TintColor";
				}
			}
			if (m_partRenderers[i] != null)
			{
				m_defaultMaterials[i] = m_partRenderers[i].material;
			}
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Messenger<HydraHead.HeadEnum>.RemoveListener("hydra_head_hit", OnHeadHit);
		Messenger<HydraHead.HeadEnum>.RemoveListener("hydra_head_cut_off", OnHeadCuttOff);
		Messenger<HydraHead.HeadEnum>.RemoveListener("hydra_head_recover", OnHeadRecovered);
	}

	public override void EnableTrailRenderer(string p_name)
	{
		int num = Array.IndexOf(m_headTrails, p_name);
		if (num >= 0 && !m_heads[num].IsCutOff)
		{
			base.EnableTrailRenderer(p_name);
		}
	}

	private void OnHeadCuttOff(HydraHead.HeadEnum p_head)
	{
		SkinnedMeshRenderer skinnedMeshRenderer = m_partRenderers[(int)p_head];
		skinnedMeshRenderer.material = m_wispMaterial;
		skinnedMeshRenderer.enabled = false;
		skinnedMeshRenderer.enabled = true;
		AnimatedUVs component = m_headMeshTransforms[(int)p_head].GetComponent<AnimatedUVs>();
		component.enabled = true;
		GameObject gameObject = UnityEngine.Object.Instantiate(m_fxSpiralPrefab) as GameObject;
		gameObject.transform.parent = m_headBoneTransforms[(int)p_head];
		gameObject.transform.localPosition = new Vector3(-2.1f, 0.7f, 0.8f);
		gameObject.transform.localRotation = Quaternion.Euler(347f, 278f, 350f);
		m_headFaders[(int)p_head].Fade(1f, 0f, 0.5f, OnFadeOutDone);
		m_headTrails[(int)p_head].enabled = false;
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_mob5_head_death");
	}

	private void OnHeadRecovered(HydraHead.HeadEnum p_head)
	{
		Transform transform = m_headMeshTransforms[(int)p_head];
		ModelFader component = transform.GetComponent<ModelFader>();
		AnimatedUVs component2 = transform.GetComponent<AnimatedUVs>();
		component2.uvOffset = Vector3.zero;
		component2.enabled = true;
		component.Fade(0f, 1f, 0.5f, OnFadeInDone);
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_mob5_head_recovery");
	}

	private void OnFadeOutDone(Renderer p_renderer)
	{
		AnimatedUVs component = p_renderer.GetComponent<AnimatedUVs>();
		component.uvOffset = Vector3.zero;
		component.enabled = false;
	}

	private void OnFadeInDone(Renderer p_renderer)
	{
		AnimatedUVs component = p_renderer.GetComponent<AnimatedUVs>();
		component.uvOffset = Vector3.zero;
		component.enabled = false;
		HydraHead.HeadEnum head = p_renderer.GetComponent<HydraHead>().m_head;
		p_renderer.material = m_defaultMaterials[(int)head];
		p_renderer.material.color = Color.white;
		p_renderer.enabled = false;
		p_renderer.enabled = true;
		p_renderer.material.shader = m_headDefaultShader;
		Color color = m_partRenderers[3].material.GetColor("_Color");
		if (p_renderer.material.HasProperty("_Color"))
		{
			p_renderer.material.SetColor("_Color", color);
		}
		if (p_renderer.material.HasProperty("_FlashColor"))
		{
			p_renderer.material.SetColor("_FlashColor", Color.black);
		}
	}

	protected override void OnCleanHit()
	{
		Color white = Color.white;
		Color white2 = Color.white;
		white.a = 0f;
		white2.a = 0.3f;
		SingletonComponent<BattleScreenUI>.Instance.FlashScreen(white, white2, 0.25f);
		DisableAllTrailRenderers();
		Hydra hydra = (Hydra)m_monster;
		if (hydra.IsAllHeadCutOff())
		{
			FlashPart(m_partRenderers[3], m_cleanHitColor);
		}
		if (SingletonComponent<SoundManager>.Instance != null)
		{
			SingletonComponent<SoundManager>.Instance.PlayPlaylist(m_woundSounds);
		}
	}

	protected override void OnExitAttack()
	{
		base.OnExitAttack();
		DisableParticle("FX_WaterBreath");
	}

	protected override void OnCutsceneStart(CutsceneController.CutsceneType p_cutscene)
	{
		base.OnCutsceneStart(p_cutscene);
		if (p_cutscene == CutsceneController.CutsceneType.DEATH_HYDRA)
		{
			for (int i = 0; i < 3; i++)
			{
				m_headFaders[i].fadeShader = m_headFadeShader;
				m_headFaders[i].colorPropertyName = "_Color";
			}
		}
	}

	protected override void OnDead()
	{
		base.OnDead();
		for (int i = 0; i < 3; i++)
		{
			if (!m_heads[i].IsCutOff)
			{
				m_headFaders[i].fadeShader = m_headFadeShader;
				m_headFaders[i].colorPropertyName = "_Color";
				m_headFaders[i].Fade(1f, 0f, m_monster.m_fadeSpeed, null);
			}
		}
		StopCoroutine("PlayExplosionEffect");
	}

	private void OnHeadHit(HydraHead.HeadEnum p_head)
	{
		FlashPart(m_partRenderers[(int)p_head], m_cleanHitColor);
	}

	private void FlashPart(SkinnedMeshRenderer p_renderer, Color p_color)
	{
		if (p_renderer.material.HasProperty("_FlashColor"))
		{
			int num = Array.IndexOf(m_partRenderers, p_renderer);
			p_renderer.material.SetColor("_FlashColor", p_color);
			m_flashTimers[num].CountDown(m_flashTime, base.OnFlashEnded);
		}
	}

	public IEnumerator PlayExplosionEffect()
	{
		if (m_coroutineRunning)
		{
			yield break;
		}
		m_coroutineRunning = true;
		if (!m_monster.IsDead())
		{
			EnableParticle("FX_ImplodeExplode");
			Waiter waiter = new Waiter();
			yield return StartCoroutine(waiter.WaitUntil(5f));
			if (!m_monster.IsDead())
			{
				DisableParticle("FX_ImplodeExplode");
				m_coroutineRunning = false;
			}
		}
	}

	protected void LateUpdate()
	{
		if (m_monster.IsDead())
		{
			return;
		}
		float num = 0f;
		bool flag = m_monster.CurrentState == Monster.State.Fall;
		for (int i = 0; i < 3; i++)
		{
			if (null == m_heads[i] || null == m_partRenderers[i])
			{
				continue;
			}
			if (!m_heads[i].IsCutOff && !flag && m_defaultMaterials[i] == m_partRenderers[i].material)
			{
				num = m_heads[i].HpPercent;
				m_headColors[i].r = base.mainColor.r;
				m_headColors[i].g = base.mainColor.g * num;
				m_headColors[i].b = base.mainColor.b * num;
				m_partRenderers[i].material.SetColor("_Color", m_headColors[i]);
			}
			else if (m_heads[i].IsCutOff)
			{
				if (m_partRenderers[i].material.shader != m_headWispFadeShader)
				{
					m_partRenderers[i].material.shader = m_headWispFadeShader;
				}
				// PORT (CD-014)
				Material headMat = m_partRenderers[i].material;
				Color color = headMat.HasProperty(m_headFaders[i].colorPropertyName) ? headMat.GetColor(m_headFaders[i].colorPropertyName) : new Color(0f, 0f, 0f, 0f);
				if (!m_headFaders[i].enabled && color.a != 0f)
				{
					color.a = 0f;
					m_partRenderers[i].material.SetColor(m_headFaders[i].colorPropertyName, color);
				}
			}
		}
	}
}
