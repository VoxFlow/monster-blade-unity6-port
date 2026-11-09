public class TutorialMonster : Monster
{
	public bool IsTutorialMode = true;

	protected override void Awake()
	{
		base.Awake();
		InitIdleAnimationList();
	}

	protected override void Start()
	{
		base.Start();
		RandomIdleAnim(0.3f, true);
	}

	protected override void Update()
	{
	}

	protected override void FixedUpdate()
	{
	}

	protected override void OnBattleStart()
	{
		base.enabled = true;
		Idle();
	}
}
