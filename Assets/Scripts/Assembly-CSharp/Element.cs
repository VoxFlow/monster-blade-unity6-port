public class Element
{
	public enum Type
	{
		None = -1,
		Fire = 0,
		Earth = 1,
		Lightning = 2,
		Water = 3,
		Metal = 4,
		Max = 5
	}

	public static readonly float[,] resistanceTable = new float[5, 5]
	{
		{ 1f, 2f, 1f, 0f, 1f },
		{ 0f, 1f, 1f, 2f, 1f },
		{ 1f, 1f, 1f, 1f, 2f },
		{ 2f, 0f, 1f, 1f, 1f },
		{ 1f, 1f, 2f, 1f, 1f }
	};

	public static readonly string[] typeStrings = new string[5] { "Fire", "Earth", "Lightning", "Water", "Metal" };

	public static Type GetWeakElement(Type p_element)
	{
		if (p_element.CompareTo(Type.None) <= 0 || p_element.CompareTo(Type.Max) >= 0)
		{
			Dbg.LogError("Invalid element type: " + (int)p_element);
			return Type.None;
		}
		Type[] array = new Type[5]
		{
			Type.Earth,
			Type.Water,
			Type.Metal,
			Type.Fire,
			Type.Lightning
		};
		return array[(int)p_element];
	}

	public static Type GetStrongElement(Type p_element)
	{
		if (p_element.CompareTo(Type.None) <= 0 || p_element.CompareTo(Type.Max) >= 0)
		{
			Dbg.LogError("Invalid element type: " + (int)p_element);
			return Type.None;
		}
		Type[] array = new Type[5]
		{
			Type.Water,
			Type.Fire,
			Type.Metal,
			Type.Earth,
			Type.Lightning
		};
		return array[(int)p_element];
	}

	public static Type FromString(string p_string)
	{
		switch (p_string.ToLower())
		{
		case "fire":
			return Type.Fire;
		case "nature":
		case "earth":
			return Type.Earth;
		case "lightning":
			return Type.Lightning;
		case "water":
			return Type.Water;
		case "metal":
		case "neutral":
			return Type.Metal;
		default:
			Dbg.LogError("Wrong element given " + p_string + ". Use Metal as default.");
			return Type.Metal;
		}
	}
}
