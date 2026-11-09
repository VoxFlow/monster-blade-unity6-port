public abstract class PetrifiedLogic : BattleLogic
{
	protected Callback m_onBreakCb;

	protected float m_defaultDef;

	protected MonsterShakeScript m_shaker;

	protected PetrifyEffect m_fx;

	protected PlayerData m_charData;

	private bool m_petrified;

	protected PlayerData charData
	{
		get
		{
			Dbg.Assert(m_charData != null, "charData has not yet been assinged.");
			return m_charData;
		}
		set
		{
			m_charData = value;
		}
	}

	protected PetrifyEffect petrifyFx
	{
		get
		{
			Dbg.Assert(m_fx != null, "petrifyFx has not yet been assinged.");
			return m_fx;
		}
		set
		{
			m_fx = value;
		}
	}

	public bool petrified
	{
		get
		{
			return m_petrified;
		}
		protected set
		{
			m_petrified = value;
		}
	}

	public NBTimer timer { get; protected set; }

	protected override void Awake()
	{
		base.Awake();
		timer = base.gameObject.AddComponent<NBTimer>();
		timer.Name = "Petrify Timer";
		petrifyFx = base.gameObject.GetComponent<PetrifyEffect>();
	}

	protected virtual void OnEnable()
	{
		if (petrified)
		{
			timer.Pause(true);
		}
	}

	protected virtual void OnDisable()
	{
		if (petrified)
		{
			timer.Pause(false);
		}
	}

	protected override void OnBattleEnd()
	{
		base.OnBattleEnd();
		if (petrified && charData.Hp != 0f)
		{
			OnBreak();
		}
	}

	public virtual void OnPetrified(float p_duration, float p_armourMod, Callback p_onBreak = null)
	{
		if (!(null == timer))
		{
			timer.CountDown(p_duration, OnBreak);
			if (null != petrifyFx)
			{
				petrifyFx.OnPetrified();
			}
			if (charData != null)
			{
				m_defaultDef = charData.Def;
				charData.Def *= p_armourMod;
			}
			m_onBreakCb = p_onBreak;
			petrified = true;
		}
	}

	public virtual void OnBreak()
	{
		petrified = false;
		timer.Reset();
		if (charData != null)
		{
			charData.Def = m_defaultDef;
		}
		if (null != petrifyFx)
		{
			petrifyFx.OnBreak();
		}
		if (m_onBreakCb != null)
		{
			m_onBreakCb();
		}
	}

	public virtual void OnShattered()
	{
		petrified = false;
		timer.Reset();
		if (charData != null)
		{
			charData.Def = m_defaultDef;
		}
		if (null != petrifyFx)
		{
			petrifyFx.OnShattered();
		}
	}

	protected virtual void Shake()
	{
		if (null != petrifyFx)
		{
			petrifyFx.OnShake();
		}
		if (null != m_shaker)
		{
			m_shaker.Shake();
		}
	}

	private new void Update()
	{
		if (petrified)
		{
			if (charData != null && charData.Hp == 0f)
			{
				OnShattered();
			}
			OnUpdate();
		}
	}

	protected abstract void OnUpdate();
}
