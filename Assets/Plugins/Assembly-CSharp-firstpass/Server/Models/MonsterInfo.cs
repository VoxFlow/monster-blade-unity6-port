namespace Server.Models
{
	public sealed class MonsterInfo : IObjectModel
	{
		public uint id;

		public string name;

		public int rarity;

		public int type;

		public int element;

		public float warnDuration;

		public float stunDuration;

		public int attackMin;

		public int attackMax;

		public float intervalMin;

		public float intervalMax;

		public float lengthMin;

		public float lengthMax;

		public int hpMin;

		public int hpMax;

		public float speedMin;

		public float speedMax;

		public float weightMin;

		public float weightMax;

		public int xpMin;

		public int xpMax;

		public int KingAppearanceDuration;

		public int timeLeft;
	}
}
