// Read-only access to the original server tables for the offline server. Register CD-005.
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class OfflineTable
{
	public readonly string[] Columns;

	public readonly List<string[]> Rows = new List<string[]>();

	private readonly Dictionary<string, int> m_col = new Dictionary<string, int>();

	private readonly Dictionary<string, string[]> m_byId = new Dictionary<string, string[]>();

	public OfflineTable(string path)
	{
		string[] lines = File.ReadAllLines(path);
		Columns = lines[0].Split('\t');
		for (int i = 0; i < Columns.Length; i++)
		{
			m_col[Columns[i]] = i;
		}
		for (int i = 1; i < lines.Length; i++)
		{
			if (lines[i].Length == 0) continue;
			string[] r = lines[i].Split('\t');
			Rows.Add(r);
			if (!m_byId.ContainsKey(r[0])) m_byId[r[0]] = r;
		}
	}

	public string[] Row(string id)
	{
		string[] r;
		return m_byId.TryGetValue(id, out r) ? r : null;
	}

	public void Put(string[] r)
	{
		string[] old;
		if (m_byId.TryGetValue(r[0], out old)) Rows[Rows.IndexOf(old)] = r;
		else Rows.Add(r);
		m_byId[r[0]] = r;
	}

	public string[] NewRow()
	{
		string[] r = new string[Columns.Length];
		for (int i = 0; i < r.Length; i++) r[i] = string.Empty;
		return r;
	}

	public void Set(string[] row, string column, object value)
	{
		int i;
		if (m_col.TryGetValue(column, out i)) row[i] = System.Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture);
	}

	public string Get(string[] row, string column)
	{
		int i;
		return row != null && m_col.TryGetValue(column, out i) && i < row.Length ? row[i] : string.Empty;
	}

	public int GetInt(string[] row, string column)
	{
		int v;
		return int.TryParse(Get(row, column), out v) ? v : 0;
	}

	public float GetFloat(string[] row, string column)
	{
		float v;
		return float.TryParse(Get(row, column), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out v) ? v : 0f;
	}
}

public static class OfflineTables
{
	private static readonly Dictionary<string, OfflineTable> s_tables = new Dictionary<string, OfflineTable>();

	public static OfflineTable Get(string name)
	{
		OfflineTable t;
		if (!s_tables.TryGetValue(name, out t))
		{
			t = new OfflineTable(Path.Combine(Application.streamingAssetsPath, "OfflineServer/tables/" + name + ".tsv"));
			s_tables[name] = t;
		}
		return t;
	}
}
