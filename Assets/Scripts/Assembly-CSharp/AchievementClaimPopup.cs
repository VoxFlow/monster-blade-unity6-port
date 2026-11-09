using UnityEngine;

public class AchievementClaimPopup : MonoBehaviour
{
	[SerializeField]
	private InventoryCell[] invCells;

	[SerializeField]
	private UILabel[] amountTexts;

	[SerializeField]
	private UISprite igcIcon;

	[SerializeField]
	private UISprite pgcIcon;

	[SerializeField]
	private UISprite xpIcon;

	[SerializeField]
	private Transform IconPos_1;

	[SerializeField]
	private Transform IconPos_2;

	[SerializeField]
	private ParticleSystem[] m_effects;

	public void SetItemInfo(AchievementManager.AchievementRewards p_achiReward)
	{
		igcIcon.enabled = false;
		pgcIcon.enabled = false;
		xpIcon.enabled = false;
		UILabel[] array = amountTexts;
		foreach (UILabel uILabel in array)
		{
			uILabel.text = string.Empty;
		}
		int num = 0;
		InventoryCell[] array2 = invCells;
		foreach (InventoryCell inventoryCell in array2)
		{
			inventoryCell.HideChildren();
			inventoryCell.enabled = false;
		}
		foreach (string rewardItemId in p_achiReward.rewardItemIds)
		{
			if (!string.IsNullOrEmpty(rewardItemId))
			{
				ItemDetails data = SingletonComponent<InventoryManager>.Instance.CreateItem(rewardItemId);
				invCells[num].enabled = true;
				invCells[num].data = data;
				amountTexts[num].gameObject.SetActiveRecursively(true);
				amountTexts[num].text = "1";
				num++;
			}
			if (num >= 2)
			{
				return;
			}
		}
		if (p_achiReward.rewardPrem > 0)
		{
			pgcIcon.enabled = true;
			amountTexts[num].gameObject.SetActiveRecursively(true);
			amountTexts[num].text = p_achiReward.rewardPrem.ToString();
			HookIcon(num, pgcIcon.transform);
			num++;
			if (num >= 2)
			{
				return;
			}
		}
		if (p_achiReward.rewardXP > 0)
		{
			xpIcon.enabled = true;
			amountTexts[num].gameObject.SetActiveRecursively(true);
			amountTexts[num].text = p_achiReward.rewardXP.ToString();
			HookIcon(num, xpIcon.transform);
			num++;
			if (num >= 2)
			{
				return;
			}
		}
		if (p_achiReward.rewardIgc > 0)
		{
			igcIcon.enabled = true;
			amountTexts[num].gameObject.SetActiveRecursively(true);
			amountTexts[num].text = p_achiReward.rewardIgc.ToString();
			HookIcon(num, igcIcon.transform);
			num++;
			if (num >= 2)
			{
				return;
			}
		}
		Dbg.Log("[Info] Show achievement reward: " + p_achiReward.ToString());
	}

	public void DeleteEffects()
	{
		ParticleSystem[] effects = m_effects;
		foreach (ParticleSystem particleSystem in effects)
		{
			if (particleSystem != null)
			{
				Object.DestroyImmediate(particleSystem.gameObject);
			}
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void HookIcon(int index, Transform trans)
	{
		if (!(trans == null) && !(IconPos_1 == null) && !(IconPos_2 == null))
		{
			if (index == 0)
			{
				trans.position = IconPos_1.position;
			}
			if (index == 1)
			{
				trans.position = IconPos_2.position;
			}
		}
	}

	private Vector3 DefineIconPosition(int p_index)
	{
		if (p_index == 0)
		{
			return new Vector3(-150f, 0f, -5f);
		}
		return new Vector3(150f, 0f, -5f);
	}
}
