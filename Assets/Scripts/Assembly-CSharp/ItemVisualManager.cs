using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemVisualManager : SingletonComponent<ItemVisualManager>
{
	public delegate void CompletedCB(bool _succeed);

	private const string m_elementSpritePrefix = "frame";

	private string m_atlasPattern = "atlas";

	private List<string> m_atlasBundles;

	private int m_downloaded;

	private Dictionary<string, UIAtlas> m_atlasMap = new Dictionary<string, UIAtlas>(15);

	private Dictionary<string, string> m_atlasNameToBundle = new Dictionary<string, string>();

	public UIAtlas m_elementAtlas;

	public UIAtlas m_globalIcons;

	public string m_errorSpriteName = string.Empty;

	public string m_releaseErrorSprite = string.Empty;

	private Dictionary<Element.Type, string> m_elementToSpriteName;

	private InventoryManager m_inventoryManager;

	private ItemManager m_itemManager;

	private DLC m_dlc;

	private int m_atlasToDownload;

	private bool m_loaded;

	private bool m_equipLoaded;

	private CompletedCB m_completedCB;

	private bool m_bCancelPreload;

	private bool m_bPreload;

	public bool IsLoaded
	{
		get
		{
			return m_loaded;
		}
	}

	public bool IsEquipmentLoaded
	{
		get
		{
			return m_equipLoaded;
		}
	}

	private bool CheckInput()
	{
		m_inventoryManager = SingletonComponent<InventoryManager>.Instance;
		if (m_inventoryManager == null)
		{
			return false;
		}
		m_itemManager = SingletonComponent<ItemManager>.Instance;
		if (m_itemManager == null)
		{
			return false;
		}
		m_dlc = SingletonComponent<DLC>.Instance;
		if (m_dlc == null)
		{
			return false;
		}
		return true;
	}

	private void AddDefaulfAtlas()
	{
		if (!m_atlasMap.ContainsKey(m_globalIcons.name))
		{
			m_atlasMap.Add(m_globalIcons.name, m_globalIcons);
			m_errorSpriteName = m_releaseErrorSprite;
		}
	}

	private void UpdateAtlasMap(BundleDownloadHandle _handle)
	{
		object obj = _handle.LoadMainAsset();
		if (m_dlc != null)
		{
			m_dlc.Unload(_handle.filename, _handle.filename, false);
		}
		if (obj == null)
		{
			return;
		}
		GameObject gameObject = (GameObject)obj;
		string key = gameObject.name.ToLower();
		if (obj.GetType() == typeof(GameObject) && !m_atlasMap.ContainsKey(key))
		{
			UIAtlas component = gameObject.GetComponent<UIAtlas>();
			if (!(component == null))
			{
				m_atlasMap.Add(key, component);
				m_atlasNameToBundle.Add(key, _handle.filename);
			}
		}
	}

	private void OnAtlasDownloaded(BundleDownloadHandle _handle)
	{
		UpdateAtlasMap(_handle);
		m_downloaded++;
		if (m_downloaded >= m_atlasToDownload)
		{
			NBProfiler.EndTask("[ITEM_VISUAL_MANAGER][LOAD_ALL_ATLAS]");
			NBProfiler.EndTask("[ITEM_VISUAL_MANAGER][INIT_MANAGER]");
			m_loaded = true;
			if (m_completedCB != null)
			{
				m_completedCB(true);
			}
		}
	}

	private void OnAtlasDownloadedFailed(BundleDownloadHandle _handle)
	{
		if (m_completedCB != null)
		{
			m_completedCB(false);
		}
		m_downloaded = 0;
		if (m_bPreload)
		{
			CancelPreload();
		}
	}

	private void FindElementsSpriteName()
	{
		if (m_elementToSpriteName != null)
		{
			return;
		}
		if (m_elementAtlas == null)
		{
			Dbg.LogError("No Elemental Atlas given.");
			return;
		}
		List<string> listOfSprites = m_elementAtlas.GetListOfSprites();
		m_elementToSpriteName = new Dictionary<Element.Type, string>();
		foreach (string item in listOfSprites)
		{
			string[] array = item.Split('_');
			if (array.Length >= 2 && !item.Contains("pet") && item.Contains("actual") && array[0] == "frame" && array[0] != "generic")
			{
				Element.Type key = Element.FromString(array[1]);
				if (!m_elementToSpriteName.ContainsKey(key))
				{
					m_elementToSpriteName.Add(key, item);
				}
			}
		}
		if (m_elementToSpriteName.Count != 5)
		{
			for (int i = 0; i < 5; i++)
			{
				Element.Type type = (Element.Type)i;
				if (!m_elementToSpriteName.ContainsKey(type))
				{
					Dbg.LogWarn("No icon for this type : " + type);
					m_elementToSpriteName.Add(type, string.Empty);
				}
			}
		}
		Dbg.Log(" NULL ELEMENT ATLAS ");
		m_elementAtlas = null;
	}

	private void IsIconDisable()
	{
	}

	public void PreloadAssetBundles()
	{
		if (!CheckInput())
		{
			base.enabled = false;
			return;
		}
		m_bPreload = true;
		m_bCancelPreload = false;
		m_loaded = false;
		m_equipLoaded = false;
		NBProfiler.BeginTask("[ITEM_VISUAL_MANAGER][INIT_MANAGER]");
		FindElementsSpriteName();
		m_atlasBundles = m_dlc.GetAssetBundleList(m_atlasPattern);
		m_atlasToDownload = m_atlasBundles.Count;
		LoadAllAtlas(null);
	}

	public void CancelPreload()
	{
		m_bPreload = false;
		m_bCancelPreload = true;
		Dbg.Log(" ItemVisualManager preload canceled ");
	}

	public void InitManager(CompletedCB _completedCB = null)
	{
		Dbg.Log(" ItemVisualManager INIT ");
		if (!CheckInput())
		{
			base.enabled = false;
			return;
		}
		if (m_bPreload)
		{
			if (_completedCB != null)
			{
				if (m_loaded)
				{
					_completedCB(true);
				}
				else
				{
					m_completedCB = _completedCB;
				}
			}
			return;
		}
		NBProfiler.BeginTask("[ITEM_VISUAL_MANAGER][INIT_MANAGER]");
		m_bCancelPreload = false;
		m_atlasMap.Clear();
		m_atlasNameToBundle.Clear();
		m_loaded = false;
		m_equipLoaded = false;
		FindElementsSpriteName();
		m_atlasBundles = m_dlc.GetAssetBundleList(m_atlasPattern);
		m_atlasToDownload = m_atlasBundles.Count;
		LoadAllAtlas(_completedCB);
	}

	private void OnApplicationPause(bool p_pause)
	{
		if (m_atlasMap == null)
		{
			return;
		}
		if (p_pause)
		{
			UIDrawCall[] array = (UIDrawCall[])UnityEngine.Object.FindObjectsOfType(typeof(UIDrawCall));
			{
				foreach (UIAtlas value in m_atlasMap.Values)
				{
					if (!(value != m_globalIcons))
					{
						continue;
					}
					bool flag = false;
					UIDrawCall[] array2 = array;
					foreach (UIDrawCall uIDrawCall in array2)
					{
						if (uIDrawCall.GetComponent<Renderer>() != null && uIDrawCall.GetComponent<Renderer>().sharedMaterial != null && uIDrawCall.GetComponent<Renderer>().sharedMaterial.mainTexture == value.spriteMaterial.mainTexture)
						{
							flag = true;
						}
					}
					if (!flag)
					{
						UnityEngine.Object.DestroyImmediate(value.spriteMaterial.mainTexture, true);
						value.spriteMaterial.mainTexture = null;
					}
				}
				return;
			}
		}
		ReloadAssetBundles();
	}

	public void ReloadAssetBundles()
	{
		if (m_atlasBundles == null)
		{
			return;
		}
		foreach (string atlasBundle in m_atlasBundles)
		{
			m_dlc.GetAssetBundle(atlasBundle, atlasBundle, OnAtlasReloaded);
		}
	}

	private void OnAtlasReloaded(BundleDownloadHandle _handle)
	{
		object obj = _handle.LoadMainAsset();
		if (m_dlc != null)
		{
			m_dlc.Unload(_handle.filename, _handle.filename, false);
		}
		if (obj == null)
		{
			return;
		}
		GameObject gameObject = (GameObject)obj;
		string key = gameObject.name.ToLower();
		if (obj.GetType() != typeof(GameObject))
		{
			return;
		}
		UIAtlas component = gameObject.GetComponent<UIAtlas>();
		UIAtlas value;
		if (!(component == null) && m_atlasMap.TryGetValue(key, out value))
		{
			if (value.spriteMaterial.mainTexture != component.spriteMaterial.mainTexture)
			{
				value.spriteMaterial.mainTexture = component.spriteMaterial.mainTexture;
			}
			component.spriteMaterial.mainTexture = null;
			UnityEngine.Object.DestroyImmediate(component, true);
		}
	}

	public UIAtlas GetAtlas(string _atlasName)
	{
		UIAtlas value = null;
		if (!m_atlasMap.TryGetValue(_atlasName.ToLower(), out value))
		{
			value = m_globalIcons;
		}
		return value;
	}

	public bool GetVisual(string _atlasName, string _spriteName, out UIAtlas _outAtlas, out string _outSpriteName)
	{
		if (!m_atlasMap.TryGetValue(_atlasName.ToLower(), out _outAtlas) || !m_loaded)
		{
			_outAtlas = m_globalIcons;
			_outSpriteName = m_errorSpriteName;
			return false;
		}
		if (!_outAtlas.Contains(_spriteName))
		{
			Dbg.LogError("Could not find sprite name : " + _spriteName + " in atlas " + _outAtlas.name);
			_outAtlas = m_globalIcons;
			_outSpriteName = m_errorSpriteName;
			return false;
		}
		_outSpriteName = _spriteName;
		return true;
	}

	public void GetVisualError(out UIAtlas _outAtlas, out string _outSpriteName)
	{
		_outAtlas = m_globalIcons;
		_outSpriteName = m_errorSpriteName;
	}

	public string GetElementSprite(Element.Type _type)
	{
		string value = string.Empty;
		if (!m_elementToSpriteName.TryGetValue(_type, out value))
		{
			Dbg.LogError("Could not find Sprite for this element : " + _type);
		}
		return value;
	}

	public void LoadAtlasList(List<string> _atlases, CompletedCB _completedCB)
	{
		if (IsLoaded || IsEquipmentLoaded)
		{
			_completedCB(true);
			return;
		}
		AddDefaulfAtlas();
		m_completedCB = _completedCB;
		m_downloaded = 0;
		string text = string.Empty;
		m_atlasToDownload = _atlases.Count;
		foreach (string _atlase in _atlases)
		{
			if (m_bCancelPreload)
			{
				return;
			}
			text = text + _atlase + "\n";
			m_dlc.GetAssetBundle(_atlase, _atlase, OnAtlasDownloaded, OnAtlasDownloadedFailed);
		}
		Dbg.Log(" --> Atlas List to Download from DLC <--\n" + text);
		m_equipLoaded = true;
	}

	public void LoadAllAtlas(CompletedCB _completedCB)
	{
		if (IsLoaded)
		{
			_completedCB(true);
			return;
		}
		AddDefaulfAtlas();
		m_completedCB = _completedCB;
		m_downloaded = 0;
		NBProfiler.BeginTask("[ITEM_VISUAL_MANAGER][LOAD_ALL_ATLAS]");
		m_atlasToDownload = m_atlasBundles.Count;
		foreach (string atlasBundle in m_atlasBundles)
		{
			m_dlc.GetAssetBundle(atlasBundle, atlasBundle, OnAtlasDownloaded, OnAtlasDownloadedFailed);
		}
	}

	public string ContainInBundle(string _atlasName)
	{
		string value = string.Empty;
		if (!m_atlasNameToBundle.TryGetValue(_atlasName, out value))
		{
			return string.Empty;
		}
		return value;
	}

	public void UnloadAllAtlas()
	{
		if (m_loaded)
		{
			m_atlasMap.Clear();
			m_atlasNameToBundle.Clear();
			Resources.UnloadUnusedAssets();
			GC.Collect();
			m_loaded = false;
		}
	}
}
