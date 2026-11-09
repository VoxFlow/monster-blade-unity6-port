using System;
using System.Collections.Generic;

[Serializable]
public class ShareData
{
	private Dictionary<object, Action<ShareData>> m_requests = new Dictionary<object, Action<ShareData>>();

	protected Mutex mutex { get; private set; }

	public bool locked
	{
		get
		{
			return mutex.locked;
		}
	}

	public Action<bool> onLockedCallback { get; set; }

	public ShareData()
	{
		mutex = new Mutex();
	}

	public void Lock(object p_key, Action<ShareData> p_onReceiveLock)
	{
		if (!m_requests.ContainsKey(p_key))
		{
			m_requests.Add(p_key, p_onReceiveLock);
		}
		mutex.GetLock(p_key, OnRecieveLock);
	}

	public void Unlock(object p_key)
	{
		if (m_requests.ContainsKey(p_key))
		{
			m_requests.Remove(p_key);
		}
		_Unlock(p_key);
	}

	private void OnRecieveLock(object p_key)
	{
		if (m_requests.ContainsKey(p_key))
		{
			if (m_requests[p_key] != null)
			{
				m_requests[p_key](this);
			}
			if (m_requests.Count == 1 && onLockedCallback != null)
			{
				onLockedCallback(true);
			}
		}
		else
		{
			_Unlock(p_key);
		}
	}

	private void _Unlock(object p_key)
	{
		if (mutex.holdingKey == p_key)
		{
			mutex.Unlock(p_key);
		}
		if (m_requests.Count == 0 && onLockedCallback != null)
		{
			onLockedCallback(false);
		}
	}
}
