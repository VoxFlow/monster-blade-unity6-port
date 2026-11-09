public class BattleStruct
{
	public string dungeonRef = string.Empty;

	public bool isBoss;

	public string monsterId = string.Empty;

	public int battleNumber;

	public Element.Type monsterElement = Element.Type.Metal;

	public float playerLife;

	public ItemEnum.Chest itemChest;

	public int lootExp;

	public BattleStruct(BattleStruct p_data)
	{
		dungeonRef = p_data.dungeonRef;
		isBoss = p_data.isBoss;
		monsterId = p_data.monsterId;
		battleNumber = p_data.battleNumber;
		monsterElement = p_data.monsterElement;
		playerLife = p_data.playerLife;
		itemChest = p_data.itemChest;
		lootExp = p_data.lootExp;
	}

	public BattleStruct()
	{
	}
}
