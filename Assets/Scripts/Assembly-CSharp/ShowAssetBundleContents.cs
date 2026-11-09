using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class ShowAssetBundleContents : MonoBehaviour
{
	private delegate void OnLoaded(WWW www);

	public bool m_enableOnGUI;

	public bool loadAllNames = true;

	public bool m_autoLoadBase = true;

	public List<string> equipmentNames;

	private int equipmentIndex;

	public UnityEngine.Object[] m_equipments;

	public int[] m_textureIdx;

	public int[] m_maxTextureIdx;

	public UnityEngine.Object m_CEDatabase;

	public UnityEngine.Object m_characterBase;

	public UnityEngine.Object assignedAsset;

	public string assignName = string.Empty;

	public bool showAssigned;

	public bool showAssignedAsMesh;

	public bool showSelected;

	public bool m_autoReleaseAfterLoad = true;

	public bool loadGameObjects;

	public bool instantiteGO = true;

	public bool instantiatePlayer;

	public MemoryInfo m_memoryInfo;

	public List<UIAtlas> m_atlasList = new List<UIAtlas>();

	public List<string> m_assignList;

	private List<WWW> m_wwwAssignList;

	private int m_index;

	public bool overrideFromServer = true;

	public string[] filesToDownload;

	private int fileDownloadedIndex;

	private List<CharacterElement> m_equipmentsCE = new List<CharacterElement>();

	private object m_characterBaseObj;

	private WWW www;

	private List<GameObject> tempLoadedGao;

	private string location = string.Empty;

	private float loadingTime;

	private OnLoaded m_onLoaded;

	private int m_equipIdx;

	private int m_characterCreatedCounter;

	private List<GameObject> m_characterList = new List<GameObject>();

	public bool unloadeAllAssets = true;

	public bool m_nullWWW = true;

	public bool m_loadMainAsset = true;

	public bool unloadAllWWW;

	public bool loadAllWWW;

	public bool unloadOnClear = true;

	public bool m_nullAtlasTexture = true;

	private List<string> m_animList = new List<string>();

	private static string m_characterBundleLoc = "/assetbundles/characters/";

	private static string m_commonBundleLoc = "/assetbundles/characters/common/";

	private static string m_weaponBundleLoc = m_commonBundleLoc + "weapons/";

	private static string m_maleBundleLoc = m_characterBundleLoc + "male/";

	private string[] m_parts = new string[5] { "body", "head", "leg", "shoulder", "weapon" };

	private string m_defaultCEM = "/assetbundles/characters/character_element_database";

	private string m_defaultBase = m_maleBundleLoc + "/male_characterbase";

	public List<string> m_setNames = new List<string>();

	private int m_currentSetIndex;

	public string m_setToLoad = string.Empty;

	public bool m_loadSelectedSet;

	public bool m_loadNextSet;

	public bool m_loadPreviousSet;

	public int m_textureOffset;

	public bool m_loadNextTexture;

	public bool m_loadPrevTexture;

	public bool m_autoDelCharacter = true;

	public bool m_deleteCharacter;

	public bool m_randomMode = true;

	public bool m_loopAnim;

	public bool m_prevAnim;

	public bool m_nextAnim;

	public int m_animIdx;

	public bool m_pauseTimer;

	public float m_timePerCharacter = 5f;

	public bool m_generateRandomly;

	private float m_currentTimer;

	private CharacterElementManager cem;

	private Dictionary<string, List<CharacterElement>> allMaleParts;

	private bool CanShow
	{
		get
		{
			return assignedAsset != null && (www == null || www.isDone);
		}
	}

	private void Start()
	{
		m_wwwAssignList = new List<WWW>();
		m_equipmentsCE = new List<CharacterElement>();
		IdentifySets(ref m_setNames);
		cem = SingletonComponent<CharacterElementManager>.Instance;
		if (m_autoLoadBase || m_randomMode)
		{
			StartScript();
		}
	}

	private void StartScript()
	{
		Dbg.Log(" Start loading all assetBundles.");
		if (loadAllNames)
		{
			string text = equipmentNames[equipmentIndex];
			string text2 = Application.streamingAssetsPath + "/" + text + ".assetbundle";
			Load(OnCEDatabaseLoaded, text2);
		}
		else if (m_CEDatabase != null)
		{
			assignedAsset = m_CEDatabase;
			Load(OnCEDatabaseLoaded, string.Empty);
		}
	}

	private void LoadAllEquipment(WWW _www = null)
	{
		if (!(cem == null))
		{
			string text = equipmentNames[equipmentIndex];
			string text2 = Application.streamingAssetsPath + "/" + text + ".assetbundle";
			bool lookForBelt = false;
			int num = equipmentIndex - 2;
			string fileName = Path.GetFileName(text);
			m_maxTextureIdx[num] = cem.GetMaxTexture(fileName);
			int num2 = m_maxTextureIdx[num];
			m_textureIdx[num] = m_textureOffset;
			int num3 = m_textureIdx[num];
			if (num3 < 0 || num3 >= num2)
			{
				m_textureOffset = 0;
				num3 = 0;
				m_textureIdx[num] = 0;
			}
			CharacterElement characterElement = cem.GetCharacterElement(fileName, num3, lookForBelt);
			if (characterElement != null)
			{
				characterElement.LoadSync(_www);
				m_equipmentsCE.Add(characterElement);
			}
			if (num >= 4 || m_equipmentsCE.Count >= 5)
			{
				CreateCharacter();
				return;
			}
			equipmentIndex++;
			text = equipmentNames[equipmentIndex];
			text2 = Application.streamingAssetsPath + "/" + text + ".assetbundle";
			Load(LoadAllEquipment, text2);
		}
	}

	private void OnCEDatabaseLoaded(WWW _www)
	{
		if (!(cem == null))
		{
			UnityEngine.Object mainAsset = _www.assetBundle.mainAsset;
			CharacterElementHolder ceh = (CharacterElementHolder)mainAsset;
			cem.LoadCEDatabase(ceh);
			DownloadBase();
			float runtimeMemorySizeInMB = NBProfiler.GetRuntimeMemorySizeInMB(mainAsset);
			Dbg.LogWarn(" !!!! CharacterCEDatabase Size : " + runtimeMemorySizeInMB + " MB !!!!");
			mainAsset = null;
			Resources.UnloadUnusedAssets();
			GC.Collect();
			allMaleParts = cem.GetCharacterElements("male");
			if (m_randomMode)
			{
				AutoCreate();
			}
		}
	}

	private void DownloadBase()
	{
		if (loadAllNames)
		{
			equipmentIndex++;
			string text = equipmentNames[equipmentIndex];
			string text2 = Application.streamingAssetsPath + "/" + text + ".assetbundle";
			Load(SetBase, text2);
		}
		else
		{
			assignedAsset = m_characterBase;
			Load(SetBase, string.Empty);
		}
	}

	private void SetBase(WWW _www)
	{
		m_characterBaseObj = _www.assetBundle.mainAsset;
	}

	private void DownloadGivenFiles(WWW _www = null)
	{
		if (filesToDownload != null)
		{
			if (_www != null)
			{
				SaveToDisk(_www);
			}
			if (fileDownloadedIndex < filesToDownload.Length)
			{
				string text = filesToDownload[fileDownloadedIndex];
				Dbg.Log(" File to Load : " + text);
				Load(DownloadGivenFiles, text);
				fileDownloadedIndex++;
			}
		}
	}

	private void SaveToDisk(WWW _www)
	{
		if (_www != null)
		{
			string fileName = Path.GetFileName(_www.url);
			string text = Application.streamingAssetsPath + "/" + fileName;
			if (File.Exists(text))
			{
				File.Delete(text);
			}
			Dbg.LogWarn("-- > Writing file: " + text);
			File.WriteAllBytes(text, _www.bytes);
		}
	}

	private void ShowSelection()
	{
	}

	private void AutoCreate()
	{
		if (cem == null)
		{
			return;
		}
		if (m_characterBaseObj == null)
		{
			Dbg.Log("The base was not loaded");
			return;
		}
		if (m_autoDelCharacter)
		{
			DeleteCharacters();
		}
		equipmentNames.Clear();
		equipmentNames.Add(m_defaultCEM);
		equipmentNames.Add(m_defaultBase);
		for (int i = 0; i < m_parts.Length; i++)
		{
			string text = m_parts[i];
			int count = allMaleParts[text].Count;
			int index = UnityEngine.Random.Range(0, count);
			string text2 = allMaleParts[text][index].bundleName.Replace(".assetbundle", string.Empty);
			string empty = string.Empty;
			empty = ((!(text == "weapon")) ? (m_characterBundleLoc + text2) : (m_commonBundleLoc + text2));
			equipmentNames.Add(empty);
		}
		m_equipIdx = 0;
		StartEquipmentDownload();
	}

	private void Update()
	{
		if (m_nextAnim || m_prevAnim || m_loopAnim)
		{
			WrapMode wrapMode = WrapMode.Once;
			if (m_nextAnim)
			{
				m_animIdx++;
				m_nextAnim = false;
			}
			if (m_prevAnim)
			{
				m_animIdx--;
				m_prevAnim = false;
			}
			if (m_loopAnim)
			{
				wrapMode = WrapMode.Loop;
			}
			if (m_characterList.Count > 0 && m_animList.Count > 0)
			{
				GameObject gameObject = m_characterList[0];
				if (gameObject != null)
				{
					if (m_animIdx >= m_animList.Count || m_animIdx < 0)
					{
						m_animIdx = 0;
					}
					string text = m_animList[m_animIdx];
					gameObject.GetComponent<Animation>()[text].wrapMode = wrapMode;
					gameObject.GetComponent<Animation>().Play(text);
					Dbg.Log("Playing anim : " + text);
				}
			}
		}
		if (m_randomMode)
		{
			if (!m_pauseTimer)
			{
				m_currentTimer += Time.deltaTime;
			}
			if (m_currentTimer >= m_timePerCharacter || m_generateRandomly)
			{
				AutoCreate();
				m_currentTimer = 0f;
				m_generateRandomly = false;
			}
		}
		if (loadAllWWW)
		{
			foreach (string assign in m_assignList)
			{
				string text2 = Application.streamingAssetsPath + "/" + assign + ".assetbundle";
				m_wwwAssignList.Add(new WWW(text2));
				Dbg.Log("New WWW : " + text2);
			}
			loadAllWWW = false;
		}
		if (unloadAllWWW)
		{
			foreach (WWW wwwAssign in m_wwwAssignList)
			{
				www = null;
				Dbg.Log("null WWW : " + wwwAssign.url);
			}
			m_wwwAssignList.Clear();
			unloadAllWWW = false;
		}
		if (showAssigned)
		{
			string text3 = Application.streamingAssetsPath + "/" + assignName + ".assetbundle";
			Load(null, text3);
			showAssigned = false;
		}
		if (showAssignedAsMesh)
		{
			string text4 = Application.streamingAssetsPath + "/" + assignName + ".assetbundle";
			Load(OnMeshLoaded, text4);
			showAssignedAsMesh = false;
		}
		if (instantiatePlayer)
		{
			m_equipIdx = 0;
			StartEquipmentDownload();
			instantiatePlayer = false;
		}
		if (m_loadSelectedSet)
		{
			LoadSet();
			m_loadSelectedSet = false;
		}
		if (m_loadNextSet)
		{
			m_currentSetIndex++;
			LoadSet(false);
			m_loadNextSet = false;
		}
		if (m_loadPreviousSet)
		{
			m_currentSetIndex--;
			LoadSet(false);
			m_loadPreviousSet = false;
		}
		if (m_loadNextTexture)
		{
			m_textureOffset++;
			LoadSet();
			m_loadNextTexture = false;
		}
		if (m_loadPrevTexture)
		{
			m_textureOffset--;
			LoadSet();
			m_loadPrevTexture = false;
		}
		if (m_deleteCharacter)
		{
			DeleteCharacters();
			m_deleteCharacter = false;
		}
	}

	private void OnGUI()
	{
		if (!m_enableOnGUI)
		{
			return;
		}
		if (GUILayout.Button("LoadAssetBundles", GUILayout.Width(100f), GUILayout.Height(100f)))
		{
			StartScript();
		}
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("CreateCharacter", GUILayout.Width(100f), GUILayout.Height(100f)))
		{
			m_equipIdx = 0;
			StartEquipmentDownload();
			instantiatePlayer = false;
		}
		if (GUILayout.Button("DeleteCharacters", GUILayout.Width(100f), GUILayout.Height(100f)))
		{
			DeleteCharacters();
		}
		GUILayout.EndHorizontal();
		GUILayout.Label(" Heap Size : " + NBProfiler.HeapSize);
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("PrintMemoryUsage", GUILayout.Width(100f), GUILayout.Height(100f)))
		{
			m_memoryInfo.m_PRINT_MEMORY_INFO = true;
		}
		if (GUILayout.Button("UnloadAllResources", GUILayout.Width(100f), GUILayout.Height(100f)))
		{
			Resources.UnloadUnusedAssets();
			Dbg.Log(" Resources.UnloadUnusedAssets(); ");
		}
		if (GUILayout.Button("GarbageCollect", GUILayout.Width(100f), GUILayout.Height(100f)))
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			Dbg.Log(" System.GC.Collect(); ");
		}
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Load Assignee WWW", GUILayout.Width(100f), GUILayout.Height(100f)) && m_assignList.Count > 0)
		{
			if (m_index < 0 || m_index >= m_assignList.Count)
			{
				m_index = 0;
				m_atlasList.Clear();
				m_wwwAssignList.Clear();
				Dbg.Log(" --- Reset List and index --- ");
				Resources.UnloadUnusedAssets();
				Dbg.Log(" --- Resources.UnloadUnusedAssets() On List Cleared! --- ");
			}
			assignName = m_assignList[m_index];
			showAssigned = true;
			m_index++;
		}
		if (GUILayout.Button("Unload WWW", GUILayout.Width(100f), GUILayout.Height(100f)))
		{
			UnloadWWW();
		}
		GUILayout.BeginVertical();
		m_autoReleaseAfterLoad = GUILayout.Toggle(m_autoReleaseAfterLoad, "Auto Release");
		unloadeAllAssets = GUILayout.Toggle(unloadeAllAssets, "Unload all assets from bundle.");
		m_nullWWW = GUILayout.Toggle(m_nullWWW, "Null WWW");
		unloadOnClear = GUILayout.Toggle(unloadOnClear, "Unload WWW list on Clear");
		m_nullAtlasTexture = GUILayout.Toggle(m_nullAtlasTexture, "Null Texture from atlas");
		GUILayout.Label("assignName : " + assignName);
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
	}

	private void Load(OnLoaded _onLoaded = null, string _location = "")
	{
		m_onLoaded = _onLoaded;
		Dbg.Log(" --> Load Start <-- ");
		string text = "file:///";
		if (_location.Contains("http"))
		{
			location = _location;
		}
		else
		{
			location = text + _location;
		}
		Dbg.Log("Selected Asset Location : " + location);
		loadingTime = Time.time;
		StartCoroutine(LoadBundle());
	}

	private void StartEquipmentDownload()
	{
		if (loadAllNames)
		{
			equipmentIndex = 2;
			string text = equipmentNames[equipmentIndex];
			string text2 = Application.streamingAssetsPath + "/" + text + ".assetbundle";
			Load(LoadAllEquipment, text2);
		}
		else
		{
			assignedAsset = m_equipments[m_equipIdx];
			Load(OnEquipmentLoaded, string.Empty);
		}
	}

	private void OnEquipmentLoaded(WWW _www = null)
	{
		if (!(cem == null))
		{
			string text = assignedAsset.name;
			bool lookForBelt = false;
			if (text.ToLower().Contains("belt"))
			{
				lookForBelt = true;
				text = text.Replace("belt", "leg");
			}
			int index = m_textureIdx[m_equipIdx];
			CharacterElement characterElement = cem.GetCharacterElement(text, index, lookForBelt);
			if (characterElement != null)
			{
				characterElement.LoadSync(_www);
				m_equipmentsCE.Add(characterElement);
			}
			if (m_equipIdx >= 5 || m_equipmentsCE.Count >= 6)
			{
				CreateCharacter();
				return;
			}
			m_equipIdx++;
			assignedAsset = m_equipments[m_equipIdx];
			Load(OnEquipmentLoaded, string.Empty);
		}
	}

	private void UnloadWWW()
	{
		if (www != null && www.assetBundle != null)
		{
			www.assetBundle.Unload(true);
			Dbg.Log("Unlaod Selected WWW Successfuly");
		}
		if (m_nullWWW)
		{
			www = null;
			Dbg.Log("WWW null");
		}
		Dbg.Log("No need to unload Selected WWW");
	}

	private void LoadContent(WWW _www, bool _showContent = true)
	{
		if (_www == null)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (m_loadMainAsset)
		{
			UnityEngine.Object mainAsset = www.assetBundle.mainAsset;
			if (mainAsset == null)
			{
				www.assetBundle.Unload(true);
				Resources.UnloadUnusedAssets();
				Dbg.LogWarn("No Main assets found.");
				return;
			}
			stringBuilder.AppendLine(string.Concat(mainAsset.name, "\t\tType: ", mainAsset.GetType(), "\t\tSize: ", NBProfiler.GetRuntimeMemorySizeInMB(mainAsset), "MB"));
			if (mainAsset.GetType() == typeof(GameObject))
			{
				GameObject gameObject = (GameObject)mainAsset;
				UIAtlas component = gameObject.GetComponent<UIAtlas>();
				if (component != null)
				{
					m_atlasList.Add(component);
					m_wwwAssignList.Add(www);
					Dbg.Log("Assign WWW and Atlas to list : " + www.url);
				}
			}
		}
		else
		{
			UnityEngine.Object[] array = www.assetBundle.LoadAllAssets();
			stringBuilder.AppendLine("Object Count in AssetBundles : " + array.Length);
			if (_showContent)
			{
				UnityEngine.Object[] array2 = array;
				foreach (UnityEngine.Object obj in array2)
				{
					stringBuilder.AppendLine(string.Concat(obj.name, "\t\tType: ", obj.GetType(), "\t\tSize: ", NBProfiler.GetRuntimeMemorySizeInMB(obj), "MB"));
					if (obj.GetType() == typeof(GameObject))
					{
						GameObject gameObject2 = (GameObject)obj;
						UIAtlas component2 = gameObject2.GetComponent<UIAtlas>();
						if (component2 != null)
						{
							m_atlasList.Add(component2);
							m_wwwAssignList.Add(www);
							Dbg.Log("Assign WWW and Atlas to list : " + www.url);
						}
					}
				}
			}
		}
		Dbg.Log(stringBuilder.ToString());
		stringBuilder = null;
		if (m_autoReleaseAfterLoad)
		{
			www.assetBundle.Unload(unloadeAllAssets);
			www.Dispose();
			Resources.UnloadUnusedAssets();
			Dbg.Log(" Resources.UnloadUnusedAssets() for www.assetBundle.Unload( " + unloadeAllAssets + " ); ");
		}
	}

	private void ShowAtlas(UnityEngine.Object _obj)
	{
		string text = "Atlas Content :\n";
		Type type = _obj.GetType();
		if (type != typeof(GameObject))
		{
			return;
		}
		GameObject gameObject = (GameObject)_obj;
		if (instantiteGO)
		{
			UnityEngine.Object.Instantiate((GameObject)_obj);
		}
		UIAtlas component = gameObject.GetComponent<UIAtlas>();
		if (component != null)
		{
			foreach (string listOfSprite in component.GetListOfSprites())
			{
				text = text + listOfSprite + "\n";
			}
		}
		Dbg.Log(text);
	}

	private void CreateCharacter()
	{
		CharacterGenerator instance = Singleton<CharacterGenerator>.Instance;
		instance.CreateCharacter(m_equipmentsCE, m_characterBaseObj, OnCharacterCreated);
		UnLoadAssets();
	}

	private void UnLoadAssets()
	{
		foreach (CharacterElement item in m_equipmentsCE)
		{
			item.UnLoad();
		}
		m_equipmentsCE.Clear();
	}

	private void OnCharacterCreated(GameObject _gao)
	{
		_gao.transform.position = new Vector3(_gao.transform.position.x - (float)m_characterCreatedCounter, _gao.transform.position.y, _gao.transform.position.z);
		m_characterCreatedCounter++;
		_gao.SetActiveRecursively(true);
		BoxCollider orAddComponent = NBComponentHelper.GetOrAddComponent<BoxCollider>(_gao);
		if (orAddComponent != null)
		{
			orAddComponent.size = new Vector3(0.6f, 1.5f, 0.8f);
			orAddComponent.center = new Vector3(0f, 0.85f, 0f);
		}
		SpinWithMouse orAddComponent2 = NBComponentHelper.GetOrAddComponent<SpinWithMouse>(_gao);
		Dbg.Log(orAddComponent2.name);
		orAddComponent2.target = _gao.transform;
		m_characterList.Add(_gao);
		if (m_animList.Count < 1)
		{
			GameObject gameObject = m_characterList[0];
			StringBuilder stringBuilder = new StringBuilder();
			foreach (AnimationState item in gameObject.GetComponent<Animation>())
			{
				m_animList.Add(item.name);
				stringBuilder.AppendLine(item.name);
			}
			gameObject.GetComponent<Animation>().Play(m_animList[m_animIdx]);
			Dbg.Log(stringBuilder.ToString());
		}
		Dbg.Log(" -------> Character Created <-------- ");
	}

	private void DeleteCharacters()
	{
		Dbg.Log(" DeleteCharacters(); ");
		for (int i = 0; i < m_characterList.Count; i++)
		{
			GameObject gameObject = m_characterList[i];
			if (gameObject != null)
			{
				UnityEngine.Object.Destroy(gameObject);
				gameObject = null;
			}
		}
		m_characterCreatedCounter = 0;
		Resources.UnloadUnusedAssets();
		GC.Collect();
	}

	private void OnMeshLoaded(WWW _www)
	{
		UnityEngine.Object[] array = www.assetBundle.LoadAllAssets();
		string text = " --> Mesh Content Report for : " + _www.url + " <---\n";
		ClearTempGao();
		tempLoadedGao = new List<GameObject>();
		List<Texture> list = new List<Texture>();
		GameObject gameObject = null;
		float num = 0f;
		string text2;
		if (instantiteGO)
		{
			UnityEngine.Object[] array2 = array;
			foreach (UnityEngine.Object obj in array2)
			{
				float runtimeMemorySizeInMB = NBProfiler.GetRuntimeMemorySizeInMB(obj);
				num += runtimeMemorySizeInMB;
				text2 = text;
				text = text2 + obj.name + "\t\t" + obj.ToString() + "\t\t " + runtimeMemorySizeInMB + "MB.\n";
				if (obj.GetType() == typeof(GameObject))
				{
					gameObject = (GameObject)UnityEngine.Object.Instantiate(obj);
					gameObject.SetActiveRecursively(false);
				}
				else if (obj.GetType() == typeof(Texture))
				{
					list.Add((Texture)obj);
				}
			}
		}
		if (gameObject != null)
		{
			int num2 = 0;
			foreach (Texture item in list)
			{
				GameObject gameObject2 = (GameObject)UnityEngine.Object.Instantiate(gameObject);
				gameObject2.GetComponent<Renderer>().material.mainTexture = item;
				gameObject2.transform.position = new Vector3(gameObject.transform.position.x + (float)(num2 * 20), gameObject.transform.position.y, gameObject.transform.position.z);
				tempLoadedGao.Add(gameObject2);
			}
		}
		text2 = text;
		text = text2 + " --> Total Size : " + num + "\n";
		Dbg.LogWarn(text);
		UnityEngine.Object.Destroy(gameObject);
		gameObject = null;
		list.Clear();
		www.assetBundle.Unload(unloadeAllAssets);
	}

	private void ClearTempGao()
	{
		if (tempLoadedGao == null)
		{
			return;
		}
		foreach (GameObject item in tempLoadedGao)
		{
			UnityEngine.Object.Destroy(item);
		}
		tempLoadedGao.Clear();
		tempLoadedGao = null;
		Resources.UnloadUnusedAssets();
		GC.Collect();
	}

	public IEnumerator LoadBundle()
	{
		www = new WWW(location);
		while (!www.isDone)
		{
			yield return null;
		}
		if (www.error != null)
		{
			Dbg.LogError("Error : " + www.error);
			yield break;
		}
		if (www.assetBundle == null)
		{
			Dbg.LogError("Error No assetBundle loaded.");
			yield break;
		}
		loadingTime = Time.time - loadingTime;
		Dbg.Log("--> Successfuly Loaded <--");
		if (m_onLoaded != null && www != null)
		{
			m_onLoaded(www);
		}
		else
		{
			LoadContent(www);
		}
	}

	private void IdentifySets(ref List<string> _setNames)
	{
		string folder = Application.streamingAssetsPath + m_maleBundleLoc;
		List<string> _fileList = new List<string>();
		FileListInFolder(folder, ref _fileList);
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("Sets found : ");
		foreach (string item in _fileList)
		{
			string fileName = Path.GetFileName(item);
			string[] array = fileName.Split('_');
			if (array.Length >= 2)
			{
				string text = array[0];
				if (!_setNames.Contains(text) && !text.Contains("meta") && !text.Contains("altas") && !text.Contains("male"))
				{
					_setNames.Add(text);
					stringBuilder.AppendLine(text);
				}
			}
		}
		Dbg.Log(stringBuilder.ToString());
	}

	private void LoadSet(bool _useSetName = true)
	{
		if (_useSetName)
		{
			m_currentSetIndex = m_setNames.IndexOf(m_setToLoad);
		}
		else
		{
			if (m_currentSetIndex < 0 || m_currentSetIndex >= m_setNames.Count)
			{
				m_currentSetIndex = 0;
			}
			m_setToLoad = m_setNames[m_currentSetIndex];
		}
		if (m_currentSetIndex == -1)
		{
			Dbg.Log("This set was not found. ");
			return;
		}
		if (m_characterBaseObj == null)
		{
			Dbg.Log("The base was not loaded");
			return;
		}
		if (m_autoDelCharacter)
		{
			DeleteCharacters();
		}
		equipmentNames.Clear();
		equipmentNames.Add(m_defaultCEM);
		equipmentNames.Add(m_defaultBase);
		string[] parts = m_parts;
		foreach (string text in parts)
		{
			string empty = string.Empty;
			if (text == "weapon")
			{
				string text2 = m_setToLoad.Replace("set", "weapon");
				empty = m_weaponBundleLoc + text2 + "_" + text;
			}
			else
			{
				empty = m_maleBundleLoc + m_setToLoad + "_" + text;
			}
			equipmentNames.Add(empty);
		}
		m_equipIdx = 0;
		StartEquipmentDownload();
	}

	private void FileListInFolder(string _folder, ref List<string> _fileList, bool _includeSubFolder = false)
	{
		string[] files = Directory.GetFiles(_folder);
		string[] array = files;
		foreach (string item in array)
		{
			if (!_fileList.Contains(item))
			{
				_fileList.Add(item);
			}
		}
		if (_includeSubFolder)
		{
			string[] directories = Directory.GetDirectories(_folder);
			string[] array2 = directories;
			foreach (string folder in array2)
			{
				FileListInFolder(folder, ref _fileList);
			}
		}
	}
}
