using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ParameterManager : SingletonComponent<ParameterManager>
{
	private string m_ParametersLocation = "Parameters/binary/client/";

	private string m_saveFolder = "Saved/";

	private string m_parameterConfig = "_clientFilesList.bin";

	private string m_ParametersMapFile = string.Empty;

	public string m_selectedFileType = string.Empty;

	public static string m_delimInCell = ",";

	public bool m_reloadAll;

	public bool m_reload;

	private Dictionary<string, NBBinaryFormat> m_csvParametersData;

	private Dictionary<string, string> m_parameterMap;

	private static bool m_triedGetParameter;

	public string ParameterLocation
	{
		get
		{
			return m_ParametersLocation;
		}
		set
		{
			m_ParametersLocation = value;
		}
	}

	public string ConfigFile
	{
		get
		{
			return m_parameterConfig;
		}
	}

	public Dictionary<string, string> ParameterMap
	{
		get
		{
			return m_parameterMap;
		}
	}

	public bool IsDataInitialized
	{
		get
		{
			return m_csvParametersData != null;
		}
	}

	public void InitManager()
	{
		NBProfiler.BeginTask("[PARAMETER_MANAGER][INIT_MANAGER]");
		m_reload = false;
		m_reloadAll = false;
		m_parameterMap = new Dictionary<string, string>();
		m_csvParametersData = new Dictionary<string, NBBinaryFormat>();
		m_ParametersMapFile = m_ParametersLocation + m_parameterConfig;
		ReloadAllCSV();
		NBProfiler.EndTask("[PARAMETER_MANAGER][INIT_MANAGER]");
	}

	private void ReloadAllCSV()
	{
		NBBinaryFormat nBBinaryFormat = new NBBinaryFormat();
		if (m_ParametersMapFile == string.Empty || nBBinaryFormat == null || m_csvParametersData == null)
		{
			Dbg.LogError("Parameter Manager was not initialized correctly: start initialization ... Initialize should be called first.");
		}
		else if (nBBinaryFormat.ReadFile(m_ParametersMapFile))
		{
			for (int i = 0; i < nBBinaryFormat.RowsCount; i++)
			{
				string data = nBBinaryFormat.GetData(i, 0);
				string text = nBBinaryFormat.RowName(i);
				UpdateDictionary(text, data, ref m_parameterMap);
				LoadCSV(text);
			}
		}
		else
		{
			Dbg.LogError("Could not load the config file for parameters.");
		}
	}

	private void LoadCSV(string p_parameterType)
	{
		if (!m_parameterMap.ContainsKey(p_parameterType))
		{
			Dbg.LogError("Could not find " + m_selectedFileType + " in parameterMap.");
			return;
		}
		string p_fileName = m_ParametersLocation + m_parameterMap[p_parameterType] + ".bin";
		NBBinaryFormat nBBinaryFormat = new NBBinaryFormat();
		if (nBBinaryFormat.ReadFile(p_fileName))
		{
			UpdateDictionary(p_parameterType, nBBinaryFormat, ref m_csvParametersData);
		}
	}

	private void UpdateDictionary<T>(string p_key, T p_value, ref Dictionary<string, T> p_dictionary)
	{
		if (!p_dictionary.ContainsKey(p_key))
		{
			p_dictionary.Add(p_key, p_value);
		}
		else
		{
			p_dictionary[p_key] = p_value;
		}
	}

	public bool SaveParameter(string p_key)
	{
		if (m_csvParametersData.ContainsKey(p_key))
		{
			Dbg.Log("Saving parameters : " + p_key);
			return m_csvParametersData[p_key].SaveFile(m_ParametersLocation + m_saveFolder + m_parameterMap[p_key]);
		}
		return false;
	}

	public bool SaveParameters()
	{
		foreach (string key in m_csvParametersData.Keys)
		{
			m_csvParametersData[key].SaveFile(m_ParametersLocation + m_saveFolder + m_parameterMap[key]);
		}
		return true;
	}

	public string GetParameter(string p_key, string p_row, string p_column)
	{
		return GetParameter(p_key, p_row, p_column, 0);
	}

	public string GetParameter(string p_key, string p_row, string p_column, int p_dataIndex)
	{
		if (m_csvParametersData == null)
		{
			Dbg.LogWarn("Data was not initialized");
			return string.Empty;
		}
		if (!m_csvParametersData.ContainsKey(p_key))
		{
			Dbg.LogWarn("Could not find " + p_key + " in the data.");
			return string.Empty;
		}
		NBBinaryFormat nBBinaryFormat = m_csvParametersData[p_key];
		string data = nBBinaryFormat.GetData(p_row, p_column);
		if (data.Contains(m_delimInCell))
		{
			string[] array = data.Split(m_delimInCell.ToCharArray());
			if (array.Length < p_dataIndex)
			{
				return string.Empty;
			}
			return array[p_dataIndex].Trim(' ');
		}
		return data;
	}

	public int GetNumberItemInCell(string p_key, string p_row, string p_column)
	{
		if (m_csvParametersData == null)
		{
			Dbg.LogWarn("Data was not initialized");
			return -1;
		}
		if (!m_csvParametersData.ContainsKey(p_key))
		{
			Dbg.LogWarn("Could not find " + p_key + " in the data.");
			return -1;
		}
		NBBinaryFormat nBBinaryFormat = m_csvParametersData[p_key];
		string data = nBBinaryFormat.GetData(p_row, p_column);
		int result = 0;
		if (data != string.Empty)
		{
			result = data.Split(m_delimInCell.ToCharArray()).Length;
		}
		return result;
	}

	public int GetNumberItemInCell(string p_key, int p_row, int p_column)
	{
		if (m_csvParametersData == null)
		{
			Dbg.LogWarn("Data was not initialized");
			return -1;
		}
		if (!m_csvParametersData.ContainsKey(p_key))
		{
			Dbg.LogWarn("Could not find " + p_key + " in the data.");
			return -1;
		}
		NBBinaryFormat nBBinaryFormat = m_csvParametersData[p_key];
		string p_rowName = nBBinaryFormat.RowName(p_row);
		string p_columnName = nBBinaryFormat.ColumnName(p_column);
		string data = nBBinaryFormat.GetData(p_rowName, p_columnName);
		int result = 0;
		if (data != string.Empty)
		{
			result = data.Split(m_delimInCell.ToCharArray()).Length;
		}
		return result;
	}

	public NBBinaryFormat GetParametersFor(string p_key)
	{
		if (m_csvParametersData == null)
		{
			Dbg.LogWarn("Data was not initialized");
			return null;
		}
		if (!m_csvParametersData.ContainsKey(p_key))
		{
			Dbg.LogWarn("Could not find " + p_key + " in the data. Try to load it!");
			if (m_triedGetParameter)
			{
				m_triedGetParameter = false;
				Dbg.LogError("Abort : The parameter file you try to load cannot be found.");
				return null;
			}
			LoadCSV(p_key);
			m_triedGetParameter = true;
			return GetParametersFor(p_key);
		}
		m_triedGetParameter = false;
		return m_csvParametersData[p_key];
	}

	public List<string> GetRowsNamesFor(string p_key)
	{
		if (m_csvParametersData == null)
		{
			Dbg.LogWarn("Data was not initialized");
			return null;
		}
		if (!m_csvParametersData.ContainsKey(p_key))
		{
			Dbg.LogWarn("Could not find " + p_key + " in the data.");
			return null;
		}
		return m_csvParametersData[p_key].RowsNames;
	}

	public void UnloadParametersFor(string p_key)
	{
		if (m_csvParametersData == null)
		{
			Dbg.LogWarn("Data was not initialized");
			return;
		}
		if (!m_csvParametersData.ContainsKey(p_key))
		{
			Dbg.LogWarn("Could not find " + p_key + " in the data. It never loaded.");
			return;
		}
		m_csvParametersData[p_key].Unload();
		m_csvParametersData.Remove(p_key);
	}

	private void Update()
	{
		if (m_reload)
		{
			Dbg.Log("Reload " + m_selectedFileType);
			LoadCSV(m_selectedFileType);
			m_reload = false;
		}
		if (m_reloadAll)
		{
			Dbg.Log("Reload all Parameters files");
			ReloadAllCSV();
			m_reloadAll = false;
		}
	}
}
