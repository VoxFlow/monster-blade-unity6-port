using UnityEngine;

public class DungeonCell : NBUIGridCell<DungeonManager.DungeonStruct>
{
	public UILabel m_labelDungeonName;

	public UISprite m_spriteTextNew;

	public UILabel m_labelStamina;

	public UIAtlas[] m_elementAtlases;

	public UIFilledSprite m_spriteElement;

	public UISprite m_spriteRank;

	private string[] m_rankSpriteName = new string[5] { "Icon_medal", "Icon_medal", "Icon_medal_bronze", "Icon_medal_silver", "Icon_medal_gold" };

	public Component m_eventTarget;

	private bool m_hidden;

	private UIWidget[] m_children;

	private int m_requiredLevel;

	[SerializeField]
	private GameObject m_LockedGroup;

	[SerializeField]
	private GameObject m_UnlockedGroup;

	[SerializeField]
	private UILabel m_requiredLevelLabel;

	[SerializeField]
	private UISprite m_scrollBG;

	public bool m_IsLastInCity;

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

	public override DungeonManager.DungeonStruct data
	{
		set
		{
			m_data = value;
			Reset();
			if (!string.IsNullOrEmpty(m_data.dungeon.iconSpriteName))
			{
				SetElementIcon(m_data.dungeon.iconSpriteName);
			}
			m_labelStamina.text = data.dungeon.stamina + string.Empty;
			m_labelDungeonName.text = SingletonComponent<NBLocalization>.Instance.Get(m_data.dungeon.name);
			m_spriteTextNew.enabled = !m_data.cleared && !m_data.visited;
			if (m_data.medal > DungeonManager.DungeonStruct.Medal.NONE)
			{
				m_spriteRank.spriteName = m_rankSpriteName[(int)m_data.medal];
			}
			m_requiredLevel = m_data.dungeon.requiredLvl;
			SetLock();
		}
	}

	public void SetElementIcon(string p_spriteName)
	{
		UIAtlas[] elementAtlases = m_elementAtlases;
		foreach (UIAtlas uIAtlas in elementAtlases)
		{
			if (uIAtlas.Contains(p_spriteName))
			{
				m_spriteElement.atlas = uIAtlas;
				m_spriteElement.spriteName = p_spriteName;
				break;
			}
		}
	}

	public void SetElementIconCutRatio(float p_cutRatio)
	{
		m_spriteElement.MakePixelPerfect();
		m_spriteElement.fillAmount = p_cutRatio;
	}

	private void Reset()
	{
		m_labelDungeonName.text = "Unknown";
		m_spriteTextNew.enabled = false;
		m_spriteRank.enabled = false;
	}

	protected override void Awake()
	{
		base.Awake();
		Dbg.Assert(m_labelDungeonName);
		Dbg.Assert(m_spriteTextNew);
		Dbg.Assert(m_labelStamina);
		Dbg.Assert(m_spriteElement);
		Dbg.Assert(m_spriteRank);
		Dbg.Assert(m_LockedGroup);
		Dbg.Assert(m_UnlockedGroup);
		Dbg.Assert(m_scrollBG);
	}

	public bool CheckIfLocked()
	{
		PlayerDataManager instance = SingletonComponent<PlayerDataManager>.Instance;
		PlayerData playerData = instance.GetPlayerData();
		int level = playerData.Level;
		return m_requiredLevel > level;
	}

	public bool SetLock()
	{
		if (CheckIfLocked())
		{
			m_LockedGroup.SetActiveRecursively(true);
			m_UnlockedGroup.SetActiveRecursively(false);
			m_scrollBG.color = Color.gray;
			if (SingletonComponent<NBLocalization>.Instance != null)
			{
				string format = SingletonComponent<NBLocalization>.Instance.Get("required_lvl");
				string text = string.Format(format, m_requiredLevel.ToString());
				m_requiredLevelLabel.text = text;
			}
			return true;
		}
		m_LockedGroup.SetActiveRecursively(false);
		m_UnlockedGroup.SetActiveRecursively(true);
		if (m_data.number > 9)
		{
		}
		m_spriteTextNew.gameObject.SetActiveRecursively(false);
		if (!m_data.visited)
		{
			m_spriteTextNew.gameObject.SetActiveRecursively(true);
		}
		if (m_data.medal == DungeonManager.DungeonStruct.Medal.NONE)
		{
			m_spriteRank.gameObject.SetActiveRecursively(false);
		}
		return false;
	}

	protected override void Start()
	{
		base.Start();
	}

	private void OnClick()
	{
		if ((bool)m_eventTarget)
		{
			m_eventTarget.SendMessage("OnDungeonCellEvent", this, SendMessageOptions.DontRequireReceiver);
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_secondary_ui_tap");
		}
	}
}
