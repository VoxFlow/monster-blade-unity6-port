// Pins the invariant culture at start-up, as the original Android runtime selected no culture. Register CD-057.
using System;
using System.Globalization;
using System.Threading;
using UnityEngine;

public static class MBCulture
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	private static void Pin()
	{
		Set(CultureInfo.InvariantCulture);
	}

	private static void Set(CultureInfo c)
	{
		CultureInfo.DefaultThreadCurrentCulture = c;
		CultureInfo.DefaultThreadCurrentUICulture = c;
		Thread.CurrentThread.CurrentCulture = c;
		Thread.CurrentThread.CurrentUICulture = c;
	}
}
