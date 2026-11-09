using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CharacterElement
{
	private struct Callback
	{
		public LoadCallback downloadCallback;

		public LoadCallback downloadCallbackFailed;

		public Callback(LoadCallback _downloadCallback, LoadCallback _downloadCallbackFailed)
		{
			downloadCallback = _downloadCallback;
			downloadCallbackFailed = _downloadCallbackFailed;
		}
	}

	public delegate void LoadCallback(CharacterElement _element);

	public string name;

	public string category;

	public string textureName;

	public string normalTextureName;

	public string bundleName;

	public string atlasName;

	public string iconName;

	private DLC dlc;

	private string m_bundlePath;

	private List<Callback> m_callbackList = new List<Callback>();

	private bool m_isLoaded;

	private bool m_isLoading;

	private static int m_objToLoad = 5;

	private int m_nRefCount;

	private int m_objLoadedCount;

	private GameObject m_mesh;

	private GameObject m_gachaMesh;

	private string[] m_boneNames;

	private string[] m_gachaBoneNames;

	private Texture m_texture;

	private AssetBundle m_assetbundle;

	public CharacterElement()
	{
		name = string.Empty;
		category = string.Empty;
		textureName = string.Empty;
		normalTextureName = string.Empty;
		bundleName = string.Empty;
		atlasName = string.Empty;
		iconName = string.Empty;
		dlc = null;
		m_isLoaded = false;
		m_isLoading = false;
		m_objLoadedCount = 0;
		m_mesh = null;
		m_boneNames = null;
		m_gachaMesh = null;
		m_gachaBoneNames = null;
		m_texture = null;
		m_nRefCount = 0;
	}

	private void OnBundleDownloadedFailed(BundleDownloadHandle _handle)
	{
		m_isLoaded = false;
		InvokeFailedCallbacks();
		UnLoadBundle();
		m_bundlePath = null;
		if (0 < m_nRefCount)
		{
			m_nRefCount--;
		}
	}

	private void OnBundleDownloaded(BundleDownloadHandle _handle)
	{
		_handle.LoadAssetAsync("rendererobject", typeof(GameObject), OnMeshDownloaded);
		_handle.LoadAssetAsync("bonenames", typeof(StringHolder), OnBonesDownloaded);
		_handle.LoadAssetAsync(textureName, typeof(Texture), OnTextureDowloaded);
		_handle.LoadAssetAsync("gacha", typeof(GameObject), OnGachaModelMeshDownloaded);
		_handle.LoadAssetAsync("gachabones", typeof(StringHolder), OnGachaBonesDownloaded);
	}

	public void LoadSync(WWW _www)
	{
		m_nRefCount++;
		m_assetbundle = _www.assetBundle;
		m_isLoaded = true;
		m_mesh = (GameObject)m_assetbundle.LoadAsset("rendererobject", typeof(GameObject));
		m_gachaMesh = (GameObject)m_assetbundle.LoadAsset("gacha", typeof(GameObject));
		StringHolder stringHolder = (StringHolder)m_assetbundle.LoadAsset("bonenames", typeof(StringHolder));
		m_boneNames = stringHolder.content;
		stringHolder = (StringHolder)m_assetbundle.LoadAsset("gachabones", typeof(StringHolder));
		if (stringHolder != null)
		{
			m_gachaBoneNames = stringHolder.content;
		}
		m_texture = (Texture)m_assetbundle.LoadAsset(textureName, typeof(Texture));
	}

	private string GetInfo(UnityEngine.Object _obj)
	{
		return _obj.name + "\t\t ";
	}

	private void OnMeshDownloaded(object _asset)
	{
		if (_asset == null)
		{
			m_mesh = null;
			m_objLoadedCount = 0;
			InvokeFailedCallbacks();
		}
		else
		{
			m_objLoadedCount++;
			m_mesh = (GameObject)_asset;
			CheckDownloadedSuccess();
		}
	}

	private void OnGachaModelMeshDownloaded(object _asset)
	{
		m_objLoadedCount++;
		m_gachaMesh = (GameObject)_asset;
		CheckDownloadedSuccess();
	}

	private void OnBonesDownloaded(object _asset)
	{
		if (_asset == null)
		{
			m_boneNames = null;
			m_objLoadedCount = 0;
			InvokeFailedCallbacks();
		}
		else
		{
			m_objLoadedCount++;
			m_boneNames = ((StringHolder)_asset).content;
			CheckDownloadedSuccess();
		}
	}

	private void OnGachaBonesDownloaded(object _asset)
	{
		if (_asset == null)
		{
			m_gachaBoneNames = null;
		}
		else
		{
			m_gachaBoneNames = ((StringHolder)_asset).content;
		}
		m_objLoadedCount++;
		CheckDownloadedSuccess();
	}

	private void OnTextureDowloaded(object _asset)
	{
		if (_asset == null)
		{
			m_texture = null;
			m_objLoadedCount = 0;
			InvokeFailedCallbacks();
		}
		else
		{
			m_objLoadedCount++;
			m_texture = (Texture)_asset;
			CheckDownloadedSuccess();
		}
	}

	private void CheckDownloadedSuccess()
	{
		if (m_objLoadedCount == m_objToLoad)
		{
			UnLoadBundle();
			m_isLoaded = true;
			m_objLoadedCount = 0;
			InvokeSucceededCallbacks();
		}
	}

	private void InvokeSucceededCallbacks()
	{
		while (0 < m_callbackList.Count)
		{
			if (m_callbackList[0].downloadCallback != null)
			{
				m_callbackList[0].downloadCallback(this);
			}
			m_callbackList.RemoveAt(0);
		}
		m_isLoading = false;
	}

	private void InvokeFailedCallbacks()
	{
		while (0 < m_callbackList.Count)
		{
			if (m_callbackList[0].downloadCallbackFailed != null)
			{
				m_callbackList[0].downloadCallbackFailed(this);
			}
			m_callbackList.RemoveAt(0);
		}
		m_isLoading = false;
		m_isLoading = false;
	}

	public void LoadBundle(LoadCallback _downloadSucceed, LoadCallback _downloadFailed)
	{
		if (m_nRefCount++ == 0)
		{
			dlc = SingletonComponent<DLC>.Instance;
			if (dlc == null)
			{
				if (_downloadFailed != null)
				{
					_downloadFailed(this);
				}
				return;
			}
			m_callbackList.Add(new Callback(_downloadSucceed, _downloadFailed));
			m_objLoadedCount = 0;
			m_isLoaded = false;
			m_isLoading = true;
			m_bundlePath = dlc.GetAssetBundle(bundleName);
			BundleDownloadHandle assetBundle = dlc.GetAssetBundle(m_bundlePath, m_bundlePath, OnBundleDownloaded, OnBundleDownloadedFailed);
			if (assetBundle == null)
			{
				OnBundleDownloadedFailed(null);
			}
		}
		else if (m_isLoading)
		{
			m_callbackList.Add(new Callback(_downloadSucceed, _downloadFailed));
		}
		else if (m_isLoaded)
		{
			if (_downloadSucceed != null)
			{
				_downloadSucceed(this);
			}
		}
		else if (_downloadFailed != null)
		{
			_downloadFailed(this);
		}
	}

	private void UnLoadBundle()
	{
		if (!(dlc == null))
		{
			dlc.Unload(m_bundlePath, m_bundlePath, false);
		}
	}

	public static void DestroyAssetBundleCharacterModel(GameObject obj, bool bDestroySharedResource = false)
	{
		SkinnedMeshRenderer skinnedMeshRenderer = obj.GetComponent<Renderer>() as SkinnedMeshRenderer;
		if (skinnedMeshRenderer != null)
		{
			if (bDestroySharedResource && skinnedMeshRenderer.sharedMesh != null)
			{
				UnityEngine.Object.DestroyImmediate(skinnedMeshRenderer.sharedMesh, true);
			}
			if (skinnedMeshRenderer.material != null && skinnedMeshRenderer.material != skinnedMeshRenderer.sharedMaterial)
			{
				UnityEngine.Object.DestroyImmediate(skinnedMeshRenderer.material, true);
			}
			if (bDestroySharedResource && skinnedMeshRenderer.sharedMaterial != null)
			{
				UnityEngine.Object.DestroyImmediate(skinnedMeshRenderer.sharedMaterial, true);
			}
		}
		UnityEngine.Object.DestroyImmediate(obj, true);
	}

	public void ReleaseAssets()
	{
		if (!m_isLoaded)
		{
			return;
		}
		Dbg.Assert(0 < m_nRefCount);
		if (--m_nRefCount == 0)
		{
			if (m_mesh != null)
			{
				DestroyAssetBundleCharacterModel(m_mesh, true);
				m_mesh = null;
			}
			if (m_texture != null)
			{
				UnityEngine.Object.DestroyImmediate(m_texture, true);
				m_texture = null;
			}
			m_boneNames = null;
			if (m_gachaMesh != null)
			{
				DestroyAssetBundleCharacterModel(m_gachaMesh, true);
				m_gachaMesh = null;
			}
			m_gachaBoneNames = null;
			m_objLoadedCount = 0;
			m_isLoaded = false;
		}
	}

	public void UnLoad()
	{
		if (m_isLoaded)
		{
			Dbg.Assert(0 < m_nRefCount);
			if (--m_nRefCount == 0)
			{
				m_mesh = null;
				m_boneNames = null;
				m_texture = null;
				m_assetbundle.Unload(true);
				m_assetbundle = null;
				m_objLoadedCount = 0;
			}
			if (m_assetbundle != null)
			{
				m_assetbundle.Unload(true);
			}
		}
	}

	public SkinnedMeshRenderer GetSkinnedMeshRenderer()
	{
		if (m_mesh == null)
		{
			return null;
		}
		GameObject gameObject = (GameObject)UnityEngine.Object.Instantiate(m_mesh);
		return (SkinnedMeshRenderer)gameObject.GetComponent<Renderer>();
	}

	public Texture GetDiffuseTexture()
	{
		return m_texture;
	}

	public string[] GetBoneNames(bool bGachaMesh = false)
	{
		if (bGachaMesh && m_gachaBoneNames != null)
		{
			return m_gachaBoneNames;
		}
		return m_boneNames;
	}

	public Mesh GetMesh(bool bGachaMesh = false)
	{
		if (bGachaMesh && m_gachaMesh != null)
		{
			return (m_gachaMesh.GetComponent<Renderer>() as SkinnedMeshRenderer).sharedMesh;
		}
		return (m_mesh.GetComponent<Renderer>() as SkinnedMeshRenderer).sharedMesh;
	}

	public Vector2 GetTextureScale()
	{
		string text = category.ToLower();
		if (text.Contains(Equipment.Shoulder) || text.Contains(Equipment.Weapon))
		{
			return new Vector2(4f, 2f);
		}
		return new Vector2(2f, 2f);
	}

	public Vector2 GetTextureOffset()
	{
		string text = category.ToLower();
		if (text.Contains(Equipment.Shoulder))
		{
			return new Vector2(-2f, -1f);
		}
		if (text.Contains(Equipment.Weapon))
		{
			return new Vector2(-3f, -1f);
		}
		if (text.Contains(Equipment.Body))
		{
			return new Vector2(-1f, 0f);
		}
		if (text.Contains(Equipment.Leg))
		{
			return new Vector2(0f, 0f);
		}
		if (text.Contains(Equipment.Head))
		{
			return new Vector2(0f, -1f);
		}
		return new Vector2(1f, 1f);
	}

	public override string ToString()
	{
		return string.Format("[CharacterElement]\t{0}\t\t{1}\t\t{2}\t\t{3}\t\t{4}\t\t{5}", name, category, textureName, bundleName, atlasName, iconName);
	}
}
