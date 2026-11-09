using System;
using System.Collections.Generic;

public class CSVParser
{
	public static readonly string s_commentChar = "#";

	public static readonly char s_fieldDelim = '^';

	public static readonly char s_inCellDelim = ',';

	public static readonly string s_stringDelim = "\"";

	public static readonly char[] s_lineDelimiters = new char[2] { '\r', '\n' };

	public static List<string> Parse(string p_contents, ref List<List<string>> p_tableContents, ref Dictionary<string, int> p_rowMap, ref Dictionary<string, int> p_colMap)
	{
		p_tableContents.Clear();
		p_rowMap.Clear();
		p_colMap.Clear();
		List<string> list = new List<string>();
		List<string> list2 = new List<string>(p_contents.Split(s_lineDelimiters, StringSplitOptions.RemoveEmptyEntries));
		if (list2 == null)
		{
			Dbg.LogError("Could not get the file lines.");
			return list;
		}
		List<string> list3 = null;
		string text = list2[0];
		string text2 = text.Replace("\"", string.Empty);
		char[] separator = new char[1] { s_fieldDelim };
		string[] array = text2.Split(separator);
		for (int i = 1; i < array.Length; i++)
		{
			p_colMap.Add(array[i], i - 1);
		}
		try
		{
			for (int j = 1; j < list2.Count; j++)
			{
				text = list2[j];
				text2 = text.Replace(s_stringDelim, string.Empty);
				string[] array2 = text2.Split(separator);
				if (array2.Length >= 1 && !text.Contains(s_commentChar) && !(array2[0] == string.Empty))
				{
					string item = array2[0];
					list.Add(item);
					list3 = new List<string>(array2);
					list3.RemoveAt(0);
					p_tableContents.Add(list3);
				}
			}
			int num = 0;
			foreach (string item2 in list)
			{
				if (!p_rowMap.ContainsKey(item2))
				{
					p_rowMap.Add(item2, num);
				}
				num++;
			}
			return list;
		}
		catch (Exception ex)
		{
			Dbg.LogError("Error parsing CSV file: " + ex.Message);
			list.Clear();
			return list;
		}
	}
}
