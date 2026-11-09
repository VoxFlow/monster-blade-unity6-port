using System.Collections;
using UnityEngine;

public class EpicDragonBattle : BattleLogic
{
	private const string FLYING_CAMERA = "Epic Dragon Flying Camera";

	private const string FALLING_IDLE_ANIM = "Falling_Idle_Fight";

	private const string FALLING_DEATH_ANIM = "Falling_Death";

	private const string FALLING_BLOCK_ANIM = "Falling_Block";

	[SerializeField]
	private float m_takeOffKnockDistance = 25f;

	[SerializeField]
	private float m_finalPhaseTimeLimit = 60f;

	[SerializeField]
	private GameObject m_finalPhaseSetupPrefab;

	[SerializeField]
	private UILabel m_timerLabel;

	private float m_defaultBattleDistance;

	private BattleCamera m_battleCamera;

	private Camera m_mainCamera;

	private PlayerSpecialFx m_playerFx;

	private EpicDragon m_dragon;

	private EpicDragonAI m_dragonAI;

	private EpicDragonAI.Phase m_phase = EpicDragonAI.Phase.NONE;

	private GameObject m_finalPhaseSetup;

	private NBTimer m_finalPhaseTimer;

	private int m_counter;

	private float m_defaultFov;

	private string m_defaultIdleAnim = string.Empty;

	private string m_defaultDeadAnim = string.Empty;

	private string m_defaultBlockAnim = string.Empty;

	protected override void Awake()
	{
		base.Awake();
		m_battleCamera = Object.FindObjectOfType(typeof(BattleCamera)) as BattleCamera;
		if (null != m_battleCamera)
		{
			m_mainCamera = m_battleCamera.MainCameraNode.gameObject.GetComponent<Camera>();
		}
		m_finalPhaseTimer = base.gameObject.AddComponent<NBTimer>();
		m_finalPhaseTimer.Name = "Final Phase Timer";
		m_timerLabel.gameObject.SetActiveRecursively(false);
		Messenger<EpicDragonAI.Phase>.AddListener("epicdragon_phase_change", OnPhaseChange);
		Messenger.AddListener("epicdragon_take_off", OnDragonTakeOff);
		Messenger.AddListener("epicdragon_landing", OnDragonLanding);
		Messenger<bool>.AddListener("enable_dodge", OnDodgeEnable);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Messenger<EpicDragonAI.Phase>.RemoveListener("epicdragon_phase_change", OnPhaseChange);
		Messenger.RemoveListener("epicdragon_take_off", OnDragonTakeOff);
		Messenger.RemoveListener("epicdragon_landing", OnDragonLanding);
		Messenger<bool>.RemoveListener("enable_dodge", OnDodgeEnable);
	}

	protected override void OnBattleReady()
	{
		base.OnBattleReady();
		if (null == m_playerFx)
		{
			m_playerFx = m_player.GetComponent<PlayerSpecialFx>();
		}
		if (null == m_dragon)
		{
			m_dragon = (EpicDragon)m_monster;
		}
		if (null == m_dragonAI)
		{
			m_dragonAI = (EpicDragonAI)m_monsterAI;
		}
	}

	protected override void OnBattleStart()
	{
		base.OnBattleStart();
		if (null == m_dragon || null == m_dragonAI)
		{
			return;
		}
		switch (m_dragonAI.phase)
		{
		case EpicDragonAI.Phase.FLYING:
			if (m_dragon.flying && null != m_battleCamera)
			{
				if (m_battleCamera.CurrentCamera == "Epic Dragon Flying Camera")
				{
					m_battleCamera.InterpolateCamera("Epic Dragon Flying Camera", 2f, true);
				}
				else
				{
					m_battleCamera.ChangeCamera("Epic Dragon Flying Camera");
				}
				m_battleCamera.Lock = false;
			}
			break;
		case EpicDragonAI.Phase.FINAL:
			StartCoroutine(StartFinalBattlePhase());
			break;
		case EpicDragonAI.Phase.GROUND:
			break;
		}
	}

	private void OnPhaseChange(EpicDragonAI.Phase p_phase)
	{
		switch (p_phase)
		{
		case EpicDragonAI.Phase.FLYING:
			if (null != m_playerController)
			{
				m_playerController.enabled = false;
			}
			BattleUtils.EnableSkills(false, this);
			break;
		case EpicDragonAI.Phase.GROUND:
			if (m_phase == EpicDragonAI.Phase.FLYING)
			{
				StartCoroutine(Land());
			}
			break;
		case EpicDragonAI.Phase.FINAL:
			StartCoroutine(StartFinalBattlePhase());
			break;
		}
		m_phase = p_phase;
	}

	private void OnDragonTakeOff()
	{
		BattleManager instance = SingletonComponent<BattleManager>.Instance;
		Vector3 a = instance.BattleReferencePoint - m_player.transform.forward * instance.BattleReferenceDistance;
		float num = Vector3.Distance(a, m_player.transform.position);
		m_defaultBattleDistance = SingletonComponent<BattleManager>.Instance.BattleReferenceDistance;
		if (m_player.Data != null && m_player.Data.Hp != 0f && num < 0.001f && !m_player.IsMoving())
		{
			Vector3 p_targetPos = m_takeOffKnockDistance * (instance.BattleReferencePoint - m_player.transform.forward).normalized;
			m_player.KnockTo(p_targetPos, OnPlayerFinishKnockBack);
			m_monsterAI.enabled = false;
			SingletonComponent<BattleManager>.Instance.BattleReferenceDistance = m_takeOffKnockDistance;
		}
		if (null != m_battleCamera)
		{
			m_battleCamera.InterpolateCamera("Epic Dragon Flying Camera", 2f, true);
		}
		m_playerFx.enableSwordSwingTilt = false;
	}

	private void OnDragonLanding()
	{
		if (null != m_battleCamera)
		{
			m_battleCamera.InterpolateCamera(BattleCamera.s_battleCameraName, 2f, true);
		}
		m_playerFx.enableSwordSwingTilt = true;
	}

	private void OnPlayerFinishKnockBack()
	{
		m_playerController.enabled = true;
		m_player.IsReady = true;
		m_player.Idle();
		m_monsterAI.enabled = true;
		if (SingletonComponent<CutsceneController>.Instance != null)
		{
			SingletonComponent<CutsceneController>.Instance.enabled = true;
		}
	}

	protected override void OnBattleEnd()
	{
		base.OnBattleEnd();
		if (null != m_dragonAI && m_dragonAI.phase == EpicDragonAI.Phase.FLYING && null != m_battleCamera)
		{
			m_battleCamera.InterpolateCamera("Epic Dragon Flying Camera", 2f, true);
		}
		if (null != m_finalPhaseTimer)
		{
			m_finalPhaseTimer.Reset();
		}
		if (m_dragonAI.phase == EpicDragonAI.Phase.FLYING && null != m_battleCamera)
		{
			m_battleCamera.Lock = true;
		}
		if (null != m_timerLabel)
		{
			m_timerLabel.gameObject.SetActiveRecursively(false);
		}
	}

	protected override void OnCutsceneStart(CutsceneController.CutsceneType p_type)
	{
		base.OnCutsceneStart(p_type);
		EpicDragonAI.Phase phase = m_dragonAI.phase;
		if (phase == EpicDragonAI.Phase.FINAL)
		{
			m_dragon.gliding = false;
			DestroyFinalPhaseScene();
		}
	}

	protected override void OnCutsceneFinish(CutsceneController.CutsceneType p_type)
	{
		switch (p_type)
		{
		case CutsceneController.CutsceneType.EPICDRAGON_FINAL_PHASE:
			SetupFinalPhaseScene();
			if (null != m_playerController)
			{
				m_playerController.DodgeEnable = false;
			}
			m_defaultIdleAnim = m_player.IdleAnim;
			m_defaultDeadAnim = m_player.DeadAnim;
			m_defaultBlockAnim = m_player.BlockAnim;
			m_player.IdleAnim = "Falling_Idle_Fight";
			m_player.DeadAnim = "Falling_Death";
			m_player.BlockAnim = "Falling_Block";
			m_player.Idle();
			m_finalPhaseTimer.CountDown(m_finalPhaseTimeLimit, OnFallingTimerEnd);
			m_timerLabel.gameObject.SetActiveRecursively(true);
			m_counter = 0;
			m_dragon.gliding = true;
			BattleUtils.EnableSkills(false, this);
			break;
		case CutsceneController.CutsceneType.INTRO_EPICDRAGON:
			if (SingletonComponent<CutsceneController>.Instance != null)
			{
				SingletonComponent<CutsceneController>.Instance.enabled = false;
			}
			break;
		case CutsceneController.CutsceneType.DEATH_EPICDRAGON:
		case CutsceneController.CutsceneType.PLAYER_DEAD_EPICDRAGON:
			m_player.IdleAnim = m_defaultIdleAnim;
			m_player.DeadAnim = m_defaultDeadAnim;
			m_player.BlockAnim = m_defaultBlockAnim;
			m_player.DeadCutscene = CutsceneController.CutsceneType.NONE;
			BattleUtils.EnableSkills(true, this);
			break;
		}
		m_mainCamera.enabled = true;
	}

	protected override void LateUpdate()
	{
		if (null == m_dragon || null == m_dragonAI)
		{
			base.enabled = false;
		}
		else if (m_timerLabel.gameObject.active)
		{
			int num = (int)m_finalPhaseTimer.Counter;
			if (num != m_counter)
			{
				m_timerLabel.text = num.ToString();
				m_counter = num;
			}
		}
	}

	private IEnumerator Land()
	{
		if (!(m_player == null) && !(m_playerController == null) && !(m_monster == null) && !(m_monsterAI == null))
		{
			m_playerController.enabled = false;
			m_monsterAI.enabled = false;
			m_player.Invincible = true;
			yield return StartCoroutine(WaitUntilCharactersReady());
			if (m_player.Data.Hp == 0f)
			{
				Dbg.LogWarn("EpicDragonBattle - Player was already dead!");
				m_player.Invincible = false;
				yield break;
			}
			SingletonComponent<BattleManager>.Instance.BattleReferenceDistance = m_defaultBattleDistance;
			yield return StartCoroutine(MovePlayerBackOnLanding());
			StartCoroutine(InterpolateCameraOnLanding());
			yield return StartCoroutine(m_dragon.Land());
			yield return StartCoroutine(WaitUntilCharactersReady());
			m_playerController.enabled = true;
			m_monsterAI.enabled = true;
			m_player.Invincible = false;
			m_player.Idle();
			m_monster.Idle();
			BattleUtils.EnableSkills(true, this);
		}
	}

	private IEnumerator MovePlayerBackOnLanding()
	{
		BattleManager bm = SingletonComponent<BattleManager>.Instance;
		Vector3 returnPos = bm.BattleReferencePoint - m_player.transform.forward * bm.BattleReferenceDistance;
		float dist = Vector3.Distance(returnPos, m_player.transform.position);
		if (m_player.Data != null && m_player.Data.Hp != 0f && dist > 0.001f)
		{
			Dbg.Log("Waiting player to return");
			while (m_player.CurrentState != Player.State.Idle)
			{
				yield return null;
			}
			m_player.RunTo(returnPos, 10f);
		}
	}

	private IEnumerator InterpolateCameraOnLanding()
	{
		if (!(null == m_mainCamera))
		{
			float lerp = 0f;
			float time = 0f;
			float lerpTime = 0.8f;
			Vector3 beginPos = m_mainCamera.transform.localPosition;
			Quaternion beginRot = m_mainCamera.transform.localRotation;
			while (lerp < 1f)
			{
				time += Time.deltaTime;
				lerp = time / lerpTime;
				m_mainCamera.transform.localPosition = Vector3.Lerp(beginPos, Vector3.zero, lerp);
				m_mainCamera.transform.localRotation = Quaternion.Slerp(beginRot, Quaternion.identity, lerp);
				yield return null;
			}
		}
	}

	private IEnumerator WaitUntilCharactersReady()
	{
		bool playerReady = false;
		bool monsterReady = false;
		while (!playerReady || !monsterReady)
		{
			if (!playerReady && m_player.CanBecomeIdle())
			{
				m_player.Idle();
				playerReady = true;
			}
			if (!monsterReady && m_monster.CanBecomeIdle())
			{
				m_monster.Idle();
				monsterReady = true;
			}
			if (m_player.Data.Hp == 0f)
			{
				break;
			}
			yield return null;
		}
	}

	private IEnumerator StartFinalBattlePhase()
	{
		SingletonComponent<CutsceneController>.Instance.enabled = true;
		if (m_dragon.flying)
		{
			yield return StartCoroutine(Land());
		}
		StartCoroutine(SingletonComponent<CutsceneController>.Instance.PlayCutscene(CutsceneController.CutsceneType.EPICDRAGON_FINAL_PHASE));
		m_player.DeadCutscene = CutsceneController.CutsceneType.PLAYER_DEAD_EPICDRAGON;
	}

	private void SetupFinalPhaseScene()
	{
		if (null == m_finalPhaseSetupPrefab)
		{
			return;
		}
		m_finalPhaseSetup = Object.Instantiate(m_finalPhaseSetupPrefab) as GameObject;
		if (null != m_finalPhaseSetup)
		{
			Transform transform = m_finalPhaseSetup.transform.FindInHierarchy(CompareTag, "Player");
			if (null != transform)
			{
				m_player.transform.AttachTo(transform, TransformAttachMode.PARENT_ORIGIN);
			}
			Transform transform2 = m_finalPhaseSetup.transform.FindInHierarchy(CompareTag, "Enemy");
			if (null != transform2)
			{
				m_monster.transform.AttachTo(transform2, TransformAttachMode.PARENT_ORIGIN);
			}
			Camera componentInChildren = MBHier.GetComponentInChildren<Camera>(m_finalPhaseSetup);
			if (null != componentInChildren)
			{
				m_defaultFov = m_mainCamera.fieldOfView;
				m_mainCamera.transform.position = componentInChildren.transform.position;
				m_mainCamera.transform.rotation = componentInChildren.transform.rotation;
				m_mainCamera.fieldOfView = componentInChildren.fieldOfView;
				componentInChildren.enabled = false;
			}
			m_playerFx.enableSwordSwingTilt = false;
		}
	}

	private void DestroyFinalPhaseScene()
	{
		if (!(null == m_finalPhaseSetup))
		{
			m_player.transform.parent = null;
			m_monster.transform.parent = null;
			m_mainCamera.transform.localPosition = Vector3.zero;
			m_mainCamera.transform.localRotation = Quaternion.identity;
			m_mainCamera.fieldOfView = m_defaultFov;
			m_playerFx.enableSwordSwingTilt = true;
			BattleManager instance = SingletonComponent<BattleManager>.Instance;
			if (null != instance)
			{
				instance.ResetPlayerAndMonsterTransform();
			}
			Object.Destroy(m_finalPhaseSetup);
		}
	}

	private bool CompareTag(Transform p_current, object p_tag)
	{
		if (!(p_tag is string))
		{
			return false;
		}
		return p_current.tag == (string)p_tag;
	}

	private void OnFallingTimerEnd()
	{
		if (null != m_player)
		{
			m_player.Data.Hp = 0f;
		}
		if (null != m_timerLabel)
		{
			m_timerLabel.gameObject.SetActiveRecursively(false);
		}
	}

	private void OnDodgeEnable(bool p_enable)
	{
		if (!(null == m_dragonAI) && !(null == m_playerController) && p_enable && m_dragonAI.phase == EpicDragonAI.Phase.FINAL)
		{
			m_playerController.DodgeEnable = false;
		}
	}
}
