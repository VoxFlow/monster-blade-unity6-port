public class SkillEnum
{
	public enum SelfSkill
	{
		None = -1,
		LifeSteal = 0,
		HitPoints = 1,
		Damage = 2,
		Critical = 3,
		Armour = 4,
		Max = 5
	}

	public enum AssistSkill
	{
		None = -1,
		Default = 0,
		MassiveDamage = 1,
		Heal = 2,
		BuffDamage = 3,
		BuffCritical = 4,
		Stun = 5,
		Max = 6
	}

	public enum ArmorSetSkill
	{
		None = -1,
		Wolf = 0,
		WingChicken = 1,
		StoneDragon = 2,
		Hydra = 3,
		LandDragon = 4,
		EpicDragon = 5,
		Max = 6
	}

	public static SelfSkill ParseSelfSkill(string p_string)
	{
		string[] array = new string[5] { "lifesteal", "hitpoints", "damage", "critical", "armour" };
		p_string = p_string.ToLower();
		p_string = p_string.Trim();
		for (int i = 0; i < 5; i++)
		{
			if (array[i] == p_string)
			{
				return (SelfSkill)i;
			}
		}
		return SelfSkill.None;
	}

	public static AssistSkill ParseAssistSkill(string p_string)
	{
		string[] array = new string[6] { "none", "massivedamage", "heal", "buffdamage", "buffcritical", "stun" };
		p_string = p_string.ToLower();
		p_string = p_string.Trim();
		for (int i = 0; i < 6; i++)
		{
			if (array[i] == p_string)
			{
				return (AssistSkill)i;
			}
		}
		return AssistSkill.None;
	}
}
