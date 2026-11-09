public class MonsterData
{
	public string monsterId = string.Empty;

	public int level = 1;

	public int difficulty;

	public bool isBoss;

	public Element.Type element = Element.Type.Metal;

	public int idleBeforeAttack = 2;

	public float attackAnimSpeed = 1f;

	public bool[] advanceAtkAbility = new bool[5];

	public bool[] specialAtkAbility = new bool[3];

	public float[] resistances = new float[5] { 1f, 1f, 1f, 1f, 1f };

	public MonsterSpecies species = MonsterSpecies.Unknown;

	public MonsterData()
	{
	}

	public MonsterData(MonsterData p_monsterData)
	{
		monsterId = p_monsterData.monsterId;
		level = p_monsterData.level;
		difficulty = p_monsterData.difficulty;
		isBoss = p_monsterData.isBoss;
		element = p_monsterData.element;
		idleBeforeAttack = p_monsterData.idleBeforeAttack;
		attackAnimSpeed = p_monsterData.attackAnimSpeed;
		advanceAtkAbility = (bool[])p_monsterData.advanceAtkAbility.Clone();
		specialAtkAbility = (bool[])p_monsterData.specialAtkAbility.Clone();
		resistances = (float[])p_monsterData.resistances.Clone();
		species = p_monsterData.species;
	}
}
