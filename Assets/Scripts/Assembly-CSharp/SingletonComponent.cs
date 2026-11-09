using UnityEngine;

public class SingletonComponent<T> : MonoBehaviour where T : SingletonComponent<T>
{
	private static T m_instance;

	public static T Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = Object.FindObjectOfType(typeof(T)) as T;
				if (m_instance != null)
				{
					Dbg.Log(string.Concat(m_instance.GetType(), " Instance Found"));
				}
				else
				{
					Dbg.LogError(string.Concat("No instance of ", typeof(T), " found !!!"));
				}
			}
			return m_instance;
		}
	}

	protected virtual void OnDestroy()
	{
		Debug.Log("********* SingletonComponent(" + base.name + ") was destroyed **********");
		m_instance = (T)null;
	}

	public void OnApplicationQuit()
	{
		m_instance = (T)null;
	}
}
