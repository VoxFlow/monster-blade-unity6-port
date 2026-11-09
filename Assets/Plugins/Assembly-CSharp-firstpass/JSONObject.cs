using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class JSONObject : Nullable
{
	public enum Type
	{
		NULL = 0,
		STRING = 1,
		NUMBER = 2,
		OBJECT = 3,
		ARRAY = 4,
		BOOL = 5
	}

	private const int MAX_DEPTH = 1000;

	private const string INFINITY = "\"INFINITY\"";

	private const string NEGINFINITY = "\"NEGINFINITY\"";

	public JSONObject parent;

	public Type type;

	public ArrayList list = new ArrayList();

	public ArrayList keys = new ArrayList();

	public string str;

	public double n;

	public bool b;

	public bool isContainer
	{
		get
		{
			return type == Type.ARRAY || type == Type.OBJECT;
		}
	}

	public int Count
	{
		get
		{
			return list.Count;
		}
	}

	public static JSONObject nullJO
	{
		get
		{
			return new JSONObject(Type.NULL);
		}
	}

	public static JSONObject obj
	{
		get
		{
			return new JSONObject(Type.OBJECT);
		}
	}

	public static JSONObject arr
	{
		get
		{
			return new JSONObject(Type.ARRAY);
		}
	}

	public JSONObject this[int index]
	{
		get
		{
			if (list.Count > index)
			{
				return (JSONObject)list[index];
			}
			return null;
		}
		set
		{
			if (list.Count > index)
			{
				list[index] = value;
			}
		}
	}

	public JSONObject this[string index]
	{
		get
		{
			return GetField(index);
		}
		set
		{
			SetField(index, value);
		}
	}

	public JSONObject(Type t)
	{
		type = t;
		switch (t)
		{
		case Type.ARRAY:
			list = new ArrayList();
			break;
		case Type.OBJECT:
			list = new ArrayList();
			keys = new ArrayList();
			break;
		}
	}

	public JSONObject(bool b)
	{
		type = Type.BOOL;
		this.b = b;
	}

	public JSONObject(float f)
	{
		type = Type.NUMBER;
		n = f;
	}

	public JSONObject(Dictionary<string, string> dic)
	{
		type = Type.OBJECT;
		foreach (KeyValuePair<string, string> item in dic)
		{
			keys.Add(item.Key);
			list.Add(new JSONObject
			{
				type = Type.STRING,
				str = item.Value
			});
		}
	}

	public JSONObject()
	{
	}

	public JSONObject(string str)
	{
		ParseUnicode(ref str);
		if (str != null)
		{
			str = str.Replace("\\n", string.Empty);
			str = str.Replace("\\t", string.Empty);
			str = str.Replace("\\r", string.Empty);
			str = str.Replace("\t", string.Empty);
			str = str.Replace("\n", string.Empty);
			str = str.Replace("\\", string.Empty);
			if (str.Length <= 0)
			{
				return;
			}
			if (string.Compare(str, "true", true) == 0)
			{
				type = Type.BOOL;
				b = true;
				return;
			}
			if (string.Compare(str, "false", true) == 0)
			{
				type = Type.BOOL;
				b = false;
				return;
			}
			switch (str)
			{
			case "null":
				type = Type.NULL;
				return;
			case "\"INFINITY\"":
				type = Type.NUMBER;
				n = double.PositiveInfinity;
				return;
			case "\"NEGINFINITY\"":
				type = Type.NUMBER;
				n = double.NegativeInfinity;
				return;
			}
			if (str[0] == '"')
			{
				type = Type.STRING;
				this.str = str.Substring(1, str.Length - 2);
				return;
			}
			try
			{
				n = Convert.ToDouble(str);
				type = Type.NUMBER;
				return;
			}
			catch (FormatException)
			{
				int num = 0;
				switch (str[0])
				{
				case '{':
					type = Type.OBJECT;
					keys = new ArrayList();
					list = new ArrayList();
					break;
				case '[':
					type = Type.ARRAY;
					list = new ArrayList();
					break;
				default:
					type = Type.NULL;
					Debug.LogWarning("improper JSON formatting:" + str);
					return;
				}
				int num2 = 0;
				bool flag = false;
				bool flag2 = false;
				for (int i = 1; i < str.Length; i++)
				{
					if (str[i] == '\\' || str[i] == '\t' || str[i] == '\n' || str[i] == '\r')
					{
						i++;
						continue;
					}
					if (str[i] == '"')
					{
						flag = !flag;
					}
					else if (str[i] == '[' || str[i] == '{')
					{
						num2++;
					}
					if (num2 == 0 && !flag)
					{
						if (str[i] == ':' && !flag2)
						{
							flag2 = true;
							try
							{
								keys.Add(str.Substring(num + 2, i - num - 3));
							}
							catch
							{
								Debug.Log(i + " - " + str.Length + " - " + str);
							}
							num = i;
						}
						if (str[i] == ',')
						{
							flag2 = false;
							list.Add(new JSONObject(str.Substring(num + 1, i - num - 1)));
							num = i;
						}
						if (str[i] == ']' || str[i] == '}')
						{
							list.Add(new JSONObject(str.Substring(num + 1, i - num - 1)));
						}
					}
					if (str[i] == ']' || str[i] == '}')
					{
						num2--;
					}
				}
				return;
			}
		}
		type = Type.NULL;
	}

	public void Absorb(JSONObject obj)
	{
		list.AddRange(obj.list);
		keys.AddRange(obj.keys);
		str = obj.str;
		n = obj.n;
		b = obj.b;
		type = obj.type;
	}

	public void Add(bool val)
	{
		Add(new JSONObject(val));
	}

	public void Add(float val)
	{
		Add(new JSONObject(val));
	}

	public void Add(int val)
	{
		Add(new JSONObject(val));
	}

	public void Add(JSONObject obj)
	{
		if ((bool)obj)
		{
			if (type != Type.ARRAY)
			{
				type = Type.ARRAY;
				Debug.LogWarning("tried to add an object to a non-array JSONObject.  We'll do it for you, but you might be doing something wrong.");
			}
			list.Add(obj);
		}
	}

	public void AddField(string name, bool val)
	{
		AddField(name, new JSONObject(val));
	}

	public void AddField(string name, float val)
	{
		AddField(name, new JSONObject(val));
	}

	public void AddField(string name, int val)
	{
		AddField(name, new JSONObject(val));
	}

	public void AddField(string name, string val)
	{
		AddField(name, new JSONObject
		{
			type = Type.STRING,
			str = val
		});
	}

	public void AddField(string name, JSONObject obj)
	{
		if ((bool)obj)
		{
			if (type != Type.OBJECT)
			{
				type = Type.OBJECT;
				Debug.LogWarning("tried to add a field to a non-object JSONObject.  We'll do it for you, but you might be doing something wrong.");
			}
			keys.Add(name);
			list.Add(obj);
		}
	}

	public void SetField(string name, JSONObject obj)
	{
		if (HasField(name))
		{
			list.Remove(this[name]);
			keys.Remove(name);
		}
		AddField(name, obj);
	}

	public JSONObject GetField(string name)
	{
		if (type == Type.OBJECT)
		{
			for (int i = 0; i < keys.Count && i < list.Count; i++)
			{
				if ((string)keys[i] == name)
				{
					return (JSONObject)list[i];
				}
			}
		}
		return null;
	}

	public bool HasField(string name)
	{
		if (type == Type.OBJECT)
		{
			for (int i = 0; i < keys.Count; i++)
			{
				if ((string)keys[i] == name)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void Clear()
	{
		type = Type.NULL;
		list.Clear();
		keys.Clear();
		str = string.Empty;
		n = 0.0;
		b = false;
	}

	public JSONObject Copy()
	{
		return new JSONObject(print());
	}

	public void Merge(JSONObject obj)
	{
		MergeRecur(this, obj);
	}

	private static void MergeRecur(JSONObject left, JSONObject right)
	{
		if (left.type == Type.NULL)
		{
			left.Absorb(right);
		}
		else if (left.type == Type.OBJECT && right.type == Type.OBJECT)
		{
			for (int i = 0; i < right.list.Count; i++)
			{
				string text = (string)right.keys[i];
				if (right[i].isContainer)
				{
					if (left.HasField(text))
					{
						MergeRecur(left[text], right[i]);
					}
					else
					{
						left.AddField(text, right[i]);
					}
				}
				else if (left.HasField(text))
				{
					left.SetField(text, right[i]);
				}
				else
				{
					left.AddField(text, right[i]);
				}
			}
		}
		else
		{
			if (left.type != Type.ARRAY || right.type != Type.ARRAY)
			{
				return;
			}
			if (right.Count > left.Count)
			{
				Debug.LogError("Cannot merge arrays when right object has more elements");
				return;
			}
			for (int j = 0; j < right.list.Count; j++)
			{
				if (left[j].type == right[j].type)
				{
					if (left[j].isContainer)
					{
						MergeRecur(left[j], right[j]);
					}
					else
					{
						left[j] = right[j];
					}
				}
			}
		}
	}

	public string print()
	{
		return print(0);
	}

	public string print(int depth)
	{
		if (depth++ > 1000)
		{
			Debug.Log("reached max depth!");
			return string.Empty;
		}
		string text = string.Empty;
		switch (type)
		{
		case Type.STRING:
			text = "\"" + str + "\"";
			break;
		case Type.NUMBER:
			text = ((n != double.PositiveInfinity) ? ((n != double.NegativeInfinity) ? (text + n) : "\"NEGINFINITY\"") : "\"INFINITY\"");
			break;
		case Type.OBJECT:
			if (list.Count > 0)
			{
				text = "{";
				text += "\n";
				depth++;
				for (int j = 0; j < list.Count; j++)
				{
					string text2 = (string)keys[j];
					JSONObject jSONObject2 = (JSONObject)list[j];
					if ((bool)jSONObject2)
					{
						for (int k = 0; k < depth; k++)
						{
							text += "\t";
						}
						text = text + "\"" + text2 + "\":";
						text = text + jSONObject2.print(depth) + ",";
						text += "\n";
					}
				}
				text = text.Substring(0, text.Length - 1);
				text = text.Substring(0, text.Length - 1);
				text += "}";
			}
			else
			{
				text = "null";
			}
			break;
		case Type.ARRAY:
			if (list.Count > 0)
			{
				text = "[";
				text += "\n";
				depth++;
				foreach (JSONObject item in list)
				{
					if ((bool)item)
					{
						for (int i = 0; i < depth; i++)
						{
							text += "\t";
						}
						text = text + item.print(depth) + ",";
						text += "\n";
					}
				}
				text = text.Substring(0, text.Length - 1);
				text = text.Substring(0, text.Length - 1);
				text += "]";
			}
			else
			{
				text = "null";
			}
			break;
		case Type.BOOL:
			text = ((!b) ? "false" : "true");
			break;
		case Type.NULL:
			text = "null";
			break;
		}
		return text;
	}

	public override string ToString()
	{
		return print();
	}

	public Dictionary<string, object> ToDictionary()
	{
		if (type == Type.OBJECT)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			for (int i = 0; i < list.Count; i++)
			{
				JSONObject jSONObject = (JSONObject)list[i];
				switch (jSONObject.type)
				{
				case Type.STRING:
					dictionary.Add((string)keys[i], jSONObject.str);
					break;
				case Type.NUMBER:
					dictionary.Add((string)keys[i], jSONObject.n + string.Empty);
					break;
				case Type.BOOL:
					dictionary.Add((string)keys[i], jSONObject.b + string.Empty);
					break;
				case Type.NULL:
					dictionary.Add((string)keys[i], null);
					break;
				default:
					Debug.LogWarning("Omitting object: " + (string)keys[i] + " in dictionary conversion");
					break;
				}
			}
			return dictionary;
		}
		if (type == Type.ARRAY)
		{
			Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
			for (int j = 0; j < list.Count; j++)
			{
				JSONObject jSONObject2 = (JSONObject)list[j];
				switch (jSONObject2.type)
				{
				case Type.OBJECT:
					dictionary2.Add(j.ToString(), jSONObject2.ToDictionary());
					break;
				case Type.ARRAY:
					dictionary2.Add(j.ToString(), jSONObject2.ToDictionary());
					break;
				default:
					Debug.LogWarning("Omitting object: " + j + " in dictionary conversion (Array case)");
					break;
				}
			}
			return dictionary2;
		}
		Debug.LogWarning("Tried to turn non-Object JSONObject into a dictionary");
		return null;
	}

	public static Dictionary<string, object> Deserialize(string json_string)
	{
		JSONObject jSONObject = new JSONObject(json_string);
		return jSONObject.ToDictionary();
	}

	public bool IsEmptyArray()
	{
		return type == Type.ARRAY && Count == 1 && ((JSONObject)list[0]).type == Type.NULL;
	}

	public void RemoveAt(int nIndex)
	{
		if (type != Type.ARRAY)
		{
			Debug.LogWarning("Trying to remove index while object is not array. This might not be correct but will be done anyway.");
		}
		if (list != null && list.Count > nIndex)
		{
			list.RemoveAt(nIndex);
		}
	}

	public void ParseUnicode(ref string p_text)
	{
		while (p_text.Contains("\\u"))
		{
			int num = p_text.IndexOf("\\u");
			int num2 = num + 6;
			if (num > -1 && num2 > num)
			{
				int num3 = num + 2;
				int length = num2 - num3;
				string text = p_text.Substring(num3, length);
				int num4 = int.Parse(text, NumberStyles.AllowHexSpecifier);
				if (num4 > 1114111 || (num4 >= 55296 && num4 <= 57343))
				{
					p_text = p_text.Replace("\\u" + text, "?");
				}
				else
				{
					p_text = p_text.Replace("\\u" + text, char.ConvertFromUtf32(num4));
				}
				continue;
			}
			break;
		}
	}
}
