public class FriendAssistNormalAttack : FriendAssistAttack
{
	protected PlayerCollisionData m_collData;

	public override SkillEnum.AssistSkill assistSkillType
	{
		get
		{
			return SkillEnum.AssistSkill.Default;
		}
	}

	protected virtual void Start()
	{
		m_collData = base.gameObject.AddComponent<PlayerCollisionData>();
		if (m_collData != null)
		{
			m_collData.ignoreArmour = true;
			m_collData.attackerData = base.friendData;
			m_collData.attack = new PlayerAttack();
			if (m_collData.attack != null)
			{
				m_collData.attack.direction = PlayerAttack.Direction.BottomToTop;
				m_collData.attack.forceCleanHit = true;
				m_collData.attack.interuptAnimation = true;
				m_collData.attack.multiplier = 2f;
				m_collData.attack.type = PlayerAttack.Type.Skill;
			}
		}
	}

	protected override void OnBattleReady()
	{
		base.OnBattleReady();
		if (base.enabled && SingletonComponent<StageManager>.Instance != null)
		{
			m_collData.attacker = SingletonComponent<StageManager>.Instance.FriendObjects[0];
		}
	}

	public override void OnSkillTrigger()
	{
		if (m_collData != null)
		{
			Messenger<PlayerCollisionData>.Invoke("monster_get_hit", m_collData);
		}
	}
}
