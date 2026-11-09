using UnityEngine;

public class WeaponCheckPopup : MonoBehaviour
{
	[SerializeField]
	private InventoryCell m_weaponCell;

	[SerializeField]
	private UILabel m_dungeonName;

	[SerializeField]
	private UILabel m_dungeonElement;

	[SerializeField]
	private UILabel m_dungeonLevel;

	[SerializeField]
	private UILabel m_weaponName;

	[SerializeField]
	private UILabel m_weaponElement;

	[SerializeField]
	private UILabel m_weaponLevel;

	[SerializeField]
	private TiersStars m_tiersStars;

	private string[] m_elementSymbol = new string[5] { "[F]", "[E]", "[L]", "[W]", "[M]" };

	private void Start()
	{
		if (!(MainMenu.Instance == null) && MainMenu.Instance.m_sharedBackButton != null)
		{
			MainMenu.Instance.m_sharedBackButton.gameObject.SetActiveRecursively(false);
		}
	}

	private void OnDestroy()
	{
		if (!(MainMenu.Instance == null) && MainMenu.Instance.m_sharedBackButton != null)
		{
			MainMenu.Instance.m_sharedBackButton.gameObject.SetActiveRecursively(true);
		}
	}

	private void Update()
	{
	}

	public void SetInfo(Equipment.Data p_weaponData, DungeonDbStruct p_dungeonData)
	{
		if (p_weaponData == null)
		{
			return;
		}
		NBLocalization instance = SingletonComponent<NBLocalization>.Instance;
		if (instance == null)
		{
			Dbg.LogError("Missing instance for NBLocalization");
			return;
		}
		if (m_weaponCell != null)
		{
			m_weaponCell.data = p_weaponData.itemDetails;
		}
		if (m_dungeonName != null)
		{
			m_dungeonName.text = instance.Get(p_dungeonData.name);
		}
		if (m_dungeonElement != null)
		{
			m_dungeonElement.text = instance.Get("weaponcheck_element_label") + ": " + m_elementSymbol[(int)p_dungeonData.element];
		}
		if (m_dungeonLevel != null)
		{
			m_dungeonLevel.text = instance.Get("weaponcheck_level_label") + ": " + p_dungeonData.min;
		}
		if (m_weaponName != null)
		{
			m_weaponName.text = instance.Get(p_weaponData.itemDetails.Name);
		}
		if (m_weaponElement != null)
		{
			string text = NBColorTags.TAG_WHITE;
			if (Element.GetWeakElement(p_weaponData.element) == p_dungeonData.element)
			{
				text = NBColorTags.TAG_GREEN;
			}
			else if (Element.GetWeakElement(p_dungeonData.element) == p_weaponData.element)
			{
				text = NBColorTags.TAG_RED;
			}
			m_weaponElement.text = text + instance.Get("weaponcheck_element_label") + ":[-] " + m_elementSymbol[(int)p_weaponData.element];
		}
		if (m_weaponLevel != null)
		{
			string text2 = NBColorTags.TAG_WHITE;
			if (p_weaponData.itemDetails.Level > p_dungeonData.min)
			{
				text2 = NBColorTags.TAG_GREEN;
			}
			else if (p_weaponData.itemDetails.Level < p_dungeonData.min)
			{
				text2 = NBColorTags.TAG_RED;
			}
			m_weaponLevel.text = text2 + instance.Get("weaponcheck_level_label") + ": " + p_weaponData.itemDetails.Level + "[-]";
		}
		if (m_tiersStars != null)
		{
			m_tiersStars.SetTier(p_weaponData.itemDetails);
		}
	}
}
