using UnityEngine;

public class WingChickenFlameLogic : MonoBehaviour
{
	private enum eFlameState
	{
		Disable = 0,
		ReadyForAttackEnd = 1,
		OnFire = 2,
		WaitForCutScene = 3,
		WaitToSelfKill = 4
	}

	public GameObject m_Target;

	public float m_fHurtPeriod = 1f;

	private bool m_IsExplosioned;

	private NBTimer m_FlameTimer;

	private NBTimer m_TotalTimer;

	private NBTimer m_DelayExplosionTimer;

	private Player m_player;

	private WingDragon m_monster;

	private Color m_OriginColor;

	private Color m_TargetColor = new Color(0f, 0.5f, 0f, 1f);

	private float m_ColorBlendingTime_Dur = 0.5f;

	private float m_ColorBlendingTime_Curr;

	private eFlameState m_FlameState;

	private MonsterCollisionData m_collisData;

	private ParticleSystem[] m_ParticleSys;

	protected void Awake()
	{
		m_FlameTimer = base.gameObject.AddComponent<NBTimer>();
		m_TotalTimer = base.gameObject.AddComponent<NBTimer>();
		m_DelayExplosionTimer = base.gameObject.AddComponent<NBTimer>();
		m_ParticleSys = MBHier.GetComponentsInChildren<ParticleSystem>(base.gameObject);
		m_collisData = GetComponent<MonsterCollisionData>();
		if (null != SingletonComponent<StageManager>.Instance)
		{
			m_player = SingletonComponent<StageManager>.Instance.PlayerObject.GetComponent<Player>();
			m_monster = SingletonComponent<StageManager>.Instance.EnemyObject.GetComponent<WingDragon>();
		}
		m_IsExplosioned = false;
		Messenger.AddListener("player_super_atk", SetWaitForCutScene);
		Messenger<PlayerCollisionData>.AddListener("monster_get_hit", OnMonsterGetHurt);
		Messenger<CutsceneController.CutsceneType>.AddListener(CutsceneMessage.CUTSCENE_START, OnCutsceneStart);
		Messenger<CutsceneController.CutsceneType>.AddListener(CutsceneMessage.CUTSCENE_END, OnCutsceneEnd);
		m_FlameState = eFlameState.Disable;
		base.gameObject.SetActiveRecursively(false);
		m_OriginColor = Color.white;
		base.enabled = false;
	}

	public void Burn()
	{
		base.enabled = true;
		base.gameObject.SetActiveRecursively(true);
		ParticleSystem[] particleSys = m_ParticleSys;
		foreach (ParticleSystem particleSystem in particleSys)
		{
			if (particleSystem != null)
			{
				particleSystem.enableEmission = true;
			}
		}
		m_FlameTimer.CountDown(m_fHurtPeriod, "OnFlameHurt");
		m_TotalTimer.CountDown(5f, "DestroySelf");
		m_IsExplosioned = false;
		m_FlameState = eFlameState.Disable;
		m_monster.isFlameOnPlayer = true;
		m_ColorBlendingTime_Curr = 0f;
	}

	protected void OnDestroy()
	{
		Messenger<PlayerCollisionData>.RemoveListener("monster_get_hit", OnMonsterGetHurt);
		Messenger.RemoveListener("player_super_atk", SetWaitForCutScene);
		Messenger<CutsceneController.CutsceneType>.RemoveListener(CutsceneMessage.CUTSCENE_START, OnCutsceneStart);
		Messenger<CutsceneController.CutsceneType>.RemoveListener(CutsceneMessage.CUTSCENE_END, OnCutsceneEnd);
	}

	private void SetWaitForCutScene()
	{
		if (base.enabled)
		{
			m_FlameState = eFlameState.WaitForCutScene;
			m_FlameTimer.Reset();
			m_TotalTimer.Reset();
		}
	}

	private void OnCutsceneStart(CutsceneController.CutsceneType p_type)
	{
		if (base.enabled)
		{
			m_FlameState = eFlameState.WaitForCutScene;
			m_FlameTimer.Pause(true);
			m_TotalTimer.Pause(true);
		}
	}

	private void OnCutsceneEnd(CutsceneController.CutsceneType p_type)
	{
		if (base.enabled)
		{
			m_FlameState = eFlameState.WaitForCutScene;
			m_FlameTimer.Pause(false);
			m_TotalTimer.Pause(false);
		}
	}

	private void OnMonsterGetHurt(PlayerCollisionData p_collData)
	{
		if (base.enabled && p_collData.attack.type != PlayerAttack.Type.None)
		{
			HurtPlayerByExplosion();
		}
	}

	private void HurtPlayerByExplosion()
	{
		if (!m_IsExplosioned)
		{
			m_collisData.attack.multiplier = 100f;
			m_collisData.attack.type = MonsterAttack.Type.Signature;
			PlayGroundExplosion(m_player.transform);
			m_IsExplosioned = true;
			m_FlameTimer.Reset();
			m_TotalTimer.Reset();
			EnableSkill(false);
			m_DelayExplosionTimer.CountDown(4f, "DelayDestroy");
			m_ColorBlendingTime_Curr = 0f;
			m_monster.isFlameOnPlayer = false;
		}
	}

	private void DelayDestroy()
	{
		EnableSkill(true);
		m_collisData.attack.type = MonsterAttack.Type.Signature;
		m_collisData.attack.multiplier = 100f;
		Messenger<MonsterCollisionData>.Invoke("player_get_hit", m_collisData);
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_mon_fall_light");
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_mon_jump");
		DestroySelf();
	}

	protected void Update()
	{
		if (m_player.CurrentState == Player.State.Death && m_FlameState != eFlameState.WaitToSelfKill)
		{
			DestroySelf();
		}
		bool flag = true;
		ParticleSystem[] particleSys = m_ParticleSys;
		foreach (ParticleSystem particleSystem in particleSys)
		{
			if (!(particleSystem == null) && (particleSystem.enableEmission || particleSystem.particleCount != 0))
			{
				flag = false;
				break;
			}
		}
		if (m_ColorBlendingTime_Curr < m_ColorBlendingTime_Dur)
		{
			m_ColorBlendingTime_Curr += Time.deltaTime;
			if (m_ColorBlendingTime_Curr > m_ColorBlendingTime_Dur)
			{
				m_ColorBlendingTime_Curr = m_ColorBlendingTime_Dur;
			}
			if (!m_IsExplosioned)
			{
				float num = m_ColorBlendingTime_Curr / m_ColorBlendingTime_Dur;
				Color playerColor = m_TargetColor * num + m_OriginColor * (1f - num);
				SetPlayerColor(playerColor);
			}
			else
			{
				float num2 = m_ColorBlendingTime_Curr / m_ColorBlendingTime_Dur;
				Color playerColor2 = m_TargetColor * (1f - num2) + m_OriginColor * num2;
				SetPlayerColor(playerColor2);
			}
		}
		if (flag)
		{
			base.gameObject.SetActiveRecursively(false);
			base.enabled = false;
		}
	}

	private void SetPlayerColor(Color newColor)
	{
		Renderer componentInChildren = MBHier.GetComponentInChildren<Renderer>(m_player.gameObject);
		if (componentInChildren.sharedMaterials == null)
		{
			return;
		}
		Material[] sharedMaterials = componentInChildren.sharedMaterials;
		foreach (Material material in sharedMaterials)
		{
			if (!(null == material))
			{
				material.SetColor("_Color", newColor);
			}
		}
	}

	public void OnFlameHurt()
	{
		if (!m_IsExplosioned)
		{
			m_collisData.attack.multiplier = 0.05f;
			m_collisData.attack.type = MonsterAttack.Type.Heavy;
			Messenger<MonsterCollisionData>.Invoke("player_get_hit", m_collisData);
			m_FlameTimer.CountDown(m_fHurtPeriod, "OnFlameHurt");
		}
	}

	protected void OnEnable()
	{
		m_FlameTimer.Pause(false);
		m_TotalTimer.Pause(false);
	}

	protected void OnDisable()
	{
		m_FlameTimer.Pause(true);
		m_TotalTimer.Pause(true);
	}

	private void DestroySelf()
	{
		m_FlameState = eFlameState.WaitToSelfKill;
		m_IsExplosioned = true;
		m_FlameTimer.Reset();
		m_TotalTimer.Reset();
		if ((bool)m_player)
		{
			SetPlayerColor(m_OriginColor);
		}
		ParticleSystem[] particleSys = m_ParticleSys;
		foreach (ParticleSystem particleSystem in particleSys)
		{
			if (particleSystem != null)
			{
				particleSystem.enableEmission = false;
			}
		}
		m_monster.isFlameOnPlayer = false;
	}

	public void PlayGroundExplosion(Transform trans)
	{
		Messenger<Transform>.Invoke("fireball_PlayDelayExplosion", trans);
	}

	private void EnableSkill(bool p_enable)
	{
		SuperAttackSystem instance = SingletonComponent<SuperAttackSystem>.Instance;
		if (null != instance && null != instance.superAttackLogic)
		{
			if (p_enable)
			{
				instance.superAttackLogic.data.EnableSkill(this);
			}
			else
			{
				instance.superAttackLogic.data.DisableSkill(this);
			}
		}
		SkillSystem instance2 = SingletonComponent<SkillSystem>.Instance;
		if (null != instance2 && null != instance2.friendSkill)
		{
			if (p_enable)
			{
				instance2.friendSkill.skillData.EnableSkill(this);
			}
			else
			{
				instance2.friendSkill.skillData.DisableSkill(this);
			}
		}
	}
}
