using UnityEngine;

public class FusionMaterialItem : MonoBehaviour
{
	public UISprite[] tierStars;

	private void Awake()
	{
		UISprite[] array = tierStars;
		foreach (UISprite uISprite in array)
		{
			if (uISprite == null)
			{
				Dbg.LogWarn("Unable to show tier stars for fusion item icon");
			}
			else
			{
				uISprite.gameObject.SetActiveRecursively(false);
			}
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void ShowItemTierStars(ItemDetails p_data)
	{
		if (p_data.Category == ItemEnum.Category.LOOT)
		{
			return;
		}
		for (int i = 0; i < p_data.equipment.maxTier; i++)
		{
			if (i < tierStars.Length)
			{
				tierStars[i].gameObject.SetActiveRecursively(true);
				if (i < p_data.Tier)
				{
					tierStars[i].spriteName = "star";
				}
				else
				{
					tierStars[i].spriteName = "no_star";
				}
			}
		}
	}
}
