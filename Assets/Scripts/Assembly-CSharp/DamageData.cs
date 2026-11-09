public class DamageData
{
	public PlayerData attacker;

	public PlayerData defender;

	public float rawDamage;

	public float finalDamage;

	public DamageType damageType = DamageType.None;

	public DamageData()
	{
	}

	public DamageData(DamageData p_rhs)
	{
		if (p_rhs != null)
		{
			attacker = p_rhs.attacker;
			defender = p_rhs.defender;
			rawDamage = p_rhs.rawDamage;
			finalDamage = p_rhs.finalDamage;
			damageType = p_rhs.damageType;
		}
	}
}
