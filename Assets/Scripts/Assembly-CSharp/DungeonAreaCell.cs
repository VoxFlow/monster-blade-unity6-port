using UnityEngine;

public class DungeonAreaCell : NBUIGridCell<DungeonManager.CityStruct>
{
	public enum ActType
	{
		I = 0,
		II = 1,
		III = 2,
		IV = 3,
		V = 4,
		MAX = 5
	}

	public UILabel labelAreaName;

	public UISprite spriteTextNew;

	public UILabel m_suggestedLevel;

	public UISprite m_cityElementIcon;

	[SerializeField]
	private UIAtlas[] bossAtlases;

	[SerializeField]
	private UISprite spriteBossIconBg;

	[SerializeField]
	private UISprite spriteLocked;

	private DungeonManager m_dm;

	private UIWidget[] m_children;

	private bool m_hidden;

	private int m_requiredLevel = 1;

	public Component eventTarget;

	public override DungeonManager.CityStruct data
	{
		set
		{
			m_data = value;
			Reset();
			string text = m_data.city.dungeonBosses[m_data.city.dungeonBosses.Count - 1];
			Dbg.LogEx("Last boss for hunt {0} is {1}", Dbg.NBLogType.COMMON_LOG, m_data.city.id, text);
			int num = SuggestedLevel(text);
			PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
			string colorCodeBasedOnLevelDiff = GetColorCodeBasedOnLevelDiff(GetBossLevel(text) - playerData.Level);
			m_suggestedLevel.text = string.Format(SingletonComponent<NBLocalization>.Instance.Get("hunt_recommended_lvl"), colorCodeBasedOnLevelDiff + num + "[-]");
			m_cityElementIcon.spriteName = GetElementIconName(m_data.city.element);
			labelAreaName.text = SingletonComponent<NBLocalization>.Instance.Get(m_data.city.cityName);
			SingletonComponent<DungeonManager>.Instance.UpdateCityStatus(m_data);
			string iconSpriteName = m_data.city.iconSpriteName;
			UIAtlas[] array = bossAtlases;
			foreach (UIAtlas uIAtlas in array)
			{
				if (uIAtlas.Contains(iconSpriteName))
				{
					spriteBossIconBg.atlas = uIAtlas;
					spriteBossIconBg.spriteName = iconSpriteName;
					break;
				}
			}
			spriteTextNew.enabled = !m_data.cleared && !m_data.visited;
			if (m_dm.GetLatestActOfCity(m_data.city) == 0)
			{
				int num2 = 1;
			}
		}
	}

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
				if (m_hidden || !(uIWidget == spriteTextNew))
				{
					uIWidget.enabled = !m_hidden;
				}
			}
		}
	}

	public bool CheckIfLocked()
	{
		PlayerDataManager instance = SingletonComponent<PlayerDataManager>.Instance;
		PlayerData playerData = instance.GetPlayerData();
		int level = playerData.Level;
		return m_requiredLevel > level;
	}

	public void SetReqLevel(int level)
	{
		m_requiredLevel = level;
		SetLock();
	}

	public bool SetLock()
	{
		bool flag = CheckIfLocked();
		if (flag)
		{
			spriteBossIconBg.color = Color.gray;
			spriteLocked.gameObject.SetActiveRecursively(true);
			spriteLocked.enabled = true;
			spriteTextNew.enabled = false;
		}
		else
		{
			spriteBossIconBg.color = Color.white;
			spriteLocked.gameObject.SetActiveRecursively(false);
			spriteLocked.enabled = false;
			spriteTextNew.enabled = !m_data.cleared && !m_data.visited;
		}
		return flag;
	}

	private void Reset()
	{
		labelAreaName.text = "Unknown";
		spriteTextNew.enabled = false;
	}

	protected override void Awake()
	{
		base.Awake();
		m_dm = SingletonComponent<DungeonManager>.Instance;
		Dbg.Assert(m_dm, "Missing Dungeon Manager Instance");
		Dbg.Assert(spriteBossIconBg, "Missing Boss Icon");
		Dbg.Assert(spriteLocked, "Missing Sprite Locked");
		Dbg.Assert(spriteTextNew, "Missing sprite text new");
	}

	protected override void Start()
	{
		base.Start();
	}

	private void OnClick()
	{
		if ((bool)eventTarget)
		{
			eventTarget.SendMessage("OnDungeonAreaCellEvent", this, SendMessageOptions.DontRequireReceiver);
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_secondary_ui_tap");
		}
	}

	private int GetBossLevel(string p_bossId)
	{
		string[] array = p_bossId.Split('_');
		int p_outResult = 999;
		if (NBGeneric.TryParse<int>(array[array.Length - 1], out p_outResult))
		{
			return p_outResult;
		}
		return 0;
	}

	private int SuggestedLevel(string p_bossId)
	{
		int num = GetBossLevel(p_bossId) - 2;
		if (num < 0)
		{
			return 0;
		}
		return num;
	}

	private string GetColorCodeBasedOnLevelDiff(int p_levelDiff)
	{
		string text = "[FF0000]";
		if (p_levelDiff >= 6)
		{
			return "[FF0000]";
		}
		if (p_levelDiff < 6 && p_levelDiff >= 2)
		{
			return "[FF6800]";
		}
		if (p_levelDiff < 2 && p_levelDiff >= -1)
		{
			return "[00BE00]";
		}
		if (p_levelDiff < -1 && p_levelDiff >= -5)
		{
			return "[76FF76]";
		}
		return "[808080]";
	}

	private string GetElementIconName(Element.Type p_element)
	{
		string result = "fire_icon";
		switch (p_element)
		{
		case Element.Type.Earth:
			result = "nature_icon";
			break;
		case Element.Type.Fire:
			result = "fire_icon";
			break;
		case Element.Type.Lightning:
			result = "lightning_icon";
			break;
		case Element.Type.Metal:
			result = "metal_icon";
			break;
		case Element.Type.Water:
			result = "water_icon";
			break;
		}
		return result;
	}
}
