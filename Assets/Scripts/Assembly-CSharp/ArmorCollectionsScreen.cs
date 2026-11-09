using UnityEngine;

public class ArmorCollectionsScreen : NBUIMenuScreen
{
	public enum ScreenState
	{
		HOME = 0,
		ARMOR_LIST_NORMAL = 1,
		ARMOR_LIST_SPECIAL = 2,
		ARMOR_DETAIL = 3
	}

	private ScreenState m_currState;

	private ScreenState m_nextState;

	private bool m_init;

	private ArmorSet m_selectedArmorSet;

	private ScreenState currState
	{
		get
		{
			return m_currState;
		}
		set
		{
			m_currState = value;
			if (null != m_backButton && !m_backButton.gameObject.active)
			{
				m_backButton.gameObject.SetActiveRecursively(true);
			}
			string text = string.Empty;
			switch (m_currState)
			{
			case ScreenState.HOME:
				text = "armorcollections_header_home";
				break;
			case ScreenState.ARMOR_LIST_NORMAL:
				text = "armorcollections_header_list_normal";
				break;
			case ScreenState.ARMOR_LIST_SPECIAL:
				text = "armorcollections_header_list_special";
				break;
			case ScreenState.ARMOR_DETAIL:
				text = string.Empty;
				break;
			}
			MainMenu instance = MainMenu.Instance;
			if (null != instance && text != string.Empty)
			{
				instance.EnableHeader(true, text);
			}
			else
			{
				instance.EnableHeader(false, string.Empty);
			}
		}
	}

	public ArmorSet selectedArmorSet
	{
		get
		{
			return m_selectedArmorSet;
		}
		set
		{
			m_selectedArmorSet = value;
		}
	}

	public override void OnEnable()
	{
		base.OnEnable();
		Init();
	}

	public override void OnDisable()
	{
		base.OnDisable();
		m_init = false;
		Reset();
	}

	protected override void Init()
	{
		if (m_init)
		{
			return;
		}
		MainMenu instance = MainMenu.Instance;
		if (null != instance)
		{
			if (null == m_backButton)
			{
				m_backButton = MainMenu.Instance.m_sharedBackButton;
			}
			instance.ShowTopAndBottomBar(false);
		}
		if (null != m_backButton)
		{
			UIButtonMessage component = m_backButton.GetComponent<UIButtonMessage>();
			component.target = base.gameObject;
			m_backButton.transform.localPosition = new Vector3(m_backButton.transform.localPosition.x, -49f, m_backButton.transform.localPosition.z);
		}
		if (null != SingletonComponent<ItemManager>.Instance)
		{
			SingletonComponent<ItemManager>.Instance.CreateArmorCollection();
		}
		PushState(ScreenState.HOME);
		m_init = true;
	}

	protected override void Reset()
	{
		ClearStack();
		SingletonComponent<StageManager>.Instance.NeedUpdatePlayerTemp = true;
		SingletonComponent<InventoryManager>.Instance.SaveNewCollectionItemsToPlayerPrefs();
	}

	public void PushState(ScreenState p_state)
	{
		if (p_state >= ScreenState.HOME && (int)p_state < menuPanels.Length && PushPanelWithTransitions(menuPanels[(int)p_state]))
		{
			m_nextState = p_state;
		}
	}

	private void PopState(bool showPrevious = true)
	{
		PopPanelWithTransitions(showPrevious, OnPopPanelDone);
	}

	private void OnPopPanelDone()
	{
		ScreenState currentPanelIndex = (ScreenState)GetCurrentPanelIndex();
		currState = currentPanelIndex;
	}

	public override void PushPanel(NBUIMenuPanel p_panel)
	{
		base.PushPanel(p_panel);
		currState = m_nextState;
	}

	private void OnButtonClick(GameObject p_sender)
	{
		if (null != m_backButton && p_sender == m_backButton.gameObject)
		{
			OnBack();
		}
	}

	private void OnBack()
	{
		if (GetCurrentPanelIndex() == 0)
		{
			MainMenu.Instance.ShowTownScreen();
		}
		else
		{
			PopState();
		}
		SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_cancel");
	}
}
