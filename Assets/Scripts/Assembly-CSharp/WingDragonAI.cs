using UnityEngine;

public class WingDragonAI : AIController
{
	public enum Phase
	{
		Extreme = 0,
		Calm = 1
	}

	private const float FIREBALL_TRIGGER_HP = 0.2f;

	private Phase m_phase;

	[SerializeField]
	private FireBallLogic m_fireBallPrefab;

	private float m_defaultMinAtk;

	private float m_defaultMaxAtk;

	private float m_nextFireballSpittingHp = 1f;

	private bool m_spittingFireball;

	private FireBallLogic m_fireballLogic;

	private WingDragon m_dragon;

	private float[] m_pahseHpConditions = new float[2] { 0.25f, 1f };

	public Phase phase
	{
		get
		{
			return m_phase;
		}
		set
		{
			m_phase = value;
			switch (m_phase)
			{
			case Phase.Extreme:
				m_monster.Data.MinAttack = m_defaultMinAtk * 2f;
				m_monster.Data.MaxAttack = m_defaultMaxAtk * 2f;
				m_monster.SpeedModifier = 1.5f;
				break;
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (null != m_fireBallPrefab)
		{
			m_fireballLogic = Object.Instantiate(m_fireBallPrefab) as FireBallLogic;
		}
	}

	protected override void OnBattleInit()
	{
		base.OnBattleInit();
		m_dragon = (WingDragon)m_monster;
		m_defaultMinAtk = m_monster.Data.MinAttack;
		m_defaultMaxAtk = m_monster.Data.MinAttack;
		UpdateFireballSpittingCondition();
	}

	protected override MonsterAttack PickAttackCommand()
	{
		if (m_comboList.Count == 0)
		{
			return null;
		}
		if (m_spittingFireball)
		{
			m_comboList[0] = MonsterAttack.Type.Signature;
		}
		else if (m_comboList[0] == MonsterAttack.Type.Signature)
		{
			m_comboList[0] = MonsterAttack.RandomAttack(false);
		}
		return base.PickAttackCommand();
	}

	protected override string DetermineAttackAnim(MonsterAttack.Type p_attackType)
	{
		if (m_spittingFireball)
		{
			return WingDragon.SIGNATURE_ATK_BEGIN_ANIM;
		}
		return base.DetermineAttackAnim(p_attackType);
	}

	protected override void UpdateGlobal()
	{
		base.UpdateGlobal();
		UpdatePhase();
		UpdateFireball();
	}

	private void UpdatePhase()
	{
		Phase phase = this.phase;
		float hpPercent = m_monster.Data.HpPercent;
		for (int i = 0; i < m_pahseHpConditions.Length; i++)
		{
			if (m_pahseHpConditions[i] >= hpPercent)
			{
				phase = (Phase)i;
				break;
			}
		}
		if (phase != this.phase)
		{
			this.phase = phase;
		}
	}

	private void UpdateFireball()
	{
		if (!m_spittingFireball)
		{
			float hpPercent = m_monster.Data.HpPercent;
			if (hpPercent <= m_nextFireballSpittingHp)
			{
				m_spittingFireball = true;
				UpdateFireballSpittingCondition();
			}
		}
	}

	private void UpdateFireballSpittingCondition()
	{
		m_nextFireballSpittingHp -= 0.2f;
	}

	private void EmitFireBall(GameObject parent)
	{
		Dbg.Log("EmitFireBall");
		if (!(m_fireballLogic == null) && m_dragon.IsFlying())
		{
			m_fireballLogic.transform.parent = null;
			m_fireballLogic.transform.position = parent.transform.position + parent.transform.right * -0.3f + parent.transform.up * 0.3f;
			m_fireballLogic.Target = m_player.gameObject;
			m_fireballLogic.gameObject.active = true;
			m_fireballLogic.OnFire();
			MonsterCollisionData component = m_fireballLogic.gameObject.GetComponent<MonsterCollisionData>();
			component.attackerData = m_monster.Data;
			component.attacker = m_monster.gameObject;
			component.attack = new MonsterAttack();
			component.attack.zone = m_currentAttack.zone;
			component.attack.phase = m_currentAttack.phase;
			component.attack.comboIndex = m_currentAttack.comboIndex;
			component.attack.animationName = m_currentAttack.animationName;
			component.attack.attackPoint = m_currentAttack.attackPoint;
			component.attack.multiplier = 0.5f;
			component.attack.knockSpeed = m_currentAttack.knockSpeed;
			component.attack.type = MonsterAttack.Type.Signature;
			Collider component2 = m_fireballLogic.gameObject.GetComponent<Collider>();
			component2.enabled = true;
		}
	}

	protected override void OnDamageWindow()
	{
		if (m_currentAttack != null && m_currentAttack.animationName == WingDragon.SIGNATURE_ATK_BEGIN_ANIM)
		{
			GameObject attackPointNode = m_dragon.GetAttackPointNode(m_currentAttack.attackPoint);
			EmitFireBall(attackPointNode);
			m_spittingFireball = false;
		}
		base.OnDamageWindow();
	}

	public override void Pause(bool p_pause)
	{
		base.Pause(p_pause);
		m_fireballLogic.enabled = !p_pause;
	}
}
