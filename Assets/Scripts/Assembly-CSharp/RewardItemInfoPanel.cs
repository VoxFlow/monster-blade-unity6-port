using System.Collections;
using UnityEngine;

public class RewardItemInfoPanel : MonoBehaviour
{
	[SerializeField]
	private ItemInfo m_itemInfo;

	[SerializeField]
	private UISprite m_overlay;

	private Hashtable m_scaleArgs = new Hashtable();

	private void Start()
	{
		if (!m_scaleArgs.ContainsKey("scale"))
		{
			m_scaleArgs.Add("scale", Vector3.one);
		}
		else
		{
			m_scaleArgs["scale"] = Vector3.one;
		}
		if (!m_scaleArgs.ContainsKey("time"))
		{
			m_scaleArgs.Add("time", 0.5f);
		}
		else
		{
			m_scaleArgs["time"] = 0.5f;
		}
		if (!m_scaleArgs.ContainsKey("easetype"))
		{
			m_scaleArgs.Add("easetype", iTween.EaseType.easeOutBack);
		}
		else
		{
			m_scaleArgs["easetype"] = iTween.EaseType.easeOutBack;
		}
	}

	private void Update()
	{
	}

	public void ShowItemInfo(ItemDetails p_itemData)
	{
		base.gameObject.SetActiveRecursively(true);
		m_overlay.gameObject.active = true;
		m_overlay.SendMessage("FadeIn", null, SendMessageOptions.DontRequireReceiver);
		iTween.ScaleTo(base.gameObject, m_scaleArgs);
		if (p_itemData == null)
		{
			m_itemInfo.ShowItem(ItemManager.s_NULL_DATA);
		}
		else
		{
			m_itemInfo.ShowItem(p_itemData, false);
		}
	}

	public void Hide()
	{
		m_overlay.SendMessage("FadeOut", null, SendMessageOptions.DontRequireReceiver);
		base.gameObject.SetActiveRecursively(false);
	}

	private void OnBackButtonClick()
	{
		Hide();
	}
}
