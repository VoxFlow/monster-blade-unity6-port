using UnityEngine;

public class Pointer : MonoBehaviour
{
	public enum State
	{
		None = 0,
		Down = 1,
		Held = 2,
		Up = 3,
		Move = 4
	}

	public Vector2 m_pos = Vector2.zero;

	public Vector2 m_prevPos = Vector2.zero;

	public int m_id;

	public State m_state;

	public Vector2 MoveVector
	{
		get
		{
			return m_pos - m_prevPos;
		}
	}

	public void Reset()
	{
		m_pos = Vector2.zero;
		m_prevPos = Vector2.zero;
		m_state = State.None;
	}

	private void Update()
	{
		if (MBInput.touches.Length >= m_id + 1)
		{
			Touch touch = MBInput.GetTouch(m_id);
			m_prevPos = m_pos;
			m_pos = touch.position;
			if (touch.phase == TouchPhase.Began)
			{
				m_state = State.Down;
			}
			else if (touch.phase == TouchPhase.Stationary)
			{
				m_state = State.Held;
			}
			else if (touch.phase == TouchPhase.Ended)
			{
				m_state = State.Up;
			}
			else if (touch.phase == TouchPhase.Moved)
			{
				m_state = State.Move;
			}
			else
			{
				m_state = State.None;
			}
		}
	}
}
