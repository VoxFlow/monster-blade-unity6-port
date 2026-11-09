using UnityEngine;

public class GachaCave : MonoBehaviour
{
	public GameObject premiumCave;

	public GameObject premiumDoor;

	public GameObject premiumChestFx;

	public GameObject premiumChest;

	public GameObject premiumFx;

	public GameObject premiumCamera1;

	public GameObject premiumCamera2;

	public Transform premiumChestTransform;

	public GameObject socialCave;

	public GameObject socialFx;

	public GameObject socialChestFx;

	public GameObject socialChest;

	public GameObject socialCamera1;

	public Transform socialChestTransform;

	private static GachaCave s_instance;

	public static GachaCave Instance
	{
		get
		{
			return s_instance;
		}
	}

	private void OnDestroy()
	{
		s_instance = null;
	}

	private void OnDisable()
	{
		s_instance = null;
	}

	private void OnEnable()
	{
		s_instance = this;
	}

	private void Awake()
	{
		if (s_instance == null)
		{
			s_instance = this;
		}
		if (Instance == null)
		{
			Dbg.LogError("No Gacha Cave found.");
		}
	}

	public void ShowChest(int p_tier, GachaScreen.Choice p_section)
	{
		GachaChest gachaChest = null;
		if (p_section == GachaScreen.Choice.FRIENDSHIP && null != socialChest)
		{
			gachaChest = socialChest.GetComponent<GachaChest>();
		}
		else if (p_section == GachaScreen.Choice.PREMIUM && null != premiumChest)
		{
			gachaChest = premiumChest.GetComponent<GachaChest>();
		}
		bool flag = false;
		if (null != gachaChest && gachaChest.m_chests != null && gachaChest.m_chests.Length > 0)
		{
			for (int i = 0; i < gachaChest.m_chests.Length; i++)
			{
				if (null != gachaChest.m_chests[i])
				{
					gachaChest.m_chests[i].SetActiveRecursively(false);
				}
			}
			int num = Mathf.Clamp(p_tier - 1, 0, gachaChest.m_chests.Length - 1);
			if (null != gachaChest.m_chests[num])
			{
				flag = true;
				gachaChest.m_chests[num].SetActiveRecursively(true);
			}
		}
		if (!flag)
		{
			Dbg.LogError("No chest of tier " + p_tier + " found in the gacha cave.", Dbg.NBLogType.GACHA);
		}
	}
}
