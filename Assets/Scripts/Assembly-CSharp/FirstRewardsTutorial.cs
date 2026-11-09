public class FirstRewardsTutorial : Tutorial
{
	protected override void OnEnable()
	{
		base.OnEnable();
		Messenger.AddListener("rewards_pet_anim_started", OnPetAnimStarted);
		Messenger.AddListener("rewards_pet_anim_completed", OnPetAnimCompleted);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Messenger.RemoveListener("rewards_pet_anim_started", OnPetAnimStarted);
		Messenger.RemoveListener("rewards_pet_anim_completed", OnPetAnimCompleted);
	}

	public override void Start()
	{
		base.Start();
	}

	private void OnItemAnimCompleted()
	{
	}

	private void OnPetAnimStarted()
	{
		TryAdvance("pet_anim_start");
	}

	private void OnPetAnimCompleted()
	{
		TryAdvance("pet_anim_end");
	}

	public override void ShowSubtitle(Action action)
	{
		base.ShowSubtitle(action);
	}

	private void ShowEggAnimation()
	{
		HideSubtitle();
		DungeonCompletePanel dungeonCompletePanel = MainMenu.Instance.currScreen.currentPanel as DungeonCompletePanel;
		dungeonCompletePanel.Play();
	}

	private void End()
	{
		HideSubtitle();
		Dbg.Log("--- TUTORIAL END --- ");
		Messenger<State>.Invoke("tutorial_completed", State.Rewards);
	}
}
