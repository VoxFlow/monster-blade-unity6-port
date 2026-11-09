using UnityEngine;

public class EncounterEnemyUi : MonoBehaviour
{
	[SerializeField]
	private UILabel m_monsterName;

	[SerializeField]
	private UILabel m_monsterLevel;

	[SerializeField]
	private UISprite m_smokeBackgroundImg;

	[SerializeField]
	private UISprite m_nameContainerImg;

	[SerializeField]
	private UISprite m_weakIcon;

	[SerializeField]
	private UILabel m_weakText;

	[SerializeField]
	private UISprite m_strongIcon;

	[SerializeField]
	private UILabel m_strongText;

	public Color[] m_colorCodes = new Color[5];

	private bool m_userPressedOnMonster;

	public bool UserPressedOnMonster
	{
		get
		{
			return m_userPressedOnMonster;
		}
	}

	public void Awake()
	{
		ParticleSystem componentInChildren = MBHier.GetComponentInChildren<ParticleSystem>(this);
		if (!(componentInChildren == null) && !(componentInChildren.name != "FX_ClickHere"))
		{
			componentInChildren.startSize = 200f * GLOBAL.UI_SCALE_FACTOR;
		}
	}

	public void ResetUserPressedOnMonsterFlag()
	{
		m_userPressedOnMonster = false;
	}

	public void OnMonsterClick()
	{
		m_userPressedOnMonster = true;
	}

	public void SetMonsterName(string p_monsterName)
	{
		m_monsterName.text = p_monsterName;
	}

	public void SetMonsterLevel(int p_level, int p_playerLevel, bool p_isBoss)
	{
		if (p_isBoss)
		{
			if (null != SingletonComponent<NBLocalization>.Instance)
			{
				m_monsterLevel.text = SingletonComponent<NBLocalization>.Instance.Get("common_level") + " ??";
			}
			m_nameContainerImg.spriteName = "bossname_bg_red";
			m_smokeBackgroundImg.spriteName = "dragon_logo_red";
		}
		else
		{
			if (null != SingletonComponent<NBLocalization>.Instance)
			{
				m_monsterLevel.text = SingletonComponent<NBLocalization>.Instance.Get("common_level") + " " + p_level;
			}
			m_nameContainerImg.spriteName = "bossname_bg";
			m_smokeBackgroundImg.spriteName = "smoke_fx";
		}
		int num = p_level - p_playerLevel;
		if (num >= 6)
		{
			m_monsterLevel.color = m_colorCodes[0];
		}
		else if (num < 6 && num >= 2)
		{
			m_monsterLevel.color = m_colorCodes[1];
		}
		else if (num < 2 && num >= -1)
		{
			m_monsterLevel.color = m_colorCodes[2];
		}
		else if (num < -1 && num >= -5)
		{
			m_monsterLevel.color = m_colorCodes[3];
		}
		else
		{
			m_monsterLevel.color = m_colorCodes[4];
		}
	}

	public void SetMonsterElement(Element.Type p_elementType)
	{
		if (m_weakIcon != null && m_strongIcon != null && m_weakText != null && m_strongText != null)
		{
			string[] array = new string[5] { "fire_icon", "nature_icon", "lightning_icon", "water_icon", "metal_icon" };
			Element.Type weakElement = Element.GetWeakElement(p_elementType);
			Element.Type strongElement = Element.GetStrongElement(p_elementType);
			if (strongElement != Element.Type.None && p_elementType != Element.Type.Lightning && p_elementType != Element.Type.Metal)
			{
				m_strongText.enabled = true;
				m_strongIcon.enabled = true;
				m_strongIcon.spriteName = array[(int)weakElement];
			}
			else
			{
				m_strongText.enabled = false;
				m_strongIcon.enabled = false;
			}
			if (weakElement != Element.Type.None)
			{
				m_weakText.enabled = true;
				m_weakIcon.enabled = true;
				m_weakIcon.spriteName = array[(int)strongElement];
			}
		}
		else
		{
			Dbg.LogError("m_weakIcon or m_strongIcon is null, do not show the weak and strong icon");
		}
	}
}
