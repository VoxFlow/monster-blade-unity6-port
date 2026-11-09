using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MonsterShakeScript))]
public class MonsterSpecialFx : MonoBehaviour
{
	protected Monster m_monster;

	protected SkinnedMeshRenderer[] m_renderers;

	protected NBTimer[] m_flashTimers;

	public Color m_cleanHitColor = new Color32(150, 150, 150, byte.MaxValue);

	public Color m_grazeHitColor = new Color32(25, 25, 25, byte.MaxValue);

	public Color m_guardHitColor = new Color32(160, 144, 32, byte.MaxValue);

	public float m_flashTime = 0.085f;

	protected MonsterShakeScript m_shakeScript;

	protected BattleCamera m_battleCamera;

	protected GameObject m_cameraObject;

	public Vector3 m_grazeShakeStrength = new Vector3(0f, 0.05f, 0f);

	public CameraShakeData[] m_shakeData;

	public ParticleSystemController[] m_particleSystems;

	public TrailRenderer[] m_trailRenderers;

	public string m_attackSounds = string.Empty;

	public string m_woundSounds = string.Empty;

	protected AudioSource m_playingSfx;

	private int m_NormalHitFXCount = 2;

	private GameObject[] m_NormalHitFX;

	private Transform m_HitFXTransform;

	private float m_slomoTime = 0.65f;

	private float m_slomoScale = 1f;

	private Color m_mainColor = Color.white;

	private bool needUpdate = true;

	public Color mainColor
	{
		get
		{
			return m_mainColor;
		}
		set
		{
			if (m_renderers != null)
			{
				for (int i = 0; i < m_renderers.Length; i++)
				{
					Material material = m_renderers[i].material;
					if (material.HasProperty("_Color"))
					{
						material.SetColor("_Color", value);
					}
				}
			}
			m_mainColor = value;
		}
	}

	protected virtual void Awake()
	{
		mainColor = Color.white;
		m_monster = base.gameObject.GetComponent<Monster>();
		m_renderers = MBHier.GetComponentsInChildren<SkinnedMeshRenderer>(base.gameObject);
		m_cameraObject = GameObject.Find("Main Camera");
		m_shakeScript = base.gameObject.GetComponent<MonsterShakeScript>();
		m_shakeData = base.gameObject.GetComponents<CameraShakeData>();
		CameraShakeData[] shakeData = m_shakeData;
		foreach (CameraShakeData cameraShakeData in shakeData)
		{
			Dbg.Log(cameraShakeData.name);
		}
		m_particleSystems = MBHier.GetComponentsInChildren<ParticleSystemController>(base.gameObject);
		ParticleSystemController[] particleSystems = m_particleSystems;
		foreach (ParticleSystemController particleSystemController in particleSystems)
		{
			Dbg.Log("Particle System: " + particleSystemController.name);
			particleSystemController.Stop();
		}
		m_trailRenderers = MBHier.GetComponentsInChildren<TrailRenderer>(base.gameObject);
		TrailRenderer[] trailRenderers = m_trailRenderers;
		foreach (TrailRenderer trailRenderer in trailRenderers)
		{
			Dbg.Log(trailRenderer.name);
		}
		GameObject gameObject = GameObject.Find("Camera_Root");
		if (gameObject != null)
		{
			m_battleCamera = gameObject.GetComponent<BattleCamera>();
			CameraRecoilFx component = m_cameraObject.GetComponent<CameraRecoilFx>();
			component.Amplitude = 10f;
			component.Period = 0.25f;
		}
		m_flashTimers = new NBTimer[m_renderers.Length];
		for (int l = 0; l < m_renderers.Length; l++)
		{
			m_flashTimers[l] = base.gameObject.AddComponent<NBTimer>();
		}
		m_NormalHitFX = new GameObject[m_NormalHitFXCount];
		for (int m = 0; m < m_NormalHitFXCount; m++)
		{
			string path = "FX/Prefabs/FX_WeaponImpact_Med" + (m + 1);
			GameObject original = Resources.Load(path) as GameObject;
			m_NormalHitFX[m] = Object.Instantiate(original) as GameObject;
			m_NormalHitFX[m].SetActiveRecursively(false);
		}
		DisableAllParticles(false);
		DisableAllTrailRenderers();
		Messenger<DamageData>.AddListener("monster_receive_dmg", OnHit);
		Messenger<CutsceneController.CutsceneType>.AddListener(CutsceneMessage.CUTSCENE_START, OnCutsceneStart);
		Messenger.AddListener("monster_end_atk", DisableAllEffects);
	}

	protected virtual void OnDestroy()
	{
		for (int i = 0; i < m_NormalHitFXCount; i++)
		{
			Object.Destroy(m_NormalHitFX[i]);
		}
		Messenger<DamageData>.RemoveListener("monster_receive_dmg", OnHit);
		Messenger<CutsceneController.CutsceneType>.RemoveListener(CutsceneMessage.CUTSCENE_START, OnCutsceneStart);
		Messenger.RemoveListener("monster_end_atk", DisableAllEffects);
	}

	protected virtual void Start()
	{
		GameObject gameObject = GameObject.Find("EnemyDamagePoint");
		if ((bool)gameObject)
		{
			m_HitFXTransform = gameObject.transform;
		}
		if (SingletonComponent<LevelSetting>.Instance != null)
		{
			SkinnedMeshRenderer[] renderers = m_renderers;
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in renderers)
			{
				skinnedMeshRenderer.material.SetColor("_AmbientColor", SingletonComponent<LevelSetting>.Instance.ambientLight);
			}
		}
	}

	protected virtual void OnAttackBegin(MonsterAttack p_attack)
	{
		if (SingletonComponent<SoundManager>.Instance != null)
		{
			SingletonComponent<SoundManager>.Instance.PlayPlaylist(m_attackSounds);
		}
	}

	protected virtual void OnCutsceneStart(CutsceneController.CutsceneType p_cutscene)
	{
		Dbg.Log("MonsterSpecialFX.OnCutsceneStart");
		DisableAllParticles();
		Dbg.Log("MonsterSpecialFX.OnCutsceneStart:DisableAllTrailRenderers");
		DisableAllTrailRenderers();
	}

	protected virtual void OnDead()
	{
		DisableAllTrailRenderers();
	}

	protected virtual void OnHit(DamageData p_damage)
	{
		switch (p_damage.damageType)
		{
		case DamageType.Critical:
			OnCriticalHit();
			break;
		case DamageType.Block:
			OnGuardHit();
			break;
		default:
			OnCleanHit();
			break;
		}
		PlayRandomHitFX();
		m_shakeScript.m_amplitude = 0.025f;
		m_shakeScript.m_duration = 0.2f;
		m_shakeScript.m_frequency = 4f;
		m_shakeScript.Shake();
	}

	public void PlayRandomHitFX()
	{
		if (!(null == m_HitFXTransform))
		{
			int num = Random.Range(0, 2);
			if (!(null == m_NormalHitFX[num]))
			{
				m_NormalHitFX[num].transform.localPosition = Vector3.zero;
				m_NormalHitFX[num].transform.localEulerAngles = Vector3.zero;
				m_NormalHitFX[num].transform.position = m_HitFXTransform.position;
				m_NormalHitFX[num].transform.localEulerAngles = m_HitFXTransform.eulerAngles;
				m_NormalHitFX[num].GetComponent<ParticleSystemController>().Play();
			}
		}
	}

	protected virtual void OnCriticalHit()
	{
		OnCleanHit();
		if (null != m_monster && m_monster.CurrentState == Monster.State.Hit)
		{
			SingletonComponent<SlowMotionSystem>.Instance.SlowTheTime(m_slomoScale, m_slomoTime);
		}
		if (SingletonComponent<SaveData>.Instance.GameData.NeedVibrate)
		{
#if UNITY_ANDROID || UNITY_IOS
			Handheld.Vibrate();
#endif
		}
	}

	protected virtual void OnCleanHit()
	{
		for (int i = 0; i < m_renderers.Length; i++)
		{
			m_renderers[i].material.SetColor("_FlashColor", m_cleanHitColor);
			m_flashTimers[i].CountDown(m_flashTime, OnFlashEnded);
		}
		DisableAllTrailRenderers();
		if (SingletonComponent<SoundManager>.Instance != null)
		{
			SingletonComponent<SoundManager>.Instance.PlayPlaylist(m_woundSounds);
		}
	}

	protected virtual void OnGuardHit()
	{
		for (int i = 0; i < m_renderers.Length; i++)
		{
			m_renderers[i].material.SetColor("_FlashColor", m_guardHitColor);
			m_flashTimers[i].CountDown(m_flashTime, OnFlashEnded);
		}
	}

	protected virtual void OnIdle()
	{
		DisableAllEffects();
	}

	protected virtual void OnRecover()
	{
		DisableAllEffects();
	}

	protected virtual void OnExitAttack()
	{
		DisableAllEffects();
	}

	protected virtual void OnWait()
	{
		DisableAllEffects();
	}

	protected virtual void DisableAllEffects()
	{
		DisableAllTrailRenderers();
		DisableAllParticles();
	}

	protected void OnFlashEnded()
	{
		for (int i = 0; i < m_renderers.Length; i++)
		{
			if (m_renderers[i].material.HasProperty("_FlashColor") && m_flashTimers[i].Counter == 0f)
			{
				m_renderers[i].material.SetColor("_FlashColor", Color.black);
			}
		}
	}

	public virtual void PlaySoundFx(string p_sfx)
	{
		if (SingletonComponent<SoundManager>.Instance != null)
		{
			SingletonComponent<SoundManager>.Instance.PlayPlaylist(p_sfx);
		}
	}

	public void ShakeCamera(string p_shakeDataName)
	{
		CameraShakeData cameraShakeData = null;
		CameraShakeData[] shakeData = m_shakeData;
		foreach (CameraShakeData cameraShakeData2 in shakeData)
		{
			if (cameraShakeData2.referenceName == p_shakeDataName)
			{
				cameraShakeData = cameraShakeData2;
				break;
			}
		}
		if (cameraShakeData == null)
		{
			Debug.LogWarning("Invalid shake data: " + p_shakeDataName);
			return;
		}
		if (m_battleCamera != null)
		{
			m_battleCamera.ShakeCamera(cameraShakeData);
		}
		else
		{
			CameraShakeFx cameraShakeFx = null;
			if ((bool)Camera.main)
			{
				cameraShakeFx = Camera.main.GetComponent<CameraShakeFx>();
			}
			if (cameraShakeFx == null)
			{
				return;
			}
			cameraShakeFx.Amplitude = cameraShakeData.amplitude;
			cameraShakeFx.Frequency = cameraShakeData.frequency;
			cameraShakeFx.Duration = cameraShakeData.duration;
			cameraShakeFx.SmoothFade = cameraShakeData.smoothFade;
			cameraShakeFx.Play();
		}
		if (SingletonComponent<SaveData>.Instance.GameData.NeedVibrate)
		{
#if UNITY_ANDROID || UNITY_IOS
			Handheld.Vibrate();
#endif
		}
	}

	public void EnableParticle(string p_name)
	{
		Dbg.Log("Playing particle effect: " + p_name);
		ParticleSystemController[] particles = GetParticles(p_name);
		ParticleSystemController[] array = particles;
		foreach (ParticleSystemController particleSystemController in array)
		{
			if (particleSystemController.IsAlive())
			{
				particleSystemController.Stop();
			}
			else
			{
				particleSystemController.gameObject.SetActiveRecursively(true);
			}
			particleSystemController.Play();
		}
	}

	public void DisableParticle(string p_name)
	{
		ParticleSystemController[] particles = GetParticles(p_name);
		ParticleSystemController[] array = particles;
		foreach (ParticleSystemController particleSystemController in array)
		{
			particleSystemController.Stop();
		}
	}

	public void DisableAllParticles(bool p_smooth = true)
	{
		ParticleSystemController[] particleSystems = m_particleSystems;
		foreach (ParticleSystemController particleSystemController in particleSystems)
		{
			particleSystemController.Stop(p_smooth);
		}
	}

	public virtual void EnableTrailRenderer(string p_name)
	{
		if (!(Application.loadedLevelName == "BattleView") || m_monster.CurrentState == Monster.State.Attack)
		{
			TrailRenderer trailRenderer = GetTrailRenderer(p_name);
			if (trailRenderer != null)
			{
				trailRenderer.enabled = true;
			}
		}
	}

	public virtual void DisableTrailRenderer(string p_name)
	{
		TrailRenderer trailRenderer = GetTrailRenderer(p_name);
		if (trailRenderer != null)
		{
			trailRenderer.enabled = false;
		}
	}

	public void DisableAllTrailRenderers()
	{
		Debug.Log("DisableAllTrailRenderers");
		TrailRenderer[] trailRenderers = m_trailRenderers;
		foreach (TrailRenderer trailRenderer in trailRenderers)
		{
			trailRenderer.enabled = false;
		}
	}

	protected ParticleSystemController[] GetParticles(string p_name)
	{
		List<ParticleSystemController> list = new List<ParticleSystemController>();
		if (m_particleSystems == null)
		{
			Debug.LogError("Particle system list has not been initialized.");
		}
		else
		{
			ParticleSystemController[] particleSystems = m_particleSystems;
			foreach (ParticleSystemController particleSystemController in particleSystems)
			{
				if (particleSystemController.name == p_name)
				{
					list.Add(particleSystemController);
				}
			}
			if (list.Count == 0)
			{
				Debug.LogWarning("Cannot find particle system with name: " + p_name);
			}
		}
		return list.ToArray();
	}

	protected TrailRenderer GetTrailRenderer(string p_name)
	{
		TrailRenderer trailRenderer = null;
		if (m_trailRenderers == null)
		{
			Debug.LogError("Trail-renderer list has not been initialized.");
		}
		else
		{
			TrailRenderer[] trailRenderers = m_trailRenderers;
			foreach (TrailRenderer trailRenderer2 in trailRenderers)
			{
				if (trailRenderer2.name == p_name)
				{
					trailRenderer = trailRenderer2;
					break;
				}
			}
			if (trailRenderer == null)
			{
				Debug.LogWarning("Cannot find trail-renderer with name: " + p_name);
			}
		}
		return trailRenderer;
	}

	protected virtual void Update()
	{
	}
}
