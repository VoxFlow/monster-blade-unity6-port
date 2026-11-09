using UnityEngine;

public class SetInfoPanel : MonoBehaviour
{
	public Component eventTarget;

	[SerializeField]
	private InventoryCell m_setInvCellTemplate;

	[SerializeField]
	private UILabel m_setName;

	[SerializeField]
	private UILabel m_setNumText;

	[SerializeField]
	private UISprite m_setElementIcon;

	private SetInformation m_setInfo;

	private void Start()
	{
		DungeonSelectPanel component = base.transform.parent.gameObject.GetComponent<DungeonSelectPanel>();
		eventTarget = component.eventTarget;
	}

	private void Update()
	{
	}

	public void SetSetInfo(SetInformation p_setInfo)
	{
		m_setInfo = p_setInfo;
		m_setName.text = p_setInfo.SetName;
		int num = 0;
		if (p_setInfo.WeaponAcquired)
		{
			num++;
		}
		if (p_setInfo.HelmAcquired)
		{
			num++;
		}
		if (p_setInfo.ShoulderAcquired)
		{
			num++;
		}
		if (p_setInfo.ChestAcquired)
		{
			num++;
		}
		if (p_setInfo.LeggingAcquired)
		{
			num++;
		}
		m_setNumText.text = num + "/5";
		ItemDetails itemDetails = SingletonComponent<InventoryManager>.Instance.CreateItem(p_setInfo.WeaponItemId, GetItemTierByItemId(p_setInfo.WeaponItemId), 1);
		if (m_setInvCellTemplate != null)
		{
			GameObject gameObject = Object.Instantiate(m_setInvCellTemplate.gameObject, Vector3.zero, Quaternion.identity) as GameObject;
			if (gameObject != null)
			{
				gameObject.transform.parent = base.transform;
				gameObject.transform.localPosition = new Vector3(-336f, 28f, -2f);
				gameObject.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
				InventoryCell component = gameObject.GetComponent<InventoryCell>();
				component.HideChildren();
				if (itemDetails != null)
				{
					component.data = itemDetails;
				}
				else
				{
					Dbg.LogWarnEx("Unable to create item data for item id: {0}", Dbg.NBLogType.COMMON_WARNING, p_setInfo.WeaponItemId);
				}
				component.data = itemDetails;
			}
		}
		switch (itemDetails.Element)
		{
		case Element.Type.Earth:
			m_setElementIcon.spriteName = "nature_icon";
			break;
		case Element.Type.Fire:
			m_setElementIcon.spriteName = "fire_icon";
			break;
		case Element.Type.Lightning:
			m_setElementIcon.spriteName = "lightning_icon";
			break;
		case Element.Type.Metal:
			m_setElementIcon.spriteName = "metal_icon";
			break;
		case Element.Type.Water:
			m_setElementIcon.spriteName = "water_icon";
			break;
		}
	}

	public void Show()
	{
		base.gameObject.SetActiveRecursively(true);
	}

	public void Hide()
	{
		base.gameObject.SetActiveRecursively(false);
	}

	public void OnViewBtnClick()
	{
		eventTarget.SendMessage("OnViewSetDemoEvent", m_setInfo, SendMessageOptions.DontRequireReceiver);
	}

	private int GetItemTierByItemId(string p_itemId)
	{
		if (p_itemId.Contains("T1"))
		{
			return 1;
		}
		if (p_itemId.Contains("T2"))
		{
			return 2;
		}
		if (p_itemId.Contains("T3"))
		{
			return 3;
		}
		if (p_itemId.Contains("T4"))
		{
			return 4;
		}
		if (p_itemId.Contains("T5"))
		{
			return 5;
		}
		return -1;
	}
}
