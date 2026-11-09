public class MonsterDamageDisplay : CharacterDamageDisplay
{
	protected override void Awake()
	{
		base.Awake();
		m_referencePointGameObject = "EnemyDamagePoint";
		m_damageValueHUD = "EnemyDamageValue_Panel";
		Messenger<DamageData>.AddListener("monster_receive_dmg", OnHit);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Messenger<DamageData>.RemoveListener("monster_receive_dmg", OnHit);
	}
}
