public class WingDragonBattle : BattleLogic
{
	protected override void OnBattleReady()
	{
		base.OnBattleReady();
		DungeonManager instance = SingletonComponent<DungeonManager>.Instance;
		if (null != instance && instance.CurrentDungeon != null && instance.CurrentDungeon.difficulty <= 1 && !instance.IsPlayingEventDungeon)
		{
			SingletonComponent<TutorialManager>.Instance.CreateTutorial(Tutorial.State.WingDragon);
		}
	}
}
