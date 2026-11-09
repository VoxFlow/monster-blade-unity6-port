// The game's PlayerPrefs, stored in a JSON file under the local application data folder with the original typed contract.
// Register CD-054.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public static class MBPrefs
{
	private enum Kind { Int, Float, String }

	private struct Entry
	{
		public Kind kind;
		public int i;
		public float f;
		public string s;
	}

	private static Dictionary<string, Entry> s_prefs;
	private static bool s_dirty;
	private static string s_path;
	private static string s_source = "";

	public static string FilePath
	{
		get
		{
			if (s_path == null)
			{
				string dir = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Application.companyName), Application.productName);
				s_path = Path.Combine(dir, "PlayerPrefs.json");
			}
			return s_path;
		}
	}

	private static Dictionary<string, Entry> Prefs
	{
		get
		{
			if (s_prefs == null) Load();
			return s_prefs;
		}
	}

	public static bool HasKey(string key) { return Prefs.ContainsKey(key); }

	public static int GetInt(string key) { return GetInt(key, 0); }
	public static int GetInt(string key, int defaultValue)
	{
		Entry e;
		return Prefs.TryGetValue(key, out e) && e.kind == Kind.Int ? e.i : defaultValue;
	}

	public static float GetFloat(string key) { return GetFloat(key, 0f); }
	public static float GetFloat(string key, float defaultValue)
	{
		Entry e;
		return Prefs.TryGetValue(key, out e) && e.kind == Kind.Float ? e.f : defaultValue;
	}

	public static string GetString(string key) { return GetString(key, ""); }
	public static string GetString(string key, string defaultValue)
	{
		Entry e;
		return Prefs.TryGetValue(key, out e) && e.kind == Kind.String ? e.s : defaultValue;
	}

	public static void SetInt(string key, int value) { Put(key, new Entry { kind = Kind.Int, i = value }); }
	public static void SetFloat(string key, float value) { Put(key, new Entry { kind = Kind.Float, f = value }); }
	public static void SetString(string key, string value) { Put(key, new Entry { kind = Kind.String, s = value ?? "" }); }

	public static void DeleteKey(string key)
	{
		if (Prefs.Remove(key)) s_dirty = true;
	}

	public static void DeleteAll()
	{
		Prefs.Clear();
		s_dirty = true;
	}

	public static void Save()
	{
		if (s_prefs == null || !s_dirty) return;
		var sb = new StringBuilder();
		sb.Append("{\n  \"format\": \"MBPrefs 1\",\n  \"source\": ").Append(Quote(s_source)).Append(",\n  \"prefs\": {");
		var keys = new List<string>(s_prefs.Keys);
		keys.Sort(StringComparer.Ordinal);
		for (int k = 0; k < keys.Count; k++)
		{
			Entry e = s_prefs[keys[k]];
			sb.Append(k == 0 ? "\n    " : ",\n    ").Append(Quote(keys[k])).Append(": {");
			if (e.kind == Kind.Int) sb.Append("\"int\": ").Append(e.i.ToString(CultureInfo.InvariantCulture));
			else if (e.kind == Kind.String) sb.Append("\"string\": ").Append(Quote(e.s));
			else if (float.IsNaN(e.f) || float.IsInfinity(e.f)) sb.Append("\"float\": ").Append(Quote(e.f.ToString("R", CultureInfo.InvariantCulture)));
			else sb.Append("\"float\": ").Append(e.f.ToString("R", CultureInfo.InvariantCulture));
			sb.Append("}");
		}
		sb.Append(keys.Count == 0 ? "}\n}\n" : "\n  }\n}\n");
		try
		{
			string path = FilePath, tmp = path + ".tmp";
			Directory.CreateDirectory(Path.GetDirectoryName(path));
			File.WriteAllText(tmp, sb.ToString(), new UTF8Encoding(false));
			if (File.Exists(path)) File.Replace(tmp, path, path + ".bak");
			else File.Move(tmp, path);
			s_dirty = false;
		}
		catch (Exception ex)
		{
			Debug.LogError("[MBPrefs] save failed: " + ex.Message);
		}
	}

	private static void Put(string key, Entry e)
	{
		if (key == null) throw new ArgumentNullException("key");
		Prefs[key] = e;
		s_dirty = true;
	}

	private static void Load()
	{
		s_prefs = new Dictionary<string, Entry>();
		string path = FilePath;
		if (File.Exists(path) || File.Exists(path + ".bak"))
		{
			if (File.Exists(path) && Read(path)) return;
			if (File.Exists(path + ".bak") && Read(path + ".bak"))
			{
				if (File.Exists(path))
				{
					Debug.LogWarning("[MBPrefs] loaded the previous copy " + path + ".bak");
					try { File.Delete(path); } catch (Exception) { }
				}
				s_dirty = true;
				return;
			}
			Debug.LogError("[MBPrefs] no readable copy of " + path + "; starting empty");
			return;
		}
		int n = ImportRegistry();
		s_source = n >= 0 ? "migrated " + n + " values from HKCU\\Software\\" + Application.companyName + "\\" + Application.productName + " on " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) : "new";
		s_dirty = true;
		Save();
		Debug.Log("[MBPrefs] " + FilePath + ": " + s_source);
	}

	private static bool Read(string path)
	{
		var read = new Dictionary<string, Entry>();
		string source = "";
		try
		{
			var root = (Dictionary<string, object>)new Json(File.ReadAllText(path, Encoding.UTF8)).Value();
			object fmt, src;
			if (!root.TryGetValue("format", out fmt) || (fmt as string) != "MBPrefs 1") throw new FormatException("format is not \"MBPrefs 1\"");
			if (root.TryGetValue("source", out src)) source = src as string ?? "";
			foreach (var kv in (Dictionary<string, object>)root["prefs"])
			{
				var v = (Dictionary<string, object>)kv.Value;
				object x;
				if (v.TryGetValue("int", out x)) read[kv.Key] = new Entry { kind = Kind.Int, i = checked((int)(double)x) };
				else if (v.TryGetValue("string", out x)) read[kv.Key] = new Entry { kind = Kind.String, s = (string)x };
				else if (v.TryGetValue("float", out x)) read[kv.Key] = new Entry { kind = Kind.Float, f = x is string ? float.Parse((string)x, CultureInfo.InvariantCulture) : (float)(double)x };
				else throw new FormatException("value of " + kv.Key + " has no int, float or string");
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("[MBPrefs] " + path + " unreadable (" + ex.Message + "); kept as .corrupt");
			try { File.Copy(path, path + ".corrupt", true); } catch (Exception) { }
			return false;
		}
		s_prefs = read;
		s_source = source;
		return true;
	}

	private static uint Hash(string key)
	{
		uint h = 5381;
		foreach (byte c in Encoding.UTF8.GetBytes(key)) h = (h * 33) ^ c;
		return h;
	}

	private static bool EngineOwned(string key)
	{
		return key.StartsWith("Screenmanager ", StringComparison.Ordinal) || key.StartsWith("unity.", StringComparison.Ordinal)
			|| key.StartsWith("unity_connect.", StringComparison.Ordinal) || key == "UnitySelectMonitor";
	}

	private static int ImportRegistry()
	{
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
		IntPtr hkey;
		if (RegOpenKeyExW(new IntPtr(unchecked((int)0x80000001)), "Software\\" + Application.companyName + "\\" + Application.productName, 0, 0x20019, out hkey) != 0)
			return -1;
		int imported = 0;
		try
		{
			var name = new StringBuilder(16384);
			var data = new byte[1 << 20];
			for (uint index = 0; ; index++)
			{
				name.Length = 0;
				uint nameLen = (uint)name.Capacity, type, dataLen = (uint)data.Length;
				int r = RegEnumValueW(hkey, index, name, ref nameLen, IntPtr.Zero, out type, data, ref dataLen);
				if (r == 259) break;
				if (r != 0) { Debug.LogWarning("[MBPrefs] registry value " + index + ": error " + r); continue; }
				string raw = name.ToString(0, (int)nameLen);
				int h = raw.LastIndexOf("_h", StringComparison.Ordinal);
				uint hash;
				if (h < 0 || !uint.TryParse(raw.Substring(h + 2), NumberStyles.None, CultureInfo.InvariantCulture, out hash) || Hash(raw.Substring(0, h)) != hash)
				{
					Debug.LogWarning("[MBPrefs] registry value not written by PlayerPrefs, skipped: " + raw);
					continue;
				}
				string key = raw.Substring(0, h);
				if (EngineOwned(key)) continue;
				if (type == 4 && dataLen == 4) s_prefs[key] = new Entry { kind = Kind.Int, i = BitConverter.ToInt32(data, 0) };
				else if ((type == 4 || type == 11) && dataLen == 8) s_prefs[key] = new Entry { kind = Kind.Float, f = (float)BitConverter.ToDouble(data, 0) };
				else if (type == 3)
				{
					int len = (int)dataLen;
					if (len > 0 && data[len - 1] == 0) len--;
					s_prefs[key] = new Entry { kind = Kind.String, s = Encoding.UTF8.GetString(data, 0, len) };
				}
				else { Debug.LogWarning("[MBPrefs] registry value of type " + type + ", " + dataLen + " bytes, skipped: " + raw); continue; }
				imported++;
			}
		}
		finally { RegCloseKey(hkey); }
		return imported;
#else
		return -1;
#endif
	}

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
	[DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
	private static extern int RegOpenKeyExW(IntPtr hKey, string subKey, uint options, int samDesired, out IntPtr result);

	[DllImport("advapi32.dll", CharSet = CharSet.Unicode)]
	private static extern int RegEnumValueW(IntPtr hKey, uint index, StringBuilder name, ref uint nameLen, IntPtr reserved, out uint type, byte[] data, ref uint dataLen);

	[DllImport("advapi32.dll")]
	private static extern int RegCloseKey(IntPtr hKey);
#endif

	private static string Quote(string s)
	{
		var sb = new StringBuilder(s.Length + 2).Append('"');
		foreach (char c in s)
		{
			if (c == '"' || c == '\\') sb.Append('\\').Append(c);
			else if (c < 0x20) sb.Append("\\u").Append(((int)c).ToString("x4"));
			else sb.Append(c);
		}
		return sb.Append('"').ToString();
	}

	private class Json
	{
		private readonly string t;
		private int p;
		public Json(string text) { t = text; }

		public object Value()
		{
			Ws();
			char c = t[p];
			if (c == '{')
			{
				var o = new Dictionary<string, object>();
				p++; Ws();
				if (t[p] == '}') { p++; return o; }
				while (true)
				{
					Ws(); string k = Str(); Ws(); Expect(':');
					o[k] = Value(); Ws();
					if (t[p] == ',') { p++; continue; }
					Expect('}'); return o;
				}
			}
			if (c == '[')
			{
				var a = new List<object>();
				p++; Ws();
				if (t[p] == ']') { p++; return a; }
				while (true)
				{
					a.Add(Value()); Ws();
					if (t[p] == ',') { p++; continue; }
					Expect(']'); return a;
				}
			}
			if (c == '"') return Str();
			if (t.Substring(p).StartsWith("true")) { p += 4; return true; }
			if (t.Substring(p).StartsWith("false")) { p += 5; return false; }
			if (t.Substring(p).StartsWith("null")) { p += 4; return null; }
			int s = p;
			while (p < t.Length && "+-0123456789.eE".IndexOf(t[p]) >= 0) p++;
			return double.Parse(t.Substring(s, p - s), NumberStyles.Float, CultureInfo.InvariantCulture);
		}

		private string Str()
		{
			Expect('"');
			var sb = new StringBuilder();
			while (t[p] != '"')
			{
				char c = t[p++];
				if (c != '\\') { sb.Append(c); continue; }
				c = t[p++];
				switch (c)
				{
					case 'n': sb.Append('\n'); break;
					case 'r': sb.Append('\r'); break;
					case 't': sb.Append('\t'); break;
					case 'b': sb.Append('\b'); break;
					case 'f': sb.Append('\f'); break;
					case 'u': sb.Append((char)int.Parse(t.Substring(p, 4), NumberStyles.HexNumber)); p += 4; break;
					default: sb.Append(c); break;
				}
			}
			p++;
			return sb.ToString();
		}

		private void Ws() { while (p < t.Length && char.IsWhiteSpace(t[p])) p++; }

		private void Expect(char c)
		{
			if (t[p] != c) throw new FormatException("expected '" + c + "' at " + p);
			p++;
		}
	}
}

public class MBPrefsFlush : MonoBehaviour
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Boot()
	{
		GameObject go = new GameObject("MBPrefsFlush");
		go.hideFlags = HideFlags.HideAndDontSave;
		DontDestroyOnLoad(go);
		go.AddComponent<MBPrefsFlush>();
	}

	private void OnApplicationPause(bool paused)
	{
		if (paused) MBPrefs.Save();
	}

	private void OnApplicationQuit()
	{
		MBPrefs.Save();
	}
}
