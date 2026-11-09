using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;

public class NBCacheManager
{
	public enum EDOWNLOAD_STATUS
	{
		READY = 0,
		ERROR = 1,
		DOWNLOADING = 2
	}

	protected class CallbackInfo
	{
		private DownloadCallback m_pCallback;

		private object m_arg1;

		private object m_arg2;

		public DownloadCallback Callback
		{
			get
			{
				return m_pCallback;
			}
			set
			{
				m_pCallback = value;
			}
		}

		public object Variable1
		{
			get
			{
				return m_arg1;
			}
			set
			{
				m_arg1 = value;
			}
		}

		public object Variable2
		{
			get
			{
				return m_arg2;
			}
			set
			{
				m_arg2 = value;
			}
		}

		public CallbackInfo(DownloadCallback p_callback, object p_arg1, object p_arg2)
		{
			m_pCallback = p_callback;
			m_arg1 = p_arg1;
			m_arg2 = p_arg2;
		}
	}

	protected class NBFileInfo
	{
		private DateTime m_cLastAccess;

		private string m_szDir;

		private string m_szHash;

		private int m_nValidPeriod = 30;

		private EDOWNLOAD_STATUS m_eDownloadStatus;

		private List<CallbackInfo> m_listCallback;

		public DateTime LastAccess
		{
			get
			{
				return m_cLastAccess;
			}
			set
			{
				m_cLastAccess = value;
			}
		}

		public string Hash
		{
			get
			{
				return m_szHash;
			}
			set
			{
				m_szHash = value;
			}
		}

		public int ValidPeriod
		{
			get
			{
				return m_nValidPeriod;
			}
			set
			{
				m_nValidPeriod = value;
			}
		}

		public string Dir
		{
			get
			{
				return m_szDir;
			}
			set
			{
				m_szDir = value;
			}
		}

		public EDOWNLOAD_STATUS Status
		{
			get
			{
				return m_eDownloadStatus;
			}
			set
			{
				m_eDownloadStatus = value;
			}
		}

		public List<CallbackInfo> CallbackList
		{
			get
			{
				return m_listCallback;
			}
			set
			{
				m_listCallback = value;
			}
		}

		public NBFileInfo()
		{
		}

		public NBFileInfo(long p_nLastAccess, string p_szDir, string p_szHash, int p_nValidPeriod)
		{
			m_cLastAccess = DateTime.FromBinary(p_nLastAccess);
			m_szDir = p_szDir;
			m_szHash = p_szHash;
			m_nValidPeriod = p_nValidPeriod;
		}
	}

	public delegate void DownloadCallback(bool p_bSuccess, string p_szError, byte[] p_fileContent, object p_arg1, object p_arg2);

	private Dictionary<string, NBFileInfo> m_dictCache = new Dictionary<string, NBFileInfo>();

	private string CACHE_PATH = UnityEngine.Application.temporaryCachePath + "/Cache/";   // PORT (CD-007)

	private string CACHE_INDEX_NAME = "cacheindex.bin";

	private static NBCacheManager m_Instance = new NBCacheManager();

	public static NBCacheManager Instance
	{
		get
		{
			return m_Instance;
		}
	}

	public bool Initialize()
	{
		if (!LoadCacheIndex())
		{
			m_dictCache = new Dictionary<string, NBFileInfo>();
		}
		else
		{
			List<string> list = new List<string>();
			foreach (KeyValuePair<string, NBFileInfo> item in m_dictCache)
			{
				if (0 > item.Value.LastAccess.AddDays(item.Value.ValidPeriod).CompareTo(DateTime.Now))
				{
					DeleteFile(item.Value.Dir, item.Key);
					list.Add(item.Key);
				}
			}
			foreach (string item2 in list)
			{
				m_dictCache.Remove(item2);
			}
		}
		return true;
	}

	public byte[] GetFileFromCacheOnly(string p_szDir, string p_szFilename)
	{
		NBFileInfo value;
		if (m_dictCache != null && m_dictCache.TryGetValue(p_szFilename, out value) && value.Status == EDOWNLOAD_STATUS.READY)
		{
			byte[] fileContent = GetFileContent(p_szDir + "/", p_szFilename);
			if (fileContent == null)
			{
				return null;
			}
			return fileContent;
		}
		return null;
	}

	public void GetFile(string p_szUrl, string p_szDir, string p_szFilename, string p_szMD5, DownloadCallback p_callback, object p_arg1, object p_arg2, int p_nValidPeriod = 14)
	{
		if (p_szUrl == null || p_szFilename == null)
		{
			Dbg.LogError("[NBCacheManager::GetFile] one or more of the essential arguments is null");
			return;
		}
		if (p_szDir != null && p_szDir.LastIndexOf('/') != p_szDir.Length - 1)
		{
			p_szDir += "/";
		}
		Dbg.Log("[NBCacheManager::GetFile]\n\tUrl: " + p_szUrl + "\n\tDir:" + p_szDir + "\n\tFilename: " + p_szFilename);
		NBFileInfo value;
		if (m_dictCache.TryGetValue(p_szFilename, out value))
		{
			SingletonComponent<JobManager>.Instance.StartCoroutine(RetrieveCache(p_szUrl, p_szDir, p_szFilename, p_szMD5, p_nValidPeriod, value, p_callback, p_arg1, p_arg2));
			return;
		}
		NBFileInfo nBFileInfo = new NBFileInfo(0L, p_szDir, string.Empty, p_nValidPeriod);
		nBFileInfo.Status = EDOWNLOAD_STATUS.DOWNLOADING;
		AddToCallbackList(nBFileInfo, p_callback, p_arg1, p_arg2);
		m_dictCache.Add(p_szFilename, nBFileInfo);
		SaveCacheIndex();
		SingletonComponent<JobManager>.Instance.StartCoroutine(DownloadFile(p_szUrl, p_szDir, p_szFilename, p_szMD5, p_nValidPeriod, nBFileInfo));
	}

	private IEnumerator RetrieveCache(string p_url, string p_dir, string p_filename, string p_szMD5, int p_nValidPeriod, NBFileInfo p_fileInfo, DownloadCallback p_callback, object p_arg1, object p_arg2)
	{
		if (p_fileInfo.Status == EDOWNLOAD_STATUS.READY)
		{
			if (p_szMD5 == null || p_fileInfo.Hash.CompareTo(string.Empty) == 0 || p_szMD5.CompareTo(p_fileInfo.Hash) != 0)
			{
				Dbg.Log("[NBCacheManager::RetrieveCache] hash not given or hash mis-matched. redownloading the file");
				AddToCallbackList(p_fileInfo, p_callback, p_arg1, p_arg2);
				SingletonComponent<JobManager>.Instance.StartCoroutine(DownloadFile(p_url, p_dir, p_filename, p_szMD5, p_nValidPeriod, p_fileInfo));
				yield break;
			}
			byte[] content = GetFileContent(p_dir, p_filename);
			yield return null;
			if (content == null)
			{
				AddToCallbackList(p_fileInfo, p_callback, p_arg1, p_arg2);
				SingletonComponent<JobManager>.Instance.StartCoroutine(DownloadFile(p_url, p_dir, p_filename, p_szMD5, p_nValidPeriod, p_fileInfo));
			}
			else
			{
				p_fileInfo.LastAccess = DateTime.Now;
				Dbg.Log("[NBCacheManager::RetrieveCache] file exists, returning cached version");
				p_callback(true, "Success", content, p_arg1, p_arg2);
			}
		}
		else if (p_fileInfo.Status == EDOWNLOAD_STATUS.DOWNLOADING)
		{
			AddToCallbackList(p_fileInfo, p_callback, p_arg1, p_arg2);
		}
		else
		{
			p_callback(false, "file failed to download", null, p_arg1, p_arg2);
		}
	}

	private static void AddToCallbackList(NBFileInfo p_fileInfo, DownloadCallback p_callback, object p_arg1, object p_arg2)
	{
		if (p_fileInfo != null)
		{
			if (p_fileInfo.CallbackList == null)
			{
				p_fileInfo.CallbackList = new List<CallbackInfo>();
			}
			p_fileInfo.CallbackList.Add(new CallbackInfo(p_callback, p_arg1, p_arg2));
		}
	}

	private string ProcessURL(string p_url)
	{
		int num = p_url.IndexOf("://") + 3;
		string text = p_url.Substring(num).Replace("//", "/");
		string text2 = p_url.Substring(0, num) + text;
		Dbg.Log("New URL : " + text2);
		return text2;
	}

	private IEnumerator DownloadFile(string p_url, string p_dir, string p_filename, string p_szMD5, int p_nValidPeriod, NBFileInfo p_fileInfo)
	{
		if (p_fileInfo != null)
		{
			p_fileInfo.Status = EDOWNLOAD_STATUS.DOWNLOADING;
		}
		string url = ProcessURL(p_url);
		WWW request = new WWW(url);
		if (request == null)
		{
			Docallback(p_fileInfo, false, "unable to create web request", null);
			yield break;
		}
		Dbg.Log("[NBCacheManager::DownloadFile] Downloading: " + url);
		yield return request;
		if (request.error != null)
		{
			Docallback(p_fileInfo, false, request.error, null);
			yield break;
		}
		MD5 hasher = MD5.Create();
		byte[] hash = hasher.ComputeHash(MBWWW.Bytes(request));   // PORT (CD-058)
		string szHash = Services.ByteToString(hash);
		if (p_szMD5 != null && szHash.CompareTo(p_szMD5) != 0)
		{
			Dbg.LogError("Hash for both files is incorrect\nFile hash:" + szHash + "\nServer :" + p_szMD5);
			Docallback(p_fileInfo, false, "Invalid hash", null);
			yield break;
		}
		yield return null;
		WriteFileContent(p_dir, p_filename, MBWWW.Bytes(request));
		yield return null;
		if (p_fileInfo != null)
		{
			p_fileInfo.LastAccess = DateTime.Now;
			p_fileInfo.Dir = p_dir;
			p_fileInfo.Hash = szHash;
			p_fileInfo.ValidPeriod = p_nValidPeriod;
		}
		else
		{
			NBFileInfo fileInfo = new NBFileInfo(DateTime.Now.ToBinary(), p_dir, szHash, p_nValidPeriod);
			m_dictCache.Add(p_filename, fileInfo);
		}
		SaveCacheIndex();
		Docallback(p_fileInfo, true, "Success", MBWWW.Bytes(request));
	}

	private void Docallback(NBFileInfo p_fileInfo, bool p_bSuccess, string p_szMessage, byte[] p_data)
	{
		if (p_bSuccess)
		{
			p_fileInfo.Status = EDOWNLOAD_STATUS.READY;
		}
		else
		{
			p_fileInfo.Status = EDOWNLOAD_STATUS.ERROR;
		}
		if (p_fileInfo.CallbackList == null)
		{
			return;
		}
		foreach (CallbackInfo callback in p_fileInfo.CallbackList)
		{
			callback.Callback(p_bSuccess, p_szMessage, p_data, callback.Variable1, callback.Variable2);
		}
		p_fileInfo.CallbackList = null;
	}

	private void CheckCreateDirectory(string p_szDirectory)
	{
		if (!Directory.Exists(p_szDirectory))
		{
			Dbg.Log("[NBCacheManager::CheckCreateDirectory] Directory " + p_szDirectory + " does not exists, creating.");
			Directory.CreateDirectory(p_szDirectory);
		}
	}

	private byte[] GetFileContent(string p_dir, string p_filename)
	{
		if (p_filename == null)
		{
			return null;
		}
		string text = null;
		if (p_dir != null)
		{
			CheckCreateDirectory(CACHE_PATH + p_dir);
			text = CACHE_PATH + p_dir + p_filename;
		}
		else
		{
			CheckCreateDirectory(CACHE_PATH);
			text = CACHE_PATH + p_filename;
		}
		Dbg.Log("[NBCacheManager::GetFileContent] " + text);
		if (File.Exists(text))
		{
			return File.ReadAllBytes(text);
		}
		return null;
	}

	private bool WriteFileContent(string p_dir, string p_filename, byte[] p_contents)
	{
		if (p_filename == null)
		{
			return false;
		}
		string text = null;
		if (p_dir != null)
		{
			CheckCreateDirectory(CACHE_PATH + p_dir);
			text = CACHE_PATH + p_dir + p_filename;
		}
		else
		{
			CheckCreateDirectory(CACHE_PATH);
			text = CACHE_PATH + p_filename;
		}
		Dbg.Log("[NBCacheManager::WriteFileContent] " + text);
		File.WriteAllBytes(text, p_contents);
		return false;
	}

	private bool DeleteFile(string p_dir, string p_filename)
	{
		if (p_filename == null)
		{
			return false;
		}
		string text = null;
		text = ((p_dir == null) ? (CACHE_PATH + p_filename) : (CACHE_PATH + p_dir + p_filename));
		Dbg.Log("[NBCacheManager::DeleteFile] " + text);
		if (File.Exists(text))
		{
			File.Delete(text);
			return true;
		}
		return false;
	}

	public bool LoadCacheIndex()
	{
		string text = CACHE_PATH + CACHE_INDEX_NAME;
		Dbg.Log("[NBCacheManager::LoadCacheIndex] " + text);
		if (!File.Exists(text))
		{
			return false;
		}
		using (FileStream fileStream = File.OpenRead(text))
		{
			if (fileStream == null)
			{
				return false;
			}
			if (m_dictCache != null)
			{
				m_dictCache.Clear();
			}
			else
			{
				m_dictCache = new Dictionary<string, NBFileInfo>();
			}
			BinaryReader binaryReader = new BinaryReader(fileStream);
			int num = binaryReader.ReadInt32();
			for (int i = 0; num > i; i++)
			{
				string key = binaryReader.ReadString();
				long p_nLastAccess = binaryReader.ReadInt64();
				string p_szDir = binaryReader.ReadString();
				string p_szHash = binaryReader.ReadString();
				int p_nValidPeriod = binaryReader.ReadInt32();
				m_dictCache.Add(key, new NBFileInfo(p_nLastAccess, p_szDir, p_szHash, p_nValidPeriod));
			}
		}
		return true;
	}

	public bool SaveCacheIndex()
	{
		CheckCreateDirectory(CACHE_PATH);
		using (FileStream fileStream = File.OpenWrite(CACHE_PATH + CACHE_INDEX_NAME))
		{
			if (fileStream == null)
			{
				return false;
			}
			BinaryWriter binaryWriter = new BinaryWriter(fileStream);
			binaryWriter.Write(m_dictCache.Count);
			foreach (KeyValuePair<string, NBFileInfo> item in m_dictCache)
			{
				binaryWriter.Write(item.Key);
				binaryWriter.Write(item.Value.LastAccess.ToBinary());
				binaryWriter.Write(item.Value.Dir);
				binaryWriter.Write(item.Value.Hash);
				binaryWriter.Write(item.Value.ValidPeriod);
			}
			binaryWriter.Flush();
		}
		return true;
	}
}
