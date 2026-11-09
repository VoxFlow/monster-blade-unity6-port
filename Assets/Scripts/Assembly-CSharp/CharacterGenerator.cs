using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterGenerator : Singleton<CharacterGenerator>
{
	public delegate void CreatedCallBack(GameObject _character);

	public static bool s_mainMenuCharacter_enable = true;

	private static string s_mainMenuBase = "mainmenu_characterbase";

	private static string s_gameplayBase = "characterbase";

	private static string s_baseCharacterName = s_gameplayBase;

	private static bool s_oneMaterialPerPart = false;

	private CharacterElementManager cem;

	private string m_config;

	private string m_currentCharacter = string.Empty;

	private Dictionary<string, CharacterElement> m_currentConfiguration;

	private Dictionary<string, CharacterElement> m_previousConfiguration;

	private CreatedCallBack m_onCharacterCreated;

	private string m_characterBaseName;

	private GameObject m_characterBase;

	private GameObject m_characterGO;

	private int m_loadedPart;

	private Dictionary<Equipment.Part, bool> m_hasEquipment;

	private float m_startTime;

	private bool m_saveTextureToDisk;

	private string m_textureSaveLocation = "Assets/AssetBundles/_DEBUG/";

	private WWW wwwTest;

	public static string[] m_textureOrder = new string[5]
	{
		Equipment.Leg,
		Equipment.Shoulder,
		Equipment.Weapon,
		Equipment.Body,
		Equipment.Head
	};

	public GameObject CharacterModelGameObject
	{
		get
		{
			return m_characterGO;
		}
		set
		{
			m_characterGO = value;
		}
	}

	private CharacterGenerator()
	{
	}

	public static void SetOneMaterialPerPart(bool p_enable)
	{
		s_oneMaterialPerPart = p_enable;
		Dbg.Log("SetOneMaterialPerPart " + p_enable);
	}

	public void UnloadPreviousConfiguration(bool _force = false)
	{
		bool flag = false;
		if (m_currentConfiguration != null && m_previousConfiguration != null && m_currentConfiguration.Keys.Count == m_previousConfiguration.Keys.Count && !_force)
		{
			foreach (string key in m_previousConfiguration.Keys)
			{
				CharacterElement characterElement = m_previousConfiguration[key];
				CharacterElement characterElement2 = m_currentConfiguration[key];
				if (characterElement.name != characterElement2.name)
				{
					characterElement.ReleaseAssets();
					flag = true;
				}
			}
		}
		else if (m_previousConfiguration != null && m_previousConfiguration.Values != null)
		{
			flag = true;
			foreach (CharacterElement value in m_previousConfiguration.Values)
			{
				value.ReleaseAssets();
			}
		}
		if (flag)
		{
			Resources.UnloadUnusedAssets();
		}
	}

	private bool PrepareConfig(string _config)
	{
		cem = SingletonComponent<CharacterElementManager>.Instance;
		string[] array = _config.Split('|');
		m_currentCharacter = array[0];
		m_currentConfiguration = new Dictionary<string, CharacterElement>();
		int num = 1;
		while (num < array.Length)
		{
			string text = array[num++];
			string text2 = array[num++];
			CharacterElement characterElement = null;
			List<CharacterElement> elements = cem.GetElements(m_currentCharacter, text);
			if (elements == null)
			{
				return false;
			}
			string text3 = text2.ToLower();
			foreach (CharacterElement item in elements)
			{
				if (item.name.ToLower() != text3)
				{
					continue;
				}
				characterElement = item;
				break;
			}
			if (characterElement == null)
			{
				return false;
			}
			if (!m_currentConfiguration.ContainsKey(text))
			{
				m_currentConfiguration.Add(text, characterElement);
			}
		}
		UnloadPreviousConfiguration();
		m_previousConfiguration = new Dictionary<string, CharacterElement>(m_currentConfiguration);
		return true;
	}

	private void DownloadCharacter()
	{
		DLC instance = SingletonComponent<DLC>.Instance;
		if (instance == null)
		{
			return;
		}
		m_loadedPart = 0;
		if (m_characterBaseName == s_baseCharacterName && m_characterBase != null)
		{
			foreach (CharacterElement value in m_currentConfiguration.Values)
			{
				value.LoadBundle(OnBundleDowloaded, OnBundleDowloadedFailed);
			}
			return;
		}
		m_characterBaseName = s_baseCharacterName;
		m_characterBase = null;
		string characterBase = instance.GetCharacterBase(s_baseCharacterName);
		Dbg.Log(" Load Character Base from DLC : " + characterBase);
		BundleDownloadHandle assetBundle = instance.GetAssetBundle(characterBase, characterBase, OnDownloadCharacterBase, OnDownloadCharacterBaseFailed);
		if (assetBundle == null && m_onCharacterCreated != null)
		{
			m_onCharacterCreated(null);
		}
	}

	private IEnumerator LoadBundleDEBUG(string location)
	{
		wwwTest = new WWW(location);
		while (!wwwTest.isDone)
		{
			yield return null;
		}
		if (wwwTest.error != null)
		{
			OnDownloadCharacterBaseFailed(null);
		}
		else if (wwwTest.assetBundle == null)
		{
			OnDownloadCharacterBaseFailed(null);
		}
		else
		{
			OnDownloadCharacterBase(null);
		}
	}

	private string ServerToConfig(string _gender, PlayerData _playerData)
	{
		List<ItemDetails> list = new List<ItemDetails>();
		for (int i = 0; i < 5; i++)
		{
			Equipment.Data equipmentData = _playerData.GetEquipmentData((Equipment.Part)i);
			list.Add(equipmentData.itemDetails);
		}
		return ItemToConfig(_gender, list);
	}

	private string ItemToConfig(string _gender, List<ItemDetails> _items)
	{
		string text = _gender;
		for (int i = 0; i < 5; i++)
		{
			ItemDetails itemDetails = _items[i];
			if (itemDetails == null || itemDetails == ItemManager.s_NULL_DATA || !itemDetails.IsEquipment || itemDetails.equipment.characterElement == null)
			{
				string text2 = text;
				text = text2 + "|" + Equipment.ToString((Equipment.Part)i) + "|" + GLOBAL.DEFAULT_PARTS[i];
			}
			else
			{
				CharacterElement characterElement = itemDetails.equipment.characterElement;
				string text2 = text;
				text = text2 + "|" + characterElement.category + "|" + characterElement.name;
			}
		}
		return text;
	}

	private string ConvertConfigDictToString()
	{
		string text = m_currentCharacter;
		foreach (KeyValuePair<string, CharacterElement> item in m_currentConfiguration)
		{
			if (item.Value != null)
			{
				string text2 = text;
				text = text2 + "|" + item.Key + "|" + item.Value.name;
			}
		}
		return text;
	}

	private Dictionary<string, CharacterElement> GetCurrentConfig()
	{
		return m_currentConfiguration;
	}

	private void OnBundleDowloaded(CharacterElement ce)
	{
		m_loadedPart++;
		if (m_loadedPart == m_currentConfiguration.Keys.Count)
		{
			GenerateCharacter();
		}
	}

	private void OnBundleDowloadedFailed(CharacterElement ce)
	{
		if (m_onCharacterCreated != null)
		{
			m_onCharacterCreated(null);
		}
	}

	private void OnDownloadCharacterBase(BundleDownloadHandle _handle)
	{
		m_characterBase = (GameObject)_handle.LoadMainAsset();
		DLC instance = SingletonComponent<DLC>.Instance;
		if (instance != null)
		{
			string characterBase = instance.GetCharacterBase(s_baseCharacterName);
			instance.Unload(characterBase, characterBase, false);
		}
		foreach (CharacterElement value in m_currentConfiguration.Values)
		{
			value.LoadBundle(OnBundleDowloaded, OnBundleDowloadedFailed);
		}
	}

	private void OnDownloadCharacterBaseFailed(BundleDownloadHandle _handle)
	{
		DLC instance = SingletonComponent<DLC>.Instance;
		if (instance != null)
		{
			string characterBase = instance.GetCharacterBase(s_baseCharacterName);
			instance.Unload(characterBase, characterBase);
		}
		if (m_onCharacterCreated != null)
		{
			m_onCharacterCreated(null);
		}
	}

	private void OnCharacterBaseLoadedDebug(object _asset)
	{
		m_characterBase = (GameObject)_asset;
	}

	private void SortCurrentConfiguration()
	{
		Dictionary<string, CharacterElement> dictionary = new Dictionary<string, CharacterElement>();
		for (int i = 0; i < m_textureOrder.Length; i++)
		{
			foreach (string key in m_currentConfiguration.Keys)
			{
				if (key.ToLower().Contains(m_textureOrder[i]))
				{
					dictionary.Add(key, m_currentConfiguration[key]);
					break;
				}
			}
		}
		foreach (string key2 in m_currentConfiguration.Keys)
		{
			if (!dictionary.ContainsKey(key2))
			{
				dictionary.Add(key2, m_currentConfiguration[key2]);
			}
		}
		DLC instance = SingletonComponent<DLC>.Instance;
		if (instance != null)
		{
			string characterBase = instance.GetCharacterBase(s_baseCharacterName);
			instance.Unload(characterBase, characterBase, false);
		}
		m_currentConfiguration = new Dictionary<string, CharacterElement>(dictionary);
		dictionary = null;
	}

	private GameObject Generate(GameObject root)
	{
		List<CombineInstance> list = new List<CombineInstance>();
		List<Material> list2 = new List<Material>();
		List<Transform> list3 = new List<Transform>();
		Transform[] componentsInChildren = MBHier.GetComponentsInChildren<Transform>(root);
		Material material = root.GetComponent<Renderer>().material;
#if UNITY_ANDROID
		Texture2D texture2D = new Texture2D(1024, 1024, TextureFormat.ETC_RGB4, false);
#else
		Texture2D texture2D = new Texture2D(1024, 1024, TextureFormat.RGB24, false);   // PORT (CD-013)
#endif
		Texture2D[] array = new Texture2D[5];
		int num = 0;
		SortCurrentConfiguration();
		ClearDebugTexture();
		foreach (CharacterElement value in m_currentConfiguration.Values)
		{
			if (value == null)
			{
				return null;
			}
			SkinnedMeshRenderer skinnedMeshRenderer = value.GetSkinnedMeshRenderer();
			if (skinnedMeshRenderer == null)
			{
				return null;
			}
			for (int i = 0; i < skinnedMeshRenderer.sharedMesh.subMeshCount; i++)
			{
				list.Add(new CombineInstance
				{
					mesh = skinnedMeshRenderer.sharedMesh,
					subMeshIndex = i
				});
			}
			if (!value.bundleName.Contains(Equipment.Belt) && num < array.Length && num >= 0)
			{
				array[num] = (Texture2D)value.GetDiffuseTexture();
				num++;
			}
			if (s_oneMaterialPerPart)
			{
				list2.Add(new Material(material));
			}
			else
			{
				list2.Add(material);
			}
			string[] boneNames = value.GetBoneNames();
			if (boneNames == null)
			{
				return null;
			}
			string[] boneNames2 = value.GetBoneNames();
			foreach (string text in boneNames2)
			{
				Transform[] array2 = componentsInChildren;
				foreach (Transform transform in array2)
				{
					if (!(transform == null) && !(transform.name != text))
					{
						list3.Add(transform);
						break;
					}
				}
			}
			if (skinnedMeshRenderer.gameObject != null)
			{
				Object.Destroy(skinnedMeshRenderer.gameObject);
			}
		}
		SkinnedMeshRenderer component = root.GetComponent<SkinnedMeshRenderer>();
		if (component == null)
		{
			return null;
		}
		component.sharedMesh = new Mesh();
		bool flag = true;
		flag = false;
		component.sharedMesh.CombineMeshes(list.ToArray(), !s_oneMaterialPerPart, false);
		if (s_oneMaterialPerPart)
		{
			component.materials = list2.ToArray();
			component.material = material;
		}
		else
		{
			component.material = material;
		}
		Dbg.Log("Material on gameObject : " + component.material.name + " shader Name : " + component.material.shader.name, Dbg.NBLogType.CHARACTER_GENERATOR);
		component.bones = list3.ToArray();
		texture2D.PackTextures(array, 0, 1024, flag);
		material.SetTexture("_MainTex", texture2D);
		if (s_oneMaterialPerPart)
		{
			foreach (Material item in list2)
			{
				item.SetTexture("_MainTex", texture2D);
			}
		}
		SaveTexture(texture2D, "CombinedTexture");
		for (int l = 0; l < array.Length; l++)
		{
			array[l] = null;
		}
		array = null;
		texture2D = null;
		list.Clear();
		list = null;
		list2.Clear();
		list2 = null;
		return root;
	}

	private GameObject Generate2(GameObject root)
	{
		Dbg.Log("Material on gameObject : " + root.GetComponent<Renderer>().material.name, Dbg.NBLogType.CHARACTER_GENERATOR);
		Material material = root.GetComponent<Renderer>().material;
		foreach (CharacterElement value in m_currentConfiguration.Values)
		{
			if (value == null)
			{
				return null;
			}
			SkinnedMeshRenderer skinnedMeshRenderer = value.GetSkinnedMeshRenderer();
			if (skinnedMeshRenderer == null)
			{
				return null;
			}
			skinnedMeshRenderer.transform.parent = root.transform;
			skinnedMeshRenderer.material = (Material)Object.Instantiate(material);
			SetupSkinnedMeshRenderer(skinnedMeshRenderer, root, value, false);
			skinnedMeshRenderer.gameObject.layer = LayerMask.NameToLayer("Player");
		}
		return root;
	}

	private void SaveTexture(Texture2D _texture, string _name = "")
	{
	}

	private void ClearDebugTexture()
	{
	}

	public void DestroyCharacter(GameObject obj)
	{
		Object.DestroyImmediate(obj);
	}

	private void GenerateCharacter()
	{
		if (m_characterBase == null)
		{
			if (m_onCharacterCreated != null)
			{
				m_onCharacterCreated(null);
			}
			return;
		}
		GameObject gameObject = (GameObject)Object.Instantiate(m_characterBase);
		gameObject.name = m_currentCharacter;
		m_characterGO = Generate(gameObject);
		gameObject.SetActiveRecursively(false);
		if (m_onCharacterCreated != null)
		{
			m_onCharacterCreated(m_characterGO);
		}
		NBProfiler.EndTask("[CREATE_CHARACTER]");
	}

	private void GenerateCharacter(CreatedCallBack _onCharacterCreated, PlayerData _playerData, bool _mainCharacter)
	{
		m_startTime = Time.realtimeSinceStartup;
		if (_playerData == null)
		{
			if (_onCharacterCreated != null)
			{
				_onCharacterCreated(null);
			}
		}
		else
		{
			string config = ServerToConfig("male", _playerData);
			CreateCharacter(_onCharacterCreated, config);
		}
	}

	private void CreateCharacter(CreatedCallBack _onCharacterCreated, string _config)
	{
		m_startTime = Time.realtimeSinceStartup;
		NBProfiler.BeginTask("[CREATE_CHARACTER]");
		cem = SingletonComponent<CharacterElementManager>.Instance;
		if (cem == null || !cem.IsLoaded)
		{
			if (_onCharacterCreated != null)
			{
				_onCharacterCreated(null);
			}
			return;
		}
		m_onCharacterCreated = _onCharacterCreated;
		if (!PrepareConfig(_config))
		{
			if (m_onCharacterCreated != null)
			{
				m_onCharacterCreated(null);
			}
		}
		else
		{
			DownloadCharacter();
		}
	}

	private static void SetupSkinnedMeshRenderer(SkinnedMeshRenderer r, GameObject baseModel, CharacterElement _ce, bool bGachaModel)
	{
		Transform child = baseModel.transform.GetChild(0);
		string[] boneNames = _ce.GetBoneNames(bGachaModel);
		if (boneNames != null)
		{
			Transform[] array = new Transform[boneNames.Length];
			for (int i = 0; i < boneNames.Length; i++)
			{
				array[i] = child.FindInHierarchy(boneNames[i]);
			}
			r.bones = array;
		}
		r.sharedMesh = _ce.GetMesh(bGachaModel);
		Material[] materials = r.materials;
		foreach (Material material in materials)
		{
			material.mainTexture = _ce.GetDiffuseTexture();
			material.mainTextureScale = _ce.GetTextureScale();
			material.mainTexture.filterMode = FilterMode.Bilinear;
		}
		r.updateWhenOffscreen = true;
	}

	private static void CreateCharacterEmelentModel(GameObject baseModel, CharacterElement _ce)
	{
		SkinnedMeshRenderer r = baseModel.GetComponent<Renderer>() as SkinnedMeshRenderer;
		SetupSkinnedMeshRenderer(r, baseModel, _ce, true);
	}

	public void GenerateMainMenuCharacter(CreatedCallBack _onCharacterCreated, PlayerData _playerData, bool _mainCharacter)
	{
		if (!s_mainMenuCharacter_enable && _onCharacterCreated != null)
		{
			_onCharacterCreated(null);
			return;
		}
		s_baseCharacterName = s_mainMenuBase;
		GenerateCharacter(_onCharacterCreated, _playerData, _mainCharacter);
	}

	public void GenerateGamePlayCharacter(CreatedCallBack _onCharacterCreated, PlayerData _playerData, bool _mainCharacter)
	{
		s_baseCharacterName = s_gameplayBase;
		GenerateCharacter(_onCharacterCreated, _playerData, _mainCharacter);
	}

	public void LoadMainMenuCharacterBase(CreatedCallBack _onCharacterCreated)
	{
		DLC dlc = SingletonComponent<DLC>.Instance;
		if (dlc == null)
		{
			return;
		}
		string characterBase;
		if (s_mainMenuCharacter_enable)
		{
			characterBase = s_mainMenuBase;
		}
		else
		{
			characterBase = s_gameplayBase;
		}
		if (m_characterBase == null || m_characterBaseName != characterBase)
		{
			m_characterBaseName = characterBase;
			m_characterBase = null;
			characterBase = dlc.GetCharacterBase(characterBase);
			BundleDownloadHandle assetBundle = dlc.GetAssetBundle(characterBase, characterBase, delegate(BundleDownloadHandle h)
			{
				m_characterBase = (GameObject)h.LoadMainAsset();
				GameObject character2 = (GameObject)Object.Instantiate(m_characterBase);
				dlc.Unload(characterBase, characterBase, false);
				if (_onCharacterCreated != null)
				{
					_onCharacterCreated(character2);
				}
			}, delegate
			{
				dlc.Unload(characterBase, characterBase);
				if (_onCharacterCreated != null)
				{
					_onCharacterCreated(null);
				}
			});
			if (assetBundle == null && _onCharacterCreated != null)
			{
				Dbg.LogError(" Could not get the base from DLC. ");
				_onCharacterCreated(null);
			}
		}
		else
		{
			GameObject character = (GameObject)Object.Instantiate(m_characterBase);
			if (_onCharacterCreated != null)
			{
				_onCharacterCreated(character);
			}
		}
	}

	public static void GenerateCharacterElementModel(GameObject baseModel, CreatedCallBack _onCharacterCreated, CharacterElement ce)
	{
		ce.LoadBundle(delegate(CharacterElement _ce)
		{
			SkinnedMeshRenderer skinnedMeshRenderer = _ce.GetSkinnedMeshRenderer();
			skinnedMeshRenderer.material = (Material)Object.Instantiate(baseModel.GetComponent<SkinnedMeshRenderer>().sharedMaterial);
			int i = 1;
			for (int num = skinnedMeshRenderer.materials.Length; i < num; i++)
			{
				skinnedMeshRenderer.materials[i] = skinnedMeshRenderer.material;
			}
			GameObject gameObject = skinnedMeshRenderer.gameObject;
			gameObject.transform.parent = baseModel.transform;
			SetupSkinnedMeshRenderer(skinnedMeshRenderer, baseModel, _ce, true);
			if (_onCharacterCreated != null)
			{
				_onCharacterCreated(gameObject);
			}
		}, delegate
		{
			Object.DestroyObject(baseModel);
			if (_onCharacterCreated != null)
			{
				_onCharacterCreated(null);
			}
		});
	}

	public static void GenerateCharacterElementModel(CreatedCallBack _onCharacterCreated, CharacterElement ce)
	{
		DLC dlc = SingletonComponent<DLC>.Instance;
		if (dlc == null)
		{
			return;
		}
		if (ce == null)
		{
			Dbg.Assert(ce != null, "null character element");
			return;
		}
		string characterBase;
		if (s_mainMenuCharacter_enable)
		{
			characterBase = dlc.GetCharacterBase(s_mainMenuBase);
		}
		else
		{
			characterBase = dlc.GetCharacterBase(s_gameplayBase);
		}
		BundleDownloadHandle assetBundle = dlc.GetAssetBundle(characterBase, characterBase, delegate(BundleDownloadHandle h)
		{
			GameObject baseModel = (GameObject)Object.Instantiate((Object)h.LoadMainAsset());
			dlc.Unload(characterBase, characterBase, false);
			ce.LoadBundle(delegate
			{
				CreateCharacterEmelentModel(baseModel, ce);
				if (_onCharacterCreated != null)
				{
					_onCharacterCreated(baseModel);
				}
			}, delegate
			{
				Object.DestroyObject(baseModel);
				if (_onCharacterCreated != null)
				{
					_onCharacterCreated(null);
				}
			});
		}, delegate
		{
			dlc.Unload(characterBase, characterBase);
			if (_onCharacterCreated != null)
			{
				_onCharacterCreated(null);
			}
		});
		if (assetBundle == null && _onCharacterCreated != null)
		{
			_onCharacterCreated(null);
		}
	}

	public void CreateCharacter(List<CharacterElement> _charactereElement, object _characterBase, CreatedCallBack _onCharacterCreated = null, string _gender = "male")
	{
		m_startTime = Time.realtimeSinceStartup;
		string text = _gender;
		foreach (CharacterElement item in _charactereElement)
		{
			string text2 = text;
			text = text2 + "|" + item.category + "|" + item.name;
		}
		OnCharacterBaseLoadedDebug(_characterBase);
		m_onCharacterCreated = _onCharacterCreated;
		if (!PrepareConfig(text))
		{
			if (m_onCharacterCreated != null)
			{
				m_onCharacterCreated(null);
			}
		}
		else
		{
			GenerateCharacter();
		}
	}

	public void ClearCache()
	{
		if (m_characterBase != null)
		{
			m_characterBase = null;
			m_characterBaseName = null;
		}
		if (m_characterGO != null)
		{
			Object.DestroyImmediate(m_characterGO);
			m_characterGO = null;
		}
		UnloadPreviousConfiguration(true);
	}
}
