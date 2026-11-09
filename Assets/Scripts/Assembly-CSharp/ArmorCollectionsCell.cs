using System;
using UnityEngine;

public class ArmorCollectionsCell : NBUIGridCell<ArmorSet>
{
	public TiersStars m_tierStarsPrefab;

	public UITexture m_armorTexture;

	public UISprite m_elementSprite;

	public UISprite m_newFlagSprite;

	public UISprite m_loadingSprite;

	public UILabel m_completedLabel;

	public NBUILocalize m_nameLabel;

	public UILabel m_countLabel;

	public Transform m_tierStarsSlot;

	public float m_cullThresholdMultiplier = 3f;

	public GameObject m_incompleteSpriteGroup;

	protected TiersStars m_tierStars;

	protected bool m_hidden;

	[SerializeField]
	private string debugPath;

	private Material m_armorMaterial;

	private UIScrollBar m_scrollBar;

	private UIPanel m_panel;

	private UIWidget[] m_children;

	public bool visible { get; set; }

	public bool hidden
	{
		get
		{
			return m_hidden;
		}
		set
		{
			m_hidden = value;
			if (m_children == null)
			{
				m_children = MBHier.GetComponentsInChildren<UIWidget>(this);
			}
			UIWidget[] children = m_children;
			foreach (UIWidget uIWidget in children)
			{
				uIWidget.enabled = !m_hidden;
			}
		}
	}

	public override ArmorSet data
	{
		set
		{
			m_data = value;
			if (m_data != null)
			{
				SetNewFlag(m_data.isNew);
				SetName(m_data.name);
				SetElement(m_data.elementType);
				SetTierStars(m_data.tier, m_data.maxTier);
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
	}

	private void OnEnable()
	{
		Init();
	}

	private void OnDisable()
	{
		Reset();
	}

	private void OnScrollBarChange(UIScrollBar p_bar)
	{
		if (!IsVisible())
		{
			base.gameObject.SetActiveRecursively(false);
		}
	}

	private bool IsVisible()
	{
		if (null == m_panel)
		{
			return false;
		}
		Bounds bounds = NGUIMath.CalculateRelativeWidgetBounds(m_panel.transform, base.transform);
		Vector3 vector = m_panel.CalculateConstrainOffset(bounds.min, bounds.max);
		float num = width * m_cullThresholdMultiplier;
		if (vector.magnitude > num)
		{
			return false;
		}
		return true;
	}

	private bool IsCompleteAny()
	{
		if (m_data == null)
		{
			return false;
		}
		return m_data.isCompleted;
	}

	private bool IsSpecial()
	{
		if (m_data == null)
		{
			return false;
		}
		return m_data.isSpecial;
	}

	private void Init()
	{
		if (null == m_panel)
		{
			UIDraggablePanel uIDraggablePanel = NGUITools.FindInParents<UIDraggablePanel>(base.gameObject);
			if (null != uIDraggablePanel)
			{
				m_panel = uIDraggablePanel.GetComponent<UIPanel>();
				m_scrollBar = uIDraggablePanel.horizontalScrollBar;
			}
		}
		if (!(null != m_panel))
		{
			return;
		}
		if (IsVisible())
		{
			if (null != m_scrollBar)
			{
				UIScrollBar scrollBar = m_scrollBar;
				scrollBar.onChange = (UIScrollBar.OnScrollBarChange)Delegate.Remove(scrollBar.onChange, new UIScrollBar.OnScrollBarChange(OnScrollBarChange));
				UIScrollBar scrollBar2 = m_scrollBar;
				scrollBar2.onChange = (UIScrollBar.OnScrollBarChange)Delegate.Combine(scrollBar2.onChange, new UIScrollBar.OnScrollBarChange(OnScrollBarChange));
			}
			if (IsCompleteAny() || IsSpecial())
			{
				ShowIncomplete(false);
				if (null == m_armorMaterial || null == m_armorMaterial.mainTexture)
				{
					ReloadArmorTexture();
				}
			}
			else
			{
				ShowIncomplete(true);
			}
			UpdateCompletionStatus();
		}
		else
		{
			base.gameObject.SetActiveRecursively(false);
		}
	}

	private void UpdateCompletionStatus()
	{
		if (m_data != null)
		{
			bool isCompleted = m_data.isCompleted;
			SetCompleted(isCompleted);
			SetNewFlag(m_data.isNew);
			if (!isCompleted)
			{
				SetCount(m_data.count, m_data.maxCount);
			}
		}
	}

	private void Reset()
	{
		if (null != m_scrollBar)
		{
			UIScrollBar scrollBar = m_scrollBar;
			scrollBar.onChange = (UIScrollBar.OnScrollBarChange)Delegate.Remove(scrollBar.onChange, new UIScrollBar.OnScrollBarChange(OnScrollBarChange));
		}
		UnloadArmorTexture();
	}

	protected new virtual void Start()
	{
		base.Start();
		if (null == m_panel)
		{
			Init();
		}
	}

	public void SetArmorTexture(string p_texturePath)
	{
		if (null != m_armorTexture)
		{
			debugPath = p_texturePath;
			if (null == m_armorMaterial)
			{
				m_armorMaterial = GLOBAL.UI_TEXTURE_MATERIAL;
			}
			if (null != m_armorMaterial)
			{
				UnloadArmorTexture();
				m_armorMaterial.mainTexture = (Texture2D)Resources.Load(GLOBAL.ARMOR_COLLECTIONS_PATH + p_texturePath, typeof(Texture2D));
			}
			m_armorTexture.material = m_armorMaterial;
			ShowLoading(false);
		}
	}

	private void ShowLoading(bool p_show)
	{
		if (null != m_loadingSprite)
		{
			m_loadingSprite.gameObject.SetActiveRecursively(p_show);
		}
	}

	public void UnloadArmorTexture()
	{
		if (null != m_armorMaterial && null != m_armorMaterial.mainTexture)
		{
			Texture2D assetToUnload = (Texture2D)m_armorMaterial.mainTexture;
			Resources.UnloadAsset(assetToUnload);
			m_armorMaterial.mainTexture = null;
			m_armorTexture.material = null;
		}
	}

	public void ReloadArmorTexture()
	{
		if (m_data != null)
		{
			SetArmorTexture(m_data.texturePath);
		}
	}

	public void SetTierStars(int p_tier, int p_maxTier)
	{
		if (null == m_tierStars)
		{
			m_tierStars = UnityEngine.Object.Instantiate(m_tierStarsPrefab) as TiersStars;
			if (null != m_tierStarsSlot)
			{
				m_tierStars.transform.parent = base.transform;
				m_tierStars.transform.localPosition = m_tierStarsSlot.transform.localPosition;
				m_tierStars.transform.localScale = m_tierStarsSlot.transform.localScale;
			}
		}
		if (null != m_tierStars)
		{
			m_tierStars.SetTier(p_tier, p_maxTier);
		}
	}

	public void SetCompleted(bool p_completed)
	{
		if (null != m_completedLabel && null != m_countLabel)
		{
			m_completedLabel.gameObject.SetActiveRecursively(p_completed);
			if (null != m_countLabel.transform.parent)
			{
				m_countLabel.transform.parent.gameObject.SetActiveRecursively(!p_completed);
			}
		}
	}

	public void SetName(string p_name)
	{
		if (null != m_nameLabel)
		{
			m_nameLabel.Key = p_name;
		}
	}

	public void SetElement(Element.Type p_element)
	{
		if (null != m_elementSprite)
		{
			string value = string.Empty;
			if (GLOBAL.s_elementIconMap.TryGetValue(p_element, out value))
			{
				m_elementSprite.spriteName = value;
			}
		}
	}

	public void SetNewFlag(bool p_enabled)
	{
		if (null != m_newFlagSprite)
		{
			m_newFlagSprite.enabled = p_enabled;
		}
	}

	public void SetCount(int current, int max)
	{
		if (null != m_countLabel)
		{
			m_countLabel.text = current + "/" + max;
		}
	}

	public void ShowIncomplete(bool p_show)
	{
		if (null != m_incompleteSpriteGroup)
		{
			m_incompleteSpriteGroup.gameObject.SetActiveRecursively(p_show);
		}
		if (null != m_armorTexture)
		{
			m_armorTexture.gameObject.SetActiveRecursively(!p_show);
		}
		if (p_show)
		{
			ShowLoading(false);
		}
	}
}
