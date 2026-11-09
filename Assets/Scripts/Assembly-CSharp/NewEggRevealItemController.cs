using UnityEngine;

public class NewEggRevealItemController : MonoBehaviour
{
	[SerializeField]
	private NewEggRevealItem m_normalEgg;

	[SerializeField]
	private NewEggRevealItem m_rareEgg;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void EnableRareEgg(bool p_enable)
	{
		m_normalEgg.gameObject.SetActiveRecursively(!p_enable);
		m_rareEgg.gameObject.SetActiveRecursively(p_enable);
	}

	public void EnableEggs(bool p_enable)
	{
		m_normalEgg.gameObject.SetActiveRecursively(p_enable);
		m_rareEgg.gameObject.SetActiveRecursively(p_enable);
	}

	public void SetEggTexture(string p_element, string p_rarity)
	{
		int num = 0;
		switch (p_element.ToLower())
		{
		case "metal":
			num = 0;
			break;
		case "water":
		case "ice":
			num = 3;
			break;
		case "lightning":
			num = 12;
			break;
		case "nature":
		case "earth":
			num = 9;
			break;
		default:
			num = 6;
			break;
		}
		switch (p_rarity.ToLower())
		{
		case "rare":
			num++;
			m_rareEgg.SetEggFracTextureBasedOnElement(num);
			m_normalEgg.gameObject.SetActiveRecursively(false);
			break;
		case "super rare":
			num += 2;
			m_rareEgg.SetEggFracTextureBasedOnElement(num);
			m_normalEgg.gameObject.SetActiveRecursively(false);
			break;
		default:
			num = num;
			m_normalEgg.SetEggFracTextureBasedOnElement(num);
			m_rareEgg.gameObject.SetActiveRecursively(false);
			break;
		}
	}
}
