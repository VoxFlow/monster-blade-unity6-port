using System;
using UnityEngine;

public class FlameLogic : MonoBehaviour
{
	protected CollisionData m_collisionData;

	protected NBTimer m_timer;

	protected NBTimer m_damageTimer;

	protected Callback m_onBurnOut;

	public PlayerData attackerData { get; set; }

	public float damageMultiplier { protected get; set; }

	public float duration { protected get; set; }

	public float damageInterval { protected get; set; }

	protected CollisionData collisionData
	{
		get
		{
			Dbg.Assert(null != m_collisionData, "FalmeLogic.collisionData == null");
			return m_collisionData;
		}
		set
		{
			m_collisionData = value;
			m_collisionData.ignoreArmour = true;
		}
	}

	protected virtual void Awake()
	{
		m_timer = base.gameObject.AddComponent<NBTimer>();
		m_timer.Name = "Flame Timer";
		m_damageTimer = base.gameObject.AddComponent<NBTimer>();
		m_damageTimer.Name = "Flame Damage Timer";
		duration = 0f;
		base.enabled = false;
		Messenger.AddListener("battle_end", OnBattleEnd);
		Messenger<CutsceneController.CutsceneType>.AddListener("cutscene_start", OnCutsceneStart);
		Messenger<CutsceneController.CutsceneType>.AddListener("cutscene_end", OnCutsceneEnd);
	}

	protected void OnDestroy()
	{
		Messenger.RemoveListener("battle_end", OnBattleEnd);
		Messenger<CutsceneController.CutsceneType>.RemoveListener("cutscene_start", OnCutsceneStart);
		Messenger<CutsceneController.CutsceneType>.RemoveListener("cutscene_end", OnCutsceneEnd);
	}

	protected void OnEnable()
	{
		m_timer.Pause(false);
		m_damageTimer.Pause(false);
	}

	protected void OnDisable()
	{
		m_timer.Pause(true);
		m_damageTimer.Pause(true);
	}

	protected void OnBattleEnd()
	{
		if (m_timer.Counter != 0f)
		{
			OnBurnOut();
		}
	}

	protected void OnCutsceneStart(CutsceneController.CutsceneType p_cutscene)
	{
		if (m_timer.Counter != 0f)
		{
			base.enabled = false;
		}
	}

	protected void OnCutsceneEnd(CutsceneController.CutsceneType p_cutscene)
	{
		if (m_timer.Counter != 0f)
		{
			base.enabled = true;
		}
	}

	public virtual void Burn(Callback p_onBurnOut = null)
	{
		if (duration == 0f)
		{
			Dbg.LogWarn("FlameLogic.duration cannot be zero.");
			return;
		}
		if (attackerData == null)
		{
			Dbg.LogWarn("FlameLogic.attackerData cannot be null.");
			return;
		}
		collisionData.attackerData = attackerData;
		m_timer.CountDown(duration);
		m_damageTimer.CountDown(damageInterval, OnDamageTimerEnd);
		m_onBurnOut = p_onBurnOut;
		base.enabled = true;
	}

	protected void OnDamageTimerEnd()
	{
		DealFlameDamage();
		m_damageTimer.CountDown(damageInterval, OnDamageTimerEnd);
	}

	protected virtual void DealFlameDamage()
	{
	}

	public void OnBurnOut()
	{
		m_timer.Reset();
		m_damageTimer.Reset();
		base.enabled = false;
		if (m_onBurnOut != null)
		{
			m_onBurnOut();
			m_onBurnOut = null;
		}
	}

	protected void Update()
	{
		if (m_timer.Counter == 0f)
		{
			OnBurnOut();
		}
	}

	public static FlameLogic FindOrCreateFlameLogic<T>() where T : FlameLogic
	{
		Type typeFromHandle = typeof(T);
		string text = typeFromHandle.ToString();
		FlameLogic flameLogic = UnityEngine.Object.FindObjectOfType(typeFromHandle) as FlameLogic;
		if (null == flameLogic)
		{
			GameObject gameObject = new GameObject(text);
			flameLogic = gameObject.AddComponent<T>();
		}
		return flameLogic;
	}
}
