public class SpeciesParser
{
	public static MonsterSpecies Parse(string p_text)
	{
		string text = p_text.ToLower();
		switch (text.Trim())
		{
		case "raptor":
			return MonsterSpecies.Raptor;
		case "wingserpent":
			return MonsterSpecies.WingSerpent;
		case "bull":
			return MonsterSpecies.Bull;
		case "wolf":
			return MonsterSpecies.Wolf;
		case "hydra":
			return MonsterSpecies.Hydra;
		case "landdragon":
			return MonsterSpecies.LandDragon;
		case "stonedragon":
			return MonsterSpecies.StoneDragon;
		case "wingchicken":
			return MonsterSpecies.WingChicken;
		case "tricep":
			return MonsterSpecies.Tricep;
		case "shark":
			return MonsterSpecies.Shark;
		case "rhino":
			return MonsterSpecies.Rhino;
		case "lizard":
			return MonsterSpecies.Lizard;
		case "mantaray":
			return MonsterSpecies.MantaRay;
		case "dodobo":
			return MonsterSpecies.Dodobo;
		case "bobodo":
			return MonsterSpecies.Bobodo;
		case "gorilla":
			return MonsterSpecies.Gorilla;
		case "dinobird":
			return MonsterSpecies.DinoBird;
		case "kingape":
			return MonsterSpecies.KingApe;
		case "bird":
			return MonsterSpecies.Bird;
		case "alligator":
			return MonsterSpecies.Alligator;
		case "worm":
			return MonsterSpecies.Worm;
		case "epicdragon":
			return MonsterSpecies.EpicDragon;
		default:
			Dbg.LogWarn("Unknown species: " + p_text);
			return MonsterSpecies.Unknown;
		}
	}

	public static string[] GetSpecieNames()
	{
		string[] array = new string[22];
		for (int i = 0; i < 22; i++)
		{
			MonsterSpecies monsterSpecies = (MonsterSpecies)i;
			array[i] = monsterSpecies.ToString();
		}
		return array;
	}
}
