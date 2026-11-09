public class EventEnum
{
	public enum Type
	{
		Dungeon = 0,
		Pet = 1,
		Tutorial = 2,
		Max = 3
	}

	public static Type FromString(string p_string)
	{
		p_string = p_string.ToLower();
		switch (p_string)
		{
		case "dungeon":
			return Type.Dungeon;
		case "pet":
			return Type.Pet;
		case "tutorial":
			return Type.Tutorial;
		default:
			return Type.Max;
		}
	}

	public static string ToString(Type p_event)
	{
		switch (p_event)
		{
		case Type.Dungeon:
			return "Dungeon";
		case Type.Pet:
			return "Pet";
		case Type.Tutorial:
			return "Tutorial";
		default:
			return string.Empty;
		}
	}
}
