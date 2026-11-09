public class PlayerAttack
{
	public enum Type
	{
		None = -1,
		Normal = 0,
		Heavy = 1,
		Ripost = 2,
		Repel = 3,
		Super = 4,
		Special = 5,
		PowerMove = 6,
		Skill = 7,
		Max = 8
	}

	public enum Direction
	{
		None = -1,
		TopToBottom = 0,
		BottomToTop = 1,
		LeftToRight = 2,
		RightToLeft = 3,
		TopLeftToBottomRight = 4,
		TopRightToBottomLeft = 5,
		BottomLeftToTopRight = 6,
		BottomRightToTopLeft = 7,
		Max = 8
	}

	public static float[,] blendTimes = new float[8, 8]
	{
		{ 0.24f, 0.09f, 0.18f, 0.18f, 0.21f, 0.21f, 0.15f, 0.15f },
		{ 0.09f, 0.24f, 0.18f, 0.18f, 0.15f, 0.15f, 0.21f, 0.21f },
		{ 0.18f, 0.18f, 0.24f, 0.09f, 0.21f, 0.15f, 0.21f, 0.15f },
		{ 0.18f, 0.18f, 0.09f, 0.24f, 0.15f, 0.21f, 0.15f, 0.21f },
		{ 0.21f, 0.15f, 0.21f, 0.15f, 0.24f, 0.18f, 0.18f, 0.09f },
		{ 0.21f, 0.15f, 0.15f, 0.21f, 0.18f, 0.24f, 0.09f, 0.18f },
		{ 0.15f, 0.21f, 0.21f, 0.15f, 0.18f, 0.09f, 0.24f, 0.18f },
		{ 0.15f, 0.21f, 0.15f, 0.21f, 0.09f, 0.18f, 0.18f, 0.24f }
	};

	public Type type;

	public Direction direction;

	public float multiplier = 1f;

	public bool forceCleanHit;

	public bool interuptAnimation;

	public string animation = string.Empty;

	public int comboIndex;

	public static bool IsFigureOfEight(Direction p_dir1, Direction p_dir2)
	{
		return p_dir1 != p_dir2;
	}

	public bool CanCritical()
	{
		return type == Type.Normal || type == Type.Heavy || type == Type.PowerMove || type == Type.Super;
	}

	public bool DoesMonsterNeedToReact()
	{
		return (type == Type.Ripost || type == Type.Super || type == Type.Special) && direction != Direction.None;
	}

	public static Direction GetDirectionFromString(string p_string)
	{
		string[] array = new string[8] { "TopToBottom", "BottomToTop", "LeftToRight", "RightToLeft", "TopLeftToBottomRight", "TopRightToBottomLeft", "BottomLeftToTopRight", "BottomRightToTopLeft" };
		for (int i = 0; i < array.Length; i++)
		{
			if (string.Compare(array[i], p_string, true) == 0)
			{
				return (Direction)i;
			}
		}
		return Direction.None;
	}

	public override string ToString()
	{
		return string.Format("[PlayerAttack Type={0}, Direction={1}, Multiplier={2}, Force Clean Hit={3}, Animation={4}]", type, direction, multiplier, forceCleanHit, animation);
	}
}
