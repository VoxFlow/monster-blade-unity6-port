// Seeds the client's download cache with the original server patch files, bytes unchanged. Register CD-006.
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class OfflinePatchSeed
{
	public struct Entry
	{
		public string filename;
		public string url;
		public string version;
		public long size;
		public string crc;
	}

	private static string SourceRoot
	{
		get { return Path.Combine(Application.streamingAssetsPath, "OfflineServer/cache"); }
	}

	private static string Crc(string path)
	{
		using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
		{
			return Services.ByteToString(Crc32.ComputeHash(fs));
		}
	}

	public static List<Entry> SeedAndList()
	{
		List<Entry> list = new List<Entry>();
		string lst = Path.Combine(SourceRoot, "saving.lst");
		if (!File.Exists(lst))
		{
			Debug.LogError("[OfflineServer] seed list missing: " + lst);
			return list;
		}
		int copied = 0;
		foreach (string line in File.ReadAllLines(lst))
		{
			string[] parts = line.Split('^');
			if (parts.Length != 3 || !parts[1].StartsWith("http"))
			{
				continue;
			}
			string src = Path.Combine(SourceRoot, parts[0]);
			if (!File.Exists(src))
			{
				Debug.LogError("[OfflineServer] seed file missing: " + parts[0]);
				continue;
			}
			string dst = DLC.CacheAssetsBasePath + parts[0];
			string srcCrc = Crc(src);
			if (!File.Exists(dst) || Crc(dst) != srcCrc)
			{
				Directory.CreateDirectory(Path.GetDirectoryName(dst));
				File.Copy(src, dst, true);
				copied++;
			}
			list.Add(new Entry
			{
				filename = parts[0],
				url = parts[1],
				version = parts[2],
				size = new FileInfo(dst).Length,
				crc = srcCrc
			});
		}
		Debug.Log("[OfflineServer] patch seed: " + list.Count + " files listed, " + copied + " copied to " + DLC.CacheAssetsBasePath);
		return list;
	}
}
