using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Nubee/Localization")]
public class NBLocalization : SingletonComponent<NBLocalization>
{
	public TextAsset configText;

	public static string[] s_supportedLanguage = new string[5] { "English", "Chinese_s", "Chinese_t", "Japanese", "Korean" };

	private UIFont m_currentFont;

	private Dictionary<string, string> m_textAssetInfoPartial = new Dictionary<string, string>();

	private Dictionary<string, string> m_textAssetInfo = new Dictionary<string, string>();

	private Dictionary<string, string> m_fontInfo = new Dictionary<string, string>();

	private Dictionary<string, string> m_dictionary = new Dictionary<string, string>();

	public UIFont currentFont
	{
		get
		{
			return m_currentFont;
		}
	}

	public void InitializeLocalization()
	{
		ByteReader byteReader = new ByteReader(configText);
		char[] separator = new char[1] { '^' };
		while (byteReader.canRead)
		{
			string text = byteReader.ReadLine();
			if (text == null)
			{
				break;
			}
			string[] array = text.Split(separator, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length == 4)
			{
				m_textAssetInfoPartial.Add(array[0].Trim(), array[1].Trim());
				m_textAssetInfo.Add(array[0].Trim(), array[2].Trim());
				m_fontInfo.Add(array[0].Trim(), array[3].Trim());
			}
		}
	}

	public void LoadPartialLanguage(string p_language)
	{
		string path = m_fontInfo[p_language];
		LoadFontAtlas(path);
		string text = m_textAssetInfoPartial[p_language];
		text = text.Replace(p_language, p_language);
		m_dictionary.Clear();
		Load(text);
	}

	private void LoadFontAtlas(string path)
	{
		GameObject gameObject = (GameObject)Resources.Load(path, typeof(GameObject));
		m_currentFont = gameObject.GetComponent<UIFont>();
		if (Debug.isDebugBuild && m_currentFont == null)
		{
			Dbg.LogError(path + " is a Invalid Font Atlas name!");
		}
	}

	public bool SetCurrentLanguage(string languageKey)
	{
		NBProfiler.BeginTask("[NB_LOCALIZATION][SET_CURRENT_LANGUAGE]");
		if (m_textAssetInfo.ContainsKey(languageKey))
		{
			m_dictionary.Clear();
			string text = m_textAssetInfo[languageKey];
			text = text.Replace("Resources/", DLC.CacheAssetsBasePath);
			string path = m_fontInfo[languageKey];
			LoadFromDLC(text);
			LoadFontAtlas(path);
			NBProfiler.EndTask("[NB_LOCALIZATION][SET_CURRENT_LANGUAGE]");
			return true;
		}
		if (Debug.isDebugBuild)
		{
			Dbg.LogError(languageKey + " is a Invaild Language name!");
		}
		NBProfiler.EndTask("[NB_LOCALIZATION][SET_CURRENT_LANGUAGE]");
		return false;
	}

	private void Load(string path)
	{
		TextAsset asset = (TextAsset)Resources.Load(path, typeof(TextAsset));
		ByteReader byteReader = new ByteReader(asset);
		m_dictionary = byteReader.ReadDictionary('^');
		NGUITools.Broadcast("OnLocalize", this);
	}

	private void LoadFromDLC(string p_path, GameObject p_requestor = null)
	{
		char[] separator = new char[2] { '\r', '\n' };
		string file = SingletonComponent<DLC>.Instance.GetFile(p_requestor, p_path);
		if (file == null)
		{
			Dbg.LogWarn("Localization file is empty, sticking with the partial");
			return;
		}
		List<string> list = new List<string>(file.Split(separator, StringSplitOptions.RemoveEmptyEntries));
		m_dictionary = new Dictionary<string, string>();
		char[] separator2 = new char[1] { '^' };
		foreach (string item in list)
		{
			string[] array = item.Split(separator2, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length == 2)
			{
				string key = array[0].Trim();
				string value = array[1].Trim();
				m_dictionary[key] = value;
			}
		}
		NGUITools.Broadcast("OnLocalize", this);
	}

	public string Get(string p_key)
	{
		if (m_dictionary == null || p_key == null)
		{
			return p_key;
		}
		string value;
		if (m_dictionary.TryGetValue(p_key, out value))
		{
			return value;
		}
		return p_key;
	}

	public void SetLabelKey(UILabel p_label, string p_key)
	{
		NBUILocalize component = p_label.GetComponent<NBUILocalize>();
		if (component != null)
		{
			component.Key = p_key;
		}
		else
		{
			p_label.text = p_key;
		}
	}

	public List<string> GetKeysContaining(string p_partialKey)
	{
		List<string> list = new List<string>();
		foreach (string key in m_dictionary.Keys)
		{
			if (key.Contains(p_partialKey))
			{
				list.Add(key);
			}
		}
		return list;
	}
}
