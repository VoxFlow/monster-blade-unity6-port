using System;
using UnityEngine;

public class AssetStream
{
	private static string lastZipFilePath;

	private static AndroidJavaObject cachedZipFile;

	public static bool GetZipFileOffsetLength(string zipFilePath, string fileName, out long offset, out long length)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		offset = 0L;
		length = 0L;
		AndroidJavaObject androidJavaObject3;
		if (zipFilePath.EndsWith("apk"))
		{
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
			{
				using (AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity"))
				{
					using (AndroidJavaObject androidJavaObject2 = androidJavaObject.Call<AndroidJavaObject>("getAssets", new object[0]))
					{
						androidJavaObject3 = androidJavaObject2.Call<AndroidJavaObject>("openFd", new object[1] { fileName });
					}
				}
			}
		}
		else
		{
			if (lastZipFilePath != zipFilePath)
			{
				lastZipFilePath = zipFilePath;
				if (cachedZipFile != null)
				{
					cachedZipFile.Dispose();
					cachedZipFile = null;
				}
				cachedZipFile = new AndroidJavaObject("com.android.vending.expansion.zipfile.ZipResourceFile", zipFilePath);
			}
			androidJavaObject3 = cachedZipFile.Call<AndroidJavaObject>("getAssetFileDescriptor", new object[1] { "assets/" + fileName });
		}
		if (androidJavaObject3 != null && androidJavaObject3.GetRawObject() != IntPtr.Zero)
		{
			offset = androidJavaObject3.Call<long>("getStartOffset", new object[0]);
			length = androidJavaObject3.Call<long>("getLength", new object[0]);
			androidJavaObject3.Dispose();
			androidJavaObject3 = null;
			return true;
		}
		Debug.LogError("Couldn't find file: " + fileName + " in: " + zipFilePath);
		return false;
#else
		offset = default(long);
		length = default(long);
		return false;
#endif
	}
}
