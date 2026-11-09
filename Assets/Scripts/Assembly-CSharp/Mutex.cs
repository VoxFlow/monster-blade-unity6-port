using System;
using System.Collections.Generic;

public class Mutex
{
	private struct LockRequest
	{
		public object key;

		public Action<object> onReceiveLockCallback;
	}

	private Queue<LockRequest> m_requestQueue = new Queue<LockRequest>();

	public bool locked
	{
		get
		{
			return m_requestQueue.Count > 0;
		}
	}

	public object holdingKey
	{
		get
		{
			if (m_requestQueue.Count == 0)
			{
				return null;
			}
			return m_requestQueue.Peek().key;
		}
	}

	public void GetLock(object p_key, Action<object> p_onReceiveLockCallback)
	{
		if (p_key == null || p_onReceiveLockCallback == null)
		{
			return;
		}
		bool flag = false;
		if (!IsWaiting(p_key))
		{
			LockRequest item = new LockRequest
			{
				key = p_key,
				onReceiveLockCallback = p_onReceiveLockCallback
			};
			m_requestQueue.Enqueue(item);
			if (m_requestQueue.Count == 1)
			{
				flag = true;
			}
		}
		else if (p_key == m_requestQueue.Peek().key)
		{
			flag = true;
		}
		if (flag)
		{
			p_onReceiveLockCallback(p_key);
		}
	}

	public bool Unlock(object p_key)
	{
		if (m_requestQueue.Count == 0)
		{
			return false;
		}
		if (m_requestQueue.Peek().key != p_key)
		{
			return false;
		}
		m_requestQueue.Dequeue();
		if (m_requestQueue.Count > 0)
		{
			LockRequest lockRequest = m_requestQueue.Peek();
			if (lockRequest.onReceiveLockCallback != null)
			{
				lockRequest.onReceiveLockCallback(lockRequest.key);
			}
		}
		return true;
	}

	public bool IsWaiting(object p_key)
	{
		LockRequest[] array = m_requestQueue.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].key == p_key)
			{
				return true;
			}
		}
		return false;
	}

	public int GetNumWaiting()
	{
		return m_requestQueue.Count;
	}
}
