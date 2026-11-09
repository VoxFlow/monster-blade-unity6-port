public class Equipment
{
	public enum Part
	{
		Helm = 0,
		Shoulder = 1,
		Chest = 2,
		Pants = 3,
		Weapon = 4,
		Max = 5
	}

	public class Data
	{
		public ItemDetails itemDetails;

		public string id = string.Empty;

		public float lifeMaxMod;

		public float minDamageMod;

		public float maxDamageMod;

		public float armorMod;

		public float critMod;

		public Element.Type element = Element.Type.Metal;

		public float elementDamage;

		public override string ToString()
		{
			return string.Format("[Equipment.Data: LifeMaxMod={0}, MinDamageMod={1}, MaxDamageMod={2}, ArmorMod={3}, CritMod={4}, Element={5}\nItemDetails={6}]", lifeMaxMod, minDamageMod, maxDamageMod, armorMod, critMod, element, itemDetails);
		}
	}

	private static Part[] materialOrder = new Part[5]
	{
		Part.Pants,
		Part.Shoulder,
		Part.Weapon,
		Part.Chest,
		Part.Helm
	};

	public static string Helm = Part.Helm.ToString().ToLower();

	public static string Shoulder = Part.Shoulder.ToString().ToLower();

	public static string Chest = Part.Chest.ToString().ToLower();

	public static string Pants = Part.Pants.ToString().ToLower();

	public static string Weapon = Part.Weapon.ToString().ToLower();

	public static string Belt = "belt";

	public static string Leg = "leg";

	public static string Body = "body";

	public static string Head = "head";

	public static string Base = "base";

	public static string Empty = "empty";

	public static int GetMaterialIndex(Part p_part)
	{
		for (int i = 0; i < materialOrder.Length; i++)
		{
			if (materialOrder[i] == p_part)
			{
				return i;
			}
		}
		return -1;
	}

	public static bool IsArmour(Part _part)
	{
		return _part == Part.Chest || _part == Part.Helm || _part == Part.Pants || _part == Part.Shoulder;
	}

	public static Part FromString(string p_part)
	{
		string text = p_part.ToLower();
		if (text.Contains("helm") || text.Contains("head"))
		{
			return Part.Helm;
		}
		if (text.Contains("shoulder"))
		{
			return Part.Shoulder;
		}
		if (text.Contains("chest") || text.Contains("body"))
		{
			return Part.Chest;
		}
		if (text.Contains("pants") || text.Contains("leg") || text.Contains("legging"))
		{
			return Part.Pants;
		}
		if (text.Contains("weapon"))
		{
			return Part.Weapon;
		}
		Dbg.LogWarn("Could not convert the given Part : " + p_part);
		return Part.Max;
	}

	public static string ConvertString(string _part)
	{
		string text = _part.ToLower();
		if (text.Contains("helm") || text.Contains("head"))
		{
			return Head;
		}
		if (text.Contains("shoulders"))
		{
			return Shoulder;
		}
		if (text.Contains("chest") || text.Contains("body"))
		{
			return Body;
		}
		if (text.Contains("pants") || text.Contains("leg") || text.Contains("legging"))
		{
			return Leg;
		}
		if (text.Contains("weapon"))
		{
			return Weapon;
		}
		return Belt;
	}

	public static string ToString(Part p_part)
	{
		switch (p_part)
		{
		case Part.Helm:
			return Head;
		case Part.Chest:
			return Body;
		case Part.Pants:
			return Leg;
		case Part.Shoulder:
			return Shoulder;
		case Part.Weapon:
			return Weapon;
		default:
			return Belt;
		}
	}
}
