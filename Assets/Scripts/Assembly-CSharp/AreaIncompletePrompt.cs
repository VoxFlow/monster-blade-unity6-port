using UnityEngine;

public class AreaIncompletePrompt : MonoBehaviour
{
	public UIButton m_buttonHome;

	public UIButton m_buttonUpgrade;

	public UIButton m_buttonTreasure;

	public UILabel m_labelUpgrade;

	public UILabel m_labelTreasure;

	private void Awake()
	{
		Dbg.Assert(m_buttonHome);
		Dbg.Assert(m_buttonUpgrade);
	}

	private void Start()
	{
		DungeonManager instance = SingletonComponent<DungeonManager>.Instance;
		bool flag = false;
		if (null != instance)
		{
			if (!instance.IsPlayerWeaponGoodElementForCurrentDungeon)
			{
				SetWidgetEnabled(m_buttonUpgrade, false);
				SetWidgetEnabled(m_labelUpgrade, false);
			}
			else if (!instance.IsPlayerWeaponGoodLevelForCurrentDungeon)
			{
				SetWidgetEnabled(m_buttonTreasure, false);
				SetWidgetEnabled(m_labelTreasure, false);
			}
			else
			{
				flag = true;
			}
		}
		else
		{
			flag = true;
		}
		if (flag)
		{
			SetWidgetEnabled(m_buttonTreasure, false);
			SetWidgetEnabled(m_buttonUpgrade, false);
			SetWidgetEnabled(m_labelTreasure, false);
			SetWidgetEnabled(m_labelUpgrade, false);
		}
	}

	private void SetWidgetEnabled(Component p_widget, bool p_enabled)
	{
		if (null != p_widget)
		{
			p_widget.gameObject.SetActiveRecursively(p_enabled);
		}
	}

	private void OnButtonClick(GameObject p_source)
	{
		if (!(null == p_source))
		{
			MainMenu instance = MainMenu.Instance;
			if (null == instance)
			{
				Dbg.LogError("There is no instance of MainMenu.");
			}
			else if (null != m_buttonHome && p_source == m_buttonHome.gameObject)
			{
				SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_cancel");
				instance.StartHomePage();
			}
			else if (null != m_buttonUpgrade && p_source == m_buttonUpgrade.gameObject)
			{
				SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_ok");
				instance.ShowBottomBar(true);
				instance.ShowUpgradePage();
			}
			else if (null != m_buttonTreasure && p_source == m_buttonTreasure.gameObject)
			{
				SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_ok");
				instance.ShowBottomBar(true);
				instance.ShowGachaPage();
			}
		}
	}

	private void Update()
	{
		MainMenu instance = MainMenu.Instance;
		if (!(null == instance) && Input.GetKeyDown(KeyCode.Escape))
		{
			instance.StartHomePage();
		}
	}
}
