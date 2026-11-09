public class Battle
{
	public enum Type
	{
		Dungeon = 0,
		Survival = 1,
		DungeonEvent = 2,
		PetEvent = 3,
		Max = 4
	}

	public static Type FromString(string p_string)
	{
		p_string = p_string.ToLower();
		switch (p_string)
		{
		case "dungeon":
			return Type.Dungeon;
		case "survival":
			return Type.Survival;
		case "dungeonevent":
			return Type.DungeonEvent;
		case "petevent":
			return Type.PetEvent;
		default:
			return Type.Max;
		}
	}

	public static string ToString(Type p_type)
	{
		switch (p_type)
		{
		case Type.Dungeon:
			return "Dungeon";
		case Type.Survival:
			return "Survival";
		case Type.DungeonEvent:
			return "DungeonEvent";
		case Type.PetEvent:
			return "PetEvent";
		default:
			return string.Empty;
		}
	}
}
