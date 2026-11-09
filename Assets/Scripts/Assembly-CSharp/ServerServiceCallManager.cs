using System;

public class ServerServiceCallManager
{
	public static event Action<string> OnServiceCallStart;

	public static event Action<string> OnServiceCallEnd;

	static ServerServiceCallManager()
	{
		ServerServiceCallManager.OnServiceCallStart = delegate
		{
		};
		ServerServiceCallManager.OnServiceCallEnd = delegate
		{
		};
	}
}
