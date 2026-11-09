using UnityEngine;

public abstract class PetrifyBlockLogic : BlockSystem
{
	[SerializeField]
	private float m_duration;

	[SerializeField]
	private float m_monsterShakeInterval = 3f;

	protected MonsterPetrifiedLogic m_petrifiedLogic;

	protected bool m_petrifyAttack;

	public float duration
	{
		get
		{
			return m_duration;
		}
		set
		{
			m_duration = value;
		}
	}

	public float armourMod { get; set; }

	public MonsterPetrifiedLogic petrifiedLogic
	{
		get
		{
			return m_petrifiedLogic;
		}
	}

	protected virtual bool IsPetrifyAttack(MonsterAttack p_attack)
	{
		return p_attack != null && p_attack.type != MonsterAttack.Type.Ranged;
	}

	protected override void Awake()
	{
		base.Awake();
		duration = 0f;
	}

	public override void Init()
	{
		base.Init();
		if (null == m_petrifiedLogic)
		{
			m_petrifiedLogic = base.gameObject.GetComponent<MonsterPetrifiedLogic>();
		}
		if (null != m_petrifiedLogic)
		{
			m_petrifiedLogic.Init();
			m_petrifiedLogic.shakeInterval = m_monsterShakeInterval;
		}
	}

	public virtual void PetrifyTarget()
	{
		m_petrifiedLogic.OnPetrified(duration, armourMod, null);
	}

	protected override void OnPerfectBlock(MonsterAttack p_attack)
	{
		if (IsPetrifyAttack(p_attack))
		{
			m_perfectBlock = true;
			m_player.IsReady = true;
			m_petrifyAttack = true;
			PetrifyTarget();
			Messenger.Invoke("perfect_block");
		}
		else
		{
			base.OnPerfectBlock(p_attack);
		}
	}

	protected override void OnPlayerFinishBlock()
	{
		if (m_perfectBlock && m_petrifyAttack)
		{
			m_petrifyAttack = false;
			Messenger.Invoke("perfect_block_end");
		}
	}
}
