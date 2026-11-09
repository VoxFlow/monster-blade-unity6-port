public class AfterEnterNameTutorial : Tutorial
{
	private void End()
	{
		HideSubtitle();
		AnalyticsManager.FirstInstallEvents("tut_ochalasttalk");
		Messenger<State>.Invoke("tutorial_completed", State.AfterEnterName);
	}
}
