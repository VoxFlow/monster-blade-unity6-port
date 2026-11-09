using System.Runtime.InteropServices;
using UnityEngine;

public class ChannelTexture
{
	private const string PLATFORM_DLL = "theorawrapper";

	public Texture2D tex;

	public ChannelTexture(int width, int height, TextureFormat format)
	{
		tex = new Texture2D(width, height, format, false);
		tex.wrapMode = TextureWrapMode.Clamp;
		ReAlloc();
	}

	[DllImport("theorawrapper")]
	private static extern void ReAllocTexture(int handle, int x, int y);

	public void ReAlloc()
	{
		ReAllocTexture(tex.GetNativeTextureID(), tex.width, tex.height);
	}

	public void Destroy()
	{
		Object.Destroy(tex);
	}
}
