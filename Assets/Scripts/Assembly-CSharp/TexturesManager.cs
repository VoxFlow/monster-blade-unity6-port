using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class TexturesManager : SingletonComponent<TexturesManager>
{
	public class AtlasData
	{
		public static string m_resourcesRefLoc = "UI/REF_ATLAS/";

		public string m_resourcesLoc = "UI/Atlas/";

		public string name = string.Empty;

		public string category = string.Empty;

		public GameObject atlasRefGAO;

		public UIAtlas realAtlas;

		public string textureName = string.Empty;

		public Texture atlasTexture;

		public bool IsLoaded
		{
			get
			{
				return textureName != string.Empty && atlasTexture != null;
			}
		}

		public AtlasData(string _name, string _category = "DK")
		{
			name = _name;
			category = _category;
		}

		public void Load()
		{
			GetTextureName();
			if (IsLoaded || textureName == string.Empty)
			{
				return;
			}
			string text = m_resourcesLoc + textureName;
			atlasTexture = (Texture)Resources.Load(text, typeof(Texture));
			if (atlasTexture == null && SingletonComponent<TexturesManager>.Instance.m_possibleSubDir != null)
			{
				Dbg.LogWarn("This texture is null : " + text);
				for (int i = 0; i < SingletonComponent<TexturesManager>.Instance.m_possibleSubDir.Length; i++)
				{
					text = m_resourcesLoc + SingletonComponent<TexturesManager>.Instance.m_possibleSubDir[i] + textureName;
					atlasTexture = (Texture)Resources.Load(text, typeof(Texture));
					if (atlasTexture != null)
					{
						break;
					}
					Dbg.LogWarn("This texture is null : " + text);
				}
			}
			if (atlasTexture == null)
			{
				Dbg.LogWarn("This texture is null : " + text);
			}
			realAtlas.spriteMaterial.SetTexture("_MainTex", atlasTexture);
			Dbg.Log("Load : " + text + "\tfrom Atlases : " + atlasRefGAO.name + " \t" + realAtlas.name, Dbg.NBLogType.TEXTURES_MANAGER);
		}

		public void Unload()
		{
			if (IsLoaded)
			{
				Resources.UnloadAsset(atlasTexture);
				Dbg.Log("Unload : " + textureName, Dbg.NBLogType.TEXTURES_MANAGER);
				atlasTexture = null;
			}
		}

		private void GetTextureName()
		{
			if (textureName != string.Empty)
			{
				return;
			}
			string text = m_resourcesRefLoc + name;
			atlasRefGAO = (GameObject)Resources.Load(text, typeof(GameObject));
			if (atlasRefGAO == null)
			{
				Dbg.LogWarn("Fail to load : " + text + " " + category, Dbg.NBLogType.TEXTURES_MANAGER);
				return;
			}
			UIAtlas component = atlasRefGAO.GetComponent<UIAtlas>();
			if (component == null)
			{
				Dbg.LogWarn("This atlas doesn't have a UIAtlas component." + name + " " + category, Dbg.NBLogType.TEXTURES_MANAGER);
				return;
			}
			realAtlas = component.replacement;
			if (realAtlas == null)
			{
				Dbg.LogWarn("This atlas is not a Reference atlas ?" + name + " " + category, Dbg.NBLogType.TEXTURES_MANAGER);
			}
			else
			{
				textureName = ((!(realAtlas != null) || !(realAtlas.texture != null)) ? string.Empty : realAtlas.texture.name);
			}
		}
	}

	private string m_globalGameTag = "GLOBAL_GAME";

	private string m_globalMenuTag = "GLOBAL_MENU";

	private string m_globalBattleTag = "GLOBAL_BATTLE";

	public List<string> m_globalGameAtlas;

	public List<string> m_globalMainMenuAtlas;

	public List<string> m_globalBattleAtlas;

	[SerializeField]
	private string[] m_possibleSubDir;

	public bool printCategories;

	private Dictionary<string, List<AtlasData>> m_allAtlas = new Dictionary<string, List<AtlasData>>();

	private string previousCategory = string.Empty;

	public void Initialize()
	{
		NBProfiler.BeginTask("[TEXTURE_MANAGER][INITIALIZE]");
		Init(ref m_globalGameAtlas, m_globalGameTag);
		Init(ref m_globalMainMenuAtlas, m_globalMenuTag);
		Init(ref m_globalBattleAtlas, m_globalBattleTag);
		LoadCategory(m_globalGameTag, false);
		NBProfiler.EndTask("[TEXTURE_MANAGER][INITIALIZE]");
	}

	public void RegisterAndLoad(GameObject p_gao, string p_category, bool p_override = false)
	{
		SingletonComponent<TexturesManager>.Instance.RegisterCategory(p_gao, p_category, p_override);
		SingletonComponent<TexturesManager>.Instance.LoadCategory(p_category);
	}

	public void RegisterCategory(GameObject _gao, string _category = "", bool _override = false)
	{
		if (_category == string.Empty)
		{
			_category = _gao.name;
		}
		if (m_allAtlas.ContainsKey(_category) && !_override)
		{
			Dbg.Log("This category is already register, it won't be overriden.", Dbg.NBLogType.TEXTURES_MANAGER);
			return;
		}
		List<string> _atlasList = FindAtlasInScreen(_gao);
		Init(ref _atlasList, _category);
		_atlasList.Clear();
		_atlasList = null;
	}

	public void LoadCategory(string _category, bool _unloadPrevious = true)
	{
		if (!m_allAtlas.ContainsKey(_category))
		{
			Dbg.LogWarn("This Category was not registered : " + _category, Dbg.NBLogType.TEXTURES_MANAGER);
			return;
		}
		if (previousCategory == _category)
		{
			Dbg.Log("There is nothing to do, this category is the same as the previous one. " + _category, Dbg.NBLogType.TEXTURES_MANAGER);
			return;
		}
		if (_unloadPrevious && previousCategory != string.Empty)
		{
			UnloadCategory(previousCategory);
		}
		List<AtlasData> list = m_allAtlas[_category];
		foreach (AtlasData item in list)
		{
			item.Load();
		}
		if (_unloadPrevious)
		{
			previousCategory = _category;
		}
	}

	public void UnloadCategory(string _category)
	{
		if (!m_allAtlas.ContainsKey(_category))
		{
			Dbg.LogWarn("This Category was not registered : " + _category, Dbg.NBLogType.TEXTURES_MANAGER);
			return;
		}
		List<AtlasData> list = m_allAtlas[_category];
		foreach (AtlasData item in list)
		{
			item.Unload();
		}
		Resources.UnloadUnusedAssets();
		GC.Collect();
	}

	public void LoadAllMenuAtlas()
	{
		LoadCategory(m_globalMenuTag, false);
	}

	public void LoadAllBattleAtlas()
	{
		LoadCategory(m_globalBattleTag, false);
	}

	public void UnloadAllMenuAtlas()
	{
		UnloadCategory(m_globalMenuTag);
	}

	public void UnloadAllBattleAtlas()
	{
		UnloadCategory(m_globalBattleTag);
	}

	private void Init(ref List<string> _atlasList, string _category)
	{
		if (m_allAtlas.ContainsKey(_category))
		{
			m_allAtlas[_category].Clear();
			m_allAtlas.Remove(_category);
		}
		m_allAtlas.Add(_category, new List<AtlasData>());
		foreach (string _atlas in _atlasList)
		{
			AtlasData item = new AtlasData(_atlas, _category);
			m_allAtlas[_category].Add(item);
		}
	}

	private void PrintAtlasPerCategory()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("--- PrintAtlasPerCategory ---");
		foreach (string key in m_allAtlas.Keys)
		{
			stringBuilder.AppendLine("\n" + key + " Contains :");
			List<AtlasData> list = m_allAtlas[key];
			foreach (AtlasData item in list)
			{
				stringBuilder.AppendLine(item.textureName);
			}
		}
		Dbg.Log(stringBuilder.ToString(), Dbg.NBLogType.TEXTURES_MANAGER);
		stringBuilder = null;
	}

	private bool ContainsInGlobal(string _atlasName)
	{
		return m_globalGameAtlas.Contains(_atlasName) || m_globalMainMenuAtlas.Contains(_atlasName) || m_globalBattleAtlas.Contains(_atlasName);
	}

	private List<string> FindAtlasInScreen(GameObject _obj, bool _ignoreGlobal = true)
	{
		List<string> list = new List<string>();
		if (_obj == null)
		{
			Dbg.LogWarn("The given obj is null. Cannot Find Atlas use in null objects.", Dbg.NBLogType.TEXTURES_MANAGER);
			return list;
		}
		UISprite[] componentsInChildren = MBHier.GetComponentsInChildren<UISprite>(_obj, true);
		UISprite[] array = componentsInChildren;
		foreach (UISprite uISprite in array)
		{
			if (!(uISprite.atlas == null))
			{
				string text = uISprite.atlas.name;
				if ((!_ignoreGlobal || !ContainsInGlobal(text)) && !list.Contains(text))
				{
					list.Add(text);
				}
			}
		}
		return list;
	}

	private new void OnDestroy()
	{
		UnloadCategory(m_globalGameTag);
	}
}
