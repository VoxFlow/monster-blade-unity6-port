using System.Collections.Generic;
using UnityEngine;

public class NBProfiler
{
	private static int m_mbSize = 1048576;

	private static MemoryInfo memoryInfo = null;

	private static Dictionary<string, NBTask> taskTiming = new Dictionary<string, NBTask>();

	public static bool Enable
	{
		get
		{
			return UnityEngine.Profiling.Profiler.enabled;
		}
	}

	public static float HeapSize
	{
		get
		{
			return 0f;
		}
	}

	public static float ObjectSize
	{
		get
		{
			return 0f;
		}
	}

	public static float TextureSize
	{
		get
		{
			return 0f;
		}
	}

	public static float TextureCount
	{
		get
		{
			return 0f;
		}
	}

	public static float MeshSize
	{
		get
		{
			return 0f;
		}
	}

	public static float AudioSize
	{
		get
		{
			return 0f;
		}
	}

	public static float AnimSize
	{
		get
		{
			return 0f;
		}
	}

	public static void Initialize()
	{
		UnityEngine.Profiling.Profiler.enabled = false;
		Debug.Log("Profiler is disable.");
	}

	public static float GetRuntimeMemorySizeInMB(Object _obj)
	{
		return BytesToMegaByte(GetRuntimeMemorySize(_obj));
	}

	public static int GetRuntimeMemorySize(Object _obj)
	{
		return 0;
	}

	public static void PrintUsedHeap(string _header = "")
	{
	}

	public static List<T> FindAllLoaded<T>(bool _useSizeLimit = false) where T : Object
	{
		return null;
	}

	public static void SearchMemoryUsage()
	{
	}

	public static void BeginTask(string _taskName)
	{
	}

	public static void EndTask(string _taskName)
	{
	}

	public static float BytesToMegaByte(long _size)
	{
		return (float)_size / (float)m_mbSize;
	}
}
