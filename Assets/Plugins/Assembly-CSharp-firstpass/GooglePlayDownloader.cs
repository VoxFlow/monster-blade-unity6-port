using System;
using System.IO;
using UnityEngine;

public class GooglePlayDownloader
{
	private const string Environment_MEDIA_MOUNTED = "mounted";

	private static AndroidJavaClass detectAndroidJNI;

	private static AndroidJavaClass Environment;

	private static string publicKey;

	public static string obb_package { get; private set; }

	public static int obb_version { get; private set; }

	static GooglePlayDownloader()
	{
		publicKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAlXu+EOuSF0WCVBQjrjKtGl0SsljaUq7BIrlkckPoAzjOYvjJSkwNwn/xpkU891ZhzEFiiTxU10JuoViStKljwynu8SeLQ+rXFmMIaAY1SeP2F3hvXoTCUba1Pvx3IaU4Xjqp+XwX6KkJy/E8MNkcjydglOMpjCYfqcYtkSwd9CObzLQVFZbQIKrt30jwOxtxv5Y1Nzm8fZAvCDDWwhN2Vg3l9D/S3vR5/cIXEqtDu50hlg2krbz0yvT3T5+iswKZv0I+Y+hfoZvcMWdE9cN0cchGnHuQFOcmZOSVFU6lGx4fQ7uDAoOsTIyVtzVxQ/ZeKY/WVkdYyOvajOd9uPHv4QIDAQAB";
		if (RunningOnAndroid())
		{
			Environment = new AndroidJavaClass("android.os.Environment");
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.plugin.downloader.UnityDownloaderService"))
			{
				androidJavaClass.SetStatic("BASE64_PUBLIC_KEY", publicKey);
				androidJavaClass.SetStatic("SALT", new byte[20]
				{
					1, 43, 244, 255, 54, 98, 156, 244, 43, 2,
					248, 252, 9, 5, 150, 148, 223, 45, 255, 84
				});
			}
			obb_package = string.Empty;
			obb_version = 0;
			populateOBBData();
		}
	}

	public static bool RunningOnAndroid()
	{
		if (detectAndroidJNI == null)
		{
			detectAndroidJNI = new AndroidJavaClass("android.os.Build");
		}
		return detectAndroidJNI.GetRawClass() != IntPtr.Zero;
	}

	public static string GetExpansionFilePath()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		if (Environment.CallStatic<string>("getExternalStorageState", new object[0]) != "mounted")
		{
			return null;
		}
		using (AndroidJavaObject androidJavaObject = Environment.CallStatic<AndroidJavaObject>("getExternalStorageDirectory", new object[0]))
		{
			string arg = androidJavaObject.Call<string>("getPath", new object[0]);
			return string.Format("{0}/{1}/{2}", arg, "Android/obb", obb_package);
		}
#else
		return Application.dataPath;
#endif
	}

	public static string GetMainOBBPath(string expansionFilePath)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		if (expansionFilePath == null)
		{
			return null;
		}
		string text = string.Format("{0}/main.{1}.{2}.obb", expansionFilePath, obb_version, obb_package);
		if (!File.Exists(text))
		{
			return null;
		}
		return text;
#else
		return expansionFilePath;
#endif
	}

	public static string GetPatchOBBPath(string expansionFilePath)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		if (expansionFilePath == null)
		{
			return null;
		}
		string text = string.Format("{0}/patch.{1}.{2}.obb", expansionFilePath, obb_version, obb_package);
		if (!File.Exists(text))
		{
			return null;
		}
		return text;
#else
		return null;
#endif
	}

	public static void FetchOBB()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
		{
			AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
			AndroidJavaObject androidJavaObject2 = new AndroidJavaObject("android.content.Intent", androidJavaObject, new AndroidJavaClass("com.unity3d.plugin.downloader.UnityDownloaderActivity"));
			int num = 65536;
			androidJavaObject2.Call<AndroidJavaObject>("addFlags", new object[1] { num });
			androidJavaObject2.Call<AndroidJavaObject>("putExtra", new object[2]
			{
				"unityplayer.Activity",
				androidJavaObject.Call<AndroidJavaObject>("getClass", new object[0]).Call<string>("getName", new object[0])
			});
			androidJavaObject.Call("startActivity", androidJavaObject2);
			if (AndroidJNI.ExceptionOccurred() != IntPtr.Zero)
			{
				Debug.LogError("Exception occurred while attempting to start DownloaderActivity - is the AndroidManifest.xml incorrect?");
				AndroidJNI.ExceptionDescribe();
				AndroidJNI.ExceptionClear();
			}
		}
#else
#endif
	}

	private static void populateOBBData()
	{
		if (obb_version != 0)
		{
			return;
		}
		using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
		{
			AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
			obb_package = androidJavaObject.Call<string>("getPackageName", new object[0]);
			AndroidJavaObject androidJavaObject2 = androidJavaObject.Call<AndroidJavaObject>("getPackageManager", new object[0]).Call<AndroidJavaObject>("getPackageInfo", new object[2] { obb_package, 0 });
			obb_version = androidJavaObject2.Get<int>("versionCode");
		}
	}
}
