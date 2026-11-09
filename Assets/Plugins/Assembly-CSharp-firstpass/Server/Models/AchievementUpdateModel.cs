namespace Server.Models
{
	public sealed class AchievementUpdateModel : IObjectModel
	{
		public uint taskId;

		public int status;

		public int currentStage;

		public int targetCount;

		public int currentCount;

		public GeneralRewardModel[] rewards;
	}
}
