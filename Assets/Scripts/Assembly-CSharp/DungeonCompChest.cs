using System.Collections;
using UnityEngine;

public class DungeonCompChest : MonoBehaviour
{
	public DungeonCompletePanel m_dungeonCompletePanelRef;

	public InventoryCell m_itemCell;

	public bool m_isNewItem;

	public bool m_canClick;

	[SerializeField]
	private UISprite m_chestPic;

	[SerializeField]
	private ParticleSystem[] m_particles;

	private ItemDetails m_itemData;

	private Hashtable m_scaleDownArg = new Hashtable();

	private Hashtable m_scaleUpArg = new Hashtable();

	private GameObject m_mysteryLabel;

	private float scalingUpTimer;

	private void Awake()
	{
		StopParticleEffects();
		Dbg.Assert(m_itemCell);
		if ((bool)m_itemCell)
		{
			m_itemCell.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
			m_itemCell.HideChildren();
			UIButtonMessage component = m_itemCell.GetComponent<UIButtonMessage>();
			if ((bool)component)
			{
				component.target = base.gameObject;
			}
		}
		Transform transform = base.gameObject.transform.Find("MysteryTreasure");
		if (transform != null)
		{
			m_mysteryLabel = transform.gameObject;
			m_mysteryLabel.SetActiveRecursively(false);
		}
		else
		{
			m_mysteryLabel = null;
		}
		if (!m_scaleDownArg.Contains("scale"))
		{
			m_scaleDownArg.Add("scale", new Vector3(0f, 0f, 0f));
		}
		if (!m_scaleDownArg.Contains("time"))
		{
			m_scaleDownArg.Add("time", 0.4f);
		}
		if (!m_scaleDownArg.Contains("oncomplete"))
		{
			m_scaleDownArg.Add("oncomplete", "OnScaleDownComp");
		}
		if (!m_scaleUpArg.Contains("scale"))
		{
			m_scaleUpArg.Add("scale", new Vector3(1f, 1f, 1f));
		}
		if (!m_scaleUpArg.Contains("time"))
		{
			m_scaleUpArg.Add("time", 0.6f);
		}
		if (!m_scaleUpArg.Contains("delay"))
		{
			m_scaleUpArg.Add("delay", 0.3f);
		}
		if (!m_scaleUpArg.Contains("oncomplete"))
		{
			m_scaleUpArg.Add("oncomplete", "OnScaleUpComp");
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (scalingUpTimer > 0f)
		{
			scalingUpTimer -= Time.deltaTime;
			if (scalingUpTimer <= 0f)
			{
				base.gameObject.transform.localScale = Vector3.one;
				m_itemCell.gameObject.GetComponent<Collider>().enabled = true;
			}
		}
	}

	public void SetItemData(string p_itemDbId, bool p_isNewItem)
	{
		m_isNewItem = p_isNewItem;
		m_itemData = SingletonComponent<InventoryManager>.Instance.GetByDBID(p_itemDbId);
	}

	public void ShowItem()
	{
		iTween.ScaleTo(m_chestPic.gameObject, m_scaleDownArg);
		if (m_itemData != null)
		{
			m_itemCell.data = m_itemData;
		}
		scalingUpTimer = 1.9f;
		StartCoroutine(DelayPlayParticle());
		iTween.ScaleTo(m_itemCell.gameObject, m_scaleUpArg);
	}

	private IEnumerator DelayPlayParticle()
	{
		yield return null;
		PlayParticleEffects();
	}

	private void OnButtonClick()
	{
		if (m_canClick && m_dungeonCompletePanelRef != null && m_itemData != null)
		{
			Dbg.Log("[Info] Show item info for: " + m_itemData.ItemID);
			UIPopupController.Instance.ShowItemStats(m_itemData);
		}
	}

	public void OnScaleUpComp()
	{
		base.gameObject.transform.localScale = Vector3.one;
		m_itemCell.gameObject.GetComponent<Collider>().enabled = true;
	}

	public ItemEnum.Chest GetChestLevel()
	{
		if (m_itemData != null)
		{
			return m_itemData.Chest;
		}
		return ItemEnum.Chest.LOWEST;
	}

	public ItemDetails GetItemData()
	{
		return m_itemData;
	}

	public void PlayParticleEffects()
	{
		ParticleSystem[] particles = m_particles;
		foreach (ParticleSystem particleSystem in particles)
		{
			particleSystem.Play();
		}
	}

	public void StopParticleEffects()
	{
		ParticleSystem[] particles = m_particles;
		foreach (ParticleSystem particleSystem in particles)
		{
			particleSystem.Stop();
		}
	}

	private void OnScaleDownComp()
	{
	}

	public void ShowMysteryLabel()
	{
		if (m_mysteryLabel != null)
		{
			m_mysteryLabel.SetActiveRecursively(true);
		}
	}
}
