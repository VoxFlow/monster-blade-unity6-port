using System.Collections.Generic;

public class CharacterElementManager : SingletonComponent<CharacterElementManager>
{
	public delegate void CompletedCB(bool _succeed);

	private string m_CEDatabase = GLOBAL.CHARACTER_ELEMENT_DATABASE;

	private Dictionary<string, Dictionary<string, List<CharacterElement>>> elementDatabase;

	private List<CharacterElement> weaponsCategory;

	public static int CategoryIndexInName = 1;

	private CompletedCB m_completedCB;

	public bool IsLoaded
	{
		get
		{
			return elementDatabase != null;
		}
	}

	private void OnCEDatabaseDownloaded(BundleDownloadHandle _handle)
	{
		LoadCEDatabase((CharacterElementHolder)_handle.LoadMainAsset());
		_handle.UnloadAssets();
	}

	public void LoadCEDatabase(CharacterElementHolder _ceh)
	{
		elementDatabase = new Dictionary<string, Dictionary<string, List<CharacterElement>>>();
		weaponsCategory = new List<CharacterElement>();
		foreach (CharacterElement item in _ceh.content)
		{
			if (item.bundleName.Contains(Equipment.Weapon))
			{
				if (!weaponsCategory.Contains(item))
				{
					weaponsCategory.Add(item);
				}
				continue;
			}
			string key = item.bundleName.Split('/')[0];
			string category = item.category;
			if (!elementDatabase.ContainsKey(key))
			{
				elementDatabase.Add(key, new Dictionary<string, List<CharacterElement>>());
			}
			if (!elementDatabase[key].ContainsKey(category))
			{
				elementDatabase[key].Add(category, new List<CharacterElement>());
			}
			elementDatabase[key][category].Add(item);
		}
		foreach (string key2 in elementDatabase.Keys)
		{
			elementDatabase[key2].Add(Equipment.Weapon, weaponsCategory);
		}
		NBProfiler.EndTask("[CHARACTER_ELEMENT_MANAGER][INIT_MANAGER]");
		if (m_completedCB != null)
		{
			m_completedCB(true);
		}
	}

	private void OnCEDatabaseDownloadedFailed(BundleDownloadHandle _handle)
	{
		Dbg.LogError("could not Load the Character Element Database.");
		if (m_completedCB != null)
		{
			m_completedCB(false);
		}
		_handle.UnloadAssets();
	}

	public void InitManager(CompletedCB _completedCB = null)
	{
		NBProfiler.BeginTask("[CHARACTER_ELEMENT_MANAGER][INIT_MANAGER]");
		if (elementDatabase == null)
		{
			DLC instance = SingletonComponent<DLC>.Instance;
			if (!(instance == null))
			{
				m_completedCB = _completedCB;
				instance.GetAssetBundle(m_CEDatabase, m_CEDatabase, OnCEDatabaseDownloaded, OnCEDatabaseDownloadedFailed);
			}
		}
	}

	public Dictionary<string, List<CharacterElement>> GetCharacterElements(string _character)
	{
		Dictionary<string, List<CharacterElement>> value = null;
		if (!elementDatabase.TryGetValue(_character, out value))
		{
			Dbg.LogError("Could not find Character : " + _character + " in the CharacterElement Database.");
		}
		return value;
	}

	public List<CharacterElement> GetElements(string _character, string _category)
	{
		Dictionary<string, List<CharacterElement>> characterElements = GetCharacterElements(_character);
		if (elementDatabase == null || characterElements == null)
		{
			return null;
		}
		List<CharacterElement> value = null;
		if (!characterElements.TryGetValue(_category, out value))
		{
			Dbg.LogError("This character : " + _character + " doesn't contains this category : " + _category);
		}
		return value;
	}

	public CharacterElement GetCharacterElement(string _key)
	{
		foreach (string key in elementDatabase.Keys)
		{
			foreach (KeyValuePair<string, List<CharacterElement>> item in elementDatabase[key])
			{
				foreach (CharacterElement item2 in item.Value)
				{
					if (item2.category != Equipment.Belt && string.Compare(item2.name, _key, true) == 0)
					{
						return item2;
					}
				}
			}
		}
		Dbg.LogWarn("-------------> BIG WARNING <------------ MISSING ITEM : " + _key, Dbg.NBLogType.CHARACTER_GENERATOR);
		return null;
	}

	public CharacterElement GetCharacterElement(string _pattern, int _index, bool _lookForBelt = false)
	{
		CharacterElement characterElement = null;
		int num = 0;
		foreach (string key in elementDatabase.Keys)
		{
			foreach (KeyValuePair<string, List<CharacterElement>> item in elementDatabase[key])
			{
				foreach (CharacterElement item2 in item.Value)
				{
					if (item2.name.ToLower().Contains(_pattern.ToLower()) && (!(item2.category == Equipment.Belt) || _lookForBelt))
					{
						characterElement = item2;
						if (_index == num)
						{
							return item2;
						}
						num++;
					}
				}
			}
		}
		if (characterElement == null)
		{
			Dbg.LogError("Could not find the CharacterElement for key : " + _pattern + " and index : " + _index);
			return null;
		}
		Dbg.LogWarn("Could not find the CharacterElement for key : " + _pattern + " and index : " + _index + ". Return the first found!");
		return characterElement;
	}

	public int GetMaxTexture(string _pattern)
	{
		int num = 0;
		foreach (string key in elementDatabase.Keys)
		{
			foreach (KeyValuePair<string, List<CharacterElement>> item in elementDatabase[key])
			{
				foreach (CharacterElement item2 in item.Value)
				{
					if (item2.name.ToLower().Contains(_pattern.ToLower()))
					{
						num++;
					}
				}
			}
		}
		return num;
	}
}
