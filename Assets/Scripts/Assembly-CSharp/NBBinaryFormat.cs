using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class NBBinaryFormat
{
	public enum DataType
	{
		INT = 0,
		UINT = 1,
		SHORT = 2,
		USHORT = 3,
		CHAR = 4,
		BYTE = 5,
		STRING = 6,
		FLOAT = 7
	}

	public class NBBinColumnInfo
	{
		private string m_szName;

		private DataType m_eType;

		public string szName
		{
			get
			{
				return m_szName;
			}
			set
			{
				m_szName = value;
			}
		}

		public DataType eDataType
		{
			get
			{
				return m_eType;
			}
			set
			{
				m_eType = value;
			}
		}
	}

	private string[] DataTypeString = new string[8] { "INT", "UINT", "SHORT", "USHORT", "CHAR", "BYTE", "STRING", "FLOAT" };

	private Dictionary<string, int> m_dictColumnNameMap;

	private List<string> m_listRowName;

	private Dictionary<string, int> m_dictRowNameMap;

	private List<List<ArrayList>> m_listTableData;

	private List<NBBinColumnInfo> m_listColumnInfo;

	public int RowsCount
	{
		get
		{
			return m_listRowName.Count;
		}
	}

	public List<string> RowsNames
	{
		get
		{
			return m_listRowName;
		}
	}

	public int ColumnsCount
	{
		get
		{
			return m_listColumnInfo.Count;
		}
	}

	public Dictionary<string, int> ColumnsNames
	{
		get
		{
			return m_dictColumnNameMap;
		}
	}

	public NBBinaryFormat()
	{
		m_listRowName = new List<string>();
		m_dictRowNameMap = new Dictionary<string, int>();
		m_dictColumnNameMap = new Dictionary<string, int>();
		m_listTableData = new List<List<ArrayList>>();
		m_listColumnInfo = new List<NBBinColumnInfo>();
	}

	public List<string> RowNameWithPattern(string _pattern)
	{
		string value = _pattern.ToLower();
		List<string> list = new List<string>();
		if (m_listRowName == null)
		{
			return list;
		}
		foreach (string item in m_listRowName)
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
		return m_listRowName.Contains(p_rowName);
	}

	public bool ContainsColumn(string p_columnName)
	{
		return m_dictColumnNameMap.ContainsKey(p_columnName);
	}

	public void GetColumn(string p_columnName, ref List<ArrayList> p_columns)
	{
		if (m_dictColumnNameMap == null || m_listTableData == null)
		{
			Dbg.LogError("[NBBinaryFormat::GetColumn] data is not initialised or not initialise properly");
			return;
		}
		int num = -1;
		if (m_dictColumnNameMap.ContainsKey(p_columnName))
		{
			num = m_dictColumnNameMap[p_columnName];
		}
		if (num == -1 || p_columns == null)
		{
			Dbg.LogError("[NBBinaryFormat::GetColumn] This parameter does not exist with column name : " + p_columnName + " Or given input is null.");
			return;
		}
		for (int i = 0; m_listTableData.Count > i; i++)
		{
			if (m_listTableData[i].Count <= num)
			{
				Dbg.LogError("[NBBinaryFormat::GetColumn] row " + i + " have less columns than specified:" + num);
				p_columns = new List<ArrayList>();
				break;
			}
			p_columns.Add(m_listTableData[i][num]);
		}
	}

	public string RowName(int p_rowIndex)
	{
		if (m_listRowName == null)
		{
			Dbg.LogError("[NBBinaryFormat::RowName] data is not initialised or not initialise properly");
			return string.Empty;
		}
		if (p_rowIndex > m_listRowName.Count)
		{
			Dbg.LogError("[NBBinaryFormat::RowNum] Wrong Row Index " + p_rowIndex);
			return string.Empty;
		}
		return m_listRowName[p_rowIndex];
	}

	public string ColumnName(int p_columnIndex)
	{
		if (m_listColumnInfo == null)
		{
			Dbg.LogError("[NBBinaryFormat::ColumnName] data is not initialised or not initialise properly");
			return string.Empty;
		}
		if (0 > p_columnIndex || m_listColumnInfo.Count <= p_columnIndex)
		{
			Dbg.LogError("[NBBinaryFormat::ColumnName] column index is invalid. requested " + p_columnIndex + " out of [0 - " + m_listColumnInfo.Count + "]");
			return string.Empty;
		}
		return m_listColumnInfo[p_columnIndex].szName;
	}

	public int GetColumnIndex(string p_columnName)
	{
		if (m_dictColumnNameMap.ContainsKey(p_columnName))
		{
			return m_dictColumnNameMap[p_columnName];
		}
		return -1;
	}

	public string GetData(int p_rowIndex, int p_columnIndex)
	{
		if (m_listColumnInfo == null || m_listTableData == null)
		{
			Dbg.LogError("[NBBinaryFormat::GetData] data is not initialised or not initialise properly");
			return null;
		}
		if (0 > p_columnIndex || p_columnIndex >= m_listColumnInfo.Count || 0 > p_rowIndex || p_rowIndex > m_listTableData.Count)
		{
			Dbg.LogError("[NBBinaryFormat::GetData] invalid row/col index: [" + p_rowIndex + "," + p_columnIndex + "]");
			return string.Empty;
		}
		ArrayList arrayList = m_listTableData[p_rowIndex][p_columnIndex];
		string text = arrayList[0].ToString();
		for (int i = 1; arrayList.Count > i; i++)
		{
			text = text + "," + arrayList[i].ToString();
		}
		return text;
	}

	public ArrayList GetDataAsArrayList(int nRow, int nCol)
	{
		if (m_listColumnInfo == null || m_listTableData == null)
		{
			Dbg.LogError("[NBBinaryFormat::GetDataAsArrayList] data is not initialised or not initialise properly");
			return null;
		}
		if (0 > nCol || nCol >= m_listColumnInfo.Count || 0 > nRow || nRow > m_listTableData.Count)
		{
			Dbg.LogError("[NBBinaryFormat::GetDataAsArrayList] invalid row/col index: [" + nRow + "," + nCol + "]");
			return null;
		}
		return m_listTableData[nRow][nCol];
	}

	public string GetData(string p_rowName, string p_columnName)
	{
		if (m_dictRowNameMap == null || m_dictColumnNameMap == null)
		{
			Dbg.LogError("[NBBinaryFormat::GetData] data is not initialised or not initialise properly");
			return null;
		}
		int num = -1;
		int num2 = -1;
		if (m_dictColumnNameMap.ContainsKey(p_columnName))
		{
			num2 = m_dictColumnNameMap[p_columnName];
		}
		if (m_dictRowNameMap.ContainsKey(p_rowName))
		{
			num = m_dictRowNameMap[p_rowName];
		}
		if (num == -1 || num2 == -1)
		{
			Dbg.LogError("[NBBinaryFormat::GetData] This parameter does not exist with row : " + p_rowName + " (" + num + ")  and column : (" + p_columnName + ") " + num2);
			return "-1";
		}
		return GetData(num, num2);
	}

	public string GetData(int p_rowIndex, string p_columnName)
	{
		int columnIndex = GetColumnIndex(p_columnName);
		return GetData(p_rowIndex, columnIndex);
	}

	public int GetDataNumber(string p_rowName, string p_columnName)
	{
		if (m_dictRowNameMap == null || m_dictColumnNameMap == null)
		{
			Dbg.LogError("[NBBinaryFormat::GetDataNumber] data is not initialised or not initialise properly");
			return -1;
		}
		int num = -1;
		int num2 = -1;
		if (m_dictColumnNameMap.ContainsKey(p_columnName))
		{
			num2 = m_dictColumnNameMap[p_columnName];
		}
		if (m_dictRowNameMap.ContainsKey(p_rowName))
		{
			num = m_dictRowNameMap[p_rowName];
		}
		if (num == -1 || num2 == -1)
		{
			Dbg.LogWarn("[NBBinaryFormat::GetDataNumber] This parameter does not exist with row : " + p_rowName + " (" + num + ")  and column : (" + num2 + ") " + p_columnName);
			return -1;
		}
		return m_listTableData[num][num2].Count;
	}

	public T GetDataGeneric<T>(int p_rowIndex, int p_columnIndex)
	{
		if (p_rowIndex < 0 || p_columnIndex < 0 || m_listTableData == null || m_listTableData[p_rowIndex] == null || m_listTableData[p_rowIndex][p_columnIndex] == null)
		{
			Dbg.LogError("[NBBinaryFormat::GetDataGeneric<T>] data is not initialised or not initialise properly Or wrong indexes row: " + p_rowIndex + " column: " + p_columnIndex);
			return default(T);
		}
		if (p_rowIndex < 0 || p_columnIndex < 0 || p_rowIndex >= m_listTableData.Count || p_columnIndex >= m_listColumnInfo.Count)
		{
			Dbg.LogError("[NBBinaryFormat::GetDataGeneric<T>] Wrong indices to acces this data.(" + p_rowIndex + "," + p_columnIndex + ")");
			return default(T);
		}
		ArrayList arrayList = m_listTableData[p_rowIndex][p_columnIndex];
		if (arrayList[0].GetType() != default(T).GetType())
		{
			Dbg.LogError("[NBBinaryFormat::GetGeneric<T>] Wrong type requested. Column of type " + DataTypeString[(int)m_listColumnInfo[p_columnIndex].eDataType] + " requested as " + default(T).GetType());
			return default(T);
		}
		return (T)m_listTableData[p_rowIndex][p_columnIndex][0];
	}

	public T GetDataGeneric<T>(string p_rowName, string p_columnName)
	{
		if (m_dictRowNameMap == null || m_dictColumnNameMap == null)
		{
			Dbg.LogError("[NBBinaryFormat::GetDataGeneric<T>] data is not initialised or not initialise properly");
			return default(T);
		}
		int num = -1;
		int num2 = -1;
		if (m_dictColumnNameMap.ContainsKey(p_columnName))
		{
			num2 = m_dictColumnNameMap[p_columnName];
		}
		if (m_dictRowNameMap.ContainsKey(p_rowName))
		{
			num = m_dictRowNameMap[p_rowName];
		}
		if (num == -1 || num2 == -1)
		{
			Dbg.LogError("[NBBinaryFormat::GetData] This parameter does not exist with row : " + p_rowName + " (" + num + ")  and column : (" + p_columnName + ") " + num2);
			return default(T);
		}
		return GetDataGeneric<T>(num, num2);
	}

	public T GetDataGeneric<T>(int p_rowIndex, string p_columnName)
	{
		int columnIndex = GetColumnIndex(p_columnName);
		return GetDataGeneric<T>(p_rowIndex, columnIndex);
	}

	public string[] GetDataAsStringArray(int p_row, int p_column)
	{
		ArrayList dataAsArrayList = GetDataAsArrayList(p_row, p_column);
		if (dataAsArrayList == null)
		{
			return null;
		}
		string[] array = new string[dataAsArrayList.Count];
		for (int i = 0; dataAsArrayList.Count > i; i++)
		{
			array[i] = dataAsArrayList[i].ToString();
		}
		return array;
	}

	public string[] GetDataAsStringArray(string p_rowName, string p_columnName)
	{
		int num = -1;
		int num2 = -1;
		if (m_dictColumnNameMap.ContainsKey(p_columnName))
		{
			num2 = m_dictColumnNameMap[p_columnName];
		}
		if (m_dictRowNameMap.ContainsKey(p_rowName))
		{
			num = m_dictRowNameMap[p_rowName];
		}
		if (num == -1 || num2 == -1)
		{
			Dbg.LogWarn("[NBBinaryFormat::GetDataNumber] This parameter does not exist with row : " + p_rowName + " (" + num + ")  and column : (" + num2 + ") " + p_columnName);
			return null;
		}
		return GetDataAsStringArray(num, num2);
	}

	public string[] GetDataAsStringArray(int p_rowIndex, string p_columnName)
	{
		int columnIndex = GetColumnIndex(p_columnName);
		return GetDataAsStringArray(p_rowIndex, columnIndex);
	}

	public Vector3 GetDataAsVector3(int p_rowIndex, int p_columnIndex)
	{
		ArrayList dataAsArrayList = GetDataAsArrayList(p_rowIndex, p_columnIndex);
		if (dataAsArrayList == null)
		{
			Dbg.LogError("[NBBinaryFormat::GetDataAsVector3] Wrong indexes to acces this data.(" + p_rowIndex + "," + p_columnIndex + ")");
			return Vector3.zero;
		}
		if (dataAsArrayList.Count != 3)
		{
			Dbg.LogError("[NBBinaryFormat::GetDataAsVector3] Error when parsing Vector3 DataType.");
			return Vector3.zero;
		}
		if (m_listColumnInfo[p_columnIndex].eDataType != DataType.FLOAT)
		{
			Dbg.LogError("[NBBinaryFormat::GetDataAsVector3] Column data type is not float.");
			return Vector3.zero;
		}
		return new Vector3((float)dataAsArrayList[0], (float)dataAsArrayList[1], (float)dataAsArrayList[2]);
	}

	public Vector4 GetDataAsVector4(int p_rowIndex, int p_columnIndex)
	{
		ArrayList dataAsArrayList = GetDataAsArrayList(p_rowIndex, p_columnIndex);
		if (dataAsArrayList == null)
		{
			Dbg.LogError("[NBBinaryFormat::GetDataAsVector4] Wrong indexes to acces this data.(" + p_rowIndex + "," + p_columnIndex + ")");
			return Vector4.zero;
		}
		if (dataAsArrayList.Count != 4)
		{
			Dbg.LogError("[NBBinaryFormat::GetDataAsVector4] Error when parsing Vector4 DataType.");
			return Vector4.zero;
		}
		if (m_listColumnInfo[p_columnIndex].eDataType != DataType.FLOAT)
		{
			Dbg.LogError("[NBBinaryFormat::GetDataAsVector4] Column data type is not float.");
			return Vector4.zero;
		}
		return new Vector4((float)dataAsArrayList[0], (float)dataAsArrayList[1], (float)dataAsArrayList[2], (float)dataAsArrayList[3]);
	}

	public Quaternion GetDataAsQuaternion(int p_rowIndex, int p_columnIndex)
	{
		ArrayList dataAsArrayList = GetDataAsArrayList(p_rowIndex, p_columnIndex);
		if (dataAsArrayList == null)
		{
			Dbg.LogError("[NBBinaryFormat::GetDataAsQuaternion] Wrong indexes to acces this data.(" + p_rowIndex + "," + p_columnIndex + ")");
			return Quaternion.identity;
		}
		if (dataAsArrayList.Count != 4)
		{
			Dbg.LogError("[NBBinaryFormat::GetDataAsQuaternion] Error when parsing Vector4 DataType.");
			return Quaternion.identity;
		}
		if (m_listColumnInfo[p_columnIndex].eDataType != DataType.FLOAT)
		{
			Dbg.LogError("[NBBinaryFormat::GetDataAsQuaternion] Column data type is not float.");
			return Quaternion.identity;
		}
		return new Quaternion((float)dataAsArrayList[0], (float)dataAsArrayList[1], (float)dataAsArrayList[2], (float)dataAsArrayList[3]);
	}

	public void Unload()
	{
		m_dictColumnNameMap.Clear();
		m_dictColumnNameMap = null;
		m_listRowName.Clear();
		m_listRowName = null;
		m_dictRowNameMap.Clear();
		m_dictRowNameMap = null;
		if (m_listColumnInfo != null)
		{
			m_listColumnInfo.Clear();
			m_listColumnInfo = null;
		}
		if (m_listTableData == null)
		{
			return;
		}
		for (int i = 0; m_listTableData.Count > i; i++)
		{
			List<ArrayList> list = m_listTableData[i];
			if (list == null)
			{
				continue;
			}
			for (int j = 0; list.Count > j; j++)
			{
				if (list[j] != null)
				{
					list[j].Clear();
					list[j] = null;
				}
			}
			list.Clear();
			m_listTableData[i] = null;
		}
		m_listTableData.Clear();
		m_listTableData = null;
	}

	public bool SaveFile(string p_fileName)
	{
		return false;
	}

	public bool ReadFile(string p_fileName, GameObject p_requestor = null)
	{
		Unload();
		m_listRowName = new List<string>();
		m_dictRowNameMap = new Dictionary<string, int>();
		m_dictColumnNameMap = new Dictionary<string, int>();
		m_listTableData = new List<List<ArrayList>>();
		m_listColumnInfo = new List<NBBinColumnInfo>();
		byte[] array = null;
		array = SingletonComponent<DLC>.Instance.GetFileData(p_requestor, p_fileName);
		if (array == null)
		{
			Dbg.LogError("[NBBinaryFormat::ReadFile] Unable to read " + p_fileName);
			return false;
		}
		if (array.Length <= 32)
		{
			Dbg.LogError("[NBBinaryFormat::ReadFile] Invalid file length for " + p_fileName);
			return false;
		}
		HMACSHA256 hMACSHA = new HMACSHA256(new byte[32]
		{
			54, 100, 43, 43, 105, 110, 107, 105, 109, 110,
			105, 47, 105, 107, 25, 107, 111, 49, 106, 8,
			100, 106, 106, 20, 100, 62, 104, 104, 48, 17,
			41, 29
		});
		byte[] array2 = hMACSHA.ComputeHash(array, 0, array.Length - 32);
		if (array2.Length != 32)
		{
			Dbg.LogError("[NBBinaryFormat::ReadFile] computedHash array's length is not 32");
			return false;
		}
		int num = array.Length - 32;
		for (int i = 0; 32 > i; i++)
		{
			if (array2[i] != array[num + i])
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine("[NBBinaryFormat::ReadFile] incorrect checksum:");
				stringBuilder.Append("    Computed: 0x");
				for (int j = 0; j < array2.Length; j++)
				{
					stringBuilder.Append(array2[j].ToString("x2"));
				}
				stringBuilder.AppendLine();
				stringBuilder.Append("    File: 0x");
				for (int k = 0; k < 32; k++)
				{
					stringBuilder.Append(array[num + k].ToString("x2"));
				}
				Dbg.LogError(stringBuilder.ToString());
				Dbg.LogError("[NBBinaryFormat::ReadFile] hash mis-matched");
				return false;
			}
		}
		int num2 = 0;
		int num3 = BitConverter.ToInt32(array, num2);
		num2 += 4;
		int num4 = array[num2++];
		for (int l = 0; num4 > l; l++)
		{
			NBBinColumnInfo nBBinColumnInfo = new NBBinColumnInfo();
			nBBinColumnInfo.eDataType = (DataType)array[num2++];
			int num5 = array[num2++];
			for (int m = 0; num5 > m; m++)
			{
				array[num2 + m] = (byte)(array[num2 + m] & -129);
			}
			nBBinColumnInfo.szName = Encoding.UTF8.GetString(array, num2, num5);
			num2 += num5;
			m_listColumnInfo.Add(nBBinColumnInfo);
			if (l > 0)
			{
				m_dictColumnNameMap.Add(nBBinColumnInfo.szName, l - 1);
			}
		}
		if (m_listColumnInfo.Count != num4)
		{
			Unload();
			Dbg.LogError("[NBBinaryFormat::ReadFile] Discrepancy encountered when reading " + p_fileName + ". Aborting read.");
			return false;
		}
		m_listTableData = new List<List<ArrayList>>();
		for (int n = 0; num3 > n; n++)
		{
			List<ArrayList> list = new List<ArrayList>();
			m_listTableData.Add(list);
			for (int num6 = 0; num4 > num6; num6++)
			{
				int num7 = array[num2++];
				ArrayList arrayList = new ArrayList();
				for (int num8 = 0; num7 > num8; num8++)
				{
					switch (m_listColumnInfo[num6].eDataType)
					{
					case DataType.BYTE:
					{
						byte b3 = array[num2++];
						arrayList.Add(b3);
						break;
					}
					case DataType.CHAR:
					{
						sbyte b2 = (sbyte)array[num2++];
						arrayList.Add(b2);
						break;
					}
					case DataType.FLOAT:
						arrayList.Add(BitConverter.ToSingle(array, num2));
						num2 += 4;
						break;
					case DataType.INT:
						arrayList.Add(BitConverter.ToInt32(array, num2));
						num2 += 4;
						break;
					case DataType.UINT:
						arrayList.Add(BitConverter.ToUInt32(array, num2));
						num2 += 4;
						break;
					case DataType.SHORT:
						arrayList.Add(BitConverter.ToInt16(array, num2));
						num2 += 2;
						break;
					case DataType.USHORT:
						arrayList.Add(BitConverter.ToUInt16(array, num2));
						num2 += 2;
						break;
					case DataType.STRING:
					{
						byte b = array[num2++];
						for (int num9 = 0; b > num9; num9++)
						{
							array[num2 + num9] = (byte)(array[num2 + num9] & -129);
						}
						arrayList.Add(Encoding.UTF8.GetString(array, num2, b));
						num2 += b;
						break;
					}
					default:
						Unload();
						Dbg.LogError("[NBBinaryFormat::ReadFile] unknow and invalid type:" + m_listColumnInfo[num6].eDataType);
						return false;
					}
				}
				if (num6 == 0)
				{
					string text = arrayList[0].ToString();
					m_listRowName.Add(text);
					if (!m_dictRowNameMap.ContainsKey(text))
					{
						m_dictRowNameMap.Add(text, n);
					}
				}
				else
				{
					list.Add(arrayList);
				}
			}
		}
		m_listColumnInfo.RemoveAt(0);
		return true;
	}

	public void PrintData()
	{
	}
}
