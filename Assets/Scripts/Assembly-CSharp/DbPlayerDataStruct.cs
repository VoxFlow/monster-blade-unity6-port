public class DbPlayerDataStruct
{
	public string id = string.Empty;

	public string name = "Finger Fighter";

	public bool isMonster;

	public CharacterType characterType;

	public bool isBoss;

	public MonsterSpecies species = MonsterSpecies.Unknown;

	public int difficulty;

	public int level = 1;

	public float agility = 5f;

	public float constitution = 240f;

	public float strength = 40f;

	public float[] resistances = new float[5] { 1f, 1f, 1f, 1f, 1f };

	public bool[] strongPoint = new bool[8];

	public bool[] weakPoint = new bool[8];

	public string configFile = string.Empty;

	public Element.Type element = Element.Type.Metal;

	public int rewardExperience;

	public int maxStamina;

	public int stamina;

	public int shield;

	public float socialCurrency;

	public float premiumCurrency;

	public float ingameCurrency;

	public int IdleBeforeAttack = 2;

	public float AttackAnimSpeed = 1f;

	public bool[] advanceAtkAbility = new bool[5];

	public bool[] specialAtkAbility = new bool[3];
}
