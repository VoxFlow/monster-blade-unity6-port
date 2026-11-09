using System.Text;
using UnityEngine;

public class NBTask
{
	public static string levelHeader = "-";

	public string name = string.Empty;

	public float startTime;

	public float endTime;

	public float duration;

	public float startHeap;

	public float endHeap;

	public float heapDiff;

	public int startFrameCount;

	public int endFrameCount;

	public float averageFPS;

	public int level;

	public NBTask(string _name, int _level = 0)
	{
		name = _name;
		startTime = Time.realtimeSinceStartup;
		endTime = startTime;
		duration = 0f;
		startHeap = NBProfiler.HeapSize;
		endHeap = startHeap;
		heapDiff = 0f;
		startFrameCount = Time.frameCount;
		endFrameCount = Time.frameCount;
		averageFPS = 0f;
		level = _level;
	}

	public void EndTask()
	{
		endTime = Time.realtimeSinceStartup;
		endHeap = NBProfiler.HeapSize;
		endFrameCount = Time.frameCount;
		heapDiff = endHeap - startHeap;
		duration = endTime - startTime;
		if (duration > 0f)
		{
			averageFPS = (float)(endFrameCount - startFrameCount) / duration;
		}
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < level + 1; i++)
		{
			stringBuilder.Append(levelHeader);
		}
		string result = stringBuilder.ToString() + ">" + name + "\n\t\tduration : " + string.Format("{0:0.00} sec", duration) + "\n\t\tHeapDiff : " + string.Format("{0:0.00} MB", heapDiff) + "\n\t\tEndHeap  : " + string.Format("{0:0.00} MB", endHeap) + "\n\t\tAvFPS  \t : " + string.Format("{0:0.00} FPS", averageFPS);
		stringBuilder = null;
		return result;
	}
}
