using System;
using System.Reflection;

public class Singleton<T> where T : class
{
	private static T m_instance;

	public static T Instance
	{
		get
		{
			if (m_instance == null)
			{
				ConstructorInfo constructorInfo = null;
				constructorInfo = typeof(T).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[0], null);
				if (constructorInfo != null)
				{
					Dbg.LogWarn("Call private constructor of : " + typeof(T));
					m_instance = (T)constructorInfo.Invoke(null);
				}
				else
				{
					Dbg.LogError("Cannot find non public constructor!");
				}
			}
			return m_instance;
		}
	}

	protected Singleton()
	{
	}
}
