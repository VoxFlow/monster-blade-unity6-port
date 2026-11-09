using UnityEngine;

public class ShopPanel : NBUIMenuPanel
{
	public UIButton m_buyBagSlotsButton;

	public UIButton m_buyStaminaButton;

	public UIButton m_buyBattlePointsButton;

	public UIButton m_buyDragonPointsButton;

	public UIButton m_buyDiamondsButton;

	public UIButton m_buyHolyDiamondsButton;

	private StoreManager storeMgr;

	private new void Awake()
	{
		storeMgr = SingletonComponent<StoreManager>.Instance;
		if (storeMgr == null)
		{
			Dbg.LogError("Could not find StoreManager.");
			base.enabled = false;
		}
	}

	private void OnButtonClick(GameObject p_sender)
	{
		if (!(null == p_sender))
		{
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_secondary_ui_tap");
			if (IsButtonPressed(m_buyDiamondsButton, p_sender))
			{
				OnBuyDiamonds();
			}
			else if (IsButtonPressed(m_buyDragonPointsButton, p_sender))
			{
				OnBuyDragonPoints();
			}
			else if (IsButtonPressed(m_buyHolyDiamondsButton, p_sender))
			{
				OnBuyHolyDiamonds();
			}
			else if (IsButtonPressed(m_buyBagSlotsButton, p_sender))
			{
				OnBuyBagSlots();
			}
			else if (IsButtonPressed(m_buyBattlePointsButton, p_sender))
			{
				OnBuyBattlePoint();
			}
			else if (IsButtonPressed(m_buyStaminaButton, p_sender))
			{
				OnBuyStamina();
			}
		}
	}

	private bool IsButtonPressed(UIButton p_button, GameObject p_sender)
	{
		return null != p_button && p_button.gameObject == p_sender;
	}

	private void OnBuyDragonPoints()
	{
		MainMenu instance = MainMenu.Instance;
		if (null != instance)
		{
			instance.ShowDragonPointsExchangePanel();
		}
	}

	private void OnBuyHolyDiamonds()
	{
		MainMenu instance = MainMenu.Instance;
		if (null != instance)
		{
			instance.ShowHolyDiamondsExchangePanel();
		}
	}

	private void OnBuyBagSlots()
	{
		if (storeMgr != null)
		{
			storeMgr.ShowBagSlotsShop();
		}
		Dbg.Log(" -- OnBuyBagSlots Clicked -- ");
	}

	private void OnBuyDiamonds()
	{
		if (storeMgr != null)
		{
			storeMgr.ShowDiamondsShop();
		}
		Dbg.Log(" -- OnBuyDiamonds Clicked -- ");
	}

	private void OnBuyStamina()
	{
		if (storeMgr != null)
		{
			storeMgr.ShowStaminaShop();
		}
		Dbg.Log(" -- OnBuyStamina Clicked -- ");
	}

	private void OnBuyBattlePoint()
	{
		if (storeMgr != null)
		{
			storeMgr.ShowBattlePointShop();
		}
		Dbg.Log(" -- OnBuyBattlePoint Clicked -- ");
	}
}
