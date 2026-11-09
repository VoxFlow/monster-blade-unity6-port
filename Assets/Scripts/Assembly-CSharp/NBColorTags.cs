using UnityEngine;

public class NBColorTags : MonoBehaviour
{
	public const string TAG_YELLOW = "[FFCC00]";

	public const string TAG_PURPLE = "[FF00FF]";

	public const string TAG_LIGHTBLUE = "[5998FF]";

	public static string TAG_END = "[-]";

	public static string TAG_BLUE = "[0000FF]";

	public static string TAG_GREEN = "[00FF00]";

	public static string TAG_RED = "[FF0000]";

	public static string TAG_WHITE = "[FFFFFF]";

	public string ColorToTag(Color col)
	{
		int num = Mathf.FloorToInt(col.r * 255f);
		int num2 = Mathf.FloorToInt(col.g * 255f);
		int num3 = Mathf.FloorToInt(col.b * 255f);
		return "[" + num.ToString("X2") + num2.ToString("X2") + num3.ToString("X2");
	}
}
