using UnityEngine;

public class BottomBar : MonoBehaviour
{
	public BottomMenuButton[] m_buttons = new BottomMenuButton[8];

	private void Awake()
	{
		Input.multiTouchEnabled = false;
		Dbg.Log("Disable multi touch");
	}

	private void OnEnable()
	{
		HideUnusedButton();
	}

	private void OnDisable()
	{
	}

	private void Start()
	{
		for (int i = 0; i < m_buttons.Length; i++)
		{
			UIButtonMessage component = m_buttons[i].GetComponent<UIButtonMessage>();
			component.id = i;
			if (MainMenu.Instance != null)
			{
				component.target = MainMenu.Instance.gameObject;
			}
		}
	}

	public void SetAllButtonsEnable(bool p_enable)
	{
		for (int i = 0; i < m_buttons.Length; i++)
		{
			m_buttons[i].isEnabled = !m_buttons[i].isLocked && p_enable;
		}
	}

	public void SetButtonEnable(int p_index, bool p_enable)
	{
		if (p_index < m_buttons.Length)
		{
			m_buttons[p_index].isEnabled = !m_buttons[p_index].isLocked && p_enable;
		}
	}

	public void LockAllMenus()
	{
		for (int i = 0; i < m_buttons.Length; i++)
		{
			m_buttons[i].isLocked = true;
		}
	}

	public void LockMenu(MainMenu.Menu menu)
	{
		m_buttons[(int)menu].isLocked = true;
	}

	public bool IsMenuLocked(MainMenu.Menu menu)
	{
		return m_buttons[(int)menu].isLocked;
	}

	public void UnlockMenu(MainMenu.Menu menu)
	{
		m_buttons[(int)menu].isLocked = false;
	}

	public void UnlockAllMenus()
	{
		for (int i = 0; i < m_buttons.Length; i++)
		{
			m_buttons[i].isLocked = false;
		}
	}

	public void HideUnusedButton()
	{
		if (m_buttons.Length < 2 && m_buttons.Length < 3)
		{
			if (m_buttons[2] != null)
			{
				m_buttons[2].gameObject.SetActiveRecursively(false);
			}
			if (m_buttons[3] != null)
			{
				m_buttons[3].gameObject.SetActiveRecursively(false);
			}
		}
	}
}
