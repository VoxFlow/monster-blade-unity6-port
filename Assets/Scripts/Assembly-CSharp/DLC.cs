using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Models;
using UnityEngine;

public class DLC : SingletonComponent<DLC>
{
	public enum Type
	{
		NormalFile = 0,
		AssetBundleFile = 1,
		Max = 2
	}

	public enum AssetBundleLocation
	{
		PatchServer = 0,
		StreamingAssets = 1,
		Max = 2
	}

	public class DlcItem
	{
		private struct Callbacks
		{
			public WWWCallback successCb;

			public WWWCallback failCb;

			public Callbacks(WWWCallback _successCb, WWWCallback _failCb)
			{
				successCb = _successCb;
				failCb = _failCb;
			}
		}

		private Type m_type;

		private string m_filename = string.Empty;

		private string m_url = string.Empty;

		private int m_version;

		private bool m_isLoaded;

		private BundleDownloadHandle m_bundle;

		private FileStream m_file;

		private List<int> m_subscribers = new List<int>();

		private List<string> m_subscribersId = new List<string>();

		private List<Callbacks> m_callbacks = new List<Callbacks>();

		private int m_nRefCount;

		private bool m_unloadAllAssets = true;

		public BundleDownloadHandle BundleHandle
		{
			get
			{
				return m_bundle;
			}
		}

		public FileStream FileHandle
		{
			get
			{
				return m_file;
			}
		}

		public List<int> Subscribers
		{
			get
			{
				return m_subscribers;
			}
		}

		public List<string> SubscribersId
		{
			get
			{
				return m_subscribersId;
			}
		}

		public string Filename
		{
			get
			{
				return m_filename;
			}
			set
			{
				m_filename = value;
			}
		}

		public int Version
		{
			get
			{
				return m_version;
			}
			set
			{
				m_version = value;
			}
		}

		public string URL
		{
			get
			{
				return m_url;
			}
			set
			{
				m_url = value;
			}
		}

		public DlcItem(string p_filename, string p_url, int p_version = 0)
		{
			m_filename = p_filename;
			m_version = p_version;
			m_url = p_url;
			if (DLC.IsAssetBundle(p_filename))
			{
				m_type = Type.AssetBundleFile;
			}
			else
			{
				m_type = Type.NormalFile;
			}
		}

		public void Load(WWWCallback p_successCallback = null, WWWCallback p_failCallback = null)
		{
			m_nRefCount++;
			if (m_isLoaded)
			{
				return;
			}
			if (0 < m_callbacks.Count)
			{
				m_callbacks.Add(new Callbacks(p_successCallback, p_failCallback));
				return;
			}
			string text = CacheAssetsBasePath + m_filename;
			if (!File.Exists(text))
			{
				text = AppAssetsBasePath + m_filename;
			}
			if (m_type == Type.AssetBundleFile)
			{
				m_callbacks.Add(new Callbacks(p_successCallback, p_failCallback));
				m_bundle = SingletonComponent<DLC>.Instance.DownloadAssetBundle(text, m_version, m_filename, InternalSuccessCallback, InternalFailCallback);
			}
			else
			{
				m_file = new FileStream(text, FileMode.Open);
				m_isLoaded = true;
			}
		}

		public void Unload(bool _allAssets)
		{
			if (m_unloadAllAssets)
			{
				m_unloadAllAssets = _allAssets;
			}
			if (m_nRefCount == 0 || --m_nRefCount != 0)
			{
				return;
			}
			if (m_type == Type.AssetBundleFile)
			{
				if (m_bundle != null)
				{
					m_bundle.UnloadAssets(m_unloadAllAssets);
				}
				m_bundle = null;
				m_isLoaded = false;
				return;
			}
			if (m_file != null)
			{
				m_file.Close();
				m_file = null;
				m_isLoaded = false;
			}
			ClearAllSubscribers();
			m_unloadAllAssets = true;
		}

		public bool IsAssetBundle()
		{
			return m_type == Type.AssetBundleFile;
		}

		public bool IsLoaded()
		{
			return m_isLoaded;
		}

		public void ClearSubscribers()
		{
			m_subscribers.Clear();
		}

		public void ClearSubscribersId()
		{
			m_subscribersId.Clear();
		}

		public void ClearAllSubscribers()
		{
			ClearSubscribers();
			ClearSubscribersId();
		}

		public int FindInSubscribers(GameObject p_go)
		{
			if (p_go == null)
			{
				return -1;
			}
			int instanceID = p_go.GetInstanceID();
			for (int i = 0; i < m_subscribers.Count; i++)
			{
				if (instanceID == m_subscribers[i])
				{
					return i;
				}
			}
			return -1;
		}

		public int FindInSubscribers(string p_uniqueId)
		{
			for (int i = 0; i < m_subscribersId.Count; i++)
			{
				if (p_uniqueId == m_subscribersId[i])
				{
					return i;
				}
			}
			return -1;
		}

		public bool IsSubscribed(GameObject p_go)
		{
			return FindInSubscribers(p_go) >= 0;
		}

		public bool IsSubscribed(string p_uniqueId)
		{
			return FindInSubscribers(p_uniqueId) >= 0;
		}

		public bool HasSubscribers()
		{
			return m_subscribers.Count > 0 || m_subscribersId.Count > 0;
		}

		public void Subscribe(GameObject p_go)
		{
			if (!IsSubscribed(p_go) && !(p_go == null))
			{
				m_subscribers.Add(p_go.GetInstanceID());
			}
		}

		public void Subscribe(string p_uniqueId)
		{
			if (!IsSubscribed(p_uniqueId))
			{
				m_subscribersId.Add(p_uniqueId);
			}
		}

		public void Unsubscribe(int p_instanceId)
		{
			m_subscribers.Remove(p_instanceId);
		}

		public void Unsubscribe(GameObject p_go)
		{
			if (IsSubscribed(p_go))
			{
				int index = FindInSubscribers(p_go.GetInstanceID().ToString());
				m_subscribers.RemoveAt(index);
			}
		}

		public void Unsubscribe(string p_uniqueId)
		{
			if (IsSubscribed(p_uniqueId))
			{
				int index = FindInSubscribers(p_uniqueId);
				m_subscribersId.RemoveAt(index);
			}
		}

		private void InternalSuccessCallback(BundleDownloadHandle p_handle)
		{
			m_isLoaded = true;
			List<Callbacks> callbacks = m_callbacks;
			m_callbacks = new List<Callbacks>();
			foreach (Callbacks item in callbacks)
			{
				if (item.successCb != null)
				{
					item.successCb(m_bundle);
				}
			}
			callbacks.Clear();
		}

		private void InternalFailCallback(BundleDownloadHandle p_handle)
		{
			if (m_isLoaded)
			{
				m_isLoaded = false;
				m_bundle.UnloadAssets();
				m_bundle = null;
			}
			List<Callbacks> callbacks = m_callbacks;
			m_callbacks = new List<Callbacks>();
			foreach (Callbacks item in callbacks)
			{
				if (item.failCb != null)
				{
					item.failCb(p_handle);
				}
			}
			callbacks.Clear();
		}
	}

	private List<PatchFileInfo> m_downloadList = new List<PatchFileInfo>();

	private List<PatchFileInfo> m_assetBundleList = new List<PatchFileInfo>();

	private Dictionary<string, DlcItem> m_dlcs = new Dictionary<string, DlcItem>();

	private VoidCallback m_onSuccess;

	private VoidCallback m_onError;

	private int m_filesDownloadedCount;

	private int m_fileCount;

	private List<BundleDownloadHandle> m_filesDownloading = new List<BundleDownloadHandle>();

	private int m_filesErrorCount;

	public int m_simultaneousDownloads = 5;

	public AssetBundleLocation m_bundleLocation;

	public static string CacheAssetsBasePath
	{
		get
		{
			return UnityEngine.Application.temporaryCachePath + "/";   // PORT (CD-007)
		}
	}

	public static string AppAssetsBasePath
	{
		get
		{
			return Application.streamingAssetsPath + "/";
		}
	}

	public int FilesDownloadedCount
	{
		get
		{
			return m_filesDownloadedCount;
		}
	}

	public int FilesToDownloadCount
	{
		get
		{
			return m_fileCount;
		}
	}

	private static byte[] GetBytes(string p_stringToConvert)
	{
		byte[] array = new byte[p_stringToConvert.Length * 2];
		Buffer.BlockCopy(p_stringToConvert.ToCharArray(), 0, array, 0, array.Length);
		return array;
	}

	private static string GetString(byte[] p_bytesToConvert)
	{
		char[] array = new char[p_bytesToConvert.Length / 2];
		Buffer.BlockCopy(p_bytesToConvert, 0, array, 0, p_bytesToConvert.Length);
		return new string(array);
	}

	private void SaveFileListToCache()
	{
		string cacheAssetsBasePath = CacheAssetsBasePath;
		string path = cacheAssetsBasePath + "saving.lst";
		if (!Directory.Exists(cacheAssetsBasePath))
		{
			Directory.CreateDirectory(cacheAssetsBasePath);
		}
		StreamWriter streamWriter = File.CreateText(path);
		foreach (KeyValuePair<string, DlcItem> dlc in m_dlcs)
		{
			DlcItem value = dlc.Value;
			streamWriter.WriteLine(value.Filename + "^" + value.URL + "^" + value.Version);
		}
		streamWriter.Close();
	}

	public bool CompareAndDownload(List<PatchFileInfo> p_patchList, VoidCallback p_onSuccess, VoidCallback p_onError)
	{
		m_downloadList.Clear();
		m_assetBundleList.Clear();
		m_onSuccess = p_onSuccess;
		m_onError = p_onError;
		m_filesDownloadedCount = 0;
		m_fileCount = 0;
		StartCoroutine(CompareAndDownloadCoroutine(p_patchList));
		return true;
	}

	private IEnumerator CompareAndDownloadCoroutine(List<PatchFileInfo> p_patchList)
	{
		Dictionary<string, DlcItem> dlcsOld = m_dlcs;
		m_dlcs = new Dictionary<string, DlcItem>();
		List<string> filesInApp = new List<string>();
		yield return StartCoroutine(GetBundleListAsync(filesInApp));
		yield return null;
		float start = Time.realtimeSinceStartup;
		foreach (string file in filesInApp)
		{
			string filename = GetRelativePath(file, AppAssetsBasePath);
			DlcItem item;
			if (!dlcsOld.TryGetValue(filename, out item))
			{
				item = new DlcItem(filename, file);
			}
			m_dlcs.Add(filename, item);
			float t = Time.realtimeSinceStartup;
			if (start + 0.016f < t)
			{
				start = t;
				yield return null;
			}
		}
		if (p_patchList.Count <= 0)
		{
			yield break;
		}
		foreach (PatchFileInfo patchItem in p_patchList)
		{
			if (!m_dlcs.ContainsKey(patchItem.filename))
			{
				DlcItem item2;
				if (dlcsOld.TryGetValue(patchItem.filename, out item2))
				{
					item2.URL = patchItem.url;
					item2.Version = int.Parse(patchItem.version);
				}
				else
				{
					item2 = new DlcItem(patchItem.filename, patchItem.url, int.Parse(patchItem.version));
				}
				m_dlcs.Add(patchItem.filename, item2);
			}
			else
			{
				DlcItem existingItem = m_dlcs[patchItem.filename];
				existingItem.URL = patchItem.url;
				existingItem.Version = int.Parse(patchItem.version);
			}
			string fullpathInCache = CacheAssetsBasePath + patchItem.filename;
			if (!File.Exists(fullpathInCache) || !IsCrcMatch(fullpathInCache, patchItem.checksum))
			{
				if (IsAssetBundle(patchItem.filename))
				{
					m_assetBundleList.Add(patchItem);
				}
				else
				{
					m_downloadList.Add(patchItem);
				}
			}
			float t2 = Time.realtimeSinceStartup;
			if (start + 0.016f < t2)
			{
				start = t2;
				yield return null;
			}
		}
		m_fileCount = m_downloadList.Count + m_assetBundleList.Count;
		if (m_fileCount == 0)
		{
			if (m_onSuccess != null)
			{
				m_onSuccess();
			}
		}
		else
		{
			yield return null;
			SaveFileListToCache();
			yield return StartCoroutine(PerformDownloadProcedure());
		}
	}

	private IEnumerator PerformDownloadProcedure()
	{
		m_filesDownloading.Clear();
		m_filesErrorCount = 0;
		while (m_fileCount > m_filesDownloadedCount && m_filesErrorCount <= 0)
		{
			if (m_filesDownloading.Count < m_simultaneousDownloads)
			{
				if (m_downloadList.Count > 0)
				{
					PatchFileInfo patchItem = m_downloadList[0];
					m_downloadList.RemoveAt(0);
					m_filesDownloading.Add(DownloadNormalAssets(patchItem.url, patchItem.filename, InternalOnDownloadSuccess, InternalOnDownloadFail));
				}
				else if (m_assetBundleList.Count > 0)
				{
					PatchFileInfo patchItem2 = m_assetBundleList[0];
					m_assetBundleList.RemoveAt(0);
					m_filesDownloading.Add(DownloadNormalAssets(patchItem2.url, patchItem2.filename, InternalOnDownloadSuccess, InternalOnDownloadFail));
				}
			}
			yield return null;
		}
		if (m_filesErrorCount > 0)
		{
			while (m_filesDownloading.Count > 0)
			{
				yield return new MBWaitForSeconds(2f);   // PORT (CD-059)
			}
			if (m_onError != null)
			{
				m_onError();
				m_onError = null;
			}
		}
	}

	public BundleDownloadHandle GetAssetBundle(GameObject p_requestor, string p_assetBundleKey, WWWCallback p_successCallback = null, WWWCallback p_failCallback = null)
	{
		DlcItem value = null;
		if (!m_dlcs.TryGetValue(p_assetBundleKey, out value))
		{
			return null;
		}
		if (!value.IsAssetBundle())
		{
			return null;
		}
		if (!value.IsSubscribed(p_requestor))
		{
			value.Subscribe(p_requestor);
		}
		if (value.BundleHandle != null && value.BundleHandle.state == BundleDownloadHandle.DownloadState.Success)
		{
			if (p_successCallback != null)
			{
				p_successCallback(value.BundleHandle);
			}
			return value.BundleHandle;
		}
		if (!value.IsLoaded())
		{
			value.Load(p_successCallback, p_failCallback);
			return value.BundleHandle;
		}
		if (p_successCallback != null)
		{
			p_successCallback(value.BundleHandle);
		}
		return value.BundleHandle;
	}

	public BundleDownloadHandle GetAssetBundle(string p_uId, string p_assetBundleKey, WWWCallback p_successCallback = null, WWWCallback p_failCallback = null)
	{
		DlcItem value = null;
		if (!m_dlcs.TryGetValue(p_assetBundleKey, out value))
		{
			return null;
		}
		if (!value.IsAssetBundle())
		{
			return null;
		}
		if (!value.IsSubscribed(p_uId))
		{
			value.Subscribe(p_uId);
		}
		if (value.BundleHandle != null && value.BundleHandle.state == BundleDownloadHandle.DownloadState.Success)
		{
			if (p_successCallback != null)
			{
				p_successCallback(value.BundleHandle);
			}
			return value.BundleHandle;
		}
		if (!value.IsLoaded())
		{
			value.Load(p_successCallback, p_failCallback);
			return value.BundleHandle;
		}
		if (p_successCallback != null)
		{
			p_successCallback(value.BundleHandle);
		}
		return value.BundleHandle;
	}

	public List<string> GetAssetBundleList(string p_pattern)
	{
		List<string> list = new List<string>();
		foreach (string key in m_dlcs.Keys)
		{
			if (key.Contains(p_pattern))
			{
				list.Add(key);
			}
		}
		return list;
	}

	public string GetAssetBundle(string _pattern)
	{
		foreach (string key in m_dlcs.Keys)
		{
			if (key.Contains(_pattern))
			{
				return key;
			}
		}
		return string.Empty;
	}

	public string GetCharacterBase(string _pattern)
	{
		string value = _pattern.ToLower();
		foreach (string key in m_dlcs.Keys)
		{
			string text = key.ToLower();
			if (text.Contains(value))
			{
				return key;
			}
		}
		return string.Empty;
	}

	public string GetFile(GameObject p_requestor, string p_fileKey)
	{
		byte[] fileData = GetFileData(p_requestor, p_fileKey);
		if (fileData == null)
		{
			return null;
		}
		return Encoding.UTF8.GetString(fileData);
	}

	public byte[] GetFileData(GameObject p_requestor, string p_fileKey)
	{
		DlcItem value = null;
		if (!m_dlcs.TryGetValue(p_fileKey, out value))
		{
			return null;
		}
		if (value.IsAssetBundle())
		{
			return null;
		}
		if (p_requestor != null && !value.IsSubscribed(p_requestor))
		{
			value.Subscribe(p_requestor);
		}
		if (!value.IsLoaded())
		{
			value.Load();
		}
		byte[] array = new byte[value.FileHandle.Length];
		value.FileHandle.Read(array, 0, (int)value.FileHandle.Length);
		return array;
	}

	public void Unload(GameObject p_requestor, string p_key, bool _allAssets = true)
	{
		DlcItem value = null;
		if (m_dlcs.TryGetValue(p_key, out value))
		{
			if (p_requestor != null && value.IsSubscribed(p_requestor))
			{
				value.Unsubscribe(p_requestor);
			}
			if (!value.HasSubscribers())
			{
				value.Unload(_allAssets);
			}
		}
	}

	public void Unload(string p_uniqueId, string p_key, bool _allAssets = true)
	{
		DlcItem value = null;
		if (m_dlcs.TryGetValue(p_key, out value))
		{
			if (p_uniqueId != null && value.IsSubscribed(p_uniqueId))
			{
				value.Unsubscribe(p_uniqueId);
			}
			if (!value.HasSubscribers())
			{
				value.Unload(_allAssets);
			}
		}
	}

	public void UnloadAll()
	{
		foreach (KeyValuePair<string, DlcItem> dlc in m_dlcs)
		{
			DlcItem value = dlc.Value;
			value.Unload(true);
		}
		GC.Collect();
		Resources.UnloadUnusedAssets();
	}

	public void CheckForCleanUp()
	{
		UnityEngine.Object[] p_objects = UnityEngine.Object.FindObjectsOfType(typeof(GameObject));
		List<DlcItem> list = new List<DlcItem>();
		foreach (KeyValuePair<string, DlcItem> dlc in m_dlcs)
		{
			DlcItem value = dlc.Value;
			if (value.Subscribers.Count <= 0)
			{
				continue;
			}
			List<int> list2 = new List<int>();
			foreach (int subscriber in value.Subscribers)
			{
				GameObject gameObject = (GameObject)FindObjectInCollection(p_objects, subscriber);
				if (gameObject == null)
				{
					list2.Add(subscriber);
					list.Add(value);
				}
			}
			foreach (int item in list2)
			{
				value.Unsubscribe(item);
			}
			list2 = null;
		}
		foreach (DlcItem item2 in list)
		{
			if (item2.Subscribers.Count < 0)
			{
				item2.Unload(true);
			}
		}
	}

	private UnityEngine.Object FindObjectInCollection(UnityEngine.Object[] p_objects, int p_instanceId)
	{
		if (p_objects == null)
		{
			return null;
		}
		foreach (UnityEngine.Object obj in p_objects)
		{
			if (p_instanceId == obj.GetInstanceID())
			{
				return obj;
			}
		}
		return null;
	}

	private BundleDownloadHandle DownloadAssetBundle(string p_url, string p_version, string p_filename, WWWCallback p_onCompleteCallback, WWWCallback p_onErrorCallback)
	{
		BundleDownloadHandle bundleDownloadHandle = new BundleDownloadHandle(p_url, int.Parse(p_version), p_filename, p_onCompleteCallback, p_onErrorCallback);
		StartCoroutine(bundleDownloadHandle.DownloadAssetBundle());
		return bundleDownloadHandle;
	}

	private BundleDownloadHandle DownloadAssetBundle(string p_url, int p_version, string p_filename, WWWCallback p_onCompleteCallback, WWWCallback p_onErrorCallback)
	{
		BundleDownloadHandle bundleDownloadHandle = new BundleDownloadHandle(p_url, p_version, p_filename, p_onCompleteCallback, p_onErrorCallback);
		StartCoroutine(bundleDownloadHandle.DownloadAssetBundle());
		return bundleDownloadHandle;
	}

	private BundleDownloadHandle DownloadNormalAssets(string p_url, string p_filename, WWWCallback p_onCompleteCallback, WWWCallback p_onErrorCallback)
	{
		BundleDownloadHandle bundleDownloadHandle = new BundleDownloadHandle(p_url, 0, p_filename, p_onCompleteCallback, p_onErrorCallback);
		StartCoroutine(bundleDownloadHandle.DownloadAndCache());
		return bundleDownloadHandle;
	}

	public static bool IsCrcMatch(string p_filepath, string p_crc)
	{
		string text = GenerateCrc(p_filepath);
		return 0 == text.CompareTo(p_crc.ToLower());
	}

	public static string GenerateCrc(string p_filepath)
	{
		string empty = string.Empty;
		using (FileStream p_fs = File.Open(p_filepath, FileMode.Open))
		{
			return Services.ByteToString(Crc32.ComputeHash(p_fs));
		}
	}

	public static bool IsAssetBundle(string p_filename)
	{
		return p_filename.EndsWith(".assetbundle", StringComparison.CurrentCultureIgnoreCase);
	}

	public static string TrimFilename(string p_fullfilename)
	{
		int num = p_fullfilename.LastIndexOf('/');
		if (num < 0)
		{
			return p_fullfilename;
		}
		return p_fullfilename.Substring(0, num + 1);
	}

	private void InternalOnDownloadSuccess(BundleDownloadHandle p_handle)
	{
		m_filesDownloadedCount++;
		m_filesDownloading.Remove(p_handle);
		if (m_onSuccess != null)
		{
			m_onSuccess();
		}
	}

	private void InternalOnDownloadFail(BundleDownloadHandle p_handle)
	{
		m_filesDownloading.Remove(p_handle);
		m_filesErrorCount++;
	}

	public static void ClearCache()
	{
		List<string> filesInDirectory = GetFilesInDirectory(CacheAssetsBasePath);
		foreach (string item in filesInDirectory)
		{
			File.Delete(item);
		}
		Caching.ClearCache();
	}

	private static List<string> GetFilesInDirectory(string p_directory, bool p_relative = false, string p_pattern = "*.*")
	{
		Dbg.Log("DLC.GetFilesInDirectory. Directory: " + p_directory);
		List<string> list = new List<string>();
		string[] files = Directory.GetFiles(p_directory, p_pattern, SearchOption.AllDirectories);
		for (int i = 0; i < files.Length; i++)
		{
			if (!files[i].Contains(".meta") && !files[i].Contains(".svn") && !files[i].Contains(".DS_Store"))
			{
				files[i] = files[i].Replace('\\', '/');
				list.Add(files[i]);
			}
		}
		Dbg.Log("DLC.GetFilesInDirectory. Return: " + list);
		return list;
	}

	private static List<string> GetBundleList()
	{
		string path = "Utils/assetbundleInfo";
		TextAsset textAsset = Resources.Load(path, typeof(TextAsset)) as TextAsset;
		List<string> list = new List<string>();
		if (textAsset != null)
		{
			StringReader stringReader = new StringReader(textAsset.text);
			for (string text = stringReader.ReadLine(); text != null; text = stringReader.ReadLine())
			{
				list.Add(text);
			}
		}
		return list;
	}

	private IEnumerator GetBundleListAsync(List<string> bundleList)
	{
		string bundleListPath = "Utils/assetbundleInfo";
		TextAsset bundleListAsset = Resources.Load(bundleListPath, typeof(TextAsset)) as TextAsset;
		yield return null;
		float start = Time.realtimeSinceStartup;
		if (!(bundleListAsset != null))
		{
			yield break;
		}
		StringReader reader = new StringReader(bundleListAsset.text);
		for (string file = reader.ReadLine(); file != null; file = reader.ReadLine())
		{
			bundleList.Add(file);
			float t = Time.realtimeSinceStartup;
			if (start + 0.016f < t)
			{
				start = t;
				yield return null;
			}
		}
	}

	public static string GetRelativePath(string p_fullFile, string p_fromDirectory)
	{
		p_fullFile = p_fullFile.Replace('\\', '/');
		p_fromDirectory = p_fromDirectory.Replace('\\', '/');
		int num = p_fullFile.IndexOf(p_fromDirectory);
		if (num >= 0 && p_fromDirectory.Length < p_fullFile.Length)
		{
			p_fullFile = p_fullFile.Remove(num, p_fromDirectory.Length);
		}
		return p_fullFile;
	}
}
