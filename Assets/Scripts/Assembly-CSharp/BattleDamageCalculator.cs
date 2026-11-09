using UnityEngine;

public class BattleDamageCalculator
{
	public const float DEFAULT_ARMOR_MOD = 0.125f;

	public const float MAX_DAMAGE = 999999f;

	private BattleDamageCalculator()
	{
	}

	public static void CalculateDamage(PlayerData p_attacker, PlayerData p_defender, bool p_ignoreCritical, bool p_ignoreArmour, out float p_rawDamage, out float p_finalDamage, out bool p_isCritical)
	{
		float atk = p_attacker.Atk;
		float num = Random.Range(0f, 100f);
		float criticalChance = p_attacker.CriticalChance;
		bool flag = !p_ignoreCritical && num < criticalChance;
		float num2 = ((!flag && !p_ignoreArmour) ? 0.125f : 0f);
		float num3 = ((!flag) ? 1f : p_attacker.CriticalMul);
		float num4 = 1f + (float)(p_attacker.Level - p_defender.Level) * 0.1f;
		if (num4 <= 0f)
		{
			num4 = 0.1f;
		}
		p_rawDamage = (atk - p_defender.Def * num2) * num4 * num3;
		p_finalDamage = (atk - p_defender.GetResistanceAgainst(p_attacker.AttackElement) * num2) * num4 * num3;
		if (!p_ignoreArmour && num2 == 0f)
		{
			p_isCritical = true;
		}
		else
		{
			p_isCritical = false;
		}
		p_rawDamage = Mathf.Clamp(p_rawDamage, 1f, 999999f);
		p_finalDamage = Mathf.Clamp(p_finalDamage, 1f, 999999f);
	}
}
