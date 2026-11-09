using System.Collections;
using UnityEngine;

public class WolfArmorPlayerSkill : ArmorSetPlayerSkill
{
	[SerializeField]
	protected PlayerBuffFx m_fxAuraPrefab;

	[SerializeField]
	protected float m_extremeAtkMod = 1.8f;

	[SerializeField]
	protected float m_extremeDefMod = 12f;

	[SerializeField]
	protected float m_extremeSpdMod = 1.5f;

	[SerializeField]
	protected float m_exhaustMod = 0.5f;

	[SerializeField]
	protected float m_extremeModeTime = 45f;

	[SerializeField]
	protected bool m_infiniteSkill;

	protected NBTimer m_timer;

	protected float m_defaultMinAtk;

	protected float m_defaultMaxAtk;

	protected float m_defaultAtkSpeed = 1f;

	protected bool m_skillEffectOn;

	protected PlayerBuffFx m_fxAura;

	protected float[] m_armors = new float[5];

	protected bool skillEffectOn
	{
		get
		{
			return m_skillEffectOn;
		}
		set
		{
			m_skillEffectOn = value;
			if (value)
			{
				if (null != m_fxAuraPrefab)
				{
					m_fxAura = PlayerBuffFx.InstantiateFx(m_fxAuraPrefab.gameObject, base.player, "Male_ Spine");
				}
			}
			else if (null != m_fxAura)
			{
				Object.Destroy(m_fxAura.gameObject);
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		skillEffectOn = false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (skillEffectOn)
		{
			skillEffectOn = false;
		}
	}

	protected override void OnBattleReady()
	{
		base.OnBattleReady();
		if (!base.enabled || m_skillTriggered)
		{
			return;
		}
		if (base.player != null && base.player.Data != null)
		{
			m_defaultMinAtk = base.player.Data.MinAttack;
			m_defaultMaxAtk = base.player.Data.MaxAttack;
			m_defaultAtkSpeed = base.player.AttackSpeedModifier;
			for (int i = 0; i < m_armors.Length; i++)
			{
				m_armors[i] = base.player.Data.Equipments[i].armorMod;
			}
		}
		else
		{
			m_skillTriggered = true;
			base.enabled = false;
		}
	}

	protected virtual void OnEnable()
	{
		m_timer = base.gameObject.AddComponent<NBTimer>();
		m_timer.Name = "Wolf Armor Set Timer";
		m_timer.IsRealtime = false;
		Messenger.AddListener("player_dead", OnPlayerDead);
		Messenger.AddListener("battle_start", OnBattleStart);
		Messenger.AddListener("battle_end", OnBattleEnd);
		Messenger.AddListener("battle_exit", OnPlayerRetreat);
	}

	protected virtual void OnDisable()
	{
		if (null != m_timer)
		{
			Object.Destroy(m_timer);
		}
		Messenger.RemoveListener("player_dead", OnPlayerDead);
		Messenger.RemoveListener("battle_start", OnBattleStart);
		Messenger.RemoveListener("battle_end", OnBattleEnd);
		Messenger.RemoveListener("battle_exit", OnPlayerRetreat);
	}

	protected void OnPlayerDead()
	{
		if (base.enabled)
		{
			if (skillEffectOn)
			{
				skillEffectOn = false;
				m_timer.Reset();
				SetPlayerStatModifier(1f, 1f, 1f);
			}
			if (!(SingletonComponent<CutsceneController>.Instance == null) && base.enabled && !m_skillTriggered)
			{
				StartCoroutine(TriggerExtremeMode());
			}
		}
	}

	protected void OnBattleStart()
	{
		if (base.enabled)
		{
			if (null != m_timer && m_timer.Counter > 0f)
			{
				m_timer.Pause(false);
			}
			if (skillEffectOn)
			{
				SetPlayerStatModifier(m_extremeAtkMod, m_extremeDefMod, m_extremeSpdMod);
			}
		}
	}

	protected virtual void OnBattleEnd()
	{
		if (base.enabled)
		{
			if (null != m_timer)
			{
				m_timer.Pause(true);
			}
			SetPlayerStatModifier(1f, 1f, 1f);
		}
	}

	protected void OnPlayerRetreat()
	{
		if (base.enabled)
		{
			SetPlayerStatModifier(1f, 1f, 1f);
		}
	}

	protected override void PreCutsceneInitialization()
	{
		if (null != m_timer)
		{
			m_timer.Pause(true);
		}
		SingletonComponent<BattleScreenUI>.Instance.EnableBattleHudCamera();
	}

	protected override void PostCutsceneInitialization()
	{
		OnTriggerExtremeMode();
		if (null != m_timer && m_timer.Counter > 0f)
		{
			m_timer.Pause(false);
		}
	}

	protected virtual void OnExtremeModeEnd()
	{
		skillEffectOn = false;
		SetPlayerStatModifier(m_exhaustMod, m_exhaustMod, m_exhaustMod);
		base.player.Data.HpPercent = 0.1f;
	}

	protected IEnumerator TriggerExtremeMode()
	{
		Dbg.Log("SkillWolfArmor.TriggerExtremeMode()");
		SingletonComponent<BattleScreenUI>.Instance.DisableBattleHudCamera();
		base.player.ChangeState(Player.State.NoMore);
		base.player.Data.Hp = 1f;
		PlayerPetrifiedLogic petrifiedLogic = Object.FindObjectOfType(typeof(PlayerPetrifiedLogic)) as PlayerPetrifiedLogic;
		if (null != petrifiedLogic && petrifiedLogic.petrified)
		{
			petrifiedLogic.OnShattered();
		}
		base.player.Invincible = true;
		if (base.monster.GetComponent<Collider>() != null)
		{
			base.monster.GetComponent<Collider>().enabled = false;
		}
		base.monsterAI.enabled = false;
		BattleManager bmgr = SingletonComponent<BattleManager>.Instance;
		if (bmgr != null)
		{
			bmgr.enabled = false;
		}
		yield return new WaitForEndOfFrame();
		while (!base.monster.IsReady)
		{
			yield return null;
		}
		while (base.player.AnimPlayer.IsPlaying && base.player.AnimPlayer.CurrentAnimName == "Death")
		{
			if (!base.monster.AnimPlayer.IsPlaying)
			{
				base.monster.AnimPlayer.Play("Idle_01");
			}
			yield return null;
		}
		base.player.ChangeState(Player.State.Idle);
		base.player.fader.FadeFromCurrentAlpha(1f, 0.5f, null);
		TriggerSkill();
	}

	protected virtual void OnTriggerExtremeMode()
	{
		skillEffectOn = true;
		base.player.Data.HpPercent = 1f;
		SetPlayerStatModifier(m_extremeAtkMod, m_extremeDefMod, m_extremeSpdMod);
		m_skillTriggered = true;
		if (base.monster.GetComponent<Collider>() != null)
		{
			base.monster.GetComponent<Collider>().enabled = true;
		}
		BattleManager instance = SingletonComponent<BattleManager>.Instance;
		if (instance != null)
		{
			instance.enabled = true;
		}
		base.player.Invincible = false;
		m_timer.CountDown(m_extremeModeTime, OnExtremeModeEnd);
		SingletonComponent<BattleManager>.Instance.ChangeState(BattleManager.State.Battle);
	}

	protected void SetPlayerStatModifier(float p_atkMod, float p_defMod, float p_spdMod)
	{
		if (base.enabled && !(null == base.player) && base.player.Data != null)
		{
			base.player.Data.MinAttack = m_defaultMinAtk * p_atkMod;
			base.player.Data.MaxAttack = m_defaultMaxAtk * p_atkMod;
			base.player.AttackSpeedModifier = m_defaultAtkSpeed * p_spdMod;
			for (int i = 0; i < m_armors.Length; i++)
			{
				base.player.Data.Equipments[i].armorMod = m_armors[i] * p_defMod;
			}
			base.player.Data.UpdateArmor();
		}
	}
}
