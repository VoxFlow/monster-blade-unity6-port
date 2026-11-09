using Models;
using UnityEngine;

public class PromoClaimPopup : MonoBehaviour
{
	public InventoryCell m_inventoryCellPrefab;

	public Transform m_itemTransform;

	public UILabel m_itemName;

	[SerializeField]
	private UISprite m_rewardSprite;

	[SerializeField]
	private UISprite m_backgroundRewardSprite;

	[SerializeField]
	private UISprite m_blackSprite;

	protected InventoryCell m_cell;

	private void Awake()
	{
		m_cell = Object.Instantiate(m_inventoryCellPrefab) as InventoryCell;
		if (null != m_itemTransform)
		{
			m_cell.transform.parent = m_itemTransform.parent;
			m_cell.transform.localPosition = m_itemTransform.localPosition;
			m_cell.transform.localScale = Vector3.one;
		}
	}

	public void SetItemDetails(ItemDetails p_data)
	{
		if (null == m_cell)
		{
			return;
		}
		if (null != m_rewardSprite && null != m_blackSprite && null != m_backgroundRewardSprite)
		{
			m_rewardSprite.gameObject.SetActiveRecursively(false);
			m_backgroundRewardSprite.gameObject.SetActiveRecursively(false);
			m_blackSprite.gameObject.SetActiveRecursively(false);
		}
		m_cell.data = p_data;
		if (null != m_itemName)
		{
			NBUILocalize component = m_itemName.GetComponent<NBUILocalize>();
			if (null != component)
			{
				component.Key = p_data.Name;
			}
		}
	}

	public void SetCommonReward(CommonRewards p_reward, UISprite p_sprite)
	{
		if (p_reward.pet == null || !(null != m_rewardSprite) || !(null != p_sprite) || !(null != m_backgroundRewardSprite) || !(null != m_blackSprite))
		{
			return;
		}
		m_rewardSprite.gameObject.SetActiveRecursively(true);
		m_backgroundRewardSprite.gameObject.SetActiveRecursively(true);
		m_blackSprite.gameObject.SetActiveRecursively(true);
		m_rewardSprite.enabled = true;
		m_backgroundRewardSprite.enabled = true;
		m_blackSprite.enabled = true;
		if (null != m_cell)
		{
			m_cell.gameObject.SetActiveRecursively(false);
		}
		PetDetails pet = p_reward.pet;
		m_rewardSprite.atlas = p_sprite.atlas;
		m_rewardSprite.spriteName = pet.PetIconPath;
		m_backgroundRewardSprite.atlas = p_sprite.atlas;
		m_backgroundRewardSprite.spriteName = pet.PetFrameName;
		if (null != m_itemName)
		{
			NBUILocalize component = m_itemName.GetComponent<NBUILocalize>();
			if (null != component)
			{
				component.Key = pet.Name;
			}
		}
	}
}
