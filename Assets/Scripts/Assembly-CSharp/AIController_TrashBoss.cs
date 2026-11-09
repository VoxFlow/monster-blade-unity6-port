using System;

public class AIController_TrashBoss : AIController
{
	public enum eSPAttack
	{
		None = -1,
		Heavy = 0,
		Roar = 1,
		Combo = 2
	}

	private eSPAttack m_CurrSPAttack;

	protected Monster_TrashBoss m_TrashBoss;

	public eSPAttack CurrSPAttack
	{
		get
		{
			return m_CurrSPAttack;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_TrashBoss = GetComponent<Monster_TrashBoss>();
		Messenger.AddListener("monster_stun_begin", OnStun);
		Messenger.AddListener("perfect_block", OnPerfectBlock);
	}

	protected override void OnDestroy()
	{
		Messenger.RemoveListener("monster_stun_begin", OnStun);
		Messenger.RemoveListener("perfect_block", OnPerfectBlock);
		base.OnDestroy();
	}

	protected override void Start()
	{
		if (!m_monster.Data.m_monsterData.isBoss)
		{
			base.Start();
			return;
		}
		m_behaviorDB = Singleton<MonsterBehaviorDB>.Instance;
		if (m_behaviorDB == null)
		{
			Dbg.LogWarn("m_behaviorDB is null.");
		}
		m_triggers = m_behaviorDB.GetTriggers(m_monster.Data.Level, m_monster.Data.m_monsterData.isBoss);
		UpdateBehavior(1f);
		m_CurrSPAttack = eSPAttack.None;
	}

	private void Attack_Special_Heavy()
	{
	}

	private void Attack_Special_Roar()
	{
		m_currentAttack.type = MonsterAttack.Type.Roar;
		m_currentAttack.zone = MonsterAttack.Zone.Area;
		MonsterCollisionData attackCollisonData = m_TrashBoss.GetAttackCollisonData(m_currentAttack.attackPoint);
		attackCollisonData.attackerData = m_monster.Data;
		attackCollisonData.attacker = m_monster.gameObject;
		attackCollisonData.attack = m_currentAttack;
		if (m_TrashBoss != null)
		{
			m_TrashBoss.PlaySPAttackFX(Monster_TrashBoss.eSPAttackFX.Roar);
		}
	}

	private void Attack_Special_Combo()
	{
		if (m_TrashBoss != null)
		{
			m_TrashBoss.ClearSPAttackFX();
			m_TrashBoss.PlaySPAttackFX(Monster_TrashBoss.eSPAttackFX.Combo);
		}
	}

	public bool IsSpecialAttack(string p_animName)
	{
		return Array.IndexOf(m_monster.SpecialAttackAnimations, p_animName) >= 0;
	}

	protected override void OnBeginAttack(MonsterAttack p_attack)
	{
		if (IsSpecialAttack(p_attack.animationName) && p_attack.comboLength > 1 && p_attack.comboIndex == 1)
		{
			Attack_Special_Combo();
		}
		base.OnBeginAttack(p_attack);
	}

	protected override void OnDamageWindow()
	{
		if (m_currentAttack != null && IsSpecialAttack(m_currentAttack.animationName))
		{
			if (m_currentAttack.type == MonsterAttack.Type.Signature)
			{
				Attack_Special_Heavy();
			}
			else if (m_currentAttack.type == MonsterAttack.Type.Roar)
			{
				Attack_Special_Roar();
			}
		}
		base.OnDamageWindow();
	}

	protected override void OnWindDown()
	{
		if (m_TrashBoss != null)
		{
			m_TrashBoss.ClearSPAttackFX();
		}
		base.OnWindDown();
	}

	protected override void OnDead()
	{
		if (m_TrashBoss != null)
		{
			m_TrashBoss.ClearSPAttackFX();
		}
		base.OnIdle();
	}

	protected override void OnStun()
	{
		if (m_TrashBoss != null)
		{
			m_TrashBoss.ClearSPAttackFX();
		}
		base.OnStun();
	}

	protected void OnPerfectBlock()
	{
		if (m_TrashBoss != null)
		{
			m_TrashBoss.ClearSPAttackFX();
		}
	}

	private void OnSPAttack_Disable()
	{
		m_TrashBoss.SetAttackable(false);
	}

	private void OnSPAttack_Enable()
	{
		m_TrashBoss.SetAttackable(true);
	}
}
