using System;

public class PowerMove
{
	public enum MoveEnum
	{
		None = -1,
		EagleClaw = 0,
		PhoenixTalon = 1,
		ViperStrike = 2,
		Max = 3
	}

	public enum Action
	{
		None = -1,
		DodgeLeft = 0,
		DodgeRight = 1,
		Block = 2,
		SlashRight = 3,
		SlashLeft = 4,
		SlashDown = 5,
		SlashUp = 6,
		SlashUpRight = 7,
		SlashUpLeft = 8,
		SlashDownRight = 9,
		SlashDownLeft = 10
	}

	public static readonly Action[][] commands = new Action[3][]
	{
		new Action[3]
		{
			Action.DodgeLeft,
			Action.DodgeLeft,
			Action.SlashUpRight
		},
		new Action[3]
		{
			Action.Block,
			Action.Block,
			Action.SlashUp
		},
		new Action[3]
		{
			Action.DodgeRight,
			Action.DodgeRight,
			Action.SlashDown
		}
	};

	public static readonly string[] moves = new string[3] { "Att_Special_01", "Att_Special_02", "Att_Special_03" };

	public static readonly float[] dmgMultipliers = new float[3] { 0.5f, 1.5f, 0.19f };

	public static readonly float[] staminaCosts = new float[3] { 0.35f, 0.61f, 0.35f };

	public static readonly PlayerAttack.Direction[] attackDirections = new PlayerAttack.Direction[3]
	{
		PlayerAttack.Direction.BottomRightToTopLeft,
		PlayerAttack.Direction.BottomToTop,
		PlayerAttack.Direction.BottomToTop
	};

	public static Action GetActionFromDirection(PlayerAttack.Direction p_dir)
	{
		Action[] array = new Action[8]
		{
			Action.SlashDown,
			Action.SlashUp,
			Action.SlashRight,
			Action.SlashLeft,
			Action.SlashDownRight,
			Action.SlashDownLeft,
			Action.SlashUpRight,
			Action.SlashUpLeft
		};
		return array[(int)p_dir];
	}

	public static MoveEnum GetMoveEnum(string p_anim)
	{
		int num = Array.IndexOf(moves, p_anim);
		if (num >= 0)
		{
			return (MoveEnum)num;
		}
		return MoveEnum.None;
	}
}
