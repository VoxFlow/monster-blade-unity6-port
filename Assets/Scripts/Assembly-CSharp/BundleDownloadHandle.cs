using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class BundleDownloadHandle
{
	public enum DownloadState
	{
		InProgress = 0,
		Failure = 1,
		Success = 2
	}

	public WWWCallback onComplete;

	public WWWCallback onError;

	public DownloadState state;

	public int version;

	public string url = string.Empty;

	public string filename = string.Empty;

	public WWW www;

	public float loadingTime;

	private static int s_nAssetBundleCount = 0;

	private static int s_nMaxAssetBundle = 1;

	public static event Action<string> OnDownloadStart;

	public static event Action<string> OnDownloadEnd;

	public BundleDownloadHandle(string p_url, int p_version, string p_filename, WWWCallback p_onComplete, WWWCallback p_onError)
	{
		url = p_url;
		version = p_version;
		filename = p_filename;
		onComplete = p_onComplete;
		onError = p_onError;
		if (www != null)
		{
			www.Dispose();
			www = null;
		}
	}

	static BundleDownloadHandle()
	{
		BundleDownloadHandle.OnDownloadStart = delegate
		{
		};
		BundleDownloadHandle.OnDownloadEnd = delegate
		{
		};
	}

	public object LoadAssetSync(string p_assetName)
	{
		return www.assetBundle.LoadAsset(p_assetName);
	}

	public object LoadAssetSync(string p_assetName, Type p_type)
	{
		return www.assetBundle.LoadAsset(p_assetName, p_type);
	}

	public UnityEngine.Object[] LoadAll(Type _type = null)
	{
		if (_type == null)
		{
			return www.assetBundle.LoadAllAssets();
		}
		return www.assetBundle.LoadAllAssets(_type);
	}

	public object LoadMainAsset()
	{
		object main = www.assetBundle.mainAsset;
		string name;
		Type type;
		if (main == null && BundleMainAssets.For(url, out name, out type))
		{
			// PORT (CD-009)
			main = www.assetBundle.LoadAsset(name, type);
		}
		return main;
	}

	public void LoadAssetAsync(string p_assetName, Type p_type, LoaderCallback p_onLoad)
	{
		SingletonComponent<DLC>.Instance.StartCoroutine(_LoadAssetAsync(p_assetName, p_type, p_onLoad));
	}

	protected IEnumerator _LoadAssetAsync(string p_assetName, Type p_type, LoaderCallback p_onLoad)
	{
		if (www != null && www.assetBundle != null)
		{
			AssetBundleRequest request = www.assetBundle.LoadAssetAsync(p_assetName, p_type);
			yield return request;
			if (p_onLoad != null)
			{
				p_onLoad(request.asset);
			}
		}
		else if (p_onLoad != null)
		{
			p_onLoad(null);
		}
	}

	public void UnloadAssets(bool _allAssets = true)
	{
		SingletonComponent<DLC>.Instance.StartCoroutine(UnloadBundle(_allAssets));
	}

	private IEnumerator UnloadBundle(bool _allAssets = true)
	{
		if (www != null)
		{
			if (www.assetBundle != null)
			{
				www.assetBundle.Unload(_allAssets);
			}
			while (!www.isDone)
			{
				yield return null;
			}
			www.Dispose();
			www = null;
			Dbg.Assert(0 < s_nAssetBundleCount);
			s_nAssetBundleCount--;
		}
	}

	public IEnumerator DownloadAssetBundle()
	{
		while (s_nMaxAssetBundle <= s_nAssetBundleCount)
		{
			yield return null;
		}
		s_nAssetBundleCount++;
		if (BundleDownloadHandle.OnDownloadStart != null)
		{
			BundleDownloadHandle.OnDownloadStart(url);
		}
		loadingTime = Time.time;
		string dlUrl = url;
		if (dlUrl.IndexOf("file://") < 0 && File.Exists(dlUrl))
		{
			// PORT (CD-010)
			dlUrl = new Uri(Path.GetFullPath(dlUrl)).AbsoluteUri;
		}
		else if (dlUrl.IndexOf("file://") < 0)
		{
			int index = dlUrl.IndexOf(":");
			if (index >= 0)
			{
				dlUrl = dlUrl.Remove(0, index + 1);
			}
			dlUrl = "file://" + dlUrl;
		}
		www = new WWW(dlUrl);
		// PORT (CD-011)
		yield return null;
		while (!www.isDone)
		{
			yield return null;
		}
		loadingTime = Time.time - loadingTime;
		if (www.error != null)
		{
			Dbg.LogError("[BundleDownloadHandle] load failed url=" + dlUrl + " error=" + www.error);
			s_nAssetBundleCount--;
			state = DownloadState.Failure;
			if (onError != null)
			{
				onError(this);
			}
			yield break;
		}
		if (www.assetBundle == null)
		{
			Dbg.LogError("[BundleDownloadHandle] load failed url=" + dlUrl + " assetBundle=null bytes=" + (www.bytes != null ? www.bytes.Length : -1));
			s_nAssetBundleCount--;
			state = DownloadState.Failure;
			if (onError != null)
			{
				onError(this);
			}
			yield break;
		}
		state = DownloadState.Success;
		if (onComplete != null)
		{
			onComplete(this);
		}
		if (BundleDownloadHandle.OnDownloadEnd != null)
		{
			BundleDownloadHandle.OnDownloadEnd(url);
		}
	}

	public IEnumerator DownloadAndCache()
	{
		if (BundleDownloadHandle.OnDownloadStart != null)
		{
			BundleDownloadHandle.OnDownloadStart(url);
		}
		loadingTime = Time.time;
		www = new WWW(url);
		while (!www.isDone)
		{
			yield return null;
		}
		loadingTime = Time.time - loadingTime;
		if (www.error != null)
		{
			state = DownloadState.Failure;
			if (onError != null)
			{
				onError(this);
			}
			yield break;
		}
		if (MBWWW.Bytes(www) == null)   // PORT (CD-058)
		{
			state = DownloadState.Failure;
			if (onError != null)
			{
				onError(this);
			}
			yield break;
		}
		string pathInCache = DLC.CacheAssetsBasePath + DLC.TrimFilename(filename);
		if (!Directory.Exists(pathInCache))
		{
			Directory.CreateDirectory(pathInCache);
		}
		File.WriteAllBytes(DLC.CacheAssetsBasePath + filename, MBWWW.Bytes(www));
		state = DownloadState.Success;
		if (onComplete != null)
		{
			onComplete(this);
		}
		if (BundleDownloadHandle.OnDownloadEnd != null)
		{
			BundleDownloadHandle.OnDownloadEnd(url);
		}
	}
}
