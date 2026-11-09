using System;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
{
	protected Enum m_state;

	protected Enum m_prevState;

	private bool[,] m_transitionTable;

	public Enum CurrentState
	{
		get
		{
			return m_state;
		}
	}

	public Enum PreviousState
	{
		get
		{
			return m_prevState;
		}
	}

	protected void InitTransitionTable(List<Enum>[] p_transitList)
	{
		if (p_transitList == null)
		{
			Debug.LogError("p_transitList cannot be null");
			return;
		}
		int num = p_transitList.Length;
		m_transitionTable = new bool[num, num];
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num; j++)
			{
				m_transitionTable[i, j] = false;
			}
		}
		for (int k = 0; k < num; k++)
		{
			foreach (Enum item in p_transitList[k])
			{
				int num2 = Convert.ToInt32(item);
				if (num2 >= 0 && num2 < num)
				{
					m_transitionTable[num2, k] = true;
				}
			}
		}
	}

	public bool IsReachable(Enum p_source, Enum p_target)
	{
		int num = Convert.ToInt32(p_source);
		int num2 = Convert.ToInt32(p_target);
		if (num < 0 || num >= m_transitionTable.GetLength(0))
		{
			return true;
		}
		if (num2 < 0 || num2 >= m_transitionTable.GetLength(1))
		{
			return false;
		}
		return m_transitionTable[Convert.ToInt32(p_source), Convert.ToInt32(p_target)];
	}

	public bool IsReachableFromCurrent(Enum p_target)
	{
		return m_state != null && m_transitionTable[Convert.ToInt32(m_state), Convert.ToInt32(p_target)];
	}

	public virtual bool ChangeState(Enum p_next)
	{
		if (m_state == null || IsReachable(m_state, p_next))
		{
			if (m_state != null)
			{
				OnExit(m_state, p_next);
			}
			m_prevState = m_state;
			m_state = p_next;
			OnEnter(m_prevState, m_state);
			return true;
		}
		Dbg.LogWarn(string.Concat("Cannot change from ", m_state, " to ", p_next, "."));
		return false;
	}

	protected virtual void OnEnter(Enum p_previous, Enum p_current)
	{
	}

	protected virtual void OnExit(Enum p_current, Enum p_next)
	{
	}

	public void DumpTransitionTable()
	{
		for (int i = 0; i < m_transitionTable.GetLength(0); i++)
		{
			string text = string.Empty;
			for (int j = 0; j < m_transitionTable.GetLength(1); j++)
			{
				text = text + m_transitionTable[i, j] + " ";
			}
			Debug.Log(text);
		}
	}

	protected virtual void Update()
	{
	}
}
