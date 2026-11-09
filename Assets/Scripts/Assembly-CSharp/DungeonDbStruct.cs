public class DungeonDbStruct
{
	public string id = string.Empty;

	public string name = string.Empty;

	public string cityRef = string.Empty;

	public string act = string.Empty;

	public string stage = string.Empty;

	public int battleCount = 1;

	public Element.Type element = Element.Type.Metal;

	public string boss_id = string.Empty;

	public int difficulty;

	public int min;

	public int max;

	public int igcReward;

	public int stamina;

	public int requiredLvl;

	public bool hasHalfDungeonCutscene;

	public float[] chanceElements = new float[7];

	public float chanceDodobo;

	public bool locked;

	public string iconSpriteName = string.Empty;
}
