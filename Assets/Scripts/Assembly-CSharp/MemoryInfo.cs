using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class MemoryInfo : SingletonComponent<MemoryInfo>
{
	public class Info
	{
		public string name = string.Empty;

		public long size;

		public bool isInObject;
	}

	public class InfoType
	{
		private Type type;

		private long m_size;

		private List<Info> m_infos = new List<Info>();

		public bool m_showList;

		public long Size
		{
			get
			{
				return m_size;
			}
		}

		public Type Type
		{
			get
			{
				return type;
			}
		}

		public int Count
		{
			get
			{
				return m_infos.Count;
			}
		}

		public InfoType(Type _type = null)
		{
			type = _type;
			m_size = 0L;
		}

		public void AddInfo(Info _info)
		{
			m_size += _info.size;
			m_infos.Add(_info);
		}

		public void ClearInfo()
		{
			m_size = 0L;
			m_infos.Clear();
		}

		public bool IncludeName(string _name)
		{
			foreach (Info info in m_infos)
			{
				if (info.name == _name)
				{
					return true;
				}
			}
			return false;
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (m_showList)
			{
				foreach (Info info in m_infos)
				{
					float num = NBProfiler.BytesToMegaByte(info.size);
					stringBuilder.Append(string.Format("{0}\t{1}\t\t{2:0.00}MB\n", info.isInObject, info.name, num));
				}
			}
			string arg = string.Empty;
			if (type != null)
			{
				arg = type.ToString();
			}
			float num2 = NBProfiler.BytesToMegaByte(m_size);
			string text = string.Format("{0}\t\tCount:{1}\t\tSize:{2:0.00}MB\n", arg, m_infos.Count, num2);
			if (m_showList)
			{
				text = text + stringBuilder.ToString() + "\n\n";
			}
			stringBuilder = null;
			return text;
		}
	}

	public int m_refreshRate = 10;

	public bool m_useRefreshRate;

	public bool m_showGao = true;

	public bool m_showTexture = true;

	public bool m_showMesh = true;

	public bool m_showMeshList = true;

	public bool m_showMaterial = true;

	public bool m_showAnim = true;

	public bool m_showAnimList = true;

	public bool m_showAudio = true;

	public bool m_showAudioList = true;

	public bool m_PRINT_MEMORY_INFO;

	public bool m_showAtlasInfo = true;

	public bool m_showUnamedFile;

	public bool m_showSmallFile;

	public float m_smallFileSize = 0.1f;

	public float m_smallTextureSize = 0.5f;

	public bool m_findUISprite;

	public InfoType m_objectInfo = new InfoType(typeof(UnityEngine.Object));

	public InfoType m_gaoInfo = new InfoType(typeof(GameObject));

	public InfoType m_textureInfo = new InfoType(typeof(Texture));

	public InfoType m_meshInfo = new InfoType(typeof(Mesh));

	public InfoType m_audioInfo = new InfoType(typeof(AudioClip));

	public InfoType m_materialInfo = new InfoType(typeof(Material));

	public InfoType m_animationInfo = new InfoType(typeof(AnimationClip));

	public InfoType m_textureAtlas = new InfoType(typeof(Texture));

	public InfoType m_otherTextures = new InfoType(typeof(Texture));

	public InfoType m_unnamedFile = new InfoType(typeof(UnityEngine.Object));

	public InfoType m_smallFile = new InfoType(typeof(UnityEngine.Object));

	private string m_atlasInfo = string.Empty;

	private int m_particleCount;

	private float m_currentTime;

	private void Start()
	{
		base.enabled = false;
		Dbg.LogWarn("Memory Info Disable!", Dbg.NBLogType.MEMORY_INFO);
	}

	private void Update()
	{
		if (m_useRefreshRate)
		{
			m_currentTime += Time.deltaTime;
		}
		if ((m_currentTime >= (float)m_refreshRate && m_refreshRate > 0) || m_PRINT_MEMORY_INFO)
		{
			RefreshMemoryInfo();
			m_currentTime = 0f;
			m_PRINT_MEMORY_INFO = false;
		}
		if (m_findUISprite)
		{
			FindUISpriteInScene();
			m_findUISprite = false;
		}
	}

	public void RefreshMemoryInfo()
	{
		ClearAllInfo();
		FindAllInfo();
		StringBuilder stringBuilder = new StringBuilder();
		string text = string.Format("{0:0.00}MB", NBProfiler.HeapSize);
		stringBuilder.Append(" ************ \nHEAP MEMORY : \t\t" + text + "\n");
		stringBuilder.Append("MANAGED MEMORY :\t\t" + NBProfiler.BytesToMegaByte(GC.GetTotalMemory(false)) + "MB\n");
		stringBuilder.Append(m_objectInfo.ToString());
		if (m_showGao)
		{
			stringBuilder.Append(m_gaoInfo.ToString());
		}
		if (m_showTexture)
		{
			stringBuilder.Append(m_textureInfo.ToString());
		}
		if (m_showMesh)
		{
			m_meshInfo.m_showList = m_showMeshList;
			stringBuilder.Append(m_meshInfo.ToString());
		}
		if (m_showAudio)
		{
			m_audioInfo.m_showList = m_showAudioList;
			stringBuilder.Append(m_audioInfo.ToString());
		}
		if (m_showMaterial)
		{
			stringBuilder.Append(m_materialInfo.ToString());
		}
		if (m_showAnim)
		{
			m_animationInfo.m_showList = m_showAnimList;
			stringBuilder.Append(m_animationInfo.ToString());
		}
		stringBuilder.Append("\nParticles Count :\t" + m_particleCount + "\n");
		if (m_showTexture)
		{
			m_textureAtlas.m_showList = true;
			stringBuilder.Append("TextureAtlas:\n" + m_textureAtlas.ToString());
			m_otherTextures.m_showList = true;
			stringBuilder.Append("OtherTextures:\n" + m_otherTextures.ToString() + "\n");
		}
		if (m_showAtlasInfo)
		{
			stringBuilder.Append("AtlasInfo:\n" + m_atlasInfo + "\n");
		}
		if (m_showUnamedFile)
		{
			stringBuilder.Append("UnnamedFiles:\n" + m_unnamedFile.ToString() + "\n");
		}
		if (m_showSmallFile)
		{
			stringBuilder.Append("SmallFiles:\n" + m_smallFile.ToString() + "\n");
		}
		Dbg.Log(stringBuilder.ToString(), Dbg.NBLogType.MEMORY_INFO);
		stringBuilder = null;
	}

	private void ClearAllInfo()
	{
		m_objectInfo.ClearInfo();
		m_gaoInfo.ClearInfo();
		m_textureInfo.ClearInfo();
		m_meshInfo.ClearInfo();
		m_audioInfo.ClearInfo();
		m_materialInfo.ClearInfo();
		m_animationInfo.ClearInfo();
		m_textureAtlas.ClearInfo();
		m_otherTextures.ClearInfo();
		m_unnamedFile.ClearInfo();
		m_smallFile.ClearInfo();
	}

	private InfoType GetInfosFrom<T>(ref UnityEngine.Object[] _objects, string _pattern = "", bool _usefilterFlags = false, bool _inclusivePattern = true, float _filterSize = -1f)
	{
		InfoType infoType = new InfoType(typeof(T));
		if (_filterSize < 0f)
		{
			_filterSize = m_smallFileSize;
		}
		UnityEngine.Object[] array = _objects;
		foreach (UnityEngine.Object obj in array)
		{
			int runtimeMemorySize = NBProfiler.GetRuntimeMemorySize(obj);
			string text = obj.name;
			if ((!_usefilterFlags || (!(NBProfiler.BytesToMegaByte(runtimeMemorySize) < _filterSize) && !(text == string.Empty))) && (!(_pattern != string.Empty) || ((text.ToLower().Contains(_pattern) || !_inclusivePattern) && (!text.ToLower().Contains(_pattern) || _inclusivePattern))))
			{
				Info info = new Info();
				info.name = obj.name;
				info.size = runtimeMemorySize;
				if (m_objectInfo.IncludeName(info.name))
				{
					info.isInObject = true;
				}
				infoType.AddInfo(info);
			}
		}
		return infoType;
	}

	private void FindAtlasInfo()
	{
		if (!m_showAtlasInfo)
		{
			return;
		}
		UISprite[] array = (UISprite[])Resources.FindObjectsOfTypeAll(typeof(UISprite));
		StringBuilder stringBuilder = new StringBuilder();
		UISprite[] array2 = array;
		foreach (UISprite uISprite in array2)
		{
			if (!(uISprite == null) && !(uISprite.atlas == null))
			{
				stringBuilder.Append(uISprite.atlas.name);
				stringBuilder.Append("\t\tgao: ");
				stringBuilder.Append(uISprite.gameObject.name);
				stringBuilder.Append("\t\troot: ");
				stringBuilder.Append(uISprite.transform.root.name);
				stringBuilder.Append("\n");
			}
		}
		m_atlasInfo = stringBuilder.ToString();
		stringBuilder = null;
	}

	public List<T> FindAll<T>(float _filterSize = -1f) where T : UnityEngine.Object
	{
		T[] array = Resources.FindObjectsOfTypeAll(typeof(T)) as T[];
		if (_filterSize < 0f)
		{
			return new List<T>(array);
		}
		List<T> list = new List<T>();
		T[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			T val = array2[i];
			int runtimeMemorySize = NBProfiler.GetRuntimeMemorySize(val);
			string text = val.name;
			if (!(NBProfiler.BytesToMegaByte(runtimeMemorySize) < _filterSize) && !(text == string.Empty))
			{
				list.Add(val);
			}
		}
		return list;
	}

	private void FindAllInfo()
	{
		UnityEngine.Object[] array = Resources.FindObjectsOfTypeAll(typeof(UnityEngine.Object));
		UnityEngine.Object[] array2 = array;
		foreach (UnityEngine.Object obj in array2)
		{
			Info info = new Info();
			info.name = obj.name;
			info.size = 0L;
			info.size = NBProfiler.GetRuntimeMemorySize(obj);
			m_objectInfo.AddInfo(info);
			if (NBProfiler.BytesToMegaByte(info.size) < m_smallFileSize)
			{
				m_smallFile.AddInfo(info);
			}
			if (obj.name == string.Empty)
			{
				m_unnamedFile.AddInfo(info);
			}
		}
		if (m_showGao)
		{
			UnityEngine.Object[] _objects = Resources.FindObjectsOfTypeAll(typeof(GameObject));
			m_gaoInfo = GetInfosFrom<GameObject>(ref _objects, string.Empty);
			_objects = null;
		}
		if (m_showTexture)
		{
			FindAtlasInfo();
			UnityEngine.Object[] _objects2 = Resources.FindObjectsOfTypeAll(typeof(Texture));
			m_textureInfo = GetInfosFrom<Texture>(ref _objects2, string.Empty);
			m_textureAtlas = GetInfosFrom<Texture>(ref _objects2, "atlas");
			m_otherTextures = GetInfosFrom<Texture>(ref _objects2, "atlas", true, false, m_smallTextureSize);
			_objects2 = null;
		}
		if (m_showMesh)
		{
			UnityEngine.Object[] _objects3 = Resources.FindObjectsOfTypeAll(typeof(Mesh));
			m_meshInfo = GetInfosFrom<Mesh>(ref _objects3, string.Empty);
			_objects3 = null;
		}
		if (m_showAudio)
		{
			UnityEngine.Object[] _objects4 = Resources.FindObjectsOfTypeAll(typeof(AudioClip));
			m_audioInfo = GetInfosFrom<AudioClip>(ref _objects4, string.Empty);
			_objects4 = null;
		}
		if (m_showMaterial)
		{
			UnityEngine.Object[] _objects5 = Resources.FindObjectsOfTypeAll(typeof(Material));
			m_materialInfo = GetInfosFrom<Material>(ref _objects5, string.Empty);
			_objects5 = null;
		}
		if (m_showAnim)
		{
			UnityEngine.Object[] _objects6 = Resources.FindObjectsOfTypeAll(typeof(AnimationClip));
			m_animationInfo = GetInfosFrom<AnimationClip>(ref _objects6, string.Empty);
			_objects6 = null;
		}
		m_particleCount = 0;
		ParticleSystem[] array3 = (ParticleSystem[])Resources.FindObjectsOfTypeAll(typeof(ParticleSystem));
		ParticleSystem[] array4 = array3;
		foreach (ParticleSystem particleSystem in array4)
		{
			m_particleCount += particleSystem.particleCount;
		}
	}

	private void FindUISpriteInScene()
	{
		UISprite[] array = UnityEngine.Object.FindSceneObjectsOfType(typeof(UISprite)) as UISprite[];
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(" FindInScene Type : " + typeof(UISprite).ToString());
		UISprite[] array2 = array;
		foreach (UISprite uISprite in array2)
		{
			stringBuilder.Append(uISprite.name);
			if (uISprite.atlas != null)
			{
				stringBuilder.Append("\t\tatlas: ");
				stringBuilder.Append(uISprite.atlas.name);
			}
			stringBuilder.Append("\t\tgao: ");
			stringBuilder.Append(uISprite.gameObject.name);
			stringBuilder.Append("\t\troot: ");
			stringBuilder.Append(uISprite.transform.root.name);
			stringBuilder.Append("\n");
		}
		Dbg.Log(stringBuilder.ToString(), Dbg.NBLogType.MEMORY_INFO);
	}

	private void FindInScene<T>() where T : MonoBehaviour
	{
		T[] array = UnityEngine.Object.FindSceneObjectsOfType(typeof(T)) as T[];
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(" FindInScene Type : " + typeof(T).ToString());
		T[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			T val = array2[i];
			stringBuilder.Append(val.name);
			if (val.gameObject != null)
			{
				stringBuilder.Append("\t\tgao: ");
				stringBuilder.Append(val.gameObject.name);
			}
			if (val.transform != null && val.transform.root != null)
			{
				stringBuilder.Append("\t\troot: ");
				stringBuilder.Append(val.transform.root.name);
			}
			stringBuilder.Append("\n");
		}
		Dbg.Log(stringBuilder.ToString(), Dbg.NBLogType.MEMORY_INFO);
	}
}
