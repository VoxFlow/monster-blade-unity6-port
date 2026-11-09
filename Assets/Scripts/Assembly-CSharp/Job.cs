using System;
using System.Collections;
using System.Collections.Generic;

public class Job
{
	private bool m_wasJobKilled;

	private IEnumerator m_coroutine;

	private Stack<Job> m_childJobStack;

	public bool Running { get; private set; }

	public bool Paused { get; private set; }

	public event Action<bool> OnJobCompleted;

	public Job(IEnumerator p_coroutine)
		: this(p_coroutine, true)
	{
	}

	public Job(IEnumerator p_coroutine, bool shouldStart)
	{
		m_coroutine = p_coroutine;
		if (shouldStart)
		{
			Start();
		}
	}

	public static Job Make(IEnumerator p_coroutine)
	{
		return new Job(p_coroutine);
	}

	public static Job Make(IEnumerator p_coroutine, bool shouldStart)
	{
		return new Job(p_coroutine, shouldStart);
	}

	public Job CreateAndAddChildJob(IEnumerator p_coroutine)
	{
		Job job = new Job(p_coroutine, false);
		AddChildJob(job);
		return job;
	}

	public void AddChildJob(Job p_childJob)
	{
		if (m_childJobStack == null)
		{
			m_childJobStack = new Stack<Job>();
		}
		m_childJobStack.Push(p_childJob);
	}

	public void RemoveChildJob(Job p_childJob)
	{
		if (!m_childJobStack.Contains(p_childJob))
		{
			return;
		}
		Stack<Job> stack = new Stack<Job>(m_childJobStack.Count - 1);
		Job[] array = m_childJobStack.ToArray();
		Array.Reverse(array);
		foreach (Job job in array)
		{
			if (job != p_childJob)
			{
				stack.Push(job);
			}
		}
		m_childJobStack = stack;
	}

	public void Start()
	{
		Running = true;
		SingletonComponent<JobManager>.Instance.StartCoroutine(DoWork());
	}

	public IEnumerator StartAsCoroutine()
	{
		Running = true;
		yield return SingletonComponent<JobManager>.Instance.StartCoroutine(DoWork());
	}

	public void Pause()
	{
		Paused = true;
	}

	public void UnPause()
	{
		Paused = false;
	}

	public void Kill()
	{
		m_wasJobKilled = true;
		Running = false;
		Paused = false;
	}

	private IEnumerator DoWork()
	{
		yield return null;
		while (Running)
		{
			if (Paused)
			{
				yield return null;
				continue;
			}
			if (m_coroutine.MoveNext())
			{
				yield return m_coroutine.Current;
				continue;
			}
			if (m_childJobStack != null)
			{
				yield return SingletonComponent<JobManager>.Instance.StartCoroutine(RunChildJobs());
			}
			Running = false;
		}
		if (this.OnJobCompleted != null)
		{
			this.OnJobCompleted(m_wasJobKilled);
		}
	}

	private IEnumerator RunChildJobs()
	{
		if (m_childJobStack != null && m_childJobStack.Count > 0)
		{
			do
			{
				Job childJob = m_childJobStack.Pop();
				yield return SingletonComponent<JobManager>.Instance.StartCoroutine(childJob.StartAsCoroutine());
			}
			while (m_childJobStack.Count > 0);
		}
	}
}
