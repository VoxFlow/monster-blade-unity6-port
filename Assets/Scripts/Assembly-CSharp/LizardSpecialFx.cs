public class LizardSpecialFx : MonsterSpecialFx
{
	private void OnStumbleDown()
	{
		if (m_monster.CurrentState != Monster.State.Cutscene)
		{
			ShakeCamera("Drop");
		}
	}
}
