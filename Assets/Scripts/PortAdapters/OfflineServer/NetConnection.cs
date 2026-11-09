// Stands in for the WWW of a service call, with the same members the client reads. Register CD-005.
using UnityEngine;

public class NetConnection
{
	private readonly WWW m_www;

	private readonly string m_text;

	private NetConnection(WWW www, string text)
	{
		m_www = www;
		m_text = text;
	}

	public static NetConnection Online(WWW www)
	{
		return new NetConnection(www, null);
	}

	public static NetConnection Offline(string text)
	{
		return new NetConnection(null, text);
	}

	public bool isDone
	{
		get { return m_www == null || m_www.isDone; }
	}

	public string error
	{
		get { return m_www != null ? m_www.error : null; }
	}

	public string text
	{
		get { return m_www != null ? m_www.text : m_text; }
	}
}
