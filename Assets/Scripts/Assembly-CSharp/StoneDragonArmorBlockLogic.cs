public class StoneDragonArmorBlockLogic : PetrifyBlockLogic
{
	public static readonly float monsterShakeInterval = 3f;

	private bool m_isStoneDragonBattle;

	public bool skillEnabled { get; set; }

	protected override void Awake()
	{
		base.Awake();
		skillEnabled = false;
	}

	public override void Init()
	{
		base.Init();
		if (null != m_petrifiedLogic)
		{
			m_petrifiedLogic.shakeInterval = monsterShakeInterval;
		}
		m_isStoneDragonBattle = m_monster != null && m_monster is StoneDragon;
	}

	protected override bool IsPetrifyAttack(MonsterAttack p_attack)
	{
		return skillEnabled || IsStoneDragonPetrifyAttack();
	}

	protected override void OnNormalBlock(MonsterAttack p_attack)
	{
		if (m_isStoneDragonBattle)
		{
			StoneDragon stoneDragon = (StoneDragon)m_monster;
			if (stoneDragon.performingPetrifyAttack)
			{
				StartCoroutine(PlayMonsterHitReact());
			}
		}
		base.OnNormalBlock(p_attack);
	}

	private bool IsStoneDragonPetrifyAttack()
	{
		return m_isStoneDragonBattle && m_monster.CurrentAttack != null && m_monster.CurrentAttack.animationName == ((StoneDragon)m_monster).petrifyAttackAnimation;
	}
}
