public class DungeonEventStruct : DungeonDbStruct
{
	public string categoryRef = string.Empty;

	public EventEnum.Type type;

	public int xpMultiplier = 1;

	public string imageUrl = string.Empty;

	public string imageMD5 = string.Empty;

	public bool visited;

	public bool cleared;

	public long endTime;
}
