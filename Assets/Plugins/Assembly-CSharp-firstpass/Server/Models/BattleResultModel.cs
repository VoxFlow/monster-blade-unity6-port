namespace Server.Models
{
	public sealed class BattleResultModel : IObjectModel
	{
		public int level;

		public int xp;

		public int xpGain;

		public int igcGain;

		public int gachaPtGain;

		public int criteriaGain;

		public int levelGain;

		public FishSpotModel area;

		public uint templateId;

		public uint newAreaId;

		public int score;
	}
}
