using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waiter
{
	private static List<Waiter> ms_waiters = new List<Waiter>();

	public bool enabled = true;

	public Waiter()
	{
		ms_waiters.Add(this);
	}

	~Waiter()
	{
		ms_waiters.Remove(this);
	}

	public static void PauseAll(bool p_flag)
	{
		foreach (Waiter ms_waiter in ms_waiters)
		{
			ms_waiter.enabled = !p_flag;
		}
	}

	public IEnumerator WaitUntil(float p_seconds, bool p_timeScaleInsensitive = false)
	{
		float duration = 0f;
		while (duration < p_seconds)
		{
			yield return new WaitForFixedUpdate();
			if (enabled)
			{
				float elapsed = Time.deltaTime / ((!p_timeScaleInsensitive) ? 1f : Time.timeScale);
				duration += elapsed;
			}
		}
	}
}
