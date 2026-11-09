public class BlendAnimSeq_Cutscene_Tutorial_Mob : CutsceneAnimSq
{
	private string[] animationList = new string[3] { "Intro", "Rage", "Idle_01" };

	private float[] sequenceTime = new float[3] { 0f, 2f, 4.3f };

	public override void Awake()
	{
		base.Awake();
		m_animationList = animationList;
		m_sequenceTime = sequenceTime;
	}
}
