using UnityEngine;

public class ArmorCollectionsHomePanel : NBUIMenuPanel
{
	public UIButton m_normalButton;

	public UIButton m_specialButton;

	private void OnEnable()
	{
		Init();
	}

	private void Init()
	{
		UpdateBadges();
	}

	private void UpdateBadges()
	{
		int p_count = 0;
		int p_count2 = 0;
		InventoryManager instance = SingletonComponent<InventoryManager>.Instance;
		if (null != instance)
		{
			p_count = instance.GetNewCollectionItemsCount(false);
			p_count2 = instance.GetNewCollectionItemsCount(true);
		}
		SetBadgeCount(m_normalButton, p_count);
		SetBadgeCount(m_specialButton, p_count2);
	}

	private void SetBadgeCount(UIButton p_button, int p_count)
	{
		if (!(null != p_button))
		{
			return;
		}
		UIBadge componentInChildren = MBHier.GetComponentInChildren<UIBadge>(p_button);
		if (null != componentInChildren)
		{
			if (p_count > 0)
			{
				componentInChildren.SetNumber(p_count);
				componentInChildren.gameObject.SetActiveRecursively(true);
			}
			else
			{
				componentInChildren.gameObject.SetActiveRecursively(false);
			}
		}
	}

	private void OnButtonClick(GameObject p_sender)
	{
		if (!(null == p_sender))
		{
			if (null != m_normalButton && p_sender == m_normalButton.gameObject)
			{
				PushState(ArmorCollectionsScreen.ScreenState.ARMOR_LIST_NORMAL);
			}
			else if (null != m_specialButton && p_sender == m_specialButton.gameObject)
			{
				PushState(ArmorCollectionsScreen.ScreenState.ARMOR_LIST_SPECIAL);
			}
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_popup_ok");
		}
	}

	private void PushState(ArmorCollectionsScreen.ScreenState p_state)
	{
		if (!(null == parentScreen))
		{
			ArmorCollectionsScreen armorCollectionsScreen = (ArmorCollectionsScreen)parentScreen;
			armorCollectionsScreen.PushState(p_state);
		}
	}
}
