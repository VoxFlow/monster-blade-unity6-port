using System;
using System.Collections.Generic;
using UnityEngine;

public class NBCSVFormat
{
	private Dictionary<string, int> m_columnNameMap;

	private List<string> m_rowName;

	private Dictionary<string, int> m_rowNameMap;

	private List<List<string>> m_parameters;

	private string m_commentChar = "#";

	private string m_fieldDelim = "^";

	private string m_stringDelim = "\"";

	private string m_inCellDelim = ParameterManager.m_delimInCell;

	public int RowsCount
	{
		get
		{
			return m_rowName.Count;
		}
	}

	public List<string> RowsNames
	{
		get
		{
			return m_rowName;
		}
	}

	public int ColumnsCount
	{
		get
		{
			return m_columnNameMap.Count;
		}
	}

	public Dictionary<string, int> ColumnsNames
	{
		get
		{
			return m_columnNameMap;
		}
	}

	public List<List<string>> SetParameters
	{
		set
		{
			m_parameters = value;
		}
	}

	public NBCSVFormat()
	{
		m_rowName = new List<string>();
		m_rowNameMap = new Dictionary<string, int>();
		m_columnNameMap = new Dictionary<string, int>();
		m_parameters = new List<List<string>>();
	}

	public List<string> RowNameWithPattern(string _pattern)
	{
		string value = _pattern.ToLower();
		List<string> list = new List<string>();
		if (m_rowName == null)
		{
			return list;
		}
		foreach (string item in m_rowName)
		{
			if (item.ToLower().Contains(value))
			{
				list.Add(item);
			}
		}
		return list;
	}

	public bool ContainsRow(string p_rowName)
	{
		return m_rowName.Contains(p_rowName);
	}

	public bool ContainsColumn(string p_columnName)
	{
		return m_columnNameMap.ContainsKey(p_columnName);
	}

	public void GetColumn(string p_columnName, ref List<string> p_columns)
	{
		int num = -1;
		if (m_columnNameMap.ContainsKey(p_columnName))
		{
			num = m_columnNameMap[p_columnName];
		}
		if (num == -1 || p_columns == null)
		{
			Dbg.LogError("This parameter does not exist with column name : " + p_columnName + " Or given input is null.");
			return;
		}
		for (int i = 0; i < m_rowName.Count; i++)
		{
			p_columns.Add(m_parameters[i][num]);
		}
	}

	public string RowName(int p_rowIndex)
	{
		if (p_rowIndex > m_rowName.Count)
		{
			Dbg.LogError("Wrong Row Index " + p_rowIndex);
			return string.Empty;
		}
		return m_rowName[p_rowIndex];
	}

	public string ColumnName(int p_columnIndex)
	{
		if (p_columnIndex > m_columnNameMap.Count || !m_columnNameMap.ContainsValue(p_columnIndex))
		{
			Dbg.LogError("Wrong Row Index " + p_columnIndex);
			return string.Empty;
		}
		foreach (KeyValuePair<string, int> item in m_columnNameMap)
		{
			if (item.Value == p_columnIndex)
			{
				return item.Key;
			}
		}
		return string.Empty;
	}

	public int GetColumnIndex(string p_columnName)
	{
		if (m_columnNameMap.ContainsKey(p_columnName))
		{
			return m_columnNameMap[p_columnName];
		}
		return -1;
	}

	public string GetData(int p_rowIndex, int p_columnIndex)
	{
		if (p_rowIndex < 0 || p_columnIndex < 0 || p_rowIndex >= m_rowName.Count || p_columnIndex >= m_columnNameMap.Count)
		{
			Dbg.LogError("Wrong indexes to acces this data.(" + p_rowIndex + "," + p_columnIndex + ")");
			return "-1";
		}
		return m_parameters[p_rowIndex][p_columnIndex];
	}

	public string GetData(string p_rowName, string p_columnName)
	{
		int num = -1;
		int num2 = -1;
		if (m_columnNameMap.ContainsKey(p_columnName))
		{
			num2 = m_columnNameMap[p_columnName];
		}
		if (m_rowNameMap.ContainsKey(p_rowName))
		{
			num = m_rowNameMap[p_rowName];
		}
		if (num == -1 || num2 == -1)
		{
			Dbg.LogWarn("This parameter does not exist with row : " + p_rowName + " (" + num + ")  and column : (" + num2 + ") " + p_columnName);
			return "-1";
		}
		return m_parameters[num][num2];
	}

	public string GetData(int p_rowIndex, string p_columnName)
	{
		int columnIndex = GetColumnIndex(p_columnName);
		return GetData(p_rowIndex, columnIndex);
	}

	public int GetDataNumber(string p_rowName, string p_columnName)
	{
		return GetDataAsStringArray(p_rowName, p_columnName).Length;
	}

	public T GetDataGeneric<T>(int p_rowIndex, int p_columnIndex)
	{
		if (p_rowIndex < 0 || p_columnIndex < 0 || p_rowIndex >= m_rowName.Count || p_columnIndex >= m_columnNameMap.Count)
		{
			Dbg.LogError("Wrong indexes to acces this data.(" + p_rowIndex + "," + p_columnIndex + ")");
			return default(T);
		}
		string p_value = m_parameters[p_rowIndex][p_columnIndex];
		T p_outResult = default(T);
		NBGeneric.TryParse(p_value, out p_outResult);
		return p_outResult;
	}

	public T GetDataGeneric<T>(string p_rowName, string p_columnName)
	{
		string data = GetData(p_rowName, p_columnName);
		T p_outResult = default(T);
		NBGeneric.TryParse(data, out p_outResult);
		return p_outResult;
	}

	public T GetDataGeneric<T>(int p_rowIndex, string p_columnName)
	{
		int columnIndex = GetColumnIndex(p_columnName);
		return GetDataGeneric<T>(p_rowIndex, columnIndex);
	}

	public string[] GetDataAsStringArray(int p_row, int p_column)
	{
		if (p_row < 0 || p_column < 0 || p_row >= m_rowName.Count || p_column >= m_columnNameMap.Count)
		{
			Dbg.LogError("Wrong indexes to acces this data.(" + p_row + "," + p_column + ")");
			return null;
		}
		string data = GetData(p_row, p_column);
		return data.Split(m_inCellDelim.ToCharArray());
	}

	public string[] GetDataAsStringArray(string p_rowName, string p_columnName)
	{
		string data = GetData(p_rowName, p_columnName);
		return data.Split(m_inCellDelim.ToCharArray());
	}

	public string[] GetDataAsStringArray(int p_rowIndex, string p_columnName)
	{
		int columnIndex = GetColumnIndex(p_columnName);
		return GetDataAsStringArray(p_rowIndex, columnIndex);
	}

	public Vector3 GetDataAsVector3(int p_rowIndex, int p_columnIndex)
	{
		if (p_rowIndex > m_rowName.Count || p_columnIndex > m_columnNameMap.Count)
		{
			Dbg.LogError("Wrong indexes to acces this data.(" + p_rowIndex + "," + p_columnIndex + ")");
			return Vector3.zero;
		}
		string text = m_parameters[p_rowIndex][p_columnIndex];
		string[] array = text.Split(m_inCellDelim.ToCharArray());
		if (array.Length != 3)
		{
			Dbg.LogError("Error when parsing Vector3 DataType.");
			return Vector3.zero;
		}
		try
		{
			return new Vector3(Convert.ToSingle(array[0]), Convert.ToSingle(array[1]), Convert.ToSingle(array[2]));
		}
		catch (Exception ex)
		{
			Dbg.LogWarn("Unable to parse parameter value at row: " + p_rowIndex + ", column: " + p_columnIndex + ", value: " + text);
			Dbg.LogWarn(ex.ToString());
			return Vector3.zero;
		}
	}

	public Vector4 GetDataAsVector4(int p_rowIndex, int p_columnIndex)
	{
		if (p_rowIndex > m_rowName.Count || p_columnIndex > m_columnNameMap.Count)
		{
			Dbg.LogError("Wrong indexes to acces this data.(" + p_rowIndex + "," + p_columnIndex + ")");
			return Vector4.zero;
		}
		string text = m_parameters[p_rowIndex][p_columnIndex];
		string[] array = text.Split(m_inCellDelim.ToCharArray());
		if (array.Length != 4)
		{
			Dbg.LogError("Error when parsing Vector4 DataType.");
			return Vector4.zero;
		}
		try
		{
			return new Vector4(Convert.ToSingle(array[0]), Convert.ToSingle(array[1]), Convert.ToSingle(array[2]), Convert.ToSingle(array[3]));
		}
		catch (Exception ex)
		{
			Dbg.LogWarn("Unable to parse parameter value at row: " + p_rowIndex + ", column: " + p_columnIndex + ", value: " + text);
			Dbg.LogWarn(ex.ToString());
			return Vector4.zero;
		}
	}

	public Quaternion GetDataAsQuaternion(int p_rowIndex, int p_columnIndex)
	{
		if (p_rowIndex > m_rowName.Count || p_columnIndex > m_columnNameMap.Count)
		{
			Dbg.LogError("Wrong indexes to acces this data.(" + p_rowIndex + "," + p_columnIndex + ")");
			return Quaternion.identity;
		}
		string text = m_parameters[p_rowIndex][p_columnIndex];
		string[] array = text.Split(m_inCellDelim.ToCharArray());
		if (array.Length != 4)
		{
			Dbg.LogError("Error when parsing Vector4 DataType.");
			return Quaternion.identity;
		}
		try
		{
			return new Quaternion(Convert.ToSingle(array[0]), Convert.ToSingle(array[1]), Convert.ToSingle(array[2]), Convert.ToSingle(array[3]));
		}
		catch (Exception ex)
		{
			Dbg.LogWarn("Unable to parse parameter value at row: " + p_rowIndex + ", column: " + p_columnIndex + ", value: " + text);
			Dbg.LogWarn(ex.ToString());
			return Quaternion.identity;
		}
	}

	public void Unload()
	{
		m_parameters.Clear();
		m_parameters = null;
		m_columnNameMap.Clear();
		m_columnNameMap = null;
		m_rowName.Clear();
		m_rowName = null;
		m_rowNameMap.Clear();
		m_rowNameMap = null;
	}

	public bool SaveFile(string p_fileName)
	{
		Dbg.LogError("We are not in Editor mode, we cannot save Parameters for now.");
		return false;
	}

	public bool ReadFile(string p_fileName, GameObject p_requestor = null)
	{
		string empty = string.Empty;
		empty = SingletonComponent<DLC>.Instance.GetFile(p_requestor, p_fileName);
		if (empty == null)
		{
			Dbg.LogWarn("There are no contents in last read file.");
			return false;
		}
		m_rowName = CSVParser.Parse(empty, ref m_parameters, ref m_rowNameMap, ref m_columnNameMap);
		return m_rowName.Count > 0;
	}

	public void PrintData()
	{
		string text = "\t";
		try
		{
			foreach (string key in m_columnNameMap.Keys)
			{
				text = text + key + " ";
			}
			text += "\n";
			for (int i = 0; i < m_rowName.Count; i++)
			{
				text = text + m_rowName[i] + " ";
				for (int j = 0; j < m_columnNameMap.Count; j++)
				{
					text = text + m_parameters[i][j] + " ";
				}
				text += "\n";
			}
		}
		catch (Exception ex)
		{
			Dbg.LogError("Could not print data : " + ex.Message);
		}
		Dbg.Log(text);
	}
}
