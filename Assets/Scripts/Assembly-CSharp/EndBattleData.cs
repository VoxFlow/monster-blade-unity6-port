using System.Collections.Generic;
using Models;

public class EndBattleData
{
	public string rank = string.Empty;

	public int score;

	public int igc;

	public int exp;

	public float scorePerc;

	public int totalNumBattle;

	public int numBattle = 1;

	public int monsterLevel;

	public int playerLevel;

	public List<ServerInventoryItem> items = new List<ServerInventoryItem>();
}
