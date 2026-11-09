using System;
using UnityEngine;

public class WingChickenArmorSkillLogic : MonoBehaviour
{
	public FlameFx m_smallFlameFxPrefab;

	public FlameFx m_largeFlameFxPrefab;

	public Explosion m_explosionPrefab;

	private GameObject m_explosion;

	private MonsterFlameLogic m_flameLogic;

	private MonsterFlameFx m_flameFx;

	private BattleCamera m_battleCamera;

	private CameraShakeData m_shakeData;

	public float explosionDamage { get; set; }

	public bool isBurning
	{
		get
		{
			return null != m_flameFx && m_flameFx.enabled;
		}
	}

	private void Awake()
	{
		m_battleCamera = UnityEngine.Object.FindObjectOfType(typeof(BattleCamera)) as BattleCamera;
		base.enabled = false;
	}

	private void OnBattleReady()
	{
		if (null == m_flameLogic)
		{
			m_flameLogic = FlameLogic.FindOrCreateFlameLogic<MonsterFlameLogic>() as MonsterFlameLogic;
		}
		StageManager instance = SingletonComponent<StageManager>.Instance;
		if (null != instance && null != instance.EnemyObject && null == m_flameFx && null != m_smallFlameFxPrefab && null != m_largeFlameFxPrefab)
		{
			string weightGroup = instance.EnemyObject.GetComponent<Monster>().weightGroup;
			GameObject p_prefab = ((!weightGroup.Equals("s", StringComparison.InvariantCultureIgnoreCase)) ? m_largeFlameFxPrefab.gameObject : m_smallFlameFxPrefab.gameObject);
			m_flameFx = FlameFx.FindOrCreateFlameFx<MonsterFlameFx>(instance.EnemyObject, p_prefab);
			ShadowProjector componentInChildren = MBHier.GetComponentInChildren<ShadowProjector>(instance.EnemyObject);
			if (null != componentInChildren)
			{
				m_flameFx.transform.parent = componentInChildren.m_reference;
			}
			m_flameFx.transform.localPosition = Vector3.zero;
			m_flameFx.targetRenderers = MBHier.GetComponentsInChildren<SkinnedMeshRenderer>(instance.EnemyObject);
			m_flameFx.monster = instance.EnemyObject.GetComponent<Monster>();
		}
	}

	private void OnEnable()
	{
		Messenger.AddListener("battle_ready", OnBattleReady);
		Messenger.AddListener("perfect_block", OnPerfectBlock);
		Messenger<CutsceneController.CutsceneType>.AddListener("cutscene_start", OnCutsceneStart);
	}

	private void OnDisable()
	{
		Messenger.RemoveListener("battle_ready", OnBattleReady);
		Messenger.RemoveListener("perfect_block", OnPerfectBlock);
		Messenger<CutsceneController.CutsceneType>.RemoveListener("cutscene_start", OnCutsceneStart);
	}

	private void OnCutsceneStart(CutsceneController.CutsceneType p_cutscene)
	{
		if (null != m_explosion)
		{
			UnityEngine.Object.DestroyImmediate(m_explosion);
		}
		if (null != m_flameFx)
		{
			m_flameFx.OnBurnOut();
		}
	}

	public void Burn(GameObject p_target, PlayerData p_attackerData, float p_damageMul, float p_interval, float p_duration)
	{
		if (!(null == m_flameFx) && !(null == m_flameLogic))
		{
			m_flameLogic.attackerData = p_attackerData;
			m_flameLogic.damageMultiplier = p_damageMul;
			m_flameLogic.duration = p_duration;
			m_flameLogic.damageInterval = p_interval;
			m_flameLogic.Burn(m_flameFx.OnBurnOut);
			m_flameFx.Burn();
		}
	}

	protected void OnPerfectBlock()
	{
		if (!(null == m_flameLogic) && m_flameLogic.enabled && !(null == m_explosionPrefab) && !(null == m_flameFx))
		{
			m_explosion = UnityEngine.Object.Instantiate(m_explosionPrefab.gameObject) as GameObject;
			m_explosion.transform.parent = m_flameFx.transform.parent;
			m_explosion.transform.localPosition = Vector3.zero;
			m_shakeData = m_explosion.GetComponent<CameraShakeData>();
			Explosion component = m_explosion.GetComponent<Explosion>();
			if (null != component)
			{
				component.attackerData = m_flameLogic.attackerData;
				component.damage = explosionDamage;
				component.Explode(OnExplode);
			}
			m_flameLogic.OnBurnOut();
			m_flameFx.Burn();
		}
	}

	protected void OnExplode()
	{
		m_flameFx.OnBurnOut();
		m_battleCamera.ShakeCamera(m_shakeData);
	}
}
