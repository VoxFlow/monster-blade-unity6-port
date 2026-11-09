using Models;
using UnityEngine;

public class ShopCell : NBUIGridCell<ServerStoreItem>
{
	public UILabel m_label;

	public UILabel m_labelPrice;

	public UILabel m_labelDiamonds;

	public UISprite m_uiSprite;

	[SerializeField]
	private UISprite m_bannerBg;

	[SerializeField]
	private UISprite m_bannerTitle;

	[SerializeField]
	private UISprite m_freeDiamondsSprite;

	[SerializeField]
	private UILabel m_freeDiamondsLabel;

	public UILabel Label
	{
		get
		{
			return m_label;
		}
	}

	public UISprite Sprite
	{
		get
		{
			return m_uiSprite;
		}
	}

	public override ServerStoreItem data
	{
		get
		{
			return base.data;
		}
		set
		{
			base.data = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Start()
	{
		base.Start();
	}

	private void OnEnable()
	{
		ShowShopCellData();
	}

	private void ShowShopCellData()
	{
		if (data == null)
		{
			Dbg.Log("Cannot show null item.");
			return;
		}
		if (m_uiSprite != null && data.spriteName != string.Empty)
		{
			m_uiSprite.spriteName = data.spriteName;
		}
		if (m_label != null)
		{
			m_label.text = ((!(SingletonComponent<NBLocalization>.Instance != null)) ? string.Empty : SingletonComponent<NBLocalization>.Instance.Get(data.name));
		}
		if (m_labelPrice != null)
		{
			m_labelPrice.text = data.priceLabel;
		}
		if (m_labelDiamonds != null)
		{
			string text = ((!(SingletonComponent<NBLocalization>.Instance != null)) ? string.Empty : SingletonComponent<NBLocalization>.Instance.Get("diamond_total_label"));
			m_labelDiamonds.text = text + " " + data.denomination + "[D]";
		}
		SetFreeDiamonds();
		SetPopularOrBestValue();
	}

	public void SetShopCellTarget(GameObject p_gameObjectTarget)
	{
		if (data != null)
		{
			UIButtonMessage component = GetComponent<UIButtonMessage>();
			if (component != null)
			{
				component.enabled = !data.isHidden;
				m_uiSprite.color = ((!data.isHidden) ? Color.white : Color.grey);
				component.target = p_gameObjectTarget;
				component.functionName = "OnShopCellClick";
				base.gameObject.SetActiveRecursively(false);
			}
		}
	}

	private void SetFreeDiamonds()
	{
		int num = ((data != null) ? data.freeDiamonds : 0);
		if (null != m_freeDiamondsSprite && null != m_freeDiamondsLabel)
		{
			m_freeDiamondsSprite.gameObject.SetActiveRecursively(num > 0);
			m_freeDiamondsSprite.spriteName = LocalizedSprite("img_discount");
			m_freeDiamondsLabel.gameObject.SetActiveRecursively(num > 0);
			m_freeDiamondsLabel.text = num.ToString();
		}
	}

	private void SetPopularOrBestValue()
	{
		bool flag = data != null && data.popular;
		bool flag2 = data != null && data.bestValue;
		string p_baseName = (flag ? "txt_mostpop" : ((!flag2) ? string.Empty : "txt_bestvalue"));
		if (null != m_bannerBg && null != m_bannerTitle)
		{
			m_bannerBg.gameObject.SetActiveRecursively(flag || flag2);
			m_bannerTitle.gameObject.SetActiveRecursively(flag || flag2);
			m_bannerTitle.spriteName = LocalizedSprite(p_baseName);
			m_bannerTitle.MakePixelPerfect();
		}
	}

	private string LocalizedSprite(string p_baseName)
	{
		if (null == SingletonComponent<SaveData>.Instance)
		{
			return p_baseName;
		}
		string language = SingletonComponent<SaveData>.Instance.GameData.Language;
		string text = p_baseName;
		switch (language)
		{
		case "Japanese":
			return text + "_jp";
		case "Chinese_s":
			return text + "_cns";
		case "Chinese_t":
			return text + "_cnt";
		case "Korean":
			return text + "_kr";
		default:
			return text + "_en";
		}
	}
}
