public class PlayerDamageDisplay : CharacterDamageDisplay
{
	protected override void Awake()
	{
		base.Awake();
		m_referencePointGameObject = "PlayerDamagePoint";
		m_damageValueHUD = "PlayerDamageValue_Panel";
		Messenger<DamageData>.AddListener("player_receive_dmg", OnHit);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Messenger<DamageData>.RemoveListener("player_receive_dmg", OnHit);
	}

	protected virtual void OnLifeSteal(float p_hp)
	{
		OnHeal(p_hp);
	}
}
