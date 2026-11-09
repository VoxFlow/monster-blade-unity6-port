namespace Models
{
	public class ServerDungeonEvent
	{
		public string id = string.Empty;

		public string name = string.Empty;

		public Element.Type element = Element.Type.Metal;

		public int difficulty = 1;

		public int min = 1;

		public int max = 1;

		public int igcReward;

		public int staminaCost;

		public string category = string.Empty;

		public EventEnum.Type type;

		public bool is_cleared;

		public bool is_new;
	}
}
