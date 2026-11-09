public class Nullable
{
	public static implicit operator bool(Nullable o)
	{
		return o != null;
	}
}
