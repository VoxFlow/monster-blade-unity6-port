using System;
using System.Collections;
using UnityEngine;

public class OmniSlash : SuperAttack
{
	[SerializeField]
	private PlayerBuffFx m_weaponGlowFxPrefab;

	[SerializeField]
	private SelfDestroy m_weaponFlareFxPrefab;

	[SerializeField]
	private SelfDestroy m_flareFxPrefab;

	[SerializeField]
	private SelfDestroy m_geyserFxPrefab;

	[SerializeField]
	private SelfDestroy[] m_slashEffects;

	[SerializeField]
	private GameObject m_cameraRootPrefab;

	[SerializeField]
	private Shader m_radiantSwordShader;

	[SerializeField]
	private Color m_radiantSwordColor = Color.white;

	private GameObject m_cameraTweenTargetRoot;

	private MonsterPetrifiedLogic[] m_monsterPetrifiedLogics;

	private OmniSlashFx m_slashFx;

	private OmniSlashCameraEvent m_cameraEvent;

	private OmniSlashSceneLightingEvent m_lightingEvent;

	private static readonly string SLASH_ANIMATION = "Att_Omnislash";

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (null != m_cameraTweenTargetRoot)
		{
			UnityEngine.Object.Destroy(m_cameraTweenTargetRoot.gameObject);
		}
	}

	public override void PrebattleInit(PlayerData p_data)
	{
		if (p_data.IsEquipingFullSet(GLOBAL.DARK_WOLF_SETNAME))
		{
			CharacterGenerator.SetOneMaterialPerPart(true);
		}
	}

	public override void Init()
	{
		base.Init();
		if (null == m_slashFx)
		{
			m_slashFx = m_player.gameObject.AddComponent<OmniSlashFx>();
			if (null != m_slashFx)
			{
				m_slashFx.flareFxPrefab = m_flareFxPrefab;
				m_slashFx.slashEffects = m_slashEffects;
				m_slashFx.weaponGlowFxPrefab = m_weaponGlowFxPrefab;
				m_slashFx.weaponFlareFxPrefab = m_weaponFlareFxPrefab;
				m_slashFx.radiantSwordShader = m_radiantSwordShader;
				m_slashFx.radiantSwordColor = m_radiantSwordColor;
				m_slashFx.geyserFxPrefab = m_geyserFxPrefab;
				m_slashFx.player = m_player;
			}
		}
		if (null == m_cameraTweenTargetRoot)
		{
			m_cameraTweenTargetRoot = AttachObjectToPlayer(m_cameraRootPrefab);
		}
		if (null == m_cameraEvent)
		{
			m_cameraEvent = m_player.gameObject.AddComponent<OmniSlashCameraEvent>();
			BattleCamera battleCamera = UnityEngine.Object.FindObjectOfType(typeof(BattleCamera)) as BattleCamera;
			if (null != m_cameraEvent && null != battleCamera)
			{
				m_cameraEvent.battleCameraRoot = battleCamera.Root;
				m_cameraEvent.cameraTweenTargetRoot = m_cameraTweenTargetRoot.transform;
				OmniSlashCameraEvent cameraEvent = m_cameraEvent;
				cameraEvent.onFinishResetCameraSetup = (Action)Delegate.Combine(cameraEvent.onFinishResetCameraSetup, new Action(OnFinishResetCameraSetup));
			}
		}
		if (null == m_lightingEvent)
		{
			m_lightingEvent = m_player.gameObject.AddComponent<OmniSlashSceneLightingEvent>();
		}
	}

	protected override void OnBattleStart()
	{
		base.OnBattleStart();
		m_monsterPetrifiedLogics = UnityEngine.Object.FindObjectsOfType(typeof(MonsterPetrifiedLogic)) as MonsterPetrifiedLogic[];
	}

	protected override IEnumerator PlayAttackSequences()
	{
		if (!(null == m_player) && !(null == m_player.GetComponent<Animation>()[SLASH_ANIMATION]))
		{
			EnableMonsterAttack(false);
			PlayerAttack attack = new PlayerAttack
			{
				animation = SLASH_ANIMATION,
				interuptAnimation = true,
				multiplier = 0.7f,
				type = PlayerAttack.Type.Super
			};
			m_player.PerformAttackSequence(attack);
			while (!m_player.IsReadyToAttack)
			{
				yield return null;
			}
			if (!m_monster.GetComponent<Animation>().enabled)
			{
				EnableMonsterAttack(true);
			}
			if (!m_player.IsMoving() && m_monster.Data.Hp > 0f)
			{
				m_player.Idle();
			}
			m_player.AnimPlayer.TimeScale = 1f;
		}
	}

	private void OnFinishResetCameraSetup()
	{
		EnableMonsterAttack(true);
	}

	private void EnableMonsterAttack(bool p_enable)
	{
		bool flag = false;
		if (m_monsterPetrifiedLogics != null && m_monsterPetrifiedLogics.Length > 0)
		{
			for (int i = 0; i < m_monsterPetrifiedLogics.Length; i++)
			{
				flag |= m_monsterPetrifiedLogics[i].petrified;
			}
		}
		if (!flag)
		{
			m_player.EnableCollision(p_enable);
			m_monsterAI.Pause(!p_enable);
		}
	}

	private GameObject AttachObjectToPlayer(GameObject p_prefab)
	{
		if (null == p_prefab || null == m_player)
		{
			return null;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(p_prefab) as GameObject;
		if (null != gameObject)
		{
			gameObject.transform.AttachTo(m_player.transform, TransformAttachMode.RETAIN_LOCAL_TRANSFORM);
		}
		return gameObject;
	}
}
