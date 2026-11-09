using UnityEngine;

public class MenuBackground3DCamera : MonoBehaviour
{
	private MainMenu m_mainMenu;

	private void Awake()
	{
		base.enabled = false;
		Messenger<MainMenu.Menu>.AddListener("mainmenu_screen_enter", OnMenuChanged);
	}

	private void OnDestroy()
	{
		Messenger<MainMenu.Menu>.RemoveListener("mainmenu_screen_enter", OnMenuChanged);
	}

	private void OnMenuChanged(MainMenu.Menu p_menu)
	{
		if (!(null == base.gameObject.GetComponent<Camera>()))
		{
			if (null == m_mainMenu)
			{
				m_mainMenu = Object.FindObjectOfType(typeof(MainMenu)) as MainMenu;
			}
			if (p_menu == MainMenu.Menu.Treasure)
			{
				base.gameObject.GetComponent<Camera>().enabled = false;
			}
			else if (null != m_mainMenu && m_mainMenu.currScreenIndex != 5)
			{
				base.gameObject.GetComponent<Camera>().enabled = true;
			}
		}
	}
}
