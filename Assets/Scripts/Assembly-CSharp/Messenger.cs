using System;
using System.Collections.Generic;

public static class Messenger
{
	private static Dictionary<string, Delegate> eventTable = new Dictionary<string, Delegate>();

	public static void AddListener(string eventType, Callback handler)
	{
		lock (eventTable)
		{
			if (!eventTable.ContainsKey(eventType))
			{
				eventTable.Add(eventType, null);
			}
			eventTable[eventType] = (Callback)Delegate.Combine((Callback)eventTable[eventType], handler);
		}
	}

	public static void RemoveListener(string eventType, Callback handler)
	{
		lock (eventTable)
		{
			if (eventTable.ContainsKey(eventType))
			{
				eventTable[eventType] = (Callback)Delegate.Remove((Callback)eventTable[eventType], handler);
				if ((object)eventTable[eventType] == null)
				{
					eventTable.Remove(eventType);
				}
			}
		}
	}

	public static void Invoke(string eventType)
	{
		Delegate value;
		if (eventTable.TryGetValue(eventType, out value))
		{
			Callback callback = (Callback)value;
			if (callback != null)
			{
				callback();
			}
		}
	}
}
public static class Messenger<T>
{
	private static Dictionary<string, Delegate> eventTable = new Dictionary<string, Delegate>();

	public static Delegate GetListener(string eventType)
	{
		if (!eventTable.ContainsKey(eventType))
		{
			return null;
		}
		return eventTable[eventType];
	}

	public static void AddListener(string eventType, Callback<T> handler)
	{
		lock (eventTable)
		{
			if (!eventTable.ContainsKey(eventType))
			{
				eventTable.Add(eventType, null);
			}
			eventTable[eventType] = (Callback<T>)Delegate.Combine((Callback<T>)eventTable[eventType], handler);
		}
	}

	public static void RemoveListener(string eventType, Callback<T> handler)
	{
		lock (eventTable)
		{
			if (eventTable.ContainsKey(eventType))
			{
				eventTable[eventType] = (Callback<T>)Delegate.Remove((Callback<T>)eventTable[eventType], handler);
				if ((object)eventTable[eventType] == null)
				{
					eventTable.Remove(eventType);
				}
			}
		}
	}

	public static void Invoke(string eventType, T arg1)
	{
		Delegate value;
		if (eventTable.TryGetValue(eventType, out value))
		{
			Callback<T> callback = (Callback<T>)value;
			if (callback != null)
			{
				callback(arg1);
			}
		}
	}
}
public static class Messenger<T, U>
{
	private static Dictionary<string, Delegate> eventTable = new Dictionary<string, Delegate>();

	public static void AddListener(string eventType, Callback<T, U> handler)
	{
		lock (eventTable)
		{
			if (!eventTable.ContainsKey(eventType))
			{
				eventTable.Add(eventType, null);
			}
			eventTable[eventType] = (Callback<T, U>)Delegate.Combine((Callback<T, U>)eventTable[eventType], handler);
		}
	}

	public static void RemoveListener(string eventType, Callback<T, U> handler)
	{
		lock (eventTable)
		{
			if (eventTable.ContainsKey(eventType))
			{
				eventTable[eventType] = (Callback<T, U>)Delegate.Remove((Callback<T, U>)eventTable[eventType], handler);
				if ((object)eventTable[eventType] == null)
				{
					eventTable.Remove(eventType);
				}
			}
		}
	}

	public static void Invoke(string eventType, T arg1, U arg2)
	{
		Delegate value;
		if (eventTable.TryGetValue(eventType, out value))
		{
			Callback<T, U> callback = (Callback<T, U>)value;
			if (callback != null)
			{
				callback(arg1, arg2);
			}
		}
	}
}
