using UnityEngine;

[RequireComponent(typeof(MonsterCollisionData))]
public class DragonFireBreath : FireBreath
{
	private MonsterCollisionData m_collisionData;

	public Monster monster { get; set; }

	public Player player { get; set; }

	protected override void Awake()
	{
		base.Awake();
		m_collisionData = base.gameObject.GetComponent<MonsterCollisionData>();
		base.gameObject.tag = "Enemy";
	}

	public override void Emit(Transform p_root, Transform p_target)
	{
		base.Emit(p_root, p_target);
		m_collisionData.attackerData = monster.Data;
		m_collisionData.attacker = base.gameObject;
		m_collisionData.attack = new MonsterAttack(monster.CurrentAttack);
		m_collisionData.attack.multiplier = m_damageMultiplier;
	}
}
