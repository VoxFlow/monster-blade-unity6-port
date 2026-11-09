// WWW.bytes of an empty reply as an empty array, as in Unity 4.0.1. Register CD-058.
using UnityEngine;

public static class MBWWW
{
	public static byte[] Bytes(WWW www)
	{
		return www.bytes ?? new byte[0];
	}
}
