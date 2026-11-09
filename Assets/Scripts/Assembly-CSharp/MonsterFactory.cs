using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MonsterFactory : Singleton<MonsterFactory>
{
	private string m_setupParam = "MonsterSetup";

	public string m_debugMonster = string.Empty;

	public string m_debugMonsterID = string.Empty;

	public string m_debugLevel = string.Empty;

	public string m_debugCurrentMonsterPrefabPath = string.Empty;

	public string[] m_debugStageList = new string[23]
	{
		"NatureLevel_Prefab_2", "FireLevel_Prefab_3", "WaterLevel_Prefab_3", "LightningLevel_Prefab_2", "NeutralLevel_Prefab_2", "NatureLevel_Prefab_3", "FireLevel_Prefab_1", "WaterLevel_Prefab_1", "LightningLevel_Prefab_3", "FireLevel_Prefab_2",
		"WaterLevel_Prefab_1", "NeutralLevel_Prefab_1", "NatureLevel_Prefab_1", "WaterLevel_Prefab_2", "FireLevel_Prefab_1", "LightningLevel_Prefab_1", "NeutralLevel_Prefab_3", "NatureLevel_Prefab_2", "LightningLevel_Prefab_2", "NeutralLevel_Prefab_3",
		"WaterLevel_Prefab_2", "FireLevel_Prefab_2", "LightningLevel_Prefab_1"
	};

	protected MonsterFactory()
	{
	}

	public GameObject CreateMonster(string p_name)
	{
		NBBinaryFormat parametersFor = SingletonComponent<ParameterManager>.Instance.GetParametersFor(m_setupParam);
		if (!parametersFor.ContainsRow(p_name))
		{
			Dbg.LogError("Cannot find configuration data for: " + p_name);
			return null;
		}
		Dbg.LogWarn("Creating monster: " + p_name);
		GameObject gameObject = null;
		string data = parametersFor.GetData(p_name, "PREFAB");
		Dbg.LogWarn("Instantiating monster prefab: " + data);
		Object obj = Resources.Load(data);
		if (obj == null)
		{
			Dbg.LogError("Invalid prefab path: " + data);
			return null;
		}
		gameObject = Object.Instantiate(obj) as GameObject;
		if (gameObject == null)
		{
			Dbg.LogError("Cannot instantiate character game-object.");
			return null;
		}
		gameObject.name = p_name;
		SkinnedMeshRenderer[] componentsInChildren = MBHier.GetComponentsInChildren<SkinnedMeshRenderer>(gameObject);
		if (componentsInChildren == null)
		{
			Dbg.LogError("Character does not have 'SkinnedMeshRenderer'!");
			return null;
		}
		data = parametersFor.GetData(p_name, "DIFFUSE_MAP");
		Texture texture = Resources.Load(data) as Texture;
		if (texture == null)
		{
			Dbg.LogError("Invalid texture parameter: " + data);
			return gameObject;
		}
		SkinnedMeshRenderer[] array = componentsInChildren;
		foreach (SkinnedMeshRenderer skinnedMeshRenderer in array)
		{
			skinnedMeshRenderer.material.SetTexture("_MainTex", texture);
			skinnedMeshRenderer.updateWhenOffscreen = true;
		}
		MonsterDamageDisplay component = gameObject.GetComponent<MonsterDamageDisplay>();
		if (component != null)
		{
			component.enabled = true;
		}
		else
		{
			Dbg.LogWarn(string.Concat(gameObject, " doesn't have 'MonsterDamageDisplay' component attached"));
		}
		return gameObject;
	}

	public void DestroyMonster(GameObject monster)
	{
		Object.DestroyImmediate(monster);
	}

	public string RandomCamera(string p_name, BattleCamera.CameraType p_camType)
	{
		NBBinaryFormat parametersFor = SingletonComponent<ParameterManager>.Instance.GetParametersFor(m_setupParam);
		if (!parametersFor.ContainsRow(p_name))
		{
			Dbg.LogError("Cannot find configuration data for: " + p_name);
			return null;
		}
		string[] array = new string[2] { "CAMERA_MAIN", "CAMERA_INTRO" };
		ParameterManager instance = SingletonComponent<ParameterManager>.Instance;
		string p_column = array[(int)p_camType];
		int numberItemInCell = instance.GetNumberItemInCell(m_setupParam, p_name, p_column);
		return instance.GetParameter(m_setupParam, p_name, p_column, Random.Range(0, numberItemInCell));
	}

	public string[] GetCameras(string p_name, BattleCamera.CameraType p_camType)
	{
		NBBinaryFormat parametersFor = SingletonComponent<ParameterManager>.Instance.GetParametersFor(m_setupParam);
		if (!parametersFor.ContainsRow(p_name))
		{
			Dbg.LogError("Cannot find configuration data for: " + p_name);
			return null;
		}
		string[] array = new string[2] { "CAMERA_MAIN", "CAMERA_INTRO" };
		ParameterManager instance = SingletonComponent<ParameterManager>.Instance;
		string p_column = array[(int)p_camType];
		int numberItemInCell = instance.GetNumberItemInCell(m_setupParam, p_name, p_column);
		string[] array2 = new string[numberItemInCell];
		for (int i = 0; i < numberItemInCell; i++)
		{
			array2[i] = instance.GetParameter(m_setupParam, p_name, p_column, i);
		}
		return array2;
	}

	public string[] GetExtraCameras(string p_name)
	{
		NBBinaryFormat parametersFor = SingletonComponent<ParameterManager>.Instance.GetParametersFor(m_setupParam);
		if (!parametersFor.ContainsRow(p_name))
		{
			Dbg.LogError("Cannot find configuration data for: " + p_name);
			return null;
		}
		ParameterManager instance = SingletonComponent<ParameterManager>.Instance;
		string p_column = "EXTRA_CAMERAS";
		int numberItemInCell = instance.GetNumberItemInCell(m_setupParam, p_name, p_column);
		string[] array = null;
		if (numberItemInCell > 0)
		{
			array = new string[numberItemInCell];
			for (int i = 0; i < numberItemInCell; i++)
			{
				array[i] = instance.GetParameter(m_setupParam, p_name, p_column, i);
			}
		}
		return array;
	}

	public string[] GetAvaiableTextures(string p_name)
	{
		string[] array = null;
		if (Application.platform != RuntimePlatform.Android)
		{
			NBBinaryFormat parametersFor = SingletonComponent<ParameterManager>.Instance.GetParametersFor(m_setupParam);
			if (!parametersFor.ContainsRow(p_name))
			{
				Dbg.LogError("Cannot find configuration data for: " + p_name);
				return null;
			}
			string p_columnName = "DIFFUSE_MAP";
			string data = parametersFor.GetData(p_name, p_columnName);
			string path = Application.dataPath + Path.GetDirectoryName("/Resources/" + data);
			array = Directory.GetFiles(path, "*.png");
			string text = "Resources/";
			int length = text.Length;
			int startIndex = array[0].IndexOf(text) + length;
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = array[i].Substring(startIndex);
				array[i] = array[i].Replace("\\", "/");
				array[i] = array[i].Replace(".png", string.Empty);
				array[i] = array[i].Replace(".tga", string.Empty);
			}
		}
		return array;
	}

	public string[] GetMonsterLists(MonsterSpecies p_species, Element.Type p_element)
	{
		if (p_species == MonsterSpecies.Unknown || p_element == Element.Type.None)
		{
			return null;
		}
		List<string> list = new List<string>();
		NBBinaryFormat parametersFor = SingletonComponent<ParameterManager>.Instance.GetParametersFor(m_setupParam);
		for (int i = 0; i < parametersFor.RowsCount; i++)
		{
			string text = parametersFor.RowsNames[i];
			string data = parametersFor.GetData(text, "SPECIES");
			string data2 = parametersFor.GetData(text, "ELEMENT");
			if (SpeciesParser.Parse(data) == p_species && Element.FromString(data2) == p_element)
			{
				list.Add(text);
			}
		}
		return list.ToArray();
	}

	public Element.Type[] GetMonsterElements(MonsterSpecies p_specie)
	{
		if (p_specie == MonsterSpecies.Unknown)
		{
			return null;
		}
		List<Element.Type> list = new List<Element.Type>();
		NBBinaryFormat parametersFor = SingletonComponent<ParameterManager>.Instance.GetParametersFor(m_setupParam);
		for (int i = 0; i < parametersFor.RowsCount; i++)
		{
			string p_rowName = parametersFor.RowsNames[i];
			string data = parametersFor.GetData(p_rowName, "SPECIES");
			if (SpeciesParser.Parse(data) == p_specie)
			{
				string data2 = parametersFor.GetData(p_rowName, "ELEMENT");
				Element.Type item = Element.FromString(data2);
				if (list.IndexOf(item) < 0)
				{
					list.Add(item);
				}
			}
		}
		return list.ToArray();
	}

	public string GetPlayList(string p_name)
	{
		NBBinaryFormat parametersFor = SingletonComponent<ParameterManager>.Instance.GetParametersFor(m_setupParam);
		return parametersFor.GetData(p_name, "PLAY_LIST");
	}

	public string GetBgm(string p_name)
	{
		NBBinaryFormat parametersFor = SingletonComponent<ParameterManager>.Instance.GetParametersFor(m_setupParam);
		return parametersFor.GetData(p_name, "BGM");
	}
}
