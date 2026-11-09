using UnityEngine;

public class PlayerSpecialFx : MonoBehaviour
{
	private PlayerAttack m_playerAttack;

	private BattleCamera m_battleCamera;

	private CameraTiltFx m_cameraTiltScript;

	private CameraShakeData m_cleanHitShakeData;

	private CameraShakeData m_blockShakeData;

	private SlashEffectEmitter m_slashFX;

	private GameObject m_smokePoof;

	private ParticleSystemController m_blockFx;

	private ParticleSystemController m_perfectBlockFx;

	private ParticleSystemController m_perfectDodgeFx;

	private Shader m_shader;

	private string m_swordSwingEffects = "SLASH_SWING";

	private string m_sfx_attack_voice = "list_player_attack_voice";

	private string m_sfx_att_element = "list_player_fire_attack";

	private string m_sfx_wounded_voice = "list_player_wounded_voice_light";

	private string m_sfx_wounded_effect = "list_player_wounded_effect_light";

	private string m_sfx_blade_hit = "list_mon_flesh_impact";

	private string m_sfx_attack_critical = "list_player_crit_attack_voice";

	private Vector3[] m_followTiltAngles = new Vector3[8]
	{
		new Vector3(4f, 0f, 0f),
		new Vector3(-4f, 0f, 0f),
		new Vector3(0f, 4f, 0f),
		new Vector3(0f, -4f, 0f),
		new Vector3(2f, 2f, 0f),
		new Vector3(2f, -2f, 0f),
		new Vector3(-2f, 2f, 0f),
		new Vector3(-2f, 2f, 0f)
	};

	public bool enableSwordSwingTilt { get; set; }

	public void Awake()
	{
		GameObject gameObject = GameObject.Find("Camera_Root");
		if (gameObject != null)
		{
			m_battleCamera = gameObject.GetComponent<BattleCamera>();
		}
		m_cleanHitShakeData = base.gameObject.AddComponent<CameraShakeData>();
		m_blockShakeData = base.gameObject.AddComponent<CameraShakeData>();
		if (m_cleanHitShakeData != null)
		{
			m_cleanHitShakeData.amplitude = new Vector3(0.2f, 0.1f, 0.1f);
			m_cleanHitShakeData.duration = 1f;
			m_cleanHitShakeData.frequency = 20f;
		}
		if (m_blockShakeData != null)
		{
			m_blockShakeData.amplitude = new Vector3(0.1f, 0.1f, 0.1f);
			m_blockShakeData.duration = 0.25f;
			m_blockShakeData.frequency = 10f;
		}
		m_shader = base.GetComponent<Renderer>().sharedMaterial.shader;
		base.GetComponent<Renderer>().sharedMaterial.SetColor("_AmbientColor", SingletonComponent<LevelSetting>.Instance.ambientLight);
		Element.Type type = Element.Type.Metal;
		Equipment.Data equipmentData = base.gameObject.GetComponent<Player>().Data.GetEquipmentData(Equipment.Part.Weapon);
		if (equipmentData != null)
		{
			type = equipmentData.element;
		}
		string[] array = new string[5] { "FX/Prefabs/Fire_SlashEmitter", "FX/Prefabs/Nature_SlashEmitter", "FX/Prefabs/Lightning_SlashEmitter", "FX/Prefabs/Ice_SlashEmitter", "FX/Prefabs/Metal_SlashEmitter" };
		string path = array[4];
		if ((int)type < array.Length)
		{
			path = array[(int)type];
		}
		Object obj = Resources.Load(path);
		Object obj2 = Resources.Load("FX/Prefabs/FX_SmokePoof");
		Object obj3 = Resources.Load("FX/Prefabs/FX_Block");
		Object obj4 = Resources.Load("FX/Prefabs/FX_Perfect_Block");
		Object obj5 = Resources.Load("FX/Prefabs/FX_SlashCrescent");
		string[] array2 = new string[5] { "list_player_fire_attack", "list_player_nature_attack", "list_player_lightning_attack", "list_player_ice_attack", "list_player_metal_attack" };
		m_sfx_att_element = array2[4];
		if ((int)type < array2.Length)
		{
			m_sfx_att_element = array2[(int)type];
		}
		if (obj != null)
		{
			m_slashFX = (Object.Instantiate(obj) as GameObject).GetComponent<SlashEffectEmitter>();
		}
		else
		{
			m_slashFX = null;
		}
		if (obj2 != null)
		{
			m_smokePoof = Object.Instantiate(obj2) as GameObject;
		}
		else
		{
			m_smokePoof = null;
		}
		GameObject gameObject2 = null;
		if (obj3 != null)
		{
			gameObject2 = Object.Instantiate(obj3) as GameObject;
			if (gameObject2 != null)
			{
				m_blockFx = gameObject2.GetComponent<ParticleSystemController>();
			}
		}
		else
		{
			m_blockFx = null;
		}
		if (obj4 != null)
		{
			gameObject2 = Object.Instantiate(obj4) as GameObject;
			if (gameObject2 != null)
			{
				m_perfectBlockFx = gameObject2.GetComponent<ParticleSystemController>();
			}
		}
		else
		{
			m_perfectBlockFx = null;
		}
		if (obj5 != null)
		{
			gameObject2 = Object.Instantiate(obj5) as GameObject;
			if (gameObject2 != null)
			{
				m_perfectDodgeFx = gameObject2.GetComponent<ParticleSystemController>();
			}
		}
		else
		{
			m_perfectDodgeFx = null;
		}
		if (m_slashFX == null)
		{
			Dbg.LogError("Unable to create slash effect");
		}
		else
		{
			m_slashFX.transform.parent = base.transform;
		}
		if (m_smokePoof == null)
		{
			Dbg.LogError("Unable to create smoke particle");
		}
		else
		{
			m_smokePoof.SetActiveRecursively(false);
		}
		if (m_blockFx == null)
		{
			Dbg.LogError("Unable to create block effect");
		}
		else
		{
			m_blockFx.Stop();
			m_blockFx.transform.parent = base.transform;
		}
		if (m_perfectBlockFx == null)
		{
			Dbg.LogError("Unable to create perfect-block effect");
		}
		else
		{
			m_perfectBlockFx.Stop();
			m_perfectBlockFx.transform.parent = base.transform;
		}
		if (m_perfectDodgeFx == null)
		{
			Dbg.LogError("Unable to create perfect-dodge effect");
		}
		else
		{
			m_perfectDodgeFx.Stop();
			m_perfectDodgeFx.transform.parent = base.transform;
		}
		enableSwordSwingTilt = true;
		Messenger<PlayerAttack>.AddListener("player_begin_atk", OnAttackBegin);
		Messenger<DamageData>.AddListener("player_receive_dmg", OnHit);
		Messenger<DamageData>.AddListener("monster_receive_dmg", OnMonsterReceiveDamage);
		Messenger<CutsceneController.CutsceneType>.AddListener(CutsceneMessage.CUTSCENE_START, OnCutsceneStart);
		Messenger.AddListener("normal_block", OnNormalBlock);
		Messenger.AddListener("perfect_block", OnPerfectBlock);
		Messenger.AddListener("battle_init", OnBattleInit);
		Messenger.AddListener("player_revived", OnRevive);
		Messenger<Player.DodgeDirection>.AddListener("perfect_dodge_start", OnPefectDodge);
		Messenger<bool>.AddListener("battle_pause", OnPause);
	}

	private void OnDestroy()
	{
		Messenger<PlayerAttack>.RemoveListener("player_begin_atk", OnAttackBegin);
		Messenger<DamageData>.RemoveListener("player_receive_dmg", OnHit);
		Messenger<DamageData>.RemoveListener("monster_receive_dmg", OnMonsterReceiveDamage);
		Messenger<CutsceneController.CutsceneType>.RemoveListener(CutsceneMessage.CUTSCENE_START, OnCutsceneStart);
		Messenger.RemoveListener("normal_block", OnNormalBlock);
		Messenger.RemoveListener("perfect_block", OnPerfectBlock);
		Messenger.RemoveListener("battle_init", OnBattleInit);
		Messenger.RemoveListener("player_revived", OnRevive);
		Messenger<Player.DodgeDirection>.RemoveListener("perfect_dodge_start", OnPefectDodge);
		Messenger<bool>.RemoveListener("battle_pause", OnPause);
	}

	private void Start()
	{
		if ((bool)m_battleCamera && (bool)m_battleCamera.MainCameraNode)
		{
			m_cameraTiltScript = m_battleCamera.MainCameraNode.GetComponent<CameraTiltFx>();
		}
	}

	private void OnCutsceneStart(CutsceneController.CutsceneType p_type)
	{
		m_playerAttack = null;
		m_slashFX.StopAllFX();
		m_blockFx.Stop();
		m_perfectBlockFx.Stop();
		m_perfectDodgeFx.Stop();
	}

	private void OnBattleInit()
	{
		Vector3 position = base.transform.position;
		Vector3 position2 = SingletonComponent<StageManager>.Instance.EnemyObject.transform.position;
		Vector3 position3 = position + (position - position2).magnitude * 0.5f * base.transform.forward;
		position3.y = 1f;
		m_slashFX.transform.position = position3;
	}

	private void OnAttackBegin(PlayerAttack p_atk)
	{
		m_playerAttack = p_atk;
		PlaySoundFx(m_swordSwingEffects);
		if (p_atk.type == PlayerAttack.Type.Ripost && m_perfectDodgeFx != null)
		{
			m_perfectDodgeFx.Play();
		}
	}

	private void OnDamageWindow()
	{
		if (enableSwordSwingTilt && m_playerAttack != null && m_playerAttack.type != PlayerAttack.Type.Ripost)
		{
			float riseUpPeriod = 0f;
			float followUpPeriod = 0.1f;
			float returnPeriod = 0.2f;
			m_cameraTiltScript.RiseUpAngle = Vector3.zero;
			m_cameraTiltScript.FollowUpAngle = m_followTiltAngles[(int)m_playerAttack.direction];
			m_cameraTiltScript.RiseUpPeriod = riseUpPeriod;
			m_cameraTiltScript.FollowUpPeriod = followUpPeriod;
			m_cameraTiltScript.ReturnPeriod = returnPeriod;
			m_cameraTiltScript.Play();
			SingletonComponent<SoundManager>.Instance.PlayPlaylist(m_sfx_attack_voice, 1f);
		}
	}

	private void OnWindDown()
	{
	}

	private void OnMonsterReceiveDamage(DamageData p_damage)
	{
		switch (p_damage.damageType)
		{
		case DamageType.Block:
			OnGuardDamage();
			break;
		case DamageType.Critical:
			OnCriticalDamage();
			PlaySoundFx(m_sfx_att_element);
			break;
		case DamageType.Graze:
			OnGrazeDamage();
			break;
		case DamageType.None:
			break;
		case DamageType.Special:
			break;
		default:
			OnCleanDamage();
			break;
		}
	}

	private void OnCleanDamage()
	{
		if (m_playerAttack != null)
		{
			CallSlashEmitter(SlashEffectEmitter.FxType.CleanHit, m_playerAttack.direction);
		}
		PlaySwordCollideSound();
	}

	private void OnGrazeDamage()
	{
		if (m_playerAttack != null)
		{
			CallSlashEmitter(SlashEffectEmitter.FxType.GrazeHit, m_playerAttack.direction);
		}
		PlaySwordCollideSound();
	}

	private void OnGuardDamage()
	{
		if (m_playerAttack != null)
		{
			CallSlashEmitter(SlashEffectEmitter.FxType.GuardHit, m_playerAttack.direction);
		}
		PlaySwordCollideSound();
		if (SingletonComponent<SoundManager>.Instance != null)
		{
			SingletonComponent<SoundManager>.Instance.PlayPlaylist(m_sfx_wounded_effect, 1f);
		}
	}

	private void OnCriticalDamage()
	{
		if (m_playerAttack != null)
		{
			CallSlashEmitter(SlashEffectEmitter.FxType.CriticalHit, m_playerAttack.direction);
		}
		PlaySwordCollideSound();
		PlaySoundFx(m_sfx_blade_hit);
		PlaySoundFx(m_sfx_attack_critical);
	}

	private void OnHit(DamageData p_damage)
	{
		DamageType damageType = p_damage.damageType;
		if (damageType == DamageType.None || damageType == DamageType.Block)
		{
			OnBlockHit();
		}
		else
		{
			OnCleanHit();
		}
	}

	private void OnCleanHit()
	{
		if (SingletonComponent<SoundManager>.Instance != null)
		{
			SingletonComponent<SoundManager>.Instance.PlayPlaylist(m_sfx_wounded_voice, 1f);
		}
		if (m_battleCamera != null)
		{
			m_battleCamera.ShakeCamera(m_cleanHitShakeData);
		}
		Color red = Color.red;
		Color red2 = Color.red;
		red.a = 0f;
		red2.a = 0.3f;
		SingletonComponent<BattleScreenUI>.Instance.FlashScreen(red, red2, 0.25f);
		if (m_slashFX != null)
		{
			m_slashFX.StopAllFX();
		}
	}

	private void OnBlockHit()
	{
		if (m_battleCamera != null)
		{
			m_battleCamera.ShakeCamera(m_blockShakeData);
		}
	}

	private void OnNormalBlock()
	{
		if (m_blockFx != null)
		{
			m_blockFx.Play();
		}
	}

	private void OnPerfectBlock()
	{
		if (m_blockFx != null)
		{
			m_blockFx.Play();
		}
		if (m_perfectBlockFx != null)
		{
			m_perfectBlockFx.Play();
		}
	}

	private void OnPefectDodge(Player.DodgeDirection p_dir)
	{
		if (m_perfectBlockFx != null)
		{
			m_perfectBlockFx.Play();
		}
	}

	private void OnPause(bool p_pause)
	{
	}

	private void OnStrafeBegin(Player.DodgeDirection p_dir)
	{
		m_smokePoof.transform.position = base.transform.position;
		m_smokePoof.transform.rotation = base.transform.rotation;
		m_smokePoof.SetActiveRecursively(true);
	}

	private void OnStrafeFinish()
	{
		m_smokePoof.SetActiveRecursively(false);
	}

	private void OnChargeBegin()
	{
	}

	private void OnChargeEnd()
	{
	}

	private void OnRevive()
	{
		if (base.GetComponent<Renderer>() != null && base.GetComponent<Renderer>().material.color.a != 1f)
		{
			ModelFader component = base.gameObject.GetComponent<ModelFader>();
			if (component != null)
			{
				component.FadeFromCurrentAlpha(1f, 1f, OnFinishFade);
			}
		}
	}

	private void OnFinishFade(Renderer p_renderer)
	{
		if (!(p_renderer == null))
		{
			Material[] materials = p_renderer.materials;
			foreach (Material material in materials)
			{
				material.shader = m_shader;
			}
		}
	}

	public virtual void PlaySoundFx(string p_sfxPlaylist)
	{
		if (SingletonComponent<SoundManager>.Instance != null)
		{
			SingletonComponent<SoundManager>.Instance.PlayPlaylist(p_sfxPlaylist);
		}
	}

	public virtual void PlaySwordCollideSound()
	{
		PlaySoundFx(m_sfx_blade_hit);
	}

	private void CallSlashEmitter(SlashEffectEmitter.FxType p_fxType, PlayerAttack.Direction p_dir)
	{
		if (m_slashFX == null)
		{
			Dbg.LogWarn("Slash emitter is null");
			return;
		}
		Vector3 vector = Vector3.zero;
		switch (p_dir)
		{
		case PlayerAttack.Direction.TopToBottom:
			vector = base.transform.up * -1f;
			break;
		case PlayerAttack.Direction.BottomToTop:
			vector = base.transform.up;
			break;
		case PlayerAttack.Direction.LeftToRight:
			vector = base.transform.right;
			break;
		case PlayerAttack.Direction.RightToLeft:
			vector = base.transform.right * -1f;
			break;
		case PlayerAttack.Direction.TopLeftToBottomRight:
			vector = (base.transform.up - base.transform.right) * -1f;
			break;
		case PlayerAttack.Direction.BottomRightToTopLeft:
			vector = base.transform.up - base.transform.right;
			break;
		case PlayerAttack.Direction.TopRightToBottomLeft:
			vector = (base.transform.right + base.transform.up) * -1f;
			break;
		case PlayerAttack.Direction.BottomLeftToTopRight:
			vector = base.transform.right + base.transform.up;
			break;
		}
		if (vector != Vector3.zero)
		{
			m_slashFX.EmitSlash(p_fxType, m_slashFX.transform.position, vector);
		}
	}

	private void PauseParticles(GameObject p_particleSystem, bool p_pause)
	{
		if (!p_particleSystem.active)
		{
			return;
		}
		ParticleSystem[] componentsInChildren = MBHier.GetComponentsInChildren<ParticleSystem>(p_particleSystem);
		foreach (ParticleSystem particleSystem in componentsInChildren)
		{
			if (particleSystem.gameObject.active)
			{
				if (p_pause)
				{
					particleSystem.Pause();
				}
				else
				{
					particleSystem.Play();
				}
			}
		}
	}
}
