using UnityEngine;

public class EventCell : NBUIGridCell<DungeonEventStruct>
{
	public UILabel m_labelDungeonName;

	public UISprite m_spriteTextNew;

	public UISprite m_spriteAreaFrame;

	public Component m_eventTarget;

	public UIAtlas[] m_elementAtlases;

	public UIFilledSprite m_spriteElement;

	public UISprite m_spriteLock;

	public UILabel m_countDownLabel;

	public UILabel m_countDownTextLabel;

	private bool m_isLocked;

	private bool m_hidden;

	private UIWidget[] m_children;

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
				if ((!(uIWidget == m_spriteTextNew) || !m_data.cleared || !m_data.visited) && (!(uIWidget == m_spriteLock) || m_isLocked) && (!(uIWidget == m_spriteTextNew) || !m_isLocked))
				{
					uIWidget.enabled = !m_hidden;
				}
			}
		}
	}

	public override DungeonEventStruct data
	{
		set
		{
			m_data = value;
			Reset();
			m_labelDungeonName.text = m_data.name;
			m_spriteTextNew.enabled = !m_data.cleared && !m_data.visited;
			float fillAmount = 0.931f;
			if (data.boss_id.Contains("TUTORIAL_MONSTER_"))
			{
				string text = data.element.ToString().ToLower();
				if (text == "earth")
				{
					text = "nature";
				}
				string text2 = data.id.Replace("TUTORIAL_", string.Empty);
				SetElementIcon("Dungeon_" + text + "_" + text2);
			}
			else if (data.boss_id.Contains("TUTORIAL_LAND_DRAGON_1"))
			{
				SetElementIcon("Lightning_landdragon_boss");
				m_spriteElement.fillAmount = fillAmount;
				m_spriteElement.MakePixelPerfect();
			}
			else
			{
				SetElementIcon(SingletonComponent<DungeonManager>.Instance.GetBossIconByBossId(data.boss_id));
				m_spriteElement.fillAmount = fillAmount;
				m_spriteElement.MakePixelPerfect();
				m_spriteElement.transform.localScale = new Vector3(270f, 250f, 0f);
			}
			SetSpriteState();
		}
	}

	private void SetElementIcon(string p_spriteName)
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

	public bool RefreshCountDownTimer()
	{
		if (m_data == null || null == m_countDownLabel)
		{
			return false;
		}
		long endTime = m_data.endTime;
		long unixEpochTime = TimeUtil.GetUnixEpochTime();
		long p_time = endTime - unixEpochTime;
		string text = NBGeneric.ConvertTimeToDDHHMM(p_time);
		m_countDownLabel.text = text;
		if (string.IsNullOrEmpty(text))
		{
			ShowCountDownTimer(false);
			return false;
		}
		return true;
	}

	public void ShowCountDownTimer(bool p_show)
	{
		if (null != m_countDownLabel)
		{
			m_countDownLabel.gameObject.SetActiveRecursively(p_show);
		}
		if (null != m_countDownTextLabel)
		{
			m_countDownTextLabel.gameObject.SetActiveRecursively(p_show);
		}
	}

	private void Reset()
	{
		m_labelDungeonName.text = SingletonComponent<NBLocalization>.Instance.Get("common_unknown_label");
		m_spriteTextNew.enabled = false;
	}

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void Start()
	{
		base.Start();
		Dbg.Assert(m_labelDungeonName, "Null referenced");
		Dbg.Assert(m_spriteTextNew, "Null referenced");
		Dbg.Assert(m_spriteLock, "Null referenced");
	}

	public void SetLock(bool p_isLocked, string p_reason = "")
	{
		m_isLocked = p_isLocked;
		SetSpriteState();
	}

	public void SetSpriteState()
	{
		if (m_isLocked)
		{
			m_spriteLock.enabled = true;
			m_spriteAreaFrame.color = Color.gray;
			m_spriteTextNew.enabled = false;
		}
		else
		{
			m_spriteLock.enabled = false;
			m_spriteAreaFrame.color = Color.white;
			m_spriteTextNew.enabled = !m_data.cleared && !m_data.visited;
		}
	}

	public bool GetLock()
	{
		return m_isLocked;
	}

	private void OnClick()
	{
		if ((bool)m_eventTarget && !m_isLocked)
		{
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_ui_secondary_ui_tap");
			m_eventTarget.SendMessage("OnEventCellEvent", this, SendMessageOptions.DontRequireReceiver);
		}
	}
}
