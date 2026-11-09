public class PlayerAttackParameter
{
	public PlayerAttack.Direction[] directions { get; private set; }

	public float[] damageWindows { get; private set; }

	public float[] windownWindows { get; private set; }

	public float attackEnd { get; private set; }

	public PlayerAttackParameter(PlayerAttack.Direction[] p_directions, float[] p_damageWindows, float[] p_windownWindows, float p_attackEnd)
	{
		directions = p_directions;
		damageWindows = p_damageWindows;
		windownWindows = p_windownWindows;
		attackEnd = p_attackEnd;
	}
}
