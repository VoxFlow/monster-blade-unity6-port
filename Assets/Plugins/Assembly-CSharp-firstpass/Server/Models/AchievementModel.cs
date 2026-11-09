namespace Server.Models
{
	public sealed class AchievementModel : IObjectModel
	{
		public uint taskId;

		public int status;

		public int stageCount;

		public int currentStage;

		public int targetCount;

		public int currentCount;

		public GeneralRewardModel[] rewards;

		public int actionType;

		public int targetRarity;

		public int targetElement;

		public int targetClassId;

		public uint targetTemplateId;

		public int targetType;

		public int targetLevel;
	}
}
